using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameController : MonoBehaviour {

    //gc
    public static GameController Instance;
    //cameras
    public GameObject currentCamera;
    private GameObject previousCamera;
    //player references
    PlayerController player;
    
    Vector3 activeCheckpoint;

    //UI references
    GameObject splashScreen;
    public float deathCoroutineDelay = 1f;

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

        player = FindObjectOfType<PlayerController>();
        splashScreen = GameObject.Find("Splash");
        splashScreen.SetActive(false);
    }

    private void Update()
    {
        if(Debug.isDebugBuild)
            DebugKeys();
    }

    private static void DebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void SwitchCamera(GameObject newCamera)
    {
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

    public void SetChekpoint(Vector2 checkpoint)
    {
        activeCheckpoint = checkpoint;
    }


    public IEnumerator DeathCoroutine()
    {
        player.controllsEnabled = false;
        splashScreen.SetActive(true);
        Animator splashAnimator = splashScreen.GetComponent<Animator>();
        splashAnimator.Play("SplashShow");
        yield return new WaitForSeconds(deathCoroutineDelay / 2);
        if (activeCheckpoint != null)
            player.gameObject.transform.position = activeCheckpoint;
        else
            player.gameObject.transform.position = Vector3.zero;
        splashAnimator.Play("SplashHide");
        yield return new WaitForSeconds(deathCoroutineDelay / 2);
        splashScreen.SetActive(false);
        player.controllsEnabled = true;
    }

    //todo add stopframes on transition
    }
