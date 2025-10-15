using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Animation
{
    [Serializable]
    public class AnimationClipSet : AnimationClipSetBase
    {
        [HorizontalGroup(Width = 0.55f, MarginRight = 0.0125f)]
        [HideLabel]
        [OnValueChanged("GetAnimationEvents")]
        [SerializeField]
        [PropertyOrder(-1)]
        AnimationClip _animation;

        public override AnimationClip AnimationClip
        {
            get => _animation;
            set => _animation = value;
        }

#if UNITY_EDITOR
        public override AnimationClip AnimationToEdit => _animation;
#endif
    }
}

