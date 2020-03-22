/// Author: Jeremy Anderson, March 10, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Increases or decreases the intensity variable of the GameManager when the player
    /// triggers the collider2d on this gameobject. The logic for modifying the intensity is all here,
    /// none is on the player or any other gameobject.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class IntensityTrigger : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE ==========
        GameManager gameManager;                        // a reference to the game manager.
        float nextTimeToModify;                         // when to increase/decrease intensity next.
        float timeToStop;                               // when to stop increasing/decreasing intensity next.
        bool wasActivated = false;                      // tracks whether this has already triggered once.
        bool isPlayerPresent = false;                   // whether the player is currently occupying it.
        bool isActive = false;                          // whether it is currently active.

        // ========== PUBLIC ==========
        [Header("Core Settings")]
        public bool triggersOnce = true;                // only triggers once, then destroys itself.
        public float amountToModify = 0.01f;            // amount to increase/decrease intensity.
        public float atInterval = 0.0f;                 // how frequently to increase/decrease intensity.

        [Header("Collision Settings")]
        public bool triggersOnlyWhilePlayerIn = false;  // only triggers while the player is in it.
        public bool destroyAfterPlayerLeaves = false;   // destroys itself after the player leaves (only needed if triggersWhileIn is true).

        [Header("Duration Settings")]
        public bool lastsForDuration = false;           // whether this effect lasts a duration.
        public bool durationFromExit = false;           // whether the duration starts counting when the player leaves the trigger.
        public bool destroyAfterDuration = false;       // whether to destroy after a duration.
        public float forDuration = 0.0f;                // how long to increase/decrease intensity.


        /********************
         * =- Functions -=
         ********************/

        // Get the game manager and prepare the trigger to fire the first time.
        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            nextTimeToModify = Time.time;
        }

        // Something entered, was it the player?
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<PlayerInput>())
            {
                isPlayerPresent = true;
                isActive = true;
                wasActivated = true;
                if (lastsForDuration)                   // lasts for a duration.
                    timeToStop = Time.time + forDuration;
            }
        }

        // If the player left, maybe it needs to be destroyed.
        void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.GetComponent<PlayerInput>()) // it wasn't the player, ignore it.
                return;

            if (triggersOnce || destroyAfterPlayerLeaves) // easy checks to destroy.
                Destroy(gameObject);

            if (lastsForDuration && durationFromExit)   // lasts for a duration calculated from exit.
                timeToStop = Time.time + forDuration;

            isPlayerPresent = false;

            if (lastsForDuration && Time.time >= timeToStop) // lasts for a duration and it's still active.
                isActive = false;
        }

        // Manage whether to modify the intensity or not.
        void Update()
        {
            if (!wasActivated)                          // the player has never triggered it.
                return;

            if (!isActive)                              // it's not active right now, ignore it.
                return;

            if (Time.time < nextTimeToModify)           // it's been triggered, but too early.
                return;

            if (!isPlayerPresent && triggersOnlyWhilePlayerIn) // it's been triggered, but the player is no longer in it and needs to be.
                return;

            // It's either active, or been activated and doesn't require the player to be in it.
            gameManager.intensity = Mathf.Clamp(gameManager.intensity + amountToModify, 0, 1);

            if (triggersOnce)                           // this only triggers once.
                Destroy(gameObject);

            // It either perpetually triggers, or triggers while the player is in it, prep for the next time to test.
            nextTimeToModify = Time.time + atInterval;

            if (!lastsForDuration)                      // doesn't last for a duration, move on.
                return;

            if (Time.time < timeToStop)                 // it lasts for a duration and it's not time to stop yet.
                return;

            if (destroyAfterDuration)                   // it lasts for a duration, the time is up, and it needs to be destroyed.
                Destroy(gameObject);

            isActive = false;                           // duration expired, but not destroyed.
        }
    }
}