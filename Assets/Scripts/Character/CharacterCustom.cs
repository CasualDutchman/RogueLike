using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterCustom", menuName = "Settings/CharacterSettings", order = 1)]
public class CharacterCustom : ScriptableObject {

    public Sprite[] hairs, heads, bodies, hands;

    public CharacterInfo GetRandomCharacter() {
        CharacterInfo info = new CharacterInfo();

        info.hair = hairs[Random.Range(0, hairs.Length)];
        info.head = heads[Random.Range(0, heads.Length)];
        info.body = bodies[Random.Range(0, bodies.Length)];
        info.hand = hands[Random.Range(0, hands.Length)];

        return info;
    }
}

public class CharacterInfo {
    public Sprite hair, head, body, hand;
}
