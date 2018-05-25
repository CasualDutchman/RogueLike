using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public SpriteRenderer renderHairFront, renderHairBack, renderHead, renderBody, renderLHand, renderRHand, renderGun;

    public CharacterCustom custom;

    public Vector3 originHairFront, originHairBack, originHead, originBody, originLHand, originRHand, originGun;

    CharacterInfo info;

    bool front = true;
    bool left = true;

    Vector3 o, p;

    void Start () {
        info = custom.GetRandomCharacter();
        renderHairFront.sprite = info.hair.front;
        renderHairBack.sprite = info.hair.back;
        renderHead.sprite = info.head.front;
        renderBody.sprite = info.body.front;
        renderLHand.sprite = info.hand;
        renderRHand.sprite = info.hand;

        originHairFront = renderHairFront.transform.localPosition;
        originHairBack = renderHairBack.transform.localPosition;
        originHead = renderHead.transform.localPosition;
        originBody = renderBody.transform.localPosition;
        originLHand = renderLHand.transform.localPosition;
        originRHand = renderRHand.transform.localPosition;

        if (renderGun != null)
            originGun = renderGun.transform.localPosition;
    }

    public void FollowArm(Vector3 origin, Vector3 target) {
        Vector3 dir = new Vector3(origin.x, 0, origin.y) - new Vector3(target.x, 0, target.y);
        Debug.Log(dir);
        o = new Vector3(origin.x, 0, origin.y);
        Debug.Log(o);
        p = new Vector3(target.x, 0, target.y);
        Debug.Log(p);

        Vector3 d = Quaternion.LookRotation(dir).eulerAngles;
        Vector3 f = new Vector3(0, 0, d.y);
        renderRHand.transform.localEulerAngles = f;
    }

    void OnDrawGizmosSelected() {
        if(renderRHand != null) {
            Gizmos.DrawLine(transform.position + o, transform.position + p);
        }
    }

    public void UpdateCharacter(float x, float z) {
        if(z > 0 && front) {
            ChangeToBack();
            front = false;
        }else if(z <= 0 && !front) {
            ChangeToFront();
            front = true;
        }

        if (x > 0 && left) {
            HorizontalFlip(true);
            left = false;
        } else if (x < 0 && !left) {
            HorizontalFlip(false);
            left = true;
        }
    }

    void HorizontalFlip(bool b) {
        renderHairFront.flipX = b;
        renderHairBack.flipX = !b;
        renderHead.flipX = b;
        renderBody.flipX = b;

        if (renderGun != null) {
            renderGun.flipX = b;
            Vector3 v3 = originGun;
            v3.x = originGun.x * (b ? -1 : 1);
            renderGun.transform.localPosition = v3;
        }
    }

    void ChangeToFront() {
        Vector3 v = originHairFront;
        v.z = -0.0008f;
        renderHairFront.transform.localPosition = v;

        v = originHairBack;
        v.z = 0.0008f;
        renderHairBack.transform.localPosition = v;

        renderHead.sprite = info.head.front;
        renderBody.sprite = info.body.front;

        renderLHand.transform.localPosition = originLHand;

        renderRHand.transform.localPosition = originRHand;
    }

    void ChangeToBack() {
        Vector3 v = originHairFront;
        v.z = 0.0008f;
        renderHairFront.transform.localPosition = v;

        v = originHairBack;
        v.z = -0.0008f;
        renderHairBack.transform.localPosition = v;

        renderHead.sprite = info.head.back;
        renderBody.sprite = info.body.back;

        v = originLHand;
        v.z += 0.02f;
        renderLHand.transform.localPosition = v;

        v = originRHand;
        v.z += 0.02f;
        renderRHand.transform.localPosition = v;
    }
}
