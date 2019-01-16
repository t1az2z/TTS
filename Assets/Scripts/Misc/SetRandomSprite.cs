using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRandomSprite : MonoBehaviour {

    SpriteRenderer sr;
    [SerializeField] Sprite[] sprites;

	// Use this for initialization
	void Start () {
        sr = GetComponent<SpriteRenderer>();
        if (sprites.Length != 0)
        {
            int rnd = Random.Range(0, sprites.Length);
            sr.sprite = sprites[rnd];
        }

	}
}
