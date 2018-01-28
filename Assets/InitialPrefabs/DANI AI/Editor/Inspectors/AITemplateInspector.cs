using InitialPrefabs.DANI;
using UnityEditor;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// A custom editor for AITemplates.  Allows the ability to open the
    /// editor via the inspector.
    /// </summary>
    [CustomEditor(typeof(AITemplate), true)]
    public class AITemplateInspector : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            if(GUILayout.Button("Open in the Editor")) {
                DaniEditorWindow.Open(target as AITemplate);
            }
        }

    }
}