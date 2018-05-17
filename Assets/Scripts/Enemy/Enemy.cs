using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {

    NavMeshAgent agent;

    public Transform target;

	void Start () {
        agent = GetComponent<NavMeshAgent>();
    }
	
	void Update () {
    }

    [ContextMenu("To Target")]
    public void ToTarget() {
        agent.SetDestination(target.position);
    }
}
