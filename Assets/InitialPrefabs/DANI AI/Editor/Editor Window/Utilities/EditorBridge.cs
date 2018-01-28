using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
	using Object = UnityEngine.Object;

	internal static class EditorBridge {
		internal static event Action OnTemplateUpdate;
		internal static event Action<Object> OnObjectSelect;

		internal static void InvokeTemplateUpdateEvent () {
			if (OnTemplateUpdate != null) {
				OnTemplateUpdate ();
			}
		}

		internal static void SelectDaniObject (Object obj) {
			if (OnObjectSelect != null) {
				OnObjectSelect (obj);
			}
		}
	}
}