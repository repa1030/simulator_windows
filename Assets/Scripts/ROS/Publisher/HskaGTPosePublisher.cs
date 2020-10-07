// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Markus Zimmermann, Patrick Rebling
// =================================================================

using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class HskaGTPosePublisher : UnityPublisher<MessageTypes.Geometry.PoseStamped>
    {
        // Public
        public Transform PublishedTransform;
        public string FrameId = "Unity";
        // Private
        private MessageTypes.Geometry.PoseStamped m_message;
        private float m_timer;
        private int m_frequency;

        protected override void Start()
        {
            m_frequency = PlayerPrefs.GetInt("frq", 30);
            base.Start();
            m_timer = Time.time;
            InitializeMessage();
        }

        private void FixedUpdate()
        {
            if (Time.time - m_timer >= 1/(float)m_frequency) 
            {
                UpdateMessage();
                m_timer = Time.time;
            }
        }

        private void InitializeMessage()
        {
            m_message = new MessageTypes.Geometry.PoseStamped
            {
                header = new MessageTypes.Std.Header()
                {
                    frame_id = FrameId
                }
            };
        }

        private void UpdateMessage()
        {
            m_message.header.Update();
            GetGeometryPoint(PublishedTransform.position.Unity2Ros(), m_message.pose.position);
            GetGeometryQuaternion(PublishedTransform.rotation.Unity2Ros(), m_message.pose.orientation);

            Publish(m_message);
        }

        private static void GetGeometryPoint(Vector3 position, MessageTypes.Geometry.Point geometryPoint)
        {
            geometryPoint.x = position.x;
            geometryPoint.y = position.y;
            geometryPoint.z = position.z;
        }

        private static void GetGeometryQuaternion(Quaternion quaternion, MessageTypes.Geometry.Quaternion geometryQuaternion)
        {
            geometryQuaternion.x = quaternion.x;
            geometryQuaternion.y = quaternion.y;
            geometryQuaternion.z = quaternion.z;
            geometryQuaternion.w = quaternion.w;
        }

    }
}
