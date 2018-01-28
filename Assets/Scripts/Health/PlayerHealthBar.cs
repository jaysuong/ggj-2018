using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace boc {
	public class PlayerHealthBar : MonoBehaviour {
		[SerializeField] private Image BackgroundBar;
		[SerializeField] private Image ForegroundBar;
		private Color[] BarColors = {
			new Color (0.4f, 0.4f, 0.4f, 1f),
			Color.red
		};
		[SerializeField] private GameObject PlayerObject;
		private IHealth PlayerHealth;
		private IEnumerator LerpHealth () {
			float t = 0;
			while (t < 1) {
				yield return null;
				t += Time.deltaTime;
				ForegroundBar.fillAmount = Mathf.Lerp (ForegroundBar.fillAmount,
					PlayerHealth.CurrentHP / PlayerHealth.MaxHP,
					t);
			}
		}
		void Start () {
			// Initialize HP bars
			BackgroundBar.color = BarColors[0];
			ForegroundBar.color = BarColors[1];
			BackgroundBar.fillAmount = 1;
			ForegroundBar.fillAmount = 1;
			ForegroundBar.transform.SetAsLastSibling ();

			// Get player object's health data
			PlayerHealth = PlayerObject.GetComponent<IHealth> ();
		}

		void Update () {
			if (Input.GetKeyUp ("a") && PlayerHealth.CurrentHP > 0) {
				// Stop the previous lerp. Update the health and start a new lerp.
				StopCoroutine ("LerpHealth");
				PlayerHealth.Damage (5);
				StartCoroutine ("LerpHealth");
			}
		}
	}
}