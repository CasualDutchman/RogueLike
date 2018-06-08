using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Chase, Attack, Reload }

public class Enemy : MonoBehaviour, IAttackable {

    NavMeshAgent agent;
    Character character;

    public Transform target;

    public int health;

    public float distanceFromPlayer;
    EnemyState enemyState = EnemyState.Chase;
    float attackTimer;
    float waitTimer;
    float reloadTimer;

    public GameObject bulletPrefab;

    public AnimationCurve bobCurve;
    public float rate;
    float timer;
    bool up = true;

    public LayerMask bulletMask;
    public LayerMask betweenMask;

    public Weapon weapon;
    int ammo;
    public AudioClip hitClip;

    float rotY;

    void Start () {
        agent = GetComponent<NavMeshAgent>();
        character = GetComponent<Character>();

        character.SetWeapon(weapon);
        character.SetNewCharacter(character.custom.GetRandomCharacter());
        ammo = weapon.maxAmmo;
    }
	
	void Update () {
        if (enemyState == EnemyState.Chase) {
            RaycastHit hit;
            if (Physics.Linecast(transform.position + Vector3.one * 0.5f, target.position + Vector3.one * 0.5f, out hit, betweenMask)) {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Room")) {
                    agent.SetDestination(target.position);
                } else {
                    //if (Vector3.Distance(transform.position, target.position) > distanceFromPlayer) {
                    //    agent.SetDestination(target.position);
                    //} else {
                        agent.ResetPath();
                        enemyState = EnemyState.Attack;
                    //}
                }
            } else {
                if(Vector3.Distance(transform.position, target.position) > distanceFromPlayer) {
                    agent.SetDestination(target.position);
                } else {
                    agent.ResetPath();
                    enemyState = EnemyState.Attack;
                }
            }
        } 
        else if(enemyState == EnemyState.Attack) {
            RaycastHit hit;
            if (Physics.Linecast(transform.position + Vector3.one * 0.5f, target.position + Vector3.one * 0.5f, out hit, betweenMask)) {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Room")) {
                    waitTimer += Time.deltaTime;
                    if(waitTimer >= 3)
                        enemyState = EnemyState.Chase;
                }
            }

            attackTimer += Time.deltaTime;
            if (attackTimer >= 1 / weapon.fireRate) {
                FireWeapon();
                attackTimer = 0;
                if (ammo <= 0) {
                    enemyState = EnemyState.Reload;
                    character.renderGun.sprite = weapon.reloadSprite;
                }
            }
        }
        else if (enemyState == EnemyState.Reload) {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= weapon.timeTillFullReload * 3) {
                ammo = weapon.maxAmmo;
                reloadTimer = 0;
                character.renderGun.sprite = weapon.sprite;

                if (Vector3.Distance(transform.position, target.position) > distanceFromPlayer) {
                    enemyState = EnemyState.Chase;
                } else {
                    enemyState = EnemyState.Attack;
                }
            }
        }

        rotY = -transform.eulerAngles.y;

        Vector3 v3 = new Vector3(agent.velocity.x, 0, agent.velocity.z);
        Animate(v3.magnitude);

        //character.UpdateCharacter(agent.velocity.x, agent.velocity.z);

        Vector3 self = Camera.main.ScreenToViewportPoint(transform.position);
        self = new Vector3(self.x, 0, self.y);
        Vector3 player = Camera.main.WorldToViewportPoint(target.position);
        player = new Vector3(player.x, 0, player.y);
        Vector3 v = self - player;
        character.UpdateCharacter(v.x, v.z);
        character.AimGun(transform.position, target.position);
    }

    void FireWeapon() {
        if (weapon == null)
            return;

        if (ammo <= 0)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(character.barrel.position);

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Plane hPlane = new Plane(Vector3.up, Vector3.zero);
        float distance = 0;
        if (hPlane.Raycast(ray, out distance)) {
            if (weapon.shootPattern == ShootPattern.Straight) {
                SpawnBullet(ray.GetPoint(distance), Random.Range(-weapon.recoilFactor, weapon.recoilFactor));
            } else if (weapon.shootPattern == ShootPattern.Three) {
                for (int i = 0; i < 3; i++) {
                    float baseOffset = Random.Range(-weapon.recoilFactor, weapon.recoilFactor);
                    float offset = 15;
                    SpawnBullet(ray.GetPoint(distance), baseOffset + (-offset + i * offset));
                }
            } else if (weapon.shootPattern == ShootPattern.Five) {
                for (int i = 0; i < 5; i++) {
                    float baseOffset = Random.Range(-weapon.recoilFactor, weapon.recoilFactor);
                    float offset = 15;
                    SpawnBullet(ray.GetPoint(distance), baseOffset + (-(offset * 2) + i * offset));
                }
            }

            ammo--;

            //audioSource.pitch = Random.Range(0.95f, 1.05f);
            //audioSource.clip = currentWeapon.weaponShot;
            //audioSource.Play();
        }
    }

    void SpawnBullet(Vector3 pos, float addedAngle) {
        GameObject go = Instantiate(bulletPrefab);
        go.transform.position = pos + Vector3.up * 0.2f;
        Bullet b = go.GetComponent<Bullet>();

        Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mouse = new Vector3(mouse.x, 0, mouse.y);

        float angle = character.AngleFromBarrel();
        angle += addedAngle;

        b.SetBullet(weapon.bulletSprite, angle, bulletMask);
    }

    public bool Damage(int f) {
        health -= f;
        if (health <= 0) {
            Destroy(gameObject);
        }

        return true;
    }

    public AudioClip GetHitClip() {
        return hitClip;
    }

    void Animate(float magnitude) {
        timer += Time.deltaTime * (up ? magnitude : -magnitude);
        if (timer >= 1) {
            up = false;
        } else if (timer < 0) {
            up = true;
        }
        if (magnitude > float.Epsilon) {
            Vector3 v3 = Vector3.Lerp(new Vector3(-0.1f, 0, 0), new Vector3(0.1f, 0, 0), timer);
            v3.y = bobCurve.Evaluate(timer);
            transform.GetChild(0).localPosition = v3;

            Vector3 r3 = Vector3.Lerp(new Vector3(0, rotY, 0), new Vector3(0, rotY, 0), timer);
            transform.GetChild(0).localEulerAngles = r3;

            r3 = Vector3.Lerp(new Vector3(20, 0, 6f), new Vector3(20, 0, -6f), timer);
            character.renderHead.transform.localEulerAngles = r3;

            //r3 = Vector3.Lerp(character.originLHand, character.originLHand + Vector3.up * 0.1f, timer);
            //character.renderLHand.transform.localPosition = r3;

            //r3 = Vector3.Lerp(character.originRHand + Vector3.up * 0.1f, character.originRHand, timer);
            //character.renderRHand.transform.localPosition = r3;

        } else {
            transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, new Vector3(0, 0, 0), Time.deltaTime);
            transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, rotY, 0), Time.deltaTime * 10);

            character.renderHead.transform.localRotation = Quaternion.Lerp(character.renderHead.transform.localRotation, Quaternion.Euler(20, 0, 0), Time.deltaTime * 10);

            //character.renderLHand.transform.localPosition = Vector3.Lerp(character.renderLHand.transform.localPosition, character.originLHand, Time.deltaTime * 10);
            //character.renderRHand.transform.localPosition = Vector3.Lerp(character.renderRHand.transform.localPosition, character.originRHand, Time.deltaTime * 10);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.GetComponent<Bullet>()) {
            Debug.Log("Hit");
            Destroy(other.gameObject);
        }
    }
}
