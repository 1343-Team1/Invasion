using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    [RequireComponent(typeof(ActorController))]
    /// <summary>
    /// Takes input from a keyboard and mouse or gamepad, allows configuration,
    /// and sends that data to the ActorController.
    /// </summary>
    public class PlayerInput : ActorInput
    {
        /********************
         * =- Variables -=
         ********************/

        // Exposed debug data being added to the parent class debug data.
        [SerializeField][DisplayWithoutEdit()] Vector2 input = new Vector2(0.0f, 0.0f); // the raw input coming in from the controller.

        // Private/protected variables.
        bool isUsingController = false;                         // toggled if the game detects a controller.
        bool controllerHasDeadzone = false;                     // allows the player to specify a deadzone if they want.

        // Public variables.
        [Header("Button Configuration")]
        public KeyCode runKey = KeyCode.LeftShift;                      // key to run.
        public bool isRunToggle = true;                                 // run is toggleable.

        [Header("Controller Settings")]
        public Vector2 deadzone = new Vector2(0f, 0f);                  // thumbstick deadzone if a controller is being used.
        public Vector2 thumbstickSensitivity = new Vector2(0.5f, 0.5f); // thumbstick sensitivity if a controller is being used.


        /********************
         * =- Functions -=
         ********************/

        // Send instant input to the ActorController as fast as possible it will handle coordinating animation with physics.
        void Update()
        {
            actorController.InformAxis(GetMovement());
            actorController.InformRun(IsRunning(), isRunToggle);

            if (IsJumping())
                actorController.InformJump(true);
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
            input = new Vector2(Input.GetAxisRaw("Horizontal"), 0.0f);

            adjustedInput = GetAdjustedInput();

            return adjustedInput;
        }

        // Calculate the adjusted speed.
        Vector2 GetAdjustedInput()
        {
            Vector2 adjustedInput = new Vector2(input.x * speed, 0.0f);
            if (actorController.moveByForce)
                adjustedInput *= actorController.accelerationRate;

            if (isUsingController)
                adjustedInput *= thumbstickSensitivity;

            return adjustedInput;
        }
    }
}
