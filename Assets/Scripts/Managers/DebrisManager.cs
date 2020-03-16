/// Author: Jeremy Anderson, March 15, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Manages the intensity of the debris makers.
    /// </summary>
    public class DebrisManager : MonoBehaviour
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== PRIVATE / PROTECTED ==========
        GameManager gameManager;                            // a reference to the game manager.
        DebrisMaker[] debrisMakers;                         // an array of all debris makers.

        // ========== PUBLIC ==========
        [Header("Debris Settings")]
        public bool proximityBased;
        public float proximity;


        /********************
         * =- Functions -=
         ********************/

        // Get all of the debris makers.
        void Start()
        {
            gameManager = GetComponent<GameManager>();
            debrisMakers = FindObjectsOfType<DebrisMaker>();
        }

        // Update is called once per frame
        void Update()
        {
            InformDebrisMakers();
        }

        // Update all of the debris makers with the current intensity.
        void InformDebrisMakers()
        {
            int count = debrisMakers.Length;
            for (int i = 0; i < count; i++) {
                debrisMakers[i].InformIntensity(gameManager.intensity, transform.position, proximityBased, proximity);
            }
        }
    }
}
