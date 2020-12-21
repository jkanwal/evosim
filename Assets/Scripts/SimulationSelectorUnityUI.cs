using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SimulationSelector))]
public class NewBehaviourScript : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SimulationSelector myScript = (SimulationSelector)target;
        if(GUILayout.Button("Generate UI"))
        {
            myScript.generateUI();
        }else if(GUILayout.Button("Remove UI"))
        {
            myScript.removeUI();
        }
    }
}