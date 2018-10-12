using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class GameController : MonoBehaviour {

    //gc
    public static GameController Instance;
    //cameras
    public GameObject currentCamera;
    private GameObject previousCamera;


    //player references
    PlayerController player;
    Animator player_animator;
    AnimatorClipInfo[] p_CurrentClipInfo;
    float deathReviveAnimationLength;

    //UI references
    GameObject splashScreen;
    Animator splash_animator;
    AnimatorClipInfo[] s_CurrentClipInfo;
    public float splashAnimationLength;

    //collectibles
    Text collectibleCounterText;
    [HideInInspector]public int collectiblesCollected = 0;
    Text deathCounterText;
    [HideInInspector]public int deaths = 0;

    Vector2 activeCheckpoint;

    [Header("Cam shake parameters")]
    public float frequency = 10f;
    public float amplitude = .1f;
    public float duration = .2f;


    public void ShakeCamera( )
    {
        currentCamera.GetComponent<CinemachineCameraShaker>().ShakeCamera(duration, amplitude, frequency);
    }

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
        SetPlayerReference();
        SetSplashScreenReference();
        SetDeathCounterReference();
        SetCollectiblesCounterReference();
        previousCamera = currentCamera;
    }

    private void SetDeathCounterReference() //todo переделать этот АД!
    {
        deathCounterText = GameObject.Find("Deaths").transform.Find("Counter").GetComponent<Text>();
        deathCounterText.text = "x 0";
    }

    private void SetCollectiblesCounterReference() //todo переделать этот АД!
    {
        collectibleCounterText = GameObject.Find("Collectibles").transform.Find("Counter").GetComponent<Text>();
        collectibleCounterText.text = "x 0";
    }
    public void UpdateCollectiblesCounter()
    {
        collectibleCounterText.text = "x " + collectiblesCollected.ToString();
    }

    //todo make splashScreen null check EVERYWHERE
    private void SetSplashScreenReference()
    {
        splashScreen = GameObject.Find("Splash");
        splash_animator = splashScreen.GetComponent<Animator>();
        splashScreen.SetActive(false);
    }

    private void SetPlayerReference()
    {
        player = FindObjectOfType<PlayerController>();
        player_animator = player.animator;
    }
    private void Update()
    {
        if(Debug.isDebugBuild)
            DebugKeys();

        if (splashScreen == null)
            SetSplashScreenReference();
        if (player == null)
            SetPlayerReference();
        if (collectibleCounterText == null)
            SetCollectiblesCounterReference();
        if (deathCounterText == null)
            SetDeathCounterReference();
    }

    private void DebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            SetChekpoint(Vector2.zero);
        }
        if(Input.GetKeyDown(KeyCode.T))
        {
            
            StartCoroutine("DeathCoroutine");
            player.isDead = true;
        }

    }

    public void SwitchCamera(GameObject newCamera)
    {
        //currentCamera.GetComponent<CinemachineCameraShaker>().StopAllCoroutines();
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
            activeCheckpoint = new Vector2(checkpoint.x, checkpoint.y-1f); //sprite bottom offset
    }


    public IEnumerator DeathCoroutine()
    {
        player.controllsEnabled = false;
        deaths++;
        deathCounterText.text = "x " + deaths.ToString();
        player_animator.Play("Death");
        if (deathReviveAnimationLength == 0)
        {
            CountDeathReviveAnimationLength();
        }
        yield return new WaitForSeconds(deathReviveAnimationLength);
        player.isDead = true;
        splashScreen.SetActive(true);
        splash_animator.Play("SplashShow");

        if(splashAnimationLength <= Mathf.Epsilon)
        {
            CountSplashAnimationLength();
        }

        yield return new WaitForSeconds(splashAnimationLength);
        MovePlayerToCheckpoint();
        splash_animator.Play("SplashHide");
        yield return new WaitForSeconds(splashAnimationLength);
        splashScreen.SetActive(false);
        player.animator.Play("Revive"); //todo get it out of here
        yield return new WaitForSeconds(deathReviveAnimationLength);
        player.isDead = false;
        player.controllsEnabled = true;
    }

    private void CountDeathReviveAnimationLength()
    {
        p_CurrentClipInfo = player_animator.GetCurrentAnimatorClipInfo(0);
        deathReviveAnimationLength = p_CurrentClipInfo[0].clip.length;
    }

    private void CountSplashAnimationLength()
    {
        s_CurrentClipInfo = splash_animator.GetCurrentAnimatorClipInfo(0);
        splashAnimationLength = s_CurrentClipInfo[0].clip.length;
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

    //todo add stopframes on transition
}
