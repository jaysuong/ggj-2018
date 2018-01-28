using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace boc {
	public class AbilityIcon : MonoBehaviour {
		[SerializeField]
		private CooldownAbility ability;
		[SerializeField]
		private Image abilityImage;

		[SerializeField, Header ("Bouncing")]
		private Vector3 bounceScale = new Vector3 (1.1f, 1.1f, 1.1f);
		[SerializeField]
		private float bounceDuration = 1f;

		private void Update () {
			switch (ability.CurrentStage) {
				case AbilityStage.Idle:
					if (!LeanTween.isTweening (gameObject)) {
						LeanTween.scale (gameObject, bounceScale, bounceDuration * 0.5f);
						LeanTween.scale (gameObject,
							Vector3.one,
							bounceDuration * 0.5f).setDelay (bounceDuration * 0.5f);
					}
					break;

				case AbilityStage.Active:
					abilityImage.fillMethod = Image.FillMethod.Vertical;
					abilityImage.fillAmount = ability.AbilityProgress;
					break;

				case AbilityStage.Cooldown:
					abilityImage.fillMethod = Image.FillMethod.Radial360;
					abilityImage.fillAmount = 1f - ability.CooldownProgress;
					break;
			}
		}

	}
}