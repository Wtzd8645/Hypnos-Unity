//using ICSharpCode.NRefactory.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public class AnimationLayerBase
    {
//        protected float deltaTime => Time.deltaTime;

//        public RuntimeAction? PreAction;
//        public RuntimeAction? CurAction;
//        public RuntimeAction? NextAction;

//        public bool IsInTransition = false;

//        public float CurActionRatio = 0f;
//        protected float CurActionNormalizeDelta = 0f;

//        public float CountingRatio;

//        public float CurActionNormalizeRatio = 0f;
//        public float PreActionRatio = 0f;

//        public readonly int LayerIndex;
//        public readonly LayerBlendingMode BlendingMode;
//        public bool IsNeedToPlay = false;

//        protected readonly ActionAnimator owner;
//        protected readonly IAnimator animator;

//        protected ActionRoleType roleType => owner.RoleType;

//        public bool IsInLayerTransitionOut = false;

//        public AnimationLayerBase(IAnimator animator, ActionAnimator owner, int layerIndex)
//        {
//            this.animator = animator;
//            this.owner = owner;
//            this.LayerIndex = layerIndex;
//            this.BlendingMode = animator.GetLayerBlendingMode(layerIndex);
//        }

//        public void PreUpdate()
//        {
//            // update transition
//            updateTransition();
//            updateAction();
//            updateParam();
//        }

//        public void SwitchAction()
//        {
//            if (!owner.LockAction)
//            {
//                switchAction();
//            }
//            else
//            {
//                switchWhenLock();
//            }

//            updateOhterData();
//        }

//        // update Layer transition cache
//        private void updateTransition()
//        {
//            IsInTransition = animator.IsInTransition(LayerIndex);
//        }

//        //update cur Action
//        protected abstract void updateAction();

//        //update Condition Data
//        protected abstract void updateParam();

//        //switch action for layer
//        protected abstract void switchAction();


//        private void switchWhenLock()
//        {
//            if (CurAction != null && CurAction.Value.Mode != WrapMode.Loop && CurActionRatio >= 1f)
//            {
//                RuntimeAction defaultAction = owner.GetDefaultAction();
//                animator.Play(defaultAction.ActionNameHash);
//            }
//        }

//        protected virtual void updateOhterData()
//        {
//            PreActionRatio = CurActionRatio;
//        }

//        //check between layersBreak
//        public abstract void CheckBreak(AnimationLayerBase otherLayer);

//        //for prevent from interupting and Playing in a same Frame!
//        //seprate The Logic And Animator Play
//        public virtual void PlayCurAction()
//        {
//            if (IsNeedToPlay && CurAction != null)
//            {
//                IsNeedToPlay = false;
//                playMotion(LayerIndex, CurAction.Value);
//            }
//            else
//            {
//                IsNeedToPlay = false;
//            }
//        }

//        public virtual void ClearPlayActionCmd()
//        {
//            return;
//        }

//        protected virtual bool ProcessAction(RuntimeAction targetAct, bool isByControl)
//        {
//            return false;
//        }

//        protected float normalizeRatio(float r)
//        {
//            //return r % 1f;
//            if (r > 1)
//            {
//                r -= (int)r;
//            }
//            return r;
//        }

//        // To protect from any transition for now!
//        protected virtual bool canSwitch()
//        {
//            //return !IsInTransition;
//            //什么时候都可以switch! 在Transition时也可以
//            return true;
//        }

//        protected int getActionState(int layer)
//        {
//            if (animator == null)
//                return -1;

//            return animator.GetActionState(layer);
//        }

//        protected void playMotion(int layer, RuntimeAction act)
//        {
//            if (animator == null || !act.IsInitialized)
//                return;

//            animator.PlayMotion(layer, act.ActionNameHash, act.ActionId);
//            owner.SetInteger(getStateLayerParamHash(layer), act.ActionId);
//        }

//        protected void playMotionDirectlyByTime(int layer, RuntimeAction act, float normalizedTime)
//        {
//            if (animator == null || !act.IsInitialized)
//                return;

//            animator.PlayMotionByTime(layer, act.ActionNameHash, act.ActionId, normalizedTime);
//            owner.SetInteger(getStateLayerParamHash(layer), act.ActionId);
//        }

//        public void InterruptCurrentMotion()
//        {
//            if (animator == null)
//                return;

//            CurAction = null;
//            CurActionRatio = 0f;
//            CurActionNormalizeRatio = 1.0f;
//            animator.Interrupt(LayerIndex);
//            owner.SetInteger(getStateLayerParamHash(LayerIndex), 0);
//        }

//        protected void onLayerInterrupt(int layer)
//        {
//            if (animator == null)
//                return;

//            animator.Interrupt(layer);
//            owner.SetInteger(getStateLayerParamHash(layer), 0);
//        }

//        int getStateLayerParamHash(int layer)
//        {
//            switch (layer)
//            {
//                case 0:
//                    return PlayerAnimatorParam.State;
//                case 1:
//                    return PlayerAnimatorParam.State2;
//                case 2:
//                    return PlayerAnimatorParam.State3;
//                case 3:
//                    return PlayerAnimatorParam.State4;
//            }

//            return 0;
//        }

//        protected virtual bool isBlendTree(RuntimeAction act)
//        {
//            return act.IsInitialized && act.IsBlendTree;
//        }

//        protected bool isCurrentActionChange()
//        {
//            return PreAction != null && PreAction != CurAction;
//        }

//        protected void cachePreAction()
//        {
//            PreAction = CurAction;
//        }

//        public bool PlayAction(RuntimeAction act, bool isByControl)
//        {
//            return act.IsInitialized && ProcessAction(act, isByControl);
//        }

//        protected float GetSwitchTime(RuntimeAction cur, ActionType nextType)
//        {
//            return RuntimeActionUtil.GetSwitchTime(ref cur, nextType);
//        }

//        public bool PlayActionWithoutSwitch(RuntimeAction act)
//        {
//            if (!act.IsInitialized)
//            {
//                return false;
//            }

//            //base Layer
//            if (act.Layer == 0)
//            {
//                bool isNext = IsInTransition && NextAction != null;

//                if (isNext || (CurAction != null))
//                {
//                    playMotion(act.Layer, act);
//                    NextAction = act;
//                }
//                else
//                {
//                    playMotion(act.Layer, act);
//                }
//            }
//            //UpperLayer
//            else if (act.Layer > 0)
//            {
//                if (IsInTransition)
//                {
//                    playMotion(act.Layer, act);
//                    NextAction = act;
//                }
//                else
//                {
//                    playMotion(act.Layer, act);
//                    CurAction = act;
//                }
//            }
//            else
//            {
//#if UNITY_EDITOR //AnimGraph            
//                if (AnimGraphPlayer.stateNameDict.TryGetValue(act.ActionNameHash, out var name))
//                {
//                    debug.PrintSystem.Log($"The Layer of {name} is wrong LayerIndex{act.Layer}");
//                }
//#endif
//            }

//            return true;
//        }

//        public virtual bool PlayActionDirectly(RuntimeAction act)
//        {
//            if (!act.IsInitialized)
//            {
//                return false;
//            }

//            //base Layer
//            if (act.Layer == 0)
//            {
//                bool isNext = IsInTransition && NextAction != null;

//                if (isNext || CurAction != null)
//                {
//                    NextAction = act;
//                }

//                playMotionDirectlyByTime(act.Layer, act, 0.0f);
//            }
//            //UpperLayer
//            else if (act.Layer > 0)
//            {
//                if (IsInTransition)
//                {
//                    playMotion(act.Layer, act);
//                    NextAction = act;
//                }
//                else
//                {
//                    playMotionDirectlyByTime(act.Layer, act, 0.0f);
//                    CurAction = act;
//                }
//            }
//            else
//            {
//#if UNITY_EDITOR //AnimGraph
//                if (AnimGraphPlayer.stateNameDict.TryGetValue(act.ActionNameHash, out var name))
//                {
//                    debug.PrintSystem.Log($"The Layer of {name} is wrong LayerIndex{act.Layer}");
//                }
//#endif
//            }

//            return true;
//        }

//        public void PlayMotion(RuntimeAction act)
//        {
//            playMotion(act.Layer, act);
//        }

//        public virtual void SetToDefaultAction()
//        {
//        }

//        public virtual bool CanTransitionToSelf(RuntimeAction act)
//        {
//            //return act.NextStateList.Exists(x => x.Id == act.ActionId);
//            return true;
//        }

//        public virtual bool IsInGraphPlayProtectTime()
//        {
//            return false;
//        }

//        protected bool isUseAnimGraph()
//        {
//            return ActionManager.instance.IsUseActionAnimGraphsForMonster;
//        }
    }
}