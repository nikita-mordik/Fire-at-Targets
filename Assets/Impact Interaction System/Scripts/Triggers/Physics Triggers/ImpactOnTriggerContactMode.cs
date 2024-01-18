using UnityEngine;

namespace Impact.Triggers
{
    public enum ImpactOnTriggerContactMode
    {
        [Tooltip("Use this object's position as the contact point.")]
        ThisObject = 0,
        [Tooltip("Use the other object's position as the contact point.")]
        OtherObject = 1
    }

}
