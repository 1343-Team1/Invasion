/// Author: Jeremy Anderson, March 19, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Keep track of all actors and provide valid targets on request.
    /// </summary>
    public class ActorManager : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE / PROTECTED ==========
        [SerializeField] [DisplayWithoutEdit()] List<ActorController> actors;

        // ========== PUBLIC ==========



        /********************
         * =- Functions -=
         ********************/

        // See if these two Actors can see eachother.
        public static bool IsTargetVisible(Vector2 attackerPosition, Vector2 targetPosition, float sightRange = 0)
        {
            float distance = Vector2.Distance(attackerPosition, targetPosition);

            if (sightRange > 0 && distance > sightRange)    // was a sight range specified and the distance is too great?
                return false;

            // Are any platforms obscuring the target? (the three level collision layers need to be considered).
            if (Physics2D.Linecast(attackerPosition, targetPosition, ~(1 << 15)))
            {
                Debug.DrawLine(attackerPosition, targetPosition, Color.red);
                return false;
            }
            else
            {
                Debug.DrawLine(attackerPosition, targetPosition, Color.green);
                return true;
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            // Initialize actors and add all actors in the scene to it.
            actors = new List<ActorController>();
            ActorController[] actorsInScene = FindObjectsOfType<ActorController>();
            int count = actorsInScene.Length;
            for (int i = 0; i < count; i++)
                actors.Add(actorsInScene[i]);
        }

        // Return a valid target or null if none is available.
        public ActorController GetValidTarget(Stats attackerStats)
        {
            Vector2 attackerPosition = attackerStats.transform.position;

            int count = actors.Count;
            for (int i = 0; i < count; i++)
            {
                // This potential target is in the same faction.
                if (actors[i].GetComponent<Stats>().faction == attackerStats.faction)
                    continue;                               // go to the next iteration.

                float distance = Vector2.Distance(actors[i].transform.position, attackerPosition);
                if (distance > attackerStats.sightRange)    // too far away.
                    continue;                               // go to the next iteration.

                // The target is obstructed by platforms.
                if (!IsTargetVisible(attackerStats.transform.position, actors[i].transform.position, attackerStats.sightRange))
                    continue;                               // go to the next iteration.

                // The target is in range and visible!
                return actors[i].GetComponent<ActorController>();
            }

            return null;                                    // no valid actors found.
        }
    }

    /// <summary>
    /// Every actor belongs to a faction. The three factions are all enemies.
    /// </summary>
    public enum Faction
    {
        None,
        Player,
        Alien,
        Security
    }
}
