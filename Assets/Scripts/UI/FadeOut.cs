using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace boc.UI {

	public class FadeOut : MonoBehaviour {

		[SerializeField]
		private float fadeOutTime = 1f;
		[SerializeField]
		private CanvasGroup canvasGroup;

		private void Start () {
			canvasGroup.blocksRaycasts = canvasGroup.interactable = true;
		}

		public void Fade () {
			StartCoroutine (FadeOutGroup ());
		}

		private IEnumerator FadeOutGroup () {
			var t = 0f;
			while (t < fadeOutTime) {
				var alpha = Time.deltaTime / fadeOutTime;
				canvasGroup.alpha -= alpha;
				t += Time.deltaTime;
				yield return null;
			}
			canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
		}
	}
}