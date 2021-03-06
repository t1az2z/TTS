﻿using UnityEngine.UI;
using UnityEngine;

public class ShowDashButton : MonoBehaviour {
    public GameObject button;

    private void Start()
    {
        if (button == null)
        {
            Debug.Log("Dash button is missing");
            button = FindObjectOfType<UIController>().transform.Find("Dash").gameObject;
        }

        if (!Debug.isDebugBuild)
        {
            button.SetActive(false);
        }
    }

#if !UNITY_WEBGL
    private void OnTriggerEnter2D(Collider2D collision)
    {

        button.SetActive(true);
        gameObject.SetActive(false);
        GameController.Instance.player.dashEnabled = true;
    }
#endif

#if UNITY_WEBGL
    private void OnTriggerEnter2D(Collider2D collision)
    {
        gameObject.SetActive(false);
        GameController.Instance.player.dashEnabled = true;
    }
#endif
}
