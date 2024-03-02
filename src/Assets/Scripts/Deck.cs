using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    const string equipmentDataPath = "Data/Equipment";
    const string utilityDataPath = "Data/Utility";
    const string resourcesDataPath = "Data/Resources";

    [SerializeField] GameObject equipmentCardPrefab;
    [SerializeField] GameObject utilityCardPrefab;
    [SerializeField] GameObject resourcesCardPrefab;

    List<GameObject> equipmentCards = new List<GameObject>();
    List<GameObject> utilityCards = new List<GameObject>();
    List<GameObject> resourcesCards = new List<GameObject>();

    string[] SplitData( string line )
    {
        var data = line.Split( ',' ).ToList();

        for( var i = 0; i < data.Count; ++i )
        {
            if( data[i].Length > 0 && data[i][0] == '"' )
            {
                int j = i + 1;
                var newStr = data[i];

                for( ; j < data.Count; ++j )
                {
                    newStr += "," + data[j];
                    if( data[j].Length > 0 && data[j][^1] == '"' )
                        break;
                }

                data.RemoveRange( i + 1, j - i );
                data[i] = newStr;
            }
        }

        return data.ToArray();
    }

    void Start()
    {
        var equipment = Resources.Load<TextAsset>( equipmentDataPath );
        var utility = Resources.Load<TextAsset>( utilityDataPath );
        var resources = Resources.Load<TextAsset>( resourcesDataPath );

        var gapBetweenDecks = 1.75f;

        foreach( var line in equipment.text.Split( '\n' )[1..] )
        {
            var data = SplitData( line );
            var name = data[0];

            if( data[1].Length < 1 || data[2].Length < 1 || data[3].Length < 1 )
                continue;

            var attack = int.Parse( data[1] );
            var defence = int.Parse( data[2] );
            var mining = int.Parse( data[3] );
            var ability = data[4];

            equipmentCards.Add( Instantiate( equipmentCardPrefab, transform.position.SetY( transform.position.y ), Quaternion.identity ) );
            var texts = equipmentCards.Back().GetComponentsInChildren<TMPro.TextMeshPro>();
            texts[0].text = name.Length > 0 ? name : "EQUIPMENT";
            texts[1].text = ability;
            texts[2].text = attack.ToString();
            texts[3].text = mining.ToString();
            texts[4].text = defence.ToString();
        }

        foreach( var line in utility.text.Split( '\n' )[1..] )
        {
            var data = SplitData( line );
            var name = data[0];
            var description = data[1];
            var count = int.Parse( data[2] );

            for( var i = 0; i < count; ++i )
            {
                utilityCards.Add( Instantiate( utilityCardPrefab, transform.position.SetY( transform.position.y + gapBetweenDecks ), Quaternion.identity ) );
                var texts = utilityCards.Back().GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = name.Length > 0 ? name : "UTILITY";
                texts[1].text = description;
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
                resourcesCards.Add( Instantiate( resourcesCardPrefab, transform.position.SetY( transform.position.y + gapBetweenDecks * 2.0f ), Quaternion.identity ) );
                var texts = resourcesCards.Back().GetComponentsInChildren<TMPro.TextMeshPro>();
                texts[0].text = gold;
                texts[1].text = name.Length > 0 ? name : "EQUIPMENT";
                texts[2].text = description;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
