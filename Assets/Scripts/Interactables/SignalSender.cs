/// Author: Jeremy Anderson, March 22, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// This parent class sends a signal to all SignalReceivers in its list.
    /// </summary>
    public class SignalSender : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE ==========
        Animator animator;                              // animator for the control panel.
        protected bool isReady;                                   // whether this signal sender is ready.

        // ========== PUBLIC ==========
        [Header("Objects To Signal")]
        public List<SignalReceiver> signalReceivers;    // populated in the editor!

        /********************
         * =- Functions -=
         ********************/

        // Initialize the animator.
        protected virtual void Start()
        {
            animator = GetComponent<Animator>();
        }

        protected virtual void Update()
        {
            // Check all the signalReceivers to see if they're ALL available for a signal.
            // This could be done more efficiently by having them report when they finish.
            foreach (SignalReceiver signalReceiver in signalReceivers)
            {
                if (!signalReceiver.ReadyForSignal()) // returns FALSE when they aren't ready.
                    return;
            }

            // They're all ready, switch the animation.
            Reset();
        }

        // Send a signal to a SignalReceiver.
        protected virtual bool SendSignal(SignalReceiver destination)
        {
            // Send a signal and report with TRUE if it succeeds.
            if (destination.TryReceiveSignal(this))
            {
                isReady = false;
                animator.SetBool("Activated", true);
                return true;
            }

            return false;
        }

        void Reset()
        {
            isReady = true;
            animator.SetBool("Activated", false);
        }
    }
}
