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
    public class HskaWheelRotationalSpeedPublisher : UnityPublisher<MessageTypes.Hska.WheelRotationalSpeed>
    {
        // Public
        public WheelSpeedSensor wheelSpeedSensor;
        public string frameId = "wheel_speed_sensor_link";
        // Private
        private MessageTypes.Hska.WheelRotationalSpeed m_message;
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
            m_message = new MessageTypes.Hska.WheelRotationalSpeed
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
            m_message.wheel_rot_speed_fl = wheelSpeedSensor.wheel_fl_rps;
            m_message.wheel_rot_speed_fr = wheelSpeedSensor.wheel_fr_rps;
            m_message.wheel_rot_speed_rl = wheelSpeedSensor.wheel_rl_rps;
            m_message.wheel_rot_speed_rr = wheelSpeedSensor.wheel_rr_rps;
            Publish(m_message);
        }
    }
}
