using System.Collections.Generic;
using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// Custom inspector for decision modules
    /// </summary>
    [CustomEditor (typeof (Decision), true)]
    public class DecisionInspector : AINodeInspector<Decision> {
        [SerializeField]
        private SerializedObject conditionObject;

        [SerializeField]
        private SerializedObject actionObject;

        private List<Connection> connections;
        private ReorderableList observerList;
        private ReorderableList actionList;

        private int selectedObserverIndex;
        private int selectedActionIndex;
        private Connection selectedActionConnection;

        public override void OnEnable () {
            base.OnEnable ();

            selectedObserverIndex = -1;
            selectedActionIndex = -1;
            PrepareObserverList ();
            PrepareActionList ();

            SpaceInsertnameSet.Add ("m_shouldDrawGizmos");
            SpaceInsertnameSet.Add ("m_scoreBoostOnFocus");

            Undo.undoRedoPerformed += HandleUndoEvent;
        }

        public virtual void OnDisable () {
            Undo.undoRedoPerformed -= HandleUndoEvent;
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI ();

            DrawCustomControls ();
            EditorGUILayout.Space ();

            // Update all representations
            foreach (var serializedObj in SerializedObjects) {
                if (serializedObj.targetObject != null) {
                    serializedObj.Update ();
                }
            }

            try {
                observerList.DoLayoutList ();

                if (observerList.count > 0) {
                    if (GUILayout.Button ("Reset Weights", EditorStyles.miniButton)) {
                        ResetWeights (connections);
                    }
                }
            } catch { }
            EditorGUILayout.Space ();

            try {
                actionList.DoLayoutList ();
            } catch { }

        }

        /// <summary>
        /// Adjusts the weights of all connections by a delta value
        /// </summary>
        /// <param name="connections">The connections to adjust</param>
        /// <param name="changedConnection">The connection that was updated</param>
        /// <param name="delta">The amount of change</param>
        private void AdjustConnectionWeights (List<Connection> connections, Connection changedConnection, float delta) {
            var redistributableCons = connections.
            Where (c => c != changedConnection && Mathf.Sign (delta) > 0f ? c.Weight > 0f : c.Weight < 1f);

            if (connections.Count > 1 && redistributableCons.Count () > 0) {
                // Run the first pass
                var averageDelta = delta / redistributableCons.Count ();
                var leftoverDelta = 0f;

                foreach (var con in redistributableCons) {
                    var serializedCon = FindSerializedObject (con);
                    var weightProperty = serializedCon.FindProperty ("weight");

                    if (weightProperty.floatValue - averageDelta >= 0f) {
                        weightProperty.floatValue -= averageDelta;
                    } else {
                        leftoverDelta += weightProperty.floatValue;
                        weightProperty.floatValue = 0f;
                    }

                    serializedCon.ApplyModifiedProperties ();
                }

                if (leftoverDelta > 0f) {
                    AdjustConnectionWeights (connections, changedConnection, leftoverDelta);
                }
            }
        }

        /// <summary>
        /// Calculates the height of a given action
        /// </summary>
        /// <param name="index">The index of the action</param>
        /// <returns>The height of the element</returns>
        private float GetActionHeight (int index) {
            if (index == selectedActionIndex) {
                var action = template.Actions.Where (a => a.Id == selectedActionConnection.TargetId).First ();
                var pointer = base.FindSerializedObject (action).GetIterator ();
                pointer.Next (true);

                var height = 0f;
                while (pointer.NextVisible (false)) {
                    if (pointer.name == "m_Script") { continue; }
                    height += EditorGUI.GetPropertyHeight (pointer);
                }

                return actionList.elementHeight + height;
            } else {
                return actionList.elementHeight;
            }
        }

        private float GetObserverHeight (int index) {
            return index == selectedObserverIndex ? observerList.elementHeight * 3f : observerList.elementHeight;
        }

        /// <summary>
        /// Delete used to handle undo events
        /// </summary>
        private void HandleUndoEvent () {
            PrepareObserverList ();
            PrepareActionList ();
            Repaint ();
        }

        /// <summary>
        /// Genenerates a new observer reorderable list
        /// </summary>
        private void PrepareObserverList () {
            connections = template.Connections.
            Where (c => c.TargetId == node.Id).
            OrderBy (c => c.name).ToList ();

            observerList = new ReorderableList (connections, typeof (Connection),
                true, true, false, true);

            observerList.drawHeaderCallback = (Rect rect) => {
                GUI.Label (rect, "Connected Observers", EditorStyles.boldLabel);
                GUI.Label (rect, "Weight", weightStyle);
            };

            observerList.drawElementCallback = (Rect rect, int index, bool active, bool focused) => {
                var element = connections[index];
                var observer = template.Observers.Where (o => o.Id == element.SourceId).FirstOrDefault ();
                var serializedCon = FindSerializedObject (element);

                // Draw the sliders
                EditorGUI.BeginChangeCheck ();
                serializedCon.Update ();
                var weightProperty = serializedCon.FindProperty ("weight");
                var oldWeight = weightProperty.floatValue;

                EditorGUI.LabelField (new Rect (rect.x, rect.y, rect.width * 0.4f, rect.height), observer.name);

                var value = GUI.HorizontalSlider (
                    new Rect (rect.x + rect.width * 0.4f, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight),
                    weightProperty.floatValue, 0f, 1f);

                weightProperty.floatValue = value;

                EditorGUI.LabelField (
                    new Rect (rect.x + rect.width * 0.9f, rect.y, rect.width * 0.1f, observerList.elementHeight),
                    new GUIContent ((Mathf.RoundToInt (weightProperty.floatValue * 100f) / 100f).ToString ()));

                if (EditorGUI.EndChangeCheck ()) {
                    serializedCon.ApplyModifiedProperties ();
                    AdjustConnectionWeights (connections, element, weightProperty.floatValue - oldWeight);
                }

                // If the element is being selected, show buttons to select the observer or condition
                if (index == selectedObserverIndex) {
                    var elementHeight = observerList.elementHeight;
                    var localOffset = (elementHeight - EditorGUIUtility.singleLineHeight) * 0.5f;

                    if (GUI.Button (new Rect (rect.x, rect.y + elementHeight + localOffset, rect.width, EditorGUIUtility.singleLineHeight),
                            string.Format ("View '{0}'", observer.name), EditorStyles.miniButton)) {

                        EditorBridge.SelectDaniObject (observer);
                    }

                    if (GUI.Button (new Rect (rect.x, rect.y + elementHeight * 2f + localOffset, rect.width, EditorGUIUtility.singleLineHeight),
                            "View Condition", EditorStyles.miniButton)) {

                        EditorBridge.SelectDaniObject (element);
                    }
                }

                var current = Event.current;
                if (current.type == EventType.MouseDrag) {
                    selectedObserverIndex = -1;
                }
            };

            observerList.onSelectCallback = (ReorderableList list) => {
                selectedObserverIndex = list.index;
            };

            observerList.onRemoveCallback = (ReorderableList list) => {
                RemoveConnection (connections[list.index]);
                connections.RemoveAt (list.index);
                Repaint ();
            };

            observerList.elementHeightCallback = (int index) => {
                return selectedObserverIndex == index ? observerList.elementHeight * 3f : observerList.elementHeight;
            };

            observerList.drawElementBackgroundCallback = (Rect r, int index, bool active, bool focused) => {
                if (index == selectedObserverIndex && observerList.count > 0) {
                    EditorGUI.DrawRect (new Rect (r.x, r.y, r.width, GetObserverHeight (index)),
                        new Color (0f, 0f, 1f, 0.05f));
                }
            };
        }

        /// <summary>
        /// Generates a new action reorderable list
        /// </summary>
        private void PrepareActionList () {
            var connections = template.Connections.
            Where (c => c.SourceId == node.Id).
            OrderBy (c => c.Priority).ToList ();

            actionList = new ReorderableList (connections, typeof (Connection),
                true, true, false, true);

            actionList.drawHeaderCallback = (Rect rect) => {
                GUI.Label (rect, "Connected Actions", EditorStyles.boldLabel);
                GUI.Label (rect, "Priority", weightStyle);
            };

            actionList.drawElementCallback = (Rect rect, int index, bool active, bool focused) => {
                var element = connections[index];
                var action = template.Actions.Where (a => a.Id == element.TargetId).FirstOrDefault ();

                // Draw the header
                EditorGUI.LabelField (rect, action.name,
                    selectedActionIndex == index ? EditorStyles.boldLabel : EditorStyles.label);
                EditorGUI.LabelField (rect, element.Priority.ToString (), weightStyle);

                // Update the action's priority
                element.Priority = index + 1;

                if (selectedActionConnection == null) { return; }

                if (index == selectedActionIndex) {
                    EditorGUI.BeginChangeCheck ();
                    var serializedAction = base.FindSerializedObject (action);
                    serializedAction.Update ();

                    var pointer = serializedAction.GetIterator ();
                    pointer.Next (true);

                    var offset = actionList.elementHeight;

                    while (pointer.NextVisible (false)) {
                        if (pointer.name == "m_Script") { continue; }

                        var pointerHeight = EditorGUI.GetPropertyHeight (pointer);
                        EditorGUI.PropertyField (new Rect (rect.x, rect.y + offset, rect.width, pointerHeight), pointer, true);
                        offset += pointerHeight;
                    }

                    if (EditorGUI.EndChangeCheck ()) {
                        serializedAction.ApplyModifiedProperties ();
                    }
                }

                var current = Event.current;
                if (current.type == EventType.MouseDrag) {
                    selectedActionIndex = -1;
                }
            };

            actionList.onSelectCallback = (ReorderableList list) => {
                selectedActionIndex = list.index;
                selectedActionConnection = connections[list.index];
            };

            actionList.onRemoveCallback = (ReorderableList list) => {
                RemoveConnection (connections[list.index]);
                connections.RemoveAt (list.index);
                Repaint ();
            };

            actionList.onReorderCallback = (ReorderableList list) => { };

            actionList.elementHeightCallback = GetActionHeight;

            actionList.drawElementBackgroundCallback = (Rect r, int index, bool active, bool focused) => {
                if (index == selectedActionIndex && actionList.count > 0) {
                    EditorGUI.DrawRect (new Rect (r.x, r.y, r.width, GetActionHeight (index)),
                        new Color (0f, 0f, 1f, 0.05f));
                }
            };
        }

        /// <summary>
        /// Resets all the weights to their default values
        /// </summary>
        /// <param name="connections">The connections to reset</param>
        private void ResetWeights (List<Connection> connections) {
            Undo.SetCurrentGroupName (string.Format ("Resetting weights on {0}", node.name));
            Undo.RecordObjects (connections.ToArray (), "Connections");

            foreach (var connection in connections) {
                var serializedCon = FindSerializedObject (connection);

                var weightProperty = serializedCon.FindProperty ("weight");
                weightProperty.floatValue = 1f / connections.Count;

                serializedCon.ApplyModifiedProperties ();
            }

            Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());
        }

    }
}