using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorController : MonoBehaviour {

	public string speed = "Speed";

	private Animator animator;
	private NavMeshAgent agent;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
	}

	void Update () {
		animator.SetFloat (speed, agent.velocity.magnitude);
	}
}