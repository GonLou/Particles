using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

    int maxParticlesNumber = 500;

    public bool isAutoFeed = true;

    public GameObject prefabObject;

    List<GameObject> particleObjects;

    Vector3 initialPosition;

    public float myLife;

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

        for (int i=0; i < count; i++)
        {
            particle = (Particle)particles[i];

            particleObjects[i].transform.position = particle.Location;

            if ((!particle.Update()) || (particle.isDead()))
            {
                particles.RemoveAt(i);
                particleObjects.RemoveAt(i);
                i--;
                if (i < 0) break;
                count = particles.Count;

                //Debug.Log("count: " + count);
                //Debug.Log("i: " + i);
            }
            
            if (count < maxParticlesNumber && isAutoFeed)
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
        myLife = Random.Range(50, 200);
        Particle particle = new Particle(   initialPosition,
                                            new Vector3(Random.Range(0.0f, 1.0f),
                                                        Random.Range(0.0f, 10.0f),
                                                        Random.Range(0.0f, 4.0f)),
                                            new Vector3(0,1,0),
                                            myLife, 
                                            Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f)   );
        GameObject newParticle = (GameObject) Instantiate(prefabObject, particle.Location, Quaternion.identity);
        //newParticle.AddComponent<Rigidbody>(); we use our own gravity system
        newParticle.AddComponent<SelfDestroy>();
        newParticle.GetComponent<SelfDestroy>().enabled = true;
        particleObjects.Add(newParticle);

        return particle;
    }
}
