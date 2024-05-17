using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ReinforcementManager))]
public class ReinforcementManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Save Model"))
        {
            ((ReinforcementManager)target).SaveModel();
        }
    }
}
