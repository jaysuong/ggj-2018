using System.Collections;
using System.Collections.Generic;
using InitialPrefabs.DANI;
using UnityEngine;
using UnityEngine.AI;

namespace boc {
	public class Idle : Action {
		[SerializeField]
		private float idleTime;

		private float timer;
		private NavMeshAgent agent;

		public override void OnStart () {
			agent = GetComponent<NavMeshAgent> ();
		}

		public override void OnActionStart () {
			timer = idleTime;
			agent.isStopped = true;
		}

		public override ActionState OnActionUpdate () {
			timer -= Time.deltaTime;

			return timer <= 0f ? ActionState.Success : ActionState.Running;
		}

	}
}