using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InitialPrefabs.DANI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InitialPrefabs.DANIEditor {
	/// <summary>
	/// A canvas for drawing variables
	/// </summary>
	public class VariableCanvas : ScriptableObject {

		public EditorGraph Graph { get; set; }

		public DaniEditorWindow Window { get; set; }

		private const float ElementHeight = 24f;
		private const string NameProp = "m_Name";
		private const string ScriptProp = "m_Script";
		private const string VariablePropName = "m_variables";
		private const string ValuePropName = "value";

		private Vector2 scroll;
		private AITemplate template;
		private ReorderableList variableList;
		private int selectedIndex;

		private GUIStyle headerStyle;
		private GUIStyle valueStyle;
		private GUIStyle nonSerializedStyle;

		private Dictionary<Object, SerializedObject> serializedVariables;

		private void OnEnable () {
			selectedIndex = -1;
			serializedVariables = new Dictionary<Object, SerializedObject> ();
			variableList = new ReorderableList (new List<Variable> (), typeof (Variable));

			valueStyle = new GUIStyle ();
			valueStyle.alignment = TextAnchor.MiddleRight;
			valueStyle.clipping = TextClipping.Clip;

			headerStyle = new GUIStyle ();
			headerStyle.alignment = TextAnchor.MiddleLeft;
			headerStyle.fontStyle = FontStyle.Bold;
			headerStyle.clipping = TextClipping.Clip;

			nonSerializedStyle = new GUIStyle ();
			nonSerializedStyle.fontSize = 10;
			nonSerializedStyle.normal.textColor = Color.gray;
			nonSerializedStyle.alignment = TextAnchor.MiddleLeft;
		}

		public void Draw (Rect rect) {
			PrepareVariableList ();
			using (var scope = new EditorGUILayout.ScrollViewScope (scroll)) {

				if (Graph.Template != null) {
					try {
						Graph.SerializedTemplate.Update ();

						variableList.DoLayoutList ();

						Graph.SerializedTemplate.ApplyModifiedProperties ();
					} catch (System.Exception e) {
						Debug.Log (e);
					}
				} else {
					EditorGUILayout.HelpBox (
						"Select an AITemplate or create a new one to see attached varibles",
						MessageType.Warning);
				}

				scroll = scope.scrollPosition;
			}
		}

		private SerializedObject GetSerializedVariable (Variable variable) {
			SerializedObject obj;

			if (!serializedVariables.TryGetValue (variable, out obj)) {
				obj = new SerializedObject (variable);
				serializedVariables.Add (variable, obj);
			}

			return obj;
		}

		/// <summary>
		/// Checks the current state of the list and regenerates a new list if the template has changed or
		/// the list is invalid
		/// </summary>
		private void PrepareVariableList () {
			var candidateTemplate = Graph.Template;

			if (template != candidateTemplate) {
				template = candidateTemplate;

				var p = Graph.SerializedTemplate.FindProperty (VariablePropName);
				variableList = new ReorderableList (Graph.SerializedTemplate, p, true, false, true, true);

				variableList.headerHeight = 0f;
				variableList.elementHeight = ElementHeight;

				variableList.drawElementCallback = (Rect r, int index, bool active, bool focused) => {
					var element = p.GetArrayElementAtIndex (index);
					var variable = element.objectReferenceValue as Variable;
					var serializedElement = GetSerializedVariable (variable);
					serializedElement.Update ();

					// Draw the header values
					var nameRect = new Rect (r.x, r.y, r.width * 0.7f, variableList.elementHeight);
					var valueRect = new Rect (r.x + r.width * 0.7f, r.y, r.width * 0.3f, variableList.elementHeight);
					EditorGUI.LabelField (nameRect, serializedElement.FindProperty (NameProp).stringValue, headerStyle);
					EditorGUI.LabelField (valueRect, variable.ToString(), valueStyle);

					if (index == selectedIndex) {
						var subElement = serializedElement.GetIterator ();
						var offset = variableList.elementHeight;

						subElement.Next (true);

						// Draw the name prop
						EditorGUI.PropertyField (
							new Rect (r.x, r.y + offset, r.width, EditorGUIUtility.singleLineHeight),
							serializedElement.FindProperty (NameProp),
							new GUIContent ("Name"));
						offset += EditorGUIUtility.singleLineHeight;

						// Draw the value prop
						if (IsValueSerializeable (serializedElement)) {
							var valueProp = serializedElement.FindProperty (ValuePropName);
							var height = EditorGUI.GetPropertyHeight (valueProp, new GUIContent ("Value"), true);
							EditorGUI.PropertyField (
								new Rect (r.x, r.y + offset, r.width, height),
								valueProp, true);

							offset += height;
						} else {
							var rect = new Rect (r.x, r.y + offset, r.width, EditorGUIUtility.singleLineHeight);
							EditorGUI.LabelField (rect, "Value");

							rect.x += 150f;
							rect.width = 150f;
							EditorGUI.LabelField (rect,
								variable.GetValue () != null ? variable.GetValue ().ToString () : "(null)",
								nonSerializedStyle);

							offset += EditorGUIUtility.singleLineHeight;
						}

						// Draw the rest of the varibles
						while (subElement.NextVisible (false)) {
							if (subElement.name != ScriptProp && subElement.name != ValuePropName) {
								var height = EditorGUI.GetPropertyHeight (subElement, GUIContent.none, true);
								EditorGUI.PropertyField (new Rect (r.x, r.y + offset, r.width, height), subElement, true);

								offset += height;
							}
						}

						serializedElement.ApplyModifiedProperties ();
					}
				};

				variableList.onAddDropdownCallback = (Rect r, ReorderableList list) => {
					ShowVariableContextMenu (r);
				};

				variableList.onRemoveCallback = (ReorderableList list) => {
					var element = p.GetArrayElementAtIndex (list.index);
					var variable = element.objectReferenceValue as Variable;
					Undo.SetCurrentGroupName (string.Format ("Delete `{0}` variable", element.objectReferenceValue.name));

					element.objectReferenceValue = null;
					p.DeleteArrayElementAtIndex (list.index);

					Graph.SerializedTemplate.ApplyModifiedProperties ();

					Undo.RecordObject (variable, string.Empty);
					Undo.DestroyObjectImmediate (variable);

					Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());
				};

				variableList.onSelectCallback = (ReorderableList list) => {
					selectedIndex = list.index;
				};

				variableList.elementHeightCallback = (int index) => {
					var height = variableList.elementHeight;

					if (index == selectedIndex) {
						var variable = p.GetArrayElementAtIndex (index).objectReferenceValue as Variable;
						var prop = GetSerializedVariable (variable).GetIterator ();
						prop.Next (true);

						while (prop.NextVisible (false)) {
							height += EditorGUI.GetPropertyHeight (prop,
								prop.name == ValuePropName ? new GUIContent ("Value: ") : GUIContent.none,
								true);

							// if it's a non serialized variable, then insert an empty space
							if (!IsValueSerializeable (GetSerializedVariable (variable))) {
								height += variableList.elementHeight;
							}
						}
					}

					return height;
				};

				serializedVariables.Clear ();
				Window.Repaint ();
			}
		}

		/// <summary>
		/// Draws a context menu for adding new variables
		/// </summary>
		/// <param name="rect">The rectangle to draw in</param>
		private void ShowVariableContextMenu (Rect rect) {
			var menu = new GenericMenu ();
			var variableScripts = AssetSearcher.GetMonoScripts<Variable> ();
			var workList = new List<MonoScript> ();
			var looseList = new List<MonoScript> ();

			foreach (var script in variableScripts) {
				var nestedPath = AssetSearcher.GetLocalMenuPath (script);

				if (!string.IsNullOrEmpty (nestedPath)) {
					workList.Add (script);
				} else {
					looseList.Add (script);
				}
			}

			workList = workList.OrderBy (f => AssetSearcher.GetLocalMenuPath (f)).ToList ();
			looseList = looseList.OrderBy (l => l.name).ToList ();
			workList.AddRange (looseList);

			foreach (var script in workList) {
				var path = AssetSearcher.GetLocalMenuPath (script);

				menu.AddItem (new GUIContent (!string.IsNullOrEmpty (path) ? path : script.name),
					false,
					() => {
						AttachVariable (script, Graph.Template);
					});
			}

			menu.DropDown (rect);
		}

		/// <summary>
		/// Attaches a variable to the template
		/// </summary>
		/// <param name="script">The variable's script</param>
		/// <param name="template">The template to attach to</param>
		private void AttachVariable (MonoScript script, AITemplate template) {
			Undo.SetCurrentGroupName (string.Format ("Create `{0}` variable", script.name));

			// Create the variable
			var variable = CreateInstance (script.GetClass ()) as Variable;
			variable.name = RegexTools.GetReadableText (script.GetClass ().Name);
			variable.hideFlags = HideFlags.HideInHierarchy;

			Undo.RecordObject (template, string.Empty);
			Undo.RegisterCreatedObjectUndo (variable, string.Empty);

			AssetDatabase.AddObjectToAsset (variable, template);

			// Insert the variable into the template
			var serializedTemplate = new SerializedObject (template);

			var array = serializedTemplate.FindProperty (VariablePropName);
			array.arraySize++;
			array.GetArrayElementAtIndex (array.arraySize - 1).objectReferenceValue = variable;

			serializedTemplate.ApplyModifiedProperties ();

			Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());
		}

		private bool IsValueSerializeable (SerializedObject serializedVariable) {
			return serializedVariable.FindProperty (ValuePropName) != null;
		}
	}
}