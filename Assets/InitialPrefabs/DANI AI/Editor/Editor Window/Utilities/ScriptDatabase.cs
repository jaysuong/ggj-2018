using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    using Action = InitialPrefabs.DANI.Action;

    internal class ScriptDatabase : ScriptableObject {
        internal MonoScript[] DaniScripts { get { return daniScripts; } }

        private MonoScript[] daniScripts;
        private Dictionary<Type, List<MonoScript>> conditionBank;

        private void OnEnable () {
            var guids = AssetDatabase.FindAssets ("t:monoscript");
            var scripts = new MonoScript[guids.Length];
            var count = 0;

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath (guid);
                scripts[count++] = AssetDatabase.LoadAssetAtPath<MonoScript> (path);
            }

            daniScripts = scripts.Where (
                (s) => {
                    var scriptClass = s.GetClass ();
                    return scriptClass != null && !scriptClass.IsAbstract &&
                        (scriptClass.IsSubclassOf (typeof (Observer)) ||
                            scriptClass.IsSubclassOf (typeof (Decision)) ||
                            scriptClass.IsSubclassOf (typeof (Action)) ||
                            scriptClass.IsSubclassOf (typeof (Variable)) ||
                            scriptClass.IsSubclassOf (typeof (Condition)));
                }).ToArray ();

            PrepareConditionScripts ();
        }

        /// <summary>
        /// Finds a condition script for the observer
        /// </summary>
        /// <param name="observer">The observer to query</param>
        public MonoScript FindConditionScript (Observer observer) {
            var obsType = observer.GetType ();
            var outputField = obsType.GetField ("output", BindingFlags.Instance | BindingFlags.NonPublic);

            if (outputField != null) {
                var fieldType = outputField.FieldType;
                List<MonoScript> conditionScripts;

                if (conditionBank.TryGetValue (fieldType, out conditionScripts)) {
                    return conditionScripts.FirstOrDefault ();
                } else {
                    return null;
                }
            }

            return null;
        }

        private void PrepareConditionScripts () {
            conditionBank = new Dictionary<Type, List<MonoScript>> ();
            var conditions = daniScripts.Where (d => d.GetClass ().IsSubclassOf (typeof (Condition)));

            foreach (var condition in conditions) {
                var field = condition.GetClass ().GetField ( 
                    "compareValue", BindingFlags.Instance | BindingFlags.NonPublic);
                var fieldType = typeof (object);

                if (field != null) {
                    fieldType = field.FieldType;
                }

                // Add the script to the condition bank
                List<MonoScript> conditionScripts;

                if (!conditionBank.TryGetValue (fieldType, out conditionScripts)) {
                    conditionScripts = new List<MonoScript> ();
                    conditionBank.Add (fieldType, conditionScripts);
                }

                conditionScripts.Add (condition);
            }
        }
    }
}