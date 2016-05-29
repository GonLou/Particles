using UnityEngine;
using System.Collections;

public class SelfDestroy : MonoBehaviour
{
    Main lifespan;
    float lifetime = 3;
    private float startTime;

    void Awake()
    {
        lifespan = GameObject.Find("Emitter").GetComponent<Main>();
        lifetime = lifespan.myLife;
        startTime = Time.timeSinceLevelLoad;
    }

    // Update is called once per frame
    void Update()
    {
        if ((startTime + lifetime) < Time.timeSinceLevelLoad)
            Destroy(gameObject);
    }
}
