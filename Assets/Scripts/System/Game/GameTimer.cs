using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	/// <summary>
	/// A timer that counts down until the game is over
	/// </summary>
	public class GameTimer : MonoBehaviour {
		[SerializeField]
		private float timeToLose;

		[SerializeField, Header ("Events")]
		private GameEvent startGameEvent;
		[SerializeField]
		private GameEvent endGameEvent;

		public float RemainingTime { get { return timer; } }

		private float timer;

		private void OnEnable () {
			if (startGameEvent != null) {
				startGameEvent.Register (HandleStartGameEvent);
			}
		}

		private void OnDisable () {
			if (startGameEvent != null) {
				startGameEvent.Unregister (HandleStartGameEvent);
			}
		}

		private void HandleStartGameEvent () {
			timer = timeToLose;
			StartCoroutine (PlayCountdownSequence ());
		}

		private IEnumerator PlayCountdownSequence () {
			while (timer > 0f) {
				yield return null;

				timer -= Time.deltaTime;
			}

			timer = 0f;
			if (endGameEvent != null) {
				endGameEvent.Invoke ();
			}

			StopAllCoroutines ();
		}

	}
}