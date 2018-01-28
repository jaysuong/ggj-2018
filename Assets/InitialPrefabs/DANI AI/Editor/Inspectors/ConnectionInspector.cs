using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// An inspector for connection objects
    /// </summary>
    [CustomEditor (typeof (Connection), true)]
    public class ConnectionInspector : Editor {

        private Connection connection;
        private AITemplate template;
        private Editor conditionEditor;

        private void OnEnable () {
            connection = target as Connection;

            if (connection == null) {
                DestroyImmediate (this);
                return;
            }

            var path = AssetDatabase.GetAssetPath (connection.GetInstanceID ());

            if (!string.IsNullOrEmpty (path)) {
                template = AssetDatabase.LoadAssetAtPath<AITemplate> (path);
            } else {
                template = connection.Template;
            }

            if (connection.ConnectionType == ConnectionType.Conditional) {
                var condition = template.Conditions.Where (c => c.Id == connection.ConditionId).FirstOrDefault ();
                if (condition != null) {
                    conditionEditor = CreateEditor (condition);
                }
            }
        }

        private void OnDisable () {
            if (conditionEditor != null) {
                DestroyImmediate (conditionEditor);
            }
        }

        public override void OnInspectorGUI () {
            serializedObject.Update ();

            try {
                var startNode = FindNode (connection.SourceId, true,
                    connection.ConnectionType == ConnectionType.Conditional);
                var targetNode = FindNode (connection.TargetId, false,
                    connection.ConnectionType == ConnectionType.Conditional);

                DrawNodeElement ("From: ", startNode);
                DrawNodeElement ("To: ", targetNode);

                var preferredName = string.Format ("{0} -> {1}", startNode.name, targetNode.name);
                var prop = serializedObject.FindProperty ("m_Name");
                if (prop.stringValue != preferredName) {
                    prop.stringValue = preferredName;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo ();
                }
            } catch {
                return;
            }

            EditorGUILayout.Space ();

            if (conditionEditor != null && conditionEditor.target != null) {
                EditorGUILayout.LabelField ("Under the following condition: ", EditorStyles.boldLabel);

                var rect = EditorGUILayout.BeginVertical ();

                GUI.Box (rect, GUIContent.none);

                EditorGUILayout.Space ();
                conditionEditor.OnInspectorGUI ();

                EditorGUILayout.EndVertical ();
            }
        }

        private void DrawNodeElement (string pretext, AINode node) {
            EditorGUILayout.BeginHorizontal ();

            EditorGUILayout.LabelField (pretext, EditorStyles.boldLabel, GUILayout.Width (35f));
            EditorGUILayout.Space ();

            if (GUILayout.Button (node.name, EditorStyles.miniButton)) {
                Selection.activeObject = node;
            }

            EditorGUILayout.EndHorizontal ();
        }

        private AINode FindNode (int id, bool isSourceNode, bool isConditional) {
            if (isSourceNode) {
                if (isConditional) {
                    return template.Observers.Where (o => o.Id == id).FirstOrDefault ();
                } else {
                    return template.Decisions.Where (d => d.Id == id).FirstOrDefault ();
                }
            } else {
                if (isConditional) {
                    return template.Decisions.Where (d => d.Id == id).FirstOrDefault ();
                } else {
                    return template.Actions.Where (a => a.Id == id).FirstOrDefault ();
                }
            }
        }
    }
}