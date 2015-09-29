using System;

namespace com.ootii.AI.Controllers
{
    /// <summary>
    /// Defines the different modes of the controller
    /// </summary>
    public class EnumControllerStance
    {
        /// <summary>
        /// Standard stance that supports jumping, climbing, etc.
        /// </summary>
        public const int TRAVERSAL = 0;

        /// <summary>
        /// Special stance for close quarter combat
        /// </summary>
        public const int COMBAT_MELEE = 1;

        /// <summary>
        /// Special stance for ranged combat
        /// </summary>
        public const int COMBAT_RANGED = 2;

        /// <summary>
        /// Special stance for stealth
        /// </summary>
        public const int STEALTH = 4;
        public const int SNEAK = 4;     // Depricated

        /// <summary>
        /// Friendly name of the type
        /// </summary>
        public static string[] Names = new string[] { 
            "Traversal", 
            "Combat-Melee", 
            "Combat-Ranged", 
            "unknown",
            "Stealth"
        };
    }
}

