namespace InitialPrefabs.DANI {
    internal interface IBrainRunnable {
        /// <summary>
        /// When the brain should run its AI Step (during Update, OnUpdate, etc.)
        /// </summary>
        ExecutionType ExecutionOrder { get; set; }

        /// <summary>
        /// Plays all AI Steps (observation, decision, action) together in one go
        /// </summary>
        void RunAIStep ();
    }
}