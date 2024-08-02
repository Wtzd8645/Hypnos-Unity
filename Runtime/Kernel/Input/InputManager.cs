#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
#undef CHECK_PHYSICAL_INPUT
#else
#define CHECK_PHYSICAL_INPUT
#endif

using System.Collections.Generic;
using System.Diagnostics;

namespace Blanketmen.Hypnos
{
    public sealed class InputManager
    {
        #region Singleton
        public static InputManager Instance { get; } = new InputManager();

        private InputManager() { }
        #endregion

        private static readonly InputModuleBase idleModule = new IdleInputModule();
        private readonly Dictionary<int, InputModuleBase> moduleMap = new Dictionary<int, InputModuleBase>(7);

        private InputModuleBase lastModule;
        private InputModuleBase currentModule;

        public void Initialize(InputConfig config)
        {
            //InputSystem.onDeviceChange
            lastModule = idleModule;
            currentModule = idleModule;
        }

        [Conditional("CHECK_PHYSICAL_INPUT")]
        internal void CheckPhysicalInput()
        {
            currentModule.Check();
        }

        internal void ProcessInput()
        {
            currentModule.Process();
        }

        public void SetActive(bool isActive)
        {
            if (isActive && currentModule == idleModule)
            {
                currentModule = lastModule;
                return;
            }

            if (!isActive && currentModule != idleModule)
            {
                currentModule.Reset();
                lastModule = currentModule;
                currentModule = idleModule;
            }
        }

        public void AddModule(int id, InputModuleBase module)
        {
            moduleMap.Add(id, module);
        }

        public void RemoveModule(int id)
        {
            moduleMap.Remove(id);
        }

        public void SwitchModule(int id)
        {
            currentModule.Reset();
            moduleMap.TryGetValue(id, out currentModule);
            currentModule ??= idleModule;
        }
    }
}