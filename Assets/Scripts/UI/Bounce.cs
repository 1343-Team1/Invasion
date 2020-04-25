/// Author: Jeremy Anderson, April 12, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Bounce the game object up and down.
    /// </summary>
    public class Bounce : MonoBehaviour
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== PRIVATE ==========
        float target;
        float bottom;

        // ========== PUBLIC ==========
        [Header("Settings")]
        public float top;
        public float bounceSpeed;


        /********************
         * =- Functions -=
         ********************/

        // Initialize everything.
        void Start()
        {
            target = top;
            bottom = transform.localPosition.y;
        }

        // Update is called once per frame
        void Update()
        {
            // Not time to play yet.
            if (GameManager.GetGameState() != GameManager.GameState.Playing)
                return;

            if (transform.localPosition.y > top)
                target = bottom;
            else if (transform.localPosition.y < bottom)
                target = top;

            float y = (target == top) ? 
                transform.localPosition.y + bounceSpeed * Time.deltaTime : 
                transform.localPosition.y - bounceSpeed * Time.deltaTime;

            transform.localPosition = new Vector2(transform.localPosition.x, y);
        }
    }
}