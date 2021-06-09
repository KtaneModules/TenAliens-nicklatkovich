using UnityEngine;

public class SlugComponent : MonoBehaviour {
	public GameObject RealHighlight;
	public Renderer Renderer;
	public KMSelectable Selectable;

	private bool _selected;
	public bool selected {
		get { return _selected; }
		set {
			_selected = value;
			UpdateHighlight();
		}
	}

	private bool _highlighted;
	public bool highlighted {
		get { return _highlighted; }
		set {
			_highlighted = value;
			UpdateHighlight();
		}
	}

	private SlugsAndTheRiverPuzzle.Slug _data;
	public SlugsAndTheRiverPuzzle.Slug data {
		get { return _data; }
		set {
			_data = value;
			Renderer.material.color = value.color;
		}
	}

	private void Start() {
		RealHighlight.gameObject.SetActive(false);
		Selectable.OnHighlight += () => { highlighted = true; };
		Selectable.OnHighlightEnded += () => { highlighted = false; };
	}

	private void UpdateHighlight() {
		RealHighlight.gameObject.SetActive(selected || highlighted);
	}
}
