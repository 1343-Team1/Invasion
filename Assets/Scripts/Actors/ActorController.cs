using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    [RequireComponent(typeof(Rigidbody2D))][RequireComponent(typeof(Collider2D))]
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
        bool waitingToJump = false;                                 // used to determine whether the actor is jumping.

        // Public variables.
        [Header("! IF NO STATS COMPONENT !")]
        public float maximumVelocity;                           // the maximum speed the character may move by force.
        public float jumpStrength;                              // the strength of the character's jumps.

        [Header("Alternate Movement Settings")]
        public bool moveByForce = false;                        // make the actor move by applying force instead of setting velocity.
        public Vector2 accelerationRate = new Vector2();        // allow precise control of acceleration when using force mode.
        public Vector2 decelerateAt = new Vector2();            // a drag amplifier to force a quick deceleration below a specified velocity.
        public Vector2 decelerationMultiplier = new Vector2();  // modify the rate of deceleration.

        [Header("Alternate Jump Settings")]
        public bool jumpByForce = false;                        // make the actor jump by applying force instead of setting velocity.
        public float jumpForceMultiplier;                       // allow precise control of jump strength when using force jump mode.

        // Exposed private/protected variables.
        [Header("Debug Data")]
        [SerializeField][DisplayWithoutEdit()] Vector2 velocity = new Vector2();    // display the velocity.


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

        // Move and animate the actor according to input and state.
        void FixedUpdate()
        {
            // ---- Movement ----
            if (!moveByForce)       // The default movement method sets the velocity directly.
                MoveByVelocity();
            else                    // Optional movement by adding force to the rigidbody2d.
                MoveByForce();

            if (ShouldDecelerate()) // Slowing down only affects moving in force mode.
                Decelerate();

            // ---- Jumping ----
            if (waitingToJump)
            {
                if (!jumpByForce)       // The default jumping method sets the velocity directly.
                    JumpByVelocity();
                else                    // Optional jumping by adding force to the rigidbody2d.
                    JumpByForce();
            }
        }

        // Jump by setting the velocity of the rigidbody2d directly from stats.jumpStrength if present, or jumpStrength.
        void JumpByVelocity()
        {
            rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, ((stats) ? stats.jumpStrength : jumpStrength));
            waitingToJump = false;
        }

        void JumpByForce()
        {
            rigidbody2d.AddForce(new Vector2(0.0f, ((stats) ? stats.jumpStrength : jumpStrength)) * jumpForceMultiplier);
            waitingToJump = false;
        }

        // Take jump input from an ActorInput component.
        public void InformJump(bool isJumping)
        {
            if (isJumping)
                waitingToJump = true;
        }

        // Move by setting the velocity of the rigidbody2d directly from stats.speed if present, or maximumVelocity.
        void MoveByVelocity()
        {
            float moveBy = input.x * ((stats) ? stats.speed : maximumVelocity);
            rigidbody2d.velocity = new Vector2(moveBy, rigidbody2d.velocity.y);
        }

        // Move by adding accelerationRate in force to the rigidbody, limited to maximumVelocity.
        void MoveByForce()
        {
            rigidbody2d.AddForce(input);
            LimitVelocity();
        }

        // Limit maximum velocity of the rigidbody to the Stats.speed if present, or the maximumVelocity.
        void LimitVelocity()
        {
            float maxVelocity = ((stats) ? stats.speed : maximumVelocity);
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
