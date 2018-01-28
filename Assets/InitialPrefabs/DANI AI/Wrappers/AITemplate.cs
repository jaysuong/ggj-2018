using UnityEngine;

namespace InitialPrefabs.DANI.Wrappers {
    /// <summary>
    /// A wrapper class that implement's Dani's AITemplate class.  Useful for 
    /// upgrading Dani without losing references
    /// </summary>    /// 
    [CreateAssetMenu(fileName = "New AI Template", menuName = "Dani AI/AI Template")]
    public class AITemplate : DANI.AITemplate { }
}