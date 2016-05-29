using UnityEngine;
using System.Collections;

public class Fluids : MonoBehaviour {
    public Material fluidMat, GUImat;

    RenderTexture GUITexture, divergenceTexture, obstaclesTexture;
    RenderTexture[] velocityTexture, densityTexture, pressureTexture, temperatureTexture;

    const float cellSize = 1.25f;

    const float ambientTemperature = 0.0f;
    const float impulseTemperature = 10.0f;
    const float impulseDensity = 1.0f;
    const int JacobiIterations = 40;
    const float timeStep = 0.125f;
    const float smokeBuoyancy = 1.0f;
    const float smokeWeight = 0.05f;
    const float gradientScale = 1.125f / cellSize;
    const float temperatureDissipation = 0.99f;
    const float velocityDissipation = 0.99f;
    const float densityDissipation = 0.9999f;

    Vector2 impulsePosition = new Vector2(0.0f, 0.0f);
    float inpulseSize = 0.1f;

    Vector2 obstaclePosition = new Vector2(0.5f, 0.5f);
    float obstacleSize = 0.1f;

    GUITexture GUI;
    
    float viewWidth, viewHeight;
    Vector2 inverseSize;

	// Use this for initialization
	void Start () 
    {
        gameObject.AddComponent<GUITexture>();
        GUI = GetComponent<GUITexture>();
        Rect GUIInset = new Rect(-256, -256, 512, 512);
        GUI.pixelInset = GUIInset;

        viewWidth = GUI.pixelInset.width;
        viewHeight = GUI.pixelInset.height;
        inverseSize = new Vector2(1.0f / viewWidth, 1.0f / viewHeight);

        GUITexture = new RenderTexture((int)viewWidth, (int)viewHeight, 0, RenderTextureFormat.ARGB32);
        GUITexture.filterMode = FilterMode.Bilinear;
        GUITexture.wrapMode = TextureWrapMode.Clamp;
        GUITexture.Create();

        divergenceTexture = new RenderTexture((int)viewWidth, (int)viewHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        divergenceTexture.filterMode = FilterMode.Point;
        divergenceTexture.wrapMode = TextureWrapMode.Clamp;
        divergenceTexture.Create();

        obstaclesTexture = new RenderTexture((int)viewWidth, (int)viewHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        obstaclesTexture.filterMode = FilterMode.Point;
        obstaclesTexture.wrapMode = TextureWrapMode.Clamp;
        obstaclesTexture.Create();

        velocityTexture = new RenderTexture[2];
        densityTexture = new RenderTexture[2];
        temperatureTexture = new RenderTexture[2];
        pressureTexture = new RenderTexture[2];

        GUI.texture = GUITexture;
        GUImat.SetTexture("Obstacles", obstaclesTexture);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
