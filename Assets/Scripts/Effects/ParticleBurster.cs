using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBurster : MonoBehaviour
{
    private new ParticleSystem particleSystem; // hiding inherited member is intended.
    public int particlePerSecond = 1;

    public bool leaveSprites;
    public GameObject[] prefabs;
    public float maxDistance;
    public float maxRotation;

    private bool isStreaming;
    private float timeStreamStarted;
    private float duration;
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
        particleSystem.Emit(particlePerSecond);
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
