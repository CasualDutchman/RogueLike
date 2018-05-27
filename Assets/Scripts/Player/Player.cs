using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, IAttackable {

    Character character;

    public float health;
    public Weapon weapon;

    public int currentAmmo;
    public int maxAmmo;
    public Transform ammoHolder;
    public Material ammoGrey;

    bool reloading = false;

    public LayerMask bulletMask;

    public GameObject bulletPrefab;

    float firing;

	void Start () {
        character = GetComponent<Character>();

        SetWeapon(weapon);
    }
	
	void Update () {
        if (Input.GetMouseButton(0) && weapon != null) {
            if (!weapon.needFullReload)
                reloading = false;

            if ((!reloading)) {
                if (!reloading || weapon.needFullReload)
                    if (firing <= 0) {
                        FireWeapon();
                    }
                firing += Time.deltaTime;
                if (firing >= 1 / weapon.fireRate) {
                    firing = 0;
                }
            }
        }else if (Input.GetMouseButtonDown(0)) {
            firing = 0;
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            reloading = true;
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload() {
        float timer = 0;

        while (reloading) {
            timer += Time.deltaTime;

            float perBullet = weapon.timeTillFullReload / weapon.maxAmmo;
            
            if (timer >= perBullet) {
                currentAmmo++;
                ammoHolder.GetChild(maxAmmo - currentAmmo).GetComponent<Image>().material = null;

                if (currentAmmo >= maxAmmo) {
                    currentAmmo = maxAmmo;
                    reloading = false;
                }

                timer = 0;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void Damage(float f) {
        health -= f;
    }

    void FireWeapon() {
        if (weapon == null)
            return;

        if (currentAmmo <= 0)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(character.barrel.position);

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Plane hPlane = new Plane(Vector3.up, Vector3.zero);
        float distance = 0;
        if (hPlane.Raycast(ray, out distance)) {
            GameObject go = Instantiate(bulletPrefab);
            go.transform.position = ray.GetPoint(distance) + Vector3.up * 0.2f;
            Bullet b = go.GetComponent<Bullet>();
            b.SetBullet(weapon.bulletSprite, character.angle, weapon.bulletSpeed, bulletMask, weapon.damage);

            ammoHolder.GetChild(maxAmmo - currentAmmo).GetComponent<Image>().material = ammoGrey;
            currentAmmo--;
        }
    }

    void SetWeapon(Weapon w) {
        //if (weapon == w)
        //    return;

        character.SetWeapon(w);

        reloading = false;

        currentAmmo = weapon.maxAmmo;
        maxAmmo = weapon.maxAmmo;

        while (ammoHolder.childCount > 0) {
            Destroy(ammoHolder.GetChild(0));
        }

        for (int i = 0; i < maxAmmo; i++) {
            GameObject go = new GameObject("bullet UI " + i);
            go.transform.SetParent(ammoHolder);
            Image im = go.AddComponent<Image>();
            im.sprite = weapon.bulletSpriteUI;
            go.GetComponent<RectTransform>().sizeDelta = im.sprite.rect.size * 5;

        }

    }
}
