using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {

    public int maxParticlesNumber;

    public bool isAutoFeed;

    Vector3 initialPosition;

    ArrayList particles = new ArrayList();

	// Use this for initialization
	void Start () {
        maxParticlesNumber = 500;
        isAutoFeed = true;
        initialPosition = new Vector3();
    }
	
	// Update is called once per frame
	void Update () {
        Particle particle;

        int count = particles.Count;

        for (int i=0; i< count; i++)
        {
            particle = (Particle)particles[i];

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = particle.Location;

            if ((!particle.Update()) || (particle.isDead()))
            {
                particles.RemoveAt(i);
                i--;
                count = particles.Count;
            }

            if (count < maxParticlesNumber)
            {
                particles.Add(GenerateParticle());
            }
        }
	}

    public void Smoke()
    {
        for (int i = 0; i < maxParticlesNumber; i++)
        {

            particles.Add(GenerateParticle());
        }
    }

    public Particle GenerateParticle()
    {

        Particle particle = new Particle(   initialPosition,
                                            new Vector3(Random.Range(-4.0F, 5.0F),
                                                        Random.Range(-10.0F, 10.0F),
                                                        Random.Range(-4.0F, 20.0F)),
                                            Random.Range(-10.0F, 10.0F),
                                            Random.Range(-10.0F, 10.0F),
                                            Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f)   );

        return particle;
    }
}
