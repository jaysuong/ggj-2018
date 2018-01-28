using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	public class PlayerTag : MonoBehaviour {
		public static Transform playerTransform;

		private void OnEnable () {
			playerTransform = transform;
		}

		private void OnDisable () {
			if (playerTransform == transform) {
				playerTransform = null;
			}
		}
	}
}