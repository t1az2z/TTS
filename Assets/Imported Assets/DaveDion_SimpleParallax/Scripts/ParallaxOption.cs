using UnityEngine;
using System.Collections;

public class ParallaxOption : MonoBehaviour {

	public bool moveParallax;

	[SerializeField]
	[HideInInspector]
	private Vector3 storedPosition;
    public float smoothing = 1f;
	public void SavePosition() {
		storedPosition = transform.position;
	}

	public void RestorePosition() {
		transform.position = storedPosition;
	}
}