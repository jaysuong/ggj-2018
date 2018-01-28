using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	public class SelfDestructor : MonoBehaviour {
		[SerializeField]
		private float timeToLive = 5f;

		private void Start () {
			Destroy (gameObject, timeToLive);
		}
	}
}