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

        // ========== PRIVATE / PROTECTED ==========
        GameManager gameManager;
        Transform playerTransform;
        float cameraZ;

        // ========== PUBLIC ==========

        // Changes the camera mode between standard and elastic.
        [Header("Camera Settings")]
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

        [Header("Camera Shake")]
        public float shakeLowerLimit;                   // when shaking starts.
        public float shakeMinFrequency;                 // the min speed of the shake.
        public float shakeMaxFrequency;                 // the max speed of the shake.
        public float shakeBaseAmplitude;                // the base amount of shaking at the lower limit.
        public float shakeMinAmplitude;                 // the multiplier applied to magnify intensity for the minimuma amount.
        public float shakeMaxAmplitude;                 // the multiplier applied to magnify intensity for the maximum amount.

        /********************
         * =- Functions -=
         ********************/

        // Get the player and store the camera's z.
        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
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

        // Determine whether the camera should be shaking.
        bool IsShaking() { return (gameManager.intensity >= shakeLowerLimit) ? true : false; }

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
            Vector3 position = new Vector3(playerTransform.position.x, playerTransform.position.y, cameraZ);

            transform.position = (IsShaking()) ? Shake(position) :
                new Vector3(position.x, position.y, cameraZ);
        }

        // Follow the target with elasticity.
        void FollowElastically()
        {
            Vector2 targetPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
            float distance = Vector2.Distance(transform.position, targetPosition);
            float effectiveSpeed = (InterpolateOverCurve(0, 1, Mathf.Clamp(distance, 0, 1)) * followSpeed);

            Vector2 position = Vector2.MoveTowards(transform.position, targetPosition, effectiveSpeed * Time.deltaTime);

            transform.position = (IsShaking()) ? Shake(position) :
                new Vector3(position.x, position.y, cameraZ);
        }

        Vector3 Shake(Vector2 position)
        {
            float xAmplitude = Random.Range(
                shakeBaseAmplitude * shakeMinAmplitude * gameManager.intensity,
                shakeBaseAmplitude * shakeMaxAmplitude * gameManager.intensity
                );

            float yAmplitude = Random.Range(
                shakeBaseAmplitude * shakeMinAmplitude * gameManager.intensity,
                shakeBaseAmplitude * shakeMaxAmplitude * gameManager.intensity
                );

            float x = xAmplitude * Mathf.Sin(Time.time * Random.Range(shakeMinAmplitude, shakeMaxAmplitude) * Mathf.PI);
            float y = yAmplitude * Mathf.Sin(Time.time * Random.Range(shakeMinAmplitude, shakeMaxAmplitude) * Mathf.PI);

            return new Vector3(position.x + x, position.y + y, cameraZ);
        }

        // Author: Sarper-Soher & lordofduct, September 12, 2015, https://forum.unity.com/threads/logarithmic-interpolation.354344/
        // Interpolate speed based on location on the curve.
        float InterpolateOverCurve(float start, float end, float distance)
        {
            return start + elasticity.Evaluate(distance) * (end - start);
        }
    }
}