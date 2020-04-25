/// Author: Jeremy Anderson, March 19, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Invasion
{
    /// <summary>
    /// Evaluates objectives and determines what to do, providing input to the AIInput on demand.
    /// Acts as a player with a controller, but only thinks when asked.
    /// </summary>
    public class AIBrain : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE / PROTECTED ==========
        public ActorManager actorManager;
        ActorController actorController;
        Stats stats;
        NavPoint navPoint;
        [SerializeField] float navPointProximityLimit = 0.2f;

        // ========== PUBLIC ==========
        [Header("Debug Data")]
        [SerializeField] GameObject currentTargetPoint;

        [Header("AI Brain Settings")]
        [SerializeField] bool isSwarmlingAlive;
        public bool followsNavPoints = false;
        public bool isSwarmling = false;

        [Header("Nav Point Settings")]
        public NavPoint startingNavPoint;                       // the first NavPoint.


        /********************
         * =- Functions -=
         ********************/

        // Get the ActorManager and Stats.
        void Start()
        {
            actorManager = FindObjectOfType<ActorManager>();
            actorController = GetComponent<ActorController>();
            stats = GetComponent<Stats>();
            Initialize();
        }

        public void Initialize()
        {
            if (isSwarmling)
            {
                IsAlive = true;
                isSwarmlingAlive = true;
                actorController.Resurrect();
                actorManager.actualSwarmlingCount++;
                stats.FillHealth();

                // Get a navpoint if one is visible.
                navPoint = GameManager.GetValidNavPoint(transform.position, navPointProximityLimit, isSwarmling);
            }
            else if (startingNavPoint)
                navPoint = startingNavPoint;
        }

        // See if this AIBrain already has a target.
        bool HasTarget(bool attackerFacingRight = true)
        {
            // No target.
            if (!currentTargetPoint)
                return false;

            // Current target is dead.
            if (currentTargetPoint.transform.parent.GetComponent<ActorController>().IsDead ||
                !currentTargetPoint.transform.parent.gameObject.activeInHierarchy)
            {
                currentTargetPoint = null;
                return false;
            }

            // Current target is not visible.
            if (!ActorManager.IsTargetVisible(stats.Position, currentTargetPoint.transform.position, attackerFacingRight, stats.sightDegrees, stats.sightRange))
            {
                currentTargetPoint = null;
                return false;
            }

            // Valid target already.
            return true;
        }

        // If the target should be fired at and this AIBrain can fire, then return true.
        public bool IsFiring(bool attackerFacingRight = false)
        {
            if (HasTarget(attackerFacingRight) && ActorManager.IsTargetVisible(stats.Position, currentTargetPoint.transform.position, attackerFacingRight, stats.sightDegrees, stats.sightRange))
                return true;

            return false;
        }

        // If this AIBrain can run, and is running, then return true.
        public bool IsRunning() { return false; }

        // If this AIBrain can jump, and is jumping, then return true.
        public bool IsJumping() { return false; }

        // Field for the swarmling pool.
        public bool IsAlive { get { return isSwarmlingAlive; } set { isSwarmlingAlive = value; } }

        // This signal is sent by the ActorController after it animates the death or by the ActorManager while off-screen.
        public void Die()
        {
            // Only swarmlings go into a pool.
            if (!isSwarmling)
            {
                actorManager.RegisterDeath(actorController);

                // Don't destroy turrets.
                if (actorController.walkSpeed > 0)
                    Destroy(gameObject);

                return; // early out.
            }

            isSwarmlingAlive = false;                            // Inform the pool.
            actorManager.actualSwarmlingCount--;
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
            gameObject.SetActive(false);                // Turn it off.
        }

        // Determines how this AIBrain should move.
        public Vector2 GetMovement(bool attackerFacingRight = true)
        {
            // There is a valid target to pursue.
            if (HasTarget(attackerFacingRight) && ActorManager.IsTargetVisible(stats.Position, currentTargetPoint.transform.position, attackerFacingRight, stats.sightDegrees, stats.sightRange))
                return currentTargetPoint.transform.position - (Vector3)(stats.Position);

            // Get a valid target to pursue from the ActorManager.
            ActorController validTarget = actorManager.GetValidTarget(stats, attackerFacingRight);
            currentTargetPoint = ((validTarget) ? validTarget.targetingPoint : currentTargetPoint);

            // There is no valid target to pursue. If this doesn't follow NavPoints, it doesn't move?
            if (!followsNavPoints)
                return Vector2.zero;

            // If there is no navPoint target already, or it is too close, request a new one.
            if (!navPoint || Vector2.Distance(stats.Position, navPoint.transform.position) < navPointProximityLimit)
            {
                if (isSwarmling)
                    navPoint = GameManager.GetValidNavPoint(transform.position, navPointProximityLimit, isSwarmling);
                else if (navPoint.nextNavPoint)
                    navPoint = navPoint.nextNavPoint;
            }

            if (navPoint)
                return navPoint.transform.position - (Vector3)stats.Position;

            return Vector2.zero;
        }

        // Draw a yellow circle to show nav points in the editor.
        void OnDrawGizmos()
        {
            if (!isSwarmling)                           // Draw NavPoint routes.
            {
                Gizmos.color = Color.cyan;

                if (startingNavPoint)
                {
                    ActorController actorController = GetComponent<ActorController>();
                    Vector3 Position = (actorController) ? actorController.targetingPoint.transform.position : transform.position;
                    if (!(Physics2D.Linecast(Position, startingNavPoint.transform.position, ~(1 << 15))))
                        Gizmos.DrawLine(Position, startingNavPoint.transform.position);
                }

                return;
            }

            Gizmos.color = Color.yellow;

            if (currentTargetPoint)
            {
                // Nothing is obscuring the player.
                if (!(Physics2D.Linecast(transform.position, currentTargetPoint.transform.position, ~(1 << 15))))
                    Gizmos.DrawLine(transform.position, currentTargetPoint.transform.position);
            }

            // Draw a line to each visible NavPoint
            List<GameObject> sceneObjects = new List<GameObject>();
            SceneManager.GetActiveScene().GetRootGameObjects(sceneObjects);

            // Find The Swarm root gameObject.
            foreach (GameObject sceneObject in sceneObjects)
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
