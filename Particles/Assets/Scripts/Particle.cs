using UnityEngine;
using System.Collections;

public class Particle
{

    Vector3 location;
    Vector3 velocity;
    float acceleration;

    float lifeSpan;

    float color;

    public Particle(Vector3 location, Vector3 velocity, float acceleration, float lifeSpan, float color)
    {
        this.location = location;
        this.velocity = velocity;
        this.acceleration = acceleration;
        if (!isDead()) this.lifeSpan = lifeSpan;
        this.color = color;
    }

    public bool Update()
    {
        velocity = velocity - Environment.getInstance().Gravity
                            + Environment.getInstance().Wind;
        location = location + velocity;
        lifeSpan--;
        if (isDead())
            return false;
        return true;
    }

    bool isDead()
    {
        if (lifeSpan < 0.0f)
            return true;
        else
            return false;
    }

}
