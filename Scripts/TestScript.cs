using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public Color heightColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    public Material cubeMaterial;
    public float seed = 0.1f;
    public Camera playerCamera;
    public int renderDistance = 3;
    public bool debugRender;
    public bool enableRenderDistance = false;
    public bool regenerateHeightmap = false;
    public bool enableColorHeightmap = false;
    public bool scrollTerrain = false;
    public float amplitude = 5.0f;
    public float frequency = 1.0f;
    public int heightVariation = 1;

    static public int mapSize = 100;
    private bool terrainGenerated = false;
    private float highestNoise = 0;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    void GenerateTerrain()
    {
        highestNoise = 0;
        vertices.Clear();
        triangles.Clear();

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                float noise = Mathf.PerlinNoise((i + seed) * frequency, (j + seed) * frequency) * amplitude;
                float height = Mathf.Round(noise * heightVariation) / heightVariation;

                Vector3 position = new Vector3(i, height, j);
                AddCubeToMesh(position);

                if (noise > highestNoise)
                {
                    highestNoise = noise;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = cubeMaterial;
    }

    void AddCubeToMesh(Vector3 position)
    {
        int vertexOffset = vertices.Count;

        // Add cube vertices with the specified position offset
        vertices.Add(position + new Vector3(0, 0, 0)); // Bottom-left-back (0)
        vertices.Add(position + new Vector3(1, 0, 0)); // Bottom-right-back (1)
        vertices.Add(position + new Vector3(0, 1, 0)); // Top-left-back (2)
        vertices.Add(position + new Vector3(1, 1, 0)); // Top-right-back (3)
        vertices.Add(position + new Vector3(0, 0, 1)); // Bottom-left-front (4)
        vertices.Add(position + new Vector3(1, 0, 1)); // Bottom-right-front (5)
        vertices.Add(position + new Vector3(0, 1, 1)); // Top-left-front (6)
        vertices.Add(position + new Vector3(1, 1, 1)); // Top-right-front (7)

        // Check neighboring blocks and add faces accordingly
        bool leftNeighbor = (position.x > 0) && (GetBlockHeight(position + new Vector3(-1, 0, 0)) >= position.y);
        bool rightNeighbor = (position.x < mapSize - 1) && (GetBlockHeight(position + new Vector3(1, 0, 0)) >= position.y);
        bool backNeighbor = (position.z > 0) && (GetBlockHeight(position + new Vector3(0, 0, -1)) >= position.y);
        bool frontNeighbor = (position.z < mapSize - 1) && (GetBlockHeight(position + new Vector3(0, 0, 1)) >= position.y);
        bool bottomNeighbor = (position.y > 0) && (GetBlockHeight(position + new Vector3(0, -1, 0)) >= position.y - 1);
        bool topNeighbor = (position.y < amplitude) && (GetBlockHeight(position + new Vector3(0, 1, 0)) >= position.y + 1);

        if (!topNeighbor)
        {
            // Top face
            triangles.AddRange(new[]
            {
            vertexOffset + 2, vertexOffset + 7, vertexOffset + 3,
            vertexOffset + 2, vertexOffset + 6, vertexOffset + 7
        });
        }

        if (!frontNeighbor)
        {
            // Front face
            triangles.AddRange(new[]
            {
            vertexOffset + 4, vertexOffset + 7, vertexOffset + 5,
            vertexOffset + 4, vertexOffset + 6, vertexOffset + 7
        });
        }

        if (!rightNeighbor)
        {
            // Right face
            triangles.AddRange(new[]
            {
            vertexOffset + 1, vertexOffset + 7, vertexOffset + 3,
            vertexOffset + 1, vertexOffset + 5, vertexOffset + 7
        });
        }

        if (!backNeighbor)
        {
            // Back face
            triangles.AddRange(new[]
            {
            vertexOffset + 0, vertexOffset + 3, vertexOffset + 1,
            vertexOffset + 0, vertexOffset + 2, vertexOffset + 3
        });
        }

        if (!leftNeighbor)
        {
            // Left face
            triangles.AddRange(new[]
            {
            vertexOffset + 4, vertexOffset + 2, vertexOffset + 0,
            vertexOffset + 4, vertexOffset + 6, vertexOffset + 2
        });
        }

        if (!bottomNeighbor)
        {
            // Bottom face
            triangles.AddRange(new[]
            {
            vertexOffset + 5, vertexOffset + 0, vertexOffset + 1,
            vertexOffset + 5, vertexOffset + 4, vertexOffset + 0
        });
        }
    }

    float GetBlockHeight(Vector3 position)
    {
        float noise = Mathf.PerlinNoise((position.x + seed) * frequency, (position.z + seed) * frequency) * amplitude;
        return Mathf.Round(noise * heightVariation) / heightVariation;
    }

    void RegenerateNoise()
    {
        highestNoise = 0;
        seed = Random.Range(0.0f, 99999.0f);
        GenerateTerrain();
    }

    void AssignColor()
    {
        float multiplier = 1.0f / highestNoise;
        float[] multiplierArray = { heightColor.r / highestNoise, heightColor.g / highestNoise, heightColor.b / highestNoise };

        Color[] colors = new Color[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float height = vertices[i].y;
            Color realColor = new Color(multiplierArray[0] * height, multiplierArray[1] * height, multiplierArray[2] * height, 1.0f);
            colors[i] = realColor;
        }

        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        mesh.colors = colors;
    }

    void Start()
    {
        GenerateTerrain();
    }

    void Update()
    {
        if (regenerateHeightmap)
        {
            RegenerateNoise();
            regenerateHeightmap = false;
            if (enableColorHeightmap)
            {
                AssignColor();
            }
        }
        if (scrollTerrain)
        {
            seed += 1.0f;
            GenerateTerrain();
            if (enableColorHeightmap)
            {
                AssignColor();
            }
        }
    }
}