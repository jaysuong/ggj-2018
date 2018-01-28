using System;
using System.Collections.Generic;
using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// A container of AINodes that dictate how the agent should observer, decide, and enact.
    /// </summary>
    public class AITemplate : ScriptableObject {
        [SerializeField, HideInInspector]
        private string guid;

        [SerializeField, TextArea]
        private string m_comments;

        [SerializeField, HideInInspector]
        private Observer[] m_observers;

        [SerializeField, HideInInspector]
        private Decision[] m_decisions;

        [SerializeField, HideInInspector]
        private Action[] m_actions;

        [SerializeField, HideInInspector]
        private Condition[] m_conditions;
        [SerializeField, HideInInspector]
        private Connection[] m_connections;

        [SerializeField, HideInInspector]
        private Variable[] m_variables;

        /// <summary>
        /// The ID of the template as a 32-character guid.
        /// </summary>
        public string Id { get { return guid; } }

        /// <summary>
        /// Any notes and decriptions of the AITemplate. Useful for reminders on what the template does.
        /// </summary>
        public string Comments { get { return m_comments; } set { m_comments = value; } }

        /// <summary>
        /// A collection of observer nodes required to run the template.
        /// </summary>
        public Observer[] Observers { get { return m_observers; } }

        /// <summary>
        /// A collection of decisions required to run the template.
        /// </summary>
        public Decision[] Decisions { get { return m_decisions; } }

        /// <summary>
        /// A collection of actions required to run the template.
        /// </summary>
        /// <returns></returns>
        public Action[] Actions { get { return m_actions; } }

        /// <summary>
        /// A collection of conditions required to run the template.
        /// </summary>
        /// <returns></returns>
        public Condition[] Conditions { get { return m_conditions; } }

        /// <summary>
        /// A collection of variables required to run the template.
        /// </summary>
        /// <returns></returns>
        public Variable[] Variables { get { return m_variables; } }

        /// <summary>
        /// A collection of connections required to run the template.
        /// </summary>
        /// <returns></returns>
        public Connection[] Connections { get { return m_connections; } }

        public AITemplate () {
            guid = Guid.NewGuid ().ToString ();
            m_observers = new Observer[0];
            m_decisions = new Decision[0];
            m_actions = new Action[0];
            m_conditions = new Condition[0];
            m_variables = new Variable[0];
            m_connections = new Connection[0];
        }

        private void OnDestroy () {
            foreach (var obs in m_observers) {
                if (obs != null) {
                    Destroy (obs);
                }
            }

            foreach (var dec in m_decisions) {
                if (dec != null) {
                    Destroy (dec);
                }
            }

            foreach (var act in m_actions) {
                if (act != null) {
                    Destroy (act);
                }
            }

            foreach (var con in m_conditions) {
                if (con != null) {
                    Destroy (con);
                }
            }

            foreach (var variable in m_variables) {
                if (variable != null) {
                    Destroy (variable);
                }
            }

            foreach (var connection in m_connections) {
                if (connection != null) {
                    Destroy (connection);
                }
            }
        }

        /// <summary>
        /// Finds an action by name.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <returns>Null if none are found, otherwise it will return an action of type T.</returns>
        public T GetAction<T> (string name) where T : Action {
            for (var i = 0; i < m_actions.Length; ++i) {
                if (m_actions[i].name == name) {
                    return m_actions[i] as T;
                }
            }

            return default (T);
        }

        /// <summary>
        /// Finds a decision by name.
        /// </summary>
        /// <param name="name">The name of the decision.</param>
        /// <returns>Null if none are found, otherwise it will return an decision of type T.</returns>
        public T GetDecision<T> (string name) where T : Decision {
            for (var i = 0; i < m_decisions.Length; ++i) {
                if (m_decisions[i].name == name) {
                    return m_decisions[i] as T;
                }
            }

            return default (T);
        }

        /// <summary>
        /// Finds an observer by name.
        /// </summary>
        /// <param name="name">The name of the observer.</param>
        /// <returnsNull if none are found, otherwise it will return an observer of type T.</returns>
        public T GetObserver<T> (string name) where T : Observer {
            for (var i = 0; i < m_observers.Length; ++i) {
                if (m_observers[i].name == name) {
                    return m_observers[i] as T;
                }
            }

            return default (T);
        }

        /// <summary>
        /// Finds a variable by name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <typeparam name="T">The type of variable to look for</typeparam>
        /// <returns>Null if none are found, otherwise it will return an variable of type T</returns>
        public T GetVariable<T> (string name) where T : Variable {
            for (var i = 0; i < m_variables.Length; ++i) {
                if (m_variables[i].name == name) {
                    return m_variables[i] as T;
                }
            }

            return default (T);
        }

        /// <summary>
        /// Creates a copy of the current template.
        /// </summary>
        /// <returns>A deep copy of the template.</returns>
        internal AITemplate CreateCopy () {
            var clone = Instantiate (this);
            clone.guid = guid;

            clone.m_observers = new Observer[m_observers.Length];
            for (var i = 0; i < m_observers.Length; ++i) {
                var obs = Instantiate (m_observers[i]);
                obs.name = m_observers[i].name;
                clone.m_observers[i] = obs;
            }

            clone.m_decisions = new Decision[m_decisions.Length];
            for (var i = 0; i < m_decisions.Length; ++i) {
                var dec = Instantiate (m_decisions[i]);
                dec.name = m_decisions[i].name;
                clone.m_decisions[i] = dec;
            }

            clone.m_actions = new Action[m_actions.Length];
            for (var i = 0; i < m_actions.Length; ++i) {
                var act = Instantiate (m_actions[i]);
                act.name = m_actions[i].name;
                clone.m_actions[i] = act;
            }

            clone.m_conditions = new Condition[m_conditions.Length];
            for (var i = 0; i < m_conditions.Length; ++i) {
                clone.m_conditions[i] = Instantiate (m_conditions[i]);
            }

            clone.m_variables = new Variable[m_variables.Length];
            for (var i = 0; i < m_variables.Length; ++i) {
                var variable = Instantiate (m_variables[i]);
                variable.name = m_variables[i].name;
                clone.m_variables[i] = variable;
            }

            clone.m_connections = new Connection[m_connections.Length];
            for (var i = 0; i < m_connections.Length; ++i) {
                var connection = Instantiate (m_connections[i]);
                connection.name = m_connections[i].name;
                clone.m_connections[i] = connection;
            }

            return clone;
        }
    }
}