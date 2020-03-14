/// Author: Jeremy Anderson, March 13, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Maintain an elastic camera that shakes based on event triggers.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // Private/protected variables.
        Transform playerTransform;
        float cameraZ;

        // Public variables.

        /********************
         * =- Functions -=
         ********************/

        // Get the player and store the camera's z.
        void Start()
        {
            playerTransform = FindObjectOfType<PlayerInput>().transform;
            cameraZ = transform.position.z;
        }

        // Follow the player at a specified rate that increases based on distance from the player.
        void Update()
        {
            transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, cameraZ);
        }
    }
}