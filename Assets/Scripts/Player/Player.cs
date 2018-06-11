using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//this class keeps track of the individual weapon-ammo
//when a player switched weapon, this keeps track of the ammo count
[System.Serializable]
public class WeaponInfo {
    public int currentAmmo;
    public int maxClipSize;
    public int ammoInventory;
}

public class Player : MonoBehaviour, IAttackable {

    Character character;

    public AudioSource audioSource;

    public int health;
    public int weaponIndex;
    public Weapon testWeapon1, testWeapon2;
    Weapon[] holdingWeapons = new Weapon[3];
    WeaponInfo[] weaponInfo = new WeaponInfo[3];

    Weapon currentWeapon;
    WeaponInfo currentWeaponInfo;
    public Transform ammoHolder;
    public Image uiGun;
    public Text uiAmmoText;

    public RectTransform crosshair;

    bool reloading = false;

    public LayerMask bulletMask;

    public GameObject bulletPrefab;

    public AudioClip hitClip;

    public Sprite heartFull, heartHalf, heartEmpty;
    public Image[] hearts;

    float firing;

    bool invincible = false;
    float invincibleTimer;

	void Start () {
        character = GetComponent<Character>();

        AddWeapon(testWeapon1);
        AddWeapon(testWeapon2);

        SetWeapon(0);
    }
	
	void Update () {
        //Cursor.visible = false;
        //crosshair.anchoredPosition = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SetWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SetWeapon(1);
        }

        if (Input.GetMouseButton(0) && currentWeapon != null) {
            if (!currentWeapon.needFullReload) {
                if (reloading) {
                    uiGun.sprite = currentWeapon.sprite;
                    character.renderGun.sprite = currentWeapon.sprite;
                }
                reloading = false;
            }

            if ((!reloading)) {
                if (!reloading || currentWeapon.needFullReload)
                    if (firing <= 0) {
                        FireWeapon();
                    }
                firing += Time.deltaTime;
                if (firing >= 1 / currentWeapon.fireRate) {
                    firing = 0;
                }
            }
        }else if (Input.GetMouseButtonDown(0)) {
            firing = 0;
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            if (currentWeaponInfo.currentAmmo < currentWeaponInfo.maxClipSize) {
                reloading = true;
                StartCoroutine(Reload());
            }
        }

        if (invincible) {
            invincibleTimer += Time.deltaTime;
            if (invincibleTimer >= 1.2f) {
                invincibleTimer = 0;
                invincible = false;
            }
        }
    }

    //used for when picking up weapon
    //not used, only for testing
    public void AddWeapon(Weapon w) {
        for (int i = 0; i < holdingWeapons.Length; i++) {
            if(holdingWeapons[i] == null) {
                holdingWeapons[i] = w;
                WeaponInfo info = new WeaponInfo();
                info.maxClipSize = w.maxAmmo;
                info.currentAmmo = w.maxAmmo;
                info.ammoInventory = w.maxAmmo * 5;
                weaponInfo[i] = info;
                break;
            }
        }
    }

    //when reloading
    IEnumerator Reload() {
        float timer = 0;

        uiGun.sprite = currentWeapon.reloadSprite;
        character.renderGun.sprite = currentWeapon.reloadSprite;

        while (reloading) {
            timer += Time.deltaTime;

            float perBullet = currentWeapon.timeTillFullReload / currentWeapon.maxAmmo;
            
            if (timer >= perBullet) {
                if (currentWeaponInfo.ammoInventory > 0 || currentWeapon.infiniteAmmo) {
                    currentWeaponInfo.currentAmmo++;
                    ammoHolder.GetChild(currentWeaponInfo.maxClipSize - currentWeaponInfo.currentAmmo).GetComponent<Image>().sprite = currentWeapon.bulletSpriteUI_On;

                    if (!currentWeapon.infiniteAmmo)
                        currentWeaponInfo.ammoInventory--;

                    uiAmmoText.text = currentWeaponInfo.currentAmmo + "/" + (currentWeapon.infiniteAmmo ? "<size=30>\u221E</size>" : currentWeaponInfo.ammoInventory.ToString());

                    if (currentWeaponInfo.currentAmmo >= currentWeaponInfo.maxClipSize) {
                        currentWeaponInfo.maxClipSize = currentWeaponInfo.currentAmmo;
                        reloading = false;
                        uiGun.sprite = currentWeapon.sprite;
                        character.renderGun.sprite = currentWeapon.sprite;
                    }

                    timer = 0;
                }else {
                    reloading = false;
                    uiGun.sprite = currentWeapon.sprite;
                    character.renderGun.sprite = currentWeapon.sprite;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public bool Damage(int f) {
        if (invincible)
            return false;

        health -= f;
        UpdateHearts();

        invincible = true;
        return true;
    }

    void UpdateHearts() {
        for (int i = 0; i < hearts.Length; i++) {
            if (i > Mathf.FloorToInt(health / 2f) - 1) {
                if ((float)i > (health / 2f) - 0.5f) {
                    hearts[i].sprite = heartEmpty;
                } else {
                    hearts[i].sprite = heartHalf;
                }
            } 
            else
            {
                hearts[i].sprite = heartFull;
            }
        }
    }

    public AudioClip GetHitClip() {
        return hitClip;
    }

    //when firing a weapon
    void FireWeapon() {
        if (currentWeapon == null)
            return;

        if (currentWeaponInfo.currentAmmo <= 0)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(character.barrel.position);

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Plane hPlane = new Plane(Vector3.up, Vector3.zero);
        float distance = 0;
        if (hPlane.Raycast(ray, out distance)) {
            if (currentWeapon.shootPattern == ShootPattern.Straight) {
                SpawnBullet(ray.GetPoint(distance), Random.Range(-currentWeapon.recoilFactor, currentWeapon.recoilFactor));
            }else if (currentWeapon.shootPattern == ShootPattern.Three) {
                for (int i = 0; i < 3; i++) {
                    float baseOffset = Random.Range(-currentWeapon.recoilFactor, currentWeapon.recoilFactor);
                    float offset = 15;
                    SpawnBullet(ray.GetPoint(distance), baseOffset + (-offset + i * offset));
                }
            } else if (currentWeapon.shootPattern == ShootPattern.Five) {
                for (int i = 0; i < 5; i++) {
                    float baseOffset = Random.Range(-currentWeapon.recoilFactor, currentWeapon.recoilFactor);
                    float offset = 15;
                    SpawnBullet(ray.GetPoint(distance), baseOffset + (-(offset * 2) + i * offset));
                }
            }

            ammoHolder.GetChild(currentWeaponInfo.maxClipSize - currentWeaponInfo.currentAmmo).GetComponent<Image>().sprite = currentWeapon.bulletSpriteUI_Off;
            currentWeaponInfo.currentAmmo--;

            uiAmmoText.text = currentWeaponInfo.currentAmmo + "/" + (currentWeapon.infiniteAmmo ? "<size=30>\u221E</size>" : currentWeaponInfo.ammoInventory.ToString());

            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.clip = currentWeapon.weaponShot;
            audioSource.Play();
        }
    }

    //spawning a bullet
    void SpawnBullet(Vector3 pos, float addedAngle) {
        GameObject go = Instantiate(bulletPrefab);
        go.transform.position = pos + Vector3.up * 0.2f;
        Bullet b = go.GetComponent<Bullet>();

        Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mouse = new Vector3(mouse.x, 0, mouse.y);

        float angle = character.AngleFromBarrel();
        angle += addedAngle;

        b.SetBullet(currentWeapon.bulletSprite, angle, bulletMask);
    }

    //when changing weapon, set new information, change ui etc
    void SetWeapon(int index) {
        if (weaponIndex == index)
            return;

        weaponIndex = index;

        currentWeapon = holdingWeapons[weaponIndex];
        currentWeaponInfo = weaponInfo[weaponIndex];

        character.SetWeapon(currentWeapon);

        reloading = false;

        int max = ammoHolder.childCount;
        for (int i = 0; i < max; i++) {
            Destroy(ammoHolder.GetChild(i).gameObject);
        }

        for (int i = 0; i < currentWeaponInfo.maxClipSize; i++) {
            GameObject go = new GameObject("bullet UI " + i);
            go.transform.SetParent(ammoHolder);
            Image im = go.AddComponent<Image>();
            im.sprite = currentWeaponInfo.maxClipSize - currentWeaponInfo.currentAmmo <= i ? currentWeapon.bulletSpriteUI_On : currentWeapon.bulletSpriteUI_Off;
            go.GetComponent<RectTransform>().sizeDelta = im.sprite.rect.size * 5;

        }

        uiGun.sprite = currentWeapon.sprite;
        uiGun.rectTransform.sizeDelta = currentWeapon.sprite.rect.size * 4;
        uiAmmoText.text = currentWeaponInfo.currentAmmo + "/" + (currentWeapon.infiniteAmmo ? "<size=30>\u221E</size>" : currentWeaponInfo.ammoInventory.ToString());
    }
}
