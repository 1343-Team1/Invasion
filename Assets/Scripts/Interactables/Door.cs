/// Author: Jeremy Anderson, March 22, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// This child class instructs an animator to Open when a signal is received.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Door : SignalReceiver
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== PRIVATE ==========
        Collider2D collider2d;                          // collider for the door.
        Animator animator;                              // animator for the door.
        float timeWhenAnimationComplete;                // used to determine when the door is ready for another signal.
        bool isAnimationPlaying;                        // used to determine whether the door is in the middle of an animation.
        float timeToClose;                              // used to determine whether the door is finished with its animation.
        bool isAutomaticSignal = false;                 // used to determine whether a signal is automatic or from the player.
        bool isOpen = false;                            // used to determine what state the door is in.

        // ========== PUBLIC ==========
        [Header("Animation Data")]
        public float openAnimationDuration;             // manually input the duration of the open animation in the editor.
        public float closeAnimationDuration;            // manually input the duration of the closing animation in the editor.

        [Header("Door Settings")]
        public bool signalCanClose = false;             // whether the signal can close the door, or just open it.
        public bool automaticallyCloses = true;         // whether the door automatically closes after a set duration.
        public float delayToClose;                      // the delay for automatic closure.


        /********************
         * =- Methods -=
         ********************/

        // Get the components from the gameObject.
        void Start()
        {
            collider2d = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
        }

        // React to any signals sent to the door.
        void Update()
        {
            // Shut the door after the timer if it auto-shuts.
            if (isOpen && automaticallyCloses && Time.time >= timeToClose)
            {
                receivedSignal = true;
                isAutomaticSignal = true;
            }

            if (!receivedSignal)                        // still waiting for a signal.
                return;                                 // check next frame.

            if (!isAnimationPlaying)                    // the animation hasn't started, start it.
                InformAnimator();

            if (Time.time >= timeWhenAnimationComplete) // the animation is complete, register that to allow new signals.
                RegisterCompletion();
        }

        // Perform the animation to open or close the door. Requires correct configuration of Animator to work.
        void InformAnimator()
        {
            if (!isOpen)                            // the door is closed and needs to open.
            {
                animator.SetBool("Open", true);     // animate the door opening.
                timeWhenAnimationComplete = Time.time + openAnimationDuration; // note the time to RegisterComplete().
            }
            else if (isAutomaticSignal || signalCanClose) // only an automatic signal or permission can close the door.
            {
                timeWhenAnimationComplete = Time.time + closeAnimationDuration; // note the time to RegisterComplete().
                animator.SetBool("Open", false);    // animate the door closing.
            }

            isAnimationPlaying = true;              // note that the animation is currently playing.
        }

        // Adjust the collider as necessary depending on the new state of the door.
        void RegisterCompletion()
        {
            isAnimationPlaying = false;             // register that the animation has finished.
            if (!isOpen)                            // the door is closed and needs to open.
            {
                isOpen = true;                      // note that the door is open now.
                collider2d.enabled = false;         // disable the collider to let Actors pass through.

                if (automaticallyCloses)            // prepare to automatically close door if needed.
                    timeToClose = Time.time + delayToClose;
            }
            else if (isAutomaticSignal || signalCanClose) // only an automatic signal or permission can close a door.
            {
                isOpen = false;                     // note that the door is closed now.
                collider2d.enabled = true;          // reenable the collider to block movement.
                isAutomaticSignal = false;          // clear the automatic signal flag.
            }

            ClearSignal();                          // allow for another signal.
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