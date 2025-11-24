using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering; // kept if future grouping needed

[RequireComponent(typeof(Collider2D))]
public class Draggable : MonoBehaviour
{
    public bool allowRotation;
    public bool flippable;
    public int rotationSnap;
    private Vector3 offset;
    private bool selected;
    private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    private List<TMPro.TextMeshPro> textMeshRenderers = new List<TMPro.TextMeshPro>();
    private List<int> initialRelativeOrder = new List<int>();
    private List<int> textRelativeOrder = new List<int>();

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true).ToList();
        textMeshRenderers = GetComponentsInChildren<TMPro.TextMeshPro>(true).ToList();

        if (spriteRenderers.Count > 0 || textMeshRenderers.Count > 0)
        {
            // Capture relative ordering for sprites
            if (spriteRenderers.Count > 0)
            {
                spriteRenderers = spriteRenderers.OrderBy(r => r.sortingOrder).ToList();
                int baseOrder = spriteRenderers[0].sortingOrder;
                initialRelativeOrder = spriteRenderers.Select(r => r.sortingOrder - baseOrder).ToList();
            }

            // Capture relative ordering for TextMeshPro
            if (textMeshRenderers.Count > 0)
            {
                textMeshRenderers = textMeshRenderers.OrderBy(t => t.sortingOrder).ToList();
                int baseOrder = textMeshRenderers[0].sortingOrder;
                textRelativeOrder = textMeshRenderers.Select(t => t.sortingOrder - baseOrder).ToList();
            }

            // If all orders are identical (likely freshly instantiated prefab) assign sequential unique orders now
            int spriteBase = spriteRenderers.Count > 0 ? spriteRenderers[0].sortingOrder : 0;
            int textBase = textMeshRenderers.Count > 0 ? textMeshRenderers[0].sortingOrder : 0;
            bool allSame = spriteRenderers.All(r => r.sortingOrder == spriteBase) &&
                          textMeshRenderers.All(t => t.sortingOrder == textBase);

            if (allSame)
            {
                int maxExisting = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                    .Where(r => !spriteRenderers.Contains(r))
                    .Select(r => r.sortingOrder)
                    .DefaultIfEmpty(0)
                    .Max();

                int maxTextExisting = FindObjectsByType<TMPro.TextMeshPro>(FindObjectsSortMode.None)
                    .Where(t => !textMeshRenderers.Contains(t))
                    .Select(t => t.sortingOrder)
                    .DefaultIfEmpty(0)
                    .Max();

                int newOrder = Mathf.Max(maxExisting, maxTextExisting) + 1;

                for (int i = 0; i < spriteRenderers.Count; ++i)
                    spriteRenderers[i].sortingOrder = newOrder + i;

                for (int i = 0; i < textMeshRenderers.Count; ++i)
                    textMeshRenderers[i].sortingOrder = newOrder + spriteRenderers.Count + i;
            }
        }
        SetupChildCanvases();
    }

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
        if (spriteRenderers.Count == 0 && textMeshRenderers.Count == 0)
            return;

        // Determine maximum sorting order among all other renderers
        int maxSpriteOrder = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Where(r => !spriteRenderers.Contains(r))
            .Select(r => r.sortingOrder)
            .DefaultIfEmpty(0)
            .Max();

        int maxTextOrder = FindObjectsByType<TMPro.TextMeshPro>(FindObjectsSortMode.None)
            .Where(t => !textMeshRenderers.Contains(t))
            .Select(t => t.sortingOrder)
            .DefaultIfEmpty(0)
            .Max();

        int newBase = Mathf.Max(maxSpriteOrder, maxTextOrder) + 1;

        for (int i = 0; i < spriteRenderers.Count; ++i)
        {
            spriteRenderers[i].sortingOrder = newBase + initialRelativeOrder[i];
        }

        for (int i = 0; i < textMeshRenderers.Count; ++i)
        {
            textMeshRenderers[i].sortingOrder = newBase + (spriteRenderers.Count > 0 ? spriteRenderers.Max(r => r.sortingOrder) - newBase + 1 : 0) + textRelativeOrder[i];
        }

        transform.position = transform.position.SetZ(0.0f);

        SetupChildCanvases();
    }

    void SetupChildCanvases()
    {
        var canvases = GetComponentsInChildren<Canvas>(true);
        if (canvases.Length == 0 || (spriteRenderers.Count == 0 && textMeshRenderers.Count == 0))
            return;

        int frontOrder = 0;
        if (spriteRenderers.Count > 0)
            frontOrder = Mathf.Max(frontOrder, spriteRenderers.Max(r => r.sortingOrder));
        if (textMeshRenderers.Count > 0)
            frontOrder = Mathf.Max(frontOrder, textMeshRenderers.Max(t => t.sortingOrder));

        foreach (var canvas in canvases)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = frontOrder + 1; // ensure UI above top sprite/text
        }
    }
}