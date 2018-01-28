using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace boc {
	[CreateAssetMenu (menuName = "ScriptableObjects/GameEvent")]
	public class GameEvent : ScriptableObject {

		[SerializeField, TextArea] private string comment;
		private event Action OnGameEventInvoke;

		public void Invoke () {
			if (OnGameEventInvoke != null)
				OnGameEventInvoke ();
		}

		public void Register (Action handler) {
			OnGameEventInvoke -= handler;
			OnGameEventInvoke += handler;
			// if (OnGameEventInvoke != null) {
			// 	// Make sure a handler is only run once.
			// 	if (!OnGameEventInvoke.GetInvocationList ().Contains (handler))
			// 		OnGameEventInvoke += handler;
			// }
		}

		public void Unregister (Action handler) {
			OnGameEventInvoke -= handler;
		}
	}
}