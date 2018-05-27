using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PageTiles
{
    public TileAction[] tiles;
}

public class MatchLevelDesigner : Singleton<MatchLevelDesigner>
{
    public PageTiles[] pageTiles;

    public TileStart startTile;
    public TileEnd endTile;
    public Material[] tileMats;

    MatchManager manager;
    Material[] surpriseTileMats;
    string[] tileNames = { "TileQuestion", "TileAdvance", "TileReturn", "TileStop", "TileBonus", "TileMinigame", "TileSurprise" };

    bool bonus;

    void Awake()
    {
        int max = tileMats.Length - 1;
        surpriseTileMats = new Material[max];
        for (int i = 0; i < max; i++)
            surpriseTileMats[i] = tileMats[i];
        manager = MatchManager.instance;
        StartCoroutine(AssignTiles());
    }

    IEnumerator AssignTiles()
    {
        // Fill out all tiles by page in order
        Dictionary<int, TileAction[]> tilesCollection = new Dictionary<int, TileAction[]>();
        for (int i = 0; i < pageTiles.Length; i++)
            tilesCollection.Add(i, pageTiles[i].tiles);
        // Assign randomized tiles
        GameObject obj;
        TileAction newTile = null;
        TileAction[] arr;
        Renderer[] rends;
        int r,
            prevR = -1,
            max = System.Enum.GetNames(typeof(TileTypes)).Length;
        // Iterate through keys
        for (int i = 0; i < tilesCollection.Count; i++)
        {
            arr = tilesCollection[i];
            bonus = false;
            // Iterate through tiles
            for (int j = 0; j < arr.Length; j++)
            {
                obj = arr[j].gameObject;
                r = Random.Range(0, max);
                /**
                 * Here it should re-roll in 3 cases:
                 *  - Tile is the same as the previous one, not being a question tile
                 *  - Tile is a return tile and it's too close to the start
                 *  - Tile is an advance tile and it's too close to the end
                 */
                while ((r == prevR && r != (int)TileTypes.Question) ||
                    (r == (int)TileTypes.Return && i == 0 && j < 2) ||
                    (r == (int)TileTypes.Advance && i == tilesCollection.Count - 1 && j > arr.Length - 3) ||
                    (r == (int)TileTypes.Bonus && bonus))
                    r = Random.Range(0, max);
                switch (r)
                {
                    case (int)TileTypes.Question:
                        newTile = obj.AddComponent<TileQuestion>();
                        (newTile as TileQuestion).page = i;
                        break;
                    case (int)TileTypes.Advance:
                        newTile = obj.AddComponent<TileMove>();
                        (newTile as TileMove).forward = true;
                        break;
                    case (int)TileTypes.Return:
                        newTile = obj.AddComponent<TileMove>();
                        (newTile as TileMove).forward = false;
                        break;
                    case (int)TileTypes.Stop:
                        newTile = obj.AddComponent<TileModifier>();
                        (newTile as TileModifier).positive = false;
                        break;
                    case (int)TileTypes.Bonus:
                        newTile = obj.AddComponent<TileModifier>();
                        (newTile as TileModifier).positive = true;
                        bonus = true;
                        break;
                    case (int)TileTypes.Minigame:
                        newTile = obj.AddComponent<TileMinigame>();
                        break;
                    case (int)TileTypes.Surprise:
                        newTile = obj.AddComponent<TileSurprise>();
                        (newTile as TileSurprise).mats = surpriseTileMats;
                        obj.AddComponent<TileQuestion>();
                        obj.AddComponent<TileMove>();
                        obj.AddComponent<TileModifier>();
                        obj.AddComponent<TileMinigame>();
                        break;
                }
                obj.name = tileNames[r];
                rends = obj.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer rd in rends)
                    rd.material = tileMats[r];
                Destroy(arr[j]);
                arr[j] = newTile;
                prevR = r;
                yield return new WaitForFixedUpdate();
            }
        }
        // Give all tiles to the manager, inserting start and end tiles
        manager.levelTiles = new Dictionary<int, Tile[]>();
        List<Tile> tiles;
        for (int i = 0; i < tilesCollection.Count; i++)
        {
            tiles = new List<Tile>();
            if (i == 0)
                tiles.Add(startTile as Tile);
            tiles.AddRange(tilesCollection[i]);
            if (i == tilesCollection.Count - 1)
                tiles.Add(endTile as Tile);
            manager.levelTiles[i] = tiles.ToArray();
        }
    }
}