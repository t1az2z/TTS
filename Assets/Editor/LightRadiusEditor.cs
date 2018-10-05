using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (RaycastLight))]
public class LightRadiusEditor : Editor {

	void OnSceneGUI()
    {
        RaycastLight rcl = (RaycastLight) target;
        Handles.color = Color.white;
        Handles.DrawWireArc(rcl.transform.position, new Vector3(0, 0, 1), Vector3.up, 360f, rcl.lightRadius);
        Vector3 lightAngleA = rcl.DirFromAngle(-rcl.lightAngle / 2, false);
        Vector3 lightAngleB = rcl.DirFromAngle(rcl.lightAngle / 2, false);

        Handles.DrawLine(rcl.transform.position, rcl.transform.position + lightAngleA * rcl.lightRadius);
        Handles.DrawLine(rcl.transform.position, rcl.transform.position + lightAngleB * rcl.lightRadius);
    }
}
