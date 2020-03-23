/// Author: Jeremy Anderson, March 22, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Instructs an animator to Open when a signal is received.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Door : SignalReceiver
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== PRIVATE ==========
        SpriteRenderer spriteRenderer;
        Collider2D collider2d;
        Animator animator;
        float timeWhenAnimationComplete;
        bool isAnimationPlaying;
        float timeToClose;
        bool isAutomaticSignal = false;
        bool isOpen = false;

        // ========== PUBLIC ==========
        [Header("Animation Data")]
        public float openAnimationDuration;
        public float closeAnimationDuration;

        [Header("Door Settings")]
        public bool signalCanClose = false;
        public bool automaticallyCloses = true;
        public float delayToClose;


        /********************
         * =- Methods -=
         ********************/

        // Start is called before the first frame update
        void Start()
        {
            collider2d = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Update is called once per frame
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

            // This door can be opened/closed repeatedly.
            if (!isAnimationPlaying)
            {
                if (!isOpen)
                {
                    animator.SetBool("Open", true);         // open the door.
                    timeWhenAnimationComplete = Time.time + openAnimationDuration;
                }
                else if (isAutomaticSignal || signalCanClose)
                {
                    timeWhenAnimationComplete = Time.time + closeAnimationDuration;
                    animator.SetBool("Open", false);        // close the door.
                }

                isAnimationPlaying = true;
            }

            if (Time.time >= timeWhenAnimationComplete)
            {
                isAnimationPlaying = false;             // the animation has finished.
                if (!isOpen)
                {
                    isOpen = true;
                    collider2d.enabled = false;         // disable the collider to let Actors pass through.
                    spriteRenderer.enabled = false;

                    if (automaticallyCloses)
                        timeToClose = Time.time + delayToClose;
                }
                else if (isAutomaticSignal || signalCanClose)
                {
                    isOpen = false;
                    collider2d.enabled = true;
                    spriteRenderer.enabled = true;
                    isAutomaticSignal = false;
                }

                ClearSignal();                          // allow for another signal.
            }
        }

        // Attempts to receive a sent signal.
        public override bool TryReceiveSignal(SignalSender origin)
        {
            if (receivedSignal)                         // already received the signal.
                return false;                           // report failure.

            // This object is ready to receive the signal.
            return receivedSignal = true;               // receive the signal and report success!
        }
    }
}