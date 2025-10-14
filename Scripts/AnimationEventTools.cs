#if UNITY_EDITOR
using RAXY.Utility;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace RAXY.Animation
{
    public class AnimationEventTools : OdinEditorWindow
    {
        [MenuItem("Tools/RAXY/Animation Event Tools")]
        private static void OpenWindow()
        {
            GetWindow<AnimationEventTools>().Show();
        }

        [HideLabel]
        [EnumToggleButtons]
        public AEToolsMode Mode;

        [ShowIf("@Mode == AEToolsMode.CopyAE")]
        [HideLabel]
        [TitleGroup("Source")]
        public AnimationClipSetBase sourceClip;

        [ShowIf("@Mode == AEToolsMode.GetPreset")]
        [HideLabel]
        [TitleGroup("Source")]
        public AnimationEventsPresetSO preset;

        [HideLabel]
        [TitleGroup("Target")]
        public AnimationClipSetBase targetClip;
        [ShowIf("@Mode == AEToolsMode.CopyAE || Mode == AEToolsMode.GetPreset")]
        [TitleGroup("Proceed Setting")]
        public bool clearTargetAE;

        [ShowIf("@Mode == AEToolsMode.CopyAE")]
        [Button("Proceed")]
        [GUIColor(0, 1, 0)]
        [TitleGroup("Proceed Setting")]
        public void ProceedCopyAE()
        {
            if (targetClip.AnimationToEdit == null)
            {
                CustomDebug.Log("Target couldn't be null");
                return;
            }

            if (clearTargetAE)
            {
                AnimationUtility.SetAnimationEvents(targetClip.AnimationToEdit, new AnimationEvent[0]);
            }

            AnimationEvent[] targetEvents = AnimationUtility.GetAnimationEvents(targetClip.AnimationToEdit);
            AnimationEvent[] sourceEvents = null;
            AnimationEvent[] mergedEvents = null;

            sourceEvents = AnimationUtility.GetAnimationEvents(sourceClip.AnimationToEdit);

            // Merge both arrays
            mergedEvents = new AnimationEvent[targetEvents.Length + sourceEvents.Length];
            targetEvents.CopyTo(mergedEvents, 0);
            sourceEvents.CopyTo(mergedEvents, targetEvents.Length);

            // Apply merged events back to targetClip.animation without modifying the animation reference
            AnimationUtility.SetAnimationEvents(targetClip.AnimationToEdit, mergedEvents);
            targetClip.GetAnimationEvents();
        }

        [ShowIf("@Mode == AEToolsMode.GetPreset")]
        [Button("Proceed")]
        [GUIColor(0, 1, 0)]
        [TitleGroup("Proceed Setting")]
        public void ProceedGetPreset()
        {
            if (preset)

                if (targetClip.AnimationToEdit == null)
                {
                    CustomDebug.Log("Target couldn't be null");
                    return;
                }

            if (clearTargetAE)
            {
                AnimationUtility.SetAnimationEvents(targetClip.AnimationToEdit, new AnimationEvent[0]);
            }

            targetClip.GetPresetAE(preset);
        }
    }

    public enum AEToolsMode
    {
        ManageAE, CopyAE, GetPreset
    }
}
#endif