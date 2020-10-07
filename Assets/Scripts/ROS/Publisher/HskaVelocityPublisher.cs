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
    public class HskaVelocityPublisher : UnityPublisher<MessageTypes.Geometry.TwistStamped>
    {
        // Public
        public GameObject baseLink;
        public string frameId = "base_link";
        // Private
        private MessageTypes.Geometry.TwistStamped m_message;
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
            m_message = new MessageTypes.Geometry.TwistStamped
            {
                header = new MessageTypes.Std.Header()
                {
                    frame_id = frameId
                }
            };
        }

        private void UpdateMessage()
        {
            m_message.header.Update();
            m_message.twist.linear.x = baseLink.transform.InverseTransformDirection(baseLink.GetComponent<Rigidbody>().velocity).z;
            Publish(m_message);
        }
    }
}
