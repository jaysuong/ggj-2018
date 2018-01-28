using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
	internal class ConnectionUI : IEdgeGUI {

		public Edge ActiveEdge { get { return activeEdge; } set { activeEdge = value; } }

		public GraphGUI host {
			get { return graphCanvas; }
			set { graphCanvas = value as EditorGraphCanvas; }
		}

		public List<int> edgeSelection { get; set; }

		private readonly Color ActiveEdgeColor = new Color (1f, 1f, 1f, 0.9f);
		private readonly Color NormalEdgeColor = new Color (0.9f, 0.9f, 0.9f, 0.6f);

		private const float MaxMouseDeviationDistance = 10f;
		private const float PositionCheckInterval = 0.05f;

		private EditorGraphCanvas graphCanvas;
		private Edge activeEdge;

		public ConnectionUI () {
			edgeSelection = new List<int> ();
		}

		public void DoEdges () {
			var current = Event.current;

			if (current.type == EventType.Repaint) {
				var nodes = graphCanvas.EditorGraph.nodes;

				for (var i = 0; i < nodes.Count; ++i) {
					(nodes[i] as EditorNode).DrawStubs (graphCanvas, graphCanvas.GetInstanceID ());
				}

				var oldColor = GUI.color;
				GUI.color = new Color (0.9f, 0.9f, 0.9f, 0.6f);

				var inputStyle = graphCanvas.Skin.FindStyle ("Node Endpoint Input");
				var outputStyle = graphCanvas.Skin.FindStyle ("Node Endpoint Output");

				for (var i = 0; i < nodes.Count; ++i) {
					var node = nodes[i] as EditorNode;

					node.DrawConnectionEndPoints (inputStyle, outputStyle);
				}

				GUI.color = oldColor;
			}

			var edges = graphCanvas.EditorGraph.edges;

			// Draw all the edges on the graph
			for (var i = 0; i < edges.Count; ++i) {
				var inputPosition = (edges[i].fromSlot.node as EditorNode).GetSlotPosition (edges[i].toSlot);
				var outputPosition = (edges[i].toSlot.node as EditorNode).GetSlotPosition (edges[i].fromSlot);

				DrawBezier (inputPosition, outputPosition, NormalEdgeColor);
			}

			if (activeEdge != null) {
				var input = (activeEdge.fromSlot.node as EditorNode).GetSlotPosition (activeEdge.toSlot);
				var output = (activeEdge.toSlot.node as EditorNode).GetSlotPosition (activeEdge.fromSlot);

				DrawBezier (input, output, ActiveEdgeColor);
			}

			if (current.type == EventType.MouseDown) {
				// Look for edges close to the mouse click.  If it exists, then highlight the node
				activeEdge = FindClosestEdge ();

				if (activeEdge != null) {
					var sourceId = AINodeUtility.ConvertToIntId (activeEdge.fromSlotName);
					var targetId = AINodeUtility.ConvertToIntId (activeEdge.toSlotName);
					var connection = graphCanvas.EditorGraph.Template.Connections.Where (
						c => c.SourceId == sourceId && c.TargetId == targetId).
					FirstOrDefault ();

					EditorBridge.SelectDaniObject (connection);
				}
			}
		}

		public void DoDraggedEdge () { }

		public void BeginSlotDragging (Slot slot, bool allowStartDrag, bool allowEndDrag) { }

		public void SlotDragging (Slot slot, bool allowEndDrag, bool allowMultiple) { }

		public void EndSlotDragging (Slot slot, bool allowMultiple) { }

		public void EndDragging () { }

		public Edge FindClosestEdge () {
			var mousePosition = Event.current.mousePosition;
			var edges = host.graph.edges;
			var minDistance = MaxMouseDeviationDistance;
			Edge candidateEdge = null;

			foreach (var edge in edges) {
				var inputPosition = (edge.fromSlot.node as EditorNode).GetSlotPosition (edge.toSlot);
				var outputPosition = (edge.toSlot.node as EditorNode).GetSlotPosition (edge.fromSlot);

				var startTan = inputPosition + Vector2.right * 50f;
				var endTan = outputPosition + Vector2.left * 50f;

				for (var t = 0f; t <= 1f; t += PositionCheckInterval) {
					var targetPos = GetBezierPoint (inputPosition, startTan, endTan, outputPosition, t);
					var distance = (targetPos - mousePosition).magnitude;

					if (distance <= minDistance) {
						minDistance = distance;
						candidateEdge = edge;
						break;
					}
				}
			}

			return candidateEdge;
		}

		/// <summary>
		/// Draws a bezier curve from start to finish
		/// </summary>
		/// <param name="startPos">The start position</param>
		/// <param name="endPos">The endpoint</param>
		private void DrawBezier (Vector3 startPos, Vector3 endPos, Color color) {
			var startTan = startPos + Vector3.right * 50f;
			var endTan = endPos + Vector3.left * 50f;
			var shadowCol = new Color (color.r, color.g, color.b, 0.06f);
			for (var i = 0; i < 3; i++) // Draw a shadow
				Handles.DrawBezier (startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5f);
			Handles.DrawBezier (startPos, endPos, startTan, endTan, color, null, 1f);
		}

		private Vector2 GetLeftConnectionPoint (Rect position) {
			return new Vector3 (position.x, position.y + position.height * 0.5f, 0f);
		}

		private Vector2 GetRightConnectionPoint (Rect position) {
			return new Vector3 (position.x + position.width,
				position.y + position.height * 0.5f,
				0f);
		}

		private Vector2 GetBezierPoint (Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float t) {
			var inverse = 1f - Mathf.Clamp01 (t);

			return inverse * inverse * inverse * p1 +
				3f * inverse * inverse * t * p2 +
				3f * inverse * t * t * p3 +
				t * t * t * p4;
		}
	}
}