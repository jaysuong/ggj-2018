using System.Collections;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
	/// <summary>
	/// A utility class that handles what items are being dragged
	/// </summary>
	public class DragHandler {

		public bool IsDragging { get; private set; }

		public Vector2 StartPosition { get; private set; }

		public Slot StartSlot { get { return slot; } }

		private Slot slot;

		public void EndDrag () {
			slot = null;
			IsDragging = false;
		}

		public void StartDrag (EditorNode node, Vector3 mousePosition) {
			slot = new Slot (SlotType.InputSlot, node.Node.Id.ToString());
			slot.node = node;
			StartPosition = mousePosition;
			IsDragging = true;
		}
	}
}