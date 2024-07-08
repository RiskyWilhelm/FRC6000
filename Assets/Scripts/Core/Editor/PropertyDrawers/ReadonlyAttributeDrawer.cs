using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ReadonlyAttribute))]
public sealed class ReadonlyAttributeDrawer : PropertyDrawer
{
	// Initialize
	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	{
		var createdElement = new PropertyField(property);
		createdElement.SetEnabled(false);
		return createdElement;
	}

}