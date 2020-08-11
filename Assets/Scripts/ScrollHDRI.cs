using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Creates a scrolling skybox effect</summary>
public class ScrollHDRI : MonoBehaviour
{
	[Tooltip("How fast should the skybox rotate?")]
	public float Speed = 1;

	[Tooltip("The material to rotate.")]
	public Material HDRIMat;

	/// <summary>The current position of the skybox.</summary>
	float CurrentPos = 0;

	void Update()
    {
		CurrentPos += Speed * Time.deltaTime;
		if (CurrentPos >= 360)
			CurrentPos -= 360;
		HDRIMat.SetFloat("_Rotation", CurrentPos);
	}
}
