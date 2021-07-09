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

	public static readonly Dictionary<char, int> nameFirstLetterToLevel = new Dictionary<char, int>();

	static TenAliensPuzzle() {
		foreach (int level in aliensNames.Keys) nameFirstLetterToLevel.Add(aliensNames[level].First(), level);
	}

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

	public struct Action {
		public int? north;
		public int south;
		public bool all;
		public Action(int? north, int south, bool all) {
			this.north = north;
			this.south = south;
			this.all = all;
		}
	}

	private class SaR {
		public HashSet<Alien> south = new HashSet<Alien>();
		public HashSet<Alien> north = new HashSet<Alien>();
		public int usedEnergy = 0;
		public readonly List<Action> history = new List<Action>();
		private void _pull(int alienLevel, int byAlienLevel) {
			Alien northAlien = north.Where(s => s.level == byAlienLevel).PickRandom();
			Alien transferedAlien = _transfer(alienLevel, 8 - byAlienLevel);
		}
		private Alien _transfer(int alienLevel, int usedEnergy) {
			Alien alien = south.Where(s => s.level == alienLevel).PickRandom();
			south.Remove(alien);
			north.Add(alien);
			this.usedEnergy += usedEnergy;
			return alien;
		}
		private void applyAction(Action action) {
			if (action.north == null) _transfer(action.south, 8);
			else {
				if (action.all && south.Count(a => a.level == action.south) < 2) action = new Action(action.north, action.south, false);
				if (!action.all) _pull(action.south, action.north.Value);
				else do { _pull(action.south, action.north.Value); } while (south.Any(a => a.level == action.south));
			}
			history.Add(action);
		}
		public void transfer(int alienLevel) {
			applyAction(new Action(null, alienLevel, false));
		}
		public void pull(int alienLevel, int byAlienLevel) {
			applyAction(new Action(byAlienLevel, alienLevel, false));
		}
		public void tryPullAll(int alienLevel, int byAlienLevel) {
			if (south.All(a => a.level != alienLevel)) return;
			applyAction(new Action(byAlienLevel, alienLevel, true));
		}
		public void solve() {
			if (south.Any(s => s.level == 6)) {
				transfer(6);
				if (south.Any(s => s.level == 1)) {
					tryPullAll(1, 6);
					tryPullAll(6, 1);
					if (south.Any(s => s.level == 4)) {
						pull(4, 1);
						tryPullAll(3, 4);
						if (south.Any(s => s.level == 2)) {
							pull(2, 4);
							tryPullAll(5, 2);
							if (north.Any(s => s.level == 5)) tryPullAll(2, 5);
							else tryPullAll(2, 4);
						}
						if (south.All(s => s.level != 4)) return;
						if (north.Any(s => s.level == 3)) tryPullAll(4, 3);
						else if (north.Any(s => s.level == 2)) tryPullAll(4, 2);
						else if (north.Any(s => s.level == 1)) tryPullAll(4, 1);
						return;
					}
					if (south.Any(s => s.level == 2)) {
						if (TwoToFive(this)) {
							if (north.Any(s => s.level == 5) || south.Any(s => s.level == 5)) {
								pull(2, 1);
								tryPullAll(5, 2);
								tryPullAll(2, 5);
								return;
							}
							tryPullAll(2, 1);
							tryPullAll(5, 2);
							return;
						}
						transfer(5);
						tryPullAll(2, 5);
						tryPullAll(5, 2);
					}
					return;
				}
			}
			if (south.Any(s => s.level == 5) && south.Any(s => s.level == 2)) {
				transfer(5);
				tryPullAll(2, 5);
				tryPullAll(5, 2);
				if (south.All(s => s.level != 4)) {
					tryPullAll(1, 2);
					return;
				}
				pull(4, 2);
				tryPullAll(3, 4);
				tryPullAll(1, 4);
				if (north.Any(s => s.level == 3)) tryPullAll(4, 3);
				else tryPullAll(4, 2);
			}
			if (south.Any(s => s.level == 4)) {
				transfer(4);
				tryPullAll(3, 4);
				tryPullAll(2, 4);
				tryPullAll(1, 4);
				if (south.All(s => s.level != 4)) return;
				if (north.Any(s => s.level == 3)) tryPullAll(4, 3);
				else if (north.Any(s => s.level == 2)) tryPullAll(4, 2);
				else if (north.Any(s => s.level == 1)) tryPullAll(4, 1);
			}
		}
	}

	private static bool TwoToFive(SaR sar) {
		if (!sar.south.Any(s => s.level == 5)) return true;
		int use = 3 + 5 * (sar.south.Count(s => s.level == 2) - 1);
		int notUse = 5 * sar.south.Count(s => s.level == 2) + 2 * (sar.south.Count(s => s.level == 5) - 1);
		return use >= notUse;
	}

	public static int Generate(Alien[] aliens, Action<string> log, out List<Action> solution) {
		SaR sar = new SaR();
		foreach (Alien alien in aliens) sar.south.Add(alien);
		sar.solve();
		foreach (Alien alien in new HashSet<Alien>(sar.south)) sar.transfer(alien.level);
		log(string.Format("Energy level: {0}", sar.usedEnergy));
		log("Solution:");
		int loggingTotalEnergyUsed = 0;
		List<string> solvingStrings = new List<string>();
		foreach (Action action in sar.history) {
			loggingTotalEnergyUsed += action.north == null ? 8 : 8 - action.north.Value;
			string alienName = aliensNames[action.south];
			if (action.north == null) {
				log(string.Format("Self teleport by southern {0} alien", alienName));
				solvingStrings.Add(string.Format("hold s {0}", alienName[0]));
			} else if (action.all) {
				log(string.Format("Teleport ALL southern {0} aliens by northern {1} alien", aliensNames[action.south], aliensNames[action.north.Value]));
				solvingStrings.Add(string.Format("tap n {0};hold s {1}", aliensNames[action.north.Value][0], alienName[0]));
			} else {
				log(string.Format("Teleport ONE southern {0} alien by northern {1} alien", aliensNames[action.south], aliensNames[action.north.Value]));
				solvingStrings.Add(string.Format("tap n {0};tap s {1}", aliensNames[action.north.Value][0], alienName[0]));
			}
		}
		log(string.Format("TP solving string: {0}", solvingStrings.Join(";")));
		solution = sar.history;
		return sar.usedEnergy;
	}

	public readonly int initialEnergy;
	public readonly int aliensCount;

	public int energy;
	public int[] initialLevels;
	public HashSet<Alien> northAliens;
	public HashSet<Alien> southAliens;
	public List<Action> solution;

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
		foreach (Alien alien in southAliens) log(string.Format("\tAlien #{0} is {1}", alien.id + 1, aliensNames[alien.level]));
		initialEnergy = Generate(southAliens.ToArray(), log, out solution);
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
