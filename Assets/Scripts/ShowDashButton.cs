using UnityEngine.UI;
using UnityEngine;

public class ShowDashButton : MonoBehaviour {
    public GameObject button;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        button.SetActive(true);
        gameObject.SetActive(false);
    }
}
