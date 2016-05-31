using UnityEngine;
using System.Collections;

public class Fluids : MonoBehaviour {
    public Material GUIMat, advectMat, buoyancyMat, divergenceMat, jacobiMat, impulseMat, gradientMat, obstaclesMat;
    //public Material GUIMat, fluidMat;

    RenderTexture GUITexture, divergenceTexture, obstaclesTexture;
    RenderTexture[] velocityTexture, densityTexture, pressureTexture, temperatureTexture;

    const float cellSize = 1.0f;

    const float ambientTemperature = 0.0f;
    const float impulseTemperature = 10.0f;
    const float impulseDensity = 1.0f;
    const int JacobiIterations = 40;
    const float timeStep = 0.125f;
    const float smokeBuoyancy = 1.0f;
    const float smokeWeight = 0.05f;
    const float gradientScale = 1.0f;
    const float temperatureDissipation = 0.99f;
    const float velocityDissipation = 0.99f;
    const float densityDissipation = 0.9999f;

    Vector2 impulsePosition = new Vector2(0.5f, 0.0f);
    float impulseSize = 0.1f;

    Vector2 obstaclePosition = new Vector2(0.5f, 0.5f);
    float obstacleSize = 0.1f;

    GUITexture GUI_tex;
    
    int viewWidth, viewHeight;
    Vector2 offSet;
    Vector2 inverseSize;

    bool obstacle = false, top = false, bottom = false, left = false, right = false;

	// Use this for initialization
	void Start () 
    {
        GUI_tex = GetComponent<GUITexture>();

        viewWidth = (int)GUI_tex.pixelInset.width;
        viewHeight = (int)GUI_tex.pixelInset.height;
        offSet = new Vector2(GUI_tex.pixelInset.x, GUI_tex.pixelInset.y);
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
        createMultiRenderTextures(velocityTexture, RenderTextureFormat.RGFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        densityTexture = new RenderTexture[2];
        createMultiRenderTextures(densityTexture, RenderTextureFormat.RFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        temperatureTexture = new RenderTexture[2];
        createMultiRenderTextures(temperatureTexture, RenderTextureFormat.RFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        pressureTexture = new RenderTexture[2]; 
        createMultiRenderTextures(pressureTexture, RenderTextureFormat.RFloat, FilterMode.Point, TextureWrapMode.Clamp, RenderTextureReadWrite.Linear);
        GetComponent<GUITexture>().texture = GUITexture;
        GUIMat.SetTexture("_Obstacles", obstaclesTexture);

        obstacle = top = bottom = left = right = true;
        obstaclesMat.SetFloat("_Active", obstacle.GetHashCode());
        obstaclesMat.SetFloat("_Top", top.GetHashCode());
        obstaclesMat.SetFloat("_Bottom", bottom.GetHashCode());
        obstaclesMat.SetFloat("_Left", left.GetHashCode());
        obstaclesMat.SetFloat("_Right", right.GetHashCode());
	}

    //Used to created an array of RenderTextures
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
	
    //Swap textures around
    void swapTextures(RenderTexture[] texture)
    {
        RenderTexture temp = texture[0];
        texture[0] = texture[1];
        texture[1] = temp;
    }

    //Clear all data from texture
    void clearTexture(RenderTexture texture)
    {
        Graphics.SetRenderTarget(texture);
        GL.Clear(false, true,new Color(0,0,0,0));
        Graphics.SetRenderTarget(null);
    }

    //Calculate advection
    void ApplyAdvect(RenderTexture velocity, RenderTexture source, RenderTexture destination, float dissipation)
    {
        advectMat.SetVector("_InverseSize", inverseSize);
        advectMat.SetFloat("_TimeStep", timeStep);
        advectMat.SetFloat("_Dissipation", dissipation);
        advectMat.SetTexture("_Velocity", velocity);
        advectMat.SetTexture("_Source", source);
        advectMat.SetTexture("_Obstacles", obstaclesTexture);

        Graphics.Blit(null, destination, advectMat);
    }

    //Calculate buoyancy
    void ApplyBuoyancy(RenderTexture velocity, RenderTexture temperature, RenderTexture density, RenderTexture destination)
    {
        buoyancyMat.SetTexture("_Velocity", velocity);
        buoyancyMat.SetTexture("_Temperature", temperature);
        buoyancyMat.SetTexture("_Density", density);
        buoyancyMat.SetFloat("_AmbientTemperature", ambientTemperature);
        buoyancyMat.SetFloat("_TimeStep", timeStep);
        buoyancyMat.SetFloat("_Sigma", smokeBuoyancy);
        buoyancyMat.SetFloat("_Kappa", smokeWeight);

        Graphics.Blit(null, destination, buoyancyMat);
    }

    //Calculate impulse
    void ApplyImpulse(RenderTexture source, RenderTexture destination, Vector2 position, float radius, float val)
    {
        impulseMat.SetVector("_Point", position);
        impulseMat.SetFloat("_Radius", radius);
        impulseMat.SetFloat("_Fill", val);
        impulseMat.SetTexture("_Source", source);


        Graphics.Blit(null, destination, impulseMat);
    }

    //Calculate divergence
    void ApplyDivergence(RenderTexture velocity, RenderTexture destination)
    {
        divergenceMat.SetFloat("_HalfInverseCellSize", 0.5f / cellSize);
        divergenceMat.SetTexture("_Velocity", velocity);
        divergenceMat.SetVector("_InverseSize", inverseSize);
        divergenceMat.SetTexture("_Obstacles", obstaclesTexture);

        Graphics.Blit(null, destination, divergenceMat);
    }

    //Calcualte Jacobi iteration
    void ApplyJacobi(RenderTexture pressure, RenderTexture divergence, RenderTexture destination)
    {
        jacobiMat.SetTexture("_Pressure", pressure);
        jacobiMat.SetTexture("_Divergence", divergence);
        jacobiMat.SetVector("_InverseSize", inverseSize);
        jacobiMat.SetFloat("_Alpha", -cellSize * cellSize);
        jacobiMat.SetFloat("_InverseBeta", 0.25f);
        jacobiMat.SetTexture("_Obstacles", obstaclesTexture);

        Graphics.Blit(null, destination, jacobiMat);
    }

    //Calcualte gradient
    void ApplySutraction(RenderTexture velocity, RenderTexture pressure, RenderTexture destination)
    {
        gradientMat.SetTexture("_Velocity", velocity);
        gradientMat.SetTexture("_Pressure", pressure);
        gradientMat.SetFloat("_GradientScale", gradientScale);
        gradientMat.SetVector("_InverseSize", inverseSize);
        gradientMat.SetTexture("_Obstacles", obstaclesTexture);

        Graphics.Blit(null, destination, gradientMat);
    }

    //Add obstacles
    void CreateObstacles()
    {       
        obstaclesMat.SetVector("_InverseSize", inverseSize);
        obstaclesMat.SetVector("_Point", obstaclePosition);
        obstaclesMat.SetFloat("_Radius", obstacleSize);

        Graphics.Blit(null, obstaclesTexture, obstaclesMat);
    }

    void UserInputs()
    {
        //Move the fluid position
        if (Input.GetMouseButton(0))
        {
            Vector2 pos = Input.mousePosition;

            pos.x -= Screen.width * 0.5f;
            pos.y -= Screen.height * 0.5f;

            pos -= offSet;

            pos.x /= viewWidth - 1.0f;
            pos.y /= viewHeight - 1.0f;
            impulsePosition = new Vector2(pos.x, pos.y);
            CreateObstacles();
        }

        //Moce the obstacle position
        if (Input.GetMouseButton(1))
        {
            Vector2 pos = Input.mousePosition;

            pos.x -= Screen.width * 0.5f;
            pos.y -= Screen.height * 0.5f;

            pos -= offSet;

            pos.x /= viewWidth - 1.0f;
            pos.y /= viewHeight - 1.0f;
            obstaclePosition = new Vector2(pos.x, pos.y);
            CreateObstacles();
        }

        //Enable or disable walls and obstacles
        if (Input.GetKeyDown(KeyCode.T))
        {
            obstacle = !obstacle;
            obstaclesMat.SetFloat("_Active", obstacle.GetHashCode());
        }

        //Enable or disable borders
        if (Input.GetKeyDown(KeyCode.W))
        {
            top = !top;
            obstaclesMat.SetFloat("_Top", top.GetHashCode());
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            left = !left;
            obstaclesMat.SetFloat("_Left", left.GetHashCode());
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            bottom = !bottom;
            obstaclesMat.SetFloat("_Bottom", bottom.GetHashCode());
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            right = !right;
            obstaclesMat.SetFloat("_Right", right.GetHashCode());
        }
        
        //Obstacle size decrease
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            if (obstacleSize > 0.01f)
                obstacleSize -= 0.01f;
            else
                obstacleSize = 0.01f;
        }

        //Obstacle size increase
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (obstacleSize < 0.09f)
                obstacleSize += 0.01f;
            else
                obstacleSize = 0.1f;
        }

        //Fluid size decrease
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (impulseSize > 0.06f)
                impulseSize -= 0.01f;
            else
                impulseSize = 0.05f;
        }

        //Fluid size increase
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (impulseSize < 0.09f)
                impulseSize += 0.01f;
            else
                impulseSize = 0.1f;
        }
    }

	// Update is called once per frame
	void Update () {
        CreateObstacles();

        //Used to differenciate between RenderTextures
        int READ = 0;
        int WRITE = 1;

        //Advection of the velocity, temperature and density
        ApplyAdvect(velocityTexture[READ], velocityTexture[READ], velocityTexture[WRITE], velocityDissipation);
        ApplyAdvect(velocityTexture[READ], temperatureTexture[READ], temperatureTexture[WRITE], temperatureDissipation);
        ApplyAdvect(velocityTexture[READ], densityTexture[READ], densityTexture[WRITE], densityDissipation);

        swapTextures(velocityTexture);
        swapTextures(temperatureTexture);
        swapTextures(densityTexture);

        //How the flow of the fluid changes the velocity
        ApplyBuoyancy(velocityTexture[READ], temperatureTexture[READ], densityTexture[READ], velocityTexture[WRITE]);

        swapTextures(velocityTexture);

        //Refresh the impulse of density and temperature
        ApplyImpulse(temperatureTexture[READ], temperatureTexture[WRITE], impulsePosition, impulseSize, impulseTemperature);
        ApplyImpulse(densityTexture[READ], densityTexture[WRITE], impulsePosition, impulseSize, impulseDensity);

        swapTextures(temperatureTexture);
        swapTextures(densityTexture);

        //User inputs
        UserInputs();

        //Calculates the divergence of the velocity
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

        //Render the tex you want to see into gui tex. Will only use the red channel
        Graphics.Blit(densityTexture[READ], GUITexture, GUIMat);

	}
}