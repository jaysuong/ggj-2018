using InitialPrefabs.DANI;
using System.Linq;
using UnityEditor;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// A utility class that performs queries involving searches in the AssetDatabase
    /// </summary>
    public static class AssetSearcher {

        /// <summary>
        /// Retrieves all MonoScript instances that are related to the class
        /// </summary>
        /// <typeparam name="T">Type to look for</typeparam>
        /// <returns>An array of MonoScripts</returns>
        public static MonoScript[] GetMonoScripts<T>() {
            var guids = AssetDatabase.FindAssets("t:MonoScript");
            var scripts = new MonoScript[guids.Length];

            for(var i = 0; i < scripts.Length; ++i) {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                scripts[i] = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            }

            return ( from s in scripts
                     where s.GetClass() != null
                     where s.GetClass().IsSubclassOf(typeof(T)) || s.GetClass() == typeof(T)
                     where !s.GetClass().IsAbstract
                     select s ).ToArray();
        }

        /// <summary>
        /// Retrieves all MonoScript instances that are related to the class
        /// </summary>
        /// <typeparam name="T">Type to look for</typeparam>
        /// <param name="canSort">Will the results be sorted alphabetically?</param>
        /// <returns>An array of MonoScripts</returns>
        public static MonoScript[] GetMonoScripts<T>(bool canSort) {
            var guids = AssetDatabase.FindAssets("t:MonoScript");
            var scripts = new MonoScript[guids.Length];

            for(var i = 0; i < scripts.Length; ++i) {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                scripts[i] = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            }

            var results =  ( from s in scripts
                     where s.GetClass() != null
                     where s.GetClass().IsSubclassOf(typeof(T))
                     where !s.GetClass().IsAbstract
                     select s ).ToArray();

            if(canSort) {
                results = results.OrderBy(r => r.GetClass().Name).ToArray();
            }

            return results;
        }

        /// <summary>
        /// Looks for any attributes named "ContextMenuPath" and outputs
        /// a string path.
        /// </summary>
        /// <param name="script">The monoscript to search with</param>
        /// <returns>A local path.  Empty string otherwise.</returns>
        public static string GetLocalMenuPath(MonoScript script) {
            var type = script.GetClass();

            if(type == null) {
                return string.Empty;
            }

            var attributes = type.GetCustomAttributes(true);
            foreach(var attr in attributes) {
                if(attr is CustomMenuPathAttribute) {
                    return ( (CustomMenuPathAttribute)attr ).path;
                }
            }

            return string.Empty;
        }
    }
}
