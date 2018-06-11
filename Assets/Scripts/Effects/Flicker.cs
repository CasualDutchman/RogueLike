using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//changed the intensity of a light in a random way
public class Flicker : MonoBehaviour {

    public Light lightbase;
    public Vector2 intentity = new Vector2(0.9f, 1.2f);
    public float changeRate = 0.1f;
    float timer;

	void Update () {
        timer += Time.deltaTime;
        if (timer >= changeRate) {
            lightbase.intensity = Random.Range(intentity.x, intentity.y);
            timer = 0;
        }
	}
}
