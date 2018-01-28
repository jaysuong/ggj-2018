namespace InitialPrefabs.DANIEditor {
    internal static class AINodeUtility {

        internal const string ObserverPropertyName = "m_observers";
        internal const string DecisionPropertyName = "m_decisions";
        internal const string ActionPropertyName = "m_actions";
        internal const string ConnectionPropertyName = "m_connections";
        internal const string ConditionPropertyName = "m_conditions";

        internal const string PositionPropertyName = "m_modulePosition";
        internal const string IdPropertyName = "m_Id";

        internal const string SourceIdPropertyName = "m_sourceId";
        internal const string TargetIdPropertyName = "m_targetId";
        internal const string ConditionIdPropertyName = "m_conditionId";

        /// <summary>
        /// Attempts to convert a string id into an int id
        /// </summary>
        public static int ConvertToIntId (string id) {
            int result;

            if (int.TryParse (id, out result)) {
                return result;
            }

            return int.MinValue;
        }
    }
}