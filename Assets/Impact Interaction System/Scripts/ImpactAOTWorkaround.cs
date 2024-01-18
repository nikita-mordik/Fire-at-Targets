/*
 * This class is used to prevent exceptions when using IL2CPP.
 * This is a side-effect of Impact's use of generic virtual methods, which you can learn more about here: https://docs.unity3d.com/Manual/ScriptingRestrictions.html
 * This class has no impact on performance and is simply used to make things compile correctly.
 */

using Impact.Interactions;
using UnityEngine;
using UnityEngine.Scripting;

namespace Impact
{
    [Preserve]
    public class ImpactAOTWorkaround
    {
        public ImpactAOTWorkaround()
        {
            var instance = ScriptableObject.CreateInstance<ImpactInteractionBase>();
            instance.GetInteractionResult(new InteractionData());
            Object.Destroy(instance);
        }
    }
}
