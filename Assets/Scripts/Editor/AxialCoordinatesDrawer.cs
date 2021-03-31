using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AxialCoordinates))]
public class AxialCoordinatesDrawer : PropertyDrawer
{
	public override void OnGUI(
		Rect position, SerializedProperty property, GUIContent label
	) {
		AxialCoordinates coordinates = new AxialCoordinates(
			property.FindPropertyRelative("x").intValue,
			property.FindPropertyRelative("z").intValue
		);
		position = EditorGUI.PrefixLabel(position, label);
		GUI.Label(position, coordinates.ToString());
	}
}