using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Weapon))]
public class EditorWeapon : Editor {

    Weapon weapon;

    Texture2D debugTex;

    void OnEnable() {
        weapon = (Weapon)target;
    }

    void OnDisable() {
        debugTex = null;
    }

    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();

        Gui();//put in change method for looking when the offsets change // could be done differently

        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(weapon);
        }
    }

    void Gui() {
        weapon.sprite = BigSelection(weapon.sprite);

        GUILayout.Label("Fire Info", EditorStyles.boldLabel);

        weapon.fireRate = EditorGUILayout.FloatField("Rate of Fire", weapon.fireRate);
        weapon.shootPattern = (ShootPattern)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), "Shoot Pattern", weapon.shootPattern);
        weapon.recoilFactor = EditorGUILayout.FloatField("Recoild factor", weapon.recoilFactor);

        weapon.infiniteAmmo = EditorGUILayout.Toggle("Has infinite Ammo", weapon.infiniteAmmo);
        weapon.maxAmmo = EditorGUILayout.IntField("Magazine size", weapon.maxAmmo);
        weapon.needFullReload = EditorGUILayout.Toggle("Need full reload", weapon.needFullReload);

        weapon.timeTillFullReload = EditorGUILayout.FloatField("Time will full reload", weapon.timeTillFullReload);

        if (weapon.sprite != null) {
            GUILayout.Label("Sprites", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
                GUILayout.Label("UI Bullets (On - Off)");
                weapon.bulletSpriteUI_On = (Sprite)EditorGUILayout.ObjectField(weapon.bulletSpriteUI_On, typeof(Sprite), false);
                weapon.bulletSpriteUI_Off = (Sprite)EditorGUILayout.ObjectField(weapon.bulletSpriteUI_Off, typeof(Sprite), false);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.Label("Reload Sprite");
                weapon.reloadSprite = (Sprite)EditorGUILayout.ObjectField(weapon.reloadSprite, typeof(Sprite), false);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Bullet Sprite");
            weapon.bulletSprite = (Sprite)EditorGUILayout.ObjectField(weapon.bulletSprite, typeof(Sprite), false);
            GUILayout.EndHorizontal();

            GUILayout.Label("Audio", EditorStyles.boldLabel);
            weapon.weaponShot = (AudioClip)EditorGUILayout.ObjectField("Audio Shot", weapon.weaponShot, typeof(AudioClip), false);
            weapon.weaponReload = (AudioClip)EditorGUILayout.ObjectField("Audio Reload", weapon.weaponReload, typeof(AudioClip), false);

            GUILayout.Label("Gun Offsets", EditorStyles.boldLabel);
            weapon.spriteOffset = EditorGUILayout.Vector2Field("Gun Offset", weapon.spriteOffset);
            weapon.LHandOffset = EditorGUILayout.Vector2Field("Left hand Offset", weapon.LHandOffset);
            weapon.barrelOffset = EditorGUILayout.Vector2Field("Barrel Offset", weapon.barrelOffset);

            Rect rect = EditorGUILayout.GetControlRect();
            rect.position = new Vector2(Screen.width / 2, rect.position.y + 100);
            DisplayGunOffsets(rect.position);
        }
    }

    void DisplayGunOffsets(Vector2 center) {

        //GUILayout.Label(SpriteToTexture(weapon.sprite));
        Texture2D tex = SpriteToTexture(weapon.sprite, 8);

        Rect rect = new Rect();
        rect.position = new Vector2(center.x - (tex.width / 2), center.y - (tex.height / 2));
        rect.size = new Vector2(tex.width, tex.height);

        Vector2 weaponCenter = rect.position + new Vector2(tex.width / 2, tex.height / 2);

        GUIStyle style = new GUIStyle();
        style.normal.background = tex;
        EditorGUI.LabelField(rect, GUIContent.none, style);

        style = new GUIStyle();
        style.normal.background = GetColoredTexture(30, new Color(0.1f, 1f, 0.5f, 0.75f));
        Vector2 rHand = new Vector2(weaponCenter.x - 15, weaponCenter.y - 15) - new Vector2(weapon.spriteOffset.x * 80, -weapon.spriteOffset.y * 80);
        EditorGUI.LabelField(new Rect(rHand, Vector2.one * 30), GUIContent.none, style);

        style = new GUIStyle();
        style.normal.background = GetColoredTexture(30, new Color(1f, 0.1f, 0.5f, 0.75f));
        Vector2 lHand = rHand + new Vector2(weapon.LHandOffset.x * 80, -weapon.LHandOffset.y * 80);
        EditorGUI.LabelField(new Rect(lHand, Vector2.one * 30), GUIContent.none, style);

        style = new GUIStyle();
        style.normal.background = GetColoredTexture(20, new Color(0.1f, 0.1f, 1f, 0.75f));
        Vector2 barrel = rHand + new Vector2(5, 5) + new Vector2(weapon.barrelOffset.x * 80, (-weapon.barrelOffset.y * 80));
        EditorGUI.LabelField(new Rect(barrel, Vector2.one * 20), GUIContent.none, style);
    }

    Texture2D SpriteToTexture(Sprite sprite, int scale) {
        Texture2D croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height);
        croppedTexture.SetPixels(pixels);

        Texture2D result = new Texture2D((int)sprite.rect.width * scale, (int)sprite.rect.height * scale, TextureFormat.RGBAFloat, false);
        for (int i = 0; i < result.height; ++i) {
            for (int j = 0; j < result.width; ++j) {
                Color newColor = croppedTexture.GetPixel(Mathf.FloorToInt(j / (float)scale), Mathf.FloorToInt(i / (float)scale));
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        result.filterMode = FilterMode.Point;
        return result;
    }

    //get a single color Texture2d
    Texture2D GetColoredTexture(int size, Color col) {
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        for (int i = 0; i < size * size; i++) {
            colors[i] = col;
        }
        tex.SetPixels(colors);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }

    //DIsplay a bigger sprite-selection-box
    Sprite BigSelection(Sprite sprite) {
        float w = sprite == null ? 80 : sprite.rect.width * 6;
        float h = sprite == null ? 80 : sprite.rect.height * 6;
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        //style.fixedWidth = size;
        GUILayout.Label(name, style);
        return (Sprite)EditorGUILayout.ObjectField(sprite, typeof(Sprite), false, GUILayout.Width(w), GUILayout.Height(h));
    }
}
#endif

public enum Handed { One, Two }

public enum ShootPattern { Straight, Three, Five, Random, Through}

//hold information need for weapons, rate of fire, ammo count, etc
[CreateAssetMenu(fileName = "Weapon", menuName = "Item/Weapon", order = 1)]
public class Weapon : Item {

    public float fireRate;
    public Handed handed;

    public ShootPattern shootPattern;

    public int maxAmmo;
    public bool infiniteAmmo = false;
    public float timeTillFullReload;
    public bool needFullReload;
    public Sprite reloadSprite;

    public float recoilFactor;

    public Sprite bulletSprite;
    public Sprite bulletSpriteUI_On;
    public Sprite bulletSpriteUI_Off;
    public Vector2 barrelOffset;

    public Vector2 LHandOffset;

    public AudioClip weaponShot, weaponReload;
}
