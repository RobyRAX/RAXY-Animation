using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RAXY.Animation
{
    [CreateAssetMenu(fileName = "Animation Events Preset", menuName = "RAXY/Unit Data/Animation Events Preset")]
    public class AnimationEventsPresetSO : ScriptableObject
    {
#if UNITY_EDITOR
        [HideLabel]
        public AnimationClipSet sampleClip;

        public void ResetAeEditor()
        {
            sampleClip?.GetAnimationEvents();
            AnimationEventEditor?.GetAnimationEvents(sampleClip);
        }

        public AnimationEventEditor AnimationEventEditor => sampleClip?.animationEventEditor;

        public List<SingleEventEditor> SingleEventEditors
        {
            get
            {
                if (AnimationEventEditor == null)
                    sampleClip?.GetAnimationEvents();

                return AnimationEventEditor?.SingleEventEditors;
            }
        }
#endif
    }
}