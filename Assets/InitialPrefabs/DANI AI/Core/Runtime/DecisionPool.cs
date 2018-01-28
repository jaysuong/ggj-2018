using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// A simple pool containing a set of decisions of the same score
    /// </summary>
    internal struct DecisionPool {
        private Decision[] pool;
        private int endIndex;
        private FastRandom rng;

        public DecisionPool(int poolSize) {
            pool = new Decision[poolSize];
            endIndex = 0;
            rng = new FastRandom(pool.GetHashCode());
        }

        public void AddDecision(Decision decision) {
            pool[endIndex++] = decision;
        }

        public Decision GetRandomDecision() {
            return pool[rng.Next(0, endIndex)];
        }

        public void Reset() {
            endIndex = 0;
        }        
    }
}
