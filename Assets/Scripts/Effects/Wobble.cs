using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wobble : MonoBehaviour {

    public Transform wobbler;
    public Vector3 min, max;
    public float changeRate;
    float timer;
    Vector3 basis;
    Vector3 go;

    void Start() {
        basis = wobbler.localPosition;
    }

	void Update () {
        timer += Time.deltaTime;
        if (timer >= changeRate) {
            go = basis + new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
            timer = 0;
        }

        wobbler.localPosition = Vector3.Lerp(wobbler.localPosition, go, Time.deltaTime);
	}
}
