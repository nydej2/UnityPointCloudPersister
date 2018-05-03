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
        private int frameCounter = 0;
        private int pointCounter = 0;
        private int pointCounterTotal = 0;
        private const int k_MaxPointCount = 6144;

        private Mesh m_Mesh;

        private Vector3[] m_Points = new Vector3[k_MaxPointCount];
        private Vector4[] m_Points_C = new Vector4[k_MaxPointCount];

        private HashSet<Vector3> storedPoints = new HashSet<Vector3>();

        private KDTree kd;

        public float threshold = 0.7f;
        public int frameThersholdForKDTreeBuild = 30;

        private Vector3 currPoint;

        /// <summary>
        /// Unity start.
        /// </summary>
        public void Start()
        {
            m_Mesh = GetComponent<MeshFilter>().mesh;
            m_Mesh.Clear();
        }

        /// <summary>
        /// Unity update.
        /// </summary>
        public void Update()
        {
            if (Frame.PointCloud.IsUpdatedThisFrame)
            {
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    if(Frame.PointCloud.GetPoint(i).w >= 0.7)
                    {
                        currPoint = Frame.PointCloud.GetPoint(i);

                        currPoint.x = (float)Math.Round((double)currPoint.x, 2);
                        currPoint.y = (float)Math.Round((double)currPoint.y, 2);
                        currPoint.z = (float)Math.Round((double)currPoint.z, 2);

                        m_Points[pointCounter] = currPoint;

                        storedPoints.Add(currPoint);

                        pointCounter++;
                        pointCounterTotal++;
                    }
                }

                // Update the mesh indicies array.
                int[] indices = new int[Frame.PointCloud.PointCount];
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    indices[i] = i;
                }

                m_Mesh.Clear();
                m_Mesh.vertices = m_Points;
                m_Mesh.SetIndices(indices, MeshTopology.Points, 0);
            }

            if(frameCounter >= frameThersholdForKDTreeBuild && pointCounterTotal > 300)
            {
                List<Vector3> bufferList = storedPoints.ToList();

                kd = KDTree.MakeFromPoints(bufferList.ToArray());

                Debug.Log("Anzahl hinzugeüfgte Punkte in KDTree: " + storedPoints.Count + "\n");
                Debug.Log("Anzahl effektiv detektierte Punkte mit threshold <= " + threshold + ": " + pointCounterTotal);
                Debug.Log("Anzahl benötigte Frames für Punktesuche: " + frameCounter + "\n");

                frameCounter = 0;
                pointCounterTotal = 0;

                storedPoints.Clear();
            } 
        }
        public void LateUpdate()
        {
            frameCounter++;
            pointCounter = 0;
        }
    }
}