// <copyright file="PointcloudVisualizer.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
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
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.Profiling;
    using System;


    /// <summary>
    /// Visualize the point cloud.
    /// </summary>
    public class PointcloudVisualizer : MonoBehaviour
    {
        private const int k_MaxPointCount = 61440;

        private Mesh m_Mesh;

        private Vector4[] m_points4 = new Vector4[k_MaxPointCount];
        private Vector3[] m_Points = new Vector3[k_MaxPointCount];

        private Dictionary<string, Vector4> currPoints;
        private Dictionary<string, Vector4> prevPoints;
        private Dictionary<string, Vector4> pointsToPersist;

        private Vector4 value = new Vector4();
        public float threshold = 0.5f;
        private int testcounter = 0;
        private int frameCounter;
        private int counter = 0;
        private int pointCloudCounter;

        public GUIText text;

        /// <summary>
        /// Unity start.
        /// </summary>
        public void Start()
        {
            m_Mesh = GetComponent<MeshFilter>().mesh;
            m_Mesh.Clear();

            currPoints = new Dictionary<string, Vector4>();
            prevPoints = new Dictionary<string, Vector4>();
            pointsToPersist = new Dictionary<string, Vector4>();
            frameCounter = 0;
            pointCloudCounter = 0;
        }

        /// <summary>
        /// Unity update.
        /// </summary>
        public void Update()
        {
            //Array.Clear(m_Points, 0, m_Points.Length);
            //Array.Clear(m_points4, 0, m_points4.Length);
            /**
             * Fügt JEDEN!(nicht nur neu erkannte Merkmalspunkte) dem Array m_Points, respektive m_points4 hinzu, wenn 
             * sie einen Wert > dem Schwellwert haben. Die Werte werden in ein Dictionary gespeichert.
             * */
            if (Frame.PointCloud.IsUpdatedThisFrame)
            {
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    if (Frame.PointCloud.GetPoint(i).w > threshold)
                    {
                        m_Points[counter] = Frame.PointCloud.GetPoint(i);
                        m_points4[counter] = Frame.PointCloud.GetPoint(i);

                        string vectorToString = m_points4[counter].x.ToString("0.00") + m_points4[counter].y.ToString("0.00") + m_points4[counter].z.ToString("0.00");

                        m_points4[counter].w = 0.01f;

                        currPoints.Add(vectorToString, m_points4[counter]);

                        counter++;
                    }
                }
                //CheckPoint();
            }

            /*foreach(KeyValuePair<string, Vector4> entry in pointsToPersist)
        {
            testcounter++;
            Debug.Log("Funktionsaufruf Nr. " + testcounter+"aufgetreten: " + entry.Value.w);
        }*/
            //}

            // Update the mesh indicies array.
            int[] indices = new int[counter + 1];
            for (int i = 0; i < counter + 1; i++)
            {
                indices[i] = i;
            }

            m_Mesh.Clear();
            m_Mesh.vertices = m_Points;
            m_Mesh.SetIndices(indices, MeshTopology.Points, 0);
        }

        void LateUpdate()
        {
            prevPoints = currPoints;
            frameCounter = frameCounter + 1;
        }

        public void CheckPoint()
        {
            if (prevPoints.Count != 0)
            {
                foreach (KeyValuePair<string, Vector4> entry in currPoints)
                {
                    if (prevPoints.ContainsKey(entry.Key) || pointsToPersist.ContainsKey(entry.Key))
                    {
                        if (!pointsToPersist.ContainsKey(entry.Key))
                        {
                            value = prevPoints[entry.Key];
                            value.w = value.w + 0.01f;

                            pointsToPersist.Add(entry.Key, value);
                        }
                        else
                        {
                            value = pointsToPersist[entry.Key];
                            value.w = value.w + 0.01f;

                            pointsToPersist.Remove(entry.Key);
                            pointsToPersist.Add(entry.Key, value);
                        }
                    }
                }
            }
        }
    }
}