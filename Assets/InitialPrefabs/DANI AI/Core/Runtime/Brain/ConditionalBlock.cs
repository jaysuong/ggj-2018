namespace InitialPrefabs.DANI {
    /// <summary>
    /// A small container of a condition and its connection. Used to calculate subscores
    /// </summary>
    internal struct ConditionalBlock {
        public Condition condition;
        public Connection connection;

        public float SubScore { get { return condition.CalculateLocalWeight () * connection.Weight; } }
    }
}