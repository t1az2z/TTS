using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine;

public class LevelChanger : MonoBehaviour {

public void OnTriggerEnter2D()
    {
        FindObjectOfType<GameController>().SetChekpoint(Vector3.zero);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
}
