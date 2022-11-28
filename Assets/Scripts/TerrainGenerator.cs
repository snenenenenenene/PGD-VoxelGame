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
    private DataGenerator dataCreator;

    void Start()
    {
        WorldData = new Dictionary<Vector3Int, int[,,]>();
        ActiveChunks = new Dictionary<Vector2Int, GameObject>();
        meshCreator = new ChunkMeshCreator(textureLoaderInstance, this);
        dataCreator = new DataGenerator(this);

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
        Mesh meshToUse = null;

        if (dataToApply == null) {
            dataCreator.QueueDataToGenerate(new DataGenerator.GenData {
                GenerationPoint = pos,
                OnComplete = x => dataToApply= x
            });
            yield return new WaitUntil(() => dataToApply != null);
        }

        meshCreator.QueueDataToDraw(new ChunkMeshCreator.CreateMesh {
            DataToDraw = dataToApply,
            OnComplete = x => meshToUse = x
        });

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
}
