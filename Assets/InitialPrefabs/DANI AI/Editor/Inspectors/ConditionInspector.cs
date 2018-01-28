using System.Collections.Generic;
using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// The base editor for displaying conditions in the inspector
    /// </summary>
    [CustomEditor (typeof (Condition), true)]
    public class ConditionInspector : Editor {

        private HashSet<string> ignoreSet;

        private void OnEnable () {
            ignoreSet = new HashSet<string> (new string[] {
                "m_Script",
                "m_shouldDrawGizmos",
                "m_isEnabled"
            });

            var attributes = target.GetType ().GetCustomAttributes (true).
            Where (a => a is HideCompareValueAttribute);

            if (attributes.Any ()) {
                ignoreSet.Add ("compareValue");
            }
        }

        public override void OnInspectorGUI () {
            serializedObject.Update ();

            EditorGUI.BeginChangeCheck ();

            var pointer = serializedObject.GetIterator ();
            pointer.Next (true);

            while (pointer.NextVisible (false)) {
                if (!ignoreSet.Contains (pointer.name)) {
                    EditorGUILayout.PropertyField (pointer, true);
                }
            }

            if (EditorGUI.EndChangeCheck ()) {
                serializedObject.ApplyModifiedProperties ();
            }

            EditorGUILayout.Space ();
        }
    }
}