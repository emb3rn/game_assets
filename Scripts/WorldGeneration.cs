using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    public Color heightColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    public Material cubeMaterial;
    public float generateSeed = 0.1f;
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

    static public int mapSize = 40;
    public GameObject[,] gameObjects = new GameObject[mapSize, mapSize];
    private bool terrainGenerated = false;
    private float highestNoise = 0; //THIS IS ONLY USED TO DEMONSTRATE HEIGHTMAP WITH COLOR U CAN REMOVE EVERYTHING TO DO WITH THIS

    void createMesh(int i, int j)
    {
        Vector3[] pos = new Vector3[8];
        pos[0] = new Vector3(0, 0, 0); //bl
        pos[1] = new Vector3(0, 1, 0); //tl
        pos[2] = new Vector3(1, 1, 0); //tr
        pos[3] = new Vector3(1, 0, 0); //br
        pos[4] = new Vector3(0, 0, 1); // Back bottom left
        pos[5] = new Vector3(0, 1, 1); // Back top left
        pos[6] = new Vector3(1, 1, 1); // Back top right
        pos[7] = new Vector3(1, 0, 1); // Back bottom right

        int[] FRONT_TRIS ={
            0, 2, 3,
            0, 1, 2};
        int[] RIGHT_TRIS ={
            4, 6, 7,
            4, 5, 6};
        int[] LEFT_TRIS ={
            8, 10, 11,
            8, 9, 10};
        int[] BACK_TRIS ={
            12, 14, 15,
            12, 13, 14};
        int[] TOP_TRIS ={
            16, 18, 19,
            16, 17, 18};

        Vector3[] vertices =
        {
            //front
            pos[0], //bl
            pos[1], //tl
            pos[2], //tr
            pos[3], //br
            //right
            pos[3], //bl
            pos[2], //tl
            pos[6], //tr
            pos[7], //br
            //left
            pos[4], //bl
            pos[5], //tl
            pos[1], //tr
            pos[0], //br
            //back
            pos[7], //bl
            pos[6], //tl
            pos[5], //tr
            pos[4], //br
            //top
            pos[1], //bl
            pos[5], //tl
            pos[6], //tr
            pos[2], //br
            //bottom
            pos[4], //bl
            pos[0], //tl
            pos[3], //tr
            pos[7], //br              
        };
        
        List<int[]> triangleFaces = new List<int[]>();

        triangleFaces.Add(TOP_TRIS);
        if (j + 1 < mapSize && gameObjects[i, j+1].transform.position.y < gameObjects[i, j].transform.position.y)
        {
            triangleFaces.Add(BACK_TRIS);
        }
        if (j - 1 >= 0 && gameObjects[i, j-1].transform.position.y < gameObjects[i, j].transform.position.y)
        {
            triangleFaces.Add(FRONT_TRIS);
            
        }
        if (i + 1 < mapSize && gameObjects[i+1, j].transform.position.y < gameObjects[i, j].transform.position.y)
        {
            triangleFaces.Add(RIGHT_TRIS);
        }
        if (i - 1 >= 0 && gameObjects[i-1, j].transform.position.y < gameObjects[i, j].transform.position.y)
        {
            triangleFaces.Add(LEFT_TRIS);
        }

        int[] triangles = new int[triangleFaces.Count * 6]; //6 per face, +6 for the top face which is always rendered
        for (int x = 0; x < triangleFaces.Count; x++)
        {
            for (int y = 0; y < triangleFaces[x].Length; y++)
            {
                int z = y + (x * 6);
                triangles[z] = triangleFaces[x][y];
            }
        }

        Mesh mesh = gameObjects[i,j].GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
    void generateTerrain()
    {
        highestNoise = 0;
        if (!terrainGenerated)
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    float noise = Mathf.PerlinNoise((i + generateSeed) * frequency, (j + generateSeed) * frequency) * amplitude;
                    gameObjects[i, j] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gameObjects[i, j].transform.position = new Vector3(i, Mathf.Round(noise * heightVariation) / heightVariation, j);
                    MeshRenderer meshRenderer = gameObjects[i, j].GetComponent<MeshRenderer>();
                    meshRenderer.enabled = false;
                    gameObjects[i, j].GetComponent<Renderer>().sharedMaterial = cubeMaterial;
                    //Destroy(gameObjects[i, j].GetComponent<BoxCollider>()); // I need to rewrite this to be an empty gameobject so I dont need to destroy things every frame.

                    if (noise > highestNoise)
                    {
                        highestNoise = noise;
                    }
                }
            }
        }
        terrainGenerated = true; //TODO: Replace this later with an if gameobjects[0,0] == null, this just ensures the map is only
                                 //generated once
        generateMeshes();
    }

    void generateMeshes()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                createMesh(i, j);
            }
        }
    }

    void regenerateNoise()
    {
        highestNoise = 0;
        generateSeed = Random.Range(0.0f, 99999.0f);
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                Vector3 pos = gameObjects[i, j].transform.position;
                float noise = Mathf.PerlinNoise((i + generateSeed) * frequency, (j + generateSeed) * frequency) * amplitude;
                gameObjects[i, j].transform.position = new Vector3(pos.x, Mathf.Round(noise * heightVariation) / heightVariation, pos.z);
                gameObjects[i, j].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                if (noise > highestNoise)
                {
                    highestNoise = noise;
                }
            }
        }
    }

    void assignColor()
    {
        float multiplier = 1.0f / highestNoise;
        float[] multiplierArray = { heightColor.r / highestNoise, heightColor.g / highestNoise, heightColor.b / highestNoise };
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                Color tempColor = new Color(Random.Range(0.0f, 0.5f), 0.0f, 0.0f, 1.0f);
                Color realColor = new Color(multiplierArray[0] * gameObjects[i, j].transform.position.y, multiplierArray[1] * gameObjects[i, j].transform.position.y, multiplierArray[2] * gameObjects[i, j].transform.position.y, 1.0f);
                gameObjects[i, j].GetComponent<Renderer>().material.SetColor("_Color", realColor);
            }
        }
    }

    void Start()
    {
        generateTerrain();
    }

    void Update()
    {
        if (enableRenderDistance || debugRender)
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    MeshRenderer meshRenderer = gameObjects[i, j].GetComponent<MeshRenderer>();
                    if (debugRender)
                    {
                        meshRenderer.enabled = true;
                    }
                    else
                    {
                        float distance = Vector3.Distance(gameObjects[i, j].transform.position, playerCamera.transform.position);
                        if (distance < renderDistance)
                        {
                            meshRenderer.enabled = true;
                        }
                        else
                        {
                            meshRenderer.enabled = false;
                        }
                    }
                }
            }
        }
       if (regenerateHeightmap)
        {
            regenerateNoise();
            regenerateHeightmap = false;
            generateMeshes(); 
            if (enableColorHeightmap)
            {
                assignColor();
            }
        }
        if (scrollTerrain)
        {
            generateSeed += 1.0f;
            highestNoise = 0;
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    Vector3 pos = gameObjects[i, j].transform.position;
                    float noise = Mathf.PerlinNoise((i + generateSeed) * frequency, (j + generateSeed) * frequency) * amplitude;
                    gameObjects[i, j].transform.position = new Vector3(pos.x, Mathf.Round(noise * heightVariation) / heightVariation, pos.z);
                    gameObjects[i, j].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                    if (noise > highestNoise)
                    {
                        highestNoise = noise;
                    }
                }
            }
            if (enableColorHeightmap)
            {
                assignColor();
            }
            generateMeshes();


        }
    }
}
