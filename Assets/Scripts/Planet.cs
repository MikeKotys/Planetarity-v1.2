using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mechanics
{
	/// <summary>Handles planet logic.</summary>
	public class Planet : MonoBehaviour
	{
		#region Parameters
		[Tooltip("The effect of a rocket hitting the planet.")]
		public GameObject PlanetHitEffect;

		[Tooltip("The effect of a planet exploding.")]
		public GameObject PlanetDestructionEffect;

		/// <summary>The current planet's HP left.</summary>
		[HideInInspector]
		public float CurrentHP = 100;

		/// <summary>How fast should this planet rotate around the sun?</summary>
		[HideInInspector]
		public float AroundSunSpeed = 1;
		/// <summary>How fast should this planet rotate around its axis?</summary>
		[HideInInspector]
		public float AroundAxisSpeed = 1;


		/// <summary>The current Stinger cooldown time left.</summary>
		[HideInInspector]
		public float StingerCooldown = 0;
		/// <summary>The current Thunder cooldown time left.</summary>
		[HideInInspector]
		public float ThunderCooldown = 0;
		/// <summary>The current Megaton cooldown time left.</summary>
		[HideInInspector]
		public float MegatonCooldown = 0;

		/// <summary>The planet's material.</summary>
		[HideInInspector]
		public Material PlanetMaterial;

		/// <summary>The color of the planet's material.</summary>
		[HideInInspector]
		public Color GroundColor = Color.white;
		/// <summary>The color of the planet's rim light material.</summary>
		[HideInInspector]
		public Color RimColor = Color.white;


		/// <summary>This planet's number in GameController. '-1' - Player planet.</summary>
		[HideInInspector]
		public int PlanetNumber = -1;

		/// <summary>The scene's GameController.</summary>
		GameController GameController;
		/// <summary>The PlanetComponents script on this GameObject.</summary>
		PlanetComponents PlanetComponents;

		/// <summary>The collider of the planet.</summary>
		[HideInInspector]
		public SphereCollider SphereCollider;

		PrecomputedPhysics PrecomputedPhysics;
		#endregion

		#region Start and one-off methods.
		private void Start()
		{
			// This must be in Start otherwise loading the gmae does not work properly.
			GameController = FindObjectOfType<GameController>();
			PlanetComponents = GetComponent<PlanetComponents>();
			SphereCollider = GetComponent<SphereCollider>();
			PrecomputedPhysics = FindObjectOfType<PrecomputedPhysics>();
		}

		private void OnDestroy()
		{
			var components = GetComponent<PlanetComponents>();

			// Make sure we are not leeking memory.
			if (components != null)
			{
				Destroy(components.Ground.material);
				Destroy(components.RimLights.material);
			}
		}

		/// <summary>Set the colors of the planet after we load the game.</summary>
		public void SetColorsAndMaterials()
		{
			if (PlanetComponents == null)
				PlanetComponents = GetComponent<PlanetComponents>();

			PlanetComponents.Ground.sharedMaterial = PlanetMaterial;
			PlanetComponents.Ground.material.SetColor("_Color", GroundColor);
			PlanetComponents.RimLights.material.SetColor("_Color", GroundColor);
		}
		#endregion

		#region Taking damage and destroying rockets.
		/// <summary>Handles planet damage and rocket destruction logic.</summary>
		private void OnTriggerEnter(Collider other)
		{
			var rocket = other.GetComponent<Rocket>();
			if (rocket != null && !rocket.IsDestroyed && rocket.PlanetToIgnore != PlanetNumber)
			{
				CurrentHP -= rocket.Damage;
				// Destroy() is not quick enough.
				rocket.IsDestroyed = true;
				Destroy(other.gameObject);

				if (CurrentHP <= 0)
				{
					// Play the planet destruction effect.
					var explosionEffect = Instantiate(PlanetDestructionEffect);
					explosionEffect.transform.position = transform.position;
					var particleSystem = explosionEffect.GetComponent<ParticleSystem>();
					particleSystem.Emit(1);

					if (PlanetNumber == -1)
					{
						var destroyAfter = explosionEffect.GetComponent<DestroyAfter>();
						destroyAfter.ShowDefeatScreenAfterwards = true;
					}
					else
					{
						GameController.EnemyPlanets[PlanetNumber] = null;

						bool allPlanetsDead = true;

						for (int i = 0; i < GameController.EnemyPlanets.Count; i++)
						{
							if (GameController.EnemyPlanets[i] != null)
							{
								allPlanetsDead = false;
								break;
							}
						}

						if (allPlanetsDead)
						{
							var destroyAfter = explosionEffect.GetComponent<DestroyAfter>();
							destroyAfter.ShowVictoryScreenAfterwards = true;
						}
					}

					Destroy(this);
					Destroy(gameObject);
				}
				else
				{
					// Play the planet hit effect
					float angle = Vector3.SignedAngle(transform.position, other.transform.position, Vector3.up);

					var effect = Instantiate(PlanetHitEffect);
					effect.transform.position = other.ClosestPointOnBounds(transform.position);
					effect.transform.parent = transform;
					var particleSystem = effect.GetComponent<ParticleSystem>();

					ParticleSystem.EmitParams options = new ParticleSystem.EmitParams();
					options.rotation3D = new Vector3(-90, angle, 0);

					particleSystem.Emit(options, 1);
				}
			}
		}
		#endregion

		#region Rocket launching
		/// <summary>Rocket launching logic.</summary>
		/// <param name="angle">Launch angle.</param>
		/// <param name="rocketType">Rocket type to shoot.</param>
		public void LaunchRocket(float angle, RocketType rocketType)
		{
			GameObject rocketPrefab = null;

			bool rocketOnCooldown = true;

			// Make sure we are not on cooldown.
			if (rocketType == RocketType.Thunder && ThunderCooldown <= 0)
			{
				rocketOnCooldown = false;
				rocketPrefab = GameController.Thunder;
				ThunderCooldown = GameController.ThunderCooldown;
			}
			else if (rocketType == RocketType.Megaton && MegatonCooldown <= 0)
			{
				rocketOnCooldown = false;
				rocketPrefab = GameController.Megaton;
				MegatonCooldown = GameController.MegatonCooldown;
			}
			else if (rocketType == RocketType.Stinger && StingerCooldown <= 0)
			{
				rocketOnCooldown = false;
				rocketPrefab = GameController.Stinger;
				StingerCooldown = GameController.StingerCooldown;
			}

			if (!rocketOnCooldown)
			{
				var rocket = Instantiate(rocketPrefab, transform.position,
					Quaternion.Euler(90, angle, 0));

				// Ensure the rocket will not destroy the planet that launches it.
				var rocketComp = rocket.GetComponent<Rocket>();
				if (rocketComp != null)
				{
					rocketComp.PlanetToIgnore = PlanetNumber;

					if (PlanetNumber != -1)
						rocketComp.EnemyRocketUI.gameObject.SetActive(true);
				}
			}
		}
		#endregion

		#region Fixed Update

		void FixedUpdate()
		{
			// Do logic in fixed update to increase reliability.

			// Lower cooldowns.
			if (StingerCooldown > 0)
				StingerCooldown = Mathf.Max(0, StingerCooldown - Time.deltaTime);
			if (ThunderCooldown > 0)
				ThunderCooldown = Mathf.Max(0, ThunderCooldown - Time.deltaTime);
			if (MegatonCooldown > 0)
				MegatonCooldown = Mathf.Max(0, MegatonCooldown - Time.deltaTime);

			// Rotate the planet.
			transform.Rotate(new Vector3(0, AroundAxisSpeed, 0) * Time.deltaTime);
			transform.position = Quaternion.AngleAxis(AroundSunSpeed * Time.deltaTime, Vector3.up) * transform.position;
		}
		#endregion
	}
}
