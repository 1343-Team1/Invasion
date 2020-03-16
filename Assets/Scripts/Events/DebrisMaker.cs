/// Author: Jeremy Anderson, March 15, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Creates debris as particles or collidable objects based on an intensity parameter.
    /// </summary>
    public class DebrisMaker : MonoBehaviour
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== PRIVATE / PROTECTED ==========
        
        // 0 to 1, determines spawn rate.
        [SerializeField] float intensity;

        // the particle burster for any particle system on this debris maker.
        [SerializeField] ParticleBurster particleBurster;

        // the next time that particles can burst.
        [SerializeField] float nextParticleBurstTime;

        // the next time that debris can spawn.
        [SerializeField] float nextDebrisSpawnTime;

        // ========== PUBLIC ==========
        [Header("Particle Settings")]
        // 0 to 1, visualizers to make plotting the curves easier.
        public AnimationCurve particleSpawnDelay;
        public AnimationCurve particleSpawnAmount;
        public AnimationCurve particleSpawnProbability;

        // the distance within which a random number is generated for particle delay.
        public float particleDelayMultiplier;
        public float particleDelayRangeRadius;

        [Header("Debris Settings")]
        // an array of possible debris to spawn.
        public GameObject[] DebrisPrefabs;

        // 0 to 1, visualizers to make plotting the curves easier.
        public AnimationCurve debrisSpawnDelay;
        public AnimationCurve debrisSpawnAmount;
        public AnimationCurve debrisSpawnProbability;

        // the distance within which a random number is generated for debris delay.
        public float debrisDelayMultiplier;
        public float debrisDelayRangeRadius;


        /********************
         * =- Functions -=
         ********************/

        // Get the particle burster.
        void Start()
        {
            particleBurster = GetComponent<ParticleBurster>();
            nextParticleBurstTime = GetNextTime(particleSpawnDelay, particleDelayRangeRadius, particleDelayMultiplier);
            nextDebrisSpawnTime = GetNextTime(debrisSpawnDelay, debrisDelayRangeRadius, debrisDelayMultiplier);
        }

        // Burst particles according to intensity.
        void Update()
        {
            BurstParticles();
        }

        // Create debris according to intensity.
        void FixedUpdate()
        {
            int count = DebrisPrefabs.Length;
            if (count < 1)                                  // there are no debris prefabs.
                return;                                     // early out.

            if (Time.time < nextParticleBurstTime) // too soon to spawn again.
                return;                                     // early out.

            // The spawn rate is the threshold for a random number to be above to spawn something.
            float spawnAmount = InterpolateOverCurve(debrisSpawnAmount, 0, 1, intensity);

            if (Random.Range(0, 1f) < InterpolateOverCurve(debrisSpawnProbability, 0, 1, intensity))             // nothing will be spawning.
                return;                                     // early out.

            // Instantiate debris.
        }

        // Get the random delay within the parameters.
        float GetNextTime(AnimationCurve delay, float radius, float multiplier)
        {
            float amount = InterpolateOverCurve(delay, 0, 1, intensity);
            return Time.time + (Random.Range(amount - radius, amount + radius) * multiplier);
        }

        // Burst particles if possible.
        void BurstParticles()
        {
            if (!particleBurster)                           // there is no particleBurster.
                return;                                     // early out.

            if (Time.time < nextParticleBurstTime)          // too soon to burst again.
                return;

            // The spawn rate is the threshold for a random number to be above to spawn something.
            float spawnAmount = InterpolateOverCurve(particleSpawnAmount, 0, 1, intensity);

            if (Random.Range(0, 1f) < InterpolateOverCurve(particleSpawnProbability, 0, 1, intensity)) // nothing will be spawning. 
                return;                                     // early out.

            // Burst the particles.
            particleBurster.BurstParticles(spawnAmount);
            nextParticleBurstTime = GetNextTime(particleSpawnDelay, particleDelayRangeRadius, particleDelayMultiplier);
        }

        // Update the intensity of this debris maker if it is within a certain distance of the origin point.
        public void InformIntensity(float intensity, Vector2 originPoint, bool proximityBased, float proximity)
        {
            // Too far away to update.
            if (proximityBased && Vector2.Distance(transform.position, originPoint) > proximity)
                return;                                     // early out.

            this.intensity = intensity;
        }
        
        // Author: Sarper-Soher & lordofduct, September 12, 2015, https://forum.unity.com/threads/logarithmic-interpolation.354344/
        // Interpolate speed based on location on the curve.
        float InterpolateOverCurve(AnimationCurve spawnRate, float start, float end, float intensity)
        {
            return start + spawnRate.Evaluate(intensity) * (end - start);
        }
    }
}
