using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using System.Threading.Tasks;

public class ChunkMeshCreator : MonoBehaviour
{

    public class FaceData {
        public FaceData(Vector3[] verts, int[] tris, int[] uvindexorder)
    {
        UVIndexOrder = uvindexorder;
            Vertices = verts;
            Indices = tris;
    }

        public Vector3[] Vertices;
        public int[] Indices;

        public int[] UVIndexOrder;
    }

    #region FaceData

    static readonly Vector3Int[] CheckDirections = new Vector3Int[]
    {
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.forward,
        Vector3Int.back
    };

    static readonly Vector3[] RightFace = new Vector3[]
    {
        new Vector3(.5f, -.5f, -.5f),
        new Vector3(.5f, -.5f, .5f),
        new Vector3(.5f, .5f, .5f),
        new Vector3(.5f, .5f, -.5f)
    };

    static readonly int[] RightTris = new int[]
    {
        0,2,1,0,3,2
    };

    static readonly Vector3[] LeftFace = new Vector3[]
    {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f, -.5f, .5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(-.5f, .5f, -.5f)
    };

    static readonly int[] LeftTris = new int[]
    {
        0,1,2,0,2,3
    };

    static readonly Vector3[] UpFace = new Vector3[]
    {
        new Vector3(-.5f, .5f, -.5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(.5f, .5f, .5f),
        new Vector3(.5f, .5f, -.5f)
    };

    static readonly int[] UpTris = new int[]
    {
        0,1,2,0,2,3
    };

    static readonly Vector3[] DownFace = new Vector3[]
    {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f, -.5f, .5f),
        new Vector3(.5f, -.5f, .5f),
        new Vector3(.5f, -.5f, -.5f)
    };

    static readonly int[] DownTris = new int[]
    {
        0,2,1,0,3,2
    };

    static readonly Vector3[] ForwardFace = new Vector3[]
    {
        new Vector3(-.5f, -.5f, .5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(.5f, .5f, .5f),
        new Vector3(.5f, -.5f, .5f)
    };

    static readonly int[] ForwardTris = new int[]
    {
        0,2,1,0,3,2
    };

    static readonly Vector3[] BackFace = new Vector3[]
    {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f, .5f, -.5f),
        new Vector3(.5f, .5f, -.5f),
        new Vector3(.5f, -.5f, -.5f)
    };

    static readonly int[] BackTris = new int[]
    {
        0,1,2,0,2,3
    };

    #endregion

#region FaceUVData

static readonly int[] XUVOrder = new int[]
{
	 2, 3, 1, 0
};

static readonly int[] YUVOrder = new int[]
{
	  0, 1, 3, 2
};


static readonly int[] ZUVOrder = new int[]
{
	  3, 1, 0, 2
};


#endregion
    private Dictionary<Vector3Int, FaceData> CubeFaces = new Dictionary<Vector3Int, FaceData>();
private TextureLoader TextureLoaderInstance;
    public ChunkMeshCreator(TextureLoader textureLoaderInstance)
    {
        TextureLoaderInstance = textureLoaderInstance;
        CubeFaces = new Dictionary<Vector3Int, FaceData>();

        for (int i = 0; i < CheckDirections.Length; i++)
        {
            if (CheckDirections[i] == Vector3Int.up)
            {
                CubeFaces.Add(CheckDirections[i], new FaceData(UpFace, UpTris, YUVOrder));
            } else if (CheckDirections[i] == Vector3Int.down)
            {
                CubeFaces.Add(CheckDirections[i], new FaceData(DownFace, DownTris, YUVOrder));
            } else if (CheckDirections[i] == Vector3Int.forward)
            {
                CubeFaces.Add(CheckDirections[i], new FaceData(ForwardFace, ForwardTris, ZUVOrder));
            } else if (CheckDirections[i] == Vector3Int.back)
            {
                CubeFaces.Add(CheckDirections[i], new FaceData(BackFace, BackTris, ZUVOrder));
            } else if (CheckDirections[i] == Vector3Int.left)
            {
                CubeFaces.Add(CheckDirections[i], new FaceData(LeftFace, LeftTris, XUVOrder));
            } else if (CheckDirections[i] == Vector3Int.right)
            {
                CubeFaces.Add(CheckDirections[i], new FaceData(RightFace, RightTris, XUVOrder));
            }
                }
    }

    public IEnumerator CreateMeshFromData(int[,,] Data, System.Action<Mesh> callback)
    {
        List<Vector3> Vertices = new List<Vector3>();
        List<int> Indices = new List<int>();
        List<Vector2> UVs = new List<Vector2>();
        Mesh m = new Mesh();

        Task t = Task.Factory.StartNew(delegate {
for (int x =0; x < TerrainGenerator.ChunkSize.x; x++)
        {
            for (int y = 0; y < TerrainGenerator.ChunkSize.y; y++)
            {
                for (int z = 0; z < TerrainGenerator.ChunkSize.z; z++)
                {
                    Vector3Int BlockPos = new Vector3Int(x, y, z);
                    for (int i = 0; i < CheckDirections.Length; i++) {
                        Vector3Int BlockToCheck = BlockPos + CheckDirections[i];
                        try
                        {
                            if (Data[BlockToCheck.x, BlockToCheck.y, BlockToCheck.z] == 0)
                            {
                                if (Data[BlockPos.x, BlockPos.y, BlockPos.z] != 0)
                                {
                                    int CurrentBlockID = Data[BlockPos.x, BlockPos.y, BlockPos.z];
                                    TextureLoader.CubeTexture TextureToApply = TextureLoaderInstance.Textures[CurrentBlockID];
                                    FaceData FaceToApply = CubeFaces[CheckDirections[i]];

                                    foreach (Vector3 vert in FaceToApply.Vertices)
                                    {
                                        Vertices.Add(new Vector3(x, y, z) + vert);
                                    }

                                    foreach (int tri in FaceToApply.Indices)
                                    {
                                        Indices.Add(Vertices.Count - 4 + tri);
                                    }

                                    Vector2[] UVsToAdd = TextureToApply.GetUVsAtDirectionT(CheckDirections[i]);

                                    foreach (int UVIndex in FaceToApply.UVIndexOrder)
                                    {
                                        UVs.Add(UVsToAdd[UVIndex]);
                                    }
                                }
                            }
                        } catch (System.Exception)
                        {

                                if (Data[BlockPos.x, BlockPos.y, BlockPos.z] != 0)
                                {
                                    int CurrentBlockID = Data[BlockPos.x, BlockPos.y, BlockPos.z];
                                    TextureLoader.CubeTexture TextureToApply = TextureLoaderInstance.Textures[CurrentBlockID];
                                    FaceData FaceToApply = CubeFaces[CheckDirections[i]];

                                    foreach (Vector3 vert in FaceToApply.Vertices)
                                    {
                                        Vertices.Add(new Vector3(x, y, z) + vert);
                                    }

                                    foreach (int tri in FaceToApply.Indices)
                                    {
                                        Indices.Add(Vertices.Count - 4 + tri);
                                    }

                                    Vector2[] UVsToAdd = TextureToApply.GetUVsAtDirectionT(CheckDirections[i]);

                                    foreach (int UVIndex in FaceToApply.UVIndexOrder)
                                    {
                                        UVs.Add(UVsToAdd[UVIndex]);
                                    }
                                }
                            }

                    }
                }
            }

        }

        });


       yield return new WaitUntil(() => t.IsCompleted || t.IsCanceled);

       if (t.Exception != null) {
        Debug.LogError(t.Exception);
       }
        m.SetVertices(Vertices);
        m.SetIndices(Indices, MeshTopology.Triangles, 0);
        m.SetUVs(0, UVs);

        m.RecalculateBounds();
        m.RecalculateTangents();
        m.RecalculateNormals();
        callback(m);

}
}
