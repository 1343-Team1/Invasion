using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
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
        Stats stats;                                            // a reference to the actors stats for speed.
        Vector2 input = new Vector2();                          // regularly updated by the ActorInput component.
        bool accelerating = false;                              // used to determine whether to slow down.

        // Public variables.
        [Header("Settings")]
        public Vector2 slowDownAt = new Vector2();              // a drag amplifier to force a quick slow down.

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
            rigidbody2d.AddForce(input);

            // Limit max velocity.
            if (rigidbody2d.velocity.magnitude > stats.maximumSpeed)
                rigidbody2d.velocity = rigidbody2d.velocity.normalized * stats.maximumSpeed;

            // Record for debugging.
            velocity = rigidbody2d.velocity;
           
            // If there is no input, slow down immediately.
            if (!accelerating && Mathf.Abs(input.x) < slowDownAt.x && rigidbody2d.velocity.magnitude > 0)
                SlowDown();
        }

        void SlowDown() {
            rigidbody2d.AddForce(-rigidbody2d.velocity);
        }

        // Take axis input from an ActorInput component.
        public void InformAxis(Vector2 input)
        {
            float oldX = Mathf.Abs(this.input.x);
            float newX = Mathf.Abs(input.x);

            accelerating = (newX >= oldX);
            this.input = input;
        }

    }
}
