using UnityEngine;
using System.Collections;

public class Fluids : MonoBehaviour {
    public Material fluidMat;

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

	// Use this for initialization
	void Start () 
    {
        gameObject.AddComponent<GUITexture>();
        GUI = GetComponent<GUITexture>();
        if(GUI)
        {
            Debug.Log("Has got a gui texture");
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
