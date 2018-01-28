using System;
using System.Collections.Generic;
using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    using Action = InitialPrefabs.DANI.Action;
    using SidePanelType = DaniEditorWindow.SidePanelType;

    /// <summary>
    /// A drawable canvas used to render graphs
    /// </summary>
    public class EditorGraphCanvas : GraphGUI {
        public EditorWindow Host { get { return m_Host; } set { m_Host = value; } }
        /// <summary>
        /// Is the editor linked to the brain?
        /// </summary>
        public bool IsLinkedToBrain { get; private set; }

        public GUIStyle NodeStyle { get; private set; }

        public EditorGraph EditorGraph { get { return editorGraph; } }

        public GUISkin Skin { get; set; }

        /// <summary>
        /// Is the mouse in any of the nodes?
        /// </summary>
        private bool IsMouseInNode {
            get {
                var isInNode = false;
                var currentPos = Event.current.mousePosition;

                for (var i = 0; i < editorGraph.nodes.Count; ++i) {
                    var node = editorGraph.nodes[i];
                    if (node.position.Contains (currentPos) || node.isDragging) {
                        isInNode = true;
                    }
                }

                return isInNode;
            }
        }

        private const string DaniLinkKey = "DANI_LINK_BRAIN";

        private readonly Color DefaultNodeColor = Color.white;
        private readonly Color DisabledNodeColor = new Color (0.4f, 0.4f, 0.4f, 0.5f);
        private readonly Color RunningNodeColor = new Color (0.1f, 1f, 0.1f, 0.8f);

        private EditorGraph editorGraph;

        // Drag properties
        private DragHandler dragHandler;
        private bool isPerformingSelectionDrag;
        private Rect selectionRect;

        // Styles
        private StyleMap styleMap;

        public override void OnEnable () {
            base.OnEnable ();

            m_EdgeGUI = new ConnectionUI ();

            NodeStyle = new GUIStyle ();
            NodeStyle.alignment = TextAnchor.MiddleCenter;

            dragHandler = new DragHandler ();
            styleMap = new StyleMap ();

            IsLinkedToBrain = EditorPrefs.GetBool (DaniLinkKey);
        }

        private void OnDisable () {
            EditorPrefs.SetBool (DaniLinkKey, IsLinkedToBrain);
        }

        public override void OnGraphGUI () {
            // Update the skin
            styleMap.Skin = Skin;

            var serializedTemplate = editorGraph.SerializedTemplate;

            if (serializedTemplate != null && serializedTemplate.targetObject != null) {
                serializedTemplate.Update ();

                m_Host.BeginWindows ();

                var nodes = editorGraph.nodes;
                for (var i = 0; i < nodes.Count; ++i) {
                    var node = nodes[i] as EditorNode;
                    node.SerializedNode.Update ();

                    var oldColor = GUI.color;
                    if (!IsLinkedToBrain) {
                        GUI.color = node.Node.IsEnabled ? DefaultNodeColor : DisabledNodeColor;
                    } else {
                        GUI.color = EditorApplication.isPlayingOrWillChangePlaymode && IsNodeRunning (node) ? RunningNodeColor :
                            node.Node.IsEnabled ? DefaultNodeColor : DisabledNodeColor;
                    }

                    var windowStyle = styleMap.GetStyle (node.Type);
                    var windowSelectStyle = new GUIStyle (windowStyle);
                    windowSelectStyle.normal = windowSelectStyle.onNormal;

                    node.position = GUI.Window (
                        node.GetInstanceID (),
                        node.position,
                        (int id) => { NodeGUI (node); },
                        string.Empty,
                        selection.Contains (node) ? windowSelectStyle : windowStyle);

                    node.CalculateSlotPositions ();

                    node.SerializedNode.FindProperty (AINodeUtility.PositionPropertyName).vector2Value = node.position.position;
                    node.SerializedNode.ApplyModifiedProperties ();

                    GUI.color = oldColor;
                }

                // Draw an empty window, if there are no nodes
                if (nodes.Count < 1) {
                    GUI.Window (GetInstanceID (),
                        new Rect (-100f, -100f, 200f, 200f),
                        (int id) => { },
                        GUIContent.none,
                        GUIStyle.none);
                }

                m_Host.EndWindows ();

                if (IsMouseInNode) {
                    DragNodes ();
                }

                var current = Event.current;

                HandleDragCommands (current);

                switch (current.type) {
                    case EventType.ContextClick:
                        DrawContextMenu (current.mousePosition);
                        break;

                    case EventType.MouseDrag:
                        if (current.button == 0) {
                            // Create a new rect to select multiple nodes
                            if (!isPerformingSelectionDrag) {
                                isPerformingSelectionDrag = true;
                                selectionRect = new Rect (current.mousePosition, Vector2.one);
                                Repaint ();
                            } else {
                                selectionRect.size = current.mousePosition - selectionRect.position;
                            }
                        }
                        break;

                    case EventType.MouseUp:
                        if (current.button == 0) {
                            if (dragHandler.IsDragging) {
                                var targetNode = graph.nodes.Where (
                                    n => (n as EditorNode).IsMouseInStub (current.mousePosition)).
                                FirstOrDefault ();

                                if (targetNode != null) {
                                    var endSlot = new Slot (SlotType.OutputSlot, (targetNode as EditorNode).Node.Id.ToString ());
                                    endSlot.node = targetNode;

                                    AttemptToConnectSlots (dragHandler.StartSlot, endSlot);
                                }

                                dragHandler.EndDrag ();
                            }

                            // If the user is dragging a rectangle, end it and select all nodes within the rectange
                            if (isPerformingSelectionDrag) {
                                isPerformingSelectionDrag = false;

                                var selectedNodes = editorGraph.nodes.
                                Where (n => selectionRect.Overlaps (n.position, true));

                                selection.Clear ();
                                selection.AddRange (selectedNodes);
                                Repaint ();
                            } else {
                                AttemptToDeselectNodes (current.mousePosition);
                            }
                        }
                        break;

                    case EventType.Ignore:
                        if (isPerformingSelectionDrag) {
                            isPerformingSelectionDrag = false;
                            selectionRect = new Rect (0f, 0f, 0f, 0f);
                        }
                        break;

                    case EventType.MouseDown:
                        if (!dragHandler.IsDragging) {
                            var potentialStartNode = graph.nodes.Where (
                                n => (n as EditorNode).IsMouseInStub (current.mousePosition)).
                            FirstOrDefault ();

                            if (potentialStartNode != null) {
                                dragHandler.StartDrag (potentialStartNode as EditorNode, current.mousePosition);
                                Repaint ();
                            }
                        }
                        break;

                    case EventType.Repaint:
                        if (dragHandler.IsDragging) {
                            DrawBezier (dragHandler.StartPosition, current.mousePosition);
                            Repaint ();
                        } else if (isPerformingSelectionDrag) {
                            EditorGUI.DrawRect (selectionRect, new Color (1f, 1f, 1f, 0.2f));
                            Repaint ();
                        }
                        break;
                }

                serializedTemplate.ApplyModifiedProperties ();
            }
        }

        public override void OnToolbarGUI () {
            // Draw the side panel toolbar
            var editorWindow = m_Host as DaniEditorWindow;

            GUILayout.BeginHorizontal (EditorStyles.toolbar, GUILayout.Width (DaniEditorWindow.PanelWidth));
            GUILayout.Space (8f);

            foreach (var value in Enum.GetValues (typeof (SidePanelType))) {
                if (GUILayout.Toggle (editorWindow.OpenPanelType == (SidePanelType) value,
                        value.ToString (),
                        EditorStyles.toolbarButton,
                        GUILayout.ExpandWidth (true))) {

                    editorWindow.OpenPanelType = (SidePanelType) value;
                }
            }

            GUILayout.Space (8f);
            GUILayout.EndHorizontal ();

            // Add padding to line up the dropdown button
            GUILayout.Space (2f);

            // Draw the dropdown menu
            if (editorGraph.Template != null) {
                if (GUILayout.Button (editorGraph.Template.name,
                        EditorStyles.toolbarDropDown, GUILayout.MinWidth (300f))) {

                    DrawTemplateMenu ();
                }
            } else {
                if (GUILayout.Button ("Select an AITemplate",
                        EditorStyles.toolbarDropDown, GUILayout.MinWidth (300f))) {

                    DrawTemplateMenu ();
                }
            }

            // Draw the linked content
            var linkContent = new GUIContent ("Live Debugging",
                "Highlights what nodes are currently running in `Play Mode`");
            IsLinkedToBrain = GUILayout.Toggle (IsLinkedToBrain, linkContent,
                EditorStyles.toolbarButton, GUILayout.Width (125f));

            // Draw the screenshot button
            if (GUILayout.Button ("Take Screenshot", EditorStyles.toolbarButton, GUILayout.Width (125f))) {
                (Host as DaniEditorWindow).IsTakingScreenShot = true;
            }
        }

        public override void NodeGUI (Node n) {
            SelectCurrentNode (n);
            n.NodeUI (this);
            DragNodes ();
        }

        /// <summary>
        /// Repaints the GUI
        /// </summary>
        public void Repaint () {
            if (m_Host != null) {
                m_Host.Repaint ();
            }
        }

        /// <summary>
        /// Updates the canvas's graph to work with
        /// </summary>
        public void UpdateGraph (EditorGraph graph) {
            editorGraph = graph;
            this.graph = graph;
        }

        /// <summary>
        /// Attempts to connect the two slots together
        /// </summary>
        /// <param name="start">The start slot</param>
        /// <param name="end">The end slot</param>
        private void AttemptToConnectSlots (Slot start, Slot end) {
            // Quit early if the start and end nodes are the same
            if (start.name == end.name) { return; }

            var startType = (start.node as EditorNode).Type;
            var endType = (end.node as EditorNode).Type;

            if (startType == NodeType.Observer && endType == NodeType.Decision ||
                startType == NodeType.Decision && endType == NodeType.Action) {

                try {
                    graph.Connect (start, end);
                } catch (System.ArgumentException) {
                    dragHandler.EndDrag ();
                }
            } else if (startType == NodeType.Decision && endType == NodeType.Observer ||
                startType == NodeType.Action && endType == NodeType.Decision) {

                try {
                    start.type = SlotType.OutputSlot;
                    end.type = SlotType.InputSlot;

                    graph.Connect (end, start);
                } catch (System.ArgumentException) {
                    dragHandler.EndDrag ();
                }
            }
        }

        /// <summary>
        /// Attempts to deselect nodes if the user clicks outside of any nodes in the
        /// graph
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        private void AttemptToDeselectNodes (Vector2 mousePosition) {
            var canDeselect = true;

            for (var i = 0; i < graph.nodes.Count; ++i) {
                if (graph.nodes[i].position.Contains (mousePosition)) {
                    canDeselect = false;
                    break;
                }
            }

            if (canDeselect) {
                ClearSelection ();
                Repaint ();
            }
        }

        /// <summary>
        /// Draws connections in the form of bezier curves
        /// </summary>
        private void DrawConnections () {
            var template = editorGraph.Template;

            if (template != null) {
                var connections = template.Connections;

                for (var i = 0; i < connections.Length; ++i) {
                    var connection = connections[i];

                    var inputNode = editorGraph.GetNode (connection.SourceId);
                    var outputNode = editorGraph.GetNode (connection.TargetId);

                    if (inputNode != null && outputNode != null) {
                        var startPos = GetRightConnectionPoint (inputNode.position);
                        var endPos = GetLeftConnectionPoint (outputNode.position);

                        DrawBezier (startPos, endPos);
                    }
                }
            }
        }

        private void DrawContextMenu (Vector2 position) {
            var menu = new GenericMenu ();
            var looseScriptList = new List<MonoScript> ();
            var nestedScriptList = new List<MonoScript> ();
            var masterScriptList = new List<MonoScript> ();

            // Fetch the scripts from the asset database
            var observerScripts = AssetSearcher.GetMonoScripts<Observer> (true);
            var decisionScripts = AssetSearcher.GetMonoScripts<Decision> (true);
            var actionScripts = AssetSearcher.GetMonoScripts<Action> (true);

            // Add observers
            if (observerScripts.Length > 0) {
                foreach (var script in observerScripts) {
                    var localPath = AssetSearcher.GetLocalMenuPath (script);

                    if (string.IsNullOrEmpty (localPath)) {
                        looseScriptList.Add (script);
                    } else {
                        nestedScriptList.Add (script);
                    }
                }

                masterScriptList.AddRange (nestedScriptList);
                masterScriptList.AddRange (looseScriptList);

                foreach (var script in masterScriptList) {
                    var type = script.GetClass ();
                    var localPath = AssetSearcher.GetLocalMenuPath (script);

                    menu.AddItem (
                        new GUIContent (
                            string.Format ("Add Observer/{0}",
                                (!string.IsNullOrEmpty (localPath)) ? localPath : type.Name)),
                        false,
                        () => { CreateNode (script, NodeType.Observer, position); });
                }
            } else {
                menu.AddDisabledItem (new GUIContent ("Add Observer"));
            }

            masterScriptList.Clear ();
            nestedScriptList.Clear ();
            looseScriptList.Clear ();

            // Add decisions
            if (decisionScripts.Length > 0) {
                foreach (var script in decisionScripts) {
                    var localPath = AssetSearcher.GetLocalMenuPath (script);

                    if (string.IsNullOrEmpty (localPath)) {
                        looseScriptList.Add (script);
                    } else {
                        nestedScriptList.Add (script);
                    }
                }

                masterScriptList.AddRange (nestedScriptList);
                masterScriptList.AddRange (looseScriptList);

                foreach (var script in masterScriptList) {
                    var type = script.GetClass ();
                    var localPath = AssetSearcher.GetLocalMenuPath (script);

                    menu.AddItem (
                        new GUIContent (
                            string.Format ("Add Decision/{0}",
                                (!string.IsNullOrEmpty (localPath)) ? localPath : type.Name)),
                        false,
                        () => { CreateNode (script, NodeType.Decision, position); });
                }
            } else {
                menu.AddDisabledItem (new GUIContent ("Add Decision"));
            }

            masterScriptList.Clear ();
            nestedScriptList.Clear ();
            looseScriptList.Clear ();

            // Add Actions
            if (actionScripts.Length > 0) {
                foreach (var script in actionScripts) {
                    var localPath = AssetSearcher.GetLocalMenuPath (script);

                    if (string.IsNullOrEmpty (localPath)) {
                        looseScriptList.Add (script);
                    } else {
                        nestedScriptList.Add (script);
                    }
                }

                masterScriptList.AddRange (nestedScriptList);
                masterScriptList.AddRange (looseScriptList);

                foreach (var script in masterScriptList) {
                    var type = script.GetClass ();
                    var localPath = AssetSearcher.GetLocalMenuPath (script);

                    menu.AddItem (
                        new GUIContent (
                            string.Format ("Add Action/{0}",
                                (!string.IsNullOrEmpty (localPath)) ? localPath : type.Name)),
                        false,
                        () => { CreateNode (script, NodeType.Action, position); });
                }
            } else {
                menu.AddDisabledItem (new GUIContent ("Add Action"));
            }

            // Add an option to paste the nodes
            menu.AddSeparator (string.Empty);
            var buffer = editorGraph.CopyBuffer;
            if (buffer.NodeCount > 0) {
                menu.AddItem (new GUIContent (buffer.NodeCount > 1 ? string.Format ("Paste {0} nodes", buffer.NodeCount) : "Paste node"),
                    false,
                    () => { buffer.PasteNodesTo (editorGraph, position); });
            } else {
                menu.AddDisabledItem (new GUIContent ("Paste node"));
            }

            // Add an option to center the graph
            menu.AddSeparator (string.Empty);
            menu.AddItem (new GUIContent ("Center Workspace"),
                false,
                () => CenterGraph ());

            GUIScaleUtility.BeginNoClip ();
            menu.ShowAsContext ();
            GUIScaleUtility.RestoreClips ();
        }

        /// <summary>
        /// Draws a dropdown menu to select various templates
        /// </summary>
        private void DrawTemplateMenu () {
            var guids = AssetDatabase.FindAssets ("t:AITemplate");
            var templates = new AITemplate[guids.Length];

            for (var i = 0; i < guids.Length; ++i) {
                var path = AssetDatabase.GUIDToAssetPath (guids[i]);
                templates[i] = AssetDatabase.LoadAssetAtPath<AITemplate> (path);
            }

            // Sort the array by name
            templates = templates.OrderBy (t => t.name).ToArray ();

            // Create a new menu
            var menu = new GenericMenu ();

            // Add an option to create a new template
            menu.AddItem (new GUIContent ("Create a new AITemplate..."),
                false,
                () => {
                    var path = EditorUtility.SaveFilePanelInProject (
                        "Create a new AI Template",
                        "AI Template",
                        "asset",
                        "Where to save?"
                    );

                    if (!string.IsNullOrEmpty (path)) {
                        var script = AssetSearcher.GetMonoScripts<AITemplate> ().Last ();
                        var createdTemplate = CreateInstance (script.GetClass ()) as AITemplate;

                        AssetDatabase.CreateAsset (createdTemplate, path);
                        AssetDatabase.SaveAssets ();

                        editorGraph.Template = createdTemplate;
                    }
                });

            menu.AddSeparator (string.Empty);

            foreach (var aiTemplate in templates) {
                if (templates == null) { continue; }

                menu.AddItem (
                    new GUIContent (aiTemplate.name),
                    aiTemplate == editorGraph.Template,
                    () => {
                        Selection.activeObject = aiTemplate;
                    }
                );
            }

            // Display the menu
            menu.DropDown (new Rect (DaniEditorWindow.PanelWidth + DaniEditorWindow.ToolbarOffset, 17f, 0f, 0f));
        }

        /// <summary>
        /// Creates a new node and adds it to the graph
        /// </summary>
        private void CreateNode (MonoScript script, NodeType type, Vector2 position) {
            var aiNode = CreateInstance (script.GetClass ()) as AINode;
            aiNode.name = RegexTools.GetReadableText (script.GetClass ().Name);
            aiNode.hideFlags = HideFlags.HideInHierarchy;

            var serializedNode = new SerializedObject (aiNode);
            serializedNode.FindProperty (AINodeUtility.IdPropertyName).intValue = aiNode.GetInstanceID ();
            serializedNode.FindProperty (AINodeUtility.PositionPropertyName).vector2Value = position;
            serializedNode.ApplyModifiedPropertiesWithoutUndo ();

            var node = CreateInstance<EditorNode> ();
            node.PrepareNode (aiNode, type);
            editorGraph.AddNode (node);
        }

        /// <summary>
        /// Draws a bezier curve from start to finish
        /// </summary>
        /// <param name="startPos">The start position</param>
        /// <param name="endPos">The endpoint</param>
        private void DrawBezier (Vector3 startPos, Vector3 endPos) {
            var startTan = startPos + Vector3.right * 50;
            var endTan = endPos + Vector3.left * 50;
            var shadowCol = new Color (1, 1, 1, 0.06f);
            for (var i = 0; i < 3; i++) // Draw a shadow
                Handles.DrawBezier (startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            Handles.DrawBezier (startPos, endPos, startTan, endTan, Color.white, null, 1);
        }

        private Vector2 GetLeftConnectionPoint (Rect position) {
            return new Vector3 (position.x, position.y + position.height * 0.5f, 0f);
        }

        private Vector2 GetRightConnectionPoint (Rect position) {
            return new Vector3 (position.x + position.width,
                position.y + position.height * 0.5f,
                0f);
        }

        /// <summary>
        /// Reads the current DragAndDrop commands and attempts to select templates or add nodes
        /// based on the current file selection
        /// </summary>
        /// <param name="current">The current event</param>
        private void HandleDragCommands (Event current) {
            if (current.type == EventType.DragUpdated) {
                var references = DragAndDrop.objectReferences;

                var templateResults = references.Where (r => r is AITemplate);
                var scriptResults = references.Where (
                    (r) => {
                        if (r is MonoScript) {
                            var type = (r as MonoScript).GetClass ();
                            return type != null && !type.IsAbstract &&
                                (type.IsSubclassOf (typeof (Observer)) ||
                                    type.IsSubclassOf (typeof (Decision)) ||
                                    type.IsSubclassOf (typeof (Action)));
                        }
                        return false;
                    });

                if (templateResults.Any () || scriptResults.Any ()) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                } else {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
            } else if (current.type == EventType.DragPerform) {
                var references = DragAndDrop.objectReferences;

                // Attempt to open the template, if possible
                var templateResults = references.Where (r => r is AITemplate);
                if (templateResults.Any ()) {
                    DragAndDrop.AcceptDrag ();
                    current.Use ();

                    editorGraph.Template = templateResults.FirstOrDefault () as AITemplate;
                } else {
                    var scriptResults = references.Where (
                        (r) => {
                            if (r is MonoScript) {
                                var type = (r as MonoScript).GetClass ();
                                return type != null && !type.IsAbstract &&
                                    (type.IsSubclassOf (typeof (Observer)) ||
                                        type.IsSubclassOf (typeof (Decision)) ||
                                        type.IsSubclassOf (typeof (Action)));
                            }
                            return false;
                        });

                    if (scriptResults.Any ()) {
                        DragAndDrop.AcceptDrag ();
                        current.Use ();

                        var script = scriptResults.FirstOrDefault () as MonoScript;
                        var scriptType = script.GetClass ().IsSubclassOf (typeof (Observer)) ? NodeType.Observer :
                            script.GetClass ().IsSubclassOf (typeof (Decision)) ? NodeType.Decision : NodeType.Action;
                        CreateNode (script, scriptType, current.mousePosition);
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the node is running in the brain
        /// </summary>
        private bool IsNodeRunning (EditorNode node) {
            var activeDecision = DaniRuntimeBridge.SelectedDecision;
            var selectedTemplate = DaniRuntimeBridge.SelectedTemplate;

            if (editorGraph.Template != selectedTemplate || activeDecision == null || selectedTemplate == null) {
                return false;
            }

            if (node.Type == NodeType.Decision && node.Node == activeDecision) {
                return true;
            }

            var connections = selectedTemplate.Connections.Where (
                c => c.SourceId == activeDecision.Id || c.TargetId == activeDecision.Id);

            return connections.Where (c => c.SourceId == node.Node.Id || c.TargetId == node.Node.Id).Any ();
        }

        private void SelectCurrentNode (Node node) {
            SelectNode (node);

            var activeObjects = Selection.objects.Where (o => !(o is Node)).ToArray ();
            Selection.objects = activeObjects;
        }
    }
}