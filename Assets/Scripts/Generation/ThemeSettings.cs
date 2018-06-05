using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThemeSettings", menuName = "Settings/ThemeSettings", order = 1)]
public class ThemeSettings : ScriptableObject {

    public Material roomMaterial;
    public GameObject doorObject;
    public GameObject fogObject;
    [Header("Gameplay")]
    public ObjectCategory[] gameplayObjects;
    public Vector2Int roomAmountGameplay;
    [Header("Decoration")]
    public ObjectCategory[] decorateObjects;
    [Header("Enemy")]
    public Vector2 randomPerRoom;
    public GameObject[] enemies;

}

public enum Place { Ground, Wall }

[System.Serializable]
public class ObjectCategory {
    public GameObject[] objects;
    public Place place;
    public float positionGridPlace = 0f;
    public Vector2 rotationMinMax = new Vector2(0, 360);
    public float rotationIncrements = 1;
    public float rotationMultiplier { get { return Mathf.FloorToInt(360 / rotationIncrements); } }
    public Vector2Int roomAmountDecorate;
}