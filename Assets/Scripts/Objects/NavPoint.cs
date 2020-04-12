/// Author: Jeremy Anderson, March 19, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        ActorManager actorManager;
        List<NavPoint> nextVisiblePoints;
        [SerializeField] GameObject player;

        // ========== PUBLIC ==========
        public NavPoint nextNavPoint;
        public bool isSwarmlingNavPoint;
        public bool isGoodPath;


        /********************
         * =- Functions -=
         ********************/

        // Initialize private variables.
        void Start()
        {
            // Get the GameManager and ActorManager.
            gameManager = FindObjectOfType<GameManager>();
            actorManager = FindObjectOfType<ActorManager>();

            // Early out if it's not a swarmling NavPoint.
            if (!isSwarmlingNavPoint)
                return;

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

        void Update()
        {
            if (!player)
                return;

            if (!isSwarmlingNavPoint)
                return;

            // Whether this NavPoint can see the player.
            if (ActorManager.IsTargetVisible(transform.position, player.transform.position))
            {
                isGoodPath = true;
                return;
            }
            else
                isGoodPath = false;

            // Whether it's parents can see the someone that sees the player.
            foreach (NavPoint navPoint in nextVisiblePoints)
            {
                if (navPoint.IsGoodPath())
                    isGoodPath = true;
            }

        }

        // Is this a valid path toward the player?
        public bool IsGoodPath()
        {
            return isGoodPath;
        }

        // Is this navpoint in the right position to spawn?
        public bool IsGoodToSpawn()
        {
            // Not a swarmling NavPoint, no spawning.
            if (!isSwarmlingNavPoint)
                return false;

            // Above the player.
            if (transform.position.y >= player.transform.position.y)
                return false;

            float distance = Vector2.Distance(transform.position, player.transform.position);

            // Too close or too far away.
            if (distance < actorManager.minDistanceToSpawn || distance > actorManager.maxDistanceToSpawn)
                return false;

            // In the goldilocks zone.
            return true;
        }

        // Draw a yellow circle to show nav points in the editor.
        void OnDrawGizmos()
        {
            if (!isSwarmlingNavPoint)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, 0.25f);

                // There's no next NavPoint set.
                if (player && !(Physics2D.Linecast(transform.position, player.GetComponent<ActorController>().targetingPoint.transform.position, ~(1 << 15))))
                    Gizmos.DrawLine(transform.position, player.GetComponent<ActorController>().targetingPoint.transform.position);

                // There's no next NavPoint set.
                if (!nextNavPoint)
                    return;

                // Nothing is obscuring the next NavPoint.
                if (!(Physics2D.Linecast(transform.position, nextNavPoint.transform.position, ~(1 << 15))))
                    Gizmos.DrawLine(transform.position, nextNavPoint.transform.position);

                return;
            }



            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.25f);

            if (player)
            {
                // Nothing is obscuring the player.
                if (!(Physics2D.Linecast(transform.position, player.transform.position, ~(1 << 15))))
                {
                    ActorController actorController = player.GetComponent<ActorController>();
                    Vector3 playerPosition = (actorController) ? actorController.targetingPoint.transform.position :
                        player.transform.position;

                    Gizmos.DrawLine(transform.position, playerPosition);
                }
            }


            // Draw a line to each visible NavPoint
            List<GameObject> sceneObjects = new List<GameObject>();
            SceneManager.GetActiveScene().GetRootGameObjects(sceneObjects);
            
            // Find The Swarm root gameObject.
            foreach( GameObject sceneObject in sceneObjects)
            {
                // Not the Swarm root gameObject.
                if (sceneObject.name != "The Swarm")
                    continue;

                // Get the transform of the second child (the NavPoints) and the number of children it has.
                Transform editorNavPoints = sceneObject.transform.GetChild(1);
                int count = editorNavPoints.childCount;

                // Iterate through the children.
                for (int i = 0; i < count; i++)
                {
                    // Not a NavPoint (just in case).
                    if (!editorNavPoints.GetChild(i).GetComponent<NavPoint>())
                        continue;

                    // Too low.
                    if (editorNavPoints.GetChild(i).transform.position.y <= transform.position.y)
                        continue;

                    // Something is in the way.
                    if (Physics2D.Linecast(transform.position, editorNavPoints.GetChild(i).transform.position, ~(1 << 15)))
                        continue;

                    Gizmos.DrawLine(transform.position, editorNavPoints.GetChild(i).transform.position);
                }
            }
        }
    }
}