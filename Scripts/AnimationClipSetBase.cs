using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using System.Linq;
using UnityEngine.Serialization;
using RAXY.Utility;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RAXY.Animation
{
    public abstract class AnimationClipSetBase
    {        
        public virtual AnimationClip AnimationClip => null;

        [HorizontalGroup(Width = 50f, MarginRight = 0.0125f)]
        [HideLabel]
        [SuffixLabel("X", overlay: true)]
        public float speed = 1;

#if UNITY_EDITOR
        [HorizontalGroup]
        [ToggleLeft]
        public bool showAeTool;

        public virtual AnimationClip AnimationToEdit => null;

        [TitleGroup("Animation Event Tool")]
        [HideLabel]
        [ShowIf("@AnimationToEdit && showAeTool")]
        [ShowInInspector]
        public AnimationEventEditor animationEventEditor;

        [TitleGroup("Animation Event Tool")]
        [ShowIf("@AnimationToEdit && showAeTool")]
        [Button]
        public void GetAnimationEvents()
        {
            if (animationEventEditor == null)
                animationEventEditor = new AnimationEventEditor();

            animationEventEditor.GetAnimationEvents(this);
        }

        [TitleGroup("Animation Event Tool")]
        public void GetPresetAE(AnimationEventsPresetSO presetSO)
        {
            AnimationEvent[] animationEvents = AnimationUtility.GetAnimationEvents(AnimationToEdit);

            List<AnimationEvent> newEvents = new List<AnimationEvent>(animationEvents);

            foreach (SingleEventEditor singleAeEditor in presetSO.SingleEventEditors)
            {
                bool isDuplicate = false;

                foreach (AnimationEvent originalAe in animationEvents)
                {
                    if (originalAe.functionName == singleAeEditor.FunctionName)
                    {
                        isDuplicate = true;
                        break; // Exit the inner loop once a match is found
                    }
                }

                if (!isDuplicate)
                {
                    AnimationEvent presetAE = new AnimationEvent
                    {
                        functionName = singleAeEditor.FunctionName,
                        intParameter = singleAeEditor.IntParameter,
                        floatParameter = singleAeEditor.FloatParameter,
                        stringParameter = singleAeEditor.StringParameter,
                    };

                    presetAE.time = Mathf.Lerp(0, AnimationToEdit.length, singleAeEditor.EventTimeRatio);

                    newEvents.Add(presetAE);
                }
            }

            animationEvents = newEvents.ToArray();

            AnimationUtility.SetAnimationEvents(AnimationToEdit, animationEvents);

            if (animationEventEditor == null)
                animationEventEditor = new AnimationEventEditor();

            animationEventEditor.GetAnimationEvents(this);
        }
#endif
    }

#if UNITY_EDITOR
    [HideReferenceObjectPicker]
    public class AnimationEventEditor
    {
        AnimationClipSetBase _clipSet;

        public AnimationClip AnimationClip => _clipSet.AnimationToEdit;
        AnimationEvent[] Events
        {
            get
            {
                if (AnimationClip == null)
                {
                    return null;
                }
                else
                {
                    return AnimationUtility.GetAnimationEvents(AnimationClip);
                }
            }
        }

        [Button]
        [GUIColor(0, 1, 0)]
        [ShowIf("@ShowUpdateAnimationEvent")]
        public void UpdateAnimationEvent()
        {

            if (AnimationClip == null)
            {
                CustomDebug.Log("Reference Missing... Click Get Animation Event !!!");
                return;
            }

            if (NewAnimationEvents == null)
            {
                AnimationUtility.SetAnimationEvents(AnimationClip, new AnimationEvent[0]);
            }
            else
            {
                AnimationUtility.SetAnimationEvents(AnimationClip, NewAnimationEvents);
            }

            GetAnimationEvents(_clipSet);
        }

        bool ShowUpdateAnimationEvent
        {
            get
            {
                if (AnimationClip == null)
                    return false;

                if (SingleEventEditors == null)
                    return false;

                foreach (var singleAeEditor in SingleEventEditors)
                {
                    if (singleAeEditor.EditModeOn)
                        return true;
                }

                if (SingleEventEditors.Count != Events.Length)
                    return true;

                return false;
            }
        }

        public void GetAnimationEvents(AnimationClipSetBase clipSet)
        {
            _clipSet = clipSet;

            if (AnimationClip == null || Events == null)
            {
                //Debug.Log("Reference Missing... Click Get Animation Event !!!");
                return;
            }

            List<SingleEventEditor> tempSingleAEEditors = new List<SingleEventEditor>();

            for (int i = 0; i < Events.Length; i++)
            {
                tempSingleAEEditors.Add(new SingleEventEditor(i, this));
            }

            SingleEventEditors = tempSingleAEEditors;
        }

        [ListDrawerSettings(ShowIndexLabels = true,
                            ListElementLabelName = "FunctionName",
                            CustomAddFunction = "AddAnimationEvent",
                            CustomRemoveIndexFunction = "RemoveAnimationEvent")]
        [LabelText("Animation Events")]
        [ShowInInspector]
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<SingleEventEditor> SingleEventEditors;
        SingleEventEditor AddAnimationEvent()
        {
            return new SingleEventEditor(SingleEventEditors.Count, this);
        }

        void RemoveAnimationEvent(int index)
        {
            if (SingleEventEditors[index].EditModeOn)
            {
                SingleEventEditors[index].EditModeOFF();
            }

            SingleEventEditors.RemoveAt(index);
        }

        public AnimationEvent[] NewAnimationEvents
        {
            get
            {
                if (SingleEventEditors == null || SingleEventEditors.Count == 0)
                {
                    return null;
                }

                AnimationEvent[] tempAE = new AnimationEvent[SingleEventEditors.Count];

                for (int i = 0; i < SingleEventEditors.Count; i++)
                {
                    if (SingleEventEditors[i].EditModeOn)
                        tempAE[i] = SingleEventEditors[i].newAnimationEvent;
                    else
                        tempAE[i] = SingleEventEditors[i].AnimationEvent;
                }

                return tempAE;
            }
        }
    }

    [HideReferenceObjectPicker]
    public class SingleEventEditor
    {
        AnimationEventEditor _animEventEditor;

        public AnimationClip AnimationClip => _animEventEditor.AnimationClip;

        [FoldoutGroup("Reference")]
        [ShowInInspector]
        [LabelText("Frame Rate")]
        public float AnimClipFrameRate
        {
            get
            {
                if (AnimationClip == null)
                    return -1f;

                return AnimationClip.frameRate;
            }
        }

        [FoldoutGroup("Reference")]
        [ShowInInspector]
        [LabelText("Duration")]
        public float AnimClipDuration
        {
            get
            {
                if (AnimationClip == null)
                    return -1f;

                return AnimationClip.length;
            }
        }

        [FoldoutGroup("Reference")]
        [HideInInspector]
        public AnimationEvent AnimationEvent
        {
            get
            {
                if (AnimationClip == null)
                    return null;

                if (animEvtIndex >= AnimationUtility.GetAnimationEvents(AnimationClip).Length)
                    return null;
                else
                {
                    return AnimationUtility.GetAnimationEvents(AnimationClip)[animEvtIndex];
                }
            }
        }

        [FoldoutGroup("Reference")]
        [ReadOnly]
        public AnimationEvent newAnimationEvent;

        [FoldoutGroup("Reference")]
        [ReadOnly]
        public int animEvtIndex;

        public bool EditModeOn { get; set; } = false;

        #region Read Only
        [ShowInInspector]
        [FoldoutGroup("Current AE Position")]
        public float EventTimeStamp
        {
            get
            {
                if (IsValid == false)
                {
                    return 0;
                }

                return AnimationEvent.time;
            }
        }

        [ShowInInspector]
        [FoldoutGroup("Current AE Position")]
        public float EventFrameStamp
        {
            get
            {
                if (IsValid == false)
                {
                    return 0;
                }

                // Calculate frame index based on time and clip frame rate
                return AnimationEvent.time * AnimationClip.frameRate;
            }
        }

        [ShowInInspector]
        [FoldoutGroup("Current AE Position")]
        public float EventTimeRatio
        {
            get
            {
                if (IsValid == false)
                {
                    return 0;
                }

                return EventTimeStamp / AnimClipDuration;
            }
        }

        [PropertySpace(5)]
        [ShowInInspector]
        [ShowIf("@!EditModeOn")]
        public string FunctionName => IsValid ? AnimationEvent.functionName : "Empty...";

        [ShowInInspector]
        [ShowIf("@!EditModeOn")]
        public int IntParameter => IsValid ? AnimationEvent.intParameter : 0;

        [ShowInInspector]
        [ShowIf("@!EditModeOn")]
        public float FloatParameter => IsValid ? AnimationEvent.floatParameter : 0;

        [ShowInInspector]
        [ShowIf("@!EditModeOn")]
        public string StringParameter => IsValid ? AnimationEvent.stringParameter : "";

        // Helper property to check validity
        private bool IsValid => AnimationEvent != null && AnimationClip != null;
        #endregion

        #region New Value
        [ShowIf("@EditModeOn")]
        [PropertyOrder(1)]
        [OnValueChanged("UpdateAnimationEvent")]
        public AnimationEventPosition AePosition;

        [PropertyOrder(1)]
        [ShowIf("@EditModeOn && AePosition == AnimationEventPosition.Custom")]
        public float newAePos;

        [PropertySpace(5)]
        [ShowIf("@EditModeOn")]
        [PropertyOrder(1)]
        [OnValueChanged("UpdateAnimationEvent")]
        public string newFunctionName;

        [ShowIf("@EditModeOn")]
        [PropertyOrder(1)]
        [OnValueChanged("UpdateAnimationEvent")]
        public int newIntParam;

        [ShowIf("@EditModeOn")]
        [PropertyOrder(1)]
        [OnValueChanged("UpdateAnimationEvent")]
        public float newFloatParam;

        [ShowIf("@EditModeOn")]
        [PropertyOrder(1)]
        [OnValueChanged("UpdateAnimationEvent")]
        public string newStringParam;
        #endregion

        public void UpdateAnimationEvent()
        {
            if (newAnimationEvent == null)
                newAnimationEvent = new AnimationEvent();

            switch (AePosition)
            {
                case AnimationEventPosition.None:
                    newAnimationEvent.time = EventTimeStamp;
                    break;

                case AnimationEventPosition.Start:
                    newAnimationEvent.time = 0f;
                    break;

                case AnimationEventPosition.Early:
                    newAnimationEvent.time = AnimClipDuration * 0.125f; // 12.5% (1/8 of the duration)
                    break;

                case AnimationEventPosition.FirstEighth:
                    newAnimationEvent.time = AnimClipDuration * 0.1875f; // 18.75% (3/16 of the duration)
                    break;

                case AnimationEventPosition.FirstQuarter:
                    newAnimationEvent.time = AnimClipDuration * 0.25f;
                    break;

                case AnimationEventPosition.BeforeMidpoint:
                    newAnimationEvent.time = AnimClipDuration * 0.375f; // 37.5% (3/8 of the duration)
                    break;

                case AnimationEventPosition.Midpoint:
                    newAnimationEvent.time = AnimClipDuration * 0.5f;
                    break;

                case AnimationEventPosition.AfterMidpoint:
                    newAnimationEvent.time = AnimClipDuration * 0.625f; // 62.5% (5/8 of the duration)
                    break;

                case AnimationEventPosition.ThirdQuarter:
                    newAnimationEvent.time = AnimClipDuration * 0.75f;
                    break;

                case AnimationEventPosition.Late:
                    newAnimationEvent.time = AnimClipDuration * 0.875f; // 87.5% (7/8 of the duration)
                    break;

                case AnimationEventPosition.End:
                    newAnimationEvent.time = AnimClipDuration;
                    break;

                case AnimationEventPosition.Custom:
                    newAnimationEvent.time = EventTimeStamp;
                    break;
            }

            newAnimationEvent.functionName = newFunctionName;
            newAnimationEvent.stringParameter = newStringParam;
            newAnimationEvent.intParameter = newIntParam;
            newAnimationEvent.floatParameter = newFloatParam;
        }

        [Button("Edit Mode OFF")]
        [ShowIf("@!EditModeOn")]
        [PropertyOrder(-1)]
        [PropertySpace(0, 5)]
        public void EditModeON()
        {
            EditModeOn = true;

            if (AnimationEvent == null)
                return;

            newAnimationEvent = new AnimationEvent
            {
                functionName = AnimationEvent.functionName,
                stringParameter = AnimationEvent.stringParameter,
                intParameter = AnimationEvent.intParameter,
                floatParameter = AnimationEvent.floatParameter,
                time = EventTimeStamp,
            };

            newFunctionName = newAnimationEvent.functionName;
            newStringParam = newAnimationEvent.stringParameter;
            newIntParam = newAnimationEvent.intParameter;
            newFloatParam = newAnimationEvent.floatParameter;

        }

        [Button("Edit Mode ON")]
        [GUIColor(0, 1, 0)]
        [ShowIf("@EditModeOn")]
        [PropertyOrder(-1)]
        [PropertySpace(0, 5)]
        public void EditModeOFF()
        {
            EditModeOn = false;
        }
        public SingleEventEditor(int animEventIndex, AnimationEventEditor aeEditor)
        {
            this.animEvtIndex = animEventIndex;
            this._animEventEditor = aeEditor;
        }
    }

    public class AnimationEventCompleteValidator
    {
        string _aePresetPath;
        AnimationClipSetBase _clipSet;

        AnimationEventsPresetSO AePreset
        {
            get
            {
                AnimationEventsPresetSO aePresetSO =
                    AssetDatabase.LoadAssetAtPath(_aePresetPath, typeof(AnimationEventsPresetSO)) as AnimationEventsPresetSO;

                return aePresetSO;
            }
        }

        [HorizontalGroup("Preset")]
        [Button("Get Preset Animation Event")]
        [GUIColor(0, 1, 0)]
        [PropertyOrder(1)]
        [ShowIf("@_showGetPreset")]
        public void GetPreset()
        {
            //EditorGUIUtility.PingObject(AePreset);
            _clipSet.GetPresetAE(AePreset);
        }

        [HorizontalGroup("Preset")]
        [ShowInInspector]
        [ToggleLeft]
        [LabelText("Complete")]
        [PropertyOrder(1)]
        [ShowIf("@_showGetPreset")]
        public bool AeComplete
        {
            get
            {
                if (AePreset == null || _clipSet == null)
                    return false;

                if (AePreset.AnimationEventEditor == null)
                    AePreset.ResetAeEditor();

                if (_clipSet.animationEventEditor == null)
                    _clipSet.GetAnimationEvents();

                // Pastikan SingleEventEditors tidak null
                List<SingleEventEditor> presetList = AePreset.SingleEventEditors ?? new List<SingleEventEditor>();
                List<SingleEventEditor> animList = _clipSet.animationEventEditor?.SingleEventEditors ?? new List<SingleEventEditor>();

                if (presetList.Count == 0)
                    return true; // Jika preset kosong, maka tidak ada yang perlu dicek

                if (animList.Count == 0)
                    return false; // Jika animasi kosong, maka pasti gagal

                return presetList.All(reqEvent => animList.Any(existEvent => existEvent.FunctionName == reqEvent.FunctionName));
            }
        }

        bool _showGetPreset
        {
            get
            {
                return _clipSet.AnimationToEdit != null;
            }
        }

        public void SetVariable(string aePresetPath, AnimationClipSetBase clipSet)
        {
            _aePresetPath = aePresetPath;
            _clipSet = clipSet;
        }
    }

    public enum AnimationEventPosition
    {
        None,
        Start,
        Early,         // New: Between Start and FirstQuarter
        FirstEighth,   // New: Between Early and FirstQuarter
        FirstQuarter,
        BeforeMidpoint,
        Midpoint,
        AfterMidpoint,
        ThirdQuarter,
        Late,          // New: Between ThirdQuarter and End
        End,
        Custom
    }
#endif
}
