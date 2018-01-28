using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// A custom editor for action modules
    /// </summary>
    [CustomEditor (typeof (Action), true)]
    public class ActionInspector : AINodeInspector<Action> {
        private ReorderableList decisionList;
        private int selectedDecisionIndex;
        private Connection selectedConnection;

        public override void OnEnable () {
            base.OnEnable ();

            if (IsRuntime) {
                EditorApplication.update += HandleRuntimeUpdate;
            } else {
                template = AssetDatabase.LoadAssetAtPath<AITemplate> (path);
                PrepareDecisionList ();
                Undo.undoRedoPerformed += HandleUndoEvent;
            }
            selectedDecisionIndex = -1;

            SpaceInsertnameSet.Add ("m_shouldDrawGizmos");
        }

        public virtual void OnDisable () {
            EditorApplication.update -= HandleRuntimeUpdate;
            Undo.undoRedoPerformed -= HandleUndoEvent;
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI ();

            DrawCustomControls ();
            EditorGUILayout.Space ();

            try {
                decisionList.DoLayoutList ();
            } catch { }
        }

        private void HandleRuntimeUpdate () {
            if (IsRuntime) {
                if (node.Template != null) {
                    template = node.Template;
                    PrepareDecisionList ();

                    Undo.undoRedoPerformed += HandleUndoEvent;
                    EditorApplication.update -= HandleRuntimeUpdate;
                }
            }
        }

        private void PrepareDecisionList () {
            var connections = template.Connections.
            Where (c => c.TargetId == node.Id).
            OrderBy (c => c.Priority).ToList ();

            decisionList = new ReorderableList (connections, typeof (Connection),
                true, true, false, true);

            decisionList.drawHeaderCallback = (Rect rect) => {
                GUI.Label (rect, "Connected Decisions");
                GUI.Label (rect, "Priority", weightStyle);
            };

            decisionList.drawElementCallback = (Rect rect, int index, bool active, bool focused) => {
                var element = connections[index];
                var decision = template.Decisions.Where (d => d.Id == element.SourceId).FirstOrDefault ();

                // Draw the header
                EditorGUI.LabelField (rect, decision.name,
                    selectedDecisionIndex == index ? EditorStyles.boldLabel : EditorStyles.label);
                EditorGUI.LabelField (rect, element.Priority.ToString (), weightStyle);

                if (selectedConnection == null) { return; }

                if (index == selectedDecisionIndex) {
                    var serializedDecision = FindSerializedObject (decision);
                    serializedDecision.Update ();

                    var pointer = serializedDecision.GetIterator ();
                    pointer.Next (true);

                    var offset = decisionList.elementHeight;

                    while (pointer.NextVisible (false)) {
                        if (pointer.name == "m_Script") { continue; }

                        var pointerHeight = EditorGUI.GetPropertyHeight (pointer);
                        EditorGUI.PropertyField (new Rect (rect.x, rect.y + offset, rect.width, pointerHeight), pointer, true);
                        offset += pointerHeight;
                    }

                    serializedDecision.ApplyModifiedProperties ();
                }

                var current = Event.current;
                if (current.type == EventType.MouseDrag) {
                    selectedDecisionIndex = -1;
                }
            };

            decisionList.onSelectCallback = (ReorderableList list) => {
                selectedDecisionIndex = list.index;
                selectedConnection = connections[list.index];
            };

            decisionList.onRemoveCallback = (ReorderableList list) => {
                RemoveConnection (connections[list.index]);
                connections.RemoveAt (list.index);
                Repaint ();
            };

            decisionList.elementHeightCallback = GetElementHeight;

            decisionList.drawElementBackgroundCallback = (Rect r, int index, bool active, bool focused) => {
                if (index == selectedDecisionIndex && decisionList.count > 0) {
                    EditorGUI.DrawRect (new Rect (r.x, r.y, r.width, GetElementHeight (index)),
                        new Color (0f, 0f, 1f, 0.05f));
                }
            };
        }

        private float GetElementHeight (int index) {
            if (index == selectedDecisionIndex) {
                var decision = template.Decisions.Where (d => d.Id == selectedConnection.SourceId).First ();
                var pointer = FindSerializedObject(decision).GetIterator ();
                pointer.Next (true);

                var height = 0f;
                while (pointer.NextVisible (false)) {
                    if (pointer.name == "m_Script") { continue; }
                    height += EditorGUI.GetPropertyHeight (pointer);
                }

                return decisionList.elementHeight + height;
            } else {
                return decisionList.elementHeight;
            }
        }

        /// <summary>
        /// Simple delegate involving undo commands
        /// </summary>
        private void HandleUndoEvent () {
            PrepareDecisionList ();
            Repaint ();
        }

    }
}