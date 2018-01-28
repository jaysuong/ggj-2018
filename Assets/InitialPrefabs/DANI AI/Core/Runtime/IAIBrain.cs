namespace InitialPrefabs.DANI {
    public interface IAIBrain {
        /// <summary>
        /// When the brain should run its AI Step (during Update, OnUpdate, etc.)
        /// </summary>
        ExecutionType ExecutionOrder { get; set; }

        /// <summary>
        /// The current running state of the brain.  The brain will only run when its status is
        /// set to running (via StartBrain())
        /// </summary>
        RunningState RunningStatus { get; }

        /// <summary>
        /// The runtime version of the template used to perform AI-related actions
        /// </summary>
        AITemplate RuntimeTemplate { get; }

        /// <summary>
        /// The template that this brain is using to perform actions
        /// </summary>
        AITemplate Template { get; }

        /// <summary>
        /// Pauses the brain and its AI nodes
        /// </summary>
        void Pause();

        /// <summary>
        /// Restarts the brain and runs all AI steps from scratch
        /// </summary>
        void RestartBrain();

        /// <summary>
        /// Attempts to resume the brain from its paused state
        /// </summary>
        void Resume();

        /// <summary>
        /// Runs the appropriate set of actions as defined by the active decision
        /// </summary>
        void RunActionStep();

        /// <summary>
        /// Plays all AI Steps (observation, decision, action) together in one go
        /// </summary>
        void RunAIStep();

        /// <summary>
        /// Updates the decision to the next active decision
        /// </summary>
        void RunDecisionStep();

        /// <summary>
        /// Updates observervation values
        /// </summary>
        void RunObservationStep();

        /// <summary>
        /// Starts the brain by picking a decision and running the observer step.
        /// </summary>
        void StartBrain();

        /// <summary>
        /// Stops the brain from running all functions
        /// </summary>
        void StopBrain();
    }
}