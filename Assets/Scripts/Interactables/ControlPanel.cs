﻿/// Author: Jeremy Anderson, March 22, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// This child class sends a signal to every object in its list of objects that can recieve signals.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ControlPanel : SignalSender
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE ==========
        bool isPlayerPresent = false;                   // used to determine if the player is in the activatable zone.
        PlayerInput playerInput;                        // REPLACE THIS WITH YOUR CHARACTER CONTROLLER!

        // ========== PUBLIC ==========
        [Header("Objects To Signal")]
        public List<SignalReceiver> signalReceivers;    // populated in the editor!


        /********************
         * =- Functions -=
         ********************/

        // Allow the player to trigger the SignalSender.
        void Update()
        {
            if (!isPlayerPresent)                       // the player isn't present, don't worry about it.
                return;

            if (playerInput && playerInput.IsPressingActionKey()) // MAKE SURE ARE REFERENCING YOUR CHARACTER CONTROLLER AND YOU HAVE THIS METHOD ON IT!
                Activate();
        }

        // Send a signal to every SignalReceiver in the list.
        void Activate()
        {
            int count = signalReceivers.Count;
            for (int i = 0; i < count; i++)
                SendSignal(signalReceivers[i]);
        }

        // Track when the player enters the trigger.
        void OnTriggerEnter2D(Collider2D collision)
        {
            // MAKE SURE YOU ARE SEARCHING FOR YOUR CHARACTER CONTROLLER!
            if (!collision.GetComponent<PlayerInput>()) // not the player, ignore it.
                return;

            isPlayerPresent = true;                     // note that the player just entered.
            if (!playerInput)                           // only store the player once.
                playerInput = collision.GetComponent<PlayerInput>(); // AGAIN, YOUR CHARACTER CONTROLLER!

            // Reveal the UI widget.
        }

        // Track when the player leave the trigger.
        void OnTriggerExit2D(Collider2D collision)
        {
            // MAKE SURE YOU ARE SEARCHING FOR YOUR CHARACTER CONTROLLER!
            if (!collision.GetComponent<PlayerInput>()) // not the player, ignore it.
                return;

            isPlayerPresent = false;

            // Hide the UI widget.
        }
    }
}