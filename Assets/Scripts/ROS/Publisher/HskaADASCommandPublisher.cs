// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Markus Zimmermann, Patrick Rebling
// =================================================================

using UnityEngine;
using UnityEngine.UI;

namespace RosSharp.RosBridgeClient
{
    public class HskaADASCommandPublisher : UnityPublisher<MessageTypes.Hska.ADASCommand>
    {
        // Public
        public string frameId = "base_link";
        public UIFunctions UIFunctions;
        public CarLightController carLightController;
        // Private
        private MessageTypes.Hska.ADASCommand m_message;
        private float m_timer;
        private int m_frequency;
        // Getter
        public bool LKAS { get { return UIFunctions.LKASActive; } }
        public bool CC { get { return UIFunctions.CCActive; } }

        protected override void Start()
        {
            m_frequency = PlayerPrefs.GetInt("frq", 40);
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
            m_message = new MessageTypes.Hska.ADASCommand
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
            m_message.lkas = UIFunctions.LKASActive;
            m_message.cc = UIFunctions.CCActive;
            m_message.cmd_velocity = UIFunctions.ccSpeed/3.6f;
            if(carLightController.turnLightsIn == CarLightController.turnlights.Left)
            {
                m_message.lane_change_req = MessageTypes.Hska.ADASCommand.LEFT;
            }
            else if(carLightController.turnLightsIn == CarLightController.turnlights.Right)
            {
                m_message.lane_change_req = MessageTypes.Hska.ADASCommand.RIGHT;
            }
            else
            {
                m_message.lane_change_req = MessageTypes.Hska.ADASCommand.NONE;
            }
            Publish(m_message);
        }
    }
}
