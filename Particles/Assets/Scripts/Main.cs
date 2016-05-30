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

    // Use this for initialization
    void Start () {
        initialPosition = gameObject.transform.position;
        particleObjects = new List<GameObject>();
        Smoke();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        Particle particle, pparticle;

        int count = particles.Count;

        for (int i=0; i < count; i++)
        {
            particle = (Particle)particles[i];

            particleObjects[i].transform.position = particle.Location;

            pparticle = (Particle)particles[GetClosestParticle(particleObjects[i], i)];
            particle.Velocity = pparticle.velocityChange();

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
        float myLife = Random.Range(1, 5);
        Vector3 velocity = new Vector3(Random.Range(-0.01f, 0.1f),
                                Random.Range(-0.01f, 1.0f),
                                Random.Range(-0.01f, 0.1f));
        Vector3 direction = new Vector3(0, 0.1f, 0);
        Color color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        Particle particle = new Particle(   initialPosition,
                                            velocity,
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

    int GetClosestParticle(GameObject pparticle, int skip)
    {
        Transform closestParticle = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = pparticle.transform.position;
        int totalObjets = particleObjects.Count;
        int particleIndex = 0;
        for (int i = 0; i < totalObjets; i++)
        {
            if (i != skip)
            {
                Vector3 directionToTarget = particleObjects[i].transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    particleIndex = i;
                }
            }
        }
        return particleIndex;
    }
}
