/// Author: Jeremy Anderson, March 31, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// A shield that can protect a character.
    /// </summary>
    public class Shield : MonoBehaviour
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== PRIVATE / PROTECTED ==========
        [SerializeField] int currentShield;
        float currentRegen = 0f;

        // ========== PUBLIC ==========
        [Header("Shield Settings")]
        public int maxShield;
        public float regenRate;
        public bool pickUpsBoost = false;

        
        /********************
         * =- Functions -=
         ********************/

        // Initialize the shield to max
        void Start()
        {
            currentShield = maxShield;
        }

        // Regenerate the shield if it regenerates.
        void Update()
        {
            Regenerate();
        }

        void Regenerate()
        {
            if (currentShield >= maxShield)                 // shield is already maxxed.
                return;

            currentRegen += regenRate;                      // regenerate.

            if (currentRegen < 1)                           // not enough to notice yet.
                return;

            // A full point has been regenerated.
            currentShield++;
            currentRegen--;
        }

        public void TakeDamage(int damage)
        {
            currentShield = Mathf.Clamp(currentShield - damage, 0, currentShield);
        }

        // Whether the shield is currently active.
        public bool IsActive() { return (currentShield > 0) ? true : false; }

        // This will take a pickup later.
        public void Boost()
        {

        }
    }
}