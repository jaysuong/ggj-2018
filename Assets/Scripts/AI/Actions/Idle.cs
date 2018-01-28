using System.Collections;
using System.Collections.Generic;
using InitialPrefabs.DANI;
using UnityEngine;
using UnityEngine.AI;

namespace boc {
	public class Idle : Action {
		[SerializeField]
		private float idleTime;
		[SerializeField]
		private string triggerName;

		private float timer;
		private NavMeshAgent agent;
		private Animator animator;

		public override void OnStart () {
			agent = GetComponent<NavMeshAgent> ();
			animator = GetComponent<Animator> ();
		}

		public override void OnActionStart () {
			timer = idleTime;
			agent.isStopped = true;
			animator.SetTrigger (triggerName);
		}

		public override ActionState OnActionUpdate () {
			timer -= Time.deltaTime;

			return timer <= 0f ? ActionState.Success : ActionState.Running;
		}

	}
}