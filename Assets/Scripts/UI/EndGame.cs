using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace boc {

	public class EndGame : MonoBehaviour {

		public float fadeOutTime = 1f;
		public int level = 0;
		public Text message;
		public GameEvent onGameLose;
		public GameEvent onGameWin;
		private CanvasGroup canvasGroup;

		private void OnEnable () {
			onGameLose.Register (ShowMenu);
			onGameLose.Register (SetLoseMsg);
			onGameWin.Register (ShowMenu);
			onGameWin.Register (SetWinMsg);
		}

		private void OnDisable () {
			onGameLose.Unregister (ShowMenu);
			onGameLose.Unregister (SetLoseMsg);
			onGameWin.Unregister (ShowMenu);
			onGameWin.Unregister (SetWinMsg);
		}

		private void Start () {
			canvasGroup = GetComponent<CanvasGroup> ();
			canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
		}

		public void Restart () {
			SceneManager.LoadSceneAsync (level);
		}

		public void ShowMenu () {
			StartCoroutine (FadeInGroup ());
		}

		public void SetLoseMsg () {
			message.text = "Game Over...";
		}

		public void SetWinMsg () {
			message.text = "Congratulations!";
		}

		private IEnumerator FadeInGroup () {
			var t = 0f;
			while (t < fadeOutTime) {
				var alpha = Time.deltaTime / fadeOutTime;
				canvasGroup.alpha += alpha;
				t += Time.deltaTime;
				yield return null;
			}
			canvasGroup.interactable = canvasGroup.blocksRaycasts = true;
		}
	}
}