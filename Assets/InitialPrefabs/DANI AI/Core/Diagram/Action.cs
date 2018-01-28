namespace InitialPrefabs.DANI {
    /// <summary>
    /// A customizable task that can agent can perform
    /// </summary>
    public abstract class Action : AINode {
        /// <summary>
        /// The current status of the action
        /// </summary>
        public ActionState CurrentState { get { return m_currentState; } }

        private ActionState m_currentState;

        /// <summary>
        /// Overridable method that is called when the action begins
        /// </summary>
        public virtual void OnActionStart() { }

        /// <summary>
        /// Overridable method that is called when the action is currently running.
        /// Actions will run continously until this method returns ActionState.Success 
        /// or ActionState.Fail
        /// </summary>
        /// <returns>The current status of the action</returns>
        public virtual ActionState OnActionUpdate() {
            return ActionState.Running;
        }

        /// <summary>
        /// Overridable method that is called when the action completes
        /// </summary>
        /// <param name="state">The curent state of the action.</param>
        public virtual void OnActionEnd(ActionState state) { }

        /// <summary>
        /// Starts the action and updates its status
        /// </summary>
        internal void StartAction() {
            m_currentState = ActionState.Running;
            OnActionStart();
        }

        /// <summary>
        /// Runs the action update step and updates its status
        /// </summary>
        /// <returns></returns>
        internal ActionState UpdateAction() {
            m_currentState = OnActionUpdate();
            return m_currentState;
        }

        /// <summary>
        /// Ends the action and updates its status
        /// </summary>
        internal void EndAction(ActionState state) {
            m_currentState = state;
            OnActionEnd(state);
        }

        /// <summary>
        /// Resets the action to the Pending state (for editor purposes)
        /// </summary>
        internal void ResetState() {
            m_currentState = ActionState.Pending;
        }
    }
}