using UnityEngine;

namespace Impact.Utility
{
    public static class PhysicsUtilities
    {
        /// <summary>
        /// Calculates the tangential velocity of a world space point.
        /// </summary>
        /// <param name="point">The point to get the velocity of.</param>
        /// <param name="angularVelocity">The angular velocity of the object the point belongs to.</param>
        /// <param name="centerOfRotation">The center of rotation of the object.</param>
        /// <returns>The tangential velocity of the point.</returns>
        public static Vector3 CalculateTangentialVelocity(Vector3 point, Vector3 angularVelocity, Vector3 centerOfRotation)
        {
            var p = point - centerOfRotation;
            var v = Vector3.Cross(angularVelocity, p);
            return v;
        }
    }
}