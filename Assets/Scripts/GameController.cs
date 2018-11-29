using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class GameController : MonoBehaviour {

    //gc
    public static GameController Instance;
    public UIController ui;

    //cameras
    public GameObject currentCamera;
    private GameObject previousCamera;


    //player references
    [HideInInspector] public PlayerController player;
    Animator player_animator;
    AnimatorClipInfo[] p_CurrentClipInfo;
    float deathReviveAnimationLength;

    //todo перенести логику обработки переменных полностью в GameController
    [HideInInspector]
    public int deaths = 0;
    [HideInInspector]
    public int collectiblesCollected = 0;

    Vector2 activeCheckpoint;

    [Header("Cam shake parameters")]
    public float frequency = .3f;
    public float amplitude = .1f;
    public float duration = .2f;


    void Awake()
    {
        SingletonImplementation();
    }

    private void SingletonImplementation()
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
    }

    private void Start()
    {
        ui = FindObjectOfType<UIController>();

        SetPlayerReference();


        previousCamera = currentCamera;
    }



    public void CollectiblesUpdate()
    {
        collectiblesCollected++;
        ui.CollectiblesTextUpdate();
    }



    private void SetPlayerReference()
    {
        player = FindObjectOfType<PlayerController>();
        player_animator = player.animator;
    }
    private void Update()
    {
        if (ui == null)
            ui = FindObjectOfType<UIController>();
        if(Debug.isDebugBuild)
            DebugKeys();

        if (player == null)
            SetPlayerReference();
    }

    private void DebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            
            StartCoroutine("DeathCoroutine");
            player.isDead = true;
        }

    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SetChekpoint(Vector2.zero);
        deaths = 0;
        collectiblesCollected = 0;
    }

    public void SwitchCamera(GameObject newCamera)
    {

        float timeToStop = .5f;
        previousCamera = currentCamera;
        currentCamera = newCamera;

        
        if (currentCamera != null && previousCamera != null && previousCamera != currentCamera)
        {
            currentCamera.SetActive(true);
            previousCamera.SetActive(false);
            StartCoroutine(player.FreezePlayer(timeToStop));

        }
        else if(previousCamera == currentCamera)
        {
            currentCamera.SetActive(true);
        }

    }

    public void SetChekpoint(Vector2 checkpoint)
    {
        if (checkpoint == Vector2.zero)
            activeCheckpoint = Vector2.zero;
        else
            activeCheckpoint = new Vector2(checkpoint.x, checkpoint.y-1f);
    }


    public IEnumerator DeathCoroutine()
    {
        player.currentState = PlayerState.Dead;
        deaths++;
        ui.DeathsTextUpdate();
        player_animator.Play("Death");
        player.deathParticles.Play();
        if (deathReviveAnimationLength == 0)
        {
            CountDeathReviveAnimationLength();
        }
        yield return new WaitForSeconds(deathReviveAnimationLength);
        //player.isDead = true;
        ui.SplashShow();


        yield return new WaitForSeconds(ui.splashAnimationLength);
        MovePlayerToCheckpoint();
        ui.SplashHide();
        yield return new WaitForSeconds(ui.splashAnimationLength);
        player.animator.Play("Revive"); //todo get it out of here
        yield return new WaitForSeconds(deathReviveAnimationLength);
        //player.isDead = false;
        //player.controllsEnabled = true;
        player.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.jumpRequest = false;
        player.dashRequest = false;
        player.currentState = PlayerState.Fall;
    }

    private void CountDeathReviveAnimationLength()
    {
        p_CurrentClipInfo = player_animator.GetCurrentAnimatorClipInfo(0);
        deathReviveAnimationLength = p_CurrentClipInfo[0].clip.length;
    }


    private void MovePlayerToCheckpoint()
    {
        if (activeCheckpoint != Vector2.zero)
        {
            player.gameObject.transform.position = activeCheckpoint;
        }
        else
            player.gameObject.transform.position = Vector3.zero;
    }

    
    

    public IEnumerator FreezeTime(float stopTime)
    {
        Time.timeScale = 0f;
        float stopEndTime = Time.realtimeSinceStartup + stopTime;
        while (Time.realtimeSinceStartup < stopEndTime)
        {
            yield return 0;
        }
        Time.timeScale = 1f;
    }
    //todo add stopframes on transition
}
