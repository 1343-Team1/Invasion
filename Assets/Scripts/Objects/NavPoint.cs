/// Author: Jeremy Anderson, March 19, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Points that direct the Swarmlings if they have no target.
    /// </summary>
    public class NavPoint : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE / PROTECTED ==========
        GameManager gameManager;
        List<NavPoint> nextVisiblePoints;
        [SerializeField] GameObject player;

        // ========== PUBLIC ==========



        /********************
         * =- Functions -=
         ********************/

        // Initialize private variables.
        void Start()
        {
            // Get the GameManager.
            gameManager = FindObjectOfType<GameManager>();

            // Prepare for the loop.
            nextVisiblePoints = new List<NavPoint>();
            int count = gameManager.navPoints.Length;

            // Check all the NavPoints to see which ones should be next in line, and store references to them.
            for (int i = 0; i < count; i++)
            {
                if (!ActorManager.IsTargetVisible(transform.position, gameManager.navPoints[i].transform.position))
                    continue;

                if (gameManager.navPoints[i].transform.position.y > transform.position.y)
                    nextVisiblePoints.Add(gameManager.navPoints[i]);
            }

            // Get the player.
            player = FindObjectOfType<PlayerInput>().GetComponent<ActorController>().targetingPoint;
        }

        // Is this a valid path toward the player?
        public bool IsGoodPath()
        {
            if (!player)                            // early out if the player variable hasn't been set yet.
                return false;

            // This NavPoint can see the player.
            if (ActorManager.IsTargetVisible(transform.position, player.transform.position))
                return true;

            if (nextVisiblePoints.Count <= 0)       // there are no higher visible NavPoints.
                return false;

            // Follow the NavPoints up and see if one leads to the player.
            foreach (NavPoint navPoint in nextVisiblePoints)
            {
                if (IsGoodPath())
                    return true;
            }

            return false;
        }
    }
}