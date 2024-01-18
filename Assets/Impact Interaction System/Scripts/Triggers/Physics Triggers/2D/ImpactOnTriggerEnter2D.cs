using UnityEngine;

namespace Impact.Triggers
{
    [AddComponentMenu("Impact/2D Collision Triggers/Impact On Trigger Enter 2D", 0)]
    public class ImpactOnTriggerEnter2D : ImpactCollisionTriggerBase<ImpactCollisionSingleContactWrapper, ImpactContactPoint>
    {
        [SerializeField]
        private ImpactOnTriggerContactMode contactPointMode;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!Enabled || (!HighPriority && ImpactManagerInstance.HasReachedPhysicsInteractionsLimit()))
                return;

            ImpactManagerInstance.IncrementPhysicsInteractionsLimit();

            int otherPhysicsMaterialID = 0;
            if (collider.sharedMaterial != null)
                otherPhysicsMaterialID = collider.sharedMaterial.GetInstanceID();

            Vector3 contactPoint = contactPointMode == ImpactOnTriggerContactMode.ThisObject ? transform.position : collider.transform.position;

            ImpactCollisionSingleContactWrapper c = new ImpactCollisionSingleContactWrapper(new ImpactContactPoint()
            {
                Point = contactPoint,
                Normal = Vector3.zero,
                ThisObject = this.gameObject,
                OtherObject = collider.gameObject,
                PhysicsType = PhysicsType.Physics3D,
                ThisPhysicsMaterialID = 0,
                OtherPhysicsMaterialID = otherPhysicsMaterialID
            }, PhysicsType.Physics2D);

            processCollision(c);
        }
    }
}