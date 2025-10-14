using System;
using System.Threading.Tasks;
using RAXY.Utility.Addressable;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RAXY.Animation
{
    [Serializable]
    public class AnimationClipSet_Addressable : AnimationClipSetBase
    {
        [HorizontalGroup(Width = 0.55f, MarginRight = 0.0125f)]
        [HideLabel]
        [OnValueChanged("GetAnimationEvents")]
        [PropertyOrder(-1)]
        public AssetReferenceAnimationClip animationRef;

        [NonSerialized]
        AnimationClip _cachedAnimation;
        public override AnimationClip AnimationClip => _cachedAnimation;

        public async Task<AnimationClip> LoadAnimation(string cacherContainerKey)
        {
            _cachedAnimation = await AddressableCacher
                .TryGet<AnimationClip>(cacherContainerKey, animationRef);

            return _cachedAnimation;
        }

        public void SetCachedAnimation(AnimationClip clip)
        {
            _cachedAnimation = clip;
        }

#if UNITY_EDITOR
        public override AnimationClip AnimationToEdit
        {
            get
            {
                if (animationRef != null && animationRef.editorAsset != null)
                    return animationRef.editorAsset;

                return null;
            }
        }
#endif
    }

    [Serializable]
    public class AssetReferenceAnimationClip : AssetReferenceT<AnimationClip>
    {
        public AssetReferenceAnimationClip(string guid) : base(guid)
        {
        }
    }
}
