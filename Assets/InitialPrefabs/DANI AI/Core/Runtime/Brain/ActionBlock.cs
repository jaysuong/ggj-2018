namespace InitialPrefabs.DANI {
    internal class ActionBlock {
        public bool IsCurrentlyRunning { get; private set; }

        private Action[] actions;
        private Decision decision;
        private int id;

        public ActionBlock (Action[] actions, Decision decision) {
            this.actions = actions;
            this.decision = decision;
            id = 0;
        }

        public void Reset () {
            id = 0;
            IsCurrentlyRunning = false;

            for (var i = 0; i < actions.Length; ++i) {
                actions[i].ResetState ();
            }
        }

        public void Run () {

            switch (decision.CurrentRunType) {
                case DecisionRunType.Sequential:
                    RunSequentially ();
                    break;

                case DecisionRunType.Concurrent:
                    RunConcurrently ();
                    break;

                case DecisionRunType.Random:
                    var status = actions[id].OnActionUpdate ();
                    if (status != ActionState.Running) {
                        actions[id].EndAction (status);
                        IsCurrentlyRunning = false;
                    }
                    break;
            }
        }

        public void Start (FastRandom rng) {
            if (actions.Length < 1) {
                IsCurrentlyRunning = false;
                return;
            }

            IsCurrentlyRunning = true;

            switch (decision.CurrentRunType) {
                case DecisionRunType.Sequential:
                    id = 0;
                    actions[id].StartAction ();
                    break;

                case DecisionRunType.Random:
                    id = rng.Next (actions.Length);
                    actions[id].StartAction ();
                    break;

                case DecisionRunType.Concurrent:
                    for (var i = 0; i < actions.Length; ++i) {
                        actions[i].StartAction ();
                    }
                    break;
            }
        }

        public void Stop () {
            IsCurrentlyRunning = false;

            if (decision.CurrentRunType == DecisionRunType.Concurrent) {
                for (var i = 0; i < actions.Length; ++i) {
                    actions[i].EndAction (actions[i].CurrentState);
                }
            } else {
                actions[id].EndAction (actions[id].CurrentState);
            }
        }

        private void RunSequentially () {
            var status = actions[id].OnActionUpdate ();

            switch (status) {
                case ActionState.Success:
                    actions[id].EndAction (status);
                    id++;
                    if (id < actions.Length) {
                        actions[id].StartAction ();
                    } else {
                        IsCurrentlyRunning = false;
                    }
                    break;

                case ActionState.Fail:
                    actions[id].EndAction (status);
                    IsCurrentlyRunning = false;
                    break;

                default:
                    break;
            }
        }

        private void RunConcurrently () {
            for (var i = 0; i < actions.Length; ++i) {
                var action = actions[i];

                if (action.CurrentState == ActionState.Running) {
                    var status = action.OnActionUpdate ();

                    switch (status) {
                        case ActionState.Success:
                            action.EndAction (status);
                            break;

                        case ActionState.Fail:
                            action.EndAction (status);
                            IsCurrentlyRunning = false;
                            break;
                    }
                }
            }
        }
    }
}