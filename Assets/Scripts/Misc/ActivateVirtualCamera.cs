using Cinemachine;
using UnityEngine;

public class ActivateVirtualCamera : MonoBehaviour {

    GameObject vcam;
    public CycledMovement[] cyclers;

    private void Awake()
    {
        vcam = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        var _vcam = vcam.GetComponent<CinemachineVirtualCamera>();
        if (_vcam.Follow == null)
            _vcam.Follow = FindObjectOfType<PlayerController>().transform;
        cyclers = GetComponentsInChildren<CycledMovement>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameController.Instance.SwitchCamera(vcam, true);
            if (cyclers.Length != 0)
            {
                foreach (var cycler in cyclers)
                {
                    cycler.isMoving = true;
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!vcam.activeSelf)
            {
                GameController.Instance.SwitchCamera(vcam, false);
            }
            foreach (var cycler in cyclers)
            {
                if (!cycler.isMoving && GameController.Instance.player._currentState != PlayerState.Dead)
                    cycler.isMoving = true;
            }
        }
    }


}
