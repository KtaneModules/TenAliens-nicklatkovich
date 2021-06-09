﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlugsAndTheRiverModule : MonoBehaviour {
	private const float HOLDING = .5f;
	private const float SLUG_TRANSFER_LENGTH = .075f;
	private readonly Vector3 SLUGS_OFFSET = new Vector3(.025f, .001f, .025f);

	private static Color SlugLevelToColor(int level) {
		return new Color(level / 4, level % 4 / 2, level % 2);
	}

	public GameObject SlugsContainer;
	public TextMesh Energy;
	public KMSelectable Selectable;
	public KMSelectable ResetButton;
	public KMBombModule Module;
	public SlugComponent SlugPrefab;

	private bool solved;
	private bool resetHolded;
	private float holdingTimer;
	private SlugComponent holdedSlug;
	private SlugComponent selectedSlug;
	private SlugComponent[] slugs;
	private SlugsAndTheRiverPuzzle puzzle;

	private void Start() {
		puzzle = new SlugsAndTheRiverPuzzle(10);
		List<KMSelectable> children = new List<KMSelectable>();
		List<SlugComponent> slugsList = new List<SlugComponent>();
		for (int i = 0; i < 10; i++) {
			SlugComponent slug = Instantiate(SlugPrefab);
			slug.data = puzzle.southSlugs.First(s => s.id == i);
			slugsList.Add(slug);
			slug.transform.parent = SlugsContainer.transform;
			SetSlugInitialPosition(slug);
			slug.transform.localScale = Vector3.one;
			slug.transform.localRotation = Quaternion.identity;
			slug.Selectable.OnInteract += () => { HoldSlug(slug); return false; };
			slug.Selectable.OnInteractEnded += () => UnholdSlug();
			slug.Selectable.Parent = Selectable;
			children.Add(slug.Selectable);
		}
		slugs = slugsList.ToArray();
		children.Add(ResetButton);
		Selectable.Children = children.ToArray();
		Selectable.UpdateChildren();
		Energy.text = puzzle.energy.ToString();
		ResetButton.OnInteract += () => { HoldReset(); return false; };
		ResetButton.OnInteractEnded += () => UnholdReset();
	}

	private void Update() {
		if (holdingTimer > 0) {
			if (holdingTimer < Time.time) {
				if (holdedSlug != null) UnholdSlug(true);
				else if (resetHolded) UnholdReset();
				else holdingTimer = 0;
			}
		}
	}

	private void HoldSlug(SlugComponent slug) {
		holdingTimer = Time.time + HOLDING;
		holdedSlug = slug;
	}

	private void HoldReset() {
		holdingTimer = Time.time + HOLDING;
		resetHolded = true;
	}

	private void UnholdReset(bool forceHold = false) {
		if (!resetHolded) return;
		bool tap = forceHold ? false : holdingTimer > Time.time;
		holdingTimer = 0;
		if (tap) return;
		if (puzzle.northSlugs.Count == 0) return;
		foreach (SlugComponent slug in slugs) SetSlugInitialPosition(slug);
		if (solved && puzzle.southSlugs.Count == 0 && puzzle.energy >= 0) {
			puzzle = new SlugsAndTheRiverPuzzle(10);
			foreach (SlugsAndTheRiverPuzzle.Slug data in puzzle.southSlugs) slugs[data.id].data = data;
		} else puzzle.reset();
		Energy.text = puzzle.energy.ToString();
		Strike();
	}

	private void UnholdSlug(bool forceHold = false) {
		if (holdedSlug == null) return;
		bool tap = forceHold ? false : holdingTimer > Time.time;
		holdingTimer = 0;
		if (tap) {
			if (selectedSlug == null) {
				if (!puzzle.northSlugs.Any(s => s.id == holdedSlug.data.id)) return;
				selectedSlug = holdedSlug;
				selectedSlug.selected = true;
				holdedSlug = null;
				return;
			}
			if (selectedSlug == holdedSlug) {
				selectedSlug.selected = false;
				selectedSlug = null;
				holdedSlug = null;
				return;
			}
			if (puzzle.northSlugs.Any(s => s.id == holdedSlug.data.id)) {
				selectedSlug.selected = false;
				selectedSlug = holdedSlug;
				selectedSlug.selected = true;
				holdedSlug = null;
				return;
			}
			if (holdedSlug.data.conflict(selectedSlug.data)) {
				selectedSlug.selected = false;
				selectedSlug = null;
				holdedSlug = null;
				Strike();
				return;
			}
			puzzle.pull(holdedSlug.data.id, selectedSlug.data.id);
			selectedSlug.selected = false;
			selectedSlug = null;
			holdedSlug.transform.localPosition += Vector3.forward * SLUG_TRANSFER_LENGTH;
			holdedSlug = null;
			Energy.text = puzzle.energy.ToString();
			if (puzzle.southSlugs.Count == 0 && puzzle.energy >= 0) Solve();
			return;
		}
		if (puzzle.southSlugs.Any(s => s.id == holdedSlug.data.id)) {
			puzzle.transfer(holdedSlug.data.id);
			holdedSlug.transform.localPosition += Vector3.forward * SLUG_TRANSFER_LENGTH;
			holdedSlug = null;
			Energy.text = puzzle.energy.ToString();
			if (puzzle.southSlugs.Count == 0 && puzzle.energy >= 0) Solve();
		}
	}

	private void SetSlugInitialPosition(SlugComponent slug) {
		int id = slug.data.id;
		slug.transform.localPosition = new Vector3((id % 5 - 2f) * SLUGS_OFFSET.x, SLUGS_OFFSET.y, (id / 5 - .5f) * SLUGS_OFFSET.z - .04f);
	}

	private void Strike() {
		if (solved) return;
		Module.HandleStrike();
	}

	private void Solve() {
		if (solved) return;
		solved = true;
		Module.HandlePass();
	}
}
