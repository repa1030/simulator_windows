// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Markus Zimmermann, Patrick Rebling
// =================================================================

using UnityEngine;
using System;

namespace RosSharp.RosBridgeClient
{
    public class HskaImuPublisher : UnityPublisher<MessageTypes.Sensor.Imu>
    {
        // Public
        public IMUUnit IMUSensor;
        public string frameId = "imu_link";
        // Private
        private MessageTypes.Sensor.Imu m_message;
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
            m_message = new MessageTypes.Sensor.Imu
            {
                header = new MessageTypes.Std.Header()
                {
                    frame_id = frameId
                }
            };
            for(int i = 0; i < m_message.orientation_covariance.Length; i++)
            {
                m_message.orientation_covariance[i] = IMUSensor.covar_orient[i];
                m_message.angular_velocity_covariance [i] = IMUSensor.covar_velo[i];
                m_message.linear_acceleration_covariance [i] = IMUSensor.covar_accel[i];
            }
        }

        private void UpdateMessage()
        {
            m_message.header.Update();
            // Orientation
            if (IMUSensor.enable_orient)
            {
                Vector3 orientation_euler = new Vector3(IMUSensor.orient_x, IMUSensor.orient_y, IMUSensor.orient_z);
                //orientation_euler = orientation_euler.Unity2Ros();
                Quaternion orientation = Quaternion.Euler(orientation_euler);
                m_message.orientation.x = orientation.x;
                m_message.orientation.y = orientation.y;
                m_message.orientation.z = orientation.z;
                m_message.orientation.w = orientation.w;
            }
            // Angular velocity
            if (IMUSensor.enable_velo)
            {
                Vector3 ang_velo = new Vector3(IMUSensor.wank_rate, IMUSensor.nick_rate, IMUSensor.yaw_rate);
                //ang_velo = ang_velo.Unity2Ros();
                m_message.angular_velocity.x = ang_velo.x;
                m_message.angular_velocity.y = ang_velo.y;
                m_message.angular_velocity.z = ang_velo.z;
            }
            // Linear acceleration
            if (IMUSensor.enable_accel)
            {
                Vector3 lin_acc = new Vector3(IMUSensor.acc_lon, IMUSensor.acc_lat, IMUSensor.acc_alt);
                //lin_acc = lin_acc.Unity2Ros();
                m_message.linear_acceleration.x = lin_acc.x;
                m_message.linear_acceleration.y = lin_acc.y;
                m_message.linear_acceleration.z = lin_acc.z;
            }
            Publish(m_message);
        }
    }
}
