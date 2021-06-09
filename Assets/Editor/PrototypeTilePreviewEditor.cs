using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

[CustomEditor(typeof(TilePrototype))]
public class PrototypeTilePreviewEditor : Editor
{
    private TilePrototype prot;
    private float angle = 10;
    void OnEnable()
    {
        prot = (TilePrototype) target;
        // spriteProperty = serializedObject.FindProperty("sprite");

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("Tile Sprite");
        Rect labelRect = GUILayoutUtility.GetLastRect();
        float y = labelRect.y + labelRect.height;
        int rectSize = 50;
        int spaceFromLeft = 25;
        int spaceBetweenLabel = 50;
        GUILayout.Space(200);

        Texture2D texture = AssetPreview.GetAssetPreview(prot.sprite);
        Matrix4x4 matrixBackup = GUI.matrix;
        Vector2 pos = new Vector2(rectSize/2 + spaceFromLeft,  (rectSize / 2) + y + spaceBetweenLabel);
        GUIUtility.RotateAroundPivot(prot.rotation * -90, pos);
        Rect thisRect = new Rect(spaceFromLeft, y + spaceBetweenLabel, rectSize, rectSize);
        GUI.DrawTexture(thisRect, texture);
        GUI.matrix = matrixBackup;


    }

}
