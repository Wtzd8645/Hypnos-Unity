using System.Runtime.InteropServices;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ActStateInfoData
    {
        public int nameHash;
        public int loopCount;
        public float normalizedTime;

        public bool IsName(string str)
        {
            return nameHash == Animator.StringToHash(str);
        }

        public bool IsName(int hash)
        {
            return nameHash == hash;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ActClipInfoData
    {
        public int ClipHash { get; set; }
        public float Weight { get; set; }

        public bool IsName(int hash)
        {
            return ClipHash == hash;
        }
    }

    public interface IAnimator
    {
        bool ApplyRootMotion { get; set; }
        bool Enabled { get; set; }
        int LayerCount { get; }
        string Name { get; set; }
        float Speed { get; set; }

        // Process Function
        void FixedUpdate(float deltaTime);
        void Update(float deltaTime);
        void LateUpdate(float deltaTime);

        void Rebind();

        void Play(string stateName, int layer = -1, float normalizedTime = float.NegativeInfinity);
        void Play(int stateName, int layer = -1, float normalizedTime = float.NegativeInfinity);
        void PlayMotion(int layer, string stateName, int actionId);
        void PlayMotion(int layer, int stateNameHash, int actionId);
        void PlayMotion(int layer, int stateNameHash, int actionId, float normlizedTime);
        void PlayMotionDirectly(int layer, int stateNameHash, int actionId);

        bool GetBool(string name);
        bool GetBool(int hash);
        void SetBool(string name, bool value);
        void SetBool(int hash, bool value);
        float GetFloat(string name);
        float GetFloat(int hash);
        void SetFloat(string name, float value);
        void SetFloat(string name, float param, float dampTime, float deltaTime);
        void SetFloat(int hash, float value);
        void SetFloat(int hash, float param, float dampTime, float deltaTime);
        int GetInteger(string name);
        int GetInteger(int hash);
        void SetInteger(string name, int value);
        void SetInteger(int hash, int value);
        void SetLayerWeight(int layerIndex, float weight);
        void SetTrigger(string name);
        void SetTrigger(int hash);
        void ResetTrigger(string name);
        void ResetTrigger(int hash);

        //ActStateInfoData GetCurrentAnimatorStateInfo(int layerIndex);
        //ActStateInfoData GetNextAnimatorStateInfo(int layerIndex);
        //ActClipInfoData[] GetCurrentAnimatorClipInfo(int layerIndex);
        //ActClipInfoData[] GetNextAnimatorClipInfo(int layerIndex);

        //void GetCurrentAnimatorClipInfo(int layerIndex, List<ActClipInfoData> clipInfoDatas);
        //void GetNextAnimatorClipInfo(int layerIndex, List<ActClipInfoData> clipInfoDatas);
        //bool IsInTransition(int layerIndex);
        //bool IsReadyToPlay(int layerIndex, int stateHash);
        //int GetLayerIndex(string layerName);
        //float GetLayerWeight(int layerIndex);
        //LayerBlendingMode GetLayerBlendingMode(int layerIndex);

        
        //void OnDestroy();

        // Other Function        
        //void Interrupt(int layer, float duration = 0);
        //int GetActionState(int layer);
        //void SetAnimationOutputWeight(float weight);
        //void SpeedUp(float time);
    }
}