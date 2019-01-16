using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public static UIController Instance;

    

    //SplashScreen
    [SerializeField] GameObject splashScreen;
    Animator splashAnim;
    AnimatorClipInfo[] s_CurrentClipInfo;
    [HideInInspector] public float splashAnimationLength;

    //Counters
    [SerializeField] Text deathsCounter;
    [SerializeField] Text collectiblesCounter;
    int collectiblesAmmount;


    void Start ()
    {
        SetSplashScreenReference();
        SetTextFieldsReferences();
        splashAnimationLength = CountSplashAnimationLength();
        collectiblesAmmount = FindObjectsOfType<CollectiblesBehaviour>().Length;

	}
	
	void Update ()
    {
        SetSplashScreenReference();
        SetTextFieldsReferences();
    }

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
        deathsCounter.text = "x " + GameController.Instance.deaths.ToString();

        if (collectiblesCounter == null)
        {
            Debug.Log("Collectibles Counter Reference lost. Fix It");
            collectiblesCounter = transform.Find("Collectibles/Counter").GetComponent<Text>();
        }
        collectiblesCounter.text = "x " + GameController.Instance.collectiblesCollected.ToString() +"/"+ collectiblesAmmount;


    }

    public void DeathsTextUpdate()
    {
        deathsCounter.text = "x " + GameController.Instance.deaths.ToString();
    }

    public void CollectiblesTextUpdate()
    {
        collectiblesCounter.text = "x " + GameController.Instance.collectiblesCollected.ToString() + "/" + collectiblesAmmount;
    }
    #endregion
    public void ReloadLevel()
    {
        GameController.Instance.RestartLevel();
    }
}
