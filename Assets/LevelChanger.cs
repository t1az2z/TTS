using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine;

public class LevelChanger : MonoBehaviour {

    int currentLevelIndex;
    [SerializeField] Image splash;
    [SerializeField] GameObject[] buttons;
    bool floatToNextLevel = false;
    [SerializeField] float floatToNextLevelSpeed = .05f;
    GameObject player;
    public Color tempColor;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        tempColor = splash.color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == player)
        {
            foreach(GameObject button in buttons)
            {
                button.SetActive(false);
            }
            floatToNextLevel = true;
        }
    }
    private void Update()
    {
        if (floatToNextLevel)
        {
            tempColor.a += 1f*Time.deltaTime;
            splash.color = tempColor;
        }
    }
    private void FixedUpdate()
    {
        if (floatToNextLevel && player.transform.position != transform.position)
        {
            player.GetComponent<PlayerController>().enabled = false;
            player.GetComponent<Rigidbody2D>().gravityScale = 0;
            player.transform.position = Vector3.MoveTowards(player.transform.position, transform.position, 2f * Time.fixedDeltaTime);
        }
        else if (floatToNextLevel && player.transform.position == transform.position)
        {
            
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(LoadNextLevelAfterDelay(1f));
    }

    IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Application.CanStreamedLevelBeLoaded(currentLevelIndex + 1))
        {
            SceneManager.LoadScene(currentLevelIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(0);
        }

    }
}
