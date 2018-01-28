using System.Collections.Generic;
using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// Enum describing how the decision should run its actions
    /// </summary>
    public enum DecisionRunType {
        /// <summary>
        /// All actions are placed in the order that they are presented in the decision
        /// </summary>
        Sequential,
        /// <summary>
        /// One action will be randomly selected to run
        /// </summary>
        Random,
        /// <summary>
        /// All actions are run at the same time
        /// </summary>
        Concurrent
    }

    /// <summary>
    /// Enum describing the current decision status
    /// </summary>
    internal enum DecisionState { Idle, Running, Done }

    /// <summary>
    /// The "thoughts" of the agent. Generates a score based on the connected conditions for the
    /// brain to consider which Decision to run. Additionally selects which actions to run when 
    /// the Decision is selected by the AIBrain.
    /// </summary>
    public abstract class Decision : AINode {
        [SerializeField, Tooltip ("Can Dani switch out of this decision while it's still running?")]
        private bool m_isInterruptable = true;
        [SerializeField, Tooltip ("The maximum possible score this decision can have when Dani picks a new decision")]
        private float m_totalScore = 1;

        [SerializeField, Tooltip ("How the brain will run the connection actions.\nSequential: Runs all actions one by one\nRandom: Picks one action randomly\nConcurrent: Runs all actions at the same time")]
        protected DecisionRunType m_runType;

        [Header ("When the decision is selected:")]
        [SerializeField, Tooltip ("When the decision is selected, should it temporarily boost its score to 'focus'?  This prevents the brain from switching decisions rapidly should two decisions generate the same score.")]
        protected bool m_focusWhenSelected = false;
        [SerializeField, Tooltip ("The bonus score that this decision recieves when it is selected by the brain")]
        protected float m_scoreBoostOnFocus = 1f;

        /// <summary>
        /// Indicates how the decision should run the connected actions.
        /// </summary>
        public DecisionRunType CurrentRunType { get { return m_runType; } set { m_runType = value; } }

        /// <summary>
        /// The decision's total score, used to determine how likely Dani will select this decision
        /// </summary>
        public float TotalScore { get { return m_totalScore; } set { m_totalScore = value; } }

        /// <summary>
        /// Can the AIBrain stop this Decision in place of another Decision with a higher score?
        /// </summary>
        public bool IsInterruptable { get { return m_isInterruptable; } set { m_isInterruptable = value; } }

        /// <summary>
        /// When set to true, artificially boosts the current Decision's score to prevent other decisions of
        /// similar scores from overriding the current Decision.
        /// </summary>
        public bool FocusWhenSelected { get { return m_focusWhenSelected; } set { m_focusWhenSelected = value; } }

        /// <summary>
        /// The bonus score that the decision recieves when it is selected by the AIBrain.  This value is only 
        /// applied when `FocusWhenSelected` is set to true.
        /// </summary>
        public float ScoreBoostOnFocus { get { return m_scoreBoostOnFocus; } set { m_scoreBoostOnFocus = value; } }
    }
}