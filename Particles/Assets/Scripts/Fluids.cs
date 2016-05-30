using UnityEngine;
using System.Collections;

public class Fluids : MonoBehaviour {
    public Material GUIMat, advectMat, buoyancyMat, divergenceMat, jacobiMat, impulseMat, gradientMat, obstaclesMat;

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

    Vector2 impulsePosition = new Vector2(0.5f, 0.0f);
    float inpulseSize = 0.1f;

    Vector2 obstaclePosition = new Vector2(0.5f, 0.5f);
    float obstacleSize = 0.1f;

    GUITexture GUI;
    
    int viewWidth, viewHeight;
    Vector2 inverseSize;

	// Use this for initialization
	void Start () 
    {
        GUI = GetComponent<GUITexture>();

        viewWidth = (int)GUI.pixelInset.width;
        viewHeight = (int)GUI.pixelInset.height;
        inverseSize = new Vector2(1.0f / viewWidth, 1.0f / viewHeight);

        GUITexture = new RenderTexture(viewWidth, viewHeight, 0, RenderTextureFormat.ARGB32);
        GUITexture.filterMode = FilterMode.Bilinear;
        GUITexture.wrapMode = TextureWrapMode.Clamp;
        GUITexture.Create();

        divergenceTexture = new RenderTexture(viewWidth, viewHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        divergenceTexture.filterMode = FilterMode.Point;
        divergenceTexture.wrapMode = TextureWrapMode.Clamp;
        divergenceTexture.Create();

        obstaclesTexture = new RenderTexture(viewWidth, viewHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        obstaclesTexture.filterMode = FilterMode.Point;
        obstaclesTexture.wrapMode = TextureWrapMode.Clamp;
        obstaclesTexture.Create();

        velocityTexture = new RenderTexture[2];
        createMultiRenderTextures(velocityTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        densityTexture = new RenderTexture[2];
        createMultiRenderTextures(densityTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        temperatureTexture = new RenderTexture[2];
        createMultiRenderTextures(temperatureTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        pressureTexture = new RenderTexture[2];
        
        createMultiRenderTextures(pressureTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        GUIMat.SetTexture("Obstacles", obstaclesTexture);
	}

    void createMultiRenderTextures(RenderTexture[] texture, RenderTextureFormat format, FilterMode filter, TextureWrapMode wrap, RenderTextureReadWrite readWrite)
    {
        for (int i = 0; i < texture.Length; i++)
        {
            texture[i] = new RenderTexture(viewWidth, viewHeight, 0, format, readWrite);
            texture[i].filterMode = filter;
            texture[i].wrapMode = wrap;
            texture[i].Create();
        }
    }
	
    void swapTextures(RenderTexture[] texture)
    {
        RenderTexture temp = texture[0];
        texture[0] = texture[1];
        texture[1] = temp;
    }

    void clearTexture(RenderTexture texture)
    {
        Graphics.SetRenderTarget(texture);
        GL.Clear(false, true,new Color(0,0,0,0));
        Graphics.SetRenderTarget(null);
    }

    void ApplyAdvect(RenderTexture velocity, RenderTexture source, RenderTexture destination, float dissipation)
    {
        advectMat.SetVector("InverseSize", inverseSize);
        advectMat.SetFloat("TimeStep", timeStep);
        advectMat.SetFloat("Dissipation", dissipation);
        advectMat.SetTexture("Velocity", velocity);
        advectMat.SetTexture("SourceTexture", source);
        advectMat.SetTexture("Obstacles", obstaclesTexture);

        Graphics.Blit(null, destination, advectMat);
    }

    void ApplyBuoyancy(RenderTexture velocity, RenderTexture temperature, RenderTexture density, RenderTexture destination)
    {
        buoyancyMat.SetTexture("Velocity", velocity);
        buoyancyMat.SetTexture("Temperature", temperature);
        buoyancyMat.SetTexture("Density", density);
        buoyancyMat.SetFloat("AmbientTemperature", ambientTemperature);
        buoyancyMat.SetFloat("TimeStep", timeStep);
        buoyancyMat.SetFloat("Sigma", smokeBuoyancy);
        buoyancyMat.SetFloat("Kappa", smokeWeight);

        Graphics.Blit(null, destination, buoyancyMat);
    }

    void ApplyImpulse(RenderTexture source, RenderTexture destination, Vector2 position, float radius, float val)
    {
        impulseMat.SetVector("Point", position);
        impulseMat.SetFloat("Radius", radius);
        impulseMat.SetFloat("Fill", val);
        impulseMat.SetTexture("SourceTexture", source);


        Graphics.Blit(null, destination, impulseMat);
    }

    void ApplyDivergence(RenderTexture velocity, RenderTexture destination)
    {
        divergenceMat.SetFloat("HalfInverseCellSize", 0.5f / cellSize);
        divergenceMat.SetTexture("Velocity", velocity);
        divergenceMat.SetVector("InverseSize", inverseSize);
        divergenceMat.SetTexture("Obstacles", obstaclesTexture);

        Graphics.Blit(null, destination, divergenceMat);
    }

    void ApplyJacobi(RenderTexture pressure, RenderTexture divergence, RenderTexture destination)
    {
        jacobiMat.SetTexture("Pressure", pressure);
        jacobiMat.SetTexture("Divergence", divergence);
        jacobiMat.SetVector("InverseSize", inverseSize);
        jacobiMat.SetFloat("Alpha", -cellSize * cellSize);
        jacobiMat.SetFloat("InverseBeta", 0.25f);
        jacobiMat.SetTexture("Obstacles", obstaclesTexture);

        Graphics.Blit(null, destination, jacobiMat);
    }

    void ApplySutraction(RenderTexture velocity, RenderTexture pressure, RenderTexture destination)
    {
        gradientMat.SetTexture("Velocity", velocity);
        gradientMat.SetTexture("Pressure", pressure);
        gradientMat.SetFloat("GradientScale", gradientScale);
        gradientMat.SetVector("InverseSize", inverseSize);
        gradientMat.SetTexture("Obstacles", obstaclesTexture);

        Graphics.Blit(null, destination, gradientMat);
    }

    void CreateObstacles()
    {       
        obstaclesMat.SetVector("InverseSize", inverseSize);
        obstaclesMat.SetVector("Point", obstaclePosition);
        obstaclesMat.SetFloat("Radius", obstacleSize);

        Graphics.Blit(null, obstaclesTexture, obstaclesMat);
    }


	// Update is called once per frame
	void Update () {
        CreateObstacles();
        int READ = 0;
        int WRITE = 1;
        ApplyAdvect(velocityTexture[READ], velocityTexture[READ], velocityTexture[WRITE], velocityDissipation);
        ApplyAdvect(velocityTexture[READ], temperatureTexture[READ], temperatureTexture[WRITE], temperatureDissipation);
        ApplyAdvect(velocityTexture[READ], densityTexture[READ], densityTexture[WRITE], densityDissipation);

        swapTextures(velocityTexture);
        swapTextures(temperatureTexture);
        swapTextures(densityTexture);

        ApplyBuoyancy(velocityTexture[READ], temperatureTexture[READ], densityTexture[READ], velocityTexture[WRITE]);

        swapTextures(velocityTexture);

        ApplyImpulse(temperatureTexture[READ], temperatureTexture[WRITE], impulsePosition, inpulseSize, impulseTemperature);
        ApplyImpulse(densityTexture[READ], densityTexture[WRITE], impulsePosition, inpulseSize, impulseDensity);

        swapTextures(temperatureTexture);
        swapTextures(densityTexture);

        ApplyDivergence(velocityTexture[READ], divergenceTexture);

        clearTexture(pressureTexture[READ]);

        int i = 0;
        for (i = 0; i < JacobiIterations; ++i)
        {
            ApplyJacobi(pressureTexture[READ], divergenceTexture, pressureTexture[WRITE]);
            swapTextures(pressureTexture);
        }

        //Use the pressure tex that was last rendered into. This computes divergence free velocity
        ApplySutraction(velocityTexture[READ], pressureTexture[READ], velocityTexture[WRITE]);

        swapTextures(velocityTexture);

        //Render the rex you want to see into gui tex. Will only use the red channel
        Graphics.Blit(densityTexture[READ], GUITexture, GUIMat);

	}
}
