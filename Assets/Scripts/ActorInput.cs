using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// A parent class, providing the foundation for PlayerInput and AI_Input.
    /// </summary>
    public abstract class ActorInput : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // Private/protected variables.
        GameManager GM;                                         // a reference to the GameManager.
        protected ActorController actorController;              // the target that the input data will be sent to.
        protected float speed = 3;                              // the base speed for editor multipliers.

        // Public variables.
        [Header("Common Settings")]
        public float horizontalSensitivity;                     // allows precise control of speed in the editor.

        // Exposed private/protected variables.
        [Header("Debug Data")]
        [SerializeField][DisplayWithoutEdit()] protected Vector2 adjustedInput = new Vector2(0.0f, 0.0f); // the adjusted input coming in from the controller.

        /********************
         * =- Functions -=
         ********************/

        // All Input types need to know about the GameManager and their ActorController.
        void Start()
        {
            GM = FindObjectOfType<GameManager>();
            actorController = GetComponent<ActorController>();
        }

    }
}
