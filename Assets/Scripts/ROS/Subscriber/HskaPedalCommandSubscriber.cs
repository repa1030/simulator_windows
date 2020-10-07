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
    public class HskaPedalCommandSubscriber : UnitySubscriber<MessageTypes.Hska.PedalCommand>
    {
        // Public
        public CarController carController;
        // Private
        private MessageTypes.Hska.PedalCommand m_msg;
        private HskaADASCommandPublisher m_adas_pub;
        private bool m_isMessageReceived;

        protected override void Start()
        {
            m_adas_pub = GetComponent<HskaADASCommandPublisher>();
            m_isMessageReceived = false;
            base.Start();
		}

        protected override void ReceiveMessage(MessageTypes.Hska.PedalCommand message)
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
            // Write pedal commands to CarController Script
            if(m_adas_pub.CC) 
            {
                carController.GasPedal = m_msg.gas_pedal;
                carController.BrakePedal = m_msg.brake_pedal;
                carController.CCFlag = m_msg.active;
            }
            else
            {
                carController.CCFlag = false;
            }
        }
    }
}