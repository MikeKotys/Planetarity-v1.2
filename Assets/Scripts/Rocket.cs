using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mechanics
{
	/// <summary>Handles Rocket logic.</summary>
	public class Rocket : MonoBehaviour
	{
		[Tooltip("The UI for the enemy rockets.")]
		public GameObject EnemyRocketUI;

		[Tooltip("Rocket's damage.")]
		public float Damage = 20;
		[Tooltip("Rocket's movement speed.")]
		public float MovementSpeed = 1;
		[Tooltip("Rocket's mass.")]
		public float Mass = 1;
		[Tooltip("Rocket's collision radius (for AI calculations).")]
		public float CollisionRadius = 1;

		/// <summary>Has this rocket been destroyed? (Prevents situations when the same rocket damages planets twice)</summary>
		[HideInInspector]
		public bool IsDestroyed = false;
		/// <summary>Which planet to ignore, according to GameController's planet order. '-1' - Player's planet.</summary>
		[HideInInspector]
		public int PlanetToIgnore = -1;

		/// <summary>The mass of the Sun.</summary>
		public const float SunMass = 2;

		/// <summary>The scene's GameController.</summary>
		GameController GameController;


		private void Awake()
		{
			GameController = FindObjectOfType<GameController>();
		}

		void FixedUpdate()
		{
			// Do logic in fixed update to increase reliability.

			// Move the rocket.
			transform.position += transform.up * Time.deltaTime * MovementSpeed;

			// Sun's attraction
			var attraction = Attract(Vector2.zero, new Vector2(transform.position.x, transform.position.z), SunMass, Time.deltaTime, Mass);
			transform.position += new Vector3(attraction.x, 0, attraction.y);


			if (GameController.PlayerPlanet != null)
			{
				// Player's planet attraction
				attraction = Attract(new Vector2(GameController.PlayerPlanet.transform.position.x,
				GameController.PlayerPlanet.transform.position.z),
				new Vector2(transform.position.x, transform.position.z), GameController.PlayerPlanet.transform.localScale.x, Time.deltaTime, Mass);
				transform.position += new Vector3(attraction.x, 0, attraction.y);
			}


			// Enemy planets' attractions.
			for (int i = 0; i < GameController.EnemyPlanets.Count; i++)
			{
				if (GameController.EnemyPlanets[i] != null)
				{
					attraction = Attract(new Vector2(GameController.EnemyPlanets[i].transform.position.x,
						GameController.EnemyPlanets[i].transform.position.z),
						new Vector2(transform.position.x, transform.position.z),
						GameController.EnemyPlanets[i].transform.localScale.x, Time.deltaTime, Mass);
					transform.position += new Vector3(attraction.x, 0, attraction.y);
				}
			}
		}

		/// <summary>Returns the delta position the rocket must pass due to planetary attraction</summary>
		/// <param name="attractorPos">The position of the planet-attractor.</param>
		/// <param name="attractorScale">The scale of the planet-attractor.</param>
		/// <returns>The delta distance the rocket must pass this frame.</returns>
		public static Vector2 Attract(Vector2 attractorPos, Vector2 rocketPos, float attractorScale, float deltaTime, float mass)
		{
			Vector2 result = Vector2.zero;

			Vector2 direction = attractorPos - rocketPos;
			float distanceSQ = direction.sqrMagnitude;

			if (distanceSQ > 0.16f)
			{
				float forceMagnitude = (mass * attractorScale) / distanceSQ;
				result = direction.normalized * forceMagnitude * deltaTime;
			}

			return result;
		}
	}

	public enum RocketType
	{
		Stinger,
		Thunder,
		Megaton
	}
}
