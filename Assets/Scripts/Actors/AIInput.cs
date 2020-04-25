/// Author: Jeremy Anderson, March 19, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Takes input from an AIBrain and sends it to the ActorController.
    /// </summary>
    public class AIInput : ActorInput
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE / PROTECTED ==========
        [SerializeField] AIBrain brain;


        // ========== PUBLIC ==========



        /********************
         * =- Functions -=
         ********************/

        // Get the AIBrain.
        protected override void Start()
        {
            base.Start();
            brain = GetComponent<AIBrain>();
        }

        // Update is called once per frame
        void Update()
        {
            // Not time to play yet.
            if (GameManager.GetGameState() != GameManager.GameState.Playing)
                return;

            // ---- Firing ----
            if (brain.IsFiring(actorController.FacingRight))
                actorController.Shoot(true);

            // ---- Movement ----
            actorController.Move(GetMovement());
            actorController.Run(brain.IsRunning());

            // ---- Jumping ----
            if (brain.IsJumping())
                actorController.Jump(true);
        }

        Vector2 GetMovement()
        {
            input = brain.GetMovement(actorController.FacingRight);
            adjustedInput = new Vector2(Mathf.Clamp(input.x, -speed, speed), Mathf.Clamp(input.y, -speed, speed));

            return adjustedInput;
        }
    }
}
