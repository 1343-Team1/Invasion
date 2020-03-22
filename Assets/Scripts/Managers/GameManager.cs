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

        // ========== STATIC ==========
        static GameManager gameManager;

        // ========== PUBLIC ==========
        // 0 to 1, controls the atmosphere of the game through screenshake, sound, and alien and debris spawn rate.        [Header("Atmosphere")]
        [Header("Atmosphere")]
        public float intensity;

        [Header("Actors")]
        public List<ActorController> activeActors;

        [Header("Nav Points")]
        public NavPoint[] navPoints;


        /********************
         * =- Functions -=
         ********************/

        // Returns a navPoint within sight, preferably one that leads to the player.
        public static NavPoint GetValidNavPoint(Vector2 inquirerPosition, float navPointProximityLimit)
        {
            // Catch a request that comes before the static gameManager is initialized.
            if (!gameManager)
                return null;

            NavPoint lastResortNavPoint = null;                   // this will store a visible navpoint that is not a good path.
            int count = gameManager.navPoints.Length;
            for (int i = 0; i < count; i++)
            {
                // This navpoint is below the swarmling.
                if (inquirerPosition.y >= gameManager.navPoints[i].transform.position.y)
                    continue;

                // This navpoint is too close.
                if (Vector2.Distance(inquirerPosition, gameManager.navPoints[i].transform.position) < navPointProximityLimit)
                    continue;

                // This navpoint is not visible.
                if (!ActorManager.IsTargetVisible(inquirerPosition, gameManager.navPoints[i].transform.position))
                    continue;

                lastResortNavPoint = gameManager.navPoints[i];    // store this as a last resort.

                // This navpoint is visible and a good path.
                if (gameManager.navPoints[i].IsGoodPath())
                    return gameManager.navPoints[i];
            }

            return lastResortNavPoint;
        }

        // Initialize and store all the NavPoints.
        void Start()
        {
            gameManager = this;
            navPoints = FindObjectsOfType<NavPoint>();
        }
    }
}
