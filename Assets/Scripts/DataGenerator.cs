using System.Security.AccessControl;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DataGenerator : MonoBehaviour


{

    public class GenData {
        public System.Action<int[,,]> OnComplete;
        public Vector3Int GenerationPoint;
    }

    private TerrainGenerator TerrainGeneratorInstance;
    private Queue<GenData> DataToGenerate;
    public bool Terminate;

    public void QueueDataToGenerate(GenData data) {
        DataToGenerate.Enqueue(data);
    }

    public DataGenerator(TerrainGenerator terrainGen) {
        TerrainGeneratorInstance = terrainGen;
        DataToGenerate = new Queue<GenData>();

        terrainGen.StartCoroutine(DataGenLoop());
    }

    public IEnumerator DataGenLoop() {
        while (Terminate == false)
        {
            if (DataToGenerate.Count > 0) {
                GenData gen = DataToGenerate.Dequeue();
                yield return TerrainGeneratorInstance.StartCoroutine(GenerateData(gen.GenerationPoint, gen.OnComplete));
            }
            yield return null;

        }
    }
    public IEnumerator GenerateData(Vector3Int offset, System.Action<int[,,]> callback){

Vector3Int ChunkSize = TerrainGenerator.ChunkSize;
Vector2 NoiseOffset = TerrainGeneratorInstance.NoiseOffset;
Vector2 NoiseScale = TerrainGeneratorInstance.NoiseScale;
float HeightIntensity = TerrainGeneratorInstance.HeightIntensity;
float HeightOffset = TerrainGeneratorInstance.HeightOffset;
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

    TerrainGenerator.WorldData.Add(offset, TempData);
    callback(TempData);

    }
}
