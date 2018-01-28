using System.Collections;
using System.Collections.Generic;
using InitialPrefabs.DANI;
using UnityEngine;
using UnityEngine.AI;

namespace boc {
	public class Smash : Action {
		[SerializeField]
		private string triggerName;
		[SerializeField]
		private float attackDuration;

		private float timer;

		private Animator animator;
		private NavMeshAgent agent;
		private MeleeWeapon weapon;

		public override void OnStart () {
			agent = GetComponent<NavMeshAgent> ();
			animator = GetComponent<Animator> ();
			weapon = GetComponentInChildren<MeleeWeapon> ();
		}

		public override void OnActionStart () {
			agent.isStopped = true;
			timer = attackDuration;

			animator.SetTrigger (triggerName);

			weapon.IsActive = true;
		}

		public override ActionState OnActionUpdate () {
			timer -= Time.deltaTime;

			return timer <= 0f ? ActionState.Success : ActionState.Running;
		}

		public override void OnActionEnd (ActionState state) {
			weapon.IsActive = false;
		}
	}
}