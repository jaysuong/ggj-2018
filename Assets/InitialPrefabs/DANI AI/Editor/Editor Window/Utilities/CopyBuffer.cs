using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
	using Object = UnityEngine.Object;

	/// <summary>
	/// A container used to store copied nodes in the editor
	/// </summary>
	public class CopyBuffer {
		public Condition[] CopiedConditions { get { return copiedConditions.ToArray (); } }
		public Connection[] CopiedConnections { get { return copiedConnections.ToArray (); } }
		public EditorNode[] CopiedNodes { get { return copiedNodes.ToArray (); } }

		public int NodeCount { get { return copiedNodes.Count; } }

		private List<EditorNode> copiedNodes;
		private List<Connection> copiedConnections;
		private List<Condition> copiedConditions;

		private Dictionary<int, int> idMap;
		private Vector2 copyOffset;

		public CopyBuffer () {
			copiedNodes = new List<EditorNode> ();
			copiedConnections = new List<Connection> ();
			copiedConditions = new List<Condition> ();
			idMap = new Dictionary<int, int> ();
		}

		/// <summary>
		/// Clears all copied instances
		/// </summary>
		public void Clear () {
			foreach (var node in copiedNodes) {
				Object.DestroyImmediate (node, true);
			}

			foreach (var connection in copiedConnections) {
				Object.DestroyImmediate (connection, true);
			}

			foreach (var condition in copiedConditions) {
				Object.DestroyImmediate (condition, true);
			}

			copiedNodes.Clear ();
			copiedConnections.Clear ();
			copiedConditions.Clear ();
		}

		/// <summary>
		/// Saves a generated instance of the nodes to copy
		/// </summary>
		/// <param name="nodes">The nodes to copy</param>
		public void CopyNodes (EditorNode[] nodes, Vector2 position) {
			copyOffset = position;

			Clear ();
			idMap.Clear ();

			var template = nodes.Length > 0 ? (nodes[0].graph as EditorGraph).Template :
				null;

			// Copy the nodes
			foreach (var node in nodes) {
				var copyNode = Object.Instantiate (node);
				var aiNode = Object.Instantiate (node.Node);
				aiNode.name = node.Node.name;
				aiNode.hideFlags = HideFlags.HideInHierarchy;

				AssignNewNodeId (aiNode);
				copyNode.PrepareNode (aiNode, node.Type);

				copiedNodes.Add (copyNode);
			}

			// copy the connections
			var connections = template.Connections.Where (
				c => idMap.ContainsKey (c.SourceId) && idMap.ContainsKey (c.TargetId));

			foreach (var connection in connections) {
				var copyConnection = Object.Instantiate (connection);
				copyConnection.hideFlags = HideFlags.HideInHierarchy;
				copyConnection.name = connection.name;
				copiedConnections.Add (copyConnection);
			}

			// Copy the conditions, if any
			var conditionals = connections.Where (c => c.ConnectionType == ConnectionType.Conditional);
			var conditions = template.Conditions.Where (
				c => conditionals.Where (con => con.ConditionId == c.Id).Any ());

			foreach (var condition in conditions) {
				var copyCondition = Object.Instantiate (condition);
				copyCondition.hideFlags = HideFlags.HideInHierarchy;
				AssignNewNodeId (copyCondition);
				copiedConditions.Add (copyCondition);
			}

			// Convert the ids of each connections to the new ids
			foreach (var connection in copiedConnections) {
				ConvertConnectionIds (connection);
			}
		}

		/// <summary>
		/// Pastes the copied nodes into a template
		/// </summary>
		public void PasteNodesTo (EditorGraph graph, Vector2 position) {
			Undo.SetCurrentGroupName ("Paste nodes");

			var serializedTemplate = graph.SerializedTemplate;
			var template = graph.Template;
			serializedTemplate.Update ();

			var observerArray = serializedTemplate.FindProperty (AINodeUtility.ObserverPropertyName);
			var decisionArray = serializedTemplate.FindProperty (AINodeUtility.DecisionPropertyName);
			var actionArray = serializedTemplate.FindProperty (AINodeUtility.ActionPropertyName);
			var connectionArray = serializedTemplate.FindProperty (AINodeUtility.ConnectionPropertyName);
			var conditionArray = serializedTemplate.FindProperty (AINodeUtility.ConditionPropertyName);

			foreach (var node in copiedNodes) {
				Undo.RegisterCreatedObjectUndo (node.Node, node.Node.Id.ToString ());
				AssetDatabase.AddObjectToAsset (node.Node, template);

				var serializedNode = new SerializedObject (node.Node);
				serializedNode.FindProperty (AINodeUtility.PositionPropertyName).vector2Value += (position - copyOffset);
				serializedNode.ApplyModifiedPropertiesWithoutUndo ();

				switch (node.Type) {
					case NodeType.Observer:
						observerArray.InsertArrayElementAtIndex (0);
						observerArray.GetArrayElementAtIndex (0).objectReferenceValue = node.Node;
						break;

					case NodeType.Decision:
						decisionArray.InsertArrayElementAtIndex (0);
						decisionArray.GetArrayElementAtIndex (0).objectReferenceValue = node.Node;
						break;

					case NodeType.Action:
						actionArray.InsertArrayElementAtIndex (0);
						actionArray.GetArrayElementAtIndex (0).objectReferenceValue = node.Node;
						break;
				}
			}

			foreach (var connection in copiedConnections) {
				Undo.RegisterCreatedObjectUndo (connection, connection.GetInstanceID ().ToString ());
				AssetDatabase.AddObjectToAsset (connection, template);

				connectionArray.InsertArrayElementAtIndex (0);
				connectionArray.GetArrayElementAtIndex (0).objectReferenceValue = connection;
			}

			foreach (var condition in copiedConditions) {
				Undo.RegisterCreatedObjectUndo (condition, condition.Id.ToString ());
				AssetDatabase.AddObjectToAsset (condition, template);

				conditionArray.InsertArrayElementAtIndex (0);
				conditionArray.GetArrayElementAtIndex (0).objectReferenceValue = condition;
			}

			AssetDatabase.SaveAssets ();
			serializedTemplate.ApplyModifiedProperties ();
			Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());

			graph.Refresh ();
		}

		/// <summary>
		/// Assigns a new id to the node to mark it as unique. This operation cannot
		/// be undone.
		/// </summary>
		/// <param name="node">The node to modify</param>
		private void AssignNewNodeId (AINode node) {
			var oldId = node.Id;

			var serializedNode = new SerializedObject (node);
			serializedNode.FindProperty (AINodeUtility.IdPropertyName).intValue = node.GetInstanceID ();
			serializedNode.ApplyModifiedPropertiesWithoutUndo ();

			idMap.Add (oldId, node.Id);
		}

		private void AssignNewNodeId (Condition condition) {
			var oldId = condition.Id;

			var serializedNode = new SerializedObject (condition);
			serializedNode.FindProperty (AINodeUtility.IdPropertyName).intValue = condition.GetInstanceID ();
			serializedNode.ApplyModifiedPropertiesWithoutUndo ();

			idMap.Add (oldId, condition.Id);
		}

		/// <summary>
		/// Converts on the connention to the new unique ids
		/// </summary>
		/// <param name="connection">The connection to convert</param>
		private void ConvertConnectionIds (Connection connection) {
			var serializedConnection = new SerializedObject (connection);

			serializedConnection.FindProperty (AINodeUtility.SourceIdPropertyName).intValue = idMap[connection.SourceId];
			serializedConnection.FindProperty (AINodeUtility.TargetIdPropertyName).intValue = idMap[connection.TargetId];

			if (connection.ConnectionType == ConnectionType.Conditional) {
				serializedConnection.FindProperty (AINodeUtility.ConditionIdPropertyName).intValue = idMap[connection.ConditionId];
			}

			serializedConnection.ApplyModifiedPropertiesWithoutUndo ();
		}

	}
}