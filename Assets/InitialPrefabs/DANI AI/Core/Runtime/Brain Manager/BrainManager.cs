using System.Collections.Generic;
using UnityEngine;

namespace InitialPrefabs.DANI {
    using Exception = System.Exception;

    /// <summary>
    /// A manager that runs all brains in a batch to improve performance.
    /// </summary>
    public class BrainManager : MonoBehaviour {
        internal static BrainManager Instance {
            get {
                lock (lockObject) {
                    if (instance == null && !isShuttingDown) {
                        instance = FindObjectOfType<BrainManager> ();
                        if (instance == null) {
                            var go = new GameObject ("~Brain Manager");
                            instance = go.AddComponent<BrainManager> ();
                        }
                    }

                    return instance;
                }
            }
        }

        private static BrainManager instance;
        private static object lockObject = new object ();
        private static bool isShuttingDown;

        private List<AIBrain> updateList;
        private List<AIBrain> fixedUpdateList;

        private int DefaultListSize = 100;

        private void Awake () {
            updateList = new List<AIBrain> (DefaultListSize);
            fixedUpdateList = new List<AIBrain> (DefaultListSize);
        }

        private void Start () {
            if (instance == this) {
                DontDestroyOnLoad (gameObject);
            } else {
                Destroy (gameObject);
            }
        }

        private void OnDestroy () {
            if (instance == this) {
                instance = null;
                isShuttingDown = true;
            }
        }

        private void OnApplicationQuit () {
            isShuttingDown = true;
        }

        private void Update () {
            for (var i = 0; i < updateList.Count; ++i) {
                try {
                    updateList[i].PlayAIStep ();
                } catch (Exception e) {
                    Debug.LogError (e);
                }
            }
        }

        private void FixedUpdate () {
            for (var i = 0; i < fixedUpdateList.Count; ++i) {
                try {
                    fixedUpdateList[i].PlayAIStep ();
                } catch (Exception e) {
                    Debug.LogError (e);
                }
            }
        }

        /// <summary>
        /// Registers the brain to run on a specific batch list
        /// </summary>
        /// <param name="brain">The brain to register</param>
        /// <param name="unregister">Should the manager attempt to unregister first?</param>
        internal void RegisterBrain (AIBrain brain, bool unregister = true) {
            if (unregister) {
                UnregisterBrain (brain);
            }

            switch (brain.ExecutionOrder) {
                case ExecutionType.OnUpdate:
                    updateList.Add (brain);
                    break;

                case ExecutionType.OnFixedUpdate:
                    fixedUpdateList.Add (brain);
                    break;
            }
        }

        /// <summary>
        /// Unregisters the brain from running on batch lists
        /// </summary>
        /// <param name="brain">The brain to remove</param>
        internal void UnregisterBrain (AIBrain brain) {
            updateList.Remove (brain);
            fixedUpdateList.Remove (brain);
        }
    }
}