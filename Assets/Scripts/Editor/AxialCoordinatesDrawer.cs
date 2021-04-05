using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(VertexCoordinates))]
public class AxialCoordinatesDrawer : PropertyDrawer
{
	public override void OnGUI(
		Rect position, SerializedProperty property, GUIContent label
	) {
		VertexCoordinates coordinates = new VertexCoordinates(
			property.FindPropertyRelative("x").intValue,
			property.FindPropertyRelative("z").intValue
		);
		position = EditorGUI.PrefixLabel(position, label);
		GUI.Label(position, coordinates.ToString());
	}
}