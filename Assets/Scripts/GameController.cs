using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController Instance;

    public GameObject currentCamera;
    public GameObject previousCamera;
    public bool switchCameraRequest;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        previousCamera = currentCamera;

    }

    public void SwitchCamera(GameObject newCamera)
    {
        print(newCamera);
        previousCamera = currentCamera;
        currentCamera = newCamera;
        if (currentCamera != null && previousCamera != null && previousCamera != currentCamera)
        {
            currentCamera.SetActive(true);
            previousCamera.SetActive(false);
        }
        else if(previousCamera == currentCamera)
        {
            currentCamera.SetActive(true);
        }
    }
    }
