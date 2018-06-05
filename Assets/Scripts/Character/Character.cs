using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public SpriteRenderer renderHairFront, renderHead, renderFace, renderBody, renderLHand, renderRHand, renderGun;

    public Transform barrel;

    public CharacterCustom custom;

    Vector3 originHairFront = new Vector3(0, 0.0156f, -0.0008f);
    Vector3 originHairBack = new Vector3(0, -0.0149f, -0.0008f);
    Vector3 originHead = new Vector3(0, 1.223853f, -0.33902f);
    Vector3 originFace = new Vector3(0, 0, -0.0004f);
    Vector3 originBody = new Vector3(0, 0.369838f, -0.33467f);
    Vector3 originLHand = new Vector3(0.25f, 0, -0.0001f);
    Vector3 originRHand = new Vector3(-0.495f, 0.363f, -0.35f);
    Vector3 originGun = new Vector3(0.4f, 0.25f, 0);
    Vector3 originBarrel = new Vector3(1, 0.45f, 0);

    public Vector3 currentRHand, currentLHand;

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

    public bool aimOverride;

    void Start () {
        //originHairFront = renderHairFront.transform.localPosition;
        //originHairBack = new Vector3(0, -0.0149f, -0.0008f);
        //originHead = renderHead.transform.localPosition;
        //originFace = renderFace.transform.localPosition;
        //originBody = renderBody.transform.localPosition;
        //originLHand = renderLHand.transform.localPosition;
        //originRHand = renderRHand.transform.localPosition;
        //originBarrel = barrel.localPosition;

        currentLHand = originLHand;
        currentRHand = originRHand;

        if (renderGun != null)
            originGun = renderGun.transform.localPosition;
    }

    public void SetNewCharacter(CharacterInfo ci) {
        info = ci;
        renderHairFront.sprite = front ? ci.hair.front : ci.hair.back;
        renderHead.sprite = ci.head;
        renderFace.sprite = ci.face;
        renderBody.sprite = front ? ci.body.front: ci.body.back;
        renderLHand.sprite = ci.hand;
        renderRHand.sprite = ci.hand;

        ChangeToFront();
    }

    public float AngleFromBarrel() {
        return zRotCurve.Evaluate(angle / 360.0f) * 360.0f;
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

        ChangeHandPosition();
    }

    public void AimGun(Vector3 origin, Vector3 target) {
        if (aimOverride)
            return;

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
        renderHairFront.flipX = front ? b : !b;
        renderHead.flipX = b;
        renderBody.flipX = b;
        renderFace.flipX = b;

        //renderGun.flipX = b;
        //renderGun.flipY = b;

        //ChangeHandPosition();
        //currentLHand = b ? originRHand : originLHand;
        //currentRHand = b ? originLHand : originRHand;
    }

    void ChangeToFront() {
        renderHairFront.transform.localPosition = originHairFront;
        if(info != null)
            renderHairFront.sprite = info.hair.front;
        renderHairFront.flipX = !left;

        renderBody.sprite = info.body.front;

        renderFace.transform.localPosition = originFace;

        //renderGun.flipY = false;

        //ChangeHandPosition();
    }

    void ChangeToBack() {
        renderHairFront.transform.localPosition = originHairBack;
        if (info != null)
            renderHairFront.sprite = info.hair.back;
        renderHairFront.flipX = left;

        if (info != null)
            renderBody.sprite = info.body.back;

        Vector3 v = originFace;
        v.z = -v.z;
        renderFace.transform.localPosition = v;

        //renderGun.flipY = true;

        //ChangeHandPosition();
    }

    void ChangeHandPosition() {
        //currentLHand = front ? originLHand : originRHand;
        //currentRHand = front ? originRHand : originLHand;

        renderGun.flipY = !left;

        Vector3 v = currentLHand;
        v.z += front ? 0 : 0.002f;
        //renderLHand.transform.localPosition = v;

        v = currentRHand;
        //v.z += front ? 0 : 0.002f;
        renderRHand.transform.localPosition = v;

        Vector3 v3 = originGun;
        v3.y = originGun.y * (renderGun.flipY ? -1 : 1);
        renderGun.transform.localPosition = v3;

        v3 = originBarrel;
        v3.y = originBarrel.y * (renderGun.flipY ? -1 : 1);
        barrel.localPosition = v3;
    }
}
