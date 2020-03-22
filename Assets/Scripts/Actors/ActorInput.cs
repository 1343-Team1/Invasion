/// Author: Jeremy Anderson, March 10, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// A parent class, providing the foundation for PlayerInput and AI_Input.
    /// </summary>
    [RequireComponent(typeof(ActorController))]
    public abstract class ActorInput : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE / PROTECTED ==========
        GameManager GM;                                         // a reference to the GameManager.
        [SerializeField] protected ActorController actorController;              // the target that the input data will be sent to.
        protected float speed = 3;                              // the hidden base speed to calibrate the editor to 1.

        // ========== PUBLIC ==========
        //[Header("Common Settings")]

        // Exposed private/protected variables.
        [Header("Debug Data")]
        [SerializeField] [DisplayWithoutEdit()] protected Vector2 adjustedInput = new Vector2(0.0f, 0.0f); // the adjusted input coming in from the controller.
        [SerializeField] [DisplayWithoutEdit()] protected Vector2 input = new Vector2(0.0f, 0.0f); // the input coming from the controller or aibrain.

        /********************
         * =- Functions -=
         ********************/

        // All Input types need to know about the GameManager and their ActorController.
        protected virtual void Start()
        {
            GM = FindObjectOfType<GameManager>();
            actorController = GetComponent<ActorController>();
        }
    }
}
