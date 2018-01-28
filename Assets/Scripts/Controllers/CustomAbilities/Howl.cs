using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	[CreateAssetMenu (menuName = "ScriptableObjects/abilities/howling")]
	public class Howl : CooldownAbility {
		[SerializeField]
		private float amountToHeal = 30;
		[SerializeField]
		private AudioClip howlingSound;
		[SerializeField]
		private KeyCode abilitykey;

		private IHealth health;
		private AudioSource source;

		public override void OnAbilityStart (GameObject player) {
			health = player.GetComponent<IHealth> ();
			source = player.GetComponent<AudioSource> ();
			currentStage = AbilityStage.Idle;
		}
		public override void OnAbilityUpdate (GameObject player) {
			switch (currentStage) {
				case AbilityStage.Idle:
					if (Input.GetKeyDown (abilitykey)) {
						source.clip = howlingSound;
						source.Play ();

						abilityTimer = duration;
						currentStage = AbilityStage.Active;
					}
					break;

				case AbilityStage.Active:
					abilityTimer -= Time.deltaTime;

					if (abilityTimer <= 0f) {
						health.Heal (amountToHeal);
						cooldownTimer = cooldown;

						currentStage = AbilityStage.Cooldown;
					}
					break;

				case AbilityStage.Cooldown:
					cooldownTimer -= Time.deltaTime;

					if (cooldownTimer <= 0f) {
						currentStage = AbilityStage.Idle;
					}

					break;
			}
		}
	}
}