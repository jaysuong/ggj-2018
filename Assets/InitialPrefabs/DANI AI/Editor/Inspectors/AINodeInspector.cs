using System.Collections.Generic;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    using Object = UnityEngine.Object;

    /// <summary>
    /// The base inspector for all AIModules
    /// </summary>
    public abstract class AINodeInspector<T> : UnityEditor.Editor where T : AINode {
        /// <summary>
        /// The inspected module
        /// </summary>
        [SerializeField]
        protected T node;

        protected SerializedObject serializedTemplate;

        /// <summary>
        /// The diagram that the module is attached to
        /// </summary>
        protected AITemplate template;

        /// <summary>
        /// The asset path of the module.  This is null if the inspected module is a
        /// runtime module
        /// </summary>
        [SerializeField]
        protected string path;

        /// <summary>
        /// The style used to display weights
        /// </summary>
        protected GUIStyle weightStyle;

        /// <summary>
        /// The set of property names that should be ignored to avoid confusion
        /// </summary>
        protected HashSet<string> ConditionIgnoreSet { get; private set; }

        /// <summary>
        /// Is the inspected module a runtime module?
        /// </summary>
        protected bool IsRuntime { get; private set; }

        /// <summary>
        /// A collection of serialized objects
        /// </summary>
        protected Dictionary<Object, SerializedObject>.ValueCollection SerializedObjects { get { return serializedBank.Values; } }

        /// <summary>
        /// The property name used to insert spaces after drawing
        /// </summary>
        protected HashSet<string> SpaceInsertnameSet { get; private set; }

        private Dictionary<Object, SerializedObject> serializedBank;

        public virtual void OnEnable () {
            node = target as T;

            path = AssetDatabase.GetAssetPath (target.GetInstanceID ());
            IsRuntime = string.IsNullOrEmpty (path);

            weightStyle = new GUIStyle ();
            weightStyle.alignment = TextAnchor.MiddleRight;

            ConditionIgnoreSet = new HashSet<string> (new string[] { "m_Script", "m_Name", "_isEnabled", "_shouldDrawGizmos" });
            SpaceInsertnameSet = new HashSet<string> ();

            serializedBank = new Dictionary<Object, SerializedObject> ();

            if (IsRuntime) {
                template = node.Template;
            } else {
                template = AssetDatabase.LoadAssetAtPath<AITemplate> (path);
            }

            serializedTemplate = new SerializedObject (template);
        }

        public override void OnInspectorGUI () {
            serializedObject.Update ();

            EditorGUI.BeginChangeCheck ();

            EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Name"));

            DrawInspector ();

            if (EditorGUI.EndChangeCheck ()) {
                serializedObject.ApplyModifiedProperties ();
            }

            EditorGUILayout.Space ();
        }

        protected override void OnHeaderGUI () {
            GUILayout.Space (3f);

            var rect = EditorGUILayout.BeginVertical (EditorStyles.inspectorFullWidthMargins);
            GUILayout.Space (3f);

            GUI.Box (rect, GUIContent.none);

            EditorGUILayout.BeginHorizontal ();

            var icon = AssetPreview.GetMiniThumbnail (target);
            GUILayout.Box (icon, GUIStyle.none, GUILayout.Width (32f), GUILayout.Height (32f));
            EditorGUILayout.LabelField (serializedObject.FindProperty ("m_Name").stringValue, EditorStyles.largeLabel);

            EditorGUILayout.EndHorizontal ();

            GUILayout.Space (3f);
            EditorGUILayout.EndVertical ();

            GUILayout.Space (3f);
        }

        /// <summary>
        /// An overridable method that draws customized controls for the AIModule
        /// </summary>
        public virtual void DrawCustomControls () { }

        protected SerializedObject FindSerializedObject (Object obj) {
            var serializedObj = default (SerializedObject);

            if (!serializedBank.TryGetValue (obj, out serializedObj)) {
                serializedObj = new SerializedObject (obj);
                serializedBank.Add (obj, serializedObj);
            }

            return serializedObj;
        }

        /// <summary>
        /// Removes a connection from the AI node
        /// </summary>
        /// <param name="connection">The connection to remove</param>
        protected void RemoveConnection (Connection connection) {
            Undo.SetCurrentGroupName ("Remove connection");
            serializedTemplate.Update ();

            Undo.RecordObject (template, string.Empty);

            var markedObjects = new List<Object> ();
            var connectionProp = serializedTemplate.FindProperty (AINodeUtility.ConnectionPropertyName);
            var conditionProp = serializedTemplate.FindProperty (AINodeUtility.ConditionPropertyName);

            // Find the correct connection
            for (var i = 0; i < connectionProp.arraySize; ++i) {
                var element = connectionProp.GetArrayElementAtIndex (i);
                var currentConnection = element.objectReferenceValue as Connection;

                if (currentConnection == connection) {
                    // If the connection is a conditional - delete the condition
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

                    markedObjects.Add (connection);
                    element.objectReferenceValue = null;
                    connectionProp.DeleteArrayElementAtIndex (i);
                    break;
                }
            }

            serializedTemplate.ApplyModifiedProperties ();

            // Delete all related nodes
            Undo.RecordObjects (markedObjects.ToArray (), string.Empty);
            for (var i = 0; i < markedObjects.Count; ++i) {
                Undo.DestroyObjectImmediate (markedObjects[i]);
            }

            serializedTemplate.ApplyModifiedProperties ();
            Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());
        }

        /// <summary>
        /// Updates the template and its serialized object reference
        /// </summary>
        /// <param name="template">The template to update</param>
        protected void UpdateTemplate (AITemplate template) {
            this.template = template;
            serializedTemplate = new SerializedObject (template);
        }

        /// <summary>
        /// Draws the inspector
        /// </summary>
        private void DrawInspector () {
            var p = serializedObject.GetIterator ();
            p.Next (true);

            EditorGUI.BeginDisabledGroup (true);

            p.NextVisible (false);
            EditorGUILayout.PropertyField (p);

            EditorGUI.EndDisabledGroup ();

            while (p.NextVisible (false)) {
                EditorGUILayout.PropertyField (p, true);

                if (SpaceInsertnameSet.Contains (p.name)) {
                    EditorGUILayout.Space ();
                }
            }
        }
    }
}