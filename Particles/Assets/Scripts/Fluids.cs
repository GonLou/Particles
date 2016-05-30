using UnityEngine;
using System.Collections;

public class Fluids : MonoBehaviour {
    public Material fluidMat, GUIMat;

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

        createRenderTextures(GUITexture, RenderTextureFormat.ARGB32, FilterMode.Bilinear, TextureWrapMode.Clamp, false);
        createRenderTextures(divergenceTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, true);
        createRenderTextures(obstaclesTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, true);

        velocityTexture = new RenderTexture[2];
        createMultiRenderTextures(velocityTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        densityTexture = new RenderTexture[2];
        createMultiRenderTextures(densityTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        temperatureTexture = new RenderTexture[2];
        createMultiRenderTextures(temperatureTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        pressureTexture = new RenderTexture[2];
        createMultiRenderTextures(pressureTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);

        GUI.texture = GUITexture;
        //GUIMat.SetTexture("Obstacles", obstaclesTexture);
	}

    void createRenderTextures(RenderTexture texture, RenderTextureFormat format, FilterMode filter, TextureWrapMode wrap, bool isReadWrite)
    {
        if(isReadWrite)
            texture = new RenderTexture((int)viewWidth, (int)viewHeight, 0, format, RenderTextureReadWrite.Linear);
        else
            texture = new RenderTexture((int)viewWidth, (int)viewHeight, 0, format);
        texture.filterMode = filter;
        texture.wrapMode = wrap;
        texture.Create();
    }

    void createMultiRenderTextures(RenderTexture[] texture, RenderTextureFormat format, FilterMode filter, TextureWrapMode wrap, RenderTextureReadWrite readWrite)
    {
        for (int i = 0; i < texture.Length; i++)
        {
            texture[i] = new RenderTexture((int)viewWidth, (int)viewHeight, 0, format, readWrite);
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
        fluidMat.SetPass(0);
        fluidMat.SetVector("InverseSize", inverseSize);
        fluidMat.SetFloat("TimeStep", timeStep);
        fluidMat.SetFloat("Dissipation", dissipation);
        fluidMat.SetTexture("Velocity", velocity);
        fluidMat.SetTexture("SourceTexture", source);
        fluidMat.SetTexture("Obstacles", obstaclesTexture);

        Graphics.Blit(null, destination, fluidMat);
    }

    void ApplyBuoyancy(RenderTexture velocity, RenderTexture temperature, RenderTexture density, RenderTexture destination)
    {
        fluidMat.SetPass(5);
        fluidMat.SetTexture("Velocity", velocity);
        fluidMat.SetTexture("Temperature", temperature);
        fluidMat.SetTexture("Density", density);
        fluidMat.SetFloat("AmbientTemperature", ambientTemperature);
        fluidMat.SetFloat("TimeStep", timeStep);
        fluidMat.SetFloat("Sigma", smokeBuoyancy);
        fluidMat.SetFloat("Kappa", smokeWeight);
        

        Graphics.Blit(null, destination, fluidMat);
    }

    void ApplyImpulse(RenderTexture source, RenderTexture destination, Vector2 position, float radius, float val)
    {
        fluidMat.SetPass(4);
        fluidMat.SetVector("Point", position);
        fluidMat.SetFloat("Radius", radius);
        fluidMat.SetFloat("Fill", val);
        fluidMat.SetTexture("SourceTexture", source);
        

        Graphics.Blit(null, destination, fluidMat);
    }

    void ApplyDivergence(RenderTexture velocity, RenderTexture destination)
    {
        fluidMat.SetPass(3);
        fluidMat.SetFloat("HalfInverseCellSize", 0.5f / cellSize);
        fluidMat.SetTexture("Velocity", velocity);
        fluidMat.SetVector("InverseSize", inverseSize);
        fluidMat.SetTexture("Obstacles", obstaclesTexture);

        Graphics.Blit(null, destination, fluidMat);
    }

    void ApplyJacobi(RenderTexture pressure, RenderTexture divergence, RenderTexture destination)
    {
        fluidMat.SetPass(1);
        fluidMat.SetTexture("Pressure", pressure);
        fluidMat.SetTexture("Divergence", divergence);
        fluidMat.SetVector("InverseSize", inverseSize);
        fluidMat.SetFloat("Alpha", -cellSize * cellSize);
        fluidMat.SetFloat("InverseBeta", 0.25f);
        fluidMat.SetTexture("Obstacles", obstaclesTexture);
        

        Graphics.Blit(null, destination, fluidMat);
    }

    void ApplySutraction(RenderTexture velocity, RenderTexture pressure, RenderTexture destination)
    {
        fluidMat.SetPass(2);
        fluidMat.SetTexture("Velocity", velocity);
        fluidMat.SetTexture("Pressure", pressure);
        fluidMat.SetFloat("GradientScale", gradientScale);
        fluidMat.SetVector("InverseSize", inverseSize);
        fluidMat.SetTexture("Obstacles", obstaclesTexture);
        

        Graphics.Blit(null,destination,fluidMat);
    }

    void CreateObstacles()
    {
        fluidMat.SetPass(6);
        fluidMat.SetVector("InverseSize", inverseSize);
        fluidMat.SetVector("Point", obstaclePosition);
        fluidMat.SetFloat("Radius", obstacleSize);
        

        Graphics.Blit(null, obstaclesTexture, fluidMat);
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
