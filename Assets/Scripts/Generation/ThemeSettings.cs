using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThemeSettings", menuName = "Settings/ThemeSettings", order = 1)]
public class ThemeSettings : ScriptableObject {

    public Material roomMaterial;
    public GameObject doorObject;
    [Header("Gameplay")]
    public GameObject[] gameplayObjects;
    public Vector2Int roomAmountGameplay;
    [Header("Decoration")]
    public GameObject[] decorateObjects;
    public Vector2Int roomAmountDecorate, corridorAmountDecorate;

}
