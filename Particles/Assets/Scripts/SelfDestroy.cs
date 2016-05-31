using UnityEngine;
using System.Collections;

public class SelfDestroy : MonoBehaviour
{
    Main lifespan;
    float lifetime = 3;
    private float startTime;

    void Start()
    {
        lifespan = GameObject.Find("Emitter").GetComponent<Main>();
        //lifetime = lifespan.myLife;
        startTime = Time.timeSinceLevelLoad;
    }

    void FixedUpdate()
    {
        if ((startTime + lifetime) < Time.timeSinceLevelLoad)
            Destroy(gameObject);
    }
}
