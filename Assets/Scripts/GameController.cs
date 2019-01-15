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
    public float timeToStopForScreenTransition = .7f;
    bool timeStoped = false;
    public float transitionMoveDistance = 2f;

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
    public int targetFps = 60;

    //reseters
    ReseterBehaviour[] resetersArray;
    
    void Awake()
    {
        SingletonImplementation();
        Application.targetFrameRate = targetFps;

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

        resetersArray = FindObjectsOfType<ReseterBehaviour>();

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

        if (Debug.isDebugBuild)
        {
            player.dashEnabled = true;
        }
    }
    private void Update()
    {
        if (ui == null)
            ui = FindObjectOfType<UIController>();
        if(Debug.isDebugBuild)
            DebugKeys();

        if (player == null)
            SetPlayerReference();
        if (resetersArray == null)
            resetersArray = FindObjectsOfType<ReseterBehaviour>();
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

    public void RestartLevel()
    {
        resetersArray = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SetChekpoint(Vector2.zero);
        deaths = 0;
        collectiblesCollected = 0;
    }

    public void SwitchCamera(GameObject newCamera, bool stopTime)
    {

        previousCamera = currentCamera;
        currentCamera = newCamera;

        
        if (currentCamera != null && previousCamera != null && previousCamera != currentCamera)
        {
            currentCamera.SetActive(true);
            previousCamera.SetActive(false);
            if (!timeStoped && stopTime)
            {
                timeStoped = true;
                MoveAtCameraTransition(previousCamera.transform.parent.position, currentCamera.transform.parent.position, player.transform, timeToStopForScreenTransition);
            }
        }
        else if(previousCamera == currentCamera)
        {
            currentCamera.SetActive(true);
        }

    }

    public void MoveAtCameraTransition(Vector2 prevScreen, Vector2 nextScreen, Transform objToMove, float transitionTime, float timer = 0)
    {
        /*var currentPos = objToMove.position;
        var t = 0f;
        while (t<1)
        {
            t += Time.unscaledDeltaTime / transitionTime;
            Vector3 destination = (nextScreen - prevScreen).normalized;
            objToMove.transform.position = Vector3.Lerp(currentPos, destination*transitionMoveDistance, t); //возможет баг с суммой векторов
            yield return null;
        }*/
        Vector2 direction = (nextScreen - prevScreen).normalized;
        if (direction.y < .1f)
            StartCoroutine(FreezeTime(timeToStopForScreenTransition));
        if (timer < transitionTime)
        {
            objToMove.Translate(direction * transitionMoveDistance *(Time.unscaledDeltaTime * (transitionMoveDistance / transitionTime)));
            timer += Time.unscaledDeltaTime;
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
        if (ui != null)
            ui.DeathsTextUpdate();
        player_animator.Play("Death");
        player.deathParticles.Play();
        if (deathReviveAnimationLength == 0)
        {
            CountDeathReviveAnimationLength();
        }
        yield return new WaitForSeconds(deathReviveAnimationLength);
        //player.isDead = true;
        if (ui != null)
            ui.SplashShow();



        yield return new WaitForSeconds(ui.splashAnimationLength);
        MovePlayerToCheckpoint();
        foreach (var reseter in resetersArray)
        {
            if (reseter != null)
                reseter.ReactivateAtPlayersDeath();
        }
        if (ui != null)
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
        /*float stopEndTime = Time.realtimeSinceStartup + stopTime;
        while (Time.realtimeSinceStartup < stopEndTime)
        {
            yield return 0;
        }*/
        yield return new WaitForSecondsRealtime(stopTime);
        Time.timeScale = 1f;
        timeStoped = false;
    }
    //todo add stopframes on transition
}
