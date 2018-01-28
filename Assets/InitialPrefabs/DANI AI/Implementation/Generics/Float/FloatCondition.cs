namespace InitialPrefabs.DANI {
    /// <summary>
    /// A generic condition for floats
    /// </summary>
    public class FloatCondition : GenericCondition<float> {
        public FloatCompareType comparison;

        public enum FloatCompareType { GreaterThan, LessThan, Equals, NotEquals }

        public override float CalculateLocalWeight() {
            var valueToCompare = observer.GetOutput();
            bool result;

            switch(comparison) {
                case FloatCompareType.GreaterThan:
                    result = valueToCompare > compareValue;
                    break;

                case FloatCompareType.LessThan:
                    result = valueToCompare < compareValue;
                    break;

                case FloatCompareType.Equals:
                    result = valueToCompare == compareValue;
                    break;

                case FloatCompareType.NotEquals:
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
