using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//mother of items
//currently only for weapons
//could also be used for power ups, consumables
[CreateAssetMenu(fileName = "Item", menuName = "Item/Item", order = 1)]
public class Item : ScriptableObject {

    public Sprite sprite;
    public Vector2 spriteOffset;
}
