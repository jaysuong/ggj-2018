namespace InitialPrefabs.DANI {
    /// <summary>
    /// A generic condition for ints
    /// </summary>
    public class IntCondition : GenericCondition<int> {
        public IntCompareType comparison;

        public enum IntCompareType { GreaterThan, LessThan, Equals, NotEquals }


        public override float CalculateLocalWeight() {
            var valueToCompare = observer.GetOutput();
            bool result;

            switch(comparison) {
                case IntCompareType.GreaterThan:
                    result = valueToCompare > compareValue;
                    break;

                case IntCompareType.LessThan:
                    result = valueToCompare < compareValue;
                    break;

                case IntCompareType.Equals:
                    result = valueToCompare == compareValue;
                    break;

                case IntCompareType.NotEquals:
                    result = valueToCompare != compareValue;
                    break;

                default:
                    result = false;
                    break;
            }

            return result ? 1f : 0f;
        }
    }
}
