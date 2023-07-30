using UnityEngine;
using UnityEngine.Tilemaps;

public class WallChunk :MonoBehaviour
{
    TilemapRenderer tilemapRenderer;

    public void SetSortingOrder(int order)
    {
        tilemapRenderer = GetComponentInChildren<TilemapRenderer>();
        tilemapRenderer.sortingOrder = order;
    }
}