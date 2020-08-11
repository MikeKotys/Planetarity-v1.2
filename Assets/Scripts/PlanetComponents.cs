using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mechanics
{
	/// <summary>Stores references that should be filled in the editor but must not be saved with the JSON sysntem.</summary>
	public class PlanetComponents : MonoBehaviour
	{
		public MeshRenderer Ground;
		public MeshRenderer RimLights;
	}
}
