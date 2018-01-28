using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	[CreateAssetMenu (menuName = "ScriptableObjects/abilities/speedboost")]
	public class SpeedBoost : CooldownAbility {
		[SerializeField]
		private float speedmultiplier = 5;
		[SerializeField]
		private KeyCode abilityKey;

		private float oldspeed;
		private PlayerController controller;

		public override void OnAbilityStart (GameObject player) {
			controller = player.GetComponent<PlayerController> ();
		}

		public override void OnAbilityUpdate (GameObject player) {
			switch (currentStage) {
				case AbilityStage.Idle:
					if (Input.GetKeyDown (abilityKey)) {
						oldspeed = controller.Speed;
						controller.Speed = oldspeed * speedmultiplier;
						abilityTimer = duration;
						currentStage = AbilityStage.Active;
					}
					break;

				case AbilityStage.Active:
					abilityTimer -= Time.deltaTime;

					if (abilityTimer <= 0) {
						controller.Speed = oldspeed;
						cooldownTimer = cooldown;
						currentStage = AbilityStage.Cooldown;

					}
					break;

				case AbilityStage.Cooldown:
					cooldownTimer -= Time.deltaTime;

					if (cooldownTimer <= 0) {
						currentStage = AbilityStage.Idle;
					}
					break;
			}
		}

	}
}