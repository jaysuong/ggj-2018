using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	public class SwordDamager : MonoBehaviour {
		[SerializeField]
		private float explosionRadius;
		[SerializeField]
		private LayerMask explosionMask;
		[SerializeField]
		private float damage;

		private new Rigidbody rigidbody;

		private void Awake () {
			rigidbody = GetComponent<Rigidbody> ();
		}

		private void OnTriggerEnter (Collider other) {
			rigidbody.isKinematic = true;
			rigidbody.velocity = Vector3.zero;

			var colliders = Physics.OverlapSphere (transform.position, explosionRadius, explosionMask);

			for (var i = 0; i < colliders.Length; ++i) {
				var hp = colliders[i].GetComponent<IHealth> ();
				if (hp != null) {
					hp.Damage (damage);
				}
			}
		}
	}
}