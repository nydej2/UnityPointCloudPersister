// <copyright file="PointcloudVisualizer.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.HelloAR
{
    using System;
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.Profiling;
    using System.Linq;

    /// <summary>
    /// Visualize the point cloud.
    /// </summary>
    public class PointcloudVisualizer : MonoBehaviour
    {
        //Variables needed for Point Cloud Generation
        private int frameCounter = 0;
        private int pointCounter = 0;
        private int pointCounterTotal = 0;
        private const int k_MaxPointCount = 6144;

        private Mesh m_Mesh;

        private Vector3[] m_Points = new Vector3[k_MaxPointCount];
        private Vector4[] m_Points_C = new Vector4[k_MaxPointCount];

        private HashSet<Vector3> storedPoints = new HashSet<Vector3>();

        private KDTree kd;

        public float threshold = 0.8f;
        public int frameThersholdForKDTreeBuild = 60;
        public float thresholdDistance = 0.003f;
        public int pointsAddedToKDTreeThreshold = 3000;
        public int thresholdDetectedPointMin = 1;

        private Vector4 currPoint;

        private List<Vector4> bufferListForPoints = new List<Vector4>();

        List<Vector3> bufferList = new List<Vector3>();

        private bool kdTreeIsCreated = false;
        private bool createNewKdTree = true;

        private int detectedPointsInKdTreeCounter = 0;
        private int detectedPointsInKdTreeCounterFrameCounter = 0;

        //only for test purposes. delete later!
        private int totalDetectedPoints = 0;
        private int distanceToBigCounter = 0;
        private int distanceOkCounter = 0;

        //Variables needed for Pixel Coordinates Calculation
        public Camera mainCamera;
        //private ARCoreBackgroundRenderer backgroundRenderer;
        //public Material backGroundMaterial;
        private Vector4 cameraCoordsOfPoint;
        public Transform prefab;
        /**
         * Unity Method start is called once at the start of Application runtime
         * */
        public void Start()
        {
            m_Mesh = GetComponent<MeshFilter>().mesh;
            m_Mesh.Clear();
        }

        /*public void OnEnable()
        {
            if(backgroundRenderer == null)
            {
                backgroundRenderer = new ARCoreBackgroundRenderer();
            }

            if(backGroundMaterial == null)
            {
                Debug.Log("Kein Background Material zur Kamera hinzugefügt.");
            }
        }*/

        /**
        * Unity Update is called once per frame
        * */
        public void Update()
        {
            //Checks if new PointCloud data (new Points) became available in the current frame
            if (Frame.PointCloud.IsUpdatedThisFrame)
            {
                /*iterates through all Points inside the current PointCloud and checks if their confidence value is over a certain threshold.
                  If yes, round all coordinates of the current point on two decimals (Coordinates are given in Meters in ARCore). That means a Point,
                  which is <= 1cm away from another, will be considered as the same Point. Finally add the current Point. We decided to rather work 
                  with rounding all three Coordinates instead of the euclidean distance of two vectors because of computational costs and the contain Function
                  which is used. Contains will acknowledge two vectors only as identical if they are excactly the same. 
                 */
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    if (Frame.PointCloud.GetPoint(i).w >= threshold &&  kdTreeIsCreated)
                    {
                        //Debug.Log("Baum wurde erstellt");

                      bool neighbourPoint = FindNearestNeighbour(Frame.PointCloud.GetPoint(i));

                      if (neighbourPoint)
                        {
                            detectedPointsInKdTreeCounter++;
                            /**
                             * TODO:
                             * Get Pixelcoordinates with arcore and add a descriptor around it with Opencv. Eventually save
                             * the Descriptor and the Point in the final PointCloud. 
                             */

                            cameraCoordsOfPoint = mainCamera.WorldToScreenPoint(Frame.PointCloud.GetPoint(i));

                            Debug.Log(("Pixelkoordinaten von Punkt: " + Frame.PointCloud.GetPoint(i)
                                      + "sind: " + cameraCoordsOfPoint.x + ", " + cameraCoordsOfPoint.y 
                                      + ", " + cameraCoordsOfPoint.z));


                            cameraCoordsOfPoint = mainCamera.ScreenToWorldPoint(cameraCoordsOfPoint);
                            Instantiate(prefab, cameraCoordsOfPoint, Quaternion.identity);
                        }
                    }
                    else
                    {
                        //Only for test purposes. delete later!
                        totalDetectedPoints++;

                        if (Frame.PointCloud.GetPoint(i).w >= threshold)
                        {
                            currPoint = Frame.PointCloud.GetPoint(i);

                            currPoint.x = (float)Math.Round((double)currPoint.x, 2);
                            currPoint.y = (float)Math.Round((double)currPoint.y, 2);
                            currPoint.z = (float)Math.Round((double)currPoint.z, 2);
                            currPoint.w = 0.1f;

                            m_Points[pointCounter] = currPoint;

                            storedPoints.Add(currPoint);

                            pointCounter++;
                            pointCounterTotal++;
                        }
                    }
                }

                /*
                 This is only for visualisation purposes. All detected Points will bi given to a mesh, which is rendered per frame.
                 MeshTopology is set to Points, since we only want to show the detected Points
                 */
                int[] indices = new int[pointCounter];
                for (int i = 0; i < pointCounter; i++)
                {
                    indices[i] = i;
                }

                m_Mesh.Clear();
                m_Mesh.vertices = m_Points;
                m_Mesh.SetIndices(indices, MeshTopology.Points, 0);

                /**
                * If not enough Points of the current frame are recognized in the KDTree(i.e. when the camera is looking at a new/different
                * scene), the whole instantiation process will start from the beginning.
                */
                if (detectedPointsInKdTreeCounter < thresholdDetectedPointMin && kdTreeIsCreated && detectedPointsInKdTreeCounterFrameCounter > 200)
                {
                    //Debug.Log(detectedPointsInKdTreeCounter);
                    createNewKdTree = true;
                    storedPoints.Clear();
                    pointCounterTotal = 0;
                    totalDetectedPoints = 0;
                    frameCounter = 0;
                    kdTreeIsCreated = false;
                }
                /*
                 * if for at least X frames all detected Points has been collected and we have at least 300 Points detected in this time,
                 * a kd tree with all these Points will be generated. From now on, new detected Points can be compared with the points inside
                 * the kd tree.
                 */
                if (frameCounter >= frameThersholdForKDTreeBuild
                    && pointCounterTotal > pointsAddedToKDTreeThreshold
                    && createNewKdTree)
                {
                    bufferList = storedPoints.ToList();

                    kd = KDTree.MakeFromPoints(bufferList.ToArray());

                    //Only for test purposes. Delete afterwards!
                    /*Debug.Log("Anzahl hinzugefügte Punkte in KDTree: " + storedPoints.Count + "\n");
                    Debug.Log("Anzahl effektiv detektierte Punkte: " + totalDetectedPoints + "\n");
                    Debug.Log("Anzahl effektiv detektierte Punkte mit threshold <= " + threshold + ": " + pointCounterTotal);
                    Debug.Log("Anzahl benötigte Frames für Punktesuche: " + frameCounter + "\n");
                    */
                    frameCounter = 0;
                    pointCounterTotal = 0;
                    totalDetectedPoints = 0;
                    storedPoints.Clear();
                    kdTreeIsCreated = true;
                    createNewKdTree = false;
                }
            }
        }
        public void LateUpdate()
        {
            frameCounter++;
            pointCounter = 0;
            if(detectedPointsInKdTreeCounterFrameCounter > 300)
            {
                /*Debug.Log("Anzahl wiedererkannte Punkte in k-d Baum: " + detectedPointsInKdTreeCounter);
                detectedPointsInKdTreeCounter = 0;
                */
                Debug.Log("Anzahl Punkte, welche in k-d-Baum enthalten sind: " + distanceOkCounter);
                Debug.Log("Anzahl Punkte, welche nicht in k-d-Baum enthalten sind: " + distanceToBigCounter);
                detectedPointsInKdTreeCounterFrameCounter = 0;
                distanceOkCounter = 0;
                distanceToBigCounter = 0;
                detectedPointsInKdTreeCounter = 0;
            }

            detectedPointsInKdTreeCounterFrameCounter++;
        }

        /**
         * returns the nearest neighbour stored inside the kdTree of the point "pointToCompare".
         * */
        bool FindNearestNeighbour(Vector3 pointToCompare)
        {
            int min_id = kd.FindNearest(pointToCompare);
            Vector3 pointV = bufferList[min_id];
            float min_distance = (pointV - pointToCompare).magnitude;
            if (min_distance <= thresholdDistance)
            {
                /*Debug.Log("Distanz zwischen Vektor " + pointV.x 
                          + " ," + pointV.y 
                          + " ," + pointV.z
                          + " und Vektor " + pointToCompare.x 
                          + ", " + pointToCompare.y 
                          + ", " + pointToCompare.z 
                          + " ist: " + min_distance);
                          */
                distanceOkCounter++;
                return true;
            }
            else
            {
                distanceToBigCounter++;
                return false;
            }
        }
    }
}