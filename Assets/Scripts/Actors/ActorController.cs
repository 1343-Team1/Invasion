using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    [RequireComponent(typeof(Rigidbody2D))] [RequireComponent(typeof(Collider2D))]
    /// <summary>
    /// Translates input received from an ActorInput object into actual movement,
    /// whether from the player or AI, and coordinates the animations.
    /// </summary>
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
        int numberOfJumps = 0;                                  // used to determine whether the actor can jump yet.

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
        }

        // ========== ANIMATIONS ==========
        // Animate the actor in the Update so that there is immediate response.
        void Update()
        {
            // ---- Movement ----
            FaceActorTowardMovement();
            InformAnimator();
            AnimateToIdle();

            // ---- Jumping ----
            AnimateJetpack();
            AnimateFalling();
        }

        // Animate the jetpack.
        void AnimateJetpack()
        {
            if (jetPackParticleBurster && isJumpBursting)
            {
                Debug.Log("animating");
                jetPackParticleBurster.BurstParticles();
            }
        }

        // Update the animator with the current velocity of the actor.
        void InformAnimator()
        {
            animator.SetFloat("Horizontal Velocity", Mathf.Abs(rigidbody2d.velocity.x));
            animator.SetFloat("Vertical Velocity", rigidbody2d.velocity.y);
            animator.SetBool("Running", isRunning);
            if (Mathf.Abs(rigidbody2d.velocity.x) > 0)
                animator.SetBool("Walking", true);
        }

        // Use the horizontal transform scale to turn the actor in the direction of their velocity.
        void FaceActorTowardMovement()
        {
            if (rigidbody2d.velocity.x < 0 && transform.localScale.x != -1)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (rigidbody2d.velocity.x > 0 && transform.localScale.x != 1)
                transform.localScale = new Vector3(1, 1, 1);
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
            rigidbody2d.AddForce(new Vector2(0.0f, ((stats) ? stats.jumpStrength : jumpStrength)) * jumpForceMultiplier);
            waitingToJump = false;

            if (jetPackParticleBurster)
                isJumpBursting = true;              // turn on the jetpack particles.
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
            LimitVelocity();
        }

        // Limit maximum velocity of the rigidbody to the Stats.speed if present, or the maximumVelocity.
        void LimitVelocity()
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
