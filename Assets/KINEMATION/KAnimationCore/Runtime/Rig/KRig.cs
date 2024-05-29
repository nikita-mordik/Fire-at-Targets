// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Input;

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.KAnimationCore.Runtime.Rig
{
    // Character skeleton asset.
    [CreateAssetMenu(fileName = "NewRig", menuName = "KINEMATION/Rig")]
    public class KRig : ScriptableObject
    {
        public RuntimeAnimatorController targetAnimator;
        public UserInputConfig inputConfig;
        public List<KRigElement> rigHierarchy = new List<KRigElement>();
        public List<KRigElementChain> rigElementChains = new List<KRigElementChain>();
        public List<string> rigCurves = new List<string>();

        public KTransformChain GetPopulatedChain(string chainName, KRigComponent rigComponent)
        {
            KTransformChain result = new KTransformChain();
            var targetChain = rigElementChains.Find(item => item.chainName.Equals(chainName));

            foreach (var element in targetChain.elementChain)
            {
                result.transformChain.Add(rigComponent.GetRigTransform(element));
            }
            
            return result;
        }
        
#if UNITY_EDITOR
        public List<int> rigDepths = new List<int>();
        private List<Object> _rigObservers = new List<Object>();
        
        public KRigElement GetElementByName(string targetName)
        {
            return rigHierarchy.Find(item => item.name.Equals(targetName));
        }

        public void RegisterRigObserver(Object newRigObserver)
        {
            // Only register Rig Observers.
            IRigObserver observer = (IRigObserver) newRigObserver;
            if (observer == null) return;
            
            if (_rigObservers.Contains(newRigObserver))
            {
                return;
            }
            
            _rigObservers.Add(newRigObserver);
            EditorUtility.SetDirty(this);
        }

        public void UnRegisterObserver(Object rigObserver)
        {
            _rigObservers.Remove(rigObserver);
            EditorUtility.SetDirty(this);
        }

        public void NotifyObservers()
        {
            foreach (var observer in _rigObservers) if (observer is IRigObserver obj) obj.OnRigUpdated();
        }
#endif
    }
}