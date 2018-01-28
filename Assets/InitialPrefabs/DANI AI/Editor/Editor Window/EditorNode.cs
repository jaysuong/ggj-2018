using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// A node in the graph-based editor, representing a unique ai node
    /// </summary>
    public class EditorNode : Node {

        /// <summary>
        /// The ai node that this editor node represents
        /// </summary>
        public AINode Node { get { return node; } }

        /// <summary>
        /// A serializable representation of the node
        /// </summary>
        public SerializedObject SerializedNode { get; private set; }

        /// <summary>
        /// The type of the node
        /// </summary>
        public NodeType Type { get; private set; }

        /// <summary>
        /// The node's title
        /// </summary>
        public override string title {
            get {
                return Node.name;
            }

            set {
                SerializedNode.FindProperty ("m_Name").stringValue = value;
                EditorUtility.SetDirty (this);
            }
        }

        private bool IsRuntimeNode { get { return !AssetDatabase.Contains (node.GetInstanceID ()); } }

        private readonly Vector2 WindowSize = new Vector2 (150f, 66f);
        private readonly Vector2 stubSize = new Vector2 (10f, 50f);
        private readonly Vector2 EndPointSize = new Vector2 (8f, 8f);
        private const float EndPointBufferSize = 3f;
        private const float EndPointLeftMargin = 10f;
        private readonly Vector2 IconSize = new Vector2 (28f, 28f);

        private AINode node;
        private SerializedProperty positionProperty;

        private GUIStyle iconStyle;
        private bool shouldOverwriteIcon;

        private Rect inputRect;
        private Rect outputRect;

        private Dictionary<Slot, Vector2> slotPositions;

        private EditorGraphCanvas canvas;

        public void PrepareNode (AINode node, NodeType nodeType) {
            this.node = node;
            Type = nodeType;
            SerializedNode = new SerializedObject (node);
            positionProperty = SerializedNode.FindProperty ("m_modulePosition");
            position.position = positionProperty.vector2Value;
            position.size = WindowSize;

            inputRect = new Rect (Vector2.zero, stubSize);
            outputRect = new Rect (Vector2.zero, stubSize);

            slotPositions = new Dictionary<Slot, Vector2> ();

            shouldOverwriteIcon = AssetPreview.GetMiniThumbnail (node) !=
                AssetPreview.GetMiniTypeThumbnail (typeof (AINode));
        }

        public override void NodeUI (GraphGUI host) {
            canvas = host as EditorGraphCanvas;
            var current = Event.current;

            if (current.type == EventType.Repaint) {
                // Draw the name of the node
                GUI.Label (new Rect (0f, 0f, position.size.x, 15f),
                    node.name,
                    canvas.Skin.FindStyle ("Node Title"));

                // Draw the icon in the center of the node
                var centerRect = new Rect (position.size * 0.5f - IconSize * 0.5f,
                    IconSize);

                switch (Type) {
                    case NodeType.Observer:
                        iconStyle = canvas.Skin.FindStyle ("Observer Icon");
                        break;

                    case NodeType.Decision:
                        iconStyle = canvas.Skin.FindStyle ("Decision Icon");
                        break;

                    case NodeType.Action:
                        iconStyle = canvas.Skin.FindStyle ("Action Icon");
                        break;

                    default:
                        iconStyle = new GUIStyle ();
                        break;
                }

                if (shouldOverwriteIcon) {
                    iconStyle = new GUIStyle (iconStyle);
                    iconStyle.normal.background = AssetPreview.GetMiniThumbnail (Node);
                }

                iconStyle.Draw (centerRect, false, false, false, false);

                var labelRect = new Rect (new Rect (0, WindowSize.y - 20f, WindowSize.x, 20f));
                var textStyle = canvas.Skin.label;
                switch (Type) {
                    case NodeType.Observer:
                        GUI.Label (labelRect, string.Format ("Output: {0}", (node as Observer).Output), textStyle);
                        break;

                    case NodeType.Decision:
                        var decision = node as Decision;

                        if (!IsRuntimeNode) {
                            GUI.Label (labelRect, string.Format ("Score: {0}", decision.TotalScore), textStyle);
                        } else {
                            var brain = DaniRuntimeBridge.SelectedBrain;
                            var score = brain != null ? brain.GetCurrentDecisionScore (decision) : 0f;
                            GUI.Label (labelRect,
                                string.Format ("Score: {0} / {1}", score, decision.TotalScore),
                                textStyle);
                        }
                        break;

                    case NodeType.Action:
                        if (IsRuntimeNode) {
                            GUI.Label (labelRect, string.Format ("Status: {0}", (node as Action).CurrentState), textStyle);
                        }
                        break;
                }
            }

            // If the user left-clicks, then show the object in the inspector
            if (current.type == EventType.MouseUp) {
                switch (current.button) {
                    case 0:
                        if (current.control) {
                            if (!host.selection.Contains (this)) {
                                host.selection.Add (this);
                            }
                        } else if (!host.selection.Contains (this)) {
                            host.selection.Add (this);
                        }

                        (host as EditorGraphCanvas).Repaint ();
                        break;

                    case 1:
                        OpenContextMenu (current.mousePosition + position.position);
                        current.Use ();
                        break;
                }
            }
        }

        public override void OnDrag () {
            base.OnDrag ();
            positionProperty.vector2Value = position.position;
        }

        public Vector2 GetSlotPosition (Slot slot) {
            Vector2 position;

            if (slotPositions.TryGetValue (slot, out position)) {
                return position;
            }

            return Vector2.zero;
        }

        public bool IsMouseInStub (Vector2 position) {
            switch (Type) {
                case NodeType.Observer:
                    return outputRect.Contains (position);

                case NodeType.Decision:
                    return inputRect.Contains (position) || outputRect.Contains (position);

                default:
                    return inputRect.Contains (position);
            }
        }

        public void DrawStubs (GraphGUI canvas, int id) {
            if (Event.current.type == EventType.Repaint) {
                var skin = (canvas as EditorGraphCanvas).Skin;

                inputRect.position = new Vector2 (position.x - stubSize.x,
                    position.y + (position.height * 0.5f - stubSize.y * 0.5f));

                outputRect.position = new Vector2 (position.x + position.width,
                    position.y + (position.height * 0.5f - stubSize.y * 0.5f));

                if (Type == NodeType.Decision || Type == NodeType.Action) {
                    var stubStyle = skin.FindStyle ("Node Stub Left");
                    stubStyle.Draw (inputRect, GUIContent.none, id);
                }

                if (Type == NodeType.Observer || Type == NodeType.Decision) {
                    var stubStyle = skin.FindStyle ("Node Stub Right");
                    stubStyle.Draw (outputRect, GUIContent.none, id);
                }
            }
        }

        /// <summary>
        /// Calculates the position for each slot in the node
        /// </summary>
        public void CalculateSlotPositions () {
            slotPositions.Clear ();
            slots.Sort ((a, b) => { return (int) (a.node.position.y - b.node.position.y); });

            var inputs = slots.Where (s => s.type == SlotType.InputSlot);
            var outputs = slots.Where (s => s.type == SlotType.OutputSlot);

            var inputCenter = new Vector2 (position.x - EndPointLeftMargin, position.y + position.height * 0.5f);
            var outputCenter = new Vector2 (position.x + position.width, position.y + position.height * 0.5f);

            CalculatePositionSet (inputs, inputCenter);

            foreach (var slot in outputs) {
                slotPositions.Add (slot, outputCenter);
            }
        }

        /// <summary>
        /// Draws the endpoint textures on the node for each connection the node is connected to
        /// </summary>
        /// <param name="inputStyle">The style for the source nodes</param>
        /// <param name="outputStyle">The style for the target nodes</param>
        public void DrawConnectionEndPoints (GUIStyle inputStyle, GUIStyle outputStyle) {
            foreach (var slot in slotPositions) {
                var position = new Rect (slot.Value, EndPointSize);
                position.position -= new Vector2 (0f, EndPointSize.y * 0.5f);

                switch (slot.Key.type) {
                    case SlotType.InputSlot:
                        inputStyle.Draw (position, false, false, false, false);
                        break;

                    case SlotType.OutputSlot:
                        outputStyle.Draw (position, false, false, false, false);
                        break;
                }
            }
        }

        private void CalculatePositionSet (IEnumerable<Slot> slots, Vector2 center) {
            var maxGroupCount = Mathf.Min (slots.Count (), 4);
            var startPoint = new Vector2 (center.x,
                center.y - ((maxGroupCount - 1) * EndPointSize.y * 0.5f) - ((maxGroupCount - 1) * EndPointBufferSize));
            var index = 0;

            foreach (var slot in slots) {
                var normalizedIndex = index % 4;
                var position = new Vector2 (startPoint.x, startPoint.y + normalizedIndex * 10f + normalizedIndex * EndPointBufferSize);
                slotPositions.Add (slot, position);
                index++;
            }
        }

        /// <summary>
        /// Opens the context menu to allow copying and deleting nodes
        /// </summary>
        private void OpenContextMenu (Vector2 mousePosition) {
            var menu = new GenericMenu ();
            var selection = canvas.selection;

            menu.AddItem (new GUIContent ("Copy"),
                false,
                () => {
                    selection.RemoveAll (s => s == this);
                    selection.Add (this);

                    var nodes = new EditorNode[selection.Count];
                    for (var i = 0; i < selection.Count; ++i) {
                        nodes[i] = selection[i] as EditorNode;
                    }

                    (graph as EditorGraph).CopyBuffer.CopyNodes (nodes, mousePosition);
                });

            menu.AddItem (new GUIContent (selection.Count > 1 ?
                    string.Format ("Delete {0} nodes", selection.Count) : string.Format ("Delete {0}", Node.name)),
                false,
                () => {
                    selection.RemoveAll (s => s == this || s == null);
                    selection.Add (this);

                    // foreach (var node in selection) {
                    //     graph.RemoveNode (node, true);
                    // }
                    graph.RemoveNodes (selection.ToList (), true);
                });

            // Add the option to enable/disable the node
            if (selection.Count < 1 || (selection.Count == 1 && selection.Contains (this))) {
                menu.AddItem (new GUIContent (node.IsEnabled ? "Disable" : "Enable"),
                    false,
                    () => {
                        SerializedNode.Update ();
                        SerializedNode.FindProperty ("_isEnabled").boolValue = !node.IsEnabled;
                        SerializedNode.ApplyModifiedProperties ();
                    });
            } else if (selection.Count > 0) {
                menu.AddItem (new GUIContent ("Enable all selected nodes"),
                    false,
                    () => {
                        foreach (var node in selection) {
                            var serializedNode = (node as EditorNode).SerializedNode;

                            serializedNode.Update ();
                            serializedNode.FindProperty ("_isEnabled").boolValue = true;
                            serializedNode.ApplyModifiedProperties ();
                        }
                    });

                menu.AddItem (new GUIContent ("Disable all selected nodes"),
                    false,
                    () => {
                        foreach (var node in selection) {
                            var serializedNode = (node as EditorNode).SerializedNode;

                            serializedNode.Update ();
                            serializedNode.FindProperty ("_isEnabled").boolValue = false;
                            serializedNode.ApplyModifiedProperties ();
                        }
                    });
            }

            GUIScaleUtility.BeginNoClip ();
            menu.ShowAsContext ();
            GUIScaleUtility.RestoreClips ();
        }
    }
}