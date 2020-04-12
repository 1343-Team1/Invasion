/// Author: Jeremy Anderson, March 10, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// The stats for an actor.
    /// </summary>
    public class Stats : MonoBehaviour
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== PRIVATE / PROTECTED ==========
        ActorController actorController;                        // the ActorController for this actor.
        Shield shield;                                          // a shield that may be protecting the actor.
        [SerializeField] int currentHealth;                     // the real-time health of this actor.

        // ========== PUBLIC ==========
        [Header("Faction Settings")]
        public Faction faction;
        //public bool isSwarm = false;

        [Header("Health")]
        public bool isAlive = true;                             // whether this character is alive.
        public int maxHealth;                                   // the starting health of this actor.

        [Header("Movement")]
        public float runMultiplier;                             // the speed this Actor will run.
        public float walkSpeed;                                 // the speed this Actor will walk.

        [Header("Jumping")]
        public float jumpStrength;                              // the strength of this Actor's jump.
        public int extraJumps = 0;                              // the number of jumps this actor can make before falling.

        [Header("Vision")]
        public float sightRange;                                // the distance this Actor can see.
        public float sightDegrees;                              // the size of the pie that the actor can see.

        public Vector2 Position { get { return actorController.targetingPoint.transform.position; } }

        /********************
         * =- Functions -=
         ********************/

        // Initialize the actor.
        void Start()
        {
            actorController = GetComponent<ActorController>();
            shield = GetComponent<Shield>();
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            // Shield is up, take damage to it instead.
            if (shield && shield.IsActive())
            {
                shield.TakeDamage(damage);
                return;
            }

            // Shield is down, take damage to health.
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, currentHealth);

            // Actor died, destroy the shield.
            if (currentHealth <= 0)
            {
                if (shield)
                    Destroy(shield);

                if (actorController.brain)
                    actorController.brain.IsAlive = false;
                actorController.Kill();
                actorController.brain.Die();
                return;
            }
        }
    }
}
