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
    public class HskaGNSSPublisher : UnityPublisher<MessageTypes.Sensor.NavSatFix>
    {
        // Enums
        public enum Device 
        {
            GPS = MessageTypes.Sensor.NavSatStatus.SERVICE_GPS,
            GLONASS = MessageTypes.Sensor.NavSatStatus.SERVICE_GPS,
            COMPASS = MessageTypes.Sensor.NavSatStatus.SERVICE_GPS,
            GALILEO = MessageTypes.Sensor.NavSatStatus.SERVICE_GPS
        };
        public enum Status
        {
            NO_FIX = MessageTypes.Sensor.NavSatStatus.STATUS_NO_FIX,
            FIX = MessageTypes.Sensor.NavSatStatus.STATUS_FIX,
            SAT_FIX = MessageTypes.Sensor.NavSatStatus.STATUS_SBAS_FIX,
            GROUND_FIX = MessageTypes.Sensor.NavSatStatus.STATUS_GBAS_FIX
        };
        public enum CovarianceType
        {
            UNKNOWN = MessageTypes.Sensor.NavSatFix.COVARIANCE_TYPE_UNKNOWN,
            APPROXIMATED = MessageTypes.Sensor.NavSatFix.COVARIANCE_TYPE_APPROXIMATED,
            DIAGONAL_KNOWN = MessageTypes.Sensor.NavSatFix.COVARIANCE_TYPE_DIAGONAL_KNOWN,
            KNOWN = MessageTypes.Sensor.NavSatFix.COVARIANCE_TYPE_KNOWN
        };
        // Public
        public GPSUnit GNSSSensor;
        public string frameId = "gps_unit_link";
        public Device deviceType = Device.GPS;
        public Status fixStatus = Status.FIX;
        public CovarianceType covarianceType = CovarianceType.UNKNOWN;
        // Private
        private MessageTypes.Sensor.NavSatFix m_message;
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
            m_message = new MessageTypes.Sensor.NavSatFix
            {
                header = new MessageTypes.Std.Header()
                {
                    frame_id = frameId
                }
            };
            m_message.status.service = (ushort)deviceType;
            m_message.status.status = (sbyte)fixStatus;
            for(int i = 0; i < m_message.position_covariance.Length; i++)
            {
                m_message.position_covariance[i] = GNSSSensor.pos_covar[i];
            }
            m_message.position_covariance_type = (byte)covarianceType;
        }

        private void UpdateMessage()
        {
            m_message.header.Update();
            m_message.latitude = GNSSSensor.lat;
            m_message.longitude = GNSSSensor.lon;
            m_message.altitude = GNSSSensor.alt;
            Publish(m_message);
        }
    }
}
