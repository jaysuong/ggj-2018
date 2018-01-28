using System;
using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// Defines a threshold which must be met for decisions to be selected.
    /// to the current decision
    /// </summary>
    public abstract class Condition : ScriptableObject {
        /// <summary>
        /// Notes and comments on the current module
        /// </summary>
        [SerializeField, TextArea, Tooltip ("Any notes and description about the node goes here")]
        private string m_comments;

        [SerializeField, HideInInspector]
        private int m_Id;

        /// <summary>
        /// The condition's unique id
        /// </summary>
        public int Id { get { return m_Id; } }

        /// <summary>
        /// Notes and descriptions related to the condition
        /// </summary>
        public string Comments { get { return m_comments; } set { m_comments = value; } }

        /// <summary>
        /// The value that the condition uses to generate a weight
        /// </summary>
        public abstract object CompareValue { get; set; }

        internal abstract void CacheObserver (Observer module);

        /// <summary>
        /// Overridable method that translates the observer's output into a weight
        /// usable by the Decision. Weights should be in the range [0, 1], where 1
        /// is `true` and 0 is `false`
        /// </summary>
        /// <returns>An evaluated value of the observer's output</returns>
        public abstract float CalculateLocalWeight ();
    }
}