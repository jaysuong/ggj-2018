namespace InitialPrefabs.DANI {
    /// <summary>
    /// Decribes the brain's ability to run its AI steps.
    /// </summary>
    public enum RunningState {
        /// <summary>
        /// The initial state of the brain. No AI steps are running at this point in time.
        /// </summary>
        NotInitialized,
        /// <summary>
        /// The normal running state of the brain. OnResume is called if the previous state was paused.
        /// </summary>
        Running,
        /// <summary>
        /// The brain has paused. OnPause is called once the brain enters this state.
        /// </summary>
        Paused,
        /// <summary>
        /// The brain has ceased functioning.
        /// </summary>
        Stopped
    }
}