using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class GizmoShowInEditor : MonoBehaviour {
    
    SpriteRenderer sr;
    
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (!Application.isPlaying)
            sr.enabled = true;
        else
            sr.enabled = false;
    }
}
