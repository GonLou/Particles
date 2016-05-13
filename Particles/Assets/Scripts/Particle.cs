using UnityEngine;
using System.Collections;

public class Particle
{

    struct pVector
    {
        Vector3 location;
        float velocity;
        float acceleration;
    }

    float lifeSpan;

    bool isDead()
    {
        if (lifeSpan < 0.0f)
            return true;
        else
            return false;
    }
}
