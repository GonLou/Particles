using UnityEngine;
using System.Collections;

public class Particle
{
    Vector3 location;
    Vector3 velocity;
    float acceleration;

    float lifeSpan;

    Color color;

    public Particle(Vector3 location, Vector3 velocity, float acceleration, float lifeSpan, Color color)
    {
        this.location = location;
        this.velocity = velocity;
        this.acceleration = acceleration;
        if (!isDead()) this.lifeSpan = lifeSpan;
        this.color = color;
    }

    public bool Update()
    {
        velocity = velocity - Particles.Outside.getInstance().Gravity
                            + Particles.Outside.getInstance().Wind;
        location = location + velocity;
        lifeSpan--;
        if (isDead())
            return false;
        return true;
    }

    public Vector3 Location 
    {
        get {return location;}
    }

    public bool isDead()
    {
        if (lifeSpan < 0.0f)
            return true;
        else
            return false;
    }

}
