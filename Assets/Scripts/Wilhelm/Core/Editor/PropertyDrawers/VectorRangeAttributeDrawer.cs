using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(VectorRangeAttribute), true)]
public sealed class VectorRangeAttributeDrawer : PropertyDrawer
{
	private VectorRangeAttribute Attribute => (VectorRangeAttribute)attribute;


	// Initialize
	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	{
		// Create elements
		var propertyElement = new PropertyField(property);

		// Register callbacks
		propertyElement.RegisterValueChangeCallback(OnVectorChanged);

		return propertyElement;
	}

	private void OnVectorChanged(SerializedPropertyChangeEvent evt)
	{
		object vectorValue = evt.changedProperty.propertyType switch
		{
			SerializedPropertyType.Vector2 => (Vector2)Clamp(evt.changedProperty.vector2Value),
			SerializedPropertyType.Vector3 => (Vector3)Clamp(evt.changedProperty.vector3Value),
			SerializedPropertyType.Vector4 => Clamp(evt.changedProperty.vector4Value),
			SerializedPropertyType.Vector2Int => (Vector2Int)Clamp((Vector3Int)evt.changedProperty.vector2IntValue),
			SerializedPropertyType.Vector3Int => Clamp(evt.changedProperty.vector3IntValue),
			_ => throw new Exception(string.Format("{0} not compitable with type {1}", nameof(VectorRangeAttribute), evt.changedProperty.propertyType)),
		};

		evt.changedProperty.boxedValue = vectorValue;
		evt.changedProperty.serializedObject.ApplyModifiedProperties();
	}

	private Vector4 Clamp(Vector4 a)
	{
		a.x = Math.Clamp(a.x, Attribute.clamped.minX, Attribute.clamped.maxX);
		a.y = Math.Clamp(a.y, Attribute.clamped.minY, Attribute.clamped.maxY);
		a.z = Math.Clamp(a.z, Attribute.clamped.minZ, Attribute.clamped.maxZ);
		a.w = Math.Clamp(a.w, Attribute.clamped.minW, Attribute.clamped.maxW);
		return a;
	}

	private Vector3Int Clamp(Vector3Int a)
	{
		a.x = (int)Math.Clamp(a.x, Attribute.clamped.minX, Attribute.clamped.maxX);
		a.y = (int)Math.Clamp(a.y, Attribute.clamped.minY, Attribute.clamped.maxY);
		a.z = (int)Math.Clamp(a.z, Attribute.clamped.minZ, Attribute.clamped.maxZ);
		return a;
	}
}