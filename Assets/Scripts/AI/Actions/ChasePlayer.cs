using System.Collections;
using System.Collections.Generic;
using InitialPrefabs.DANI;
using UnityEngine;
using UnityEngine.AI;

namespace boc {
	public class ChasePlayer : Action {
		[SerializeField]
		private float arrivalDistance = 10f;

		private Transform playerTransform;
		private NavMeshAgent agent;

		public override void OnStart () {
			agent = GetComponent<NavMeshAgent> ();
		}

		public override void OnActionStart () {
			playerTransform = PlayerTag.playerTransform;

			if (playerTransform != null) {
				agent.SetDestination (playerTransform.position);
				agent.isStopped = false;
			}
		}

		public override ActionState OnActionUpdate () {
			if (playerTransform == null) {
				return ActionState.Fail;
			}

			return !agent.pathPending && agent.remainingDistance <= arrivalDistance ?
				ActionState.Success : ActionState.Running;
		}

	}
}