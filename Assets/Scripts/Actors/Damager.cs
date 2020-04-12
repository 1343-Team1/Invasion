/// Author: Jeremy Anderson, April 4, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// TakesDamage from any source.
    /// </summary>
    [RequireComponent(typeof(Collider2D))][RequireComponent(typeof(Stats))]
    public class Damager : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE / PROTECTED ==========
        Rigidbody2D rigidbody2d;                                // reference to the rigidbody.
        ActorController actorController;                        // reference to the ActorController on the gameObject.
        float timeCanMeleeAgain;                                // used to allow melee attacking with a delay between strikes.
        float timeCanFireAgain;                                 // used to allow firing with a delay between projectiles.
        float timeReboundComplete;                              // used to allow rebound after successful melee attack.

        // ========== PUBLIC ==========
        [Header("Melee Attack Settings")]
        public bool hasMeleeAttack = false;                     // if the actor has a melee attack.
        public int meleeDamage;                                 // the amount of damage this actor deals on contact.
        public float meleeReboundDuration;                      // how long the actor bounces backward.
        public float meleeReboundDistance;                      // how far back the rebound pushes them.
        public float meleeAttackDelay;                          // how long to wait between melee attacks.

        [Header("Ranged Attack Settings")]
        public bool hasRangedAttack = false;                    // if the actor has a ranged attack.
        public AudioClip fireAudio;                             // the audio clip for firing the gun.
        public bool canFireWhileInAir = false;                  // whether the actor can fire while jumping or falling.
        public GameObject projectilePrefab;                     // the prefab of the projectile the ranged attack makes.
        public GameObject projectileOrigin;                     // the object that is the spawn origin of the projectile.
        public bool usesFireAngles = false;                     // whether the actor uses the fire angles below.
        public Vector2 fireAngleUpCoords;                       // the relative coordinates of the origin when firing "up" at an angle.
        public float fireAngleUpXAngle;                         // the x angle of the upward firing position.
        public Vector2 fireAngleCenterCoords;                   // the relative coordinates of the origin when firing "ahead" to the right.
        public float fireAngleCenterXAngle;                     // the x angle of the center firing position.
        public Vector2 fireAngleDownCoords;                     // the relative coordinates of the origin when firing "down" at an angle.
        public float fireAngleDownXAngle;                       // the x angle of the downward firing position.
        public float fireDelay;                                 // how long to wait between bullets, and to stop the firing animation.
        // public Vector2 fireAngleCrouchedUpCoords;
        //public float fireAngleCrouchUpXAngle;        
        // public Vector2 fireAngleCrouchedCenterCoords;
        //public float fireAngleCrouchCenterXAngle;    
        // public Vector2 fireAngleCrouchedDownCoords;
        //public float fireAngleCrouchDownXAngle;      

        /********************
         * =- Functions -=
         ********************/

        // Initialize.
        void Start()
        {
            actorController = GetComponent<ActorController>();
            rigidbody2d = GetComponent<Rigidbody2D>();
            timeCanFireAgain = Time.time;
            timeCanMeleeAgain = Time.time;
        }

        // Move the projectile origin and allow shooting at intervals.
        void Update()
        {
            // (It is important that these settings match the animator)
            PositionProjectileOrigin();                         // this is not related to physics.
            StopShooting();                                     // stop shooting if the gun wasn't fired recently.

            if (Time.time > timeReboundComplete)
                actorController.MeleeRebounding(false);
        }

        // ========== MELEE ATTACK ==========
        void OnCollisionEnter2D(Collision2D collision)
        {
            // If dead, don't worry about it.
            if (actorController.IsDead)
                return;

            // If no melee attack, don't worry about it.
            if (!hasMeleeAttack)
                return;

            // If too soon, don't worry about it.
            if (Time.time < timeCanMeleeAgain)
                return;

            ActorController otherActorController = collision.gameObject.GetComponent<ActorController>();

            // Collision is not an actor.
            if (!otherActorController)
                return;

            Stats otherActorStats = otherActorController.stats;

            // Same faction, ignore the attack.
            if (otherActorStats.faction == actorController.stats.faction)
                return;

            // Collision is with an enemy actor and this actor has a melee attack, deal damage!
            otherActorStats.TakeDamage(meleeDamage);
            timeCanMeleeAgain = Time.time + meleeAttackDelay;

            // Push the attacker back a little to seperate the colliders.
            Vector2 direction = transform.position - collision.transform.position;
            rigidbody2d.AddForce(direction.normalized * meleeReboundDistance);
            actorController.MeleeRebounding(true);
            timeReboundComplete = Time.time + meleeReboundDuration;
        }


        // ========== RANGED ATTACK ==========
        public bool FireProjectile()
        {
            if (!hasRangedAttack)                               // doesn't have a ranged attack.
                return false;                                   // early out.

            if (Time.time < timeCanFireAgain)                   // can't fire again yet.
                return false;                                   // early out.

            if (!canFireWhileInAir && rigidbody2d.velocity.y != 0) // can't fire in the air.
                return false;                                   // early out.

            timeCanFireAgain = Time.time + fireDelay;           // calculate the next time to fire.

            // Instantiate the projectile.
            float originX = projectileOrigin.transform.localPosition.x;
            float originY = projectileOrigin.transform.localPosition.y;
            originX = ((actorController.FacingRight) ? originX : -originX);

            Vector3 originPosition = transform.position + new Vector3(originX, originY, 0);

            GameObject obj = Instantiate(projectilePrefab, originPosition, Quaternion.identity);
            Projectile projectile = obj.GetComponent<Projectile>();

            if (usesFireAngles)
                projectile.Initialize(actorController.stats.faction, GetFiringAngle());

            else
                projectile.Initialize(actorController.stats.faction, (actorController.FacingRight) ? fireAngleCenterXAngle : 180 + fireAngleCenterXAngle);

            AudioManager.PlaySFX(fireAudio);
            return true;
        }

        // Determine which firing angle is being used according to vertical input data.
        float GetFiringAngle()
        {
            if (actorController.Input.y > 0.01)                 // firing up.
                return ((actorController.FacingRight) ? fireAngleUpXAngle : 180 - fireAngleUpXAngle);
            else if (actorController.Input.y < -0.01)           // firing down.
                return ((actorController.FacingRight) ? fireAngleDownXAngle : 180 - fireAngleDownXAngle);
            else                                                // firing ahead.
                return ((actorController.FacingRight) ? fireAngleCenterXAngle : 180 + fireAngleCenterXAngle);
        }

        // Determine which firing angle is being used according to vertical input data.
        Vector3 GetFiringAngleCoords()
        {
            if (!usesFireAngles)                                // only left and right.
                return fireAngleCenterCoords;

            if (actorController.Input.y > 0)                    // firing up.
                return fireAngleUpCoords;
            else if (actorController.Input.y < 0)               // firing down.
                return fireAngleDownCoords;
            else                                                // firing ahead.
                return fireAngleCenterCoords;
        }

        // Position projectile origin according to vertical input data.
        void PositionProjectileOrigin()
        {
            if (!projectileOrigin)                              // non-ranged actors have no projectile origin.
                return;

            projectileOrigin.transform.localPosition = GetFiringAngleCoords();
        }

        // Stop shooting.
        void StopShooting()
        {
            if (Time.time < timeCanFireAgain)                   // the animation should still be playing.
                return;                                         // early out.

            actorController.Shoot(false);
        }

    }
}
