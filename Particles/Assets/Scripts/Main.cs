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
    //ArrayList objects = new ArrayList();

    // Use this for initialization
    void Start () {
        initialPosition = gameObject.transform.position;
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

            particleObjects[i].transform.position = particle.Location;

            if ((!particle.Update()) || (particle.isDead()))
            {
                particles.RemoveAt(i);
                i--;
                count = particles.Count;
                particleObjects.RemoveAt(i);
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
                                            new Vector3(Random.Range(0.0f, 1.0f),
                                                        Random.Range(0.0f, 10.0f),
                                                        Random.Range(0.0f, 4.0f)),
                                            new Vector3(0,1,0),
                                            Random.Range(1f, 20.0f), //lifespans
                                            Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f)   );
        GameObject newParticle = (GameObject) Instantiate(prefabObject, particle.Location, Quaternion.identity);
        newParticle.AddComponent<Rigidbody>();
        newParticle.AddComponent<SelfDestroy>();
        particleObjects.Add(newParticle);

        return particle;
    }
}
