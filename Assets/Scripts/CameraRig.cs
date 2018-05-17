using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour {

    public static CameraRig instance;

    public AnimationCurve curve;

    void Awake() {
        instance = this;
    }

    public void SetNewPosition(Vector3 newpos) {
        StartCoroutine(SetPos(newpos));
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
