using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.WSA;
using System.Drawing;


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

    [SerializeField] float hexSize;
    [SerializeField] GameObject equipmentCardPrefab;
    [SerializeField] GameObject utilityCardPrefab;
    [SerializeField] GameObject resourcesCardPrefab;
    [SerializeField] GameObject monstersCardPrefab;
    [SerializeField] GameObject minesCardPrefab;
    [SerializeField] GameObject tilePrefab;

    class TileGroup
    {
        public string name;
        public List<Tile> tiles = new();
        public GameObject obj;
    }

    class Tile
    {
        public string name;
        public string description;
        public Vector2Int coords;
    }

    List<GameObject> equipmentCards = new List<GameObject>();
    List<GameObject> utilityCards = new List<GameObject>();
    List<GameObject> resourcesCards = new List<GameObject>();
    List<GameObject> monsterCards = new List<GameObject>();
    List<GameObject> mineCards = new List<GameObject>();
    List<TileGroup> tileGroups = new();

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

    Vector2 HexToPixel( Vector2Int coord )
    {
        var x = hexSize * 3 / 2 * coord.x;
        var y = hexSize * Mathf.Sqrt( 3 ) * ( coord.y + 0.5f * ( coord.x & 1 ) );
        return new Vector2( x, y ) / 1.95f;
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
            if( !int.TryParse( data[2], out var count ) ) 
                continue;

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
            if( !int.TryParse( data[2], out var attack ) ) continue;
            if( !int.TryParse( data[3], out var defence ) ) continue;
            if( !int.TryParse( data[4], out var count ) ) continue;

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
            if( int.TryParse( data[2], out var defence ) ) continue;
            if( int.TryParse( data[3], out var count ) ) continue;

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

        TileGroup tileGroup = null;

        foreach( var line in tiles.text.Split( '\n' )[1..] )
        {
            var data = SplitData( line );
            var group = data[0];
            var name = data[1];
            var description = data[2];
            var coordsStr = data[3].Split( ',' );

            if( data[1].Length == 0 )
            {
                if( tileGroup != null )
                {
                    tileGroups.Add( tileGroup );
                    tileGroup.obj.transform.Rotate( 0.0f, 0.0f, 60.0f * Random.Range( 0, 5 ) );
                }
                tileGroup = null;
                continue;
            }

            if( tileGroup == null )
            {
                tileGroup = new TileGroup();
                tileGroup.name = group;
                tileGroup.obj = Instantiate( tilePrefab, new Vector2( tileGroups.Count * 2.0f, 0.0f ), Quaternion.identity );

                foreach( Transform child in tileGroup.obj.transform )
                    Destroy( child.gameObject );
            }

            var coords = new Vector2Int( int.Parse( coordsStr[0] ), int.Parse( coordsStr[1] ) );
            tileGroup.tiles.Add( new Tile()
            {
                name = name,
                description = description,
                coords = coords,
            } );

            var offset = HexToPixel( coords );
            var newTile = Instantiate( tilePrefab, offset + new Vector2( tileGroups.Count * 2.0f, 0.0f ), Quaternion.identity, tileGroup.obj.transform );
            Destroy( newTile.GetComponent<Draggable>() );
            tileGroup.obj.AddComponent<CircleCollider2D>().offset = newTile.transform.localPosition;
            Destroy( newTile.GetComponent<Collider2D>() );

            var texts = newTile.GetComponentsInChildren<TMPro.TextMeshProUGUI>( true );
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
