// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Attributes;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.Core.Playables;

using System;
using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.Core.Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kinemation.FPSFramework.Runtime.Core.Components
{
    // DynamicBone is essentially an IK bone
    [Serializable]
    public struct DynamicBone
    {
        [Tooltip("Target Skeleton Bone")] 
        [Bone] public Transform target;

        [Tooltip("Elbow/Knee Skeleton Bone")] 
        [Bone] public Transform hintTarget;
        
        [Tooltip("Elbow/Knee IK Object")]
        public GameObject hintObj;

        [Tooltip("Target IK Object")]
        public GameObject obj;

        private LocRot _cachedHintTransform;

        public void CacheHintTransform()
        {
            _cachedHintTransform = new LocRot(hintObj.transform);
        }

        public void BlendHintCachedTransform(float alpha = 1f)
        {
            LocRot targetTransform = new LocRot(hintObj.transform);
            targetTransform = LocRot.Lerp(_cachedHintTransform, targetTransform, alpha);

            hintObj.transform.position = targetTransform.position;
            hintObj.transform.rotation = targetTransform.rotation;
        }

        public void Retarget()
        {
            if (target == null)
            {
                return;
            }

            obj.transform.position = target.position;
            obj.transform.rotation = target.rotation;

            if (hintObj == null || hintTarget == null)
            {
                return;
            }
            
            hintObj.transform.position = hintTarget.position;
            hintObj.transform.rotation = hintTarget.rotation;
        }
        
        public void Offset(Transform parent, Quaternion rotation, float alpha = 1f)
        {
            CoreToolkitLib.RotateInBoneSpace(parent.rotation, obj.transform, rotation, alpha);
        }

        public void Offset(Quaternion parent, Quaternion rotation, float alpha = 1f)
        {
            CoreToolkitLib.RotateInBoneSpace(parent, obj.transform, rotation, alpha);
        }

        public void Offset(Quaternion rotation, float alpha = 1f)
        {
            CoreToolkitLib.RotateInBoneSpace(obj.transform.rotation, obj.transform, rotation, alpha);
        }

        public void Offset(Transform parent, Vector3 offset, float alpha = 1f)
        {
            CoreToolkitLib.MoveInBoneSpace(parent, obj.transform, offset, alpha);
        }

        public void Offset(Vector3 offset, float alpha = 1f)
        {
            CoreToolkitLib.MoveInBoneSpace(obj.transform, obj.transform, offset, alpha);
        }

        public void Override(Transform space, Vector3 offset, float alpha = 1f)
        {
            obj.transform.position = Vector3.Lerp(obj.transform.position, space.TransformPoint(offset), alpha);
        }
        
        public void Override(Vector3 offset, float alpha = 1f)
        {
            obj.transform.position = Vector3.Lerp(obj.transform.position, offset, alpha);
        }
        
        public void Override(Transform space, Quaternion offset, float alpha = 1f)
        {
            obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, space.rotation * offset, alpha);
        }
        
        public void Override(Quaternion offset, float alpha = 1f)
        {
            obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, offset, alpha);
        }
        
        public void Override(Transform space, LocRot offset, float alpha = 1f)
        {
            Override(space, offset.position, alpha);
            Override(space, offset.rotation, alpha);
        }
        
        public void Override(LocRot offset, float alpha = 1f)
        {
            Override(offset.position, alpha);
            Override(offset.rotation, alpha);
        }
    }

    // Essential skeleton data, used by Anim Layers
    [Serializable]
    public struct DynamicRigData
    {
        public AnimationClip tPose;
    
        public Animator animator;
        [Bone] public Transform pelvis;

        [Tooltip("Check if your rig has an IK gun bone")]
        public Transform weaponBone;
        
        [Tooltip("Check if your rig has an IK gun bone")]
        public Transform weaponBoneAdditive;

        public Transform weaponBoneRight;
        public Transform weaponBoneLeft;

        [HideInInspector] public float weaponBoneWeight;
        [HideInInspector] public LocRot weaponTransform;
        [HideInInspector] public float aimWeight;

        public DynamicBone masterDynamic;
        public DynamicBone rightHand;
        public DynamicBone leftHand;
        public DynamicBone rightFoot;
        public DynamicBone leftFoot;

        [Tooltip("Used for mesh space calculations")] [Bone]
        public Transform spineRoot;

        [Bone] public Transform rootBone;

        public Quaternion GetPelvisMS()
        {
            return Quaternion.Inverse(rootBone.rotation) * pelvis.rotation;
        }

        public void RetargetHandBones()
        {
            weaponBoneRight.position = weaponBone.position;
            weaponBoneRight.rotation = weaponBone.rotation;

            weaponBoneLeft.position = weaponBoneRight.position;
            weaponBoneLeft.rotation = weaponBoneRight.rotation;
        }

        public void RetargetWeaponBone()
        {
            weaponBone.position = rootBone.TransformPoint(weaponTransform.position);
            weaponBone.rotation = rootBone.rotation * weaponTransform.rotation;
        }

        public void UpdateWeaponParent()
        {
            LocRot boneDefault = new LocRot(weaponBoneRight);
            LocRot boneRight = new LocRot(masterDynamic.obj.transform);
            LocRot boneLeft = new LocRot(weaponBoneLeft);

            LocRot result = weaponBoneWeight >= 0f
                ? LocRot.Lerp(boneDefault, boneRight, weaponBoneWeight)
                : LocRot.Lerp(boneDefault, boneLeft, -weaponBoneWeight);

            masterDynamic.obj.transform.position = result.position;
            masterDynamic.obj.transform.rotation = result.rotation;
        }

        public void AlignWeaponBone(Vector3 offset)
        {
            if (!Application.isPlaying) return;

            masterDynamic.Offset(offset, 1f);

            weaponBone.position = masterDynamic.obj.transform.position;
            weaponBone.rotation = masterDynamic.obj.transform.rotation;
        }

        public void Retarget()
        {
            rightFoot.Retarget();
            leftFoot.Retarget();
        }
    }
    
    [ExecuteInEditMode, AddComponentMenu("FPS Animator")]
    public class CoreAnimComponent : MonoBehaviour
    {
        public UnityEvent onPreUpdate;
        public UnityEvent onPostUpdate;
        
        [FormerlySerializedAs("rigData")] public DynamicRigData ikRigData;
        public CharAnimData characterData;

        public WeaponAnimAsset weaponAsset;
        [HideInInspector] public WeaponTransformData weaponTransformData;

        [HideInInspector] public CoreAnimGraph animGraph;
        [SerializeField] [HideInInspector] private List<AnimLayer> animLayers;
        [SerializeField] private bool useIK = true;

        [SerializeField] private bool drawDebug;

        private bool _updateInEditor = false;
        private float _interpHands;
        private float _interpLayer;
        
        private Quaternion _pelvisPoseMS = Quaternion.identity;
        private Quaternion _pelvisPoseMSCache = Quaternion.identity;
        
        // Static weapon bone pose in mesh space
        private LocRot _weaponBonePose;
        private LocRot _weaponBoneSpinePose;
        
        private bool _isPivotValid = false;

        private Tuple<float, float> _rightHandWeight = new(1f, 1f);
        private Tuple<float, float> _leftHandWeight = new(1f, 1f);
        private Tuple<float, float> _rightFootWeight = new(1f, 1f);
        private Tuple<float, float> _leftFootWeight = new(1f, 1f);
        
        private JobHandle _ikJobHandle;
        private NativeArray<TwoBoneIkJobData> _ikDataArray;

        private void ScheduleIkJobs()
        {
            if (!useIK) return;
            
            void UpdateJobData(DynamicBone tipBone, Tuple<float, float> weights,
                int index)
            {
                Transform mid = tipBone.target.parent;
                Transform root = mid.parent;
                
                _ikDataArray[index] = new TwoBoneIkJobData()
                {
                    effectorWeight = weights.Item1,
                    hintWeight = weights.Item2,

                    root = new LocRot(root),
                    mid = new LocRot(mid),
                    tip = new LocRot(tipBone.target),

                    effectorTarget = new LocRot(tipBone.obj.transform),
                    hasValidHint = tipBone.hintObj != null,
                    hintTarget = tipBone.hintObj != null ? new LocRot(tipBone.hintObj.transform) : LocRot.identity
                };
            }
            
            UpdateJobData(ikRigData.rightHand, _rightHandWeight, 0);
            UpdateJobData(ikRigData.leftHand, _leftHandWeight, 1);
            UpdateJobData(ikRigData.rightFoot, _rightFootWeight, 2);
            UpdateJobData(ikRigData.leftFoot, _leftFootWeight, 3);

            var job = new TwoBoneIKJob()
            {
                jobData = _ikDataArray
            };

            _ikJobHandle = job.Schedule(4, 1);
        }

        private void CompleteIkJobs()
        {
            void ApplyIk(ref DynamicBone bone, int index)
            {
                Transform tip = bone.target;
                Transform mid = tip.parent;
                Transform root = mid.parent;

                var jobData = _ikDataArray[index];

                root.position = jobData.root.position;
                root.rotation = jobData.root.rotation;
                
                mid.position = jobData.mid.position;
                mid.rotation = jobData.mid.rotation;
                
                tip.position = jobData.tip.position;
                tip.rotation = jobData.tip.rotation;
            }
            
            _ikJobHandle.Complete();

            ApplyIk(ref ikRigData.rightHand, 0);
            ApplyIk(ref ikRigData.leftHand, 1);
            ApplyIk(ref ikRigData.rightFoot, 2);
            ApplyIk(ref ikRigData.leftFoot, 3);
        }

        private void OnEnable()
        {
            animLayers ??= new List<AnimLayer>();
            animGraph = GetComponent<CoreAnimGraph>();

            if (animGraph == null)
            {
                animGraph = gameObject.AddComponent<CoreAnimGraph>();
            }
        }

        public void InitializeComponent()
        {
            foreach (var layer in animLayers)
            {
                layer.InitializeLayer();
            }
            
            ikRigData.weaponTransform = LocRot.identity;
            ikRigData.weaponBoneRight.localPosition = ikRigData.weaponBoneLeft.localPosition = Vector3.zero;
            ikRigData.weaponBoneRight.localRotation = ikRigData.weaponBoneLeft.localRotation = Quaternion.identity;
            
            _ikDataArray = new NativeArray<TwoBoneIkJobData>(4, Allocator.Persistent);

            if (!Application.isPlaying) return;
            
            var additiveBone = new GameObject("WeaponBoneAdditive")
            {
                transform =
                {
                    parent = ikRigData.rootBone,
                    localRotation = Quaternion.identity,
                    localPosition = Vector3.zero
                }
            };
            
            ikRigData.weaponBoneAdditive = additiveBone.transform;
        }

        private void UpdateSpineStabilization()
        {
            Transform spineRoot = ikRigData.spineRoot;
            
            Quaternion rootWorldRotation = ikRigData.rootBone.rotation;
            Quaternion invRootWorldRotation = Quaternion.Inverse(rootWorldRotation);
            
            Quaternion pelvisWorldRotation =
                rootWorldRotation * Quaternion.Slerp(_pelvisPoseMSCache, _pelvisPoseMS, animGraph.GetPoseProgress());

            Quaternion stableRotation = pelvisWorldRotation * spineRoot.localRotation;
            stableRotation = Quaternion.Slerp(spineRoot.rotation, stableRotation, animGraph.GetGraphWeight());

            spineRoot.rotation = rootWorldRotation * (animGraph.GetSpineOffset() * (invRootWorldRotation * stableRotation));
        }

        private void UpdateWeaponBone()
        {
            // Not parented to the right or left hand
            if (ikRigData.weaponBoneWeight > 0f)
            {
                LocRot basePose = _weaponBonePose.FromSpace(ikRigData.rootBone);
                LocRot combinedPose = _weaponBoneSpinePose.FromSpace(ikRigData.spineRoot);

                combinedPose.position -= basePose.position;
                combinedPose.rotation = Quaternion.Inverse(basePose.rotation) * combinedPose.rotation;
                
                ikRigData.weaponBone.position += combinedPose.position;
                ikRigData.weaponBone.rotation *= combinedPose.rotation;
            }
            
            ikRigData.masterDynamic.Retarget();
            ikRigData.UpdateWeaponParent();
            
            var rotOffset = weaponAsset != null ? weaponAsset.rotationOffset : Quaternion.identity;
            ikRigData.masterDynamic.Offset(rotOffset);
            
            var pivotOffset = _isPivotValid ? weaponTransformData.pivotPoint.localPosition : Vector3.zero;
            ikRigData.masterDynamic.Offset(pivotOffset);
            
            ikRigData.rightHand.Retarget();
            ikRigData.leftHand.Retarget();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && (!_updateInEditor || !animGraph.IsPlaying()))
            {
                return;
            }
#endif
            onPreUpdate.Invoke();
            PreUpdateLayers();
            
            ikRigData.Retarget();
            
            UpdateSpineStabilization();
            UpdateWeaponBone();
            UpdateLayers();
            ScheduleIkJobs();
            
            var pivotOffset = _isPivotValid ? weaponTransformData.pivotPoint.localPosition : Vector3.zero;
            ikRigData.AlignWeaponBone(-pivotOffset);

            CompleteIkJobs();
            
            onPostUpdate.Invoke();
        }

        private void OnDestroy()
        {
            onPreUpdate = onPostUpdate = null;

            if (_ikDataArray.IsCreated)
            {
                _ikDataArray.Dispose();
            }
        }
        
        // Called right after retargeting
        private void PreUpdateLayers()
        {
            foreach (var layer in animLayers)
            {
                if (!Application.isPlaying && !layer.runInEditor || layer.CanUseParallelExecution())
                {
                    continue;
                }

                layer.PreUpdateLayer();
            }
        }

        private void UpdateLayers()
        {
            bool isPlaying = Application.isPlaying;
            
            foreach (var layer in animLayers)
            {
                if (!isPlaying && !layer.runInEditor || !layer.CanUpdate())
                {
                    continue;
                }
                
                ikRigData.rightHand.CacheHintTransform();
                ikRigData.leftHand.CacheHintTransform();

                if (isPlaying && layer.CanUseParallelExecution())
                {
                    layer.CompleteJobs();
                }
                else
                {
                    layer.UpdateLayer();
                }
                
                float weight = layer.elbowsWeight;
                ikRigData.rightHand.BlendHintCachedTransform(weight);
                ikRigData.leftHand.BlendHintCachedTransform(weight);
            }
        }
        
        // Called right before the pose sampling
        public void OnPrePoseSampled()
        {
            // Overwrite the weaponBone transform with the user data
            // Might be overwritten by the static pose after the pose is sampled
            LocRot target = new LocRot()
            {
                position = ikRigData.rootBone.TransformPoint(ikRigData.weaponTransform.position),
                rotation = ikRigData.rootBone.rotation * ikRigData.weaponTransform.rotation
            };

            ikRigData.weaponBone.position = target.position;
            ikRigData.weaponBone.rotation = target.rotation;
        }

        // Called after the pose is sampled
        public void OnPoseSampled()
        {
            ikRigData.RetargetHandBones();
            _pelvisPoseMSCache = _pelvisPoseMS;
            _pelvisPoseMS = ikRigData.GetPelvisMS();

            _weaponBonePose = new LocRot(ikRigData.weaponBone, false);
            _weaponBoneSpinePose = new LocRot(ikRigData.weaponBone).ToSpace(ikRigData.spineRoot);
            
            foreach (var layer in animLayers)
            {
                if (!Application.isPlaying && !layer.runInEditor)
                {
                    continue;
                }

                layer.OnPoseSampled();
            }
        }
        
        public void OnGunEquipped(WeaponAnimAsset asset, WeaponTransformData data)
        {
            weaponAsset = asset;
            weaponTransformData = data;
            _isPivotValid = weaponTransformData.pivotPoint != null;
        }

        public void OnSightChanged(Transform newSight)
        {
            weaponTransformData.aimPoint = newSight;
        }

        public void SetCharData(CharAnimData data)
        {
            characterData = data;
        }

        public void ScheduleJobs()
        {
            if (!Application.isPlaying) return;
            
            foreach (var layer in animLayers)
            {
                if(!layer.CanUseParallelExecution()) continue;
                
                layer.PreUpdateLayer();
                layer.ScheduleJobs();
            }
        }

        public void SetRightHandIKWeight(float effector, float hint)
        {
            _rightHandWeight = Tuple.Create(effector, hint);
        }

        public void SetLeftHandIKWeight(float effector, float hint)
        {
            _leftHandWeight = Tuple.Create(effector, hint);
        }

        public void SetRightFootIKWeight(float effector, float hint)
        {
            _rightFootWeight = Tuple.Create(effector, hint);
        }

        public void SetLeftFootIKWeight(float effector, float hint)
        {
            _leftFootWeight = Tuple.Create(effector, hint);
        }

// Editor utils
#if UNITY_EDITOR
        public void EnableEditorPreview()
        {
            if (_updateInEditor) return;
            
            if (ikRigData.animator == null)
            {
                ikRigData.animator = GetComponent<Animator>();
            }

            InitializeComponent();

            animGraph.StartPreview();
            EditorApplication.QueuePlayerLoopUpdate();
            _updateInEditor = true;
        }

        public void DisableEditorPreview()
        {
            if (!_updateInEditor) return;
            
            _updateInEditor = false;

            if (ikRigData.animator == null)
            {
                return;
            }

            animGraph.StopPreview();
            ikRigData.animator.Rebind();
            ikRigData.animator.Update(0f);

            if (ikRigData.tPose != null)
            {
                ikRigData.tPose.SampleAnimation(gameObject, 0f);
            }

            ikRigData.weaponBone.localPosition = Vector3.zero;
            ikRigData.weaponBone.localRotation = Quaternion.identity;

            if (_ikDataArray.IsCreated) _ikDataArray.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (drawDebug)
            {
                Gizmos.color = Color.green;

                void DrawDynamicBone(ref DynamicBone bone, string boneName)
                {
                    if (bone.obj != null)
                    {
                        var loc = bone.obj.transform.position;
                        Gizmos.DrawWireSphere(loc, 0.03f);
                        Handles.Label(loc, boneName);
                    }
                }

                DrawDynamicBone(ref ikRigData.rightHand, "RightHandIK");
                DrawDynamicBone(ref ikRigData.leftHand, "LeftHandIK");
                DrawDynamicBone(ref ikRigData.rightFoot, "RightFootIK");
                DrawDynamicBone(ref ikRigData.leftFoot, "LeftFootIK");

                Gizmos.color = Color.blue;
                if (ikRigData.rootBone != null)
                {
                    var mainBone = ikRigData.rootBone.position;
                    Gizmos.DrawWireCube(mainBone, new Vector3(0.1f, 0.1f, 0.1f));
                    Handles.Label(mainBone, "rootBone");
                }
            }
        }

        public void SetupBones()
        {
            if (ikRigData.animator == null)
            {
                ikRigData.animator = GetComponent<Animator>();
            }

            if (ikRigData.rootBone == null)
            {
                var root = transform.Find("rootBone");

                if (root != null)
                {
                    ikRigData.rootBone = root.transform;
                }
                else
                {
                    var bone = new GameObject("rootBone");
                    bone.transform.parent = transform;
                    ikRigData.rootBone = bone.transform;
                    ikRigData.rootBone.localPosition = Vector3.zero;
                }
            }

            if (ikRigData.weaponBone == null)
            {
                var gunBone = ikRigData.rootBone.Find("WeaponBone");

                if (gunBone != null)
                {
                    ikRigData.weaponBone = gunBone.transform;
                }
                else
                {
                    var bone = new GameObject("WeaponBone");
                    bone.transform.parent = ikRigData.rootBone;
                    ikRigData.weaponBone = bone.transform;
                    ikRigData.weaponBone.localPosition = Vector3.zero;
                }
            }

            if (ikRigData.rightFoot.obj == null)
            {
                var bone = transform.Find("RightFootIK");

                if (bone != null)
                {
                    ikRigData.rightFoot.obj = bone.gameObject;
                }
                else
                {
                    ikRigData.rightFoot.obj = new GameObject("RightFootIK");
                    ikRigData.rightFoot.obj.transform.parent = transform;
                    ikRigData.rightFoot.obj.transform.localPosition = Vector3.zero;
                }
            }

            if (ikRigData.leftFoot.obj == null)
            {
                var bone = transform.Find("LeftFootIK");

                if (bone != null)
                {
                    ikRigData.leftFoot.obj = bone.gameObject;
                }
                else
                {
                    ikRigData.leftFoot.obj = new GameObject("LeftFootIK");
                    ikRigData.leftFoot.obj.transform.parent = transform;
                    ikRigData.leftFoot.obj.transform.localPosition = Vector3.zero;
                }
            }

            if (ikRigData.animator.isHuman)
            {
                ikRigData.pelvis = ikRigData.animator.GetBoneTransform(HumanBodyBones.Hips);
                ikRigData.spineRoot = ikRigData.animator.GetBoneTransform(HumanBodyBones.Spine);
                ikRigData.rightHand.target = ikRigData.animator.GetBoneTransform(HumanBodyBones.RightHand);
                ikRigData.rightHand.hintTarget = ikRigData.animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                ikRigData.leftHand.target = ikRigData.animator.GetBoneTransform(HumanBodyBones.LeftHand);
                ikRigData.leftHand.hintTarget = ikRigData.animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                ikRigData.rightFoot.target = ikRigData.animator.GetBoneTransform(HumanBodyBones.RightFoot);
                ikRigData.rightFoot.hintTarget = ikRigData.animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                ikRigData.leftFoot.target = ikRigData.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                ikRigData.leftFoot.hintTarget = ikRigData.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);

                Transform head = ikRigData.animator.GetBoneTransform(HumanBodyBones.Head);
                SetupIKBones(head);
                SetupWeaponBones();
                return;
            }

            var meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogWarning("Core: Skinned Mesh Renderer not found!");
                return;
            }

            var children = meshRenderer.bones;

            bool foundRightHand = false;
            bool foundLeftHand = false;
            bool foundRightFoot = false;
            bool foundLeftFoot = false;
            bool foundHead = false;
            bool foundPelvis = false;

            foreach (var bone in children)
            {
                if (bone.name.ToLower().Contains("ik"))
                {
                    continue;
                }

                bool bMatches = bone.name.ToLower().Contains("hips") || bone.name.ToLower().Contains("pelvis");
                if (!foundPelvis && bMatches)
                {
                    ikRigData.pelvis = bone;
                    foundPelvis = true;
                    continue;
                }

                bMatches = bone.name.ToLower().Contains("lefthand") || bone.name.ToLower().Contains("hand_l")
                                                                    || bone.name.ToLower().Contains("l_hand")
                                                                    || bone.name.ToLower().Contains("hand l")
                                                                    || bone.name.ToLower().Contains("l hand")
                                                                    || bone.name.ToLower().Contains("l.hand")
                                                                    || bone.name.ToLower().Contains("hand.l")
                                                                    || bone.name.ToLower().Contains("hand_left")
                                                                    || bone.name.ToLower().Contains("left_hand");
                if (!foundLeftHand && bMatches)
                {
                    ikRigData.leftHand.target = bone;

                    if (ikRigData.leftHand.hintTarget == null)
                    {
                        ikRigData.leftHand.hintTarget = bone.parent;
                    }
                    
                    foundLeftHand = true;
                    continue;
                }

                bMatches = bone.name.ToLower().Contains("righthand") || bone.name.ToLower().Contains("hand_r")
                                                                     || bone.name.ToLower().Contains("r_hand")
                                                                     || bone.name.ToLower().Contains("hand r")
                                                                     || bone.name.ToLower().Contains("r hand")
                                                                     || bone.name.ToLower().Contains("r.hand")
                                                                     || bone.name.ToLower().Contains("hand.r")
                                                                     || bone.name.ToLower().Contains("hand_right")
                                                                     || bone.name.ToLower().Contains("right_hand");
                if (!foundRightHand && bMatches)
                {
                    ikRigData.rightHand.target = bone;

                    if (ikRigData.rightHand.hintTarget == null)
                    {
                        ikRigData.rightHand.hintTarget = bone.parent;
                    }
                    
                    foundRightHand = true;
                }

                bMatches = bone.name.ToLower().Contains("rightfoot") || bone.name.ToLower().Contains("foot_r")
                                                                     || bone.name.ToLower().Contains("r_foot")
                                                                     || bone.name.ToLower().Contains("foot_right")
                                                                     || bone.name.ToLower().Contains("right_foot")
                                                                     || bone.name.ToLower().Contains("foot r")
                                                                     || bone.name.ToLower().Contains("r foot")
                                                                     || bone.name.ToLower().Contains("r.foot")
                                                                     || bone.name.ToLower().Contains("foot.r");
                if (!foundRightFoot && bMatches)
                {
                    ikRigData.rightFoot.target = bone;
                    ikRigData.rightFoot.hintTarget = bone.parent;

                    foundRightFoot = true;
                }

                bMatches = bone.name.ToLower().Contains("leftfoot") || bone.name.ToLower().Contains("foot_l")
                                                                    || bone.name.ToLower().Contains("l_foot")
                                                                    || bone.name.ToLower().Contains("foot l")
                                                                    || bone.name.ToLower().Contains("foot_left")
                                                                    || bone.name.ToLower().Contains("left_foot")
                                                                    || bone.name.ToLower().Contains("l foot")
                                                                    || bone.name.ToLower().Contains("l.foot")
                                                                    || bone.name.ToLower().Contains("foot.l");
                if (!foundLeftFoot && bMatches)
                {
                    ikRigData.leftFoot.target = bone;
                    ikRigData.leftFoot.hintTarget = bone.parent;

                    foundLeftFoot = true;
                }

                if (!foundHead && bone.name.ToLower().Contains("head"))
                {
                    SetupIKBones(bone);
                    foundHead = true;
                }
            }

            SetupWeaponBones();
            
            bool bFound = foundRightHand && foundLeftHand && foundRightFoot && foundLeftFoot && foundHead &&
                          foundPelvis;

            Debug.Log(bFound ? "All bones are found!" : "Some bones are missing!");
        }

        private void SetupIKBones(Transform head)
        {
            if (ikRigData.masterDynamic.obj == null)
            {
                var boneObject = head.transform.Find("MasterIK");

                if (boneObject != null)
                {
                    ikRigData.masterDynamic.obj = boneObject.gameObject;
                }
                else
                {
                    ikRigData.masterDynamic.obj = new GameObject("MasterIK");
                    ikRigData.masterDynamic.obj.transform.parent = head;
                    ikRigData.masterDynamic.obj.transform.localPosition = Vector3.zero;
                }
            }
            
            ikRigData.masterDynamic.target = ikRigData.weaponBone;

            if (ikRigData.rightHand.obj == null)
            {
                var boneObject = ikRigData.masterDynamic.obj.transform.Find("RightHandIK");

                if (boneObject != null)
                {
                    ikRigData.rightHand.obj = boneObject.gameObject;
                }
                else
                {
                    ikRigData.rightHand.obj = new GameObject("RightHandIK");
                }

                ikRigData.rightHand.obj.transform.parent = ikRigData.masterDynamic.obj.transform;
                ikRigData.rightHand.obj.transform.localPosition = Vector3.zero;
            }

            if (ikRigData.rightHand.hintObj == null)
            {
                var boneObject = ikRigData.masterDynamic.obj.transform.Find("RightElbowIK");

                if (boneObject != null)
                {
                    ikRigData.rightHand.hintObj = boneObject.gameObject;
                }
                else
                {
                    ikRigData.rightHand.hintObj = new GameObject("RightElbowIK");
                }

                ikRigData.rightHand.hintObj.transform.parent = ikRigData.masterDynamic.obj.transform;
                ikRigData.rightHand.hintObj.transform.localPosition = Vector3.zero;
            }
            
            if (ikRigData.leftHand.obj == null)
            {
                var boneObject = ikRigData.masterDynamic.obj.transform.Find("LeftHandIK");

                if (boneObject != null)
                {
                    ikRigData.leftHand.obj = boneObject.gameObject;
                }
                else
                {
                    ikRigData.leftHand.obj = new GameObject("LeftHandIK");
                }

                ikRigData.leftHand.obj.transform.parent = ikRigData.masterDynamic.obj.transform;
                ikRigData.leftHand.obj.transform.localPosition = Vector3.zero;
            }
            
            if (ikRigData.leftHand.hintObj == null)
            {
                var boneObject = ikRigData.masterDynamic.obj.transform.Find("LeftElbowIK");

                if (boneObject != null)
                {
                    ikRigData.leftHand.hintObj = boneObject.gameObject;
                }
                else
                {
                    ikRigData.leftHand.hintObj = new GameObject("LeftElbowIK");
                }

                ikRigData.leftHand.hintObj.transform.parent = ikRigData.masterDynamic.obj.transform;
                ikRigData.leftHand.hintObj.transform.localPosition = Vector3.zero;
            }
        }

        private void SetupWeaponBones()
        {
            var rightHand = ikRigData.rightHand.target;
            var lefTHand= ikRigData.leftHand.target;
            
            if (rightHand != null && ikRigData.weaponBoneRight == null)
            {
                var boneObject = rightHand.Find("WeaponBoneRight");

                if (boneObject == null)
                {
                    var weaponBone = new GameObject("WeaponBoneRight");
                    ikRigData.weaponBoneRight = weaponBone.transform;
                    ikRigData.weaponBoneRight.parent = rightHand;
                }
                else
                {
                    ikRigData.weaponBoneRight = boneObject;
                }
            }
            
            if (lefTHand != null && ikRigData.weaponBoneLeft == null)
            {
                var boneObject = lefTHand.Find("WeaponBoneLeft");

                if (boneObject == null)
                {
                    var weaponBone = new GameObject("WeaponBoneLeft");
                    ikRigData.weaponBoneLeft = weaponBone.transform;
                    ikRigData.weaponBoneLeft.parent = lefTHand;
                }
                else
                {
                    ikRigData.weaponBoneLeft = boneObject;
                }
            }
        }

        public void AddLayer(AnimLayer newLayer)
        {
            animLayers.Add(newLayer);
        }

        public void RemoveLayer(int index)
        {
            if (index < 0 || index > animLayers.Count - 1)
            {
                return;
            }

            var toRemove = animLayers[index];
            animLayers.RemoveAt(index);
            DestroyImmediate(toRemove, true);
        }

        public bool IsLayerUnique(Type layer)
        {
            bool isUnique = true;
            foreach (var item in animLayers)
            {
                if (item.GetType() == layer)
                {
                    isUnique = false;
                    break;
                }
            }

            return isUnique;
        }

        public AnimLayer GetLayer(int index)
        {
            if (index < 0 || index > animLayers.Count - 1)
            {
                return null;
            }

            return animLayers[index];
        }

        public bool HasA(AnimLayer item)
        {
            return animLayers.Contains(item);
        }
#endif
    }
}