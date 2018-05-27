using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/Item", order = 1)]
public class Item : ScriptableObject {

    public Sprite sprite;
    public Vector2 spriteOffset;
}
