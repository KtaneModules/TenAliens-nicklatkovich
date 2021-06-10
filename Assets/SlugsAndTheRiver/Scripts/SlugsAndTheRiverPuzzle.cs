using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlugsAndTheRiverPuzzle {
	public struct Slug {
		public bool red;
		public bool green;
		public bool blue;
		public int level;
		public int id;
		public Color color { get { return new Color(red ? 1f : 0f, green ? 1f : 0f, blue ? 1f : 0f); } }
		public Slug(int id, int level) {
			this.id = id;
			this.level = level;
			this.red = level / 4 > 0;
			this.green = level % 4 / 2 > 0;
			this.blue = level % 2 > 0;
		}
		public bool conflict(Slug other) {
			return (red && other.red) || (green && other.green) || (blue && other.blue);
		}
	}

	private class SaR {
		public HashSet<Slug> south = new HashSet<Slug>();
		public HashSet<Slug> north = new HashSet<Slug>();
		public int usedEnergy = 0;
		public void transfer(int slugLevel, int usedEnergy = 8) {
			Debug.Log(string.Format("transfer {0} for {1}", slugLevel, usedEnergy));
			Slug slug = south.First(s => s.level == slugLevel);
			south.Remove(slug);
			north.Add(slug);
			this.usedEnergy += usedEnergy;
		}
		public void pull(int slugLevel, int bySlugLevel) {
			Debug.Log(string.Format("pulling {0} by {1}", slugLevel, bySlugLevel));
			Debug.Assert(north.Any(s => s.level == bySlugLevel));
			transfer(slugLevel, 8 - bySlugLevel);
		}
	}

	private static bool TwoToFive(SaR sar) {
		if (!sar.south.Any(s => s.level == 5)) return true;
		int use = 3 + 5 * (sar.south.Count(s => s.level == 2) - 1);
		int notUse = 5 * sar.south.Count(s => s.level == 2) + 2 * (sar.south.Count(s => s.level == 5) - 1);
		return use >= notUse;
	}

	public static int Generate(Slug[] slugs) {
		SaR sar = new SaR();
		foreach (Slug slug in slugs) sar.south.Add(slug);
		if (sar.south.Any(s => s.level == 6)) {
			sar.transfer(6);
			if (sar.south.Any(s => s.level == 1)) {
				do { sar.pull(1, 6); } while (sar.south.Any(s => s.level == 1));
				while (sar.south.Any(s => s.level == 6)) sar.pull(6, 1);
				if (sar.south.Any(s => s.level == 4)) {
					sar.pull(4, 1);
					while (sar.south.Any(s => s.level == 3)) sar.pull(3, 4);
					if (sar.south.Any(s => s.level == 2)) {
						sar.pull(2, 4);
						while (sar.south.Any(s => s.level == 5)) sar.pull(5, 2);
						if (sar.north.Any(s => s.level == 5)) {
							while (sar.south.Any(s => s.level == 2)) sar.pull(2, 5);
						} else while (sar.south.Any(s => s.level == 2)) sar.pull(2, 4);
					}
					while (sar.south.Any(s => s.level == 4)) {
						if (sar.north.Any(s => s.level == 3)) sar.pull(4, 3);
						else if (sar.north.Any(s => s.level == 2)) sar.pull(4, 2);
						else if (sar.north.Any(s => s.level == 1)) sar.pull(4, 1);
					}
					return sar.usedEnergy + 8 * sar.south.Count;
				}
				if (sar.south.Any(s => s.level == 2)) {
					if (TwoToFive(sar)) {
						sar.pull(2, 1);
						while (sar.south.Any(s => s.level == 5)) sar.pull(5, 2);
						if (sar.north.Any(s => s.level == 5)) {
							while (sar.south.Any(s => s.level == 2)) sar.pull(2, 5);
						} else while (sar.south.Any(s => s.level == 2)) sar.pull(2, 1);
						return sar.usedEnergy + 8 * sar.south.Count;
					}
					sar.transfer(5);
					while (sar.south.Any(s => s.level == 2)) sar.pull(2, 5);
					while (sar.south.Any(s => s.level == 5)) sar.pull(5, 2);
				}
				return sar.usedEnergy + 8 * sar.south.Count;
			}
		}
		if (sar.south.Any(s => s.level == 5) && sar.south.Any(s => s.level == 2)) {
			sar.transfer(5);
			while (sar.south.Any(s => s.level == 2)) sar.pull(2, 5);
			while (sar.south.Any(s => s.level == 5)) sar.pull(5, 2);
			if (!sar.south.Any(s => s.level == 4)) {
				while (sar.south.Any(s => s.level == 1)) sar.pull(1, 2);
				return sar.usedEnergy + 8 * sar.south.Count;
			}
			sar.pull(4, 2);
			while (sar.south.Any(s => s.level == 3)) sar.pull(3, 4);
			while (sar.south.Any(s => s.level == 1)) sar.pull(1, 4);
			if (sar.north.Any(s => s.level == 3)) {
				while (sar.south.Any(s => s.level == 4)) sar.pull(4, 3);
			} else while (sar.south.Any(s => s.level == 4)) sar.pull(4, 2);
		}
		if (sar.south.Any(s => s.level == 4)) {
			sar.transfer(4);
			while (sar.south.Any(s => s.level == 3)) sar.pull(3, 4);
			while (sar.south.Any(s => s.level == 2)) sar.pull(2, 4);
			while (sar.south.Any(s => s.level == 1)) sar.pull(1, 4);
			while (sar.south.Any(s => s.level == 4)) {
				if (sar.north.Any(s => s.level == 3)) sar.pull(4, 3);
				else if (sar.north.Any(s => s.level == 2)) sar.pull(4, 2);
				else if (sar.north.Any(s => s.level == 1)) sar.pull(4, 1);
			}
		}
		return sar.usedEnergy + 8 * sar.south.Count;
	}

	public readonly int initialEnergy;
	public readonly int slugsCount;

	public int energy;
	public int[] initialLevels;
	public HashSet<Slug> northSlugs;
	public HashSet<Slug> southSlugs;

	public static int GetRandomLevel() {
		if (Random.Range(0, 3) == 0) {
			if (Random.Range(0, 3) == 0) return 6;
			if (Random.Range(0, 2) == 0) return 5;
			return 3;
		}
		if (Random.Range(0, 3) == 0) return 4;
		if (Random.Range(0, 2) == 0) return 2;
		return 1;
	}

	public SlugsAndTheRiverPuzzle(int slugsCount) {
		this.slugsCount = slugsCount;
		southSlugs = new HashSet<Slug>(Enumerable.Range(0, slugsCount).Select(i => new Slug(i, GetRandomLevel())));
		northSlugs = new HashSet<Slug>();
		initialEnergy = Generate(southSlugs.ToArray());
		energy = initialEnergy;
	}

	public bool transfer(int slugId) {
		Slug slug = southSlugs.First(s => s.id == slugId);
		southSlugs.Remove(slug);
		northSlugs.Add(slug);
		energy -= 8;
		return southSlugs.Count == 0 && energy >= 0;
	}

	public bool pull(int slugId, int bySlugId) {
		Slug slug = southSlugs.First(s => s.id == slugId);
		Slug bySlug = northSlugs.First(s => s.id == bySlugId);
		southSlugs.Remove(slug);
		northSlugs.Add(slug);
		energy -= 8 - bySlug.level;
		return southSlugs.Count == 0 && energy >= 0;
	}

	public void reset() {
		foreach (Slug slug in northSlugs) southSlugs.Add(slug);
		northSlugs = new HashSet<Slug>();
		energy = initialEnergy;
	}
}
