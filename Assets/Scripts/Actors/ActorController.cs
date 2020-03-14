/// Author: Jeremy Anderson, March 10, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Translates input received from an ActorInput object into actual movement,
    /// whether from the player or AI, and coordinates the animations.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))][RequireComponent(typeof(Collider2D))]
    public class ActorController : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // Private/protected variables.
        GameManager GM;                                         // a reference to the GameManager.
        Rigidbody2D rigidbody2d;                                // a reference to the rigidbody component.
        Animator animator;                                      // a reference to the animator.
        Stats stats;                                            // a reference to the actors stats for speed. If this component isn't present, maximumSpeed will be used.
        Vector2 input = new Vector2();                          // regularly updated by the ActorInput component.
        bool accelerating = false;                              // used to determine whether to slow down.
        bool waitingToJump = false;                             // used to determine whether the actor is jumping.
        bool isRunning = false;                                 // used to determine whether the actor is running.
        bool isJumpBursting = false;                            // used to determine whether the actor is using a jetpack.
        bool isFalling = false;                                 // used to determine whether the actor is falling.
        [SerializeField] bool isDead = false;                   // used to determine whether the actor is dead.
        int numberOfJumps = 0;                                  // used to determine whether the actor can jump yet.
        float editorGravity;                                    // used to store the editor gravity for the actor.
        float timeCanFireAgain;                                 // used to allow firing with a delay between projectiles.
        bool facingRight = true;                                // used to coordinate projectile origin when still.                           

        // Public variables.
        [Header("! IF NO STATS COMPONENT !")]
        public float runMultiplier;                             // the running speed of the actor.
        public float walkSpeed;                                 // the walking speed of the actor.
        public float jumpStrength;                              // the strength of the character's jumps.
        public int extraJumps;                                  // the maximum jumps this actor can make before falling.

        [Header("Force Based Movement Settings")]
        public bool moveByForce = false;                        // make the actor move by applying force instead of setting velocity.
        public Vector2 accelerationRate = new Vector2();        // allow precise control of acceleration when using force mode.
        public Vector2 decelerateAt = new Vector2();            // a drag amplifier to force a quick deceleration below a specified velocity.
        public Vector2 decelerationMultiplier = new Vector2();  // modify the rate of deceleration.

        [Header("General Jump Settings")]
        public ParticleBurster jetPackParticleBurster;          // a reference to the particle burster (if there is one).
        public bool jetPackOnlyOnForceJump;                     // is the jetpack only used on the force jump?
        public float fallingControlMultiplier;                  // how much the player can influence the fall trajectory of the actor.

        [Header("Force Based Jump Settings")]
        public bool jumpByForce = false;                        // make the actor jump by applying force instead of setting velocity.
        public float jumpForceMultiplier;                       // allow precise control of jump strength when using force jump mode.
        public float jumpVelocityMaximum;                       // stops the force-based jump from compounding too strongly.
        public float jumpGravity;                               // adjust the gravity effect for force jumping.

        [Header("Ranged Attack")]
        public bool hasRangedAttack = false;                    // if the actor has a ranged attack.
        public bool canFireWhileInAir = false;                  // whether the actor can fire while jumping or falling.
        public GameObject projectilePrefab;                     // the prefab of the projectile the ranged attack makes.
        public GameObject projectileOrigin;                     // the object that is the spawn origin of the projectile.
        public Vector2 fireAngleUpCoords;                       // the relative coordinates of the origin when firing "up" at an angle.
        public float fireAngleUpXAngle;                         // the x angle of the upward firing position.
        public Vector2 fireAngleCenterCoords;                   // the relative coordinates of the origin when firing "ahead" to the right.
        public float fireAngleCenterXAngle;                     // the x angle of the center firing position.
        public Vector2 fireAngleDownCoords;                     // the relative coordinates of the origin when firing "down" at an angle.
        public float fireAngleDownXAngle;                       // the x angle of the downward firing position.
        public float fireDelay;                                 // how long to wait between bullets.
        // public Vector2 fireAngleCrouchedUpCoords;
        //public float fireAngleCrouchUpXAngle;        
        // public Vector2 fireAngleCrouchedCenterCoords;
        //public float fireAngleCrouchCenterXAngle;    
        // public Vector2 fireAngleCrouchedDownCoords;
        //public float fireAngleCrouchDownXAngle;      

        // Exposed private/protected variables.
        [Header("Debug Data")]
        [SerializeField] [DisplayWithoutEdit()] Vector2 velocity = new Vector2();    // display the velocity.


        /********************
         * =- Functions -=
         ********************/

        // Give the ActorController component access to the GameManager and certain other components.
        void Start()
        {
            GM = FindObjectOfType<GameManager>();
            rigidbody2d = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            stats = GetComponent<Stats>();
            editorGravity = rigidbody2d.gravityScale;
            timeCanFireAgain = Time.time;
        }

        // ========== ANIMATIONS ==========
        // Animate the actor in the Update so that there is immediate response.
        void Update()
        {
            // ---- Unique ----
            animator.SetBool("Dead", isDead);                   // inform the animator whether the actor is alive or dead.
            if (isDead)
                return;                                         // don't worry about the rest of Update if the actor is dead.

            UpdateGravity();

            // ---- Movement ----
            FaceActorTowardMovement();
            InformAnimator();
            AnimateToIdle();

            // ---- Jumping ----
            AnimateJetpack();
            AnimateFalling();

            // ---- Firing ---- (It is important that these settings match the animator)
            PositionProjectileOrigin();                         // this is not related to physics.
        }

        // Fire a projectile.
        public void FireProjectile()
        {
            if (!hasRangedAttack)                               // doesn't have a ranged attack.
                return;                                         // early out.

            if (Time.time < timeCanFireAgain)                   // can't fire again yet.
                return;                                         // early out.

            if (!canFireWhileInAir && rigidbody2d.velocity.y != 0) // can't fire in the air.
                return;                                         // early out.

            timeCanFireAgain = Time.time + fireDelay;           // calculate the next time to fire.

            // Instantiate the projectile.
            float originX = projectileOrigin.transform.localPosition.x;
            float originY = projectileOrigin.transform.localPosition.y;
            originX = ((facingRight) ? originX : -originX);

            Vector3 originPosition = transform.position + new Vector3(originX, originY, 0);
            
            GameObject obj = Instantiate(projectilePrefab, originPosition, Quaternion.identity);
            Projectile projectile = obj.GetComponent<Projectile>();

            projectile.Initialize(GetFiringAngle());                 // set its angle.
        }

        // Determine which firing angle is being used according to vertical input data.
        float GetFiringAngle()
        {
            if (input.y > 0.01)                                 // firing up.
                return ((facingRight) ? fireAngleUpXAngle : 180 - fireAngleUpXAngle);
            else if (input.y < -0.01)                           // firing down.
                return ((facingRight) ? fireAngleDownXAngle : 180 - fireAngleDownXAngle);
            else                                                // firing ahead.
                return ((facingRight) ? fireAngleCenterXAngle : 180 + fireAngleCenterXAngle);
        }

        // Determine which firing angle is being used according to vertical input data.
        Vector3 GetFiringAngleCoords()
        {
            if (input.y > 0)                                    // firing up.
                return fireAngleUpCoords;
            else if (input.y < 0)                               // firing down.
                return fireAngleDownCoords;
            else                                                // firing ahead.
                return fireAngleCenterCoords;
        }

        // Position projectile origin according to vertical input data.
        void PositionProjectileOrigin()
        {
            projectileOrigin.transform.localPosition = GetFiringAngleCoords();
        }

        // Keep track of the actor's gravity based on force jump settings.
        void UpdateGravity()
        {
            if (jumpByForce && rigidbody2d.gravityScale != jumpGravity)
                rigidbody2d.gravityScale = jumpGravity;

            else if (!jumpByForce && rigidbody2d.gravityScale != editorGravity)
                rigidbody2d.gravityScale = editorGravity;
        }

        // Animate the jetpack.
        void AnimateJetpack()
        {
            if (jetPackParticleBurster && isJumpBursting)
                jetPackParticleBurster.BurstParticles();
        }

        // Update the animator with the current velocity of the actor.
        void InformAnimator()
        {
            animator.SetFloat("Horizontal Velocity", Mathf.Abs(rigidbody2d.velocity.x));
            animator.SetFloat("Vertical Velocity", rigidbody2d.velocity.y);
            animator.SetFloat("Vertical Input", input.y);
            animator.SetBool("Running", isRunning);
            if (Mathf.Abs(rigidbody2d.velocity.x) > 0)
                animator.SetBool("Walking", true);
        }

        // Use the horizontal transform scale to turn the actor in the direction of their velocity.
        void FaceActorTowardMovement()
        {
            if (rigidbody2d.velocity.x < 0 && transform.localScale.x != -1)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                facingRight = false;
            }
            else if (rigidbody2d.velocity.x > 0 && transform.localScale.x != 1)
            {
                transform.localScale = new Vector3(1, 1, 1);
                facingRight = true;
            }
        }

        // Let the animator know the actor is falling.
        void AnimateFalling()
        {
            if (rigidbody2d.velocity.y >= 0)        // grounded or jumping, but not falling.
            {
                animator.SetBool("Falling", false); // let the animator know the actor isn't falling.
                isFalling = false;                  // record that the actor is no longer falling.

                if (rigidbody2d.velocity.y == 0)    // grounded!
                    numberOfJumps = 0;              // reset the jumps.

                return;                             // early out.
            }

            // Velocity.y is less than 0, the actor is falling.
            if (!waitingToJump)
                animator.SetBool("Jumping", false);     // let the animator know the actor is not jumping.

            isJumpBursting = false;                     // turn off the jetPack particles.
            animator.SetBool("Falling", true);          // let the animator know the actor is falling.
            isFalling = true;                           // record that the actor is falling.
        }

        // Animate back to idle only if not moving.
        void AnimateToIdle()
        {
            if (!waitingToJump && rigidbody2d.velocity.x > -0.01 && rigidbody2d.velocity.x < 0.01)
                animator.SetBool("Walking", false);
        }

        // ========== PHYSICS ==========
        // Move the actor according to input and state in the FixedUpdate, so it synchs with physics.
        void FixedUpdate()
        {
            // ---- Unique ----
            if (isDead)
                return;                                 // don't worry about the rest of Update if the actor is dead.

            // ---- Movement ----
            if (!moveByForce)       // the default movement method sets the velocity directly.
                MoveByVelocity();
            else                    // optional movement by adding force to the rigidbody2d.
                MoveByForce();

            if (ShouldDecelerate()) // slowing down only affects moving in force mode.
                Decelerate();

            // ---- Jumping ----
            if (waitingToJump)
            {
                if (!jumpByForce)       // the default jumping method sets the velocity directly.
                    JumpByVelocity();
                else                    // optional jumping by adding force to the rigidbody2d.
                    JumpByForce();
            }
        }

        // Jump by setting the velocity of the rigidbody2d directly from stats.jumpStrength if present, or jumpStrength.
        void JumpByVelocity()
        {
            rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, ((stats) ? stats.jumpStrength : jumpStrength));
            waitingToJump = false;

            if (jetPackParticleBurster && !jetPackOnlyOnForceJump)
                isJumpBursting = true;              // turn on the jetpack particles.
        }

        void JumpByForce()
        {
            float jumpForce = ((stats) ? stats.jumpStrength : jumpStrength) * jumpForceMultiplier;
            jumpForce = ((jumpForce + rigidbody2d.velocity.y > jumpVelocityMaximum) ? 0 : jumpForce);
            rigidbody2d.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
            waitingToJump = false;

            if (jetPackParticleBurster)
                isJumpBursting = true;              // turn on the jetpack particles.

            velocity = rigidbody2d.velocity;
        }

        // Take jump input from an ActorInput component.
        public void InformJump(bool isJumping)
        {
            if (!waitingToJump && isJumping && numberOfJumps < ((stats) ? stats.extraJumps : extraJumps)) // can this actor jump right now?
            {
                waitingToJump = true;                // this will be looked at to inform physics.
                animator.SetBool("Jumping", true);   // this will be looked at to inform animation.
                numberOfJumps++;                     // one less jump available.
            }
        }

        // Take run input from an ActorInput component.
        public void InformRun(bool isRunning, bool isToggle)
        {
            if (!isToggle)                          // if it's not a toggle, just track the data.
            {
                this.isRunning = isRunning;
                return;
            }

            if (isRunning)                          // if running is a toggle, toggle it.
                this.isRunning = !this.isRunning;
        }

        // Move by setting the velocity of the rigidbody2d directly from stats.speed if present, or maximumVelocity.
        void MoveByVelocity()
        {
            float moveBy = input.x * ((stats) ? stats.walkSpeed : walkSpeed);

            if (isRunning)
                moveBy *= runMultiplier;

            if (isFalling)
                moveBy *= fallingControlMultiplier;

            rigidbody2d.velocity = new Vector2(moveBy, rigidbody2d.velocity.y);
        }

        // Move by adding accelerationRate in force to the rigidbody, limited to maximumVelocity.
        void MoveByForce()
        {
            if (isRunning)
                input *= runMultiplier;

            if (isFalling)
                input *= fallingControlMultiplier;

            rigidbody2d.AddForce(input);
            LimitRunVelocity();
        }

        // Limit maximum velocity of the rigidbody to the Stats.speed if present, or the maximumVelocity.
        void LimitRunVelocity()
        {
            float maxVelocity = ((stats) ? stats.walkSpeed : walkSpeed);

            if (isRunning)
                maxVelocity *= runMultiplier;

            if (Mathf.Abs(rigidbody2d.velocity.x) > maxVelocity)
                rigidbody2d.velocity = new Vector2 (
                    ((rigidbody2d.velocity.x > 0) ? maxVelocity : -maxVelocity),
                    rigidbody2d.velocity.y);

            // Record for debugging.
            velocity = rigidbody2d.velocity;
        }

        // If there is no input, slow down immediately.
        bool ShouldDecelerate()
        {
            return !accelerating && Mathf.Abs(rigidbody2d.velocity.x) > 0 && Mathf.Abs(rigidbody2d.velocity.x) < decelerateAt.x;
        }

        // Decelerate at an increased rate.
        void Decelerate() {
            rigidbody2d.AddForce(-rigidbody2d.velocity * decelerationMultiplier);
        }

        // Take axis input from an ActorInput component.
        public void InformAxis(Vector2 input)
        {
            float oldX = Mathf.Abs(this.input.x);
            float newX = Mathf.Abs(input.x);

            accelerating = (newX != 0) || (newX > oldX);
            this.input = input;
        }

    }
}
