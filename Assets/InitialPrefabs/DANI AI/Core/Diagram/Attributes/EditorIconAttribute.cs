using UnityEngine;
using System;

namespace InitialPrefabs.DANI {
    /// <summary>
    /// Attribute used to display an icon for a particular AINode in the editor
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [Obsolete("To use a custom icon for your nodes, please change the icon in the MonoScript asset containing your class.")]
    public class EditorIconAttribute : Attribute {
        /// <summary>
        /// The relative path of the icon e.g. Assets/Textures/icon.png
        /// </summary>
        public string path;

        public EditorIconAttribute(string path) {
            this.path = path;
        }
    }
}
