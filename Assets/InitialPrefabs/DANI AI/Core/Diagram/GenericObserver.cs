using System;
using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// A generic version of an observer.  Handles any serializable type, like floats, ints, string, etc.
    /// </summary>
    /// <typeparam name="T">The type of observer</typeparam>
    public class GenericObserver<T> : Observer {
        [SerializeField]
        protected T output;

        public override object Output { get { return output; } }
        public sealed override Type OutputType { get { return typeof (T); } }

        /// <summary>
        /// The Observer's output value
        /// </summary>
        public T GetOutput () { return output; }

        /// <summary>
        /// Sets the observer's output value
        /// </summary>
        /// <param name="value">The value to set to</param>
        public void SetOutput (T value) {
            output = value;
        }

        /// <summary>
        /// Updates the observer's value and returns an output value as a result
        /// </summary>
        public virtual T OnObserverUpdate () {
            return output;
        }

        internal override void UpdateObserver () {
            output = OnObserverUpdate ();
        }
    }
}