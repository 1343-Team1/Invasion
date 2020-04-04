/// Author: Jeremy Anderson, March 12, 2020.

using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Takes input from a keyboard and mouse or gamepad, allows configuration,
    /// and sends that data to the ActorController.
    /// </summary>
    [RequireComponent(typeof(Collider2D))][RequireComponent(typeof(Animator))]
    public class Projectile : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // Private/protected variables.
        Animator animator;                                  // the animator.
        Collider2D collider2d;                              // the collider.
        float timeToDestroy;                                // the timer, predictive to avoid calculating every frame.
        bool hitAnimationTriggered = false;                 // used to determine whether to play the hit animation or destroy the gameObject.
        Faction faction;                                    // the faction that fired this projectile.

        // Public variables.
        [Header("Movement Settings")]
        public float speed;                                 // units per second the projectile moves in the direciton it is facing.
        public float duration;                              // when the projectile will self-destroy. 0 is 10 seconds - nothing should last that long...

        [Header("Damage Settings")]
        public int damage;                                  // damage the projectile does to anything it hits.

        [Header("Destruction Settings")]
        public float destroyDelay;                          // delay to destroy the projectile, so it can complete its 'hit' animation.


        /********************
         * =- Functions -=
         ********************/

        // Called by unity on object instantiation.
        void Start()
        {
            animator = GetComponent<Animator>();
            timeToDestroy = ((duration > 0) ? Time.time + duration : Time.time + 10); // set the time to destroy in the past if it's infinite.
        }

        // Called as part of the physics system.
        void FixedUpdate()
        {
            if (Time.time > timeToDestroy && hitAnimationTriggered)
                Destroy(gameObject);                        // animation finished, destroy.

            if (Time.time > timeToDestroy && !hitAnimationTriggered)
                AnimateHit();                               // just hit something, animate the hit.

            if (hitAnimationTriggered)                      // hit something, stop moving.
                return;

            transform.position += (transform.right * speed); // move the projectile forward.
        }

        // Called when a collider contacts another collider.
        void OnCollisionEnter2D(Collision2D collision)
        {
            AnimateHit();

            ActorController actorController = collision.gameObject.GetComponent<ActorController>();

            // Collision is not an actor, or is an actor of the same faction.
            if (!actorController || actorController.GetComponent<Stats>().faction == faction)
                return;

            // Collision is with an enemy actor and this actor has a melee attack, deal damage!
            actorController.GetComponent<Stats>().TakeDamage(damage);
        }

        // Inform the animator that the projectile hit something.
        void AnimateHit()
        {
            animator.SetBool("Hit", true);
            timeToDestroy = Time.time + destroyDelay;
            hitAnimationTriggered = true;
        }


        // Initialize the angle of the bullet so that it will move "forward" (to its right).
        public void Initialize(Faction faction, float xDegrees)
        {
            this.faction = faction;
            transform.Rotate(Vector3.forward, xDegrees);
//            transform.localEulerAngles = new Vector3(xDegrees, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }
}
