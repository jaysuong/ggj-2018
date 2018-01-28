using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	public class LaserEyes : MonoBehaviour {
		[SerializeField]
		private Laser[] lasers;
		[SerializeField]
		private float laserDistance = 20f;
		[SerializeField]
		private LayerMask laserMask;
		[SerializeField]
		private float damage = 1f;

		private bool isFiring;

		[System.Serializable]
		public struct Laser {
			public Transform eye;
			public LineRenderer renderer;
		}

		private void Update () {
			if (isFiring) {
				for (var i = 0; i < lasers.Length; ++i) {
					var laser = lasers[i];
					RaycastHit hit;

					laser.renderer.SetPosition (0, laser.eye.position);

					if (Physics.Raycast (laser.eye.position, laser.eye.forward, out hit, laserDistance, laserMask)) {
						var health = hit.collider.GetComponent<IHealth> ();

						if (health != null) {
							health.Damage (Time.deltaTime * damage);
						}

						laser.renderer.SetPosition (1, hit.point);
					} else {
						laser.renderer.SetPosition (1, (laser.eye.position + laser.eye.forward * 100f));
					}
				}
			}
		}

		public void ActivateLasers () {
			isFiring = true;
		}

		public void DeactivateLasers () {
			isFiring = false;
		}
	}
}