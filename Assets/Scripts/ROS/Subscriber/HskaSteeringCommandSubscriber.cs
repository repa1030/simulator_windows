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
    public class HskaSteeringCommandSubscriber : UnitySubscriber<MessageTypes.Hska.SteeringCommand>
    {
        // Public
        public CarController carController;
        public CarLightController carLightController;
        // Private
        private MessageTypes.Hska.SteeringCommand m_msg;
        private HskaADASCommandPublisher m_adas_pub;
        private bool m_isMessageReceived;

        protected override void Start()
        {
            m_adas_pub = GetComponent<HskaADASCommandPublisher>();
            m_isMessageReceived = false;
            base.Start();
		}

        protected override void ReceiveMessage(MessageTypes.Hska.SteeringCommand message)
        {
            m_msg = message;
            m_isMessageReceived = true;
        }

        private void Update()
        {
            if (m_isMessageReceived)
            {
                ProcessMessage();
                m_isMessageReceived = false;
            }
        }

        private void ProcessMessage()
        {
            if(m_adas_pub.LKAS) 
            {
                // Write pedal commands to CarController Script (convert from rad to deg)
                carController.SteeringAngle = m_msg.steering_angle * 180.0f/(float)Math.PI;
                carController.LKASFlag = m_msg.active;
                
                // Blinker state analysis
                if (m_msg.active && m_msg.reset_blinker_state)
                {
                    carLightController.turnLightsIn = CarLightController.turnlights.Off;
                }
            }
            else
            {
                carController.LKASFlag = false;
            }  
        }
    }
}