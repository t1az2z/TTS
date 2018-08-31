#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;


[CustomEditor(typeof(BackGround))]
public class BackGroundEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BackGround backGroundScript = (BackGround)target;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Create BackGround"))
        {
            backGroundScript.CreateBackGround();
        }

        if (GUILayout.Button("Delete BackGround"))
        {
            backGroundScript.DeleteBackGround();

        }
        GUILayout.EndHorizontal();
    }
}
