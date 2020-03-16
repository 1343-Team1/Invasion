/// Author: Jeremy Anderson, March 10, 2020.

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
        [Header("Atmosphere")]
        // 0 to 1, controls the atmosphere of the game through screenshake, sound, and alien and debris spawn rate.
        public float intensity;

        [Header("Actors")]
        public List<ActorController> activeActors;
    }
}
