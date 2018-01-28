using System;
using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// A value-storing object that is local to the template.  They are like Parameters
    /// in the Animator.
    /// </summary>
    public abstract class Variable : ScriptableObject {
        /// <summary>
        /// The variable's type
        /// </summary>
        public abstract Type ValueType { get; }

        /// <summary>
        /// Gets the variable's current value.
        /// </summary>
        public abstract object GetValue ();
    }
}