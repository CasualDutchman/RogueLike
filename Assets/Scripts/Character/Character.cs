using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public SpriteRenderer renderHair, renderHead, renderBody, renderLHand, renderRHand;

    public CharacterCustom custom;

    public Vector3 originHair, originHead, originBody, originLHand, originRHand;

	void Start () {
        CharacterInfo info = custom.GetRandomCharacter();
        renderHair.sprite = info.hair;
        renderHead.sprite = info.head;
        renderBody.sprite = info.body;
        renderLHand.sprite = info.hand;
        renderRHand.sprite = info.hand;

        originHair = renderHair.transform.localPosition;
        originHead = renderHead.transform.localPosition;
        originBody = renderBody.transform.localPosition;
        originLHand = renderLHand.transform.localPosition;
        originRHand = renderRHand.transform.localPosition;
    }
}
