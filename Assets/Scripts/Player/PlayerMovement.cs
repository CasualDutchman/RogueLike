using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    CharacterController controller;
    Character character;

    public float speed = 6.0F;
    private float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    public AnimationCurve bobCurve, idleHeadCurve, idleHandCurve;
    public float idleRate;
    float timer;
    bool up = true;

    void Start () {
        controller = GetComponent<CharacterController>();
        character = GetComponent<Character>();
    }
	
	void Update () {
        if (controller.isGrounded) {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            //moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);

        Vector3 v3 = new Vector3(moveDirection.x, 0, moveDirection.z);
        Animate(v3.magnitude);
    }

    void Animate(float magnitude) {
        timer += Time.deltaTime * (magnitude > float.Epsilon ? (up ? magnitude : -magnitude) : (up ? idleRate : -idleRate));
        if (timer >= 1) {
            up = false;
        }else if (timer < 0) {
            up = true;
        }
        if (magnitude > float.Epsilon) {
            Vector3 v3 = Vector3.Lerp(new Vector3(-0.1f, 0, 0), new Vector3(0.1f, 0, 0), timer);
            v3.y = bobCurve.Evaluate(timer);
            transform.GetChild(0).localPosition = v3;

            Vector3 r3 = Vector3.Lerp(new Vector3(0, 0, 6f), new Vector3(0, 0, -6f), timer);
            transform.GetChild(0).localEulerAngles = r3;
        }else {
            transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, new Vector3(0, 0, 0), Time.deltaTime);
            transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 10);

            character.renderHead.transform.localPosition = character.originHead + character.renderHead.transform.up * (idleHeadCurve.Evaluate(timer) * 0.5f);
        }
    }
}
