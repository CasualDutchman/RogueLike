using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRotator : MonoBehaviour {
	
	void Update () {
        transform.localEulerAngles = new Vector3(0, -transform.parent.eulerAngles.y, 0);
    }
}
