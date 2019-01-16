using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour {

    [SerializeField] float delay = 1f;
	void Start () 
	{
		Destroy(this.gameObject, delay);
	}
}
