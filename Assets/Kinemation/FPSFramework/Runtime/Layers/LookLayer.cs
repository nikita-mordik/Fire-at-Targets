// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Attributes;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    [Serializable]
    public struct AimOffsetBone
    {
        [Bone]
        public Transform bone;
        public Vector2 maxAngle;

        public AimOffsetBone(Transform bone, Vector2 maxAngle)
        {
            this.bone = bone;
            this.maxAngle = maxAngle;
        }
    }

    // Collection of AimOffsetBones, used to rotate spine bones to look around
    [Serializable]
    public struct AimOffset
    {
        [Fold(false)] public List<AimOffsetBone> bones;
        public int indexOffset;

        [HideInInspector] public List<Vector2> angles;
        
        public void Init()
        {
            if (angles == null)
            {
                angles = new List<Vector2>();
            }
            else
            {
                angles.Clear();
            }

            bones ??= new List<AimOffsetBone>();

            for (int i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];
                angles.Add(bone.maxAngle);
            }
        }
        
        public bool IsValid()
        {
            return bones != null && angles != null;
        }

        public bool IsChanged()
        {
            return bones.Count != angles.Count;
        }
    }
    
    public class LookLayer : AnimLayer
    {
        [SerializeField, Range(0f, 1f)] protected float pelvisLayerAlpha = 1f;
        [SerializeField] protected float pelvisLerpSpeed;
        protected float interpPelvis;
        
        [Header("Offsets")] 
        [SerializeField] protected Vector3 pelvisOffset;
        
        [SerializeField] protected AimOffsetTable aimOffsetTable;
        [SerializeField] protected AimOffset lookUpOffset;
        [SerializeField] protected AimOffset lookRightOffset;
        
        protected Transform[] characterBones;
        [SerializeField] protected AimOffset targetUpOffset;
        [SerializeField] protected AimOffset targetRightOffset;

        [FormerlySerializedAs("enableAutoDistribution")] 
        [SerializeField] protected bool autoDistribution;

        [SerializeField, Range(-90f, 90f)] protected float aimUp;
        [SerializeField, Range(-90f, 90f)] protected float aimRight;

        // Aim rotation lerp speed. If 0, no lag will be applied.
        [SerializeField] protected float smoothAim;

        [Header("Leaning")]
        [SerializeField] [Range(-1, 1)] protected int leanDirection;
        [SerializeField] protected float leanAmount = 45f;
        [SerializeField, Range(0f, 1f)] protected float pelvisLean = 0f;
        [SerializeField] protected float leanSpeed;

        [Header("Misc")]
        [SerializeField] protected bool useRightOffset = true;
        
        protected float leanInput;
        protected Vector2 lerpedAim;
  
        public void SetAimOffsetTable(AimOffsetTable table)
        {
            if (table == null)
            {
                return;
            }
            
            float aimOffsetAlpha = core.animGraph.GetPoseProgress();

            // Cache current max angle
            for (int i = 0; i < targetUpOffset.bones.Count; i++)
            {
                var bone = lookUpOffset.bones[i];
                bone.maxAngle = Vector2.Lerp(bone.maxAngle, targetUpOffset.bones[i].maxAngle, aimOffsetAlpha);
                lookUpOffset.bones[i] = bone;
            }
            
            for (int i = 0; i < targetRightOffset.bones.Count; i++)
            {
                var bone = lookRightOffset.bones[i];
                bone.maxAngle = Vector2.Lerp(bone.maxAngle, targetRightOffset.bones[i].maxAngle, aimOffsetAlpha);
                lookRightOffset.bones[i] = bone;
            }
            
            aimOffsetTable = table;
            
            for (int i = 0; i < aimOffsetTable.aimOffsetUp.Count; i++)
            {
                var bone = aimOffsetTable.aimOffsetUp[i];
                var newBone = new AimOffsetBone(characterBones[bone.boneIndex], bone.angle);
                targetUpOffset.bones[i] = newBone;
            }
            
            for (int i = 0; i < aimOffsetTable.aimOffsetRight.Count; i++)
            {
                var bone = aimOffsetTable.aimOffsetRight[i];
                var newBone = new AimOffsetBone(characterBones[bone.boneIndex], bone.angle);
                targetRightOffset.bones[i] = newBone;
            }
        }

        public void SetPelvisWeight(float weight)
        {
            pelvisLayerAlpha = Mathf.Clamp01(weight);
        }

        public override void InitializeLayer()
        {
            base.InitializeLayer();
            
            lookUpOffset.Init();
            lookRightOffset.Init();
            
            elbowsWeight = 1f;
            
            Transform[] bones = null;
            var boneContainer = GetComponentInChildren<BoneContainer>();
            if (boneContainer != null)
            {
                bones = boneContainer.boneContainer.ToArray();
            }
            else
            {
                var meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                if (meshRenderer != null)
                {
                    bones = meshRenderer.bones;
                }
            }
            
            if (bones == null)
            {
                Debug.LogWarning("[LookLayer]: No Skinned Mesh Renderer or Bone Container!");
                return;
            }
            
            characterBones = bones;

            targetUpOffset.bones = new List<AimOffsetBone>();
            targetRightOffset.bones = new List<AimOffsetBone>();

            // If no data asset selected copy the editor data
            if (aimOffsetTable == null)
            {
                foreach (var bone in lookUpOffset.bones)
                {
                    targetUpOffset.bones.Add(bone);
                }
                
                foreach (var bone in lookRightOffset.bones)
                {
                    targetRightOffset.bones.Add(bone);
                }
                return;
            }
            
            lookUpOffset.bones.Clear();
            lookRightOffset.bones.Clear();
            
            lookUpOffset.angles.Clear();
            lookRightOffset.angles.Clear();

            // Aim Offset Table contains bone indexes
            foreach (var bone in aimOffsetTable.aimOffsetUp)
            {
                targetUpOffset.bones.Add(new AimOffsetBone(characterBones[bone.boneIndex], bone.angle));
                lookUpOffset.bones.Add(new AimOffsetBone(characterBones[bone.boneIndex], bone.angle));
                lookUpOffset.angles.Add(bone.angle);
            }
            
            foreach (var bone in aimOffsetTable.aimOffsetRight)
            {
                targetRightOffset.bones.Add(new AimOffsetBone(characterBones[bone.boneIndex], bone.angle));
                lookRightOffset.bones.Add(new AimOffsetBone(characterBones[bone.boneIndex], bone.angle));
                lookRightOffset.angles.Add(bone.angle);
            }
        }

        public override bool CanUpdate()
        {
            return base.CanUpdate() || !Application.isPlaying;
        }

        public override void PreUpdateLayer()
        {
            base.PreUpdateLayer();
            UpdateSpineBlending();
        }

        public override void UpdateLayer()
        {
            RotateSpine();
        }

        private void DrawDefaultSpine()
        {
            int count = lookUpOffset.bones.Count;
            
            for (int i = 0; i < count - lookUpOffset.indexOffset; i++)
            {
                var pos = lookUpOffset.bones[i].bone.position;

                if (i > 0)
                {
                    var prevBone = lookUpOffset.bones[i - 1].bone.position;
                    CoreToolkitLib.DrawBone(prevBone, pos, 0.01f);
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!drawDebugInfo || lookUpOffset.bones == null)
            {
                return;
            }

            var color = Gizmos.color;
            
            Gizmos.color = Color.cyan;
            DrawDefaultSpine();
            Gizmos.color = color;
        }

        private void UpdateSpineBlending()
        {
            interpPelvis = CoreToolkitLib.Interp(interpPelvis, pelvisLayerAlpha * smoothLayerAlpha,
                pelvisLerpSpeed, Time.deltaTime);

            if (Application.isPlaying)
            {
                aimUp = GetCharData().totalAimInput.y;
                aimRight = GetCharData().totalAimInput.x;

                if (lookRightOffset.bones.Count == 0 || !useRightOffset)
                {
                    aimRight = 0f;
                }

                leanInput = CoreToolkitLib.Interp(leanInput, leanAmount * GetCharData().leanDirection,
                    leanSpeed, Time.deltaTime);
            }
            else
            {
                leanInput = CoreToolkitLib.Interp(leanInput, leanAmount * leanDirection, leanSpeed, Time.deltaTime);
            }
            
            lerpedAim.y = CoreToolkitLib.InterpLayer(lerpedAim.y, aimUp, smoothAim, Time.deltaTime);
            lerpedAim.x = CoreToolkitLib.InterpLayer(lerpedAim.x, aimRight, smoothAim, Time.deltaTime);
        }

        private void OffsetPelvis()
        {
            float normalLean = pelvisLean * -leanInput / leanAmount;
            Vector3 pelvisAdditive = pelvisOffset * interpPelvis + new Vector3(normalLean, 0f, 0f);
            CoreToolkitLib.MoveInBoneSpace(GetRootBone(), GetPelvis(), pelvisAdditive, 1f);
        }
        
        private void RotateSpine()
        {
            OffsetPelvis();
            
            float alpha = smoothLayerAlpha * (1f - GetCurveValue(CurveLib.Curve_MaskLookLayer));
            float aimOffsetAlpha = core.animGraph.GetPoseProgress();
            
            Quaternion rootBoneRotation = GetRootBone().rotation;
            Quaternion invRootBonRotation = Quaternion.Inverse(rootBoneRotation);
            float fraction = alpha * leanInput / 90f;
            
            if (!Mathf.Approximately(fraction, 0f))
            {
                for (int i = 0; i < lookRightOffset.bones.Count; i++)
                {
                    Transform boneTransform = targetRightOffset.bones[i].bone;
                    Vector2 cachedAngle = lookRightOffset.bones[i].maxAngle;
                    Vector2 targetAngle = targetRightOffset.bones[i].maxAngle;
                    
                    if (boneTransform == null)
                    {
                        continue;
                    }
                    
                    float angle = Mathf.Lerp(cachedAngle.x, targetAngle.x, aimOffsetAlpha);
                    Quaternion offset = Quaternion.Euler(0f, 0f, fraction * angle);
                    offset *= invRootBonRotation * boneTransform.rotation;
                    boneTransform.rotation = rootBoneRotation * offset;
                }
            }
            
            fraction = alpha * lerpedAim.x / 90f;
            bool useY = lerpedAim.x >= 0f;
            
            if (!Mathf.Approximately(fraction, 0f))
            {
                for (int i = 0; i < lookRightOffset.bones.Count; i++)
                {
                    Transform boneTransform = targetRightOffset.bones[i].bone;
                    Vector2 cachedAngle = lookRightOffset.bones[i].maxAngle;
                    Vector2 targetAngle = targetRightOffset.bones[i].maxAngle;
                    
                    if (boneTransform == null)
                    {
                        continue;
                    }

                    Vector2 angle = Vector2.Lerp(cachedAngle, targetAngle, aimOffsetAlpha);
                    Quaternion offset = Quaternion.Euler(0f, fraction * (useY ? angle.y : angle.x), 0f);
                    offset *= invRootBonRotation * boneTransform.rotation;
                    boneTransform.rotation = rootBoneRotation * offset;
                }
            }
            
            fraction = alpha * lerpedAim.y / 90f;
            
            if (Mathf.Approximately(fraction, 0f)) return;
            
            rootBoneRotation *= Quaternion.Euler(0f, lerpedAim.x, 0f);
            invRootBonRotation = Quaternion.Inverse(rootBoneRotation);
            
            useY = lerpedAim.y >= 0f;
            
            for (int i = 0; i < lookUpOffset.bones.Count; i++)
            {
                Transform boneTransform = targetUpOffset.bones[i].bone;
                Vector2 cachedAngle = lookUpOffset.bones[i].maxAngle;
                Vector2 targetAngle = targetUpOffset.bones[i].maxAngle;
                
                if (boneTransform == null)
                {
                    continue;
                }
                
                Vector2 angle = Vector2.Lerp(cachedAngle, targetAngle, aimOffsetAlpha);
                Quaternion offset = Quaternion.Euler(fraction * (useY ? angle.y : angle.x), 0f, 0f);
                offset *= invRootBonRotation * boneTransform.rotation;
                boneTransform.rotation = rootBoneRotation * offset;
            }
        }

        private void OnValidate()
        {
            if (!lookUpOffset.IsValid() || lookUpOffset.IsChanged())
            {
                lookUpOffset.Init();
            }

            if (!lookRightOffset.IsValid() || lookRightOffset.IsChanged())
            {
                lookRightOffset.Init();
            }

            void Distribute(ref AimOffset aimOffset)
            {
                if (autoDistribution)
                {
                    bool enable = false;
                    int divider = 1;
                    float sum = 0f;

                    int boneCount = aimOffset.bones.Count - aimOffset.indexOffset;

                    for (int i = 0; i < boneCount; i++)
                    {
                        if (enable)
                        {
                            var bone = aimOffset.bones[i];
                            bone.maxAngle.x = (90f - sum) / divider;
                            aimOffset.bones[i] = bone;
                            continue;
                        }

                        if (!Mathf.Approximately(aimOffset.bones[i].maxAngle.x, aimOffset.angles[i].x))
                        {
                            divider = boneCount - (i + 1);
                            enable = true;
                        }

                        sum += aimOffset.bones[i].maxAngle.x;
                    }

                    enable = false;
                    divider = 1;
                    sum = 0f;

                    for (int i = 0; i < boneCount; i++)
                    {
                        if (enable)
                        {
                            var bone = aimOffset.bones[i];
                            bone.maxAngle.y = (90f - sum) / divider;
                            aimOffset.bones[i] = bone;
                            continue;
                        }

                        if (!Mathf.Approximately(aimOffset.bones[i].maxAngle.y, aimOffset.angles[i].y))
                        {
                            divider = boneCount - (i + 1);
                            enable = true;
                        }

                        sum += aimOffset.bones[i].maxAngle.y;
                    }

                    // Copy max angles to angles list
                    for (int i = 0; i < boneCount; i++)
                    {
                        aimOffset.angles[i] = aimOffset.bones[i].maxAngle;
                    }
                }
            }

            if (lookUpOffset.bones.Count > 0)
            {
                Distribute(ref lookUpOffset);
            }

            if (lookRightOffset.bones.Count > 0)
            {
                Distribute(ref lookRightOffset);
            }
        }
        
#if UNITY_EDITOR
        public AimOffsetTable SaveTable()
        {
            Transform[] bones = null;
            var boneContainer = GetComponentInChildren<BoneContainer>();
            if (boneContainer != null)
            {
                bones = boneContainer.boneContainer.ToArray();
            }
            else
            {
                var meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                if (meshRenderer != null)
                {
                    bones = meshRenderer.bones;
                }
            }

            if (bones == null)
            {
                Debug.LogWarning("[LookLayer]: No Skinned Mesh Renderer or Bone Container!");
                return null;
            }

            int GetBoneIndex(Transform target)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    if (target == bones[i])
                    {
                        return i;
                    }
                }

                return 0;
            }

            if (aimOffsetTable != null)
            {
                aimOffsetTable.aimOffsetUp.Clear();
                aimOffsetTable.aimOffsetRight.Clear();

                foreach (var aimOffsetBone in lookUpOffset.bones)
                {
                    Vector2 angle = aimOffsetBone.maxAngle;
                    int boneIndex = GetBoneIndex(aimOffsetBone.bone);
                    aimOffsetTable.aimOffsetUp.Add(new BoneAngle(boneIndex, angle));
                }

                foreach (var aimOffsetBone in lookRightOffset.bones)
                {
                    Vector2 angle = aimOffsetBone.maxAngle;
                    int boneIndex = GetBoneIndex(aimOffsetBone.bone);
                    aimOffsetTable.aimOffsetRight.Add(new BoneAngle(boneIndex, angle));
                }

                return aimOffsetTable;
            }

            aimOffsetTable = ScriptableObject.CreateInstance<AimOffsetTable>();
            aimOffsetTable.aimOffsetRight = new List<BoneAngle>();
            aimOffsetTable.aimOffsetUp = new List<BoneAngle>();

            foreach (var aimOffsetBone in lookUpOffset.bones)
            {
                Vector2 angle = aimOffsetBone.maxAngle;
                int boneIndex = GetBoneIndex(aimOffsetBone.bone);
                aimOffsetTable.aimOffsetUp.Add(new BoneAngle(boneIndex, angle));
            }

            foreach (var aimOffsetBone in lookRightOffset.bones)
            {
                Vector2 angle = aimOffsetBone.maxAngle;
                int boneIndex = GetBoneIndex(aimOffsetBone.bone);
                aimOffsetTable.aimOffsetRight.Add(new BoneAngle(boneIndex, angle));
            }

            return aimOffsetTable;
        }
#endif
    }
}