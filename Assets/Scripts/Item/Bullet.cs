using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    LayerMask hitMask;

    float timerAlive;

    public AudioClip hitSound;
    public GameObject audioPrefab;

	public void SetBullet(Sprite bulletSprite, float angle, LayerMask mask) {
        GetComponent<SpriteRenderer>().sprite = bulletSprite;
        transform.localEulerAngles = new Vector3(90, angle + 180, 0);
        hitMask = mask;
    }
	
	void Update () {
        timerAlive += Time.deltaTime;
        if (timerAlive >= 10) {
            Destroy(gameObject);
        }

        transform.Translate(Vector3.up * Time.deltaTime * 4, Space.Self);

        Ray ray = new Ray(transform.position - transform.up, transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.2f, hitMask)){
            GameObject go = Instantiate(audioPrefab);
            go.transform.position = transform.position;
            AudioSource source = go.GetComponent<AudioSource>();

            bool remove = true;
            if (hit.collider.GetComponent<IAttackable>() != null) {
                IAttackable att = hit.collider.GetComponent<IAttackable>();
                Debug.Log(hit.collider.name);
                att.Damage(1);
                source.clip = att.GetHitClip();
            }else {
                source.clip = hitSound;
            }

            source.pitch = Random.Range(0.95f, 1.05f);
            source.volume = Random.Range(0.1f, 0.2f);

            if (remove) {
                source.Play();
                Destroy(gameObject);
            }
        }
    }
}