using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

    int maxParticlesNumber = 50;

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
            float growFactor = 0.8f;
            float newX = particleObjects[i].transform.localScale.x + growFactor;
            float newY = particleObjects[i].transform.localScale.y + growFactor;
            float newZ = particleObjects[i].transform.localScale.z + growFactor;
            particleObjects[i].transform.localScale = new Vector3(  newX,
                                                                    newY,
                                                                    newZ);

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
        float myLife = Random.Range(1, 15);
        Vector3 velocity = new Vector3( Random.Range(-1.11f, 1.1f),
                                        Random.Range(-1.11f, 1.1f),
                                        Random.Range(-1.11f, 1.1f));
        Vector3 direction = new Vector3(Random.Range(-0.2f, 0.2f),
                                        Random.Range(0, 0.2f),
                                        Random.Range(-0.2f, 0.2f));
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

                if (isColliding(pparticle.transform, particleObjects[i].transform)) calculateRebound(pparticle.transform);
            }
        }
        return particleIndex;
    }

    void calculateRebound(Transform pf)
    {
        float x = pf.position.x * Mathf.Cos(45*Mathf.PI / 180);
        float y = pf.position.y * Mathf.Sin(45* Mathf.PI / 180);
        pf.position = new Vector3(x, y, pf.position.z);
    }


    bool isColliding(Transform iParticle, Transform other)
    {
        float radius = 1.0f;
        var distance = Mathf.Sqrt((iParticle.position.x - other.position.x) * (iParticle.position.x - other.position.x) +
                                  (iParticle.position.y - other.position.y) * (iParticle.position.y - other.position.y) +
                                  (iParticle.position.z - other.position.z) * (iParticle.position.z - other.position.z));

        if (distance < (radius + radius)) return true;
        //Debug.Log(distance);
        return false;
    }
}