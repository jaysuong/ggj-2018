using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	[CreateAssetMenu (menuName = "ScriptableObjects/abilities/Throw Item")]
	public class ThrowWeapon : CooldownAbility {
		[SerializeField]
		private KeyCode abilityKey;
		[SerializeField]
		private GameObject itemToThrow;
		[SerializeField]
		private GameObject indicatorPrefab;
		[SerializeField]
		private Vector3 launchOffset;

		[SerializeField, Header ("Input")]
		private string horizontalInputName = "Horizontal";
		[SerializeField]
		private string verticalInputName = "Vertical";

		private Transform indicatorTransform;

		public override void OnAbilityStart (GameObject player) {
			if (indicatorTransform != null) {
				Destroy (indicatorTransform);
			}

			indicatorTransform = Instantiate (indicatorPrefab, player.transform).transform;
			indicatorTransform.localPosition = Vector3.zero;
		}

		public override void OnAbilityUpdate (GameObject player) {
			RotateIndicator ();

			switch (currentStage) {
				case AbilityStage.Idle:
					if (Input.GetKey (abilityKey)) {
						Launch (player.transform);

						abilityTimer = duration;
						currentStage = AbilityStage.Active;
					}
					break;

				case AbilityStage.Active:
					abilityTimer -= Time.deltaTime;

					if (abilityTimer <= 0) {

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

		private void Launch (Transform launchPoint) {
			var launchPosition = launchOffset + launchPoint.position;

			Instantiate (itemToThrow, launchPosition, indicatorTransform.rotation);
		}

		private void RotateIndicator () {
			var horizontal = Input.GetAxis (horizontalInputName) * -1;
			var vertical = Input.GetAxis (verticalInputName) * -1;

			var lookDirection = new Vector3 (horizontal, 0f, vertical);
			if (lookDirection.sqrMagnitude > 0) {
				indicatorTransform.rotation = Quaternion.LookRotation (lookDirection);
			}
		}
	}
}