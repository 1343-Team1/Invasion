using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flickerer : MonoBehaviour
{
    private Light pointLight;

    public float range;
    public float frequency;
    public List<Color> colors;

    private Vector3 defaultPosition;
    private float timeLastFlickered;

    void Start()
    {
        pointLight = GetComponent<Light>();
        colors.Add(pointLight.color);
        defaultPosition = transform.position;
        timeLastFlickered = Time.time;
    }

    void Update()
    {
        defaultPosition = new Vector3(transform.parent.position.x, transform.parent.position.y, transform.parent.position.z);
        if (range == 0 && frequency == 0) return;

        if (Time.time - timeLastFlickered > frequency) Flicker();
    }

    private void Flicker()
    {
        SetNewPosition();
        SetNewColor();
        timeLastFlickered = Time.time;
    }

    private void SetNewColor()
    {
        if (colors.Count == 1) return;

        int index = Random.Range(0, colors.Count);
        pointLight.color = colors[index];
    }

    private void SetNewPosition()
    {
        float x = Random.Range(defaultPosition.x - range, defaultPosition.x + range);
        float y = Random.Range(defaultPosition.y - range, defaultPosition.y + range);

        transform.position = new Vector3(x, y, defaultPosition.z);
    }
}
