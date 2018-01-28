using System;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// A utility class used to hide the `compareValue` field in the Condition. Mainly
    /// used for Conditions where the compare value is not useful or confusing, such
    /// as `BoolCondition`
    /// </summary>
    public class HideCompareValueAttribute : Attribute { }
}