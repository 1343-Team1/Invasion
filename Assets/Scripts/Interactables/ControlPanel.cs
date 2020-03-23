/// Author: Jeremy Anderson, March 22, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Sends a signal to every object in its list of objects that can recieve signals.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ControlPanel : SignalSender
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE ==========
        bool isPlayerPresent = false;
        PlayerInput playerInput;

        // ========== PUBLIC ==========
        [Header("Objects To Signal")]
        public List<SignalReceiver> signalReceivers;    // populated in the editor!


        /********************
         * =- Functions -=
         ********************/

        void Update()
        {
            if (!isPlayerPresent)                       // the player isn't present, don't worry about it.
                return;

            if (playerInput && playerInput.IsPressingActionKey())
            {
                int count = signalReceivers.Count;
                for (int i = 0; i < count; i++)
                {
                    SendSignal(signalReceivers[i]);
                }
            }
        }

        // Track when the player enters the trigger.
        void OnTriggerEnter2D(Collider2D collision)
        {
            playerInput = collision.GetComponent<PlayerInput>();
            if (playerInput)
                isPlayerPresent = true;

            // Reveal the UI widget.
        }

        // Track when the player leave the trigger.
        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.GetComponent<PlayerInput>())
            {
                isPlayerPresent = false;
                playerInput = null;
            }
        }
    }
}
