using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Shooting
{
    public class ShootHitEffect : MonoBehaviour
    {
        [SerializeField] private GameObject hitPrefab;
        
        public async UniTaskVoid HitMilitaryTargetEffect(Vector3 point)
        {
            var hit = Instantiate(hitPrefab, point, Quaternion.identity);
            await UniTask.Delay(200);
            Destroy(hit);
        }

        public async UniTaskVoid HitDefaultEffect(Vector3 point)
        {
            var hit = Instantiate(hitPrefab, point, Quaternion.identity);
            await UniTask.Delay(200);
            Destroy(hit);
        }
    }
}