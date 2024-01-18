namespace Impact
{
    /// <summary>
    /// Implementation of the IImpactCollisionWrapper interface that only has a single contact point. Uses the ImpactContactPoint struct.
    /// </summary>
    public struct ImpactCollisionSingleContactWrapper : IImpactCollisionWrapper<ImpactContactPoint>
    {
        /// <summary>
        /// The number of contacts in the collision.
        /// </summary>
        public int ContactCount { get { return 1; } }

        /// <summary>
        /// Whether the source Collision data was 3D or 2D.
        /// </summary>
        public PhysicsType PhysicsType { get; private set; }

        private ImpactContactPoint contactPoint;

        /// <summary>
        /// Initializes the wrapper for 3D or 2D OnTrigger calls.
        /// </summary>
        /// <param name="contactPoint">The source Collision object.</param>
        public ImpactCollisionSingleContactWrapper(ImpactContactPoint contactPoint, PhysicsType physicsType)
        {
            this.contactPoint = contactPoint;
            PhysicsType = physicsType;
        }

        /// <summary>
        /// Returns the contact point.
        /// </summary>
        /// <param name="index">Does nothing for this struct since there is only a single contact point.</param>
        /// <returns>The contact point.</returns>
        public ImpactContactPoint GetContact(int index)
        {
            return contactPoint;
        }
    }
}
