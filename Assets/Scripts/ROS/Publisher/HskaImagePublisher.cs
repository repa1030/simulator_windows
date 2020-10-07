// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Markus Zimmermann, Patrick Rebling
// =================================================================

using System.Collections;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace RosSharp.RosBridgeClient
{
    public class HskaImagePublisher : UnityPublisher<MessageTypes.Sensor.CompressedImage>
    {
        // Enums
        public enum EncodeType
        {
            JPEG,
            PNG
        };
        // Public
        public Camera imageCamera;
        public string FrameId = "Camera";
        public int resolutionWidth = 1280;
        public int resolutionHeight = 720;
        [Range(0, 100)]
        public int JPEGQualityLevel = 99;
        [Range(0, 9)]
        public int PNGCompressionLevel = 0;
        public EncodeType encodeType = EncodeType.JPEG; 
        // Private
        private MessageTypes.Sensor.CompressedImage m_message;
        private float m_timer;
        private int m_frequency;
        private bool m_capturing;
        private NativeArray<byte> m_readbuffer;
        private byte[] m_img_data;
        

        protected override void Start()
        {
            // Maximum image size (x 2 to have a buffer)
            int max_img_size = resolutionWidth * resolutionHeight * 3 * 2;
            // Initializing
            m_capturing = false;
            m_timer = Time.time;
            m_img_data = null;
            m_img_data = new byte[max_img_size];
            m_frequency = PlayerPrefs.GetInt("imgfrq", 20);
            base.Start();
            InitializeMessage();
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
            if (m_readbuffer.IsCreated) 
                m_readbuffer.Dispose();
            if (imageCamera != null)
                imageCamera.targetTexture?.Release();
        }

        public void Update()
        {
            if (m_capturing)
                return;
            CheckTexture();
            StartCoroutine(Capture());
        }

        IEnumerator Capture()
        {
            m_capturing = true;
            var capture_start = Time.time;

            imageCamera.Render();
            m_message.header.Update();

            var readback = AsyncGPUReadback.Request(imageCamera.targetTexture, 0, TextureFormat.RGB24);
            yield return new WaitUntil(() => readback.done);
            if (readback.hasError)
            {
                Debug.Log("Failed to read GPU texture");
                imageCamera.targetTexture.Release();
                imageCamera.targetTexture = null;
                m_capturing = false;
                yield break;
            }
            m_readbuffer.CopyFrom(readback.GetData<byte>());

            int length = 0;

            // encode original image
            if (encodeType == EncodeType.PNG)
                length = PngEncoder.Encode(m_readbuffer, resolutionWidth, resolutionHeight, 3, 0, m_img_data);
            else if (encodeType == EncodeType.JPEG)
                length = JpegEncoder.Encode(m_readbuffer, resolutionWidth, resolutionHeight, 3, 99, m_img_data);
            
            UpdateAndSendMessage(length);

            var capture_delay = 1.0f / m_frequency - (Time.time - capture_start);
            if (capture_delay > 0)
            {
                yield return new WaitForSeconds(capture_delay);
            }

            m_capturing = false;

        }

        private void CheckTexture()
        {
            // First time
            if (imageCamera.targetTexture == null)
            {
                imageCamera.targetTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24, RenderTextureFormat.Default)
                {
                    dimension = TextureDimension.Tex2D,
                    antiAliasing = 1,
                    useMipMap = false,
                    useDynamicScale = false,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };

                createReadbackBuffer();

            }
            // Check for consistency
            else
            {
                if (resolutionWidth != imageCamera.targetTexture.width || resolutionHeight != imageCamera.targetTexture.height)
                {
                    imageCamera.targetTexture.Release();
                    imageCamera.targetTexture = null;
                    m_readbuffer.Dispose();
                }
                else if (!imageCamera.targetTexture.IsCreated())
                {
                    // if we have lost rendertexture due to Unity window resizing or otherwise
                    imageCamera.targetTexture.Release();
                    imageCamera.targetTexture = null;
                }
            }
        }

        private void createReadbackBuffer()
        {
            if (!m_readbuffer.IsCreated)
            {
                m_readbuffer = new NativeArray<byte>(resolutionWidth * resolutionHeight * 3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }
        }

        private void InitializeMessage()
        {
            m_message = new MessageTypes.Sensor.CompressedImage();
            m_message.header.frame_id = FrameId;
            if (encodeType == EncodeType.JPEG)
                m_message.format = "jpeg";
            else if (encodeType == EncodeType.PNG)
                m_message.format = "png";
            else
                m_message.format = "jpeg";
        }

        private void UpdateAndSendMessage(int length)
        {
            if (length > 0)
            {
                m_message.data = null;
                m_message.data = new byte[length];
                System.Buffer.BlockCopy(m_img_data, 0, m_message.data, 0, length);
                Publish(m_message);
            }
        }

    }
}
