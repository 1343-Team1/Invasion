/// Author: Jeremy Anderson, April 25, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// This child class instructs an animator to Open when a signal is received.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collapser : SignalReceiver
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== PRIVATE ==========
        Collider2D collider2d;                          // collider for the collapsing platform.
        Animator animator;                              // animator for the collapsing platform.
        float timeWhenAnimationComplete;                // used to determine when the collapsing platform is finished animating.
        bool isAnimationPlaying;                        // used to determine whether the collapsing platform is in the middle of an animation.

        // ========== PUBLIC ==========
        [Header("Animation Data")]
        public float collapseAnimationDuration;         // manually input the duration of the activation animation in the editor.

        [Header("Sound Settings")]
        public AudioClip collapseClip;

        [Header("Collapse Settings")]
        public bool colliderFalls = false;              // whether the collapser collider will fall.
        public bool togglesCollider = false;            // whether the collapser collider remains active after finishing.


        /********************
         * =- Methods -=
         ********************/

        // Get the components from the gameObject.
        void Start()
        {
            collider2d = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
        }

        // React to any signals sent to the collapsable object.
        void Update()
        {
            if (!receivedSignal)                        // still waiting for a signal.
                return;                                 // check next frame.

            if (animator && !isAnimationPlaying)        // the animation hasn't started, start it.
                InformAnimator();

            if (!animator || Time.time >= timeWhenAnimationComplete) // the animation is complete, register that to allow new signals.
                RegisterCompletion();
        }

        // Perform the animation to collapse the platform. Requires correct configuration of Animator to work.
        void InformAnimator()
        {
            AudioManager.PlaySFX(collapseClip);

            // There's no animator, early out.
            if (!animator)
                return;

            animator.SetBool("Collapsing", true);     // animate the collapse.
            timeWhenAnimationComplete = Time.time + collapseAnimationDuration; // note the time to RegisterComplete().
            isAnimationPlaying = true;              // note that the animation is currently playing.
        }

        // Adjust the collider as necessary depending on the settings.
        void RegisterCompletion()
        {
            isAnimationPlaying = false;             // register that the animation has finished.

            // Fall.
            if (colliderFalls)
            {
                Rigidbody2D rigidbody2d = GetComponent<Rigidbody2D>();
                rigidbody2d.constraints = RigidbodyConstraints2D.None;
                rigidbody2d.AddForce(Vector2.up);
            }

            // Deactivate Collider.
            if (togglesCollider)
                collider2d.enabled = false;

            ClearSignal();
        }

        // Attempts to receive a sent signal.
        public override bool TryReceiveSignal(SignalSender origin)
        {
            if (receivedSignal)                         // already received the signal.
                return false;                           // report failure - doesn't matter right now.

            // This object is ready to receive the signal.
            return receivedSignal = true;               // receive the signal and report success - doesn't matter right now.
        }
    }
}