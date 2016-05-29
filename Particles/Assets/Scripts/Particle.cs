using UnityEngine;
using System.Collections;

public class Particle
{
    Vector3 location;
    Vector3 velocity;
    Vector3 acceleration;

    float lifeSpan;

    Color color;

    public Particle(Vector3 location, Vector3 velocity, Vector3 acceleration, float lifeSpan, Color color)
    {
        this.location = location;
        this.velocity = velocity;
        this.acceleration = acceleration;
        if (!isDead()) this.lifeSpan = lifeSpan;
        this.color = color;
    }

    public bool Update()
    {
        provideAcceleration();
        velocity += acceleration - Particles.Outside.getInstance().Gravity
                            + Particles.Outside.getInstance().Wind;
        location += velocity;
        lifeSpan--;
        if (isDead())
            return false;
        return true;
    }

    void provideAcceleration()
    {
        acceleration.y = acceleration.y + 0.000001f;
        acceleration = new Vector3(acceleration.x, acceleration.y, acceleration.z);
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
