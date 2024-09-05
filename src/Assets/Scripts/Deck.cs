using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TileData
{
    public string name;
    public string description;
    public Vector2Int gridOffset;
}

public class TileGroupData
{
    public List<TileData> tiles = new();
}

public class Deck : MonoBehaviour
{
    const string equipmentDataPath = "Data/Depths - Card Game - Equipment";
    const string utilityDataPath = "Data/Depths - Card Game - Utility";
    const string resourcesDataPath = "Data/Depths - Card Game - Resources";
    const string monstersDataPath = "Data/Depths - Card Game - Monsters";
    const string minesDataPath = "Data/Depths - Card Game - Mines";
    const string tilesDataPath = "Data/Depths - Card Game - Tiles";

    [SerializeField] GameObject equipmentCardPrefab;
    [SerializeField] GameObject utilityCardPrefab;
    [SerializeField] GameObject resourcesCardPrefab;
    [SerializeField] GameObject monstersCardPrefab;
    [SerializeField] GameObject minesCardPrefab;
    [SerializeField] Tilemap gameBoardRef;
    [SerializeField] int numSpecialTiles = 15;

    List<GameObject> equipmentCards = new List<GameObject>();
    List<GameObject> utilityCards = new List<GameObject>();
    List<GameObject> resourcesCards = new List<GameObject>();
    List<GameObject> monsterCards = new List<GameObject>();
    List<GameObject> mineCards = new List<GameObject>();
    List<List<>>

    string[] SplitData( string line )
    {
        var data = line.Split( ',' ).ToList();

        for( var i = 0; i < data.Count; ++i )
        {
            if( data[i].Length > 0 && data[i][0] == '"' )
            {
                int j = i + 1;
                var newStr = data[i][1..];

                for( ; j < data.Count; ++j )
                {
                    newStr += "," + data[j];
                    if( data[j].Length > 0 && data[j][^1] == '"' )
                        break;
                }

                data.RemoveRange( i + 1, Mathf.Min( data.Count - ( i + 1 ), j - i ) );
                data[i] = newStr.Trim()[..^1];
            }
        }

        return data.ToArray();
    }

    void InitiateDeck(ref List<GameObject> deck )
    {
        deck.RandomShuffle();
        for( var i = 0; i < deck.Count; ++i )
            deck[i].transform.position = deck[i].transform.position.SetZ( i );
    }

    void Start()
    {
        var equipment = Resources.Load<TextAsset>( equipmentDataPath );
        var utility = Resources.Load<TextAsset>( utilityDataPath );
        var resources = Resources.Load<TextAsset>( resourcesDataPath );
        var monsters = Resources.Load<TextAsset>( monstersDataPath );
        var mines = Resources.Load<TextAsset>( minesDataPath );
        var tiles = Resources.Load<TextAsset>( tilesDataPath );

        var gapBetweenDecksX = -1.5f;
        var gapBetweenDecksY = 1.75f;

        foreach( var line in equipment.text.Split( '\n' )[1..].RandomShuffle() )
        {
            var data = SplitData( line );
            var name = data[0];

            if( data[1].Length < 1 || data[2].Length < 1 || data[3].Length < 1 )
                continue;

            var attack = int.Parse( data[1] );
            var defence = int.Parse( data[2] );
            var mining = int.Parse( data[3] );
            var ability = data[4];

            var newCard = Instantiate( equipmentCardPrefab, transform.position.SetY( transform.position.y ), Quaternion.identity );
            var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
            texts[0].text = name.Length > 0 ? name : "EQUIPMENT";
            texts[1].text = ability;
            texts[2].text = attack.ToString();
            texts[3].text = mining.ToString();
            texts[4].text = defence.ToString();
            equipmentCards.Add( newCard );
        }

        foreach( var line in utility.text.Split( '\n' )[1..].RandomShuffle() )
        {
            var data = SplitData( line );
            var name = data[0];
            var description = data[1];
            var count = int.Parse( data[2] );

            for( var i = 0; i < count; ++i )
            {
                var newCard = Instantiate( utilityCardPrefab, transform.position.SetY( transform.position.y + gapBetweenDecksY ), Quaternion.identity );
                var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = name.Length > 0 ? name : "UTILITY";
                texts[1].text = description;
                utilityCards.Add( newCard );
            }
        }

        foreach( var line in resources.text.Split( '\n' )[1..] )
        {
            var data = SplitData( line );
            var name = data[0];
            var gold = data[1];
            var description = data[2];
            var count = int.Parse( data[3] );

            for( var i = 0; i < count; ++i )
            {
                var newCard = Instantiate( resourcesCardPrefab, transform.position.SetY( transform.position.y + gapBetweenDecksY * 2.0f ), Quaternion.identity );
                var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = gold;
                texts[1].text = name.Length > 0 ? name : "EQUIPMENT";
                texts[2].text = description;
                resourcesCards.Add( newCard );
            }
        }

        foreach( var line in monsters.text.Split( '\n' )[1..].RandomShuffle() )
        {
            var data = SplitData( line );

            if( data.Length <= 4 || data[1].Length < 1 || data[2].Length < 1 || data[3].Length < 1 )
                continue;

            var name = data[0];
            var description = data[1];
            var attack = int.Parse( data[2] );
            var defence = int.Parse( data[3] );
            var count = int.Parse( data[4] );

            for( var i = 0; i < count; ++i )
            {
                var newCard = Instantiate( monstersCardPrefab, transform.position.SetX( transform.position.x + gapBetweenDecksX ), Quaternion.identity );
                var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = name.Length > 0 ? name : "MONSTER";
                texts[1].text = description;
                texts[2].text = attack.ToString();
                texts[3].text = defence.ToString();
                monsterCards.Add( newCard );
            }
        }

        foreach( var line in mines.text.Split( '\n' )[1..].RandomShuffle() )
        {
            var data = SplitData( line );
            var name = data[0];
            var description = data[1];
            var defence = int.Parse( data[2] );
            var count = int.Parse( data[3] );

            for( var i = 0; i < count; ++i )
            {
                var newCard = Instantiate( minesCardPrefab, transform.position.SetX( transform.position.x + gapBetweenDecksX ).SetY( transform.position.y + gapBetweenDecksY ), Quaternion.identity );
                var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = name.Length > 0 ? name : "MINE";
                texts[1].text = description;
                texts[2].text = defence.ToString();
                mineCards.Add( newCard );
            }
        }

        var tileLines = tiles.text.Split( '\n' )[1..].RandomShuffle();
        var tilesSelector = new List<Vector3Int>();
        foreach( var pos in gameBoardRef.cellBounds.allPositionsWithin )
            if( gameBoardRef.GetInstantiatedObject( pos ) )
                tilesSelector.Add( pos );
        tilesSelector = tilesSelector.Skip( 1 ).Take( tilesSelector.Count - 2 ).ToList();

        for( var i = 0; i < Mathf.Min( numSpecialTiles, tileLines.Count ); ++i )
        {
            var vecPos = tilesSelector.RemoveAndGet( Random.Range( 0, tilesSelector.Count ) );
            var tile = gameBoardRef.GetInstantiatedObject( vecPos );
            var data = SplitData( tileLines[i] );
            var name = data[0];
            var description = data[1];
            var texts = tile.GetComponentsInChildren<TMPro.TextMeshProUGUI>( true );
            texts[0].text = name;
            texts[1].text = description;
        }

        InitiateDeck( ref equipmentCards );
        InitiateDeck( ref utilityCards );
        InitiateDeck( ref resourcesCards );
        InitiateDeck( ref monsterCards );
        InitiateDeck( ref mineCards );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
