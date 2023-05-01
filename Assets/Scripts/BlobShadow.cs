using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobShadow : MonoBehaviour {
	private CharacterController cc;
	private Renderer render;
	
	private float offset;
	
	void Start() {
		render = GetComponent<Renderer>();
		cc = GetComponentInParent<CharacterController>();
		
		offset = transform.localPosition.y;
	}

	bool among() {
		Debug.Log("bweh");
		return false;
	}
	
	void LateUpdate() {
		Transform parent = transform.parent;
		
		// tired and need to lay down but
		// so like cast first with line and then
		// go back over it with sphere??
		
		// cuz line has more accuracy for flat surfaces
		// and sphere has more accuracy for where you'll rest upon
		// but i did have the problem of the ..
		// um the sphere hitting the side of a surface before it found the front of the surface and
		// that resulting in a weird blob shadow..
		
		RaycastHit? sphereHit = null; {
			RaycastHit shit;
			if (Physics.SphereCast(
				parent.position + cc.center, cc.radius / 2f, Vector3.down,
				out shit, Mathf.Infinity,
				-1, QueryTriggerInteraction.Ignore
			)) sphereHit = shit;
		}
		
		// RaycastHit? rayHit = null; {
		// 	RaycastHit shit;
		// 	if (Physics.Raycast(
		// 		parent.position, Vector3.down,
		// 		out shit, Mathf.Infinity,
		// 		-1, QueryTriggerInteraction.Ignore
		// 	)) rayHit = shit;
		// }
		
		bool foundSomething = sphereHit.HasValue;
		bool shouldBeVisible = foundSomething;
		
		if (foundSomething) {
			// If the ray hit another thing, it's probably not what the player
			// will land on, so fall back to the sphere's hit info.
			RaycastHit hit = (
				sphereHit
				// (sphereHit.HasValue && sphereHit?.transform != rayHit?.transform)
				// 	? sphereHit
				// 	: rayHit
			).Value;
			
			transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
			// if (Mathf.Abs(Quaternion.Angle(hit.transform.rotation, transform.rotation)) > 40f)
			// 	transform.localRotation = Quaternion.identity; // lazy hack
			
			transform.position = hit.point + Vector3.up * offset;
		}
		
		render.enabled = shouldBeVisible;
	}
}
