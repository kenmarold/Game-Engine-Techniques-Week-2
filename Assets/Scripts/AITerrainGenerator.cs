using UnityEngine;

public class AITerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int terrainWidth = 512;  // Width of the terrain
    public int terrainLength = 512; // Length of the terrain
    public int terrainHeight = 100; // Max terrain height

    [Header("Noise Settings")]
    public float noiseScale = 50f;  // Scale of Perlin noise
    public int seed = 0;            // Random seed for variation

    [Header("AI Settings")]
    public int smoothingIterations = 5;  // Number of smoothing passes
    public float threshold = 0.5f;        // Elevation threshold for smoothing

    private Terrain terrain;              // Reference to Terrain component

    void Start()
    {
        terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("No Terrain component found! Attach this script to a terrain object.");
            return;
        }

        GenerateAITerrain();
    }

    void GenerateAITerrain()
    {
        terrain.terrainData = GenerateTerrainData();
    }

    TerrainData GenerateTerrainData()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainWidth + 1;
        terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);

        float[,] heights = GenerateInitialHeights();
        heights = ApplySmoothing(heights, smoothingIterations);

        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    float[,] GenerateInitialHeights()
    {
        float[,] heights = new float[terrainWidth, terrainLength];

        System.Random random = new System.Random(seed);
        float offsetX = random.Next(0, 100000);
        float offsetY = random.Next(0, 100000);

        for (int x = 0; x < terrainWidth; x++)
        {
            for (int y = 0; y < terrainLength; y++)
            {
                float sampleX = (x + offsetX) / noiseScale;
                float sampleY = (y + offsetY) / noiseScale;
                heights[x, y] = Mathf.PerlinNoise(sampleX, sampleY);
            }
        }
        return heights;
    }

    float[,] ApplySmoothing(float[,] heights, int iterations)
    {
        int width = heights.GetLength(0);
        int length = heights.GetLength(1);

        for (int i = 0; i < iterations; i++)
        {
            float[,] tempHeights = new float[width, length];

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < length - 1; y++)
                {
                    // AI-inspired rule: average height with neighbors to smooth terrain
                    float avgHeight =
                        (heights[x, y] +
                        heights[x - 1, y] +
                        heights[x + 1, y] +
                        heights[x, y - 1] +
                        heights[x, y + 1]) / 5.0f;

                    tempHeights[x, y] = Mathf.Lerp(heights[x, y], avgHeight, 0.5f);

                    // AI thresholding to simulate terrain growth
                    if (heights[x, y] > threshold)
                    {
                        tempHeights[x, y] = Mathf.Clamp(tempHeights[x, y] + 0.01f, 0f, 1f);
                    }
                }
            }
            heights = tempHeights;
        }
        return heights;
    }
}
