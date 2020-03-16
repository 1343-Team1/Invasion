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

        // ========== PUBLIC ==========
        [Header("Camera Settings")]
        // Changes the camera mode between standard and elastic.
        public bool isElastic;

        // Base speed of the camera.
        public float followSpeed;

        // 0 to 1, a multiplier to Follow Speed, applied based on distance from target.
        public AnimationCurve elasticity;

        // Imposes limits on the camera's panning.
        public bool isLimited;

        // How far left the camera can pan.
        public float leftPanLimit;

        // How far right the camera can pan.
        public float rightPanLimit;

        // How far up the camera can pan.
        public float upPanLimit;

        // How far down the camera can pan.
        public float downPanLimit;

        /********************
         * =- Functions -=
         ********************/

        // Get the player and store the camera's z.
        void Start()
        {
            playerTransform = FindObjectOfType<PlayerInput>().transform;
            cameraZ = transform.position.z;
        }

        // Follow the player with elasticity, and within the pan limits.
        void Update()
        {
            if (isElastic)
                FollowElastically();
            else
                FollowNormally();

            if (isLimited)
                LimitCamera();
        }

        // Force the camera to remain inside of the pan limits.
        void LimitCamera()
        {
            float x = transform.position.x;
            x = ((x < leftPanLimit) ? leftPanLimit : x);
            x = ((x > rightPanLimit) ? rightPanLimit : x);
            float y = transform.position.y;
            y = ((y < downPanLimit) ? downPanLimit : y);
            y = ((y > upPanLimit) ? upPanLimit : y);

            transform.position = new Vector3(x, y, cameraZ);
        }
        
        // Follow the target perfectly.
        void FollowNormally()
        {
            transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, cameraZ);
        }

        // Follow the target with elasticity.
        void FollowElastically()
        {
            Vector2 targetPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
            float distance = Vector2.Distance(transform.position, targetPosition);
            float effectiveSpeed = (InterpolateOverCurve(0, 1, Mathf.Clamp(distance, 0, 1)) * followSpeed);

            Vector2 newPosition = Vector2.MoveTowards(transform.position, targetPosition, effectiveSpeed * Time.deltaTime);

            transform.position = new Vector3(newPosition.x, newPosition.y, cameraZ);
        }

        // Author: Sarper-Soher & lordofduct, September 12, 2015, https://forum.unity.com/threads/logarithmic-interpolation.354344/
        // Interpolate speed based on location on the curve.
        float InterpolateOverCurve(float start, float end, float distance)
        {
            return start + elasticity.Evaluate(distance) * (end - start);
        }
    }
}