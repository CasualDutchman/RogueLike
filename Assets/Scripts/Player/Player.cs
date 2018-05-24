using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    Animator anim;

    public GameObject testSword;

    bool hasItem = false;
    Handed handed = Handed.One_Handed;
    bool att = false;

    public Transform t;

	void Start () {
        anim = GetComponent<Animator>();

        SetAnim();
    }
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.I)) {
            hasItem = false;
            att = false;
        }
        else if (Input.GetKeyDown(KeyCode.O)) {
            hasItem = true;
            handed = Handed.One_Handed;
            att = false;
        }
        else if (Input.GetKeyDown(KeyCode.P)) {
            hasItem = true;
            handed = Handed.Two_Handed;
            att = false;
        }
        else if (Input.GetKeyDown(KeyCode.L)) {
            att = true;
        }

        if (att) {
            t.Translate(Vector3.up * Time.deltaTime);
        }

        SetAnim();
    }

    void SetAnim() {
        testSword.SetActive(hasItem);
        anim.SetBool("Item", hasItem);
        anim.SetBool("OneHand", hasItem && handed == Handed.One_Handed);
        anim.SetBool("TwoHand", hasItem && handed == Handed.Two_Handed);
        anim.SetBool("Attacking", att);
    }
}
