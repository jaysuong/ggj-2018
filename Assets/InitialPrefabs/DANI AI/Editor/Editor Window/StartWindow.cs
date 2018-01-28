using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    public class StartWindow : EditorWindow {

        private bool ShouldAutoStart {
            get { return EditorPrefs.GetBool (DaniAutoStartKey); }
            set { EditorPrefs.SetBool (DaniAutoStartKey, value); }
        }

        internal const string DaniAutoStartKey = "DANI_AUTO_START_WINDOW";
        internal const string StartSkinName = "DANI Start Skin";
        internal const string TitleName = "Dani AI v1.0";

        private const string ManualUrl = "http://www.initialprefabs.com";
        private const string TutorialUrl = "http://www.initialprefabs.com";
        private const string ScriptingAPIUrl = "http://www.initialprefabs.com";
        private const string ContactUrl = "mailto:info@initialprefabs.com";

        private GUISkin skin;
        private GUILayoutOption width;
        private GUILayoutOption height;


        [MenuItem ("Tools/InitialPrefabs/Dani AI/Start Menu")]
        public static void Open () {
            GetWindow<StartWindow> (true);
        }

        private void OnEnable () {
            AttemptToLoadSkin ();
            width = GUILayout.Width (100f);
            height = GUILayout.Height (100f);

            minSize = new Vector2 (360f, 380f);
            maxSize = new Vector2 (360f, 380f);

            titleContent = new GUIContent (TitleName);
        }

        private void OnGUI () {
            using (var mainLayout = new EditorGUILayout.VerticalScope ()) {
                // Draw the logo
                var logo = skin.FindStyle ("Logo");
                if (logo != null) {
                    GUILayout.Box (GUIContent.none, logo, GUILayout.Height (90f));
                }
                GUILayout.Space (5f);

                // Draw the Dani elements
                using (var daniLayout = new EditorGUILayout.HorizontalScope ()) {
                    EditorGUILayout.Space ();
                    if (GUILayout.Button ("Manual", skin.FindStyle ("Manual"), width, height)) {
                        Application.OpenURL (ManualUrl);
                    }
                    GUILayout.Space (5f);

                    if (GUILayout.Button ("Tutorials", skin.FindStyle ("Tutorial"), width, height)) {
                        Application.OpenURL (TutorialUrl);
                    }
                    GUILayout.Space (5f);

                    if (GUILayout.Button ("Scripting API", skin.FindStyle ("API"), width, height)) {
                        Application.OpenURL (ScriptingAPIUrl);
                    }
                    EditorGUILayout.Space ();
                }

                GUILayout.Space (30f);

                using (var contactLayout = new EditorGUILayout.HorizontalScope ()) {
                    EditorGUILayout.Space ();

                    // Draw the contact button
                    if (GUILayout.Button ("Contact", skin.FindStyle ("Contact"), width, height)) {
                        Application.OpenURL (ContactUrl);
                    }

                    EditorGUILayout.Space ();
                }

                GUILayout.Space (30f);
                // Draw the checkbox for autostart
                using (var changeCheck = new EditorGUI.ChangeCheckScope ()) {
                    var autoStart = ShouldAutoStart;

                    autoStart = EditorGUILayout.Toggle ("Show on startup", autoStart);

                    if (changeCheck.changed) {
                        ShouldAutoStart = autoStart;
                    }
                }
            }
        }

        private void AttemptToLoadSkin () {
            var guids = AssetDatabase.FindAssets ("t:GUISkin");

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath (guid);
                var skin = AssetDatabase.LoadAssetAtPath<GUISkin> (path);

                if (skin != null && skin.name == StartSkinName) {
                    this.skin = skin;
                    break;
                }
            }
        }

    }
}