using System;
using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// A generic representation of a condition.  Used to create conditions for specific
    /// data types e.g. floats
    /// </summary>
    /// <typeparam name="TType">The type of the condition</typeparam>
    public abstract class GenericCondition<TType> : Condition {
        [SerializeField, Tooltip ("The value to compare the observer's output with")]
        protected TType compareValue;

        [NonSerialized]
        protected GenericObserver<TType> observer;

        internal sealed override void CacheObserver (Observer module) {
            observer = module as GenericObserver<TType>;
        }

        public override object CompareValue {
            get { return compareValue; }
            set { compareValue = (TType) value; }
        }
    }
}