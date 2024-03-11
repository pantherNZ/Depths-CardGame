using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoll : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI buttonLabel;
    [SerializeField] TMPro.TextMeshProUGUI rollLabel;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener( () =>
        {
            if( rollLabel.text.Length == 0 )
            {
                rollLabel.text = Random.Range( 1, 6 ).ToString();
                buttonLabel.text = "Clear";
            }
            else
            {
                rollLabel.text = string.Empty;
                buttonLabel.text = "Roll Dice";
            }
        } );
    }
}
