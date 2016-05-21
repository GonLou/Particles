using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

    public int maxParticlesNumber = 500;

    public bool isAutoFeed = true;

    public GameObject prefabObject;

    List<GameObject> particleObjects;

    Vector3 initialPosition;

    ArrayList particles = new ArrayList();
    ArrayList objects = new ArrayList();

    // Use this for initialization
    void Start () {
        initialPosition = new Vector3();
        particleObjects = new List<GameObject>();

        Smoke();
    }
	
	// Update is called once per frame
	void Update () {
        Particle particle;

        int count = particles.Count;

        for (int i=0; i< count; i++)
        {
            particle = (Particle)particles[i];

            if ((!particle.Update()) || (particle.isDead()))
            {
                particles.RemoveAt(i);
                i--;
                count = particles.Count;
                particleObjects.Remove(prefabObject);
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

        particleObjects.Add( (GameObject) Instanciate(prefabObject, particle.Location, Quaternion.Identity));

        return particle;
    }
}
