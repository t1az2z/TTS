using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour {
	public float speedX;
	public float speedY;
	public bool moveInOppositeDirection;

	private Transform cameraTransform;
	private Vector3 previousCameraPosition;
	private bool previousMoveParallax;
	private ParallaxOption options;
    Vector3 distance;
    float direction;
    float movementX;
    float movementY;
    Vector3 transformNextPos;

    void OnEnable() {
		GameObject gameCamera = FindObjectOfType<ParallaxOption>().gameObject;
		options = gameCamera.GetComponent<ParallaxOption>();
		cameraTransform = gameCamera.transform;
		previousCameraPosition = cameraTransform.position;
	}

	void LateUpdate () {
		if(options.moveParallax && !previousMoveParallax)
			previousCameraPosition = cameraTransform.position;

		previousMoveParallax = options.moveParallax;

		if(!Application.isPlaying && !options.moveParallax)
			return;

		distance = cameraTransform.position - previousCameraPosition;
		direction = (moveInOppositeDirection) ? -1f : 1f;

        movementX = distance.x * speedX*direction;
        movementY = distance.y * speedY*direction;

        var nextPos = transform.position + new Vector3(movementX, movementY, transform.position.z);
        transformNextPos = Vector3.Lerp(transform.position, nextPos, options.smoothing*Time.deltaTime) ;

        transform.position = transformNextPos;
        previousCameraPosition = cameraTransform.position;
    }

}
