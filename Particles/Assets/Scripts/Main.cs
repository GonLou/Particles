using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

    int maxParticlesNumber = 100;

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
	void FixedUpdate () {
        Particle particle;

        int count = particles.Count;

        Debug.Log("count: " + count);

        for (int i=0; i < count; i++)
        {
            particle = (Particle)particles[i];

            particleObjects[i].transform.position = particle.Location;

            if ( !particle.Update() )
            {
                particles.RemoveAt(i);

                Destroy(particleObjects[i]);
                particleObjects.RemoveAt(i);

                if (isAutoFeed)
                {
                    particles.Add(GenerateParticle());
                }
                else
                {
                    i--;
                    if (i < 0) break;
                    count = particles.Count;
                }

                //Debug.Log("count: " + count);
                //Debug.Log("i: " + i);
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
        Vector3 location;
        float myLife = Random.Range(1, 5);
        location = new Vector3(Random.Range(-0.01f, 0.1f),
                                Random.Range(-0.01f, 1.0f),
                                Random.Range(-0.01f, 0.1f));
        Vector3 direction = new Vector3(0, 0.1f, 0);
        //myLife = 1;
        //Debug.Log("myLife " + myLife);
        Color color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        Particle particle = new Particle(   initialPosition,
                                            location,
                                            direction,
                                            myLife,
                                            color);
        GameObject newParticle = (GameObject) Instantiate(prefabObject, particle.Location, Quaternion.identity);
        //newParticle.AddComponent<Rigidbody>(); we use our own gravity system
        //newParticle.AddComponent<SelfDestroy>();
        //newParticle.GetComponent<SelfDestroy>().enabled = true;
        newParticle.GetComponent<Renderer>().material.color = color;
        particleObjects.Add(newParticle);

        return particle;
    }
}
