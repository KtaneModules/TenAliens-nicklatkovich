using UnityEngine;

public class SlugComponent : MonoBehaviour {
	public Renderer Renderer;
	public KMSelectable Selectable;

	private Color _color;
	public Color color {
		get { return _color; }
		set {
			_color = value;
			UpdateColor();
		}
	}

	private void UpdateColor() {
		Renderer.material.color = _color;
	}
}
