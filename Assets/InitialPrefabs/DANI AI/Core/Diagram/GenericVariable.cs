using System;
using System.Collections;
using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// A variable that contains a generic type
    /// </summary>
    public abstract class GenericVariable<T> : Variable {
        [SerializeField]
        protected T value;

        /// <summary>
        /// The variable's value
        /// </summary>
        public T Value {
            get { return value; }
            set { this.value = value; }
        }

        /// <summary>
        /// The variable's type
        /// </summary>
        public override Type ValueType { get { return value.GetType (); } }

        public override object GetValue () {
            return value;
        }

        /// <summary>
        /// Outputs a formatted value of the variable
        /// </summary>
        public override string ToString () {
            return value != null ? value.ToString () : "(null)";
        }
    }
}