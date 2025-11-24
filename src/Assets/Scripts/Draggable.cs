using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class Draggable : MonoBehaviour
{
    public bool allowRotation;
    public bool flippable;
    public int rotationSnap;
    private Vector3 offset;
    private bool selected;
    private static SpriteRenderer[] allRenderers;

    void OnMouseDown()
    {
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f));
        selected = true;

        // Bring this card to the front
        BringToFront();
    }

    private void OnMouseUp()
    {
        selected = false;
    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = curPosition;
    }

    private void Update()
    {
        if (selected)
        {
            if (Input.GetKeyDown(KeyCode.R) || Mathf.Abs(Input.mouseScrollDelta.x) > 0.001f)
            {
                gameObject.transform.Rotate(new Vector3(0.0f, 0.0f, rotationSnap * Mathf.Sign(Input.mouseScrollDelta.x)));
                offset = offset.RotateZ(rotationSnap);

                foreach (Transform child in gameObject.transform)
                    child.transform.rotation = Quaternion.identity;

                OnMouseDrag();
            }

            if (Input.GetKeyDown(KeyCode.F))
                foreach (var child in GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                    child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }

    void BringToFront()
    {
        // Cache all renderers once at startup
        if (allRenderers == null)
            allRenderers = FindObjectsOfType<SpriteRenderer>();

        // Find the highest sorting order among all cards
        int maxOrder = 0;
        foreach (var renderer in allRenderers)
        {
            if (renderer.sortingOrder > maxOrder)
                maxOrder = renderer.sortingOrder;
        }

        // Set this card's sorting order above all others
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
            renderer.sortingOrder = maxOrder + 1;
    }
}