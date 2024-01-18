using Impact.Objects;

namespace Impact.Triggers
{
    /// <summary>
    /// Interface for built-in Impact Trigger components.
    /// You can use this interface if you want to get all Impact Trigger components attached to an object.
    /// </summary>
    public interface IImpactTrigger
    {
        /// <summary>
        /// Should this trigger process any collisions? 
        /// You should use this instead of the normal enabled property because collision messages are still sent to disabled components.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// The ImpactObjectBase this trigger will use for interaction calculations.
        /// </summary>
        ImpactObjectBase MainTarget { get; set; }

        /// <summary>
        /// Should this trigger use the material composition of the objects it hits?
        /// If true, interaction data will be sent for each material at the interaction point.
        /// If false, interaction data will only be sent for the primary material at the interaction point. 
        /// </summary>
        bool UseMaterialComposition { get; set; }

        /// <summary>
        /// How collision contacts should be handled.
        /// </summary>
        ImpactTriggerContactMode ContactMode { get; set; }

        /// <summary>
        /// Should this trigger ignore the Physics Interactions Limit set in the Impact Manager?
        /// </summary>
        bool HighPriority { get; set; }
    }
}
