using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(UIGeneration))]
public class SimulationSelectorUnityUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UIGeneration myScript = (UIGeneration)target;
        if(GUILayout.Button("Create Menu"))
        {
            myScript.createMenu();
        }else if(GUILayout.Button("Generate UI"))
        {
            myScript.generateUI();
        }else if(GUILayout.Button("Remove UI"))
        {
            myScript.removeUI();
        }
    }
}
