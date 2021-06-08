using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlugsAndTheRiverModule : MonoBehaviour {
	private readonly Vector3 SLUGS_OFFSET = new Vector3(.025f, .001f, .025f);

	private static Color SlugLevelToColor(int level) {
		return new Color(level / 4, level % 4 / 2, level % 2);
	}

	public GameObject SlugsContainer;
	public KMSelectable Selectable;
	public SlugComponent SlugPrefab;

	private int[] slugsLevels;

	private void Start() {
		slugsLevels = Enumerable.Range(0, 10).Select(_ => Random.Range(1, 7)).ToArray();
		List<KMSelectable> children = new List<KMSelectable>();
		for (int i = 0; i < 10; i++) {
			SlugComponent slug = Instantiate(SlugPrefab);
			slug.transform.parent = SlugsContainer.transform;
			slug.transform.localPosition = new Vector3((i % 5 - 2f) * SLUGS_OFFSET.x, SLUGS_OFFSET.y, (i / 5 - .5f) * SLUGS_OFFSET.z - .04f);
			slug.transform.localScale = Vector3.one;
			slug.transform.localRotation = Quaternion.identity;
			slug.color = SlugLevelToColor(slugsLevels[i]);
			slug.Selectable.Parent = Selectable;
			children.Add(slug.Selectable);
		}
		Selectable.Children = children.ToArray();
		Selectable.UpdateChildren();
	}
}
