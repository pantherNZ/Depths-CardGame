using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

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

    void InitiateDeck( Vector3 position, List<GameObject> deck )
    {
        deck.RandomShuffle();
        for( var i = 0; i < deck.Count; ++i )
            deck[i].transform.position = position.SetZ( i );
    }

    Vector2 HexToPixel( Vector2Int coord )
    {
        var x = hexSize * 3 / 2 * coord.x;
        var y = hexSize * Mathf.Sqrt( 3 ) * ( coord.y + 0.5f * ( coord.x & 1 ) );
        return new Vector2( x, y ) / 1.95f;
    }

    string HTMLFormat( string str )
    {
        return str.Replace( "<BR>", "\n" );
    }

    void Start()
    {
        var equipment = Resources.Load<TextAsset>( equipmentDataPath );
        var utility = Resources.Load<TextAsset>( utilityDataPath );
        var resources = Resources.Load<TextAsset>( resourcesDataPath );
        var monsters = Resources.Load<TextAsset>( monstersDataPath );
        var mines = Resources.Load<TextAsset>( minesDataPath );
        var tiles = Resources.Load<TextAsset>( tilesDataPath );

        foreach( var line in equipment.text.Split( '\n' )[1..].RandomShuffle() )
        {
            var data = SplitData( line );
            var name = data[0];

            if( data[0].Length < 1 || data[2].Length < 1 || data[3].Length < 1 )
                continue;

            var cost = data[1].Length > 0 ? int.Parse( data[1] ) : 0;
            var attack = int.Parse( data[2] );
            var defence = int.Parse( data[3] );
            var mining = int.Parse( data[4] );
            var ability = HTMLFormat(data[5]);

            var newCard = Instantiate( equipmentCardPrefab, transform.position, Quaternion.identity );
            newCard.name = name;
            var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
            texts[0].text = name;
            texts[1].text = ability;
            texts[2].text = attack.ToString();
            texts[3].text = mining.ToString();
            texts[4].text = defence.ToString();
            texts[5].text = cost.ToString();
            equipmentCards.Add( newCard );
        }

        foreach( var line in utility.text.Split( '\n' )[1..].RandomShuffle() )
        {
            var data = SplitData( line );
            var name = data[0];
            var description = HTMLFormat( data[1] );
            if( !int.TryParse( data[2], out var cost ) ) 
                continue;
            if( !int.TryParse( data[3], out var count ) )
                continue;

            for( var i = 0; i < count; ++i )
            {
                var newCard = Instantiate( utilityCardPrefab, Vector3.zero, Quaternion.identity );
                newCard.name = name;
                var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = name;
                texts[1].text = description;
                texts[2].text = cost.ToString();
                utilityCards.Add( newCard );
            }
        }

        foreach( var line in resources.text.Split( '\n' )[1..] )
        {
            var data = SplitData( line );
            var name = data[0];
            if( !int.TryParse( data[1], out var cost ) )
                continue;
            if( !int.TryParse( data[2], out var gold ) )
                continue;
            var description = HTMLFormat( data[3] );
            var count = int.Parse( data[4] );

            for( var i = 0; i < count; ++i )
            {
                var newCard = Instantiate( resourcesCardPrefab, Vector3.zero, Quaternion.identity );
                newCard.name = name;
                var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = gold.ToString();
                texts[1].text = name;
                texts[2].text = description;
                texts[3].text = cost.ToString();
                resourcesCards.Add( newCard );
            }
        }

        foreach( var line in monsters.text.Split( '\n' )[1..].RandomShuffle() )
        {
            var data = SplitData( line );

            if( data.Length < 5 || data[0].Length < 1 )
                continue;

            var name = data[0];
            var description = HTMLFormat( data[1] );
            var reward = data[2];
            if( !int.TryParse( data[3], out var defence ) ) 
                continue;
            if( !int.TryParse( data[4], out var count ) ) 
                continue;
            var onLose = data[5];

            for( var i = 0; i < count; ++i )
            {
                var newCard = Instantiate( monstersCardPrefab, Vector3.zero, Quaternion.identity );
                newCard.name = name;
                var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = name;
                texts[1].text = description + "\n\nWin: " + reward + "\nLose: " + onLose;
                texts[2].text = defence.ToString();
                monsterCards.Add( newCard );
            }
        }

        foreach( var line in mines.text.Split( '\n' )[1..].RandomShuffle() )
        {
            var data = SplitData( line );
            var name = data[0];
            var description = HTMLFormat( data[1] );
            var reward = data[2];
            if( !int.TryParse( data[3], out var defence ) ) 
                continue;
            if( !int.TryParse( data[4], out var count ) ) 
                continue;
            var lose = data[5];

            for( var i = 0; i < count; ++i )
            {
                var newCard = Instantiate( minesCardPrefab, Vector3.zero, Quaternion.identity );
                newCard.name = name;
                var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = name;
                texts[1].text = description;
                texts[2].text = defence.ToString();
                mineCards.Add( newCard );
            }
        }

        TileGroup tileGroup = null;

        foreach( var ( idx, line ) in tiles.text.Split( '\n' )[1..].Enumerate() )
        {
            var data = SplitData( line );
            var group = data[0] + idx;
            var name = data[1];
            var description = HTMLFormat( data[2] );
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

        var gapBetweenDecksY = 1.75f;

        var mainDeck = equipmentCards;
        mainDeck.InsertRange( 0, utilityCards );
        mainDeck.InsertRange( 0, resourcesCards );

        InitiateDeck( transform.position.SetY( transform.position.y + gapBetweenDecksY ), mainDeck );

        var sideDeck = monsterCards;
        monsterCards.InsertRange( 0, mineCards );

        InitiateDeck( transform.position.SetY( transform.position.y + gapBetweenDecksY * 2.0f ), sideDeck );

        try
        {
            var bytes = File.ReadAllBytes( "allcards" );
            using var memoryStream = new MemoryStream( bytes, writable: false );
            using var reader = new BinaryReader( memoryStream );

            LoadCards( equipmentCards, reader );
            LoadCards( utilityCards, reader );
            LoadCards( resourcesCards, reader );
            LoadCards( monsterCards, reader );
            LoadCards( mineCards, reader );

            var groupCount = reader.ReadInt32();
            for( int i = 0; i < groupCount; ++i )
            {
                var name = reader.ReadString();
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                var z = reader.ReadSingle();
                var rot = reader.ReadSingle();
                var group = tileGroups.FirstOrDefault( g => g.name == name );
                if( group != null )
                {
                    group.obj.transform.position = new Vector3( x, y, z );
                    group.obj.transform.rotation = Quaternion.Euler( 0.0f, 0.0f, rot );
                }
            }
        }
        catch( System.Exception e )
        {
            Debug.Log( "No save data found: " + e );
        }

        StartCoroutine( SaveLoop() );
    }

    void WriteCards( List<GameObject> cards, BinaryWriter writer )
    {
        writer.Write( cards.Count );
        foreach( var card in cards )
        {
            writer.Write( card.name );
            writer.Write( card.transform.position.x );
            writer.Write( card.transform.position.y );
            writer.Write( card.transform.position.z );
        }
    }

    void LoadCards( List<GameObject> cards, BinaryReader reader )
    {
        var save = reader != null ? ReadCards( reader ) : null;
        if( save != null )
        {
            foreach( var card in cards )
            {
                if( save.TryGetValue( card.name, out var pos ) )
                {
                    card.transform.position = pos[0];
                    pos.RemoveAt( 0 );
                }
            }
        }
    }

    Dictionary<string, List<Vector3>> ReadCards( BinaryReader reader )
    {
        var results = new Dictionary<string, List<Vector3>>();
        var count = reader.ReadInt32();
        for( int i = 0; i < count; ++i )
        {
            var name = reader.ReadString();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            results.GetOrAdd( name ).Add( new Vector3( x, y, z ) );
        }
        return results;
    }

    // Update is called once per frame
    IEnumerator SaveLoop()
    {
        while( true )
        {
            yield return new WaitForSeconds( 5.0f );
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    void Save()
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter( memoryStream );

        WriteCards( equipmentCards, writer );
        WriteCards( utilityCards, writer );
        WriteCards( resourcesCards, writer );
        WriteCards( monsterCards, writer );
        WriteCards( mineCards, writer );

        writer.Write( tileGroups.Count );
        foreach( var group in tileGroups )
        {
            writer.Write( group.name );
            writer.Write( group.obj.transform.position.x );
            writer.Write( group.obj.transform.position.y );
            writer.Write( group.obj.transform.position.z );
            writer.Write( group.obj.transform.rotation.eulerAngles.z );
        }

        var content = memoryStream.ToArray();
        System.IO.File.WriteAllBytes( "allcards", content );
    }

#if UNITY_EDITOR
    [MenuItem( "Scripts/Delete Save Data" )]
#endif
    public static void DeleteSaveData()
    {
        PlayerPrefs.DeleteAll();
        System.IO.File.Delete( "allcards" );
    }
}
