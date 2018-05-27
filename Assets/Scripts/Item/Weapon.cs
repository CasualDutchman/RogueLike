using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Handed { One, Two }

[CreateAssetMenu(fileName = "Weapon", menuName = "Item/Weapon", order = 1)]
public class Weapon : Item {

    public float fireRate;
    public float damage;
    public float bulletSpeed;
    public Handed handed;

    public int maxAmmo;
    public float timeTillFullReload;
    public bool needFullReload;

    public Sprite bulletSprite;
    public Sprite bulletSpriteUI;
    public Vector2 barrelOffset;

    public Vector2 RHandOffset, LHandOffset;
}
