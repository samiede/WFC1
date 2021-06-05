using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WavefunctionCollapse))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WavefunctionCollapse map = target as WavefunctionCollapse;

        if (GUILayout.Button("Generate Map"))
        {
            map.SetupMap();
        }
        
        if (GUILayout.Button("Start Collapse"))
        {
            map.StartCollapse();
        }
        
    }
}