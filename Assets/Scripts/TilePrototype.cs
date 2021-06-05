using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default Prototype", menuName = "ScriptableObjects/Tiles/Prototype", order = 1)]
public class TilePrototype : ScriptableObject
{
    public Sprite sprite;
    public int rotation = 0;
    public List<Socket> sockets = new List<Socket>();
    // [HideInInspector] public SocketDictionary socketDict = new SocketDictionary();
    public Dictionary<string, int> socketDict = new Dictionary<string, int>();

}

[Serializable]
public class Socket
{
    public string key;
    public int value;

    public Socket(string _key, int _value)
    {
        key = _key;
        value = _value;
    }
}