using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ThreadNinja;
using System.Threading;

namespace Mechanics
{
	/// <summary>Accurately computes rocket trajectories in background threads.</summary>
	public class PrecomputedPhysics : MonoBehaviour
	{
		/// <summary>The scene's GameController.</summary>
		GameController GameController;

		/// <summary>.</summary>
		public static List<Vector3> AllShifts = new List<Vector3>(360);

		/// <summary>.</summary>
		public const float FixedDeltaTime = 1.0f / 60.0f;

		private void Awake()
		{
			GameController = FindObjectOfType<GameController>();

			for (int i = 0; i < 360; i++)
				AllShifts.Add(Quaternion.AngleAxis(i, Vector3.up) * Vector3.forward * FixedDeltaTime);
		}

		/// <summary>Finds an angle a rocket should be launched at 20 frames from now to hit the player with 70-80% accuracy.</summary>
		/// <param name="enemy">What planet is shooting?</param>
		/// <param name="rocketCollisionRadius">The rocket's radius.</param>
		/// <param name="rocketMass">The rocket's mass.</param>
		/// <param name="rocketSpeed">The rocket's speed.</param>
		/// <returns>The class that holds all the data about the background computation as well as the eventual computation result.</returns>
		public BackgroundSimulation FindAngle(Planet enemy, float rocketCollisionRadius, float rocketMass, float rocketSpeed)
		{
			BackgroundSimulation simulation = new BackgroundSimulation();

			// Find where planets are 20 frames from now:
			simulation.PlayerPos = Quaternion.AngleAxis(GameController.PlayerPlanet.AroundSunSpeed * FixedDeltaTime * 20, Vector3.up)
				* GameController.PlayerPlanet.transform.position;

			simulation.EnemyData.Clear();

			for (int i = 0; i < GameController.EnemyPlanets.Count; i++)
			{
				simulation.EnemyData.Add(null);
				if (GameController.EnemyPlanets[i] != null)
				{
					simulation.EnemyData[i] = new EnemyPlanetData();
					simulation.EnemyData[i].Position = Quaternion.AngleAxis(GameController.EnemyPlanets[i].AroundSunSpeed 
						* FixedDeltaTime * 20, Vector3.up)
						* GameController.EnemyPlanets[i].transform.position;
					simulation.EnemyData[i].Scale = GameController.EnemyPlanets[i].transform.localScale.x;
					simulation.EnemyData[i].Radius = GameController.EnemyPlanets[i].transform.localScale.x
						* GameController.EnemyPlanets[i].SphereCollider.radius;
					simulation.EnemyData[i].FixedRotation = Quaternion.AngleAxis(GameController.EnemyPlanets[i].AroundSunSpeed 
						* FixedDeltaTime, Vector3.up);
				}
			}


			// Create rockets' initial positions, 360 rockets for 360 degrees of possible rocket shots.
			for (int i = 0; i < 360; i++)
				simulation.Rockets.Add(simulation.EnemyData[enemy.PlanetNumber].Position);

			simulation.FixedPlayerRotation = Quaternion.AngleAxis(GameController.PlayerPlanet.AroundSunSpeed * FixedDeltaTime, Vector3.up);

			simulation.PlayerScale = GameController.PlayerPlanet.transform.localScale.x;
			simulation.PlayerRadius = GameController.PlayerPlanet.transform.localScale.x * GameController.PlayerPlanet.SphereCollider.radius;
			simulation.PlanetToIgnore = enemy.PlanetNumber;

			simulation.RocketCollisionRadius = rocketCollisionRadius;
			simulation.RocketSpeed = rocketSpeed;
			simulation.RocketMass = rocketMass;

			simulation.OutAngle = -1;
			Task task;
			this.StartCoroutineAsync(simulation.FrameStep(), out task);

			return simulation;
		}
	}

	/// <summary>Simulates 5 seconds of flight for 360 rockets (1 for each angle in 360 degrees) as well as movements of all the planets.
	///		Computes the angle (rocket) that is guaranteed to hit the Player's planet. Rocket simulation starts 20 frames in the future.</summary>
	public class BackgroundSimulation
	{
		/// <summary>Rocket coordinates.</summary>
		public List<Vector3> Rockets = new List<Vector3>(360);
		/// <summary>Which rocket numbers to ignore (due to collision with enemy planets/sun).</summary>
		public HashSet<int> RocketsToIgnore = new HashSet<int>();
		/// <summary>All the data about all enemy planets.</summary>
		public List<EnemyPlanetData> EnemyData = new List<EnemyPlanetData>(4);
		/// <summary>The quaternion that the player planet rotates each 1/60 of a second.</summary>
		public Quaternion FixedPlayerRotation;
		/// <summary>The position of the player 20 frames in the future.</summary>
		public Vector3 PlayerPos;
		/// <summary>The scale of the Player's planet.</summary>
		public float PlayerScale;
		/// <summary>The radius of the Player's collision.</summary>
		public float PlayerRadius;
		/// <summary>The enemy planet number that should be ignored when we compute rocket vs planet collision.</summary>
		public int PlanetToIgnore;
		/// <summary>The rocket collision radius.</summary>
		public float RocketCollisionRadius;
		/// <summary>The speed of the rocket.</summary>
		public float RocketSpeed;
		/// <summary>The mass of the rocket.</summary>
		public float RocketMass;
		/// <summary>The end result of the computation - the angle the rocket should be
		/// launched at to ensure it hits the Player's planet. '-1' - means it's impossible to hit the Player's planet.</summary>
		public int OutAngle = -1;

		/// <summary>The background worker thread that computes the angle needed to hit the player planet.</summary>
		public IEnumerator FrameStep()
		{
			for (int k = 0; k < 360; k++)
			{
				//Find planets positions.
				PlayerPos = FixedPlayerRotation * PlayerPos;

				for (int i = 0; i < EnemyData.Count; i++)
				{
					if (EnemyData[i] != null)
						EnemyData[i].Position = EnemyData[i].FixedRotation * EnemyData[i].Position;
				}

				if (Rockets != null && Rockets.Count > 0)
				{
					for (int i = 0; i < Rockets.Count; i++)
					{
						if (!RocketsToIgnore.Contains(i))
						{
							// Move rockets
							Rockets[i] += PrecomputedPhysics.AllShifts[i] * RocketSpeed;

							// Process planets' and Sun's attraction.
							Vector2 attraction = Rocket.Attract(Vector2.zero, new Vector2(Rockets[i].x, Rockets[i].z),
								Rocket.SunMass, PrecomputedPhysics.FixedDeltaTime, RocketMass);
							Rockets[i] += new Vector3(attraction.x, 0, attraction.y);

							attraction = Rocket.Attract(PlayerPos, new Vector2(Rockets[i].x, Rockets[i].z),
								PlayerScale, PrecomputedPhysics.FixedDeltaTime, RocketMass);
							Rockets[i] += new Vector3(attraction.x, 0, attraction.y);

							for (int j = 0; j < EnemyData.Count; j++)
							{
								if (EnemyData[j] != null)
								{
									attraction = Rocket.Attract(EnemyData[j].Position, new Vector2(Rockets[i].x, Rockets[i].z),
										EnemyData[j].Scale, PrecomputedPhysics.FixedDeltaTime, RocketMass);
									Rockets[i] += new Vector3(attraction.x, 0, attraction.y);
								}
							}

							// Now compute collisions.
							if ((Rockets[i] - PlayerPos).sqrMagnitude < Mathf.Pow(RocketCollisionRadius + PlayerRadius, 2))
							{
								// We hit the player's planet.
								OutAngle = i;
								break;
							}
							else
							{
								// Check collision with the Sun.
								if (Rockets[i].sqrMagnitude < Mathf.Pow(RocketCollisionRadius + 0.41965f, 2))
								{
									RocketsToIgnore.Add(i);
									break;
								}

								// Check collision with other planets.
								bool requestBreak = false;
								for (int j = 0; j < EnemyData.Count; j++)
								{
									if (EnemyData[j] != null && PlanetToIgnore != j
										&& (Rockets[i] - EnemyData[j].Position).sqrMagnitude
											< Mathf.Pow(RocketCollisionRadius + EnemyData[j].Radius, 2))
									{
										requestBreak = true;
										RocketsToIgnore.Add(i);
										break;
									}
								}

								if (requestBreak)
									break;
							}
						}
					}
				}

				// We hit the player - no more work needed.
				if (OutAngle > -1)
					yield break;
			}
			yield break;
		}

	}

	public class EnemyPlanetData
	{
		public Vector3 Position;
		public float Scale;
		public float Radius;
		public Quaternion FixedRotation;
	}
}
