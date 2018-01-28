namespace InitialPrefabs.DANI {
    /// <summary>
    /// Describes the current running state of an action.
    /// Running = The action is still running
    /// Success = The action is done, and was successful
    /// Fail = The action is done, but failed
    /// </summary>
    public enum ActionState {
        /// <summary>
        /// The Action is waiting to be run
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The Action is currently running
        /// </summary>
        Running = 1,

        /// <summary>
        /// The Action ran successfully
        /// </summary>
        Success = 2,

        /// <summary>
        /// The Action has failed
        /// </summary>
        Fail = 3
    }
}