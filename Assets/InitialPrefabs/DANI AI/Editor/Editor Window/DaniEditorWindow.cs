using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace InitialPrefabs.DANIEditor {
    public class DaniEditorWindow : EditorWindow {
        [SerializeField]
        private AITemplate template;
        [SerializeField]
        private float zoom;

        public enum SidePanelType { Inspector, Variables }

        internal SidePanelType OpenPanelType { get; set; }

        internal float CurrentZoom { get { return zoom; } set { zoom = Mathf.Clamp (value, MinZoom, MaxZoom); } }

        internal bool IsTakingScreenShot { get; set; }

        internal Vector2 ZoomOffset { get; private set; }

        private bool IsValid { get { return graphCanvas != null && variableCanvas != null && graph != null; } }

        private EditorGraphCanvas graphCanvas;
        private EditorGraph graph;
        private VariableCanvas variableCanvas;
        private Editor inspectorTarget;
        private Vector2 inspectorScroll;

        private ScriptDatabase scriptDatabase;

        private bool canDeleteNodes;

        // Constants
        private const string TemplateIdKey = "DANI_TEMPLATE_ID";
        private const string ZoomIdKey = "DANI_ZOOM";

        private const float DefaultZoom = 1f;
        private const float MinZoom = 0.6f;
        private const float MaxZoom = 1.5f;
        private const float ZoomIncrement = 0.1f;

        private readonly Vector2 DefaultWindowSize = new Vector2 (640, 480);
        internal const float PanelWidth = 320f;
        internal const float ToolbarOffset = 8f;

        /// <summary>
        /// Opens the editor window
        /// </summary>
        [MenuItem ("Tools/InitialPrefabs/Dani AI/Open Editor Window")]
        public static void Open () {
            var window = GetWindow<DaniEditorWindow> (false, "Dani Editor", true);
            window.minSize = window.DefaultWindowSize;

            window.OpenStartWindow ();
        }

        public static void Open (AITemplate template) {
            var window = GetWindow<DaniEditorWindow> (false, "Dani Editor", true);
            window.minSize = window.DefaultWindowSize;

            window.graph.Template = template;
            window.OpenStartWindow ();
        }

        private void OnEnable () {
            graphCanvas = CreateInstance<EditorGraphCanvas> ();
            graph = CreateInstance<EditorGraph> ();

            variableCanvas = CreateInstance<VariableCanvas> ();
            variableCanvas.Graph = graph;
            variableCanvas.Window = this;

            scriptDatabase = CreateInstance<ScriptDatabase> ();
            graph.ScriptDatabase = scriptDatabase;
            canDeleteNodes = true;

            if (!EditorPrefs.HasKey (ZoomIdKey)) {
                EditorPrefs.SetFloat (ZoomIdKey, DefaultZoom);
            }
            zoom = EditorPrefs.GetFloat (ZoomIdKey);

            LoadGUISkin ();

            graphCanvas.Host = this;
            graphCanvas.UpdateGraph (graph);

            // Load a template, if possible
            if (Selection.activeObject is AITemplate) {
                graph.Template = Selection.activeObject as AITemplate;
            } else if (template != null) {
                graph.Template = template;
            } else if (EditorPrefs.HasKey (TemplateIdKey)) {
                var id = EditorPrefs.GetString (TemplateIdKey);
                var guids = AssetDatabase.FindAssets ("t:AITemplate");
                for (var i = 0; i < guids.Length; ++i) {
                    var path = AssetDatabase.GUIDToAssetPath (guids[i]);
                    var loadedTemplate = AssetDatabase.LoadAssetAtPath<AITemplate> (path);

                    if (loadedTemplate.Id == id) {
                        graph.Template = loadedTemplate;
                        break;
                    }
                }

            }

            graphCanvas.CenterGraph ();

            Undo.undoRedoPerformed += graph.Refresh;
            Undo.undoRedoPerformed += Repaint;
            graph.OnRequireRepaint += Repaint;

            DaniRuntimeBridge.OnTemplateSelectEvent += HandleTemplateSelectEvent;
            EditorBridge.OnObjectSelect += HandleDaniObjectSelection;

            Repaint ();
        }

        private void OnDisable () {
            Undo.undoRedoPerformed -= Repaint;
            Undo.undoRedoPerformed -= graph.Refresh;
            graph.OnRequireRepaint -= Repaint;

            DaniRuntimeBridge.OnTemplateSelectEvent -= HandleTemplateSelectEvent;
            EditorBridge.OnObjectSelect -= HandleDaniObjectSelection;

            EditorPrefs.SetFloat (ZoomIdKey, zoom);

            if (graphCanvas != null) {
                DestroyImmediate (graphCanvas);
            }
            if (graph != null) {
                DestroyImmediate (graph);
            }
            if (inspectorTarget != null) {
                DestroyImmediate (inspectorTarget);
            }
            if (variableCanvas != null) {
                DestroyImmediate (variableCanvas);
            }
            if (scriptDatabase != null) {
                DestroyImmediate (scriptDatabase);
            }
        }

        private void Update () {
            if (EditorApplication.isPlaying && !EditorApplication.isPaused) {
                Repaint ();
            }
        }

        private void OnGUI () {
            if (!IsValid) {
                OnDisable ();
                OnEnable ();
            }

            // clean references, if possible
            graph.nodes.RemoveAll (n => n == null || (n as EditorNode).Node == null);

            // Draw the panel
            switch (OpenPanelType) {
                case SidePanelType.Inspector:
                    DrawLocalInspector ();
                    break;

                case SidePanelType.Variables:
                    DrawVariableInspector ();
                    break;
            }

            DrawNodeGraph ();

            graphCanvas.BeginToolbarGUI (new Rect (-ToolbarOffset, 0f, position.width + ToolbarOffset, 16f));
            graphCanvas.OnToolbarGUI ();
            graphCanvas.EndToolbarGUI ();

            var current = Event.current;
            if (current.type == EventType.KeyUp) {
                if (canDeleteNodes) {
                    if (current.keyCode == KeyCode.Delete) {
                        current.Use ();

                        if (graphCanvas.selection.Count > 0) {
                            var selectedNodes = graphCanvas.selection.ToList ();
                            graph.RemoveNodes (selectedNodes, true);
                            Repaint ();
                        } else {
                            var connectionUI = graphCanvas.edgeGUI as ConnectionUI;
                            if (connectionUI.ActiveEdge != null) {
                                graph.RemoveConnectionEdge (connectionUI.ActiveEdge);
                                Repaint ();
                            }
                        }
                    }
                } else {
                    canDeleteNodes = true;
                }

                // Check for zoom commands
                if (current.alt || current.command) {
                    if (current.keyCode == KeyCode.Minus || current.keyCode == KeyCode.KeypadMinus) {
                        CurrentZoom += ZoomIncrement;
                        EditorPrefs.SetFloat (ZoomIdKey, CurrentZoom);
                        Repaint ();
                    } else if (current.keyCode == KeyCode.Equals || current.keyCode == KeyCode.KeypadPlus) {
                        CurrentZoom -= ZoomIncrement;
                        EditorPrefs.SetFloat (ZoomIdKey, CurrentZoom);
                        Repaint ();
                    } else if (current.keyCode == KeyCode.Alpha0 || current.keyCode == KeyCode.Keypad0) {
                        CurrentZoom = DefaultZoom;
                        EditorPrefs.SetFloat (ZoomIdKey, CurrentZoom);
                        Repaint ();
                    }
                }
            }

            // Draw annotations if a screenshot is being taken
            if (IsTakingScreenShot) {
                GUI.Label (new Rect (PanelWidth, 17f, PanelWidth, 50f),
                    graph.Template != null ? graph.Template.name : string.Empty,
                    EditorStyles.whiteLargeLabel);
            }

            if (current.type == EventType.Repaint) {
                if (IsTakingScreenShot) {
                    IsTakingScreenShot = false;

                    var screenshot = new Texture2D (
                        Mathf.RoundToInt (position.width - PanelWidth),
                        Mathf.RoundToInt (position.height - 16f),
                        TextureFormat.RGB24, false);
                    screenshot.ReadPixels (
                        new Rect (PanelWidth, 0, position.width - PanelWidth, position.height - 16),
                        0, 0);

                    var path = EditorUtility.SaveFilePanel ("Save screenshot", Directory.GetCurrentDirectory (), "Screenshot", "png");
                    if (!string.IsNullOrEmpty (path)) {
                        var bytes = screenshot.EncodeToPNG ();

                        // Write the file to the path
                        File.WriteAllBytes (path, bytes);
                    }

                    Repaint ();
                }
            }
        }

        private void OnSelectionChange () {
            if (Selection.activeObject is AITemplate) {
                graphCanvas.selection.Clear ();

                var template = Selection.activeObject as AITemplate;
                graph.Template = template;
                this.template = template;
                graphCanvas.CenterGraph ();

                EditorPrefs.SetString (TemplateIdKey, template.Id);

                if (inspectorTarget != null) {
                    DestroyImmediate (inspectorTarget);
                }

                Repaint ();
            }
        }

        private void AssignLocalInspector () {
            if (inspectorTarget != null && inspectorTarget.target == null) {
                DestroyImmediate (inspectorTarget);
            }

            var node = graphCanvas.selection.Count > 0 ?
                (graphCanvas.selection.Last () as EditorNode).Node as Object : null;

            // Look for an active connection, if it exists, then overwrite the node
            var activeConnection = LookForActiveConnection ();
            if (activeConnection != null) {
                node = activeConnection;
            }

            if (inspectorTarget != null) {
                if (inspectorTarget.target != node) {
                    DestroyImmediate (inspectorTarget);
                    inspectorTarget = Editor.CreateEditor (node);
                }
            } else {
                inspectorTarget = Editor.CreateEditor (node);
            }
        }

        /// <summary>
        /// Draws a panel containing the local inspector
        /// </summary>
        private void DrawLocalInspector () {
            AssignLocalInspector ();

            GUI.Box (new Rect (0f, 16f, PanelWidth, position.height - 16f), GUIContent.none);

            GUILayout.BeginArea (new Rect (4f, 16f, PanelWidth - 8f, position.height - 17f));
            inspectorScroll = GUILayout.BeginScrollView (inspectorScroll);

            var oldColor = Handles.color;
            Handles.color = Color.black;

            Handles.DrawLine (Vector3.one, Vector3.right * PanelWidth);

            Handles.color = oldColor;

            if (inspectorTarget != null) {
                try {
                    inspectorTarget.DrawHeader ();
                    inspectorTarget.OnInspectorGUI ();
                } catch { }

                var current = Event.current;
                if (current.type == EventType.Used && current.keyCode == KeyCode.Delete) {
                    canDeleteNodes = false;
                }
            }

            GUILayout.EndScrollView ();
            GUILayout.EndArea ();
        }

        private void DrawNodeGraph () {
            GUIScaleUtility.CheckInit ();
            var scaledRect = new Rect (PanelWidth, 17f, position.width - PanelWidth, position.height - 17f);
            var pivot = new Vector2 ((position.width - PanelWidth) * 0.5f,
                (position.height - 17f) * 0.5f);
            ZoomOffset = GUIScaleUtility.BeginScale (ref scaledRect, pivot, zoom, true, false);
            GUILayout.BeginArea (scaledRect);
            graphCanvas.BeginGraphGUI (this, new Rect (0f, 0f, scaledRect.width, scaledRect.height));

            // Draw the edges
            graphCanvas.edgeGUI.host = graphCanvas;
            graphCanvas.edgeGUI.DoEdges ();

            // Finish drawing the graph
            graphCanvas.OnGraphGUI ();

            graphCanvas.EndGraphGUI ();
            GUILayout.EndArea ();

            GUIScaleUtility.EndScale ();
        }

        private void DrawVariableInspector () {
            GUILayout.BeginArea (new Rect (0f, 16f, PanelWidth, position.height - 16f));
            GUI.Box (new Rect (0f, 0f, PanelWidth, position.height - 16f), GUIContent.none);

            try {
                variableCanvas.Draw (new Rect (0f, 0f, PanelWidth, position.height - 16f));
            } catch (System.Exception e) {
                Debug.Log (e);
            }
            GUILayout.EndArea ();
        }

        private void HandleDaniObjectSelection (Object obj) {
            if (obj != null) {
                if (inspectorTarget != null && inspectorTarget.target != obj) {
                    DestroyImmediate (inspectorTarget);
                }

                inspectorTarget = Editor.CreateEditor (obj);

                graphCanvas.selection.Clear ();

                if (obj is AINode) {

                    graphCanvas.selection.Add (graph.nodes.Where (n => (n as EditorNode).Node == obj).First ());
                } else {
                    var connectionUI = graphCanvas.edgeGUI as ConnectionUI;
                    var connection = obj as Connection;
                    var edge = graph.edges.Where (
                        e => e.fromSlotName == connection.SourceId.ToString () &&
                        e.toSlotName == connection.TargetId.ToString ()).FirstOrDefault ();

                    if (edge != null) {
                        connectionUI.ActiveEdge = edge;
                    }
                }

                Repaint ();
            }
        }

        private void HandleTemplateSelectEvent (AITemplate template) {
            graph.Template = template;
            this.template = template;

            graphCanvas.selection.Clear ();
            graphCanvas.CenterGraph ();

            EditorPrefs.SetString (TemplateIdKey, template.Id);

            if (inspectorTarget != null) {
                DestroyImmediate (inspectorTarget);
            }

            Repaint ();
        }

        private void LoadGUISkin () {
            var guids = AssetDatabase.FindAssets ("t:GUISkin");
            var skins = new GUISkin[guids.Length];

            for (var i = 0; i < guids.Length; ++i) {
                var path = AssetDatabase.GUIDToAssetPath (guids[i]);
                skins[i] = AssetDatabase.LoadAssetAtPath<GUISkin> (path);
            }

            var skin = skins.Where (s => s.name == "Default DANI Skin").FirstOrDefault ();
            Assert.IsNotNull (skin, "Editor is missing a skin!");

            graphCanvas.Skin = skin;
        }

        private Connection LookForActiveConnection () {
            var edge = (graphCanvas.edgeGUI as ConnectionUI).ActiveEdge;
            var template = graph.Template;

            if (edge == null || template == null) { return null; }

            return template.Connections.Where (
                c => c.SourceId.ToString () == edge.fromSlotName && c.TargetId.ToString () == edge.toSlotName).
            FirstOrDefault ();
        }

        /// <summary>
        /// Attempts to open the start window, if the option is enabled
        /// </summary>
        private void OpenStartWindow () {
            if (!EditorPrefs.HasKey (StartWindow.DaniAutoStartKey)) {
                EditorPrefs.SetBool (StartWindow.DaniAutoStartKey, true);
            }

            if (EditorPrefs.GetBool (StartWindow.DaniAutoStartKey)) {
                GetWindow<StartWindow> (true);
            }
        }
    }
}