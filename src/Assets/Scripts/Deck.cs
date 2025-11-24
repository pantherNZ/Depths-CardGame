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
    const string tilesDataPath = "Data/Depths - Card Game - Tiles";

    [SerializeField] int numPlayers = 2;
    [SerializeField] float hexSize;
    [SerializeField] GameObject equipmentCardPrefab;
    [SerializeField] GameObject utilityCardPrefab;
    [SerializeField] GameObject resourcesCardPrefab;
    [SerializeField] GameObject monstersCardPrefab;
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

    readonly List<GameObject>[] playerDeck = new List<GameObject>[2] { new(), new() };
    readonly List<List<GameObject>> startingCards = new List<List<GameObject>>();
    readonly List<GameObject> equipmentCards = new List<GameObject>();
    readonly List<GameObject> utilityCards = new List<GameObject>();
    readonly List<GameObject> resourcesCards = new List<GameObject>();
    readonly List<GameObject> monsterCards = new List<GameObject>();
    readonly List<TileGroup> tileGroups = new();

    string[] SplitData(string line)
    {
        var data = line.Split(',').ToList();

        for (var i = 0; i < data.Count; ++i)
        {
            if (data[i].Length > 0 && data[i][0] == '"')
            {
                int j = i + 1;
                var newStr = data[i][1..];

                for (; j < data.Count; ++j)
                {
                    newStr += "," + data[j];
                    if (data[j].Length > 0 && data[j][^1] == '"')
                        break;
                }

                data.RemoveRange(i + 1, Mathf.Min(data.Count - (i + 1), j - i));
                data[i] = newStr.Trim()[..^1];
            }
        }

        return data.ToArray();
    }

    Dictionary<string, int> ParseHeaders(string headerLine)
    {
        var headers = SplitData(headerLine);
        var headerMap = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++)
        {
            headerMap[headers[i]] = i;
        }
        return headerMap;
    }

    string GetColumnValue(string[] data, Dictionary<string, int> headers, string columnName, string defaultValue = "")
    {
        if (headers.TryGetValue(columnName, out int index) && index < data.Length)
            return data[index];
        return defaultValue;
    }

    void InitiateDeck(Vector3 position, List<GameObject> deck)
    {
        deck.RandomShuffle();
        for (var i = 0; i < deck.Count; ++i)
            deck[i].transform.position = position.SetZ(i);
    }

    Vector2 HexToPixel(Vector2Int coord)
    {
        var x = hexSize * 3 / 2 * coord.x;
        var y = hexSize * Mathf.Sqrt(3) * (coord.y + 0.5f * (coord.x & 1));
        return new Vector2(x, y) / 1.95f;
    }

    string HTMLFormat(string str)
    {
        if (str == "EMPTY")
            return string.Empty;
        return str.Replace("<BR>", "\n");
    }

    void Start()
    {
        var equipment = Resources.Load<TextAsset>(equipmentDataPath);
        var utility = Resources.Load<TextAsset>(utilityDataPath);
        var resources = Resources.Load<TextAsset>(resourcesDataPath);
        var monsters = Resources.Load<TextAsset>(monstersDataPath);
        var tiles = Resources.Load<TextAsset>(tilesDataPath);

        var equipmentLines = equipment.text.Split('\n');
        var equipmentHeaders = ParseHeaders(equipmentLines[0]);
        foreach (var line in equipmentLines[1..].RandomShuffle())
        {
            var data = SplitData(line);
            var name = GetColumnValue(data, equipmentHeaders, "Name");
            var attackStr = GetColumnValue(data, equipmentHeaders, "Attack");
            var defenceStr = GetColumnValue(data, equipmentHeaders, "Defence");

            if (name.Length < 1 || attackStr.Length < 1 || defenceStr.Length < 1)
                continue;

            var costStr = GetColumnValue(data, equipmentHeaders, "Cost");
            var cost = costStr.Length > 0 ? int.Parse(costStr) : 0;
            var attack = int.Parse(attackStr);
            var defence = int.Parse(defenceStr);
            var mining = int.Parse(GetColumnValue(data, equipmentHeaders, "Mining"));
            var ability = HTMLFormat(GetColumnValue(data, equipmentHeaders, "Ability"));

            var newCard = Instantiate(equipmentCardPrefab, transform.position, Quaternion.identity);
            newCard.name = name;
            var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
            texts[0].text = name;
            texts[1].text = ability;
            texts[2].text = attack.ToString();
            texts[3].text = mining.ToString();
            texts[4].text = defence.ToString();
            texts[5].text = cost.ToString();
            equipmentCards.Add(newCard);
        }

        var utilityLines = utility.text.Split('\n');
        var utilityHeaders = ParseHeaders(utilityLines[0]);
        foreach (var line in utilityLines[1..].RandomShuffle())
        {
            var data = SplitData(line);
            var name = GetColumnValue(data, utilityHeaders, "Name");
            var description = HTMLFormat(GetColumnValue(data, utilityHeaders, "Description"));
            if (!int.TryParse(GetColumnValue(data, utilityHeaders, "Cost"), out var cost))
                continue;
            if (!int.TryParse(GetColumnValue(data, utilityHeaders, "Deck Count"), out var count))
                continue;
            if (!int.TryParse(GetColumnValue(data, utilityHeaders, "Extra deck (always available)"), out var playerDeckCount))
                playerDeckCount = 0;
            if (!int.TryParse(GetColumnValue(data, utilityHeaders, "Starting Count"), out var startingDeckCount))
                startingDeckCount = 0;

            if (startingDeckCount > 0)
                startingCards.Add(new List<GameObject>());

            for (var i = 0; i < count + playerDeckCount + startingDeckCount; ++i)
            {
                GameObject Create(string cardName)
                {
                    var newCard = Instantiate(utilityCardPrefab, Vector3.zero, Quaternion.identity);
                    newCard.name = cardName;
                    var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                    texts[0].text = cardName;
                    texts[1].text = description;
                    texts[2].text = cost.ToString();
                    return newCard;
                }

                if (i < count)
                {
                    utilityCards.Add(Create(name));
                }
                else if (i >= count && i < count + playerDeckCount)
                {
                    for (int j = 0; j < numPlayers; ++j)
                        playerDeck[j].Add(Create(name + j));
                }
                else
                {
                    startingCards.Back().Add(Create(name));
                }
            }
        }

        var resourcesLines = resources.text.Split('\n');
        var resourcesHeaders = ParseHeaders(resourcesLines[0]);
        foreach (var line in resourcesLines[1..])
        {
            var data = SplitData(line);
            var name = GetColumnValue(data, resourcesHeaders, "Name");
            if (!int.TryParse(GetColumnValue(data, resourcesHeaders, "Cost"), out var cost))
                continue;
            if (!int.TryParse(GetColumnValue(data, resourcesHeaders, "Gold"), out var gold))
                continue;
            var description = HTMLFormat(GetColumnValue(data, resourcesHeaders, "Description"));
            var count = int.Parse(GetColumnValue(data, resourcesHeaders, "Deck Count"));
            if (!int.TryParse(GetColumnValue(data, resourcesHeaders, "Extra deck (always available)"), out var playerDeckCount))
                playerDeckCount = 0;

            for (var i = 0; i < count + playerDeckCount; ++i)
            {
                GameObject Create(string cardName)
                {
                    var newCard = Instantiate(resourcesCardPrefab, Vector3.zero, Quaternion.identity);
                    newCard.name = cardName;
                    var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                    texts[0].text = gold.ToString();
                    texts[1].text = cardName;
                    texts[2].text = description;
                    texts[3].text = cost.ToString();
                    return newCard;
                }

                if (i < count)
                {
                    resourcesCards.Add(Create(name));
                }
                else if (i >= count && i < count + playerDeckCount)
                {
                    for (int j = 0; j < numPlayers; ++j)
                        playerDeck[j].Add(Create(name + j));
                }
            }
        }

        var monstersLines = monsters.text.Split('\n');
        var monstersHeaders = ParseHeaders(monstersLines[0]);
        foreach (var line in monstersLines[1..].RandomShuffle())
        {
            var data = SplitData(line);

            var name = GetColumnValue(data, monstersHeaders, "Name");
            if (name.Length < 1)
                continue;

            var description = HTMLFormat(GetColumnValue(data, monstersHeaders, "Description"));
            var reward = HTMLFormat(GetColumnValue(data, monstersHeaders, "Reward"));
            if (!int.TryParse(GetColumnValue(data, monstersHeaders, "Defence"), out var defence))
                continue;
            if (!int.TryParse(GetColumnValue(data, monstersHeaders, "Deck Count"), out var count))
                continue;
            var onLose = GetColumnValue(data, monstersHeaders, "On Lose");

            for (var i = 0; i < count; ++i)
            {
                var newCard = Instantiate(monstersCardPrefab, Vector3.zero, Quaternion.identity);
                newCard.name = name;
                var texts = newCard.GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = name;
                texts[1].text = description + "\n\nWin: " + reward + "\nLose: " + onLose;
                texts[2].text = defence.ToString();
                monsterCards.Add(newCard);
            }
        }

        TileGroup tileGroup = null;

        var tilesLines = tiles.text.Split('\n');
        var tilesHeaders = ParseHeaders(tilesLines[0]);
        foreach (var (idx, line) in tilesLines[1..].Enumerate())
        {
            var data = SplitData(line);
            var group = GetColumnValue(data, tilesHeaders, "Group") + idx;
            var name = GetColumnValue(data, tilesHeaders, "Name");
            var description = HTMLFormat(GetColumnValue(data, tilesHeaders, "Description"));
            var coordsStr = GetColumnValue(data, tilesHeaders, "Coords").Split(',');

            if (name.Length == 0)
            {
                if (tileGroup != null)
                {
                    tileGroups.Add(tileGroup);
                    tileGroup.obj.transform.Rotate(0.0f, 0.0f, 60.0f * Random.Range(0, 5));
                    foreach (Transform child in tileGroup.obj.transform)
                        child.transform.rotation = Quaternion.identity;
                }
                tileGroup = null;
                continue;
            }

            if (tileGroup == null)
            {
                tileGroup = new TileGroup();
                tileGroup.name = group;
                tileGroup.obj = Instantiate(tilePrefab, new Vector2(tileGroups.Count * 2.0f, 0.0f), Quaternion.identity);

                foreach (Transform child in tileGroup.obj.transform)
                    Destroy(child.gameObject);
            }

            var coords = new Vector2Int(int.Parse(coordsStr[0]), int.Parse(coordsStr[1]));
            tileGroup.tiles.Add(new Tile()
            {
                name = name,
                description = description,
                coords = coords,
            });

            var offset = HexToPixel(coords);
            var newTile = Instantiate(tilePrefab, offset + new Vector2(tileGroups.Count * 2.0f, 0.0f), Quaternion.identity, tileGroup.obj.transform);
            Destroy(newTile.GetComponent<Draggable>());
            tileGroup.obj.AddComponent<CircleCollider2D>().offset = newTile.transform.localPosition;
            Destroy(newTile.GetComponent<Collider2D>());

            var texts = newTile.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            texts[0].text = name;
            texts[1].text = description;
        }

        var gapBetweenDecksX = 1.2f;
        var gapBetweenDecksY = 1.75f;

        var mainDeck = equipmentCards;
        mainDeck.InsertRange(0, utilityCards);
        mainDeck.InsertRange(0, resourcesCards);

        InitiateDeck(transform.position.SetY(transform.position.y + gapBetweenDecksY), mainDeck);

        var sideDeck = monsterCards;

        InitiateDeck(transform.position.SetY(transform.position.y + gapBetweenDecksY * 2.0f), sideDeck);

        foreach (var (idx, deck) in startingCards.Enumerate())
            InitiateDeck(transform.position.SetX(transform.position.x + gapBetweenDecksX * idx), deck);

        foreach (var (idx, deck) in playerDeck.Enumerate())
            InitiateDeck(transform.position.SetX(transform.position.x + 10.0f + gapBetweenDecksX * idx), deck);

        try
        {
            var bytes = File.ReadAllBytes("allcards.save");
            using var memoryStream = new MemoryStream(bytes, writable: false);
            using var reader = new BinaryReader(memoryStream);

            LoadCards(equipmentCards, reader);
            LoadCards(utilityCards, reader);
            LoadCards(resourcesCards, reader);
            LoadCards(monsterCards, reader);
            foreach (var deck in startingCards)
                LoadCards(deck, reader);
            foreach (var deck in playerDeck)
                LoadCards(deck, reader);

            var groupCount = reader.ReadInt32();
            for (int i = 0; i < groupCount; ++i)
            {
                var name = reader.ReadString();
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                var z = reader.ReadSingle();
                var rot = reader.ReadSingle();
                var group = tileGroups.FirstOrDefault(g => g.name == name);
                if (group != null)
                {
                    group.obj.transform.position = new Vector3(x, y, z);
                    group.obj.transform.rotation = Quaternion.Euler(0.0f, 0.0f, rot);
                    foreach (Transform child in group.obj.transform)
                        child.transform.rotation = Quaternion.identity;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("No save data found: " + e);
        }

        StartCoroutine(SaveLoop());
    }

    void WriteCards(List<GameObject> cards, BinaryWriter writer)
    {
        writer.Write(cards.Count);
        foreach (var card in cards)
        {
            writer.Write(card.name);
            writer.Write(card.transform.position.x);
            writer.Write(card.transform.position.y);
            writer.Write(card.transform.position.z);
        }
    }

    void LoadCards(List<GameObject> cards, BinaryReader reader)
    {
        var save = reader != null ? ReadCards(reader) : null;
        if (save != null)
        {
            foreach (var card in cards)
            {
                if (save.TryGetValue(card.name, out var pos))
                {
                    card.transform.position = pos[0];
                    pos.RemoveAt(0);
                }
            }
        }
    }

    Dictionary<string, List<Vector3>> ReadCards(BinaryReader reader)
    {
        var results = new Dictionary<string, List<Vector3>>();
        var count = reader.ReadInt32();
        for (int i = 0; i < count; ++i)
        {
            var name = reader.ReadString();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            results.GetOrAdd(name).Add(new Vector3(x, y, z));
        }
        return results;
    }

    // Update is called once per frame
    IEnumerator SaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5.0f);
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
        using var writer = new BinaryWriter(memoryStream);

        WriteCards(equipmentCards, writer);
        WriteCards(utilityCards, writer);
        WriteCards(resourcesCards, writer);
        WriteCards(monsterCards, writer);
        foreach (var deck in startingCards)
            WriteCards(deck, writer);
        foreach (var deck in playerDeck)
            WriteCards(deck, writer);

        writer.Write(tileGroups.Count);
        foreach (var group in tileGroups)
        {
            writer.Write(group.name);
            writer.Write(group.obj.transform.position.x);
            writer.Write(group.obj.transform.position.y);
            writer.Write(group.obj.transform.position.z);
            writer.Write(group.obj.transform.rotation.eulerAngles.z);
        }

        var content = memoryStream.ToArray();
        System.IO.File.WriteAllBytes("allcards.save", content);
    }

#if UNITY_EDITOR
    [MenuItem( "Scripts/Delete Save Data" )]
#endif
    public static void DeleteSaveData()
    {
        PlayerPrefs.DeleteAll();
        System.IO.File.Delete("allcards.save");
    }
}
