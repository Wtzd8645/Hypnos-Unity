using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blanketmen.Hypnos.Audio
{
    public class AudioManager : MonoBehaviour
    {
        #region Singleton
        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
        }

        private void OnDestroy()
        {
            if (this == Instance)
            {
                Instance = null;
            }
        }
        #endregion

        public static string[] GetMicrophoneDevices()
        {
            return Microphone.devices;
        }

        private readonly Dictionary<string, AudioClip> microphoneClipMap = new Dictionary<string, AudioClip>();

        public void StartRecord(string deviceName, int maxLength = 10, int frequency = 44100)
        {
            if (Microphone.IsRecording(deviceName))
            {
                Kernel.Log($"[AudioManager] Device is recording. Device: {deviceName}", (int)LogChannel.Audio);
                return;
            }

            microphoneClipMap.TryGetValue(deviceName, out AudioClip clip);
            if (clip != null)
            {
                clip.UnloadAudioData();
                ResourceManager.Instance.Destroy(clip);
            }
            microphoneClipMap[deviceName] = Microphone.Start(deviceName, false, maxLength, frequency);
        }

        public byte[] EndRecord(string deviceName)
        {
            if (!Microphone.IsRecording(deviceName))
            {
                Kernel.Log($"[AudioManager] Device isn't recording. Device: {deviceName}", (int)LogChannel.Audio);
                return new byte[0];
            }

            Microphone.End(deviceName);
            microphoneClipMap.TryGetValue(deviceName, out AudioClip clip);
            if (clip == null)
            {
                Kernel.LogError($"[AudioManager] The recorded audio clip is null. Device: {deviceName}");
                return new byte[0];
            }

            float[] data = new float[clip.samples * clip.channels];
            clip.GetData(data, 0);
            byte[] byteData = new byte[data.Length * sizeof(float)];
            Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);
            return byteData;
        }
    }
}