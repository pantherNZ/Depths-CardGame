using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float translationSpeed = 1.0f;
    [SerializeField] float zoomSpeed = 1.0f;

    void Update()
    {
        if( Application.isFocused )
        {
            if( Input.GetMouseButton( 1 ) )
            {
                var difference = Camera.main.ScreenToWorldPoint( Input.mousePosition ) - Camera.main.ScreenToWorldPoint( Input.mousePosition - Input.mousePositionDelta );
                transform.Translate( difference * -translationSpeed );
            }

            Camera.main.orthographicSize -= Input.mouseScrollDelta.y * zoomSpeed;
        }
    }
}
