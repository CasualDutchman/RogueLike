using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    float speed;
    LayerMask hitMask;
    float damage;

    float timerAlive;

	public void SetBullet(Sprite bulletSprite, float angle, float s, LayerMask mask, float d) {
        GetComponent<SpriteRenderer>().sprite = bulletSprite;
        transform.localEulerAngles = new Vector3(90, angle + 180, 0);
        speed = s;
        hitMask = mask;
        damage = d;
    }
	
	void Update () {
        timerAlive += Time.deltaTime;
        if (timerAlive >= 10) {
            Destroy(gameObject);
        }

        transform.Translate(Vector3.up * Time.deltaTime * speed, Space.Self);

        Ray ray = new Ray(transform.position - transform.up, transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.2f, hitMask)){
            if (hit.collider.GetComponent<IAttackable>() != null) {
                IAttackable att = hit.collider.GetComponent<IAttackable>();
                att.Damage(damage);
            }
            Destroy(gameObject);
        }
    }
}