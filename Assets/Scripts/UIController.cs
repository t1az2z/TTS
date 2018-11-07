using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public static UIController Instance;
    public GameController gc;


    //SplashScreen
    [SerializeField] GameObject splashScreen;
    Animator splashAnim;
    AnimatorClipInfo[] s_CurrentClipInfo;
    [HideInInspector] public float splashAnimationLength;

    //Counters
    [SerializeField] Text deathsCounter;
    [SerializeField] Text collectiblesCounter;

    void Awake()
    {
        SingletonImplementation();
    }



    void Start () {
        gc = FindObjectOfType<GameController>();
        SetSplashScreenReference();
        SetTextFieldsReferences();
        splashAnimationLength = CountSplashAnimationLength();
	}
	
	void Update ()
    {
        SetSplashScreenReference();
        SetTextFieldsReferences();
    }

    #region Singleton
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
    #endregion
    #region Splash Screen
    private void SetSplashScreenReference()
    {
        if (splashScreen == null)
        {
            Debug.Log("Splash Screen Reference lost. Fix It");
            splashScreen = gameObject.transform.Find("Splash").gameObject;
        }
        splashAnim = splashScreen.GetComponent<Animator>();
    }

    public void SplashShow()
    {
        splashAnim.Play("SplashShow");
    }

    public void SplashHide()
    {
        splashAnim.Play("SplashHide");
    }

    private float CountSplashAnimationLength()
    {
        s_CurrentClipInfo = splashAnim.GetCurrentAnimatorClipInfo(0);
        return s_CurrentClipInfo[0].clip.length;
    }
    #endregion
    #region Counters
    private void SetTextFieldsReferences()
    {
        if (deathsCounter == null)
        {
            Debug.Log("Death Counter Reference lost. Fix It");
            deathsCounter = transform.Find("Deaths/Counter").GetComponent<Text>();
        }
        deathsCounter.text = "x " + gc.deaths.ToString();

        if (collectiblesCounter == null)
        {
            Debug.Log("Collectibles Counter Reference lost. Fix It");
            collectiblesCounter = transform.Find("Collectibles/Counter").GetComponent<Text>();
        }
        collectiblesCounter.text = "x " + gc.collectiblesCollected.ToString();


    }

    public void DeathsTextUpdate()
    {
        deathsCounter.text = "x " + gc.deaths.ToString();
    }

    public void CollectiblesTextUpdate()
    {
        collectiblesCounter.text = "x " + gc.collectiblesCollected.ToString();
    }
    #endregion

}
