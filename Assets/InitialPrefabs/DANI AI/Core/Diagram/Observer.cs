using System;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// The senses of the AI.  Outputs the information for the AI
    /// to reference with.
    /// </summary>
    public abstract class Observer : AINode {
        /// <summary>
        /// The output value from the observation.  
        /// </summary>
        public abstract object Output { get; }

        /// <summary>
        /// The current type of the output
        /// </summary>
        public abstract Type OutputType { get; }

        /// <summary>
        /// Updates the observer's values
        /// </summary>
        internal abstract void UpdateObserver();
    }
}
