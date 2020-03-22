/// Author: Jeremy Anderson, March 10, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        ActorManager actorManager;
        Stats stats;
        NavPoint navPoint;
        [SerializeField] float navPointProximityLimit = 0.2f;

        // ========== PUBLIC ==========
        [Header("Debug Data")]
        [SerializeField] GameObject currentTargetPoint;

        [Header("AI Brain Settings")]
        public bool followsNavPoints = false;

        /********************
         * =- Functions -=
         ********************/

        // Get the ActorManager and Stats.
        void Start()
        {
            actorManager = FindObjectOfType<ActorManager>();
            stats = GetComponent<Stats>();

            // Get a navpoint if one is visible.
            navPoint = GameManager.GetValidNavPoint(transform.position, navPointProximityLimit);
        }

        // See if this AIBrain already has a target.
        bool HasTarget()
        {
            return (!currentTargetPoint) ? false : true;
        }

        // If the target should be fired at and this AIBrain can fire, then return true.
        public bool IsFiring()
        {
            return false;
        }

        // If this AIBrain can run, and is running, then return true.
        public bool IsRunning()
        {
            return false;
        }

        // If this AIBrain can jump, and is jumping, then return true.
        public bool IsJumping()
        {
            return false;
        }

        // Determines how this AIBrain should move.
        public Vector2 GetMovement()
        {
            // There is a valid target to pursue.
            if (HasTarget() && ActorManager.IsTargetVisible(transform.position, currentTargetPoint.transform.position, stats.sightRange))
                return currentTargetPoint.transform.position - transform.position;

            // Get a valid target to pursue from the ActorManager.
            ActorController validTarget = actorManager.GetValidTarget(stats);
            currentTargetPoint = ((validTarget) ? validTarget.targetingPoint : currentTargetPoint);

            // There is no valid target to pursue. If this doesn't follow NavPoints, it doesn't move?
            if (!followsNavPoints)
                return Vector2.zero;

            // If there is no navPoint target already, or it is too close, request a new one.
            if (!navPoint || Vector2.Distance(transform.position, navPoint.transform.position) < navPointProximityLimit)
                navPoint = GameManager.GetValidNavPoint(transform.position, navPointProximityLimit);

            if (navPoint)
                return navPoint.transform.position - transform.position;

            return Vector2.zero;
        }
    }
}
