namespace InitialPrefabs.DANI {
    /// <summary>
    /// An enum describing the a boolean
    /// </summary>
    public enum BoolType { True, False }

    /// <summary>
    /// A generic comparison for booleans
    /// </summary>
    [HideCompareValue]
    public class BoolCondition : GenericCondition<bool> {
        public BoolType comparison;

        public override float CalculateLocalWeight () {
            var value = observer.GetOutput ();

            return (comparison == BoolType.True && value) || (comparison == BoolType.False && !value) ?
                1f : 0f;
        }
    }
}