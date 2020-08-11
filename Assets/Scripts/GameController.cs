using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;

namespace Mechanics
{
	/// <summary>Processes player input, spawns planets in a new game scenario, holds critical game varialbes and prefabs,
	///		controlls the AI behaviour of the enemy planets.</summary>
	public class GameController : MonoBehaviour
	{
		#region Parameters
		[Header("Managers.")]
		[Tooltip("Computes rocket trajectories.")]
		public PrecomputedPhysics PrecomputedPhysics;
		[Tooltip("Contains all huds.")]
		public HUDController HUDController;

		[Header("Game parameters.")]
		[Tooltip("Laser's polygon that will be manipulated as a HUD for aiming the rockets.")]
		public Transform Laser;
		[Tooltip("The player's planet - must exist in the scene.")]
		public Planet PlayerPlanet;

		[Tooltip("The cooldown of the Stinger rocket.")]
		public float StingerCooldown = 3;
		[Tooltip("The cooldown of the Thunder rocket.")]
		public float ThunderCooldown = 8;
		[Tooltip("The cooldown of the Megaton rocket.")]
		public float MegatonCooldown = 20;

		[Header("Planet spawner parameters.")]
		[Tooltip("How close to the sun a planet can spawn.")]
		public float DistanceMin = 1.5f;
		[Tooltip("How far from the sun a planet can spawn.")]
		public float DistanceMax = 7.5f;

		[Tooltip("How small can the smalles planet be.")]
		public float ScaleMin = 0.25f;
		[Tooltip("How large can the largest planet be.")]
		public float ScaleMax = 0.7f;

		[Tooltip("The minimal rotation speed around the sun for planets.")]
		public float RotationMin = 5;
		[Tooltip("The maximum rotation speed around the sun for planets.")]
		public float RotationMax = 20;

		[Tooltip("Planet material.")]
		public Material Planet1;
		[Tooltip("Planet material.")]
		public Material Planet2;
		[Tooltip("Planet material.")]
		public Material Planet3;
		[Tooltip("Planet material.")]
		public Material Planet4;
		[Tooltip("Planet rim light material.")]
		public Material Planet1Rim;
		[Tooltip("Planet rim light material.")]
		public Material Planet2Rim;
		[Tooltip("Planet rim light material.")]
		public Material PlanetRimGray;

		[Header("Prefabs.")]
		[Tooltip("Enemy planet.")]
		public GameObject EnemyPlanet;
		[Tooltip("Stinger rocket.")]
		public GameObject Stinger;
		[Tooltip("Thunder rocket.")]
		public GameObject Thunder;
		[Tooltip("Megaton rocket.")]
		public GameObject Megaton;

		[Header("Sounds.")]
		[Tooltip("Main game music.")]
		public AudioSource OST;
		[Tooltip("Warning sound of an enemy rocket launched.")]
		public AudioSource EnemyRocketLaunched;
		[Tooltip("Warning sound of an enemy megaton launched.")]
		public AudioSource EnemyMegatonLaunched;

		/// <summary>Reference for the Stinger's Rocket script.</summary>
		[HideInInspector]
		public Rocket StingerRocket;

		/// <summary>Reference for the Thunder's Rocket script.</summary>
		[HideInInspector]
		public Rocket ThunderRocket;

		/// <summary>Reference for the Megaton's Rocket script.</summary>
		[HideInInspector]
		public Rocket MegatonRocket;

		/// <summary>Reference for the enemy planets.</summary>
		[HideInInspector]
		public List<Planet> EnemyPlanets = new List<Planet>();


		/// <summary>Should this class spawn planets in Awake()?</summary>
		public static bool SpawnPlanetsOnAwake = true;

		/// <summary>How many enemies to spawn in SpawnPlanets()?</summary>
		public static int NumberOfEnemies = 4;
		#endregion


		void Awake()
		{
			var music = GameObject.Find("Main Menu OST");
			if (music != null)
				Destroy(OST);
			else
			{
				OST.Play();
				OST.name = "Main Menu OST";
				OST.ignoreListenerPause = true;
				DontDestroyOnLoad(OST);
			}


			StingerRocket = Stinger.GetComponent<Rocket>();
			ThunderRocket = Thunder.GetComponent<Rocket>();
			MegatonRocket = Megaton.GetComponent<Rocket>();

			Simulations.Add(null);
			Simulations.Add(null);
			Simulations.Add(null);
			Simulations.Add(null);

			if (SpawnPlanetsOnAwake)
				SpawnPlanets();
		}

		#region Spawning planets.
		/// <summary>Creates enemy planets with random materials and parameters. Positions them as well as the player planet
		///		on random orbits around the sun.</summary>
		public void SpawnPlanets()
		{
			EnemyPlanets.Add(null);
			EnemyPlanets.Add(null);
			EnemyPlanets.Add(null);
			EnemyPlanets.Add(null);

			// Given min and max distances from the sun, let's create five 'lanes' that our planets can occupy.
			float firstLaneDistance = DistanceMin;
			float secondLaneDistance = DistanceMin + (DistanceMax - DistanceMin) * .25f;
			float thirdLaneDistance = DistanceMin + (DistanceMax - DistanceMin) * .5f;
			float fourthLaneDistance = DistanceMin + (DistanceMax - DistanceMin) * .75f;
			float fifthLaneDistance = DistanceMax;

			// Position player planet.
			int currentPlanetLane = UnityEngine.Random.Range(0, 5);

			switch (currentPlanetLane)
			{
				case 0:
					PlayerPlanet.transform.position = Vector3.right * firstLaneDistance;
					break;
				case 1:
					PlayerPlanet.transform.position = Vector3.right * secondLaneDistance;
					break;
				case 2:
					PlayerPlanet.transform.position = Vector3.right * thirdLaneDistance;
					break;
				case 3:
					PlayerPlanet.transform.position = Vector3.right * fourthLaneDistance;
					break;
				case 4:
					PlayerPlanet.transform.position = Vector3.right * fifthLaneDistance;
					break;
			}

			HashSet<int> occupiedLanes = new HashSet<int>();
			occupiedLanes.Add(currentPlanetLane);

			PlayerPlanet.transform.RotateAround(Vector3.zero, Vector3.up, UnityEngine.Random.Range(0, 360));
			PlayerPlanet.transform.localScale = Vector3.one * UnityEngine.Random.Range(ScaleMin, ScaleMax);

			var playerPlanetScript = PlayerPlanet.GetComponent<Planet>();
			
			// Set random rotation speed. Make sure planets further away from the sun rotate slower.
			playerPlanetScript.AroundSunSpeed = UnityEngine.Random.Range(RotationMin, RotationMax) / Mathf.Max(1, currentPlanetLane);

			// Set initial cooldowns.
			playerPlanetScript.ThunderCooldown = ThunderCooldown;
			playerPlanetScript.MegatonCooldown = MegatonCooldown;
			playerPlanetScript.PlanetNumber = -1;

			for (int i = 0; i < NumberOfEnemies; i++)
			{
				var enemyPlanet = Instantiate(EnemyPlanet);

				// Position an enemy planet on a free orbit.
				while (occupiedLanes.Contains(currentPlanetLane))
					currentPlanetLane = UnityEngine.Random.Range(0, 5);

				switch (currentPlanetLane)
				{
					case 0:
						enemyPlanet.transform.position = Vector3.right * firstLaneDistance;
						break;
					case 1:
						enemyPlanet.transform.position = Vector3.right * secondLaneDistance;
						break;
					case 2:
						enemyPlanet.transform.position = Vector3.right * thirdLaneDistance;
						break;
					case 3:
						enemyPlanet.transform.position = Vector3.right * fourthLaneDistance;
						break;
					case 4:
						enemyPlanet.transform.position = Vector3.right * fifthLaneDistance;
						break;
				}
				occupiedLanes.Add(currentPlanetLane);

				enemyPlanet.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(0.0f, 35.0f),
					UnityEngine.Random.Range(0.0f, 35.0f), UnityEngine.Random.Range(0.0f, 35.0f));

				enemyPlanet.transform.RotateAround(Vector3.zero, Vector3.up, UnityEngine.Random.Range(0, 360));
				enemyPlanet.transform.localScale = Vector3.one * UnityEngine.Random.Range(ScaleMin, ScaleMax);

				// Set random materials.
				var planet = enemyPlanet.GetComponent<Planet>();
				var planetComponents = enemyPlanet.GetComponent<PlanetComponents>();

				if (planet != null && planetComponents != null)
				{
					int randomMat = UnityEngine.Random.Range(0, 4);

					switch (randomMat)
					{
						case 0:
							planet.PlanetMaterial = Planet1;
							planetComponents.RimLights.sharedMaterial = Planet1Rim;
							break;
						case 1:
							planet.PlanetMaterial = Planet2;
							planetComponents.RimLights.sharedMaterial = Planet2Rim;
							break;
						case 2:
							planet.PlanetMaterial = Planet3;
							planet.GroundColor = new Color(UnityEngine.Random.Range(0.0f, 1.0f),
								UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
							planetComponents.RimLights.sharedMaterial = PlanetRimGray;
							planet.RimColor = planet.GroundColor;
							break;
						case 3:
							planet.PlanetMaterial = Planet4;
							planet.GroundColor = new Color(UnityEngine.Random.Range(0.0f, 1.0f),
								UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
							planetComponents.RimLights.sharedMaterial = PlanetRimGray;
							planet.RimColor = planet.GroundColor;
							break;
					}

					planet.SetColorsAndMaterials();

					// Set initial cooldowns.
					planet.ThunderCooldown = ThunderCooldown;
					planet.MegatonCooldown = MegatonCooldown;

					// Store the number of this planet into its Planet component so that it will be saved into the savegame file.
					planet.PlanetNumber = i;
					EnemyPlanets[planet.PlanetNumber] = planet;

					// Set random rotation speed. Make sure planets further away from the sun rotate slower.
					planet.AroundSunSpeed = UnityEngine.Random.Range(RotationMin, RotationMax) / Mathf.Max(1, currentPlanetLane);
				}

			}
		}
		#endregion

		#region AI of enemy planets
		// Enemies start by shooting rockets every 6 seconds (only one enemy can shoot at a time).
		//	20 seconds after they shoot every 4 seconds, 20 seconds after they shoot every 2 seconds,
		//	20 seconds after they shoot every 1 seconds for the rest of the game.

		// Enemies shoot rockets only when their rockets are not on cooldown. If enemies have more than 1
		//	rocket type available they will choose what rocket to shoot at random.

		/// <summary>When is the next time we launch a rocket at the Player's planet?</summary>
		float NextRocketLaunch = 3;

		/// <summary>How long to wait between rocket launches?</summary>
		float RocketLaunchDelay = 6;

		/// <summary>When should the bacground computation results be ready?</summary>
		float TimeToCheckRocketResults = float.MaxValue;

		/// <summary>What planet will shoot next?</summary>
		int NextPlanetRocketeer = -1;

		/// <summary>What rocket will be used next?</summary>
		int ChosenRocket = 0;

		/// <summary>Holds the references to each ongoing rocket simulation.</summary>
		List<BackgroundSimulation> Simulations = new List<BackgroundSimulation>(4);

		/// <summary>Controlls the rocket launches from enemy planets towards the player planet.</summary>
		void ProcessAI()
		{
			if (Time.timeSinceLevelLoad > TimeToCheckRocketResults)
			{

				if (EnemyPlanets[NextPlanetRocketeer] != null && Simulations[NextPlanetRocketeer] != null)
				{
					// The simulation has failed to find the needed angle we need to run a new one imediately.
					if (Simulations[NextPlanetRocketeer].OutAngle == -1)
						NextRocketLaunch = -1;
					else
					{
						// We are ready to launch our rockets.
						RocketType rocketType = RocketType.Stinger;
						if (ChosenRocket == 1)
							rocketType = RocketType.Thunder;
						else if (ChosenRocket == 2)
							rocketType = RocketType.Megaton;

						EnemyPlanets[NextPlanetRocketeer].LaunchRocket(Simulations[NextPlanetRocketeer].OutAngle, rocketType);

						if (rocketType == RocketType.Megaton)
							EnemyMegatonLaunched.PlayOneShot(EnemyMegatonLaunched.clip);
						else
							EnemyRocketLaunched.PlayOneShot(EnemyRocketLaunched.clip);
					}
				}

				TimeToCheckRocketResults = float.MaxValue;
			}

			if (Time.timeSinceLevelLoad > NextRocketLaunch)
			{
				// Check if any planet has rockets not on cooldown.
				bool noReadyPlanets = true;

				for (int i = 0; i < EnemyPlanets.Count; i++)
				{
					if (EnemyPlanets[i] != null
						&& (EnemyPlanets[i].MegatonCooldown <= 0
							|| EnemyPlanets[i].ThunderCooldown <= 0
							|| EnemyPlanets[i].StingerCooldown <= 0))
					{
						noReadyPlanets = false;
						break;
					}
				}

				if (noReadyPlanets)		// All rockets on cooldown - check in a second.
					NextRocketLaunch += 1;
				else
				{
					while (true)
					{
						// Pick a planet at random.
						NextPlanetRocketeer = UnityEngine.Random.Range(0, NumberOfEnemies);

						var enemyPlanet = EnemyPlanets[NextPlanetRocketeer];

						if (enemyPlanet != null
							&& (enemyPlanet.MegatonCooldown <= 0 || enemyPlanet.ThunderCooldown <= 0 || enemyPlanet.StingerCooldown <= 0))
						{
							while (true)
							{
								ChosenRocket = UnityEngine.Random.Range(0, 3);

								// Check what rockets the planet has.
								if (ChosenRocket == 0 && enemyPlanet.StingerCooldown <= 0)
									break;
								else if (ChosenRocket == 1 && enemyPlanet.ThunderCooldown <= 0)
									break;
								else if (ChosenRocket == 2 && enemyPlanet.MegatonCooldown <= 0)
									break;
							}

							break;
						}
					}

					Rocket rocket = StingerRocket;
					if (ChosenRocket == 1)
						rocket = ThunderRocket;
					else if (ChosenRocket == 2)
						rocket = MegatonRocket;

					Simulations[NextPlanetRocketeer] = PrecomputedPhysics.FindAngle(EnemyPlanets[NextPlanetRocketeer],
						rocket.CollisionRadius, rocket.Mass, rocket.MovementSpeed);

					TimeToCheckRocketResults = Time.timeSinceLevelLoad + PrecomputedPhysics.FixedDeltaTime * 20;

					if (Time.timeSinceLevelLoad > 60)
						RocketLaunchDelay = 1;
					else if (Time.timeSinceLevelLoad > 40)
						RocketLaunchDelay = 2;
					else if (Time.timeSinceLevelLoad > 20)
						RocketLaunchDelay = 4;

					NextRocketLaunch += RocketLaunchDelay;
				}
			}
		}
		#endregion



		#region Update
		/// <summary>What is the current selected rocket? It will be used to shoot if the player clicks on left mouse button.</summary>
		RocketType SelectedRocketType = RocketType.Stinger;

		void Update()
		{
			if (PlayerPlanet == null)
				Laser.gameObject.SetActive(false);

			// Only receive input and process AI if the game is not paused and the player's planet is alive.
			if (Time.timeScale > 0 && PlayerPlanet != null)
			{
				ProcessAI();

				// Get the angle the player is aiming at.
				var planetPos = Camera.main.WorldToScreenPoint(PlayerPlanet.transform.position);
				planetPos.z = 0;

				var difference = planetPos - Input.mousePosition;

				float crossSign = Mathf.Sign(Vector3.Cross(Vector3.up, difference).z);

				float angle = Vector3.Angle(Vector3.up, difference) * crossSign;

				if (angle > 360)
					angle -= 360;
				else if (angle < 0)
					angle += 360;

				// Scale the laser towards the cursor so that its easier to aim.
				Laser.transform.localScale = new Vector3(difference.magnitude / (new Vector3(Screen.width, Screen.height, 0)).magnitude * 11.35f,
					Laser.transform.localScale.y, Laser.transform.localScale.z);
				Laser.transform.position = PlayerPlanet.transform.position;
				Laser.transform.eulerAngles = new Vector3(Laser.transform.eulerAngles.x, 270 - angle, Laser.transform.eulerAngles.z);

				// Process fire input
				if (Input.GetMouseButton(0))
				PlayerPlanet.LaunchRocket(180 - angle, SelectedRocketType);

				// Process change rocket input.
				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					HUDController.ChangeSelectedRocket(1);
					SelectedRocketType = RocketType.Stinger;
				}

				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					HUDController.ChangeSelectedRocket(2);
					SelectedRocketType = RocketType.Thunder;
				}

				if (Input.GetKeyDown(KeyCode.Alpha3))
				{
					HUDController.ChangeSelectedRocket(3);
					SelectedRocketType = RocketType.Megaton;
				}
			}
		}
		#endregion
	}
}
