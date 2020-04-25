/// Author: Jeremy Anderson, March 10, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Takes input from a keyboard and mouse or gamepad, allows configuration,
    /// and sends that data to the ActorController.
    /// </summary>
    [RequireComponent(typeof(ActorController))]
    public class PlayerInput : ActorInput
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE / PROTECTED ==========
        bool isUsingController = false;                         // toggled if the game detects a controller.
        bool controllerHasDeadzone = false;                     // allows the player to specify a deadzone if they want.

        // ========== PUBLIC ==========
        [Header("Button Configuration")]
        public KeyCode runKey = KeyCode.LeftShift;                      // key to run.
        public bool isRunToggle = true;                                 // run is toggleable.
        public KeyCode fireKey = KeyCode.Mouse0;                        // key to fire.
        public bool isFireToggle = false;                               // semi or full auto.
        public KeyCode actionKey = KeyCode.F;                           // key to perform a context sensitive 'action'.

        // Not set up yet.
        [Header("Controller Settings")]
        public Vector2 deadzone = new Vector2(0f, 0f);                  // thumbstick deadzone if a controller is being used.
        public Vector2 thumbstickSensitivity = new Vector2(0.5f, 0.5f); // thumbstick sensitivity if a controller is being used.


        /********************
         * =- Functions -=
         ********************/

        // Send instant input to the ActorController as fast as possible it will handle coordinating animation with physics.
        void Update()
        {
            // Not time to play yet.
            if (GameManager.GetGameState() != GameManager.GameState.Playing)
                return;

            if (transform.position.y < -50)
                actorController.Kill();

            // ---- Movement ----
            actorController.Move(GetMovement());
            actorController.Run(IsRunning(), isRunToggle);

            // ---- Jumping ----
            actorController.Jump(IsJumping());

            // ---- Firing ----
            actorController.Shoot(IsShooting());
        }

        // Is the player pressing the action key?
        public bool IsPressingActionKey()
        {
            if (Input.GetKeyDown(actionKey))
                return true;

            return false;
        }

        // Is the player firing?
        bool IsShooting()
        {
            if (isFireToggle && Input.GetKeyDown(fireKey))
                return true;

            if (!isFireToggle && Input.GetKey(fireKey))
                return true;

            return false;
        }

        // Is the player running?
        bool IsRunning()
        {
            if (isRunToggle && Input.GetKeyDown(runKey))    // only trigger on the frame key is down for toggle.
                return true;

            if (!isRunToggle && Input.GetKey(runKey))       // maintain accurate data stream if not toggle.
                return true;

            return false;
        }

        // Is the player inputting a jump command?
        bool IsJumping()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        // Calculate input from either gamepad or keyboard.
        Vector2 GetMovement()
        {
            input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            adjustedInput = GetAdjustedInput();

            return adjustedInput;
        }

        // Calculate the adjusted speed.
        Vector2 GetAdjustedInput()
        {
            Vector2 adjustedInput = new Vector2(input.x * speed, input.y);
            if (actorController.moveByForce)
                adjustedInput *= actorController.accelerationRate;

            if (isUsingController)
                adjustedInput *= thumbstickSensitivity;

            return adjustedInput;
        }
    }
}
