using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public SpriteRenderer renderHair, renderHead, renderBody, renderLHand, renderRHand;

    public CharacterCustom custom;

	void Start () {
        CharacterInfo info = custom.GetRandomCharacter();
        renderHair.sprite = info.hair;
        renderHead.sprite = info.head;
        renderBody.sprite = info.body;
        renderLHand.sprite = info.hand;
        renderRHand.sprite = info.hand;
    }
}
