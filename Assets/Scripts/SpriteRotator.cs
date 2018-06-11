using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//keep the child rotated to the parent
public class SpriteRotator : MonoBehaviour {
	
	void Update () {
        transform.localEulerAngles = new Vector3(0, -transform.parent.eulerAngles.y, 0);
    }
}
