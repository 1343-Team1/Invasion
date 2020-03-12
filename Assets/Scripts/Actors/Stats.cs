using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// The stats for an actor.
    /// </summary>
    public class Stats : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // Private/protected variables.

        // Public variables.
        [Header("Movement")]
        public float runMultiplier;                             // the speed this Actor will run.
        public float walkSpeed;                                 // the speed this Actor will walk.

        [Header("Jumping")]
        public float jumpStrength;                              // the strength of this Actor's jump.
        public int extraJumps = 0;                              // the number of jumps this actor can make before falling.

        // Exposed private/protected variables.
        //[Header("Debug Data")]

        /********************
         * =- Functions -=
         ********************/
    }
}
