using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Handed { One_Handed, Two_Handed}

public class Item : MonoBehaviour {

    public Handed handed;
    public Sprite sprite;
}
