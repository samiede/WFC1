using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


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
    private LineRenderer _lr;

    public int Entropy => _prototypes.Count;

    private void Awake()
    {
        PopulatePrototypeList();
        _renderer = GetComponent<SpriteRenderer>();
        _lr = GetComponent<LineRenderer>();
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
        Debug.Log("Current tile: " + _tileIndex);
        int chosenIndex = Random.Range(0, _prototypes.Count);
        TilePrototype prot = _prototypes[chosenIndex];
        _prototypes.Clear();
        _prototypes.Add(prot);
        ShowCollapsedSprite();
    }

    public void NotifyAboutChange()
    {
        StartCoroutine(NotifyAboutChangeCoroutine());
    }

    public IEnumerator NotifyAboutChangeCoroutine()
    {
        EnableLineRenderer();
        
        // TODO only notify the tiles that have not prompted the prototype update
        
        // Get tile above 
        if (_tileIndex.y < _waveFunction.MapSize.y - 1)
        {
            Tile above = _waveFunction.Tiles[_tileIndex.x, _tileIndex.y + 1];
            above.NeighbourHasChanged("bottom", this);
            DrawLineToNeighbourTile(above);
            yield return new WaitForSeconds(0.5f);
        }
        
        // Get tile to the right
        if (_tileIndex.x < _waveFunction.MapSize.x - 1)
        {
            Tile toTheRight =  _waveFunction.Tiles[_tileIndex.x + 1, _tileIndex.y];
            toTheRight.NeighbourHasChanged("left", this);
            DrawLineToNeighbourTile(toTheRight);
            yield return new WaitForSeconds(0.5f);

        }
        
        // Get tile below
        if (_tileIndex.y > 0)
        {
            Tile below = _waveFunction.Tiles[_tileIndex.x, _tileIndex.y -1];
            below.NeighbourHasChanged("top", this);
            DrawLineToNeighbourTile(below);
            yield return new WaitForSeconds(0.5f);


        }
        
        // Get tile to the left
        if (_tileIndex.x > 0)
        {
            Tile toTheLeft =  _waveFunction.Tiles[_tileIndex.x - 1, _tileIndex.y];
            toTheLeft.NeighbourHasChanged("right", this);
            DrawLineToNeighbourTile(toTheLeft);
            yield return new WaitForSeconds(0.5f);

        }
        
        DisableLineRenderer();
    }


    public void NeighbourHasChanged(string socket, Tile neighbour)
    {
        if (Entropy == 1)
            return;

        int numberOfPrototyesBeforeUpdate = Prototypes.Count;
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
        
        
        if (Prototypes.Count != numberOfPrototyesBeforeUpdate)
            NotifyAboutChange();
        
        
    }
    
    
    void UpdateCompatiblePrototypes(string neighbourSocket, string ownSocket, List<TilePrototype> neighbourPrototypes)
    {
        int ownSocketIndex = WavefunctionCollapse.Directions[ownSocket];
        int neighbourSocketIndex = WavefunctionCollapse.Directions[neighbourSocket];
        
        for (int n = 0; n < neighbourPrototypes.Count; n++)
        {
            _prototypes.RemoveAll(prototype => 
                prototype.sockets[ownSocketIndex].value != neighbourPrototypes[n].sockets[neighbourSocketIndex].value);


        }
        
        
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
        _transform.rotation = Quaternion.Euler(0, 0, 90 * _prototypes[0].rotation);
    }

    void EnableLineRenderer() => _lr.enabled = true;
    void DisableLineRenderer() => _lr.enabled = false;

    void DrawLineToNeighbourTile(Tile neighbour)
    {
        _lr.enabled = true;
        _lr.positionCount = 2;
        Vector3 start = _transform.position;
        Vector3 end = neighbour.transform.position;
        Vector3[] points = {start, end};
        _lr.SetPositions(points);
    }




}
