using System;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// Attribute used for positioning custom scripts in the node editor's dropdown menu
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class CustomMenuPathAttribute : Attribute {
        /// <summary>
        /// The local path of the script e.g. 'Humanoid Actions/Talk'
        /// </summary>
        public string path;

        public CustomMenuPathAttribute(string path) {
            this.path = path;
        }
    }
}