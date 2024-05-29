// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.FPSAnimationFramework.Runtime.Layers.WeaponLayer;
using KINEMATION.FPSAnimationFramework.Runtime.Recoil;
using KINEMATION.KAnimationCore.Runtime.Core;

using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.AdditiveLayer
{
    // Applies curve animations and recoil.
    public class AdditiveLayerState : WeaponLayerState
    {
        private AdditiveLayerSettings _settings;
        private RecoilAnimation _recoilAnimation;

        private Transform _additiveBone;
        private KTransform _ikMotion = KTransform.Identity;

        private int _aimingWeightPropertyIndex;
        
        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            base.InitializeState(newSettings);
            
            _settings = (AdditiveLayerSettings) newSettings;
            _recoilAnimation = _owner.GetComponent<RecoilAnimation>();
            _additiveBone = _rigComponent.GetRigTransform(_settings.additiveBone);

            _aimingWeightPropertyIndex = _inputController.GetPropertyIndex(_settings.aimingInputProperty);
        }

        protected override void OnEvaluateWeaponPose()
        {
            float weight = Weight;
            float aimingWeight = _inputController.GetValue<float>(_aimingWeightPropertyIndex);

            Vector3 pivotOffset = Vector3.zero;
            Quaternion recoilR = Quaternion.identity;
            Vector3 recoilT = Vector3.zero;

            if (_recoilAnimation != null && _recoilAnimation.RecoilData != null)
            {
                var data = _recoilAnimation.RecoilData;
                pivotOffset = Vector3.Lerp(data.hipPivotOffset, data.aimPivotOffset, aimingWeight);
                
                recoilR = Quaternion.Euler(_recoilAnimation.OutRot);
                recoilT = _recoilAnimation.OutLoc;
            }

            recoilT += recoilR * pivotOffset - pivotOffset;

            KAnimationMath.MoveInSpace(_weaponIkBone, _weaponIkBone, recoilT, weight);
            KAnimationMath.RotateInSpace(_weaponIkBone, _weaponIkBone, recoilR, weight);
            
            float t = KMath.ExpDecayAlpha(_settings.interpSpeed, Time.deltaTime);
            if (Mathf.Approximately(_settings.interpSpeed, 0f))
            {
                t = 1f;
            }
            
            weight *= Mathf.Lerp(1f, _settings.adsScalar, aimingWeight);
            _ikMotion = KTransform.Lerp(_ikMotion, new KTransform(_additiveBone.transform, false), t);

            KAnimationMath.MoveInSpace(_owner.transform, _weaponIkBone, _ikMotion.position, weight);
            KAnimationMath.RotateInSpace(_owner.transform, _weaponIkBone, _ikMotion.rotation, weight);
        }
    }
}