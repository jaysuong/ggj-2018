using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace boc {
	public class BossHealthBar : MonoBehaviour {
		[SerializeField] private Image BackgroundBar;
		[SerializeField] private Image ForegroundBar;
		// HP Bar colors: Red, Orange, Yellow, Green, Blue, Cyan, Blue, Purple
		private Color[] BarColors = {
			new Color (0.4f, 0.4f, 0.4f, 1f), // dark gray
			Color.red,
			new Color (1f, 0.4f, 0, 1f), // orange
			Color.yellow,
			Color.green,
			Color.cyan,
			Color.blue,
			new Color (1f, 0, 1f, 1f) // violet
		};
		private int ColorIndex;
		private float OneBarHP;
		[SerializeField] private GameObject BossObject;
		private IHealth BossHealth;
		private IEnumerator LerpHealth () {
			float t = 0;
			while (t < 1) {
				yield return null;
				t += Time.deltaTime;
				ForegroundBar.fillAmount = Mathf.Lerp (ForegroundBar.fillAmount,
					(BossHealth.CurrentHP - (ColorIndex - 1) * OneBarHP) / OneBarHP,
					t);
			}
		}
		void Start () {
			// Initialize HP bars
			ColorIndex = BarColors.Length - 1;
			BackgroundBar.color = BarColors[ColorIndex - 1];
			ForegroundBar.color = BarColors[ColorIndex];
			BackgroundBar.fillAmount = 1;
			ForegroundBar.fillAmount = 1;
			ForegroundBar.transform.SetAsLastSibling ();

			// Get boss object's health data
			BossHealth = BossObject.GetComponent<IHealth> ();

			// For convenience: amount of hp for each color
			OneBarHP = BossHealth.MaxHP / (BarColors.Length - 1);
		}

		void Update () {
			// if (Input.GetKeyUp ("s") && BossHealth.CurrentHP > 0) {
			// 	// Stop the previous lerp. Update the health and start a new lerp.
			// 	BossHealth.Damage (10);

			// 	StopCoroutine ("LerpHealth");
			// 	if (BossHealth.CurrentHP > 0 && BossHealth.CurrentHP < (ColorIndex - 1) * OneBarHP) {
			// 		// Update the foreground and background hp bar colors.
			// 		ColorIndex--;
			// 		ForegroundBar.color = BarColors[ColorIndex];
			// 		BackgroundBar.color = BarColors[ColorIndex - 1];
			// 		// New HP bar starts at full.
			// 		ForegroundBar.fillAmount = 1;
			// 	}
			// 	StartCoroutine ("LerpHealth");
			// }

			if (BossHealth != null) {
				ForegroundBar.fillAmount = BossHealth.CurrentHP / BossHealth.MaxHP;
			}
		}

	}
}