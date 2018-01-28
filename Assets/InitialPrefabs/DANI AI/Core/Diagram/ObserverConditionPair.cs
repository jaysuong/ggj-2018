using UnityEngine;
using System.Collections;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// A combination of a ConditionModule and an Observer.  Used to describe
    /// whether or not an observer is valid in the current situation
    /// </summary>
    [System.Obsolete]
    public struct ObserverConditionPair {
        public readonly Observer observer;
        public readonly Condition condition;

        public ObserverConditionPair(Observer observer, Condition condition) {
            this.observer = observer;
            this.condition = condition;
            condition.CacheObserver(observer);
        }

        public override bool Equals(object obj) {
            if(!(obj is ObserverConditionPair))
                return false;

            var other = (ObserverConditionPair)obj;

            return observer == other.observer && condition == other.condition;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
