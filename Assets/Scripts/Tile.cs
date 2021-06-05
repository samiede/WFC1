using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

enum Directions
{
    left,
    top,
    right,
    bottom
}

[ExecuteAlways]
public class Tile : MonoBehaviour
{
    [SerializeField]
    private List<TilePrototype> _prototypes = new List<TilePrototype>();
    public List<TilePrototype> Prototypes => _prototypes;
    private SpriteRenderer _renderer;
    private WavefunctionCollapse _waveFunction;
    private Transform _transform;
    private Vector2Int _tileIndex;

    public int Entropy => _prototypes.Count;

    private void Awake()
    {
        PopulatePrototypeList();
        _renderer = GetComponent<SpriteRenderer>();
        _transform = transform;
    }

    private void Start()
    {
        _waveFunction = FindObjectOfType<WavefunctionCollapse>();
    }

    void PopulatePrototypeList()
    {
        string[] assetNames = AssetDatabase.FindAssets("t:TilePrototype", new[] {"Assets/Prototypes" });
        _prototypes.Clear();
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var prototype = AssetDatabase.LoadAssetAtPath<TilePrototype>(SOpath);
            _prototypes.Add(prototype);
        }
    }

    // TODO this can be adapted, does not have to collapse to random!
    public void CollapseToRandom()
    {
        int chosenIndex = UnityEngine.Random.Range(0, _prototypes.Count);
        TilePrototype prot = _prototypes[chosenIndex];
        _prototypes.Clear();
        _prototypes.Add(prot);
        ShowCollapsedSprite();
    }

    public void NotifyAboutChange()
    {
        // Get tile above 
        if (_tileIndex.y < _waveFunction.MapSize.y - 1)
        {
            Tile above = _waveFunction.Tiles[_tileIndex.x, _tileIndex.y + 1];
            above.NeighbourHasChanged("bottom", this);
        }
        
        // Get tile to the right
        if (_tileIndex.x < _waveFunction.MapSize.x - 1)
        {
            Tile toTheRight =  _waveFunction.Tiles[_tileIndex.x + 1, _tileIndex.y];
            toTheRight.NeighbourHasChanged("left", this);
        }
        
        // Get tile below
        if (_tileIndex.y > 0)
        {
            Tile below = _waveFunction.Tiles[_tileIndex.x, _tileIndex.y -1];
            below.NeighbourHasChanged("top", this);

        }
        
        // Get tile to the left
        // Get tile to the right
        if (_tileIndex.x > 0)
        {
            Tile toTheLeft =  _waveFunction.Tiles[_tileIndex.x - 1, _tileIndex.y];
            toTheLeft.NeighbourHasChanged("right", this);
        }
    }


    public void NeighbourHasChanged(string socket, Tile neighbour)
    {
        if (Entropy == 1)
            return;
        
        List<TilePrototype> neighbourPrototypes = neighbour.Prototypes;
        switch (socket)
        {
            case "top":
                UpdateCompatiblePrototypes("bottom", socket, neighbourPrototypes);
                break;
            case "right":
                UpdateCompatiblePrototypes("left", socket, neighbourPrototypes);
                break;
            case "bottom":
                UpdateCompatiblePrototypes("top", socket, neighbourPrototypes);
                break;
            case "left":
                UpdateCompatiblePrototypes("right", socket, neighbourPrototypes);
                break;
            default:
                break;
        }
        
    }
    
    
    // Print

    void UpdateCompatiblePrototypes(string neighbourSocket, string ownSocket, List<TilePrototype> neighbourPrototypes)
    {
        List<TilePrototype> remainingPrototypes = new List<TilePrototype>();
        
        // foreach (TilePrototype neighbourPrototype in neighbourPrototypes)
        // {
        //     int neighbourIndex = 0;
        //     for (int n = 0; n < neighbourPrototype.sockets.Count; n++)
        //     {
        //         if (neighbourPrototype.sockets[n].key == neighbourSocket)
        //         {
        //             neighbourIndex = n;
        //             break;
        //         }
        //     }
        //     Debug.Log("Neighbour Index: " + neighbourIndex);
        //     
        //     foreach (TilePrototype ownPrototype in _prototypes)
        //     {
        //         int ownIndex = 0;
        //         for (int o = 0; o < ownPrototype.sockets.Count; o++)
        //         {
        //             if (ownPrototype.sockets[o].key == ownSocket)
        //             {
        //                 ownIndex = o;
        //                 break;
        //             }
        //         }
        //         
        //         if (ownPrototype.sockets[ownIndex].value == neighbourPrototype.sockets[neighbourIndex].value)
        //             remainingPrototypes.Add(ownPrototype);
        //     }
        // }

        // _prototypes.Clear();
        // _prototypes = remainingPrototypes;
        
        if (Entropy == 1)
            ShowCollapsedSprite();
    }
    
    public void SetTileIndex(Vector2Int tileIndex)
    {
        _tileIndex = tileIndex;
    }

    void ShowCollapsedSprite()
    {
        _renderer.sprite = _prototypes[0].sprite;
    }
    
    
}
