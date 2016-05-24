using UnityEngine;
using System.Collections;

public class SelfDestroy : MonoBehaviour
{
    //Main lifespan;
    float lifetime = 3;
    private float startTime;

    void Awake()
    {
        //lifespan = transform.root.GetComponent<Main>();
        //lifespan.par
    }

    void OnEnable()
    {
        startTime = Time.timeSinceLevelLoad;
    }

    // Update is called once per frame
    void Update()
    {
        if ((startTime + lifetime) < Time.timeSinceLevelLoad)
            Destroy(gameObject);
    }
}
