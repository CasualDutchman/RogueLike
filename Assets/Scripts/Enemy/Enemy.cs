using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {

    NavMeshAgent agent;
    Character character;

    public Transform target;

    public AnimationCurve bobCurve;
    public float rate;
    float timer;
    bool up = true;

    float rotY;

    void Start () {
        agent = GetComponent<NavMeshAgent>();
        character = GetComponent<Character>();
    }
	
	void Update () {
        agent.SetDestination(target.position);

        rotY = -transform.eulerAngles.y;

        Vector3 v3 = new Vector3(agent.velocity.x, 0, agent.velocity.z);
        Animate(v3.magnitude);

        character.UpdateCharacter(agent.velocity.x, agent.velocity.z);
    }

    void Animate(float magnitude) {
        timer += Time.deltaTime * (up ? magnitude : -magnitude);
        if (timer >= 1) {
            up = false;
        } else if (timer < 0) {
            up = true;
        }
        if (magnitude > float.Epsilon) {
            Vector3 v3 = Vector3.Lerp(new Vector3(-0.1f, 0, 0), new Vector3(0.1f, 0, 0), timer);
            v3.y = bobCurve.Evaluate(timer);
            transform.GetChild(0).localPosition = v3;

            Vector3 r3 = Vector3.Lerp(new Vector3(0, rotY, 6f), new Vector3(0, rotY, -6f), timer);
            transform.GetChild(0).localEulerAngles = r3;

            r3 = Vector3.Lerp(new Vector3(20, 0, 6f), new Vector3(20, 0, -6f), timer);
            character.renderHead.transform.localEulerAngles = r3;

            r3 = Vector3.Lerp(character.originLHand, character.originLHand + Vector3.up * 0.1f, timer);
            character.renderLHand.transform.localPosition = r3;

            r3 = Vector3.Lerp(character.originRHand + Vector3.up * 0.1f, character.originRHand, timer);
            character.renderRHand.transform.localPosition = r3;

        } else {
            transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, new Vector3(0, 0, 0), Time.deltaTime);
            transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, rotY, 0), Time.deltaTime * 10);

            character.renderHead.transform.localRotation = Quaternion.Lerp(character.renderHead.transform.localRotation, Quaternion.Euler(20, 0, 0), Time.deltaTime * 10);

            character.renderLHand.transform.localPosition = Vector3.Lerp(character.renderLHand.transform.localPosition, character.originLHand, Time.deltaTime * 10);
            character.renderRHand.transform.localPosition = Vector3.Lerp(character.renderRHand.transform.localPosition, character.originRHand, Time.deltaTime * 10);
        }
    }
}
