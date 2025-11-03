using System.Collections;
using System.Collections.Generic;
using Animancer;
using RAXY.Utility;
using UnityEngine;

namespace RAXY.Animation
{
    [RequireComponent(typeof(AnimancerComponent))]
    public class AnimancerController : MonoBehaviour
    {
        public bool removeAcOnRuntime = true;

        public AnimancerComponent Animancer { get; set; }
        public Animator Animator { get; set; }

        public const int MAIN_LAYER = 0;

        private Coroutine applyRootMotionCoroutine;

        protected virtual void Awake()
        {
            Animancer = GetComponent<AnimancerComponent>();
            Animator = GetComponent<Animator>();
        }

        public virtual void PlayAnimation(AnimationClipSetBase clipSet,
                                          float fadeDuration,
                                          int layer = MAIN_LAYER,
                                          FadeMode fadeMode = FadeMode.FixedDuration)
        {
            if (clipSet == null || clipSet.AnimationClip == null)
            {
                CustomDebug.Log("Animasi Kosong");
                return;
            }

            if (fadeDuration <= 0)
            {
                fadeDuration = 0.005f;
            }

            var state = Animancer.Layers[layer].Play(clipSet.AnimationClip, fadeDuration, fadeMode);
            state.Speed = clipSet.speed;

            if (Animancer.Layers[layer].Mask)
            {
                state.Events(this).OnEnd = () =>
                {
                    Animancer.Layers[layer].StartFade(0, 0.2f);
                };
            }
        }

        public virtual void ApplyRootMotion(bool applyRoot, float delay = 0)
        {
            // Stop any previous coroutine
            if (applyRootMotionCoroutine != null)
            {
                StopCoroutine(applyRootMotionCoroutine);
                applyRootMotionCoroutine = null;
            }

            applyRootMotionCoroutine = StartCoroutine(ApplyRootMotionRoutine(delay, applyRoot));
        }

        private IEnumerator ApplyRootMotionRoutine(float delay, bool applyRoot)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            if (Animator != null && Animator.applyRootMotion != applyRoot)
                Animator.applyRootMotion = applyRoot;

            applyRootMotionCoroutine = null;
        }

        public void StopAnimation(AnimationClipSetBase clipSet)
        {
            try
            {
                if (clipSet?.AnimationClip != null)
                    Animancer.Stop(clipSet.AnimationClip);
            }
            catch { }
        }

        public void UpdateAnimancerSpeed(float newSpeed)
        {
            foreach (var layer in Animancer.Layers)
            {
                layer.Speed = newSpeed;
            }
        }

        public virtual void ResetAnimancerSpeed()
        {
            foreach (var layer in Animancer.Layers)
            {
                layer.Speed = 1;
            }
        }

        protected virtual void OnDisable()
        {
            // Stop coroutine when disabled or destroyed (replacement for MEC CancelWith)
            if (applyRootMotionCoroutine != null)
            {
                StopCoroutine(applyRootMotionCoroutine);
                applyRootMotionCoroutine = null;
            }
        }
    }
}
