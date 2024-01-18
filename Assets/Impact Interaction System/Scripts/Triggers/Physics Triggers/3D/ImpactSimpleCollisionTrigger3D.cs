using Impact.Objects;
using UnityEngine;

namespace Impact.Triggers
{
    [AddComponentMenu("Impact/3D Collision Triggers/Impact Simple Collision Trigger 3D", 0)]
    public class ImpactSimpleCollisionTrigger3D : ImpactTriggerBase<ImpactCollisionWrapper, ImpactContactPoint>
    {
        private void OnCollisionEnter()
        {
            if (!Enabled || (!HighPriority && ImpactManagerInstance.HasReachedPhysicsInteractionsLimit()))
                return;

            ImpactManagerInstance.IncrementPhysicsInteractionsLimit();

            VelocityData myVelocityData = MainTarget.GetVelocityDataAtPoint(transform.position);

            InteractionData c = new InteractionData()
            {
                InteractionType = InteractionData.InteractionTypeCollision,
                Point = transform.position,
                Velocity = myVelocityData.TotalPointVelocity,
                CompositionValue = 1
            };

            ImpactManagerInstance.ProcessInteraction(c, MainTarget);
        }
    }
}