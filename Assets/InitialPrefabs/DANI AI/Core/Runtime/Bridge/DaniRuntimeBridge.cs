using InitialPrefabs.DANI;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// The runtime bridge between the editor and runtime.  Allows the ability to display and edit
    /// runtime diagram in realtime.
    /// </summary>
    public class DaniRuntimeBridge {
        /// <summary>
        /// The brain as selected by clicking on an AIBrain in the hierarchy
        /// </summary>
        public static AIBrain SelectedBrain { get; private set; }

        /// <summary>
        /// The selected decision as decided by the AIBrain
        /// </summary>
        public static Decision SelectedDecision { get; private set; }

        /// <summary>
        /// The selected AITemplate decided by the AIBrain
        /// </summary>
        public static AITemplate SelectedTemplate { get; private set; }

        /// <summary>
        /// An event that is called when a brain with a working runtime AITemplate is selected
        /// </summary>
        public static OnTemplateSelectDelegate OnTemplateSelectEvent;

        /// <summary>
        /// Selects the brain to display in the editor
        /// </summary>
        public static void SelectBrain (AIBrain brain) {
            SelectedBrain = brain;
            SelectedTemplate = brain.RunningStatus != RunningState.NotInitialized ? 
                brain.RuntimeTemplate : brain.Template;

            if (OnTemplateSelectEvent != null) {
                OnTemplateSelectEvent (SelectedTemplate);
            }
        }

        /// <summary>
        /// Selects the decision to be displayed in the editor
        /// </summary>
        public static void SelectDecision (Decision decision, AIBrain brain) {
            if (brain == SelectedBrain) {
                SelectedDecision = decision;
            }
        }
    }
}