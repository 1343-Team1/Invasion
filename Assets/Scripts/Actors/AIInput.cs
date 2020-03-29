﻿/// Author: Jeremy Anderson, March 19, 2020.

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
            // ---- Movement ----
            actorController.InformAxis(GetMovement());
            actorController.InformRun(brain.IsRunning());

            // ---- Jumping ----
            if (brain.IsJumping())
                actorController.InformJump(true);

            // ---- Firing ----
            if (brain.IsFiring())
                actorController.FireProjectile();
        }

        Vector2 GetMovement()
        {
            input = brain.GetMovement();
            adjustedInput = new Vector2(Mathf.Clamp(input.x, -speed, speed), Mathf.Clamp(input.y, -speed, speed));

            return adjustedInput;
        }
    }
}