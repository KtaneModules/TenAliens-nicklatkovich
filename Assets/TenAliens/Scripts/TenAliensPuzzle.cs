using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TenAliensPuzzle {
	public static readonly Dictionary<int, string> aliensNames = new Dictionary<int, string>() {
		{ 1, "blue" },
		{ 2, "green" },
		{ 3, "cyan" },
		{ 4, "red" },
		{ 5, "magenta" },
		{ 6, "yellow" },
	};

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
		public readonly List<KeyValuePair<Alien, Alien?>> teleportsHistory = new List<KeyValuePair<Alien, Alien?>>();
		public void transfer(int alienLevel) {
			Alien alien = _transfer(alienLevel, 8);
			teleportsHistory.Add(new KeyValuePair<Alien, Alien?>(alien, null));
		}
		public void pull(int alienLevel, int byAlienLevel) {
			Alien northAlien = north.First(s => s.level == byAlienLevel);
			Alien transferedAlien = _transfer(alienLevel, 8 - byAlienLevel);
			teleportsHistory.Add(new KeyValuePair<Alien, Alien?>(transferedAlien, northAlien));
		}
		private Alien _transfer(int alienLevel, int usedEnergy) {
			Alien alien = south.First(s => s.level == alienLevel);
			south.Remove(alien);
			north.Add(alien);
			this.usedEnergy += usedEnergy;
			return alien;
		}
		public void solve() {
			if (south.Any(s => s.level == 6)) {
				transfer(6);
				if (south.Any(s => s.level == 1)) {
					do { pull(1, 6); } while (south.Any(s => s.level == 1));
					while (south.Any(s => s.level == 6)) pull(6, 1);
					if (south.Any(s => s.level == 4)) {
						pull(4, 1);
						while (south.Any(s => s.level == 3)) pull(3, 4);
						if (south.Any(s => s.level == 2)) {
							pull(2, 4);
							while (south.Any(s => s.level == 5)) pull(5, 2);
							if (north.Any(s => s.level == 5)) {
								while (south.Any(s => s.level == 2)) pull(2, 5);
							} else while (south.Any(s => s.level == 2)) pull(2, 4);
						}
						while (south.Any(s => s.level == 4)) {
							if (north.Any(s => s.level == 3)) pull(4, 3);
							else if (north.Any(s => s.level == 2)) pull(4, 2);
							else if (north.Any(s => s.level == 1)) pull(4, 1);
						}
						return;
					}
					if (south.Any(s => s.level == 2)) {
						if (TwoToFive(this)) {
							pull(2, 1);
							while (south.Any(s => s.level == 5)) pull(5, 2);
							if (north.Any(s => s.level == 5)) {
								while (south.Any(s => s.level == 2)) pull(2, 5);
							} else while (south.Any(s => s.level == 2)) pull(2, 1);
							return;
						}
						transfer(5);
						while (south.Any(s => s.level == 2)) pull(2, 5);
						while (south.Any(s => s.level == 5)) pull(5, 2);
					}
					return;
				}
			}
			if (south.Any(s => s.level == 5) && south.Any(s => s.level == 2)) {
				transfer(5);
				while (south.Any(s => s.level == 2)) pull(2, 5);
				while (south.Any(s => s.level == 5)) pull(5, 2);
				if (!south.Any(s => s.level == 4)) {
					while (south.Any(s => s.level == 1)) pull(1, 2);
					return;
				}
				pull(4, 2);
				while (south.Any(s => s.level == 3)) pull(3, 4);
				while (south.Any(s => s.level == 1)) pull(1, 4);
				if (north.Any(s => s.level == 3)) {
					while (south.Any(s => s.level == 4)) pull(4, 3);
				} else while (south.Any(s => s.level == 4)) pull(4, 2);
			}
			if (south.Any(s => s.level == 4)) {
				transfer(4);
				while (south.Any(s => s.level == 3)) pull(3, 4);
				while (south.Any(s => s.level == 2)) pull(2, 4);
				while (south.Any(s => s.level == 1)) pull(1, 4);
				while (south.Any(s => s.level == 4)) {
					if (north.Any(s => s.level == 3)) pull(4, 3);
					else if (north.Any(s => s.level == 2)) pull(4, 2);
					else if (north.Any(s => s.level == 1)) pull(4, 1);
				}
			}
		}
	}

	private static bool TwoToFive(SaR sar) {
		if (!sar.south.Any(s => s.level == 5)) return true;
		int use = 3 + 5 * (sar.south.Count(s => s.level == 2) - 1);
		int notUse = 5 * sar.south.Count(s => s.level == 2) + 2 * (sar.south.Count(s => s.level == 5) - 1);
		return use >= notUse;
	}

	public static int Generate(Alien[] aliens, Action<string> log) {
		SaR sar = new SaR();
		foreach (Alien alien in aliens) sar.south.Add(alien);
		sar.solve();
		foreach (Alien alien in new HashSet<Alien>(sar.south)) sar.transfer(alien.level);
		log(string.Format("   Energy level: {0}", sar.usedEnergy));
		log("Solution:");
		int loggingTotalEnergyUsed = 0;
		foreach (KeyValuePair<Alien, Alien?> pair in sar.teleportsHistory) {
			Alien alien = pair.Key;
			loggingTotalEnergyUsed += pair.Value == null ? 8 : 8 - pair.Value.Value.level;
			if (pair.Value == null) {
				log(string.Format("   Self teleport by alien #{0} ({1}). Used energy: {2}", alien.id + 1, aliensNames[alien.level], loggingTotalEnergyUsed));
			} else {
				Alien other = pair.Value.Value;
				log(string.Format("   Teleport alien #{0} ({1}) by alien #{2} ({3}). Used energy: {4}", alien.id + 1, aliensNames[alien.level], other.id + 1,
					aliensNames[other.level], loggingTotalEnergyUsed));
			}
		}
		return sar.usedEnergy;
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

	public TenAliensPuzzle(int aliensCount, Action<string> log = null) {
		if (log == null) log = (s) => Debug.LogFormat("<Ten Aliens> {0}", s);
		this.aliensCount = aliensCount;
		southAliens = new HashSet<Alien>(Enumerable.Range(0, aliensCount).Select(i => new Alien(i, GetRandomLevel())));
		northAliens = new HashSet<Alien>();
		log("Generation:");
		foreach (Alien alien in southAliens) log(string.Format("   Alien #{0} is {1}", alien.id + 1, aliensNames[alien.level]));
		initialEnergy = Generate(southAliens.ToArray(), log);
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
