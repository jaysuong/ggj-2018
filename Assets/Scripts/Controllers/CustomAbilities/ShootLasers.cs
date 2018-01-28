using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	[CreateAssetMenu (menuName = "ScriptableObjects/abilities/Shoot Lasers")]
	public class ShootLasers : CooldownAbility {
		[SerializeField]
		private KeyCode abilityKey;

		private LaserEyes laserEyes;

		public override void OnAbilityStart (GameObject player) {
			laserEyes = player.GetComponent<LaserEyes> ();
		}

		public override void OnAbilityUpdate (GameObject player) {
			switch (currentStage) {
				case AbilityStage.Idle:
					if (Input.GetKeyDown (abilityKey)) {
						laserEyes.ActivateLasers ();

						abilityTimer = duration;
						currentStage = AbilityStage.Active;
					}
					break;

				case AbilityStage.Active:
					abilityTimer -= Time.deltaTime;

					if (abilityTimer <= 0) {
						laserEyes.DeactivateLasers ();

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