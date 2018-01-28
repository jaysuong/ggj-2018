namespace InitialPrefabs.DANI {
    /// <summary>
    /// Decribes when the brain should run its AI steps
    /// </summary>
    public enum ExecutionType {
        /// <summary>
        /// The AIBrain will run during the Update() step
        /// </summary>
        OnUpdate,

        /// <summary>
        /// The AIBrain will run during the FixedUpdate() step
        /// </summary>
        OnFixedUpdate,

        /// <summary>
        /// The AIBrain will not run automatically.  Use this if you want to control when the
        /// brain should run its steps e.g. for a turn-based AI.
        /// </summary>
        ByScript
    }
}