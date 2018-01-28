using System.Collections;
using System.Collections.Generic;
using InitialPrefabs.DANI;
using UnityEngine;
using UnityEngine.AI;

namespace boc {
	public class SummonMinion : Action {
		[SerializeField]
		private float idleTime;
		[SerializeField]
		private GameObject minion;
		[SerializeField]
		private float spawnDistance;

		private float timer;
		private NavMeshAgent agent;

		public override void OnStart () {
			agent = GetComponent<NavMeshAgent> ();
		}

		public override void OnActionStart () {
			Debug.Log ("Sadsdfnsodfnaods;f");
			timer = idleTime;
			agent.isStopped = true;

			var spawnPos = GetRandomSpawn ();
			NavMeshHit hit;

			if (NavMesh.SamplePosition (spawnPos, out hit, 10f, NavMesh.AllAreas)) {
				spawnPos = hit.position;

				Instantiate (minion, spawnPos, Quaternion.identity);
			}
		}

		public override ActionState OnActionUpdate () {
			timer -= Time.deltaTime;

			return timer <= 0f ? ActionState.Success : ActionState.Running;
		}

		private Vector3 GetRandomSpawn () {
			var current = Transform.position;
			var angle = Random.value * 360f;

			current.x += Mathf.Cos (angle) * spawnDistance;
			current.z += Mathf.Sin (angle) * spawnDistance;

			return current;
		}
	}
}