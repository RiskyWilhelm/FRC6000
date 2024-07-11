using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// Used for support editing multiple objects
[CustomEditor(typeof(DirectionRelativeParallax))]
[CanEditMultipleObjects]
public sealed partial class DirectionRelativeParallaxEditor : Editor
{
	public override VisualElement CreateInspectorGUI()
	{
		var rootElement = new VisualElement();
		InspectorElement.FillDefaultInspector(rootElement, serializedObject, this);
		return rootElement;
	}
}