using Morpheus.GameTime;
using Morpheus.IO;
using Morpheus.Network;
using Morpheus.Resource;
using System.Collections;
using UnityEngine;

namespace Morpheus
{
    [DisallowMultipleComponent]
    public partial class AppKernel : MonoBehaviour
    {
        #region Singleton
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            enabled = false;
            DontDestroyOnLoad(this);
            CreateServices();
        }

        private void OnDestroy()
        {
            if (this != Instance)
            {
                return;
            }

            ReleaseServices();
            Instance = null;
        }
        #endregion

        #region Coroutine
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        public static Coroutine ExecuteCoroutine(IEnumerator routine)
        {
            return Instance.StartCoroutine(routine);
        }

        public static void TerminateCoroutine(Coroutine routine)
        {
            Instance.StopCoroutine(routine);
        }

        public static void TerminateAllCoroutines()
        {
            Instance.StopAllCoroutines();
        }
        #endregion

        private void FixedUpdate()
        {
            GameTimeManager.Instance.FixedUpdate();
        }

        private void Update()
        {
            GameTimeManager.Instance.Update();
            NetworkManager.Instance.Update();
        }

        private void LateUpdate()
        {
            InputManager.Instance.CheckPhysicalInput();
            InputManager.Instance.ProcessInput();
        }

        private void OnApplicationPause(bool isPause)
        {
            // TODO: 手機平台(Android)在應用程式從後台切換到前台時，好像會有橫向縱向切換的問題，需要重設解析度。
            OnApplicationPauseEvent?.Invoke(isPause);
        }

        private void OnApplicationQuit()
        {
            TerminateAllCoroutines();
            NetworkManager.Instance.Release();
            ResourceManager.Instance.Release();
            OnApplicationQuitEvent?.Invoke();
        }
    }
}