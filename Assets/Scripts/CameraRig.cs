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

    void Update() {
        Vector3 player = Camera.main.WorldToViewportPoint(target.position);
        Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Vector3 offset = mouse - player;
        transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x + offset.x * influence, 0, target.position.z + offset.y * influence), Time.deltaTime * 5f);
    }

    public void SetNewPosition(Vector3 newpos) {
        //StartCoroutine(SetPos(newpos));
    }

    IEnumerator SetPos(Vector3 newpos) {
        Vector3 currentPos = transform.position;
        bool moving = true;
        float timer = 0;

        while (moving) {
            timer += Time.deltaTime;

            transform.position = Vector3.Lerp(currentPos, newpos, curve.Evaluate(timer));

            if (timer >= 1) {
                moving = true;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        yield return 0;
    }
}
