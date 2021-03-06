﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWallsOnTriggerEnter : MonoBehaviour {

    Animator animator;
    bool isHiden = true;
	void Start () {
        animator = GetComponent<Animator>();
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
	}
	
    private void OnTriggerEnter2D(Collider2D collision)
    {
        isHiden = false;
        //StartCoroutine("HideWalls");
        if (!isHiden)
            animator.Play("Hide");
    }

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (isHiden)
    //        StartCoroutine("HideWalls");

    //}

    private void OnTriggerExit2D(Collider2D collision)
    {
        isHiden = true;
        animator.Play("Show");
    }
    //private void HideWalls()
    //{
    //    //yield return new WaitForSeconds(.2f);
    //    if (!isHiden)
    //        animator.Play("Hide");
    //}
}
