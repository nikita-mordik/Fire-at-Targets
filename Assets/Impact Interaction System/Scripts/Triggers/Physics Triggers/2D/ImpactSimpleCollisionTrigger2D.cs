using Impact.Objects;
using UnityEngine;

namespace Impact.Triggers
{
    [AddComponentMenu("Impact/2D Collision Triggers/Impact Simple Collision Trigger 2D", 0)]
    public class ImpactSimpleCollisionTrigger2D : ImpactTriggerBase<ImpactCollisionWrapper, ImpactContactPoint>
    {
        private void OnCollisionEnter2D()
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