using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine;

public class LevelChanger : MonoBehaviour {

public void OnTriggerEnter2D()
    {
        GameController.Instance.SetChekpoint(Vector3.zero);
        GameController.Instance.RestartLevel();

    }
}
