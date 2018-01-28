using System;
using System.Collections.Generic;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    public class StyleMap {
        /// <summary>
        /// The mao's skin
        /// </summary>
        public GUISkin Skin {
            get { return skin; }
            set { Update (value); }
        }

        private GUISkin skin;
        private Dictionary<NodeType, GUIStyle> styles;

        private readonly GUIStyle DefaultStyle = new GUIStyle ();

        public StyleMap () {
            styles = new Dictionary<NodeType, GUIStyle> ();
        }

        /// <summary>
        /// Gets the style of a particular node type
        /// </summary>
        /// <param name="type">The type of node</param>
        /// <returns>A default style if no such node is found</returns>
        public GUIStyle GetStyle (NodeType type) {
            var style = DefaultStyle;
            styles.TryGetValue (type, out style);
            return style;
        }

        /// <summary>
        /// Updates the map with a new skin
        /// </summary>
        /// <param name="skin">The skin to update with</param>
        /// <param name="forceUpdate">Should the map clean all of its references and start over?</param>
        public void Update (GUISkin skin, bool forceUpdate = false) {
            if (skin != null && (this.skin != skin || forceUpdate)) {
                this.skin = skin;
                styles.Clear ();

                foreach (var value in Enum.GetValues (typeof (NodeType))) {
                    var styleName = string.Format ("{0} Node", (NodeType) value);

                    styles.Add ((NodeType) value, skin.FindStyle (styleName));
                }
            }
        }
    }
}