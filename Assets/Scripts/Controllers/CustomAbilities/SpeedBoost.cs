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
		[SerializeField]
		private string isRunningFieldName = "Is Running";

		private float oldspeed;
		private PlayerController controller;
		private Animator animator;
		private int isRunningId;

		public override void OnAbilityStart (GameObject player) {
			controller = player.GetComponent<PlayerController> ();
			animator = player.GetComponent<Animator> ();
			isRunningId = Animator.StringToHash (isRunningFieldName);
		}

		public override void OnAbilityUpdate (GameObject player) {
			switch (currentStage) {
				case AbilityStage.Idle:
					if (Input.GetKeyDown (abilityKey)) {
						oldspeed = controller.Speed;
						controller.Speed = oldspeed * speedmultiplier;
						abilityTimer = duration;
						currentStage = AbilityStage.Active;
						animator.SetBool (isRunningId, false);
					}
					break;

				case AbilityStage.Active:
					abilityTimer -= Time.deltaTime;
					animator.SetBool (isRunningId, true);
					if (abilityTimer <= 0) {
						controller.Speed = oldspeed;
						cooldownTimer = cooldown;
						currentStage = AbilityStage.Cooldown;
					}
					break;

				case AbilityStage.Cooldown:
					cooldownTimer -= Time.deltaTime;
					animator.SetBool (isRunningId, false);

					if (cooldownTimer <= 0) {
						currentStage = AbilityStage.Idle;
					}
					break;
			}
		}

	}
}