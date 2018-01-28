using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// A graph that is used to draw the nodes
    /// </summary>
    public class EditorGraph : Graph {

        /// <summary>
        /// The copy buffer to copy nodes
        /// </summary>
        public CopyBuffer CopyBuffer { get { return copyBuffer; } }

        /// <summary>
        /// A serialized representation of the template
        /// </summary>
        public SerializedObject SerializedTemplate { get; private set; }

        internal ScriptDatabase ScriptDatabase { get; set; }

        /// <summary>
        /// The current template data involved in this graph
        /// </summary>
        public AITemplate Template {
            get { return template; }
            set {
                template = value;
                SerializedTemplate = new SerializedObject (template);
                Refresh ();
            }
        }

        public event RequireRepaintHandler OnRequireRepaint;

        public delegate void RequireRepaintHandler ();

        private AITemplate template;
        private Dictionary<int, EditorNode> editorNodes;

        private CopyBuffer copyBuffer;

        private void Awake () {
            editorNodes = new Dictionary<int, EditorNode> ();
            copyBuffer = new CopyBuffer ();

            EditorBridge.OnTemplateUpdate += Refresh;
        }

        private void OnDestroy () {
            EditorBridge.OnTemplateUpdate -= Refresh;
        }

        /// <summary>
        /// Adds a node to the graph
        /// </summary>
        /// <param name="node">The node to add</param>
        public override void AddNode (Node node) {
            var editorNode = node as EditorNode;
            var aiNode = editorNode.Node;
            Undo.RegisterCreatedObjectUndo (aiNode, string.Format ("Created {0}", aiNode.name));
            AssetDatabase.AddObjectToAsset (aiNode, template);
            AssetDatabase.SaveAssets ();

            SerializedTemplate.Update ();

            SerializedProperty arrayProp;
            switch (editorNode.Type) {
                case NodeType.Observer:
                    arrayProp = SerializedTemplate.FindProperty (AINodeUtility.ObserverPropertyName);
                    break;

                case NodeType.Decision:
                    arrayProp = SerializedTemplate.FindProperty (AINodeUtility.DecisionPropertyName);
                    break;

                default:
                    arrayProp = SerializedTemplate.FindProperty (AINodeUtility.ActionPropertyName);
                    break;
            }

            arrayProp.InsertArrayElementAtIndex (0);
            arrayProp.GetArrayElementAtIndex (0).objectReferenceValue = aiNode;

            SerializedTemplate.ApplyModifiedProperties ();

            node.graph = this;
            editorNodes.Add (aiNode.Id, editorNode);
            nodes.Add (node);
            Dirty ();
        }

        /// <summary>
        /// Checks to see if the two slots are able to connect
        /// </summary>
        public override bool CanConnect (Slot fromSlot, Slot toSlot) {
            var input = fromSlot.node as EditorNode;
            var output = toSlot.node as EditorNode;

            return input.Type == NodeType.Observer && output.Type == NodeType.Decision ||
                input.Type == NodeType.Decision && output.Type == NodeType.Action;
        }

        /// <summary>
        /// Connects the two nodes together
        /// </summary>
        /// <param name="fromSlot"></param>
        /// <param name="toSlot"></param>
        /// <returns></returns>
        public override Edge Connect (Slot fromSlot, Slot toSlot) {
            if (fromSlot.name == toSlot.name) {
                throw new System.ArgumentException ("Cannot connect a node to itself");
            }

            if (Connected (fromSlot, toSlot)) {
                throw new System.ArgumentException ("There is already a connection between the two slots");
            }

            var connection = CreateInstance<Connection> ();
            connection.hideFlags = HideFlags.HideInHierarchy;
            
            var serializedConnection = new SerializedObject (connection);
            var sourceNode = (fromSlot.node as EditorNode).Node;
            var targetNode = (toSlot.node as EditorNode).Node;

            // Prepare undo operations
            Undo.SetCurrentGroupName (string.Format ("Connect {0} to {1}", sourceNode.name, targetNode.name));

            serializedConnection.FindProperty ("m_Name").stringValue = string.Format ("{0} -> {1}", sourceNode.name, targetNode.name);
            serializedConnection.FindProperty (AINodeUtility.SourceIdPropertyName).intValue = sourceNode.Id;
            serializedConnection.FindProperty (AINodeUtility.TargetIdPropertyName).intValue = targetNode.Id;

            // Attach a condition, if possible
            var condition = CreateCondition (sourceNode, targetNode);
            if (condition != null) {
                serializedConnection.FindProperty ("connectionType").enumValueIndex = (int) ConnectionType.Conditional;
                serializedConnection.FindProperty (AINodeUtility.ConditionIdPropertyName).intValue = condition.Id;
            }

            serializedConnection.ApplyModifiedPropertiesWithoutUndo ();

            // Attach the objects to the template
            Undo.RegisterCreatedObjectUndo (connection, connection.name);
            AssetDatabase.AddObjectToAsset (connection, template);
            if (condition != null) {
                Undo.RegisterCreatedObjectUndo (condition, condition.name);
                AssetDatabase.AddObjectToAsset (condition, template);
            }

            AssetDatabase.SaveAssets ();

            SerializedTemplate.Update ();

            // Insert the node into the template
            var connectionArray = SerializedTemplate.FindProperty (AINodeUtility.ConnectionPropertyName);
            connectionArray.InsertArrayElementAtIndex (0);
            connectionArray.GetArrayElementAtIndex (0).objectReferenceValue = connection;

            // Insert the condition, if possible
            if (condition != null) {
                var conditionArray = SerializedTemplate.FindProperty (AINodeUtility.ConditionPropertyName);
                conditionArray.InsertArrayElementAtIndex (0);
                conditionArray.GetArrayElementAtIndex (0).objectReferenceValue = condition;
            }

            SerializedTemplate.ApplyModifiedProperties ();
            Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());

            var edge = new Edge (fromSlot, toSlot);
            edges.Add (edge);

            fromSlot.AddEdge (edge);
            toSlot.AddEdge (edge);

            fromSlot.node.slots.Add (toSlot);
            toSlot.node.slots.Add (fromSlot);

            (fromSlot.node as EditorNode).CalculateSlotPositions ();
            (toSlot.node as EditorNode).CalculateSlotPositions ();

            // Adjust all the weights if the connection is a conditional
            if (connection.ConnectionType == ConnectionType.Conditional) {
                var targetId = 0;
                if (!int.TryParse (edge.toSlotName, out targetId)) {
                    targetId = int.MinValue;
                }

                var connections = Template.Connections.Where (c => c.TargetId == targetId);
                var adjustedCount = Mathf.Max (connections.Count (), 1f);
                var weightDelta = 1f / adjustedCount;

                foreach (var con in connections) {
                    var serializedCon = new SerializedObject (con);
                    serializedCon.FindProperty ("weight").floatValue = weightDelta;
                    serializedCon.ApplyModifiedProperties ();
                }
            } else {
                var sourceId = 0;
                if (!int.TryParse (edge.fromSlotName, out sourceId)) {
                    sourceId = int.MinValue;
                }

                // Adjust the priorities such that the newly added connection is last on the list
                var connections = Template.Connections.Where (c => c.SourceId == sourceId);
                serializedConnection.FindProperty ("priority").intValue = connections.Count () + 1;
                serializedConnection.ApplyModifiedPropertiesWithoutUndo ();
            }

            return edge;
        }

        public override bool Connected (Slot fromSlot, Slot toSlot) {
            var sourceId = int.MinValue;
            var targetId = int.MaxValue;

            if (int.TryParse (fromSlot.name, out sourceId) && int.TryParse (toSlot.name, out targetId)) {
                return base.Connected (fromSlot, toSlot) ||
                    template.Connections.Where (c => c.SourceId == sourceId && c.TargetId == targetId).Any ();
            }

            return false;
        }

        /// <summary>
        /// Removes a node from the graph
        /// </summary>
        /// <param name="node">The node to remove</param>
        /// <param name="destroyNode">Should the graph delete this copy manually?</param>
        public override void RemoveNode (Node node, bool destroyNode = false) {
            if (node == null) { return; }

            var aiNode = (node as EditorNode).Node;
            var nodeId = aiNode.Id;

            Undo.SetCurrentGroupName (string.Format ("Delete {0} from {1}", aiNode.name, template.name));
            Undo.RecordObject (template, template.name);
            SerializedTemplate.Update ();

            // Delete the node
            SerializedProperty arrayProp;

            switch ((node as EditorNode).Type) {
                case NodeType.Observer:
                    arrayProp = SerializedTemplate.FindProperty (AINodeUtility.ObserverPropertyName);
                    break;

                case NodeType.Decision:
                    arrayProp = SerializedTemplate.FindProperty (AINodeUtility.DecisionPropertyName);
                    break;

                default:
                    arrayProp = SerializedTemplate.FindProperty (AINodeUtility.ActionPropertyName);
                    break;
            }

            for (var i = 0; i < arrayProp.arraySize; ++i) {
                var element = arrayProp.GetArrayElementAtIndex (i);
                var nodeToCompare = element.objectReferenceValue as AINode;

                if (nodeToCompare.Id == aiNode.Id) {
                    element.objectReferenceValue = null;
                    arrayProp.DeleteArrayElementAtIndex (i);
                    break;
                }
            }

            SerializedTemplate.ApplyModifiedProperties ();

            // Delete all instances of connections (and conditions) that contains the node's id
            RemoveConnectionsContainingId (aiNode.Id);

            // Delete the node via an undo
            Undo.DestroyObjectImmediate (aiNode);

            base.RemoveNode (node, destroyNode);

            // Clean up slots
            var nodeStringId = nodeId.ToString ();
            edges.RemoveAll (e => e.fromSlotName == nodeStringId || e.toSlotName == nodeStringId);
            for (var i = 0; i < nodes.Count; ++i) {
                nodes[i].slots.RemoveAll (s => s.name == nodeStringId);
            }

            Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());

            if (OnRequireRepaint != null) {
                OnRequireRepaint ();
            }
        }

        /// <summary>
        /// Removes a set of nodes from the graph
        /// </summary>
        /// <param name="nodesToRemove">The nodes to remove</param>
        /// <param name="destroyNodes">Should the nodes be destroyed?</param>
        public override void RemoveNodes (List<Node> nodesToRemove, bool destroyNodes = false) {
            Undo.SetCurrentGroupName (string.Format ("Remove {0} nodes from {1}", nodesToRemove.Count, template.name));
            Undo.RecordObject (template, template.name);
            SerializedTemplate.Update ();

            var deletionList = new List<Object> ();
            var idSet = new HashSet<int> ();
            var observerArray = SerializedTemplate.FindProperty (AINodeUtility.ObserverPropertyName);
            var decisionArray = SerializedTemplate.FindProperty (AINodeUtility.DecisionPropertyName);
            var actionArray = SerializedTemplate.FindProperty (AINodeUtility.ActionPropertyName);
            var connectionArray = SerializedTemplate.FindProperty (AINodeUtility.ConnectionPropertyName);
            var conditionArray = SerializedTemplate.FindProperty (AINodeUtility.ConditionPropertyName);

            for (var i = 0; i < nodesToRemove.Count; ++i) {
                var node = nodesToRemove[i] as EditorNode;
                var aiNode = node.Node;

                if (aiNode == null) { continue; }

                // Delete the node
                switch (node.Type) {
                    case NodeType.Observer:
                        DeleteAINodeReference (aiNode, observerArray, deletionList);
                        break;

                    case NodeType.Decision:
                        DeleteAINodeReference (aiNode, decisionArray, deletionList);
                        break;

                    case NodeType.Action:
                        DeleteAINodeReference (aiNode, actionArray, deletionList);
                        break;
                }

                // Delete the connections, if any
                DeleteConnectionsContainingId (aiNode.Id, connectionArray, conditionArray, deletionList);

                idSet.Add (aiNode.Id);
            }

            SerializedTemplate.ApplyModifiedProperties ();

            // Clear out all the editor nodes involving the nodes to delete
            nodes.RemoveAll (n => nodesToRemove.Contains (n));
            edges.RemoveAll ((e) => {
                var sourceId = int.MinValue;
                var targetId = int.MinValue;

                if (int.TryParse (e.fromSlotName, out sourceId) && int.TryParse (e.toSlotName, out targetId)) {
                    return idSet.Contains (sourceId) || idSet.Contains (targetId);
                }

                return false;
            });

            for (var i = 0; i < nodes.Count; ++i) {
                var node = nodes[i];

                node.slots.RemoveAll ((s) => {
                    var id = int.MinValue;
                    return int.TryParse (s.name, out id) && idSet.Contains (id);
                });
            }

            // Delete the actual reference
            Undo.RecordObjects (deletionList.ToArray (), string.Empty);
            for (var i = 0; i < deletionList.Count; ++i) {
                Undo.DestroyObjectImmediate (deletionList[i]);
            }

            SerializedTemplate.ApplyModifiedProperties ();
            Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());

            if (OnRequireRepaint != null) {
                OnRequireRepaint ();
            }
        }

        /// <summary>
        /// Attempts to find a node with the same id
        /// </summary>
        /// <param name="id">The id of the node</param>
        /// <returns>Null if no node with the same id is found</returns>
        [System.Obsolete]
        public EditorNode GetNode (string id) {
            EditorNode node;

            if (editorNodes.TryGetValue (id.GetHashCode (), out node)) {
                return node;
            }

            return null;
        }

        /// <summary>
        /// Attempts to find a node with the same id
        /// </summary>
        /// <param name="id">The id of the node</param>
        /// <returns>Null if no node with the same id is found</returns>
        public EditorNode GetNode (int id) {
            EditorNode node = default (EditorNode);

            if (editorNodes.TryGetValue (id, out node)) {
                return node;
            }

            return node;
        }

        /// <summary>
        /// Updates the template data to create new nodes
        /// </summary>
        public void Refresh () {
            CleanReferences ();

            if (template == null) {
                return;
            }

            SanitizeTemplate (template);

            var observers = template.Observers;
            foreach (var obs in observers) {
                var node = CreateInstance<EditorNode> ();
                node.graph = this;
                node.PrepareNode (obs, NodeType.Observer);
                editorNodes.Add (obs.Id, node);
                nodes.Add (node);
            }

            var decisions = template.Decisions;
            foreach (var dec in decisions) {
                var node = CreateInstance<EditorNode> ();
                node.graph = this;
                node.PrepareNode (dec, NodeType.Decision);
                editorNodes.Add (dec.Id, node);
                nodes.Add (node);
            }

            var actions = template.Actions;
            foreach (var act in actions) {
                var node = CreateInstance<EditorNode> ();
                node.graph = this;
                node.PrepareNode (act, NodeType.Action);
                editorNodes.Add (act.Id, node);
                nodes.Add (node);
            }

            var connections = template.Connections;
            foreach (var con in connections) {
                var input = new Slot (SlotType.InputSlot, con.SourceId.ToString ());
                var output = new Slot (SlotType.OutputSlot, con.TargetId.ToString ());

                input.node = editorNodes[con.SourceId];
                output.node = editorNodes[con.TargetId];

                input.node.slots.Add (output);
                output.node.slots.Add (input);

                var edge = new Edge (input, output);
                input.AddEdge (edge);
                output.AddEdge (edge);

                edges.Add (new Edge (input, output));
            }

            foreach (var node in editorNodes.Values) {
                node.CalculateSlotPositions ();
            }
        }

        public void RemoveConnectionEdge (Edge edge) {
            var sourceId = AINodeUtility.ConvertToIntId (edge.fromSlotName);
            var targetId = AINodeUtility.ConvertToIntId (edge.toSlotName);
            var markedObject = new List<Object> ();

            Undo.SetCurrentGroupName ("Remove connection");
            SerializedTemplate.Update ();
            Undo.RecordObject (template, template.name);

            var connections = SerializedTemplate.FindProperty (AINodeUtility.ConnectionPropertyName);
            var conditions = SerializedTemplate.FindProperty (AINodeUtility.ConditionPropertyName);

            for (var i = 0; i < connections.arraySize; ++i) {
                var element = connections.GetArrayElementAtIndex (i);
                var connection = element.objectReferenceValue as Connection;

                if (connection.SourceId == sourceId && connection.TargetId == targetId) {
                    // If the connection contains a condition, remove that too
                    if (connection.ConnectionType == ConnectionType.Conditional) {
                        var conditionId = connection.ConditionId;

                        for (var k = 0; k < conditions.arraySize; ++k) {
                            var conditionElement = conditions.GetArrayElementAtIndex (k);
                            var condition = conditionElement.objectReferenceValue as Condition;

                            if (condition.Id == conditionId) {
                                conditionElement.objectReferenceValue = null;
                                conditions.DeleteArrayElementAtIndex (k);
                                markedObject.Add (condition);

                                SerializedTemplate.ApplyModifiedProperties ();
                                break;
                            }
                        }
                    }

                    markedObject.Add (connection);
                    element.objectReferenceValue = null;
                    connections.DeleteArrayElementAtIndex (i);
                    SerializedTemplate.ApplyModifiedProperties ();
                    break;
                }
            }

            Undo.RecordObjects (markedObject.ToArray (), string.Empty);
            foreach (var obj in markedObject) {
                Undo.DestroyObjectImmediate (obj);
            }

            SerializedTemplate.ApplyModifiedProperties ();
            Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());

            edges.RemoveAll (e => e == edge);
            foreach (var node in nodes) {
                node.slots.RemoveAll (s => s == edge.fromSlot || s == edge.toSlot);
            }
        }

        /// <summary>
        /// Removes all nodes instances from the graph
        /// </summary>
        private void CleanReferences () {
            foreach (var node in editorNodes.Values) {
                if (node != null) {
                    DestroyImmediate (node);
                }
            }

            edges.Clear ();
            editorNodes.Clear ();
            nodes.Clear ();
        }

        /// <summary>
        /// Attempts to create a condition between the two nodes
        /// </summary>
        private Condition CreateCondition (AINode source, AINode target) {
            // If the nodes are valid for a conditional, create the conditional files
            if (source is Observer && target is Decision) {
                var script = ScriptDatabase.FindConditionScript (source as Observer);

                if (script != null) {
                    var conditional = CreateInstance (script.GetClass ()) as Condition;

                    var serializedCondition = new SerializedObject (conditional);
                    serializedCondition.FindProperty (AINodeUtility.IdPropertyName).intValue = conditional.GetInstanceID ();
                    serializedCondition.ApplyModifiedPropertiesWithoutUndo ();

                    conditional.name = string.Format ("{0} -> {1}", source.name, target.name);
                    conditional.hideFlags = HideFlags.HideInHierarchy;

                    return conditional;
                }
            }

            return default (Condition);
        }

        private void DeleteAINodeReference (AINode node, SerializedProperty array, List<Object> deletionList) {
            for (var i = 0; i < array.arraySize; ++i) {
                var element = array.GetArrayElementAtIndex (i);
                var elementNode = element.objectReferenceValue as AINode;

                if (elementNode == node) {
                    deletionList.Add (elementNode);
                    element.objectReferenceValue = null;
                    array.DeleteArrayElementAtIndex (i);
                    break;
                }
            }
        }

        private void DeleteConnectionsContainingId (int id, SerializedProperty connectionArray, SerializedProperty conditionArray, List<Object> deletionList) {
            for (var i = 0; i < connectionArray.arraySize; ++i) {
                var element = connectionArray.GetArrayElementAtIndex (i);
                var connection = element.objectReferenceValue as Connection;

                if (connection.SourceId == id || connection.TargetId == id) {
                    // If the connection is a conditional, delete that too
                    if (connection.ConnectionType == ConnectionType.Conditional) {
                        for (var k = 0; k < conditionArray.arraySize; ++k) {
                            var conditionElement = conditionArray.GetArrayElementAtIndex (k);
                            var condition = conditionElement.objectReferenceValue as Condition;

                            if (condition.Id == connection.ConditionId) {
                                deletionList.Add (condition);
                                conditionElement.objectReferenceValue = null;
                                conditionArray.DeleteArrayElementAtIndex (k);
                                break;
                            }
                        }
                    }

                    deletionList.Add (connection);
                    element.objectReferenceValue = null;
                    connectionArray.DeleteArrayElementAtIndex (i);
                    break;
                }
            }
        }

        /// <summary>
        /// Checks to see if the template contains no null references
        /// </summary>
        /// <returns></returns>
        private bool IsTemplateDirty (AITemplate template) {
            return template.Observers.Where (o => o == null).Any () ||
                template.Decisions.Where (d => d == null).Any () ||
                template.Actions.Where (a => a == null).Any () ||
                template.Connections.Where (c => c == null).Any () ||
                template.Conditions.Where (c => c == null).Any ();
        }

        /// <summary>
        /// Marks and deletes all connections that contains the id
        /// </summary>
        /// <param name="id">The id to check against</param>
        private void RemoveConnectionsContainingId (int id) {
            SerializedTemplate.Update ();
            Undo.RecordObject (template, "Removing connections");

            var markedObjects = new List<Object> ();
            var connectionProp = SerializedTemplate.FindProperty (AINodeUtility.ConnectionPropertyName);
            var conditionProp = SerializedTemplate.FindProperty (AINodeUtility.ConditionPropertyName);

            // Find and delete connections
            for (var i = 0; i < connectionProp.arraySize; ++i) {
                var element = connectionProp.GetArrayElementAtIndex (i);
                var connection = element.objectReferenceValue as Connection;

                // If the connection's source OR target id matches the current id,
                // then delete the connection
                if (connection.SourceId == id || connection.TargetId == id) {
                    // Check if it has a condition. If it is - then delete that too
                    if (connection.ConnectionType == ConnectionType.Conditional) {
                        var conditionId = connection.ConditionId;

                        for (var k = 0; k < conditionProp.arraySize; ++k) {
                            var conditionElement = conditionProp.GetArrayElementAtIndex (k);
                            var condition = conditionElement.objectReferenceValue as Condition;

                            if (condition.Id == conditionId) {
                                markedObjects.Add (condition);
                                conditionElement.objectReferenceValue = null;
                                conditionProp.DeleteArrayElementAtIndex (k);
                                break;
                            }
                        }
                    }

                    // Mark the connection for deletion
                    markedObjects.Add (connection);
                    element.objectReferenceValue = null;
                    connectionProp.DeleteArrayElementAtIndex (i);

                    i--;
                }
            }

            SerializedTemplate.ApplyModifiedProperties ();

            // Delete all related nodes
            Undo.RecordObjects (markedObjects.ToArray (), string.Empty);
            for (var i = 0; i < markedObjects.Count; ++i) {
                Undo.DestroyObjectImmediate (markedObjects[i]);
            }

            SerializedTemplate.ApplyModifiedProperties ();
        }

        /// <summary>
        /// Removes all null references from an array
        /// </summary>
        private void SanitizeArrayProp (SerializedProperty array) {
            for (var i = 0; i < array.arraySize; ++i) {
                var element = array.GetArrayElementAtIndex (i);

                if (element.objectReferenceValue == null) {
                    array.DeleteArrayElementAtIndex (i);
                    i--;
                }
            }
        }

        private void SanitizeConnections (SerializedProperty array, AITemplate template) {
            for (var i = 0; i < array.arraySize; ++i) {
                var element = array.GetArrayElementAtIndex (i);
                var connection = element.objectReferenceValue as Connection;
                var shouldDelete = false;

                switch (connection.ConnectionType) {
                    case ConnectionType.Conditional:
                        shouldDelete = !(template.Observers.Where (o => o.Id == connection.SourceId).Any () &&
                            template.Decisions.Where (d => d.Id == connection.TargetId).Any ());
                        break;

                    case ConnectionType.Simple:
                        shouldDelete = !(template.Decisions.Where (d => d.Id == connection.SourceId).Any () &&
                            template.Actions.Where (a => a.Id == connection.TargetId).Any ());
                        break;
                }

                if (shouldDelete) {
                    DestroyImmediate (element.objectReferenceValue, true);
                    element.objectReferenceValue = null;
                    array.DeleteArrayElementAtIndex (i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Removes all null references from the template
        /// </summary>
        private void SanitizeTemplate (AITemplate template) {
            var serializedTemplate = new SerializedObject (template);

            SanitizeArrayProp (serializedTemplate.FindProperty (AINodeUtility.ObserverPropertyName));
            SanitizeArrayProp (serializedTemplate.FindProperty (AINodeUtility.DecisionPropertyName));
            SanitizeArrayProp (serializedTemplate.FindProperty (AINodeUtility.ActionPropertyName));
            SanitizeArrayProp (serializedTemplate.FindProperty (AINodeUtility.ConnectionPropertyName));
            SanitizeArrayProp (serializedTemplate.FindProperty (AINodeUtility.ConditionPropertyName));

            serializedTemplate.ApplyModifiedPropertiesWithoutUndo ();

            SanitizeConnections (serializedTemplate.FindProperty (AINodeUtility.ConnectionPropertyName), template);
            serializedTemplate.ApplyModifiedPropertiesWithoutUndo ();

            var path = AssetDatabase.GetAssetPath (Template);
            if (!string.IsNullOrEmpty (path)) {
                var assets = AssetDatabase.LoadAllAssetsAtPath (path);
                var deletionList = new List<Object> ();

                foreach (var asset in assets) {
                    var assetExists = template.Observers.Where (o => o == asset).Any () ||
                        template.Decisions.Where (d => d == asset).Any () ||
                        template.Actions.Where (a => a == asset).Any () ||
                        template.Conditions.Where (c => c == asset).Any () ||
                        template.Connections.Where (c => c == asset).Any () ||
                        template.Variables.Where (v => v == asset).Any () ||
                        template == asset;

                    if (!assetExists) {
                        deletionList.Add (asset);
                    }
                }

                if (deletionList.Count > 0) {
                    foreach (var asset in deletionList) {
                        DestroyImmediate (asset, true);
                    }
                }
            }
        }
    }
}