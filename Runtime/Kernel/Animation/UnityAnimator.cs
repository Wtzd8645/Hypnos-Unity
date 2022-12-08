using System.Runtime.CompilerServices;
using UnityEngine;

namespace Morpheus.Animation
{
    public class UnityAnimator : Animator, IAnimator
    {
        private const string LayerParamName = "State";

        public bool ApplyRootMotion { get => applyRootMotion; set => applyRootMotion = value; }
        public bool Enabled { get => enabled; set => enabled = value; }
        public int LayerCount => layerCount;
        public string Name { get => name; set => name = value; }
        public float Speed { get => speed; set => speed = value; }

        public void FixedUpdate(float deltaTime) { }

        public void LateUpdate(float deltaTime) { }

        public void PlayMotion(int layer, string stateName, int motionId)
        {
            //SetInteger(GetLayerParamName(layer), motionId);
        }

        public void PlayMotion(int layer, int stateNameHash, int motionId)
        {
            //SetInteger(GetLayerParamName(layer), motionId);
        }

        public void PlayMotion(int layer, int stateNameHash, int motionId, float normlizedTime)
        {
            //SetInteger(GetLayerParamName(layer), motionId);
            //Play(stateNameHash, layer, normlizedTime);
        }

        public void PlayMotionDirectly(int layer, int stateNameHash, int motionId)
        {
            //SetInteger(GetLayerParamName(layer), motionId);
            //Play(stateNameHash, layer, 0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetLayerParamName(int layer)
        {
            return layer != 0 ? LayerParamName + (layer + 1).ToString() : LayerParamName;
        }
    }
}