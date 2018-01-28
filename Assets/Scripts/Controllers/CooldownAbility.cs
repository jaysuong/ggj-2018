using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	public class CooldownAbility : Ability {
		[SerializeField]
		protected float duration;
		[SerializeField]
		protected float cooldown;

		public AbilityStage CurrentStage { get { return currentStage; } }
		public float AbilityProgress { get { return abilityTimer / duration; } }
		public float CooldownProgress { get { return cooldownTimer / duration; } }

		protected float abilityTimer;
		protected float cooldownTimer;

		protected AbilityStage currentStage;

		private void OnEnable () {
			currentStage = AbilityStage.Idle;
		}

	}
}