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
        acceleration.y += 0.1f;
        if (acceleration.y > 2) acceleration.y = 1;

        acceleration = new Vector3(acceleration.x, acceleration.y, acceleration.z);

        velocity += (acceleration - Particles.Outside.getInstance().Gravity
                                  + Particles.Outside.getInstance().Wind);

        location += velocity;
        lifeSpan = lifeSpan - 0.1f;
        if (isDead())
            return false;
        return true;
    }

    public Vector3 Location 
    {
        get {return location;}
    }

    public Vector3 Acceleration
    {
        get { return acceleration; }
        set { acceleration = value; }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }

    public Vector3 velocityChange()
    {
        if (-velocity.y < 0) velocity.y = 0.0f;
        return new Vector3(-velocity.x, -velocity.y, -velocity.z);
    }

    public bool isDead()
    {
        if (lifeSpan < 0.0f)
            return true;
        else
            return false;
    }

}
