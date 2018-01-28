using System.Collections.Generic;
using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// Custom inspector for observer modules
    /// </summary>
    [CustomEditor (typeof (Observer), true)]
    public class ObserverInspector : AINodeInspector<Observer> {
        [SerializeField]
        private SerializedObject conditionObject;

        private ReorderableList decisionList;

        private int selectedDecisionIndex;
        private SerializedObject selectedCondition;

        public override void OnEnable () {
            base.OnEnable ();

            selectedDecisionIndex = -1;

            PrepareDecisionList ();

            Undo.undoRedoPerformed += HandleUndoEvent;

            weightStyle = new GUIStyle ();
            weightStyle.alignment = TextAnchor.MiddleRight;

            SpaceInsertnameSet.Add ("m_shouldDrawGizmos");
        }

        public virtual void OnDisable () {
            Undo.undoRedoPerformed -= HandleUndoEvent;
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI ();

            DrawCustomControls ();
            EditorGUILayout.Space ();

            try {
                if (decisionList != null) {
                    decisionList.DoLayoutList ();
                }
            } catch (System.Exception e) {
                Debug.LogError (e);
            }
        }

        private void PrepareDecisionList () {
            var connections = template.Connections.
            Where (c => c.SourceId == node.Id).
            OrderBy (c => c.Priority).ToList ();

            decisionList = new ReorderableList (connections, typeof (Connection),
                true, true, false, true);

            decisionList.drawHeaderCallback = (Rect r) => {
                GUI.Label (r, "Connected Decisions", EditorStyles.boldLabel);
                GUI.Label (r, "Weight", weightStyle);
            };

            decisionList.drawElementCallback = (Rect r, int index, bool active, bool focused) => {
                var element = connections[index];
                var decision = template.Decisions.Where (d => d.Id == element.TargetId).FirstOrDefault ();

                // Draw the header
                EditorGUI.LabelField (r, decision.name,
                    selectedDecisionIndex == index ? EditorStyles.boldLabel : EditorStyles.label);
                EditorGUI.LabelField (r, element.Weight.ToString (), weightStyle);

                if (selectedCondition == null || selectedCondition.targetObject == null) {
                    return;
                }

                // Draw condition contents
                if (selectedDecisionIndex == index) {
                    EditorGUI.BeginChangeCheck ();
                    selectedCondition.Update ();

                    var pointer = selectedCondition.GetIterator ();
                    var offset = decisionList.elementHeight;
                    pointer.Next (true);

                    EditorGUI.LabelField (new Rect (r.x, r.y + offset, r.width, EditorGUIUtility.singleLineHeight),
                        "Under the following condiiton", EditorStyles.miniBoldLabel);
                    offset += EditorGUIUtility.singleLineHeight;
                    GUI.Box (
                        new Rect (
                            r.x,
                            r.y + offset,
                            r.width,
                            GetElementHeight (index) - offset - EditorGUIUtility.singleLineHeight),
                        GUIContent.none);

                    while (pointer.NextVisible (false)) {
                        if (!ConditionIgnoreSet.Contains (pointer.name)) {
                            var pointerHeight = EditorGUI.GetPropertyHeight (pointer);
                            EditorGUI.PropertyField (new Rect (r.x + 3f, r.y + offset, r.width - 6f, pointerHeight), pointer, true);
                            offset += pointerHeight;
                        }
                    }

                    if (EditorGUI.EndChangeCheck ()) {
                        selectedCondition.ApplyModifiedProperties ();
                    }
                }
            };

            decisionList.onSelectCallback = (ReorderableList list) => {
                selectedDecisionIndex = list.index;
                selectedCondition = new SerializedObject (
                    template.Conditions.Where (c => c.Id == connections[list.index].ConditionId).FirstOrDefault ());
            };

            decisionList.onRemoveCallback = (ReorderableList list) => {
                RemoveConnection (connections[list.index]);
                connections.RemoveAt (list.index);
                Repaint ();
                EditorBridge.InvokeTemplateUpdateEvent ();
            };

            decisionList.drawElementBackgroundCallback = (Rect r, int index, bool active, bool focused) => {
                if (index == selectedDecisionIndex && decisionList.count > 0) {
                    EditorGUI.DrawRect (new Rect (r.x, r.y, r.width, GetElementHeight (index)),
                        new Color (0f, 0f, 1f, 0.05f));
                }
            };

            decisionList.elementHeightCallback = GetElementHeight;
        }

        /// <summary>
        /// Fetches the height of the element based on the index of the reorderable list
        /// </summary>
        private float GetElementHeight (int index) {
            if (index == selectedDecisionIndex) {
                var height = 0f;

                if (selectedCondition != null) {
                    var pointer = selectedCondition.GetIterator ();
                    pointer.Next (true);

                    while (pointer.NextVisible (false)) {
                        if (!ConditionIgnoreSet.Contains (pointer.name)) {
                            height += EditorGUI.GetPropertyHeight (pointer);
                        }
                    }
                }

                return decisionList.elementHeight + height + EditorGUIUtility.singleLineHeight * 2f;
            } else {
                return decisionList.elementHeight;
            }
        }

        /// <summary>
        /// Delete used to handle undo events
        /// </summary>
        private void HandleUndoEvent () {
            PrepareDecisionList ();
            Repaint ();
        }
    }
}