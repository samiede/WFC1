using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEditor;
using UnityEngine;

public class PrototypeCreator : EditorWindow
{
    [SerializeField] public List<Sprite> tiles = new List<Sprite>();

    private SerializedObject _so;
    private SerializedProperty propTiles;

    [MenuItem("Tools/Prototype Creator")]
    public static void OpenWindow() => GetWindow<PrototypeCreator>("Prototype Creator");

    private void OnEnable()
    {
        _so = new SerializedObject(this);
        propTiles = _so.FindProperty("tiles");
    }


    private void OnGUI()
    {
        EditorGUILayout.LabelField("Tiles", EditorStyles.boldLabel);
        
        _so.Update();
        EditorGUILayout.PropertyField(propTiles);
        using (new EditorGUI.DisabledScope(tiles.Count == 0))
        {
            if (GUILayout.Button("Generate!"))
                GeneratePrototypes();
        }
        _so.ApplyModifiedProperties();

        GUILayout.FlexibleSpace();
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            // TODO make this undo-able using "Record"
            DrawDragAndDrop();
        }
        using (new EditorGUI.DisabledScope(tiles.Count == 0))
        {
            if (GUILayout.Button("Reset"))
                tiles.Clear();
        }
    }

    void GeneratePrototypes()
    {
        foreach (Sprite sprite in tiles)
        {
            string[] assetNames =
                AssetDatabase.FindAssets(sprite.name + " t:TilePrototype", new[] {"Assets/Prototypes"});

            foreach (string assetName in assetNames)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetName);
                AssetDatabase.DeleteAsset(path);
            }

            TilePrototype prototype = CreateInstance<TilePrototype>();
            prototype.sprite = sprite;
            prototype.sockets = CalculateSockets(sprite);
            
            if (AllSocketsAreEqual(prototype.sockets))
            {
                SocketDictionary socketDictionary = new SocketDictionary();
                // Create serializable dict to use in WFC
                foreach (Socket socket in prototype.sockets)
                {
                    socketDictionary[socket.key] = socket.value;
                }
                prototype.socketDict = socketDictionary;
                AssetDatabase.CreateAsset(prototype, "Assets/Prototypes/" + sprite.name + ".asset");
                AssetDatabase.SaveAssets();
            }
            else
            {
                AssetDatabase.CreateAsset(prototype, "Assets/Prototypes/" + sprite.name + "_0" + ".asset");
                
                for (int i = 1; i < 4; i++)
                {
                    TilePrototype rotatedPrototype = CreateInstance<TilePrototype>();
                    List<Socket> rotatedSockets = new List<Socket>();
                    for (int s = 0; s < prototype.sockets.Count; s++)
                    {
                        rotatedSockets.Add(new Socket(prototype.sockets[s].key, prototype.sockets[(s + prototype.sockets.Count - i) % prototype.sockets.Count].value));
                    }
                    
                    rotatedPrototype.sprite = sprite;
                    rotatedPrototype.rotation = i;
                    rotatedPrototype.sockets = rotatedSockets;
                    
                    // Also create a (serializable) dict
                    SocketDictionary socketDictionary = new SocketDictionary();
                    foreach (Socket socket in rotatedPrototype.sockets)
                    {
                        socketDictionary[socket.key] = socket.value;
                    }
                    
                    rotatedPrototype.socketDict = socketDictionary;
                    if (rotatedPrototype.socketDict.Count > 0)
                    {
                        Debug.Log("Yes: " + rotatedPrototype.socketDict.Count);
                    }
                    AssetDatabase.CreateAsset(rotatedPrototype, "Assets/Prototypes/" + sprite.name + "_" + i + ".asset");
                }
                AssetDatabase.SaveAssets();
                
            }

        }
    }

    void DrawDragAndDrop()
    {
        
        GUIStyle GuistyleBoxDND = new GUIStyle(GUI.skin.box);
        GuistyleBoxDND.alignment = TextAnchor.MiddleCenter;
        GuistyleBoxDND.fontStyle = FontStyle.Italic; 
        GuistyleBoxDND.fontSize = 12;
        GUI.skin.box = GuistyleBoxDND;
        // GuistyleBoxDND.normal.background = MakeTex( 2, 2, Color.white);
        
        Rect myRect = GUILayoutUtility.GetRect(0,50,GUILayout.ExpandWidth(true));
        GUI.Box(myRect,"Drag and Drop Sprites to this Box!",GuistyleBoxDND);
        if (myRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use ();
            }
            
            else if (Event.current.type == EventType.DragPerform)
            {
                for(int i = 0; i< DragAndDrop.objectReferences.Length; i++ )
                {
                    Sprite sprite = DragAndDrop.objectReferences[i] as Sprite;
                    if (sprite != null)
                    {
                        CheckAndAddSprite(sprite);
                    }

                    Texture2D spriteSheet = DragAndDrop.objectReferences[i] as Texture2D;
                    if (spriteSheet != null)
                    {
                        UnityEngine.Object[] data = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(spriteSheet));
    
                        foreach (UnityEngine.Object obj in data)
                        {
                            Sprite subSprite = obj as Sprite;
                            if (subSprite != null)
                                CheckAndAddSprite(subSprite);
                        }

                    }


                }
                Event.current.Use ();
            }
        }
    }

    List<Socket> CalculateSockets(Sprite sprite)
    {
        List<Socket> sockets = new List<Socket>();

        Rect spriteRect = sprite.textureRect;
        int width = (int) spriteRect.width;
        int height =  (int) spriteRect.height;
        
        sockets.Add(new Socket("left", GetPixelValues(0, 0, sprite, false)));
        sockets.Add(new Socket("top", GetPixelValues(0, height, sprite, true)));
        sockets.Add(new Socket("right", GetPixelValues(width, 0, sprite, false)));
        sockets.Add(new Socket("bottom", GetPixelValues(0, 0, sprite, true)));
        return sockets;
    }
    
    int GetPixelValues(int x, int y, Sprite sprite, bool increaseX)
    {
        Color[] pixelValues = new Color[3]; 
        Rect spriteRect = sprite.textureRect;
        
        float width = spriteRect.width;
        float height = spriteRect.height;

        if (increaseX)
        {
            pixelValues[0] = sprite.texture.GetPixel((int) (spriteRect.x + width /4), (int) spriteRect.y + y);
            pixelValues[1] = sprite.texture.GetPixel((int) (spriteRect.x + width/2), (int) spriteRect.y + y);
            pixelValues[2] = sprite.texture.GetPixel((int) (spriteRect.x + 3 * width / 4), (int) spriteRect.y + y);
        }
        else
        {
            pixelValues[0] = sprite.texture.GetPixel((int) spriteRect.x + x, (int) (spriteRect.y + height/4));
            pixelValues[1] = sprite.texture.GetPixel((int) spriteRect.x + x, (int) (spriteRect.y + height/2));
            pixelValues[2] = sprite.texture.GetPixel((int) spriteRect.x + x, (int) (spriteRect.y + 3 * height / 4));
        }
        

        return Keygen(
            ColorUtility.ToHtmlStringRGBA(pixelValues[0]),
            ColorUtility.ToHtmlStringRGBA(pixelValues[1]),
            ColorUtility.ToHtmlStringRGBA(pixelValues[2])
            );
    }

    void CheckAndAddSprite(Sprite sprite)
    {
        if (tiles.Contains(sprite))
            Debug.Log("Already in list!");
        else
            tiles.Add(sprite);
    }

    bool AllSocketsAreEqual(List<Socket> sockets)
    {
        return sockets[0].value == sockets[1].value && sockets[0].value == sockets[2].value &&
               sockets[0].value == sockets[3].value;
    }
    
    public int Keygen(string a, string b, string c)
    {
        return new { a, b, c }.GetHashCode();
    }

}
