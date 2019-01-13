using Cinemachine;
using UnityEngine;

public class ActivateVirtualCamera : MonoBehaviour {

    GameObject vcam;

    private void Awake()
    {
        vcam = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        var _vcam = vcam.GetComponent<CinemachineVirtualCamera>();
        if (_vcam.Follow == null)
            _vcam.Follow = FindObjectOfType<PlayerController>().transform;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameController.Instance.SwitchCamera(vcam);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(!vcam.activeSelf)
        {
            GameController.Instance.SwitchCamera(vcam);
        }
    }
}
