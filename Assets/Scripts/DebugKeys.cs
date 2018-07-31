using UnityEngine.SceneManagement;
using UnityEngine;

public class DebugKeys : MonoBehaviour {


	void Update () {
		if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
	}

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
}
