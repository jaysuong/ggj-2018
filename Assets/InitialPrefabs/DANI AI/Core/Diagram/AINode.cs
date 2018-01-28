using System;
using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// The base node class to reference all components in the AITemplate
    /// </summary>
    public abstract class AINode : ScriptableObject {
        [SerializeField, TextArea, Tooltip ("Any notes and description about the node goes here")]
        private string m_comments;

        [SerializeField, Tooltip ("If enabled, Dani will run this node normally.  Otherwise, it will be ignored.  Useful for debugging.")]
        private bool m_isEnabled = true;

        [SerializeField, HideInInspector]
        private int m_Id;

        [SerializeField, HideInInspector]
        private Vector2 m_modulePosition;

        /// <summary>
        /// The id of the node
        /// </summary>
        public int Id { get { return m_Id; } }

        /// <summary>
        /// Determines whether or not the node is active.  The brain will not run  
        /// this node if it is disabled.
        /// </summary>
        public bool IsEnabled {
            get { return m_isEnabled; }
            set { m_isEnabled = value; }
        }

        /// <summary>
        /// Custom notes and descriptions that describe this node.  Useful for adding 
        /// reminders on what the node does.
        /// </summary>
        public string Comments {
            get { return m_comments; }
            set { m_comments = value; }
        }

        /// <summary>
        /// The game object that the AIBrain is attached to.  This value is initialized
        /// when the node is a local copy in an AIBrain.
        /// </summary>
        /// <returns></returns>
        public GameObject GameObject { get; private set; }

        /// <summary>
        /// The transform component of the brain.  This value is initialized when 
        /// the node is a local copy in an AIBrain.
        /// </summary>
        public Transform Transform { get; private set; }

        /// <summary>
        /// The template that this node belongs to
        /// </summary>
        public AITemplate Template { get; private set; }

        /// <summary>
        /// The brain that this node is running in.  This value is initialized when 
        /// the node is a local copy in an AIBrain.
        /// </summary>
        public AIBrain Brain { get; private set; }

        /// <summary>
        /// Simple generic setup that mimics MonoBehaviours
        /// </summary>
        internal virtual void Setup (AITemplate template, AIBrain brain, GameObject gameObject, Transform transform) {
            Template = template;
            Brain = brain;
            GameObject = gameObject;
            Transform = transform;
        }

        /// <summary>
        /// Overridable method to initialize tasks.  Called during AIBrain's Start() method
        /// </summary>
        public virtual void OnStart () { }

        /// <summary>
        /// Overridable method that is called when the brain is paused
        /// </summary>
        public virtual void OnPause () { }

        /// <summary>
        /// Overridable method that is called when the brain resumes
        /// </summary>
        public virtual void OnResume () { }

        /// <summary>
        /// Overridable method that is called when the brain is destroyed or 
        /// is restarted
        /// </summary>
        public virtual void OnDestroy () { }

        #region Components

        /// <summary>
        /// Gets the first instance of a component on the game object.  
        /// Shortcut for gameObject.GetComponent &lt; T &gt; ()
        /// </summary>
        /// <typeparam name="T">The type of the component to search</typeparam>
        /// <returns>The found component.  Null otherwise</returns>
        public T GetComponent<T> () {
            return (GameObject != null) ? GameObject.GetComponent<T> () : default (T);
        }

        /// <summary>
        /// Gets the first instance of a component on the game object and all of its 
        /// children.  Shortcut for gameObject.GetComponentInChildren &lt; T &gt; ()
        /// </summary>
        /// <typeparam name="T">The type of the component to search</typeparam>
        /// <returns>The found component.  Null otherwise</returns>
        public T GetComponentInChildren<T> () {
            return (GameObject != null) ? GameObject.GetComponentInChildren<T> () : default (T);
        }

        /// <summary>
        /// Gets the first instance of a component on the game object and all of its parents.
        /// Shortcut for gameObject.GetComponentInParent &lt; T &gt; ()
        /// </summary>
        /// <typeparam name="T">The type of the component to search</typeparam>
        /// <returns>The found component.  Null otherwise</returns>
        public T GetComponentInParent<T> () {
            return (GameObject != null) ? GameObject.GetComponentInParent<T> () : default (T);
        }

        /// <summary>
        /// Gets all components of the given type on the game object.  Shortcut for 
        /// gameObject.GetComponents &lt; T &gt; ()
        /// </summary>
        /// <typeparam name="T">The type of the components to search</typeparam>
        /// <returns>An array of found components</returns>
        public T[] GetComponents<T> () {
            return (GameObject != null) ? GameObject.GetComponents<T> () : new T[0];
        }

        /// <summary>
        /// Gets all components of the given type on the game object and all of its children.
        /// Shortcut for gameObject.GetComponentsInChildren &lt; T &gt; ()
        /// </summary>
        /// <typeparam name="T">The type of the components to search</typeparam>
        /// <returns>An array of found components</returns>
        public T[] GetComponentsInChildren<T> () {
            return (GameObject != null) ? GameObject.GetComponentsInChildren<T> () : new T[0];
        }

        /// <summary>
        /// Gets all the components of the given type on the game object and all of its parents.
        /// Shortcut for gameObject.GetComponentsInParent &lt; T &gt; ()
        /// </summary>
        /// <typeparam name="T">The type of the components to search</typeparam>
        /// <returns>An array of found components</returns>
        public T[] GetComponentsInParent<T> () {
            return (GameObject != null) ? GameObject.GetComponentsInParent<T> () : new T[0];
        }

        #endregion

    }
}