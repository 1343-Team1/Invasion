using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Makes important data easily available and controls the flow of the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // Public variables.
        [Header("Actors")]
        public List<ActorController> activeActors;
    }
}
