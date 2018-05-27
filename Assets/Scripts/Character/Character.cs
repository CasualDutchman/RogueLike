using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public SpriteRenderer renderHairFront, renderHairBack, renderHead, renderBody, renderLHand, renderRHand, renderGun;

    public Transform barrel;

    public CharacterCustom custom;

    public Vector3 originHairFront, originHairBack, originHead, originBody, originLHand, originRHand, originGun, originBarrel;
    Vector3 currentRHand, currentLHand;

    public AnimationCurve zRotCurve;

    [Header("one handed")]
    public AnimationCurve xRotCurveOne;
    public AnimationCurve xPosCurveOne, zPosCurveOne;

    [Header("Two handed")]
    public AnimationCurve xRotCurveTwo;
    public AnimationCurve xPosCurveTwo, zPosCurveTwo;

    CharacterInfo info;

    Handed handed = Handed.One;

    bool front = true;
    bool left = true;

    public float angle;

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
        originBarrel = barrel.localPosition;

        currentLHand = originLHand;
        currentRHand = originRHand;

        if (renderGun != null)
            originGun = renderGun.transform.localPosition;
    }

    public void SetWeapon(Weapon weapon) {
        if (weapon == null)
            return;

        handed = weapon.handed;

        renderGun.sprite = weapon.sprite;

        originLHand = new Vector3(weapon.LHandOffset.x, weapon.LHandOffset.y, -0.0001f);
        renderLHand.transform.localPosition = originLHand;
        originGun = weapon.spriteOffset;
        originBarrel = weapon.barrelOffset;
    }

    public void AimGun(Vector3 origin, Vector3 target) {
        Vector3 dir = origin - target;

        angle = Quaternion.LookRotation(dir.normalized).eulerAngles.y;

        if (handed == Handed.Two) {
            float rotZ = 1 - zRotCurve.Evaluate(angle / 360.0f);
            float rotX = 1 - xRotCurveTwo.Evaluate(angle / 360.0f);
            Vector3 rot = new Vector3(-rotX * 360.0f, 0, rotZ * 360.0f + 90);
            renderRHand.transform.localEulerAngles = rot;

            float posZ = zPosCurveTwo.Evaluate(1 - (angle / 360.0f));
            float posX = xPosCurveTwo.Evaluate(1 - (angle / 360.0f));
            Vector3 pos = new Vector3(posX, 0, posZ);
            renderRHand.transform.localPosition = currentRHand + pos;
        } else {
            float rotZ = 1 - zRotCurve.Evaluate(angle / 360.0f);
            float rotX = 1 - xRotCurveOne.Evaluate(angle / 360.0f);
            Vector3 rot = new Vector3(-rotX * 360.0f, 0, rotZ * 360.0f + 90);
            renderRHand.transform.localEulerAngles = rot;

            float posZ = zPosCurveOne.Evaluate(1 - (angle / 360.0f));
            float posX = xPosCurveOne.Evaluate(1 - (angle / 360.0f));
            Vector3 pos = new Vector3(posX, 0, posZ);
            renderRHand.transform.localPosition = currentRHand + pos;
        }
    }

    public void UpdateCharacter(float x, float z) {
        bool tL = left, tF = front;

        if (x > 0 && left) {
            HorizontalFlip(true);
            left = false;
        } else if (x < 0 && !left) {
            HorizontalFlip(false);
            left = true;
        }

        if (z > 0 && front) {
            ChangeToBack();
            front = false;
        }else if(z <= 0 && !front) {
            ChangeToFront();
            front = true;
        }

        if(tL != left || tF != front)
            ChangeHandPosition();
    }

    void HorizontalFlip(bool b) {
        renderHairFront.flipX = b;
        renderHairBack.flipX = !b;
        renderHead.flipX = b;
        renderBody.flipX = b;

        //renderGun.flipX = b;
        //renderGun.flipY = b;

        //ChangeHandPosition();
        //currentLHand = b ? originRHand : originLHand;
        //currentRHand = b ? originLHand : originRHand;
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

        //renderGun.flipY = false;

        //ChangeHandPosition();
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

        //renderGun.flipY = true;

        //ChangeHandPosition();
    }

    void ChangeHandPosition() {
        currentLHand = front ? originLHand : originRHand;
        currentRHand = front ? originRHand : originLHand;

        renderGun.flipY = !left;

        Vector3 v = currentLHand;
        v.z += front ? 0 : 0.02f;
        //renderLHand.transform.localPosition = v;

        v = currentRHand;
        v.z += front ? 0 : 0.02f;
        renderRHand.transform.localPosition = v;

        Vector3 v3 = originGun;
        v3.y = originGun.y * (renderGun.flipY ? -1 : 1);
        renderGun.transform.localPosition = v3;

        v3 = originBarrel;
        v3.y = originBarrel.y * (renderGun.flipY ? -1 : 1);
        barrel.localPosition = v3;
    }
}
