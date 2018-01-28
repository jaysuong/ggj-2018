using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace boc {
	[CustomEditor (typeof (GameEvent))]
	public class GameEventInspector : Editor {
		private GameEvent ge;

		private void OnEnable () { ge = target as GameEvent; }

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();
			if (GUILayout.Button ("Invoke this event")) {
				ge.Invoke ();
			}
		}
	}
}