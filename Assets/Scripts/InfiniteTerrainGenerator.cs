using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrainGenerator : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private int RenderDistance;

    private TerrainGenerator GeneratorInstance;
    private List<Vector2Int> CoordsToRemove;



    void Start()
    {
        GeneratorInstance = GetComponent<TerrainGenerator>();
        CoordsToRemove = new List<Vector2Int>();

    }

    void Update()
    {
        int plrChunkX = (int)Player.position.x / TerrainGenerator.ChunkSize.x;
        int plrChunky = (int)Player.position.z / TerrainGenerator.ChunkSize.z;
        CoordsToRemove.Clear();

        foreach (KeyValuePair<Vector2Int, GameObject> activeChunk in TerrainGenerator.ActiveChunks)
        {
            // if (Chunk.Key.x < plrChunkX - RenderDistance || Chunk.Key.x > plrChunkX + RenderDistance || Chunk.Key.y < plrChunky - RenderDistance || Chunk.Key.y > plrChunky + RenderDistance) {
            CoordsToRemove.Add(activeChunk.Key);
            // }
        }

        for (int x = plrChunkX - RenderDistance; x <= plrChunkX + RenderDistance; x++)
        {
            for (int y = plrChunky - RenderDistance; y <= plrChunky + RenderDistance; y++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);
                if (!TerrainGenerator.ActiveChunks.ContainsKey(chunkCoord))
                {
                    StartCoroutine(GeneratorInstance.CreateChunk(chunkCoord));
                }
                CoordsToRemove.Remove(chunkCoord);

            }
        }
        foreach (Vector2Int coord in CoordsToRemove)
        {

            // GameObject chunkToDelete = TerrainGenerator.ActiveChunks[coord];
            TerrainGenerator.ActiveChunks.Remove(coord);
            // Destroy(chunkToDelete);
        }
    }
}
