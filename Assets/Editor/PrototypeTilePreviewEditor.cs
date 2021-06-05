using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

[CustomEditor(typeof(TilePrototype))]
public class PrototypeTilePreviewEditor : Editor
{
    SerializedProperty spriteProperty;
    private TilePrototype prot;
    void OnEnable()
    {
        prot = (TilePrototype) target;
        spriteProperty = serializedObject.FindProperty("sprite");

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GUILayout.Label("Tile Sprite");
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            var texture = AssetPreview.GetAssetPreview(prot.sprite);
            GUILayout.Label(texture);
            GUILayout.FlexibleSpace();
            
        }

    }

}
