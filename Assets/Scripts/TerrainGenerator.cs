using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class TerrainGenerator : MonoBehaviour
{

    public static Dictionary<Vector3Int, int[,,]> WorldData;
    public static Dictionary<Vector2Int,GameObject> ActiveChunks;
    public static readonly Vector3Int ChunkSize = new Vector3Int(16, 256, 16);

    [SerializeField] private TextureLoader textureLoaderInstance;
    [SerializeField] private Material ChunkMaterial;
    [Space]

    public Vector2 NoiseScale = Vector2.one;
    public Vector2 NoiseOffset = Vector2.zero;
    [Space]
    public int HeightOffset = 60;
    public float HeightIntensity = 5f;

    public ChunkMeshCreator meshCreator;

    void Start()
    {
        WorldData = new Dictionary<Vector3Int, int[,,]>();
        ActiveChunks = new Dictionary<Vector2Int, GameObject>();
        meshCreator = new ChunkMeshCreator(textureLoaderInstance);

    }

    public IEnumerator CreateChunk(Vector2Int ChunkCoord) {
        Vector3Int pos = new Vector3Int(ChunkCoord.x, 0, ChunkCoord.y);

        string chunkName = $"Chunk {ChunkCoord.x} {ChunkCoord.y}";
        GameObject newChunk = new GameObject(chunkName, new System.Type[] {
            typeof(MeshRenderer),
            typeof(MeshFilter),
            typeof(MeshCollider)
        });
newChunk.transform.position = new Vector3(ChunkCoord.x * 16, 0f, ChunkCoord.y * 16);
        ActiveChunks.Add(ChunkCoord, newChunk);

        int[,,] dataToApply = WorldData.ContainsKey(pos) ? WorldData[pos] : null;
        if (dataToApply == null) {
            StartCoroutine(GenerateData(pos, x=> dataToApply = x));
            yield return new WaitUntil(() => dataToApply != null);
        }

        Mesh meshToUse = null;

        StartCoroutine(meshCreator.CreateMeshFromData(dataToApply, x => meshToUse = x));
        yield return new WaitUntil(() => meshToUse != null);

        if (newChunk != null) {
            MeshRenderer newChunkRenderer = newChunk.GetComponent<MeshRenderer>();
            MeshFilter newChunkFilter = newChunk.GetComponent<MeshFilter>();
            MeshCollider newChunkCollider = newChunk.GetComponent<MeshCollider>();

            newChunkFilter.mesh = meshToUse;
            newChunkRenderer.material = ChunkMaterial;
            newChunkCollider.sharedMesh = newChunkFilter.mesh;
        }
    }
public IEnumerator GenerateData(Vector3Int offset, System.Action<int[,,]> callback){
    int [,,] TempData = new int[ChunkSize.x, ChunkSize.y, ChunkSize.z];
    Task t = Task.Factory.StartNew(delegate {
         for(int x = 0; x < ChunkSize.x; x++)
        {
            for (int z = 0; z < ChunkSize.z; z++)
            {
                float PerlinCoordX = NoiseOffset.x + (x + (offset.x * 16f)) / (float)ChunkSize.x * NoiseScale.x;
                float PerlinCoordY = NoiseOffset.y + (z + (offset.z * 16f)) / (float)ChunkSize.z * NoiseScale.y;
                int HeightGen = Mathf.RoundToInt(Mathf.PerlinNoise(PerlinCoordX, PerlinCoordY) * HeightIntensity + HeightOffset);


                // PLAINS BIOME IN MC
                for (int y = HeightGen; y >= 0; y--)
                {
                    int BlockTypeToAssign = 0;


                    // GRASS
                    if (y == HeightGen) BlockTypeToAssign = 1;

                    // DIRT
                    if (y < HeightGen && y > HeightGen - 4) BlockTypeToAssign = 1;


                    // STONE
                    if (y <= HeightGen - 4 && y > 0) BlockTypeToAssign = 2;

                    // BEDROCK
                    if (y == 0) BlockTypeToAssign = 3;

                    TempData[x, y, z] = BlockTypeToAssign;
                }
            }

        }

    });

    yield return new WaitUntil(() => t.IsCompleted || t.IsCanceled);

    if (t.Exception != null) {
        Debug.LogError(t.Exception);
    }

    WorldData.Add(offset, TempData);
    callback(TempData);

    }
}
