using UnityEngine;
using System;

[RequireComponent( typeof( Collider2D ) )]
public class Draggable : MonoBehaviour
{
    public bool allowRotation;
    public bool flippable;
    public int rotationSnap;
    private Vector3 offset;
    private bool selected;

    void OnMouseDown()
    {
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x, Input.mousePosition.y, 0.0f ) );
        selected = true;
    }

    private void OnMouseUp()
    {
        selected = false;
    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3( Input.mousePosition.x, Input.mousePosition.y, 0.0f );
        Vector3 curPosition = Camera.main.ScreenToWorldPoint( curScreenPoint ) + offset;
        transform.position = curPosition;
    }

    private void Update()
    {
        if( selected )
        {
            if( Input.GetKeyDown( KeyCode.R ) || Mathf.Abs( Input.mouseScrollDelta.x ) > 0.001f ) 
            {
                gameObject.transform.Rotate( new Vector3( 0.0f, 0.0f, rotationSnap * Mathf.Sign( Input.mouseScrollDelta.x ) ) );
                offset = offset.RotateZ( rotationSnap );
                OnMouseDrag();
            }

            if( Input.GetKeyDown( KeyCode.F ) )
                foreach( var child in GetComponentsInChildren<TMPro.TextMeshProUGUI>( true ) )
                    child.gameObject.SetActive( !child.gameObject.activeSelf );
        }    
    }
}