using System;
using System.Collections.Generic;
using UnityEngine;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// The main runtime component of Dani.  Creates and runs a copy of an `AITemplate`
    /// instance.
    /// </summary>
    [AddComponentMenu ("")]
    public class AIBrain : MonoBehaviour, IAIBrain, IBrainRunnable {
        [SerializeField, Tooltip ("The template that this brain will use to run")]
        private AITemplate m_template;

        [SerializeField, Tooltip ("When should Dani run its AI step?")]
        private ExecutionType m_executionOrder = ExecutionType.OnUpdate;

        /// <summary>
        /// The local copy of the template used to perform AI-related actions
        /// </summary>
        public AITemplate RuntimeTemplate { get; private set; }

        /// <summary>
        /// The template that this brain is using to perform AI-related actions
        /// </summary>
        public AITemplate Template { get { return m_template; } }

        /// <summary>
        /// When the brain should run its AI Step (during Update, OnUpdate, etc.)
        /// </summary>
        public ExecutionType ExecutionOrder { get { return m_executionOrder; } set { m_executionOrder = value; } }

        /// <summary>
        /// The current running state of the brain.  The brain will only run when its running status is
        /// set to RunningState.Running (via StartBrain())
        /// </summary>
        public RunningState RunningStatus { get; private set; }

        public event DrawGizmosHandler OnDrawGizmosEvent;
        public event DrawGizmosHandler OnDrawGizmosSelectedEvent;
        public event PauseHandler OnPauseEvent;
        public event ResumeHandler OnResumeEvent;

        public delegate void PauseHandler ();
        public delegate void ResumeHandler ();
        public delegate void DrawGizmosHandler ();

        private FastRandom rng;

        private Decision activeDecision;
        private ActionBlock activeActionBlock;

        private BrainManager manager;

        private Decision placeholderDecision;

        private Tuple<Decision, ConditionalBlock[]>[] weightData;
        private Tuple<int, float>[] decisionScoreMap;
        private Dictionary<int, Decision> decisionBank;
        private Dictionary<int, ActionBlock> actionBank;

        private void Awake () {
            RunningStatus = RunningState.NotInitialized;
            decisionBank = new Dictionary<int, Decision> ();
            actionBank = new Dictionary<int, ActionBlock> ();
            rng = new FastRandom ();

            placeholderDecision = ScriptableObject.CreateInstance<PlaceholderDecision> ();
            activeDecision = placeholderDecision;
            activeActionBlock = new ActionBlock (new Action[0], placeholderDecision);
        }

        private void Start () {
            manager = BrainManager.Instance;

            StartBrain ();
        }

        private void OnEnable () {
            Resume ();
        }

        private void OnDisable () {
            Pause ();
        }

        private void OnDestroy () {
            StopBrain ();

            if (RuntimeTemplate != null) {
                Destroy (RuntimeTemplate);
            }

            Destroy (placeholderDecision);
        }

        private void OnDrawGizmos () {
            if (RunningStatus == RunningState.Running) {
                if (OnDrawGizmosEvent != null) {
                    OnDrawGizmos ();
                }
            }
        }

        private void OnDrawGizmosSelected () {
            if (RunningStatus == RunningState.Running) {
                if (OnDrawGizmosSelectedEvent != null) {
                    OnDrawGizmosSelectedEvent ();
                }
            }
        }

        /// <summary>
        /// Gets the current score of a decision
        /// </summary>
        /// <param name="decision">The decision to query</param>
        /// <returns>0 if the brain does not recognize the decision</returns>
        public float GetCurrentDecisionScore (Decision decision) {
            for (var i = 0; i < decisionScoreMap.Length; ++i) {
                if (decisionScoreMap[i].Item1 == decision.Id) {
                    return decisionScoreMap[i].Item2;
                }
            }
            return 0f;
        }

        /// <summary>
        /// Pauses the brain and its AI nodes.
        /// </summary>
        public void Pause () {
            if (RunningStatus == RunningState.Running) {
                try {
                    if (OnPauseEvent != null) {
                        OnPauseEvent ();
                    }
                } catch (Exception e) {
                    Debug.LogErrorFormat (this, "A bug occured when pausing the brain:\n{0}", e);
                }

                manager.UnregisterBrain (this);
                RunningStatus = RunningState.Paused;
            }
        }

        /// <summary>
        /// Restarts the brain and runs all AI steps from scratch.
        /// </summary>
        public void RestartBrain () {
            StopBrain ();
            StartBrain ();
        }

        /// <summary>
        /// Attempts to resume the brain from its paused state.
        /// </summary>
        public void Resume () {
            if (RunningStatus == RunningState.Paused) {
                try {
                    if (OnResumeEvent != null) {
                        OnResumeEvent ();
                    }
                } catch (Exception e) {
                    Debug.LogErrorFormat (this, "A bug occured when resuming the brain:\n{0}", e);
                }

                manager.RegisterBrain (this);
                RunningStatus = RunningState.Running;
            }
        }

        /// <summary>
        /// Runs the appropriate set of actions as defined by the active decision.
        /// </summary>
        public void RunActionStep () {
            if (RunningStatus == RunningState.Running) {
                PlayActionStep ();
            }
        }

        /// <summary>
        /// Plays all AI Steps (observation, decision, action) together in one go.
        /// </summary>
        public void RunAIStep () {
            if (RunningStatus == RunningState.Running) {
                RunObservationStep ();
                RunDecisionStep ();
                RunActionStep ();
            }
        }

        /// <summary>
        /// Updates the decision to the next active decision.
        /// </summary>
        public void RunDecisionStep () {
            if (RunningStatus == RunningState.Running) {
                PlayDecisionStep ();
            }
        }

        /// <summary>
        /// Updates all enabled Observers' values.
        /// </summary>
        public void RunObservationStep () {
            if (RunningStatus == RunningState.Running) {
                PlayObservationStep ();
            }
        }

        /// <summary>
        /// Starts the brain by picking a decision and running the observer step.
        /// </summary>
        public void StartBrain () {
            if (m_template == null) {
                Debug.LogError ("Brain does not have an template!");
            }

            if (RuntimeTemplate != null) {
                Destroy (RuntimeTemplate);
            }

            RuntimeTemplate = m_template.CreateCopy ();
            RuntimeTemplate.name = string.Format ("{0} (Runtime in `{1}`)", m_template.name, name);

            // TODO: Run setup routine here
            PrepareTemplateForRuntime (RuntimeTemplate);

            manager.RegisterBrain (this);
            RunningStatus = RunningState.Running;
        }

        /// <summary>
        /// Stops the brain from running all functions
        /// </summary>
        public void StopBrain () {
            if (RunningStatus != RunningState.NotInitialized) {
                manager.UnregisterBrain (this);
                RunningStatus = RunningState.Stopped;
            }

        }

        internal void PlayAIStep () {
            PlayObservationStep ();
            PlayDecisionStep ();
            PlayActionStep ();
        }

        internal void PlayActionStep () {
            if (activeActionBlock.IsCurrentlyRunning) {
                activeActionBlock.Run ();
            }
        }

        internal void PlayDecisionStep () {
            var areActionsRunning = activeActionBlock.IsCurrentlyRunning;
            for (var i = 0; i < weightData.Length; i++) {
                var data = weightData[i];
                var decision = data.Item1;
                var total = 0f;
                var isFocused = decision.FocusWhenSelected && activeDecision == decision;

                for (var k = 0; k < data.Item2.Length; k++) {
                    var block = data.Item2[k];
                    total += block.SubScore;
                }

                total *= decision.IsEnabled ? decision.TotalScore : 0f;
                total += areActionsRunning && isFocused ?
                    decision.ScoreBoostOnFocus : 0f;
                decisionScoreMap[i] = new Tuple<int, float> (decision.Id, total);
            }

            // Sort the scores
            // TODO: Refactor quicksort
            Sort.QuickSort (ref decisionScoreMap, 0, decisionScoreMap.Length - 1);

            // Pick the best decision randomly
            var highestScore = decisionScoreMap[decisionScoreMap.Length - 1].Item2;
            var index = GetOuterDecisionIndex (highestScore);

            var candidateId = decisionScoreMap[rng.Next (index, decisionScoreMap.Length)].Item1;
            var candidateDecision = decisionBank[candidateId];

            if (activeDecision != candidateDecision) {
                if (activeDecision == null) {
                    activeDecision = candidateDecision;
                    activeActionBlock = actionBank[candidateId];

                    activeActionBlock.Reset ();
                    activeActionBlock.Start (rng);

                    DaniRuntimeBridge.SelectDecision (candidateDecision, this);
                } else if (!activeActionBlock.IsCurrentlyRunning || activeDecision.IsInterruptable) {
                    activeDecision = candidateDecision;

                    if (activeActionBlock.IsCurrentlyRunning) {
                        activeActionBlock.Stop ();
                    }

                    activeActionBlock = actionBank[candidateId];
                    activeActionBlock.Reset ();
                    activeActionBlock.Start (rng);

                    DaniRuntimeBridge.SelectDecision (candidateDecision, this);
                }
            }
        }

        internal void PlayObservationStep () {
            var observers = RuntimeTemplate.Observers;

            for (var i = 0; i < observers.Length; ++i) {
                if (observers[i].IsEnabled) {
                    try {
                        observers[i].UpdateObserver ();
                    } catch (Exception e) {
                        Debug.LogError (e);
                    }
                }
            }
        }

        private int GetOuterDecisionIndex (float highestScore) {
            var index = -1;

            for (var i = decisionScoreMap.Length - 1; i > -1; --i) {
                if (decisionScoreMap[i].Item2 < highestScore) {
                    index = i;
                    break;
                }
            }

            return index + 1;
        }

        private void PrepareBlocks (AITemplate template, Dictionary<int, ScriptableObject> nodeDictionary) {
            var decisions = template.Decisions;
            var connections = template.Connections;
            var blockWorkList = new List<ConditionalBlock> ();
            var actionWorkList = new List<Action> ();

            weightData = new Tuple<Decision, ConditionalBlock[]>[decisions.Length];

            for (var i = 0; i < decisions.Length; i++) {
                blockWorkList.Clear ();
                actionWorkList.Clear ();

                var decision = decisions[i];

                for (var k = 0; k < connections.Length; k++) {
                    var con = connections[k];

                    if (con.TargetId == decision.Id && con.ConnectionType == ConnectionType.Conditional) {
                        var observer = nodeDictionary[con.SourceId] as Observer;
                        var condition = nodeDictionary[con.ConditionId] as Condition;

                        condition.CacheObserver (observer);
                        blockWorkList.Add (new ConditionalBlock {
                            condition = condition,
                                connection = con
                        });
                    } else if (con.SourceId == decision.Id && con.ConnectionType == ConnectionType.Simple) {
                        var action = nodeDictionary[con.TargetId] as Action;
                        actionWorkList.Add (action);
                    }
                }

                var tuple = new Tuple<Decision, ConditionalBlock[]> (decision, blockWorkList.ToArray ());
                weightData[i] = tuple;

                var actionBlock = new ActionBlock (actionWorkList.ToArray (), decision);
                actionBank.Add (decision.Id, actionBlock);
            }
        }

        private void PrepareTemplateForRuntime (AITemplate template) {
            var observers = template.Observers;
            var decisions = template.Decisions;
            var actions = template.Actions;
            var connections = template.Connections;
            var conditions = template.Conditions;

            var gameObject = this.gameObject;
            var transform = this.transform;

            var nodeDictionary = new Dictionary<int, ScriptableObject> ();

            // TODO: Split the nodeDictionary into multiple dictionaries
            // TODO: Multithread the for loops
            // Setup all the initial values for the nodes
            for (var i = 0; i < observers.Length; ++i) {
                observers[i].Setup (template, this, gameObject, transform);
                nodeDictionary.Add (observers[i].Id, observers[i]);
            }

            for (var i = 0; i < decisions.Length; ++i) {
                decisions[i].Setup (template, this, gameObject, transform);
                decisionBank.Add (decisions[i].Id, decisions[i]);
            }

            for (var i = 0; i < actions.Length; ++i) {
                actions[i].Setup (template, this, gameObject, transform);
                nodeDictionary.Add (actions[i].Id, actions[i]);
            }

            for (var i = 0; i < conditions.Length; ++i) {
                nodeDictionary.Add (conditions[i].Id, conditions[i]);
            }

            for (var i = 0; i < connections.Length; ++i) {
                connections[i].Template = template;
            }

            decisionScoreMap = new Tuple<int, float>[decisions.Length];
            PrepareBlocks (template, nodeDictionary);

            foreach (var node in nodeDictionary) {
                if (node.Value is AINode) {
                    (node.Value as AINode).OnStart ();
                }
            }
        }
    }

}