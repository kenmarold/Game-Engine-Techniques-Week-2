using UnityEngine;

public class TerrainWithMaterial : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int terrainWidth = 512;  // Width of the terrain
    public int terrainLength = 512; // Length of the terrain
    public int terrainHeight = 100; // Max terrain height

    [Header("Noise Settings")]
    public float noiseScale = 50f;  // Scale of Perlin noise
    public int seed = 0;            // Random seed for variation

    [Header("Material Settings")]
    public Gradient terrainGradient;  // Color gradient based on height
    public Material customTerrainMaterial; // Assign your own material in Inspector

    public enum RenderPipelineType { BuiltIn, URP, HDRP }

    [Header("Rendering Pipeline")]
    public RenderPipelineType renderPipeline = RenderPipelineType.BuiltIn;

    private Terrain terrain;           // Reference to the Terrain component
    private TerrainData terrainData;    // Terrain data object

    void Start()
    {
        // Create the terrain
        GenerateTerrain();

        // Apply the material with procedural texture
        ApplyMaterial();
    }

    void GenerateTerrain()
    {
        GameObject terrainObject = new GameObject("Generated Terrain");
        terrain = terrainObject.AddComponent<Terrain>();
        TerrainCollider terrainCollider = terrainObject.AddComponent<TerrainCollider>();

        terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainWidth + 1;
        terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);

        float[,] heights = GenerateHeights();
        terrainData.SetHeights(0, 0, heights);

        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
    }

    float[,] GenerateHeights()
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

    void ApplyMaterial()
    {
        // Generate the texture based on terrain height
        Texture2D heightTexture = GenerateHeightTexture();

        Material terrainMaterial;

        // Assign material based on the selected rendering pipeline
        switch (renderPipeline)
        {
            case RenderPipelineType.URP:
                terrainMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                break;

            case RenderPipelineType.HDRP:
                terrainMaterial = new Material(Shader.Find("HDRP/Lit"));
                break;

            default:
                terrainMaterial = new Material(Shader.Find("Standard"));
                break;
        }

        // If a custom material is assigned, use it instead
        if (customTerrainMaterial != null)
        {
            terrainMaterial = customTerrainMaterial;
        }

        terrainMaterial.mainTexture = heightTexture;
        terrainMaterial.SetFloat("_Glossiness", 0f);  // Remove shininess for realism

        // Apply the material to the terrain
        terrain.materialTemplate = terrainMaterial;
    }

    Texture2D GenerateHeightTexture()
    {
        int width = terrainWidth;
        int length = terrainLength;
        float[,] heights = terrainData.GetHeights(0, 0, width, length);

        Texture2D texture = new Texture2D(width, length);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float heightValue = heights[x, y];

                // Normalize height to range 0 - 1 based on terrainHeight
                float normalizedHeight = heightValue * terrainHeight / terrainData.size.y;

                // Evaluate color from gradient based on normalized height
                Color terrainColor = terrainGradient.Evaluate(normalizedHeight);

                texture.SetPixel(x, y, terrainColor);
            }
        }

        texture.Apply();
        return texture;
    }
}
