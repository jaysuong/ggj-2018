using System.Reflection;
using InitialPrefabs.DANI;
using UnityEditor;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// The main inspector for AIBrain.  Allows hooking into Dani Editor
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor (typeof (AIBrain), true)]
    public class AIBrainInspector : UnityEditor.Editor {
        private AIBrain brain;

        private const string TemplatePropertyName = "m_template";
        private const string ActiveDecisionName = "activeDecision";

        private void OnEnable () {
            brain = target as AIBrain;

            if (EditorApplication.isPlaying) {
                EditorApplication.update += HandleUpdateEvent;

                if (brain.Template != null) {
                    DaniRuntimeBridge.SelectBrain (brain);
                }
            } else {
                if (brain.Template != null) {
                    DaniRuntimeBridge.SelectBrain (brain);
                }
            }
        }

        private void OnDisable () {
            EditorApplication.update -= HandleUpdateEvent;
        }

        private void HandleUpdateEvent () {
            var state = brain.RunningStatus;

            if (state != RunningState.NotInitialized) {
                DaniRuntimeBridge.SelectBrain (brain);

                var field =typeof(AIBrain).GetField (ActiveDecisionName,
                    BindingFlags.Instance | BindingFlags.NonPublic);

                var decision = field.GetValue (brain) as Decision;
                DaniRuntimeBridge.SelectDecision (decision, brain);

                EditorApplication.update -= HandleUpdateEvent;
            }
        }
    }
}