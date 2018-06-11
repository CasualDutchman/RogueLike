using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour {

    public static CameraRig instance;

    public Transform target;
    public AnimationCurve curve;
    public float influence;

    void Awake() {
        instance = this;
    }

    //set the camera rig to the position between the character and the mouse
    void Update() {
        Vector3 player = Camera.main.WorldToViewportPoint(target.position);
        Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Vector3 offset = mouse - player;
        transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x + offset.x * influence, 0, target.position.z + offset.y * influence), Time.deltaTime * 5f);
    }
}
