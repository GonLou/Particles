using UnityEngine;
using System.Collections;

public class Audio : MonoBehaviour {
    public int startingPitch = 4;
    public int timeToDecrease = 5;
    AudioSource audio;
	// Use this for initialization
	void Start () 
    {
        audio = GetComponent<AudioSource>();
        audio.pitch = startingPitch;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (audio.pitch > 0)
            audio.pitch -= Time.deltaTime * startingPitch / timeToDecrease;
	}
}
