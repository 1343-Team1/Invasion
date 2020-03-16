/// Author: Jeremy Anderson, November, 2019.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Burst particles from a particle system.
    /// </summary>
    public class ParticleBurster : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE / PROTECTED ==========
        new ParticleSystem particleSystem;

        // ========== PUBLIC ==========
        [Header("Particle System Settings")]
        public int baseParticles = 20;

        [Header("Particle Streaming Settings")]
        private bool isStreaming;
        private float timeStreamStarted;
        private float duration;

        [Header("Sprite Instantiation Settings")]
        public bool leaveSprites;
        public GameObject[] prefabs;
        public float maxDistance;
        public float maxRotation;

        /********************
         * =- Functions -=
         ********************/

        void Start()
        {
            // You can use particleSystem instead of
            // gameObject.particleSystem.
            // They are the same, if I may say so
            particleSystem = GetComponent<ParticleSystem>();
            particleSystem.Stop();
        }

        void Update()
        {
            if (!isStreaming) return;

            BurstParticles();
            if (Time.time - timeStreamStarted > duration)
            {
                isStreaming = false;
            }
        }

        public void BurstParticles()
        {
            particleSystem.Emit(baseParticles);
            if (!leaveSprites) return;

            int index = Random.Range(0, prefabs.Length - 1);
            float x = Random.Range(-maxDistance, maxDistance);
            float y = Random.Range(-maxDistance, maxDistance);
            Quaternion rot = Quaternion.Euler(new Vector3(x, y, Random.Range(-maxRotation, maxRotation)));
            Vector3 pos = new Vector3(transform.position.x + x, transform.position.y + y, 0);
            Instantiate(prefabs[index], pos, rot);
        }

        public void BurstParticles(float intensity)
        {
            particleSystem.Emit((int)(baseParticles * intensity));
            if (!leaveSprites) return;

            int index = Random.Range(0, prefabs.Length - 1);
            float x = Random.Range(-maxDistance, maxDistance);
            float y = Random.Range(-maxDistance, maxDistance);
            Quaternion rot = Quaternion.Euler(new Vector3(x, y, Random.Range(-maxRotation, maxRotation)));
            Vector3 pos = new Vector3(transform.position.x + x, transform.position.y + y, 0);
            Instantiate(prefabs[index], pos, rot);
        }

        public void BurstLongStream(float duration)
        {
            isStreaming = true;
            this.duration = duration;
            timeStreamStarted = Time.time;
        }
    }
}
