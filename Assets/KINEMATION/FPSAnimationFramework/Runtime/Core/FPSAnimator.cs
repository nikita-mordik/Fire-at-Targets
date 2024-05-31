// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Camera;
using KINEMATION.FPSAnimationFramework.Runtime.Playables;
using KINEMATION.KAnimationCore.Runtime.Input;
using KINEMATION.KAnimationCore.Runtime.Rig;

using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KINEMATION.FPSAnimationFramework.Runtime.Core
{
    [HelpURL("https://kinemation.gitbook.io/scriptable-animation-system/workflow/components")]
    [RequireComponent(typeof(FPSBoneController), typeof(UserInputController))]
    public class FPSAnimator : MonoBehaviour
    {
        public bool HasLinkedProfile { get; private set; }

        [SerializeField] protected FPSAnimatorProfile animatorProfile;
        protected FPSBoneController _boneController;
        protected UserInputController _inputController;
        protected FPSCameraController _cameraController;
        
        protected IPlayablesController _playablesController;

        protected virtual void Start()
        {
            _boneController = GetComponent<FPSBoneController>();
            _inputController = GetComponent<UserInputController>();
            _playablesController = GetComponent<IPlayablesController>();
            _cameraController = GetComponentInChildren<FPSCameraController>();

            _inputController.Initialize();
            _playablesController.InitializeController();
            _boneController.Initialize();
            
            if(_cameraController != null) _cameraController.Initialize();
            
            _boneController.LinkAnimatorProfile(animatorProfile);
        }

        protected virtual void Update()
        {
            if (_boneController == null) return;
            _boneController.GameThreadUpdate();
        }

        protected virtual void LateUpdate()
        {
            if (_boneController == null && _cameraController != null)
            {
                _cameraController.UpdateCamera();
                return;
            }
            
            // Caches the active pose in case of blending.
            _boneController.CachePose();
            // Apply procedural animation.
            _boneController.EvaluatePose();
            // Blends in the cached pose.
            _boneController.ApplyCachedPose();
            
            if (_cameraController != null)
            {
                _cameraController.UpdateCamera();
            }
            
            _boneController.PostEvaluatePose();
        }

        protected virtual void OnDestroy()
        {
            if (_boneController == null) return;
            _boneController.Dispose();
        }
        
        public void UnlinkAnimatorProfile()
        {
            if (_boneController == null) return;
            
            _boneController.UnlinkAnimatorProfile();
            HasLinkedProfile = false;
        }

        public void LinkAnimatorProfile(GameObject itemEntity)
        {
            if (_boneController == null) return;
            
            if (itemEntity.GetComponent<FPSAnimatorEntity>() is var entity && entity != null)
            {
                LinkAnimatorProfile(entity.animatorProfile);
                _boneController.UpdateEntity(entity);
            }
        }

        public void LinkAnimatorProfile(FPSAnimatorProfile newProfile)
        {
            if (_boneController == null) return;
            
            _boneController.LinkAnimatorProfile(newProfile);
            HasLinkedProfile = true;
        }
        
        // Will force to dynamically link the layer via OnSettingsUpdated callback.
        public void LinkAnimatorLayer(FPSAnimatorLayerSettings newSettings)
        {
            if (_boneController == null) return;
            
            _boneController.LinkAnimatorLayer(newSettings);
        }

#if UNITY_EDITOR
        private Transform FindTransform(Transform[] hierarchy, string[] query)
        {
            foreach (var element in hierarchy)
            {
                foreach (var entry in query)
                {
                    if (element.name.ToLower().Contains(entry.ToLower())) return element;
                }
            }

            return null;
        }
        
        private GameObject CreateElement(KRigComponent rigComponent, Transform parent, string elementName)
        {
            if (rigComponent.Contains(elementName))
            {
                return null;
            }
            
            var element = new GameObject(elementName);
            element.transform.parent = parent;
            element.transform.localRotation = Quaternion.identity;
            element.transform.localPosition = Vector3.zero;
            
            return element;
        }

        private void ProcessVirtualElement(KRigComponent rigComponent, GameObject component, string[] query)
        {
            if (component == null)
            {
                return;
            }

            Transform targetBone = FindTransform(rigComponent.GetHierarchy(), query);
            if (targetBone == null)
            {
                Debug.LogWarning($"Couldn't add a Virtual Element to {component.name}: no target bone found.");
                return;
            }

            var virtualElement = component.AddComponent<KVirtualElement>();
            virtualElement.targetBone = targetBone;
        }

        private void ProcessVirtualElement(GameObject component, Transform targetBone)
        {
            if (component == null)
            {
                return;
            }
            
            if (targetBone == null)
            {
                Debug.LogWarning($"Couldn't add a Virtual Element to {component.name}: no target bone found.");
                return;
            }

            var virtualElement = component.AddComponent<KVirtualElement>();
            virtualElement.targetBone = targetBone;
        }

        private void FindLegIkElements(KRigComponent rigComponent)
        {
            Transform skeletonRoot = rigComponent.transform;
            
            var rightFoot = CreateElement(rigComponent, skeletonRoot, FPSANames.IkRightFoot);
            var rightKnee = CreateElement(rigComponent, skeletonRoot, FPSANames.IkRightKnee);
            var leftFoot = CreateElement(rigComponent, skeletonRoot, FPSANames.IkLeftFoot);
            var leftKnee = CreateElement(rigComponent, skeletonRoot, FPSANames.IkLeftKnee);
            
            ProcessVirtualElement(rigComponent, rightFoot, new []
            {
                "right_foot", "foot_r", "rightfoot", "footright", "r_foot",
                "right_ankle", "ankle_r", "rightankle", "ankleright", "r_ankle"
            });
            
            ProcessVirtualElement(rigComponent, leftFoot, new []
            {
                "left_foot", "foot_l", "leftfoot", "footleft", "l_foot",
                "left_ankle", "ankle_l", "leftankle", "ankleleft", "l_ankle"
            });
            
            ProcessVirtualElement(rigComponent, rightKnee, new []
            {
                "lowerleg_r", "lowerleg_right", "r_lowerleg", "right_lowerleg",
                "shin_r", "shin_right", "r_shin", "right_shin",
                "leg_r", "leg_right", "r_leg", "right_leg", "rightleg"
            });
            
            ProcessVirtualElement(rigComponent, leftKnee, new []
            {
                "lowerleg_l", "lowerleg_left", "l_lowerleg", "left_lowerleg",
                "shin_l", "shin_left", "l_lshin", "left_shin",
                "leg_l", "leg_left", "l_leg", "left_leg", "leftleg"
            });
        }

        private void FindHandIkElements(KRigComponent rigComponent, Transform parent)
        {
            var rightHand = CreateElement(rigComponent, parent, FPSANames.IkRightHand);
            var rightElbow = CreateElement(rigComponent, parent, FPSANames.IkRightElbow);
            
            var leftHand = CreateElement(rigComponent, parent, FPSANames.IkLeftHand);
            var leftElbow = CreateElement(rigComponent, parent, FPSANames.IkLeftElbow);

            var hierarchy = rigComponent.GetHierarchy();
            Transform rightHandTransform = FindTransform(hierarchy, new[]
            {
                "hand_r", "hand_right", "right_hand", "r_hand", 
                "righthand", "handright", "r.hand", "hand.r"
            });
            
            Transform leftHandTransform = FindTransform(hierarchy, new[]
            {
                "hand_l", "hand_left", "left_hand", "l_hand", 
                "lefthand", "handleft", "l.hand", "hand.l"
            });
            
            ProcessVirtualElement(rightHand, rightHandTransform);
            ProcessVirtualElement(leftHand, leftHandTransform);

            CreateElement(rigComponent, rightHandTransform, FPSANames.IkWeaponBoneRight);
            CreateElement(rigComponent, leftHandTransform, FPSANames.IkWeaponBoneLeft);
            
            ProcessVirtualElement(rigComponent, rightElbow, new []
            {
                "lowerarm_r", "lowerarm_right", "r_lowerarm", "right_lowerarm",
                "arm_r", "arm_right", "r_arm", "right_arm", "rightarm"
            });
            
            ProcessVirtualElement(rigComponent, leftElbow, new []
            {
                "lowerarm_l", "lowerarm_left", "l_lowerarm", "left_lowerarm",
                "arm_l", "arm_left", "l_arm", "left_arm", "leftarm"
            });
        }
        
        public void CreateIkElements()
        {
            KRigComponent rigComponent = GetComponentInChildren<KRigComponent>();
            if (rigComponent == null)
            {
                Debug.LogError($"{gameObject.name} does not have a Rig Component!");
                return;
            }
            
            rigComponent.RefreshHierarchy();

            Transform[] hierarchy = rigComponent.GetHierarchy();
            Transform skeletonRoot = rigComponent.transform;

            CreateElement(rigComponent, skeletonRoot, FPSANames.WeaponBone);
            CreateElement(rigComponent, skeletonRoot, FPSANames.WeaponBoneAdditive);
            
            FindLegIkElements(rigComponent);

            Transform headBone = FindTransform(hierarchy, new[] { "head" });
            if (headBone == null)
            {
                Debug.LogError($"FPSAnimator: couldn't find the head bone.");
                rigComponent.RefreshHierarchy();
                return;
            }

            var ikWeaponBone = CreateElement(rigComponent, headBone, FPSANames.IkWeaponBone);

            var weaponBoneTransform = ikWeaponBone == null
                ? FindTransform(hierarchy, new[] {FPSANames.IkWeaponBone})
                : ikWeaponBone.transform;

            FindHandIkElements(rigComponent, weaponBoneTransform);
            
            rigComponent.RefreshHierarchy();
        }
#endif
    }
}