using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterCustom", menuName = "Settings/CharacterSettings", order = 1)]
public class CharacterCustom : ScriptableObject {

    public List<FrontBack> hairs = new List<FrontBack>();
    public List<FrontBack> heads = new List<FrontBack>();
    public List<FrontBack> bodies = new List<FrontBack>();
    public List<Sprite> hands = new List<Sprite>();

    public CharacterInfo GetRandomCharacter() {
        CharacterInfo info = new CharacterInfo();

        info.hair = hairs[Random.Range(0, hairs.Count)];
        info.head = heads[Random.Range(0, heads.Count)];
        info.body = bodies[Random.Range(0, bodies.Count)];
        info.hand = hands[Random.Range(0, hands.Count)];

        return info;
    }
}

public class CharacterInfo {
    public FrontBack hair, head, body;
    public Sprite hand;
}

[System.Serializable]
public class FrontBack {
    public Sprite front, back;
}
