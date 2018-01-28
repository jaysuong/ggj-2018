using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	public class PlayerController : MonoBehaviour {
		[SerializeField]
		private float turnSpeed = 6;
		[SerializeField]
		private Transform pivot;
		[SerializeField]
		private float speed = 3;

		[SerializeField]
		private bool isActive;

		[SerializeField, Header ("Controls")]
		private string horizontalName = "Horizontal";
		[SerializeField]
		private string verticalName = "Vertical";

		public bool IsActive { get { return isActive; } set { isActive = value; } }
		public float Speed { get { return speed; } set { speed = value; } }

		private CharacterController controller;

		private void Awake () {
			controller = GetComponent<CharacterController> ();
		}

		private void Update () {
			if (!isActive) { return; }

			float horizontal = Input.GetAxis (horizontalName);
			float vertical = Input.GetAxis (verticalName);

			float gravity = 0;
			if (!controller.isGrounded) {
				gravity = -9.81f;
			}

			Vector3 direction = new Vector3 (horizontal, gravity, vertical);
			controller.Move (direction * Time.deltaTime * speed);

			RotateCharacter (horizontal, vertical);
		}

		private void RotateCharacter (float horizontal, float vertical) {
			Vector3 lookDirection = new Vector3 (horizontal, 0, vertical);
			if (lookDirection.magnitude > 0) {
				pivot.rotation = Quaternion.Slerp (pivot.rotation, Quaternion.LookRotation (lookDirection), Time.deltaTime * turnSpeed);
			}
		}

	}
}