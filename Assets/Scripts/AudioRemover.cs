using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//remove the AudioSource GameObject when the audio is done playing
public class AudioRemover : MonoBehaviour {

    AudioSource source;

	void Start () {
        source = GetComponent<AudioSource>();
	}
	
	void Update () {
        if (!source.isPlaying) {
            Destroy(gameObject);
        }
	}
}
