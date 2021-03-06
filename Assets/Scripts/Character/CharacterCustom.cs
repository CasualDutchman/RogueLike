﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//contains all the possible sprites a character can use
[CreateAssetMenu(fileName = "CharacterCustom", menuName = "Settings/CharacterSettings", order = 1)]
public class CharacterCustom : ScriptableObject {

    public List<FrontBack> hairs = new List<FrontBack>();
    public List<Sprite> heads = new List<Sprite>();
    public List<Sprite> faces = new List<Sprite>();
    public List<FrontBack> bodies = new List<FrontBack>();
    public List<Sprite> hands = new List<Sprite>();

    public CharacterInfo GetRandomCharacter() {
        CharacterInfo info = new CharacterInfo();

        info.hair = hairs[Random.Range(0, hairs.Count)];
        info.head = heads[Random.Range(0, heads.Count)];
        info.face = faces[Random.Range(0, faces.Count)];
        info.body = bodies[Random.Range(0, bodies.Count)];
        info.hand = hands[Random.Range(0, hands.Count)];

        return info;
    }

    public CharacterInfo GetCharacter(int h, int he, int f, int b, int ha) {
        CharacterInfo info = new CharacterInfo();

        info.hair = hairs[h];
        info.head = heads[he];
        info.face = faces[f];
        info.body = bodies[b];
        info.hand = hands[ha];

        return info;
    }
}

//info for 1 character, 1 sprite per part
public class CharacterInfo {
    public FrontBack hair, body;
    public Sprite hand, head, face;
}

//used if a sprite changes when looking front or back
[System.Serializable]
public class FrontBack {
    public Sprite front, back;
}
