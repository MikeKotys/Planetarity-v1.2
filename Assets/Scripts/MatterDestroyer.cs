using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mechanics
{
	/// <summary>Destroys any object that comes into contact with it.</summary>
	public class MatterDestroyer : MonoBehaviour
	{
		[Tooltip("The effect of a rocket exploding.")]
		public GameObject RocketExplosionEffect;

		public bool OnlyDestroyRockets = false;

		private void OnTriggerEnter(Collider other)
		{
			if (OnlyDestroyRockets && other.GetComponent<Rocket>() != null)
			{
				if (RocketExplosionEffect != null)
				{
					// Play the rocket destruction effect.
					var explosionEffect = Instantiate(RocketExplosionEffect);
					explosionEffect.transform.position = transform.position;
					explosionEffect.transform.localScale = Vector3.one * .35f;
					var particleSystem = explosionEffect.GetComponent<ParticleSystem>();
					particleSystem.Emit(1);

					explosionEffect = Instantiate(RocketExplosionEffect);
					explosionEffect.transform.position = other.transform.position;
					explosionEffect.transform.localScale = Vector3.one * .35f;
					particleSystem = explosionEffect.GetComponent<ParticleSystem>();
					particleSystem.Emit(1);
				}
				Destroy(gameObject);
				Destroy(other.gameObject);
			}
			else if (!OnlyDestroyRockets)
			{
				if (RocketExplosionEffect != null)
				{
					// Play the rocket destruction effect.
					var explosionEffect = Instantiate(RocketExplosionEffect);
					explosionEffect.transform.position = other.transform.position;
					explosionEffect.transform.localScale = Vector3.one * .35f;
					var particleSystem = explosionEffect.GetComponent<ParticleSystem>();
					particleSystem.Emit(1);
				}

				Destroy(other.gameObject);
			}
		}
	}
}
