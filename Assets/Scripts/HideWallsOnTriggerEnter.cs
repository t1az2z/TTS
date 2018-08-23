using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWallsOnTriggerEnter : MonoBehaviour {

    Animator animator;
    bool isHiden = true;
	void Start () {
        animator = GetComponent<Animator>();
	}
	
    private void OnTriggerEnter2D(Collider2D collision)
    {
        isHiden = false;
        StartCoroutine("HideWalls");
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        isHiden = true;
        animator.Play("Show");
    }
    private IEnumerator HideWalls()
    {
        yield return new WaitForSeconds(.2f);
        if (!isHiden)
            animator.Play("Hide");
    }
}
