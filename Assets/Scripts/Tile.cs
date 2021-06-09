using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;


// [ExecuteAlways]
public class Tile : MonoBehaviour
{
    [SerializeField] private GlobalVars _vars;
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
        ShowCollapsedSprite();
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
        // EnableLineRenderer();
        
        // TODO only notify the tiles that have not prompted the prototype update
        
        // Get tile above 
        if (_tileIndex.y < _waveFunction.MapSize.y - 1)
        {
            Tile above = _waveFunction.Tiles[_tileIndex.x, _tileIndex.y + 1];
            if (above.Entropy > 1)
            {
                if (_vars.animationDelay > 0)
                {
                    DrawLineToNeighbourTile(above);
                    yield return new WaitForSeconds(_vars.animationDelay);
                    yield return above.NeighbourHasChanged("bottom", this);
                }
                else
                {
                    StartCoroutine(above.NeighbourHasChanged("bottom", this));
                }
            }
        }
        
        // Get tile to the right
        if (_tileIndex.x < _waveFunction.MapSize.x - 1)
        {
            Tile toTheRight =  _waveFunction.Tiles[_tileIndex.x + 1, _tileIndex.y];
            if (toTheRight.Entropy > 1)
            {
                if (_vars.animationDelay > 0)
                {
                    DrawLineToNeighbourTile(toTheRight);
                    yield return new WaitForSeconds(_vars.animationDelay);
                    yield return toTheRight.NeighbourHasChanged("left", this);
                }
                else
                {
                    StartCoroutine(toTheRight.NeighbourHasChanged("left", this));
                }
            }

        }
        
        // Get tile below
        if (_tileIndex.y > 0)
        {
            Tile below = _waveFunction.Tiles[_tileIndex.x, _tileIndex.y -1];
            if (below.Entropy > 1)
            {
                if (_vars.animationDelay > 0)
                {
                    DrawLineToNeighbourTile(below);
                    yield return new WaitForSeconds(_vars.animationDelay);
                    yield return below.NeighbourHasChanged("top", this);
                }
                else
                {
                    StartCoroutine(below.NeighbourHasChanged("top", this));
                }
            }


        }
        
        // Get tile to the left
        if (_tileIndex.x > 0)
        {
            Tile toTheLeft =  _waveFunction.Tiles[_tileIndex.x - 1, _tileIndex.y];
            if (toTheLeft.Entropy > 1)
            {
                if (_vars.animationDelay > 0)
                {
                    DrawLineToNeighbourTile(toTheLeft);
                    yield return new WaitForSeconds(_vars.animationDelay);
                    yield return toTheLeft.NeighbourHasChanged("right", this);
                }
                else
                {
                    StartCoroutine(toTheLeft.NeighbourHasChanged("right", this));
                }
            }

        }
        
        DisableLineRenderer();
    }


    public IEnumerator NeighbourHasChanged(string socket, Tile neighbour)
    {
        if (Entropy == 1)
            yield break;

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
        {
            if (_vars.animationDelay > 0)
                yield return NotifyAboutChangeCoroutine();
            else
            {
                NotifyAboutChange();
            }
            
        }
        
        
    }
    
    
    void UpdateCompatiblePrototypes(string neighbourSocket, string ownSocket, List<TilePrototype> neighbourPrototypes)
    {
        int ownSocketIndex = WavefunctionCollapse.Directions[ownSocket];
        int neighbourSocketIndex = WavefunctionCollapse.Directions[neighbourSocket];
        List<TilePrototype> fittingSockets = new List<TilePrototype>();
        
        for (int n = 0; n < neighbourPrototypes.Count; n++)
        {
            for (int o = 0; o < Prototypes.Count; o++)
            {
                if (_prototypes[o].sockets[ownSocketIndex].value ==
                    neighbourPrototypes[n].sockets[neighbourSocketIndex].value && !fittingSockets.Contains(_prototypes[o]))
                {
                    TilePrototype prot = _prototypes[o];
                    fittingSockets.Add(prot);
                }
            }
        }

        if (fittingSockets.Count != Prototypes.Count)
        {
            _prototypes.Clear();
            _prototypes.AddRange(fittingSockets);
        }
        ShowCollapsedSprite();
    }
    
    public void SetTileIndex(Vector2Int tileIndex)
    {
        _tileIndex = tileIndex;
    }

    void ShowCollapsedSprite()
    {
        int numberOfPrototypes = Prototypes.Count;
        RemoveAllChildren(gameObject);
        _renderer.sprite = _prototypes[0].sprite;
        if (numberOfPrototypes == 1)
        {
            _renderer.enabled = true;
            _transform.rotation = Quaternion.Euler(0, 0, -90 * _prototypes[0].rotation);
        }
        else
        {
            _renderer.enabled = false;
            int numberOfImagesPerRow = Mathf.CeilToInt(Mathf.Sqrt(numberOfPrototypes));
            float tileSize = (1f / numberOfImagesPerRow) * 0.95f;
            for (int x = 0; x < numberOfImagesPerRow; x++)
            {
                for (int y = 0; y < numberOfImagesPerRow; y++)
                {
                    if (_prototypes.Count - 1 >= x * numberOfImagesPerRow + y)
                    {
                        GameObject newTileHolder = new GameObject(_prototypes[x * numberOfImagesPerRow + y].name);
                        newTileHolder.transform.parent = _transform;
                        SpriteRenderer r = newTileHolder.AddComponent<SpriteRenderer>();
                        r.sprite = _prototypes[x * numberOfImagesPerRow + y].sprite;
                        newTileHolder.transform.localPosition = new Vector3(
                            x * tileSize - _transform.localScale.x / 2 + tileSize/2, 
                            _transform.localScale.y - y * tileSize - tileSize/2 - _transform.localScale.y / 2, 
                            0f);
                        newTileHolder.transform.localScale = new Vector3(tileSize, tileSize, 1);
                        newTileHolder.transform.localRotation = Quaternion.Euler(0f, 0f, -90 * _prototypes[x * numberOfImagesPerRow + y].rotation);
                    }
                }
            }
        }
        
    }

    public static void RemoveAllChildren(GameObject parent)
    {
        Transform transform;
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            transform = parent.transform.GetChild(i);
            Destroy(transform.gameObject);
        }
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
