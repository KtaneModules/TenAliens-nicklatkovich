using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TenAliensPuzzle {
	public struct Alien {
		public bool red;
		public bool green;
		public bool blue;
		public int level;
		public int id;
		public Color color { get { return new Color(red ? 1f : 0f, green ? 1f : 0f, blue ? 1f : 0f); } }
		public Alien(int id, int level) {
			this.id = id;
			this.level = level;
			this.red = level / 4 > 0;
			this.green = level % 4 / 2 > 0;
			this.blue = level % 2 > 0;
		}
		public bool conflict(Alien other) {
			return (red && other.red) || (green && other.green) || (blue && other.blue);
		}
	}

	private class SaR {
		public HashSet<Alien> south = new HashSet<Alien>();
		public HashSet<Alien> north = new HashSet<Alien>();
		public int usedEnergy = 0;
		public void transfer(int alienLevel, int usedEnergy = 8) {
			// Debug.Log(string.Format("transfer {0} for {1}", alienLevel, usedEnergy));
			Alien alien = south.First(s => s.level == alienLevel);
			south.Remove(alien);
			north.Add(alien);
			this.usedEnergy += usedEnergy;
		}
		public void pull(int alienLevel, int byAlienLevel) {
			// Debug.Log(string.Format("pulling {0} by {1}", alienLevel, byAlienLevel));
			Debug.Assert(north.Any(s => s.level == byAlienLevel));
			transfer(alienLevel, 8 - byAlienLevel);
		}
	}

	private static bool TwoToFive(SaR sar) {
		if (!sar.south.Any(s => s.level == 5)) return true;
		int use = 3 + 5 * (sar.south.Count(s => s.level == 2) - 1);
		int notUse = 5 * sar.south.Count(s => s.level == 2) + 2 * (sar.south.Count(s => s.level == 5) - 1);
		return use >= notUse;
	}

	public static int Generate(Alien[] aliens) {
		SaR sar = new SaR();
		foreach (Alien alien in aliens) sar.south.Add(alien);
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
	public readonly int aliensCount;

	public int energy;
	public int[] initialLevels;
	public HashSet<Alien> northAliens;
	public HashSet<Alien> southAliens;

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

	public TenAliensPuzzle(int aliensCount) {
		this.aliensCount = aliensCount;
		southAliens = new HashSet<Alien>(Enumerable.Range(0, aliensCount).Select(i => new Alien(i, GetRandomLevel())));
		northAliens = new HashSet<Alien>();
		initialEnergy = Generate(southAliens.ToArray());
		energy = initialEnergy;
	}

	public bool transfer(int alienId) {
		Alien alien = southAliens.First(s => s.id == alienId);
		southAliens.Remove(alien);
		northAliens.Add(alien);
		energy -= 8;
		return southAliens.Count == 0 && energy >= 0;
	}

	public bool pull(int alienId, int byAlienId) {
		Alien alien = southAliens.First(s => s.id == alienId);
		Alien byAlien = northAliens.First(s => s.id == byAlienId);
		southAliens.Remove(alien);
		northAliens.Add(alien);
		energy -= 8 - byAlien.level;
		return southAliens.Count == 0 && energy >= 0;
	}

	public void reset() {
		foreach (Alien alien in northAliens) southAliens.Add(alien);
		northAliens = new HashSet<Alien>();
		energy = initialEnergy;
	}
}
