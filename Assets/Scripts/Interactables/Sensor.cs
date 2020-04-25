/// Author: Jeremy Anderson, April 25, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// This child class sends a signal to every object in its list of objects that can recieve signals.
    /// The signal is sent when the player character enters the or is inside of the trigger.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Sensor : SignalSender
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE ==========
        PlayerInput playerInput;                        // REPLACE THIS WITH YOUR CHARACTER CONTROLLER!

        // ========== PUBLIC ==========
        [Header("Activation Settings")]
        public bool triggersOnce = false;               // Whether it should destroy itself after triggering.
        public bool closesWhenPlayerLeaves = false;     // Whether it should send a signal when the player leaves.

        [Header("Audio Settings")]
        public AudioClip activationClip;


        /********************
         * =- Functions -=
         ********************/

        // Initialize the widget.
        protected override void Start()
        {
            base.Start();
        }

        // Allow the player to trigger the SignalSender.
        protected override void Update()
        {
            // The parent class' Update method.
            base.Update();
        }

        // Send a signal to every SignalReceiver in the list.
        void Activate()
        {
            bool signalSent = false;
            int count = signalReceivers.Count;
            for (int i = 0; i < count; i++)
                signalSent = (SendSignal(signalReceivers[i]) || signalSent) ? true : false;

            if (signalSent)
                AudioManager.PlaySFX(activationClip);
        }

        // Track when the player enters the trigger.
        void OnTriggerEnter2D(Collider2D collision)
        {
            // Find out who entered.
            playerInput = collision.GetComponent<PlayerInput>();

            // It's not the player, ignore it.
            if (!playerInput) 
                return;

            // Activate and destroy the object if necessary.
            Activate();
            if (triggersOnce)
                Destroy(this);
        }

        // Track when the player leaves the trigger.
        void OnTriggerExit2D(Collider2D collision)
        {
            // MAKE SURE YOU ARE SEARCHING FOR YOUR CHARACTER CONTROLLER!
            if (!collision.GetComponent<PlayerInput>()) // not the player, ignore it.
                return;

            if (closesWhenPlayerLeaves)
                Activate();
        }
    }
}
