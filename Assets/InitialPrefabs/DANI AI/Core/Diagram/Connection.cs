using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// Enum describing the type of connection involved in the link between two nodes
    /// </summary>
    public enum ConnectionType {
        /// <summary>
        /// The connection is a simple link between two AI nodes e.g. Decision to Action
        /// </summary>
        Simple,
        /// <summary>
        /// The connection contains a condition between two AI nodes e.g. Observer to Decision
        /// </summary>
        Conditional
    }

    /// <summary>
    /// A node that represents a relation between two AINodes, depicted by a bezier curve in 
    /// the editor.
    /// </summary>
    public class Connection : ScriptableObject {
        [SerializeField, HideInInspector]
        private ConnectionType connectionType;
        [SerializeField, HideInInspector]
        private int priority;
        [SerializeField, HideInInspector]
        private float weight;

        [SerializeField, HideInInspector]
        private int m_conditionId;
        [SerializeField, HideInInspector]
        private int m_sourceId;
        [SerializeField, HideInInspector]
        private int m_targetId;

        /// <summary>
        /// The type of connection.
        /// </summary>
        public ConnectionType ConnectionType { get { return connectionType; } }

        /// <summary>
        /// The priority of the connection. When there are multiple Actions
        /// connected to a decision, this value determines the run order of
        /// those Actions.
        /// </summary>
        public int Priority { get { return priority; } set { priority = value; } }

        /// <summary>
        /// The template that this connection belongs to.
        /// </summary>
        public AITemplate Template { get; internal set; }

        /// <summary>
        /// The maximum weight of the connection, if this connection contains a condition.
        /// </summary>
        public float Weight { get { return weight; } }

        /// <summary>
        /// The id of the attached condition. This value is 0 if the connection type is simple.
        /// </summary>
        public int ConditionId { get { return m_conditionId; } }

        /// <summary>
        /// The id of the AI node on the left side of the connection.
        /// </summary>
        public int SourceId { get { return m_sourceId; } }
        
        /// <summary>
        /// The id of the AI node on the right side of the connection.
        /// </summary>
        public int TargetId { get { return m_targetId; } }
    }
}