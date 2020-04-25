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
        GameManager gameManager;
        [SerializeField] [DisplayWithoutEdit()] List<ActorController> actors;
        List<AIBrain> swarmlingPool;
        List<NavPoint> swarmlingNavPoints;
        PlayerInput player;

        // ========== PUBLIC ==========
        [Header("Swarmling Settings")]
        public GameObject swarmlingParentObject;
        public GameObject swarmlingPrefab;
        public int desiredSwarmlingCount;
        public int actualSwarmlingCount;
        public float minimumSwarmlings;
        public float swarmlingCountMultiplier;
        public float minDistanceToSpawn;
        public float maxDistanceToSpawn;

        /********************
         * =- Functions -=
         ********************/

        // See if these two Actors can see eachother.
        public static bool IsTargetVisible(Vector2 attackerPosition, Vector2 targetPosition, bool attackerFacingRight = true, float sightDegrees = 360, float sightRange = 0)
        {
            float distance = Vector2.Distance(attackerPosition, targetPosition);

            if (sightRange > 0 && distance > sightRange)    // was a sight range specified and the distance is too great?
                return false;

            // Not in the proper direction to be seen.
            Vector2 direction = targetPosition - attackerPosition;
            Vector2 originDirection = (attackerFacingRight) ? Vector2.right : Vector2.left;
            if (Vector2.Angle(direction, originDirection) > sightDegrees / 2)
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
            gameManager = FindObjectOfType<GameManager>();
            actors = new List<ActorController>();
            ActorController[] actorsInScene = FindObjectsOfType<ActorController>();
            int count = actorsInScene.Length;
            for (int i = 0; i < count; i++)
                actors.Add(actorsInScene[i]);

            swarmlingPool = new List<AIBrain>();
            AIBrain[] swarmlings = FindObjectsOfType<AIBrain>();
            count = swarmlings.Length;
            for (int i = 0; i < count; i++)
                if (swarmlings[i].isSwarmling)
                    swarmlingPool.Add(swarmlings[i]);

            swarmlingNavPoints = new List<NavPoint>();
            NavPoint[] navPointsInScene = FindObjectsOfType<NavPoint>();
            count = navPointsInScene.Length;
            for (int i = 0; i < count; i++)
                if (navPointsInScene[i].isSwarmlingNavPoint)
                    swarmlingNavPoints.Add(navPointsInScene[i]);

            player = FindObjectOfType<PlayerInput>();
        }

        // Manage the desiredSwarmlingCount based on GameManager intensity.
        void Update()
        {
            desiredSwarmlingCount = (int)(minimumSwarmlings + (swarmlingCountMultiplier * gameManager.intensity));

            // Not enough swarmlings.
            if (actualSwarmlingCount < desiredSwarmlingCount)
            {
                NavPoint navPoint = FindSpawnPoint();
                if (!navPoint) // no spawn point found.
                    return;

                // A good spawn point was found.
                SpawnSwarmling(navPoint.transform.position);
            }

            // Too many, kill off an off-screen ones.
            else if (actualSwarmlingCount > desiredSwarmlingCount)
            {
                int count = swarmlingPool.Count;
                for (int i = 0; i < count; i++)
                {
                    // Not alive.
                    if (!swarmlingPool[i].IsAlive)
                        continue;

                    // Not far enough away.
                    if (swarmlingPool[i].transform.position.y < player.transform.position.y + minDistanceToSpawn &&
                        swarmlingPool[i].transform.position.y > player.transform.position.y - minDistanceToSpawn)
                        continue;

                    // Found one to kill!
                    swarmlingPool[i].Die();
                    return;
                }
            }

            // Cull any that get too far ahead of the player.
            for (int i = 0; i < actualSwarmlingCount; i++)
            {
                if (!swarmlingPool[i].IsAlive)
                    continue;

                if (swarmlingPool[i].transform.position.y > player.transform.position.y + minDistanceToSpawn)
                    swarmlingPool[i].Die();
            }
        }

        // Return a valid target or null if none is available.
        public ActorController GetValidTarget(Stats attackerStats, bool attackerFacingRight = true)
        {
            Vector2 attackerPosition = attackerStats.transform.position;

            int count = actors.Count;
            for (int i = 0; i < count; i++)
            {
                // This actor is dead.
                if (actors[i].IsDead || !actors[i].gameObject.activeInHierarchy)
                    continue;

                // This potential target is in the same faction.
                if (actors[i].GetComponent<Stats>().faction == attackerStats.faction)
                    continue;                               // go to the next iteration.

                float distance = Vector2.Distance(actors[i].transform.position, attackerPosition);
                if (distance > attackerStats.sightRange)    // too far away.
                    continue;                               // go to the next iteration.

                // The target is obstructed by platforms.
                if (!IsTargetVisible(attackerStats.Position, actors[i].targetingPoint.transform.position, attackerFacingRight, attackerStats.sightDegrees, attackerStats.sightRange))
                    continue;                               // go to the next iteration.

                // The target is in range and visible!
                return actors[i].GetComponent<ActorController>();
            }

            return null;                                    // no valid actors found.
        }

        // Get a viable nav point to spawn at.
        NavPoint FindSpawnPoint()
        {
            int count = swarmlingNavPoints.Count;
            for (int i = 0; i < count; i++)
                if (swarmlingNavPoints[i].IsGoodToSpawn())
                    return swarmlingNavPoints[i];

            return null;
        }

        // Manage swarmling spawning.
        void SpawnSwarmling(Vector2 position)
        {
            int count = swarmlingPool.Count;
            for (int i = 0; i < count; i++)
            {
                // This one is alive already.
                if (swarmlingPool[i].IsAlive)
                    continue;

                // Found a dead one! Re-initialize it.
                swarmlingPool[i].transform.position = position;
                swarmlingPool[i].Initialize();
                swarmlingPool[i].gameObject.SetActive(true);
                swarmlingPool[i].transform.GetChild(0).gameObject.SetActive(true);

                // Success, exit this method.
                return;
            }

            // No swarmling exists. Make one and add it to the pool.
            GameObject gameObject = Instantiate(swarmlingPrefab, position, Quaternion.identity);
            gameObject.transform.SetParent(swarmlingParentObject.transform);
            swarmlingPool.Add(gameObject.GetComponent<AIBrain>());
        }

        // Remove a dead actor from the list of actors.
        public void RegisterDeath(ActorController actorController)
        {
             actors.Remove(actorController);
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
