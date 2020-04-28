/// Author: Jeremy Anderson, April 12, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Bounce the game object up and down.
    /// </summary>
    public class NavPointFollower : MonoBehaviour
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== PRIVATE ==========
        Rigidbody2D rigidbody2d;
        NavPoint navPoint;                                      // the nav point currently being followed.
        [SerializeField] float navPointProximityLimit = 0.2f;
        List<ActorController> actorsOnPlatform = new List<ActorController>();

        // ========== PUBLIC ==========
        [Header("Settings")]
        public NavPoint startingNavPoint;                       // the first NavPoint.
        public float moveSpeed;                                 // the speed of the platform.


        /********************
         * =- Functions -=
         ********************/

        // Initialize.
        void Start()
        {
            rigidbody2d = GetComponent<Rigidbody2D>();
            navPoint = startingNavPoint;
        }

        // Move the platform and it's occupants.
        void FixedUpdate()
        {
            if (!navPoint || Vector2.Distance(transform.position, navPoint.transform.position) < navPointProximityLimit)
            {
                if (navPoint.nextNavPoint)
                    navPoint = navPoint.nextNavPoint;
            }

            if (navPoint)
                Move();
        }

        // Store actors that contact the platform.
        void OnCollisionEnter2D(Collision2D collision)
        {
            ActorController actorController = collision.gameObject.GetComponent<ActorController>();
            if (!actorController) // not an actor.
                return;

            actorsOnPlatform.Add(actorController);
        }

        // Remove actors that leave the platform.
        void OnCollisionExit2D(Collision2D collision)
        {
            ActorController actorController = collision.gameObject.GetComponent<ActorController>();
            if (!actorController) // not an actor.
                return;

            actorController.StopMovingRaw();
            actorsOnPlatform.Remove(actorController);
        }

        void Move()
        {
            Vector2 moveBy = (navPoint.transform.position - transform.position) * moveSpeed;

            if (moveBy.x != 0 || moveBy.y != 0)
            {
                float multiplier = (Mathf.Abs(moveBy.x) > Mathf.Abs(moveBy.y)) ?
                    ((moveBy.x < 0) ? -moveSpeed / moveBy.x : moveSpeed / moveBy.x) :
                    ((moveBy.y < 0) ? -moveSpeed / moveBy.y : moveSpeed / moveBy.y);

                moveBy = new Vector2(moveBy.x * multiplier, moveBy.y * multiplier);
            }

            rigidbody2d.velocity = new Vector2(moveBy.x, moveBy.y);

            // Move the actors that are in contact with the platform.
            if (actorsOnPlatform.Count > 0)
            {
                int numberOfActors = actorsOnPlatform.Count;
                for (int i = 0; i < numberOfActors; i++)
                {
                    if (!actorsOnPlatform[i].IsJumping())
                    {
                        actorsOnPlatform[i].MoveRaw(moveBy);
                        actorsOnPlatform[i].ResetJumps();
                    }
                }
            }

        }
    }
}