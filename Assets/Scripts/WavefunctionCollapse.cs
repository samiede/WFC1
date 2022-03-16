using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class WavefunctionCollapse : MonoBehaviour
{
    [SerializeField] private GlobalVars _vars;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Vector2Int mapSize;
    [SerializeField] private Vector2Int startingTile = new Vector2Int(0, 0);

    private Tile[,] tiles;
    public Tile[,] Tiles => tiles;
    public Vector2Int MapSize => mapSize;
    
    public static Dictionary<string, int> Directions = new Dictionary<string, int>() {
        {"left", 0}, 
        {"top", 1},
        {"right", 2},
        {"bottom", 3}};

    private void Start()
    {
        // SetupMap();
    }

    public void SetupMap()
    {
        tiles = new Tile[mapSize.x, mapSize.y];
        
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            Destroy(transform.Find(holderName).gameObject);
        }
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        
        Vector3 tileScale = tilePrefab.transform.localScale;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y, tileScale);
                Tile newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity, mapHolder);
                newTile.SetTileIndex(new Vector2Int(x, y));
                tiles[x, y] = newTile;
            }
        }
    }

    public void StartCollapse()
    {
        // SetupMap();
        StartCoroutine(Collapse());
    }


    IEnumerator Collapse()
    {
        Tile initialTile = tiles[startingTile.x, startingTile.y];
        initialTile.CollapseToRandom();
        
        if (_vars.animationDelay > 0)
            yield return StartCoroutine(initialTile.NotifyAboutChangeCoroutine());
        else StartCoroutine(initialTile.NotifyAboutChangeCoroutine());
        
        while (!AllTilesCollapsed())
        {
            Tile currentTile = GetTileWithLowestEntropy();
            currentTile.CollapseToRandom();
            if (_vars.animationDelay > 0)
                yield return StartCoroutine(currentTile.NotifyAboutChangeCoroutine());
            else
                StartCoroutine(currentTile.NotifyAboutChangeCoroutine());
        }

        Debug.Log("Done");
    }

    public Vector2Int GetIndexFromLocation(Vector2 position, Vector3 scale)
    {
        int x = (int) (position.x / scale.x - 0.5f + (mapSize.x / 2));
        int y = (int) (position.y / scale.y - 0.5f + (mapSize.y / 2));
        return new Vector2Int(x, y);
    }
    
    Vector3 CoordToPosition(int x, int y, Vector3 scale)
    {
        return new Vector3((-mapSize.x / 2 + 0.5f + x) * scale.x,
            (-mapSize.y / 2 + 0.5f + y) * scale.y);
    }

    bool AllTilesCollapsed()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (tiles[x, y].Entropy > 1)
                {
                    return false;

                }
            }
        }
        return true;
    }

    Tile GetTileWithLowestEntropy()
    {
        string[] assetNames = AssetDatabase.FindAssets("t:TilePrototype", new[] {"Assets/Prototypes" });
        int lowestEntropy = assetNames.Length;
        List<Tile> lowestEntropyTiles = new List<Tile>(); 
        
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (tiles[x, y].Entropy < lowestEntropy && tiles[x, y].Entropy > 1)
                    lowestEntropy = tiles[x, y].Entropy;
            }
        }
        
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (tiles[x,y].Entropy == lowestEntropy)
                    lowestEntropyTiles.Add(tiles[x, y]);
            }
        }

        Tile randomTileWithLowestEntropy = lowestEntropyTiles[Random.Range(0, lowestEntropyTiles.Count)];
        return randomTileWithLowestEntropy;

    }

}
