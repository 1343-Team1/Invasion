using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickASprite : MonoBehaviour
{
    public Sprite[] sprites;
    private SpriteRenderer renderer;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        int index = Random.Range(0, sprites.Length - 1);
        renderer.sprite = sprites[index];
    }
}
