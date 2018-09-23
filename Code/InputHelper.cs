/* Author: Nicholas Blackburn
*
* Utility function to make screen-to-world and screen-to-ui raycasts easy.
*/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TouchScript.Gestures;
using System.Linq;

// For all your raycasting needs. :)
public class TapInputHelper : MonoBehaviour
{
	// Will detect if the cursor's location is over any UI elements, and if so, will return them in closest Z order.
	public List<RaycastResult> CastRayAgainstUI(Vector2 screenPosition)
	{
		List<RaycastResult> results = new List<RaycastResult>();
		PointerEventData pointer = new PointerEventData(EventSystem.current);
		pointer.position = screenPosition;

		if(EventSystem.current!=null)
		{
			EventSystem.current.RaycastAll(pointer, results);
		}

		return results;
	}

	// Hits whatever gameobject the cursor is over
	public RaycastHit CastRayIntoWorld(Vector3 screenPressPosition)
	{
		screenPressPosition.z = Camera.main.nearClipPlane;
		Ray raycast = Camera.main.ScreenPointToRay(screenPressPosition);
		RaycastHit hit;
		Physics.Raycast(raycast, out hit, float.PositiveInfinity);
    
		return hit;
	}

	// Hits whatever gameobject the cursor is over
	public RaycastHit CastRayIntoWorld(Vector3 screenPressPosition, Camera cam)
	{
		screenPressPosition.z = cam.nearClipPlane;
		Ray raycast = cam.ScreenPointToRay(screenPressPosition);
		RaycastHit hit;
		Physics.Raycast(raycast, out hit, float.PositiveInfinity);

		return hit;
	}

	// Hits whatever gameobject the cursor is over, as long as gameobject's layer is within the layermask
	public RaycastHit CastRayIntoWorld(Vector3 screenPressPosition, int layerMask)
	{
		screenPressPosition.z = Camera.main.nearClipPlane;
		Ray raycast = Camera.main.ScreenPointToRay(screenPressPosition);
		RaycastHit hit;
		Physics.Raycast(raycast, out hit, float.PositiveInfinity, layerMask);

		return hit;
	}

	// Returns ALL objects that the cursor is over.
	public RaycastHit[] CastRayAllIntoWorld(Vector3 screenPressPosition)
	{
		screenPressPosition.z = Camera.main.nearClipPlane;
		Ray raycast = Camera.main.ScreenPointToRay(screenPressPosition);
    
		return Physics.RaycastAll(raycast, float.PositiveInfinity).OrderBy(h=>h.distance).ToArray();
	}

	// Returns ALL objects that the cursor is over.
	public RaycastHit[] CastRayAllIntoWorld(Vector3 screenPressPosition, Camera cam)
	{
		screenPressPosition.z = cam.nearClipPlane;
		Ray raycast = cam.ScreenPointToRay(screenPressPosition);
		return Physics.RaycastAll(raycast, float.PositiveInfinity);
	}

	// Returns ALL objects within the layermask that the cursor is over
	public RaycastHit[] CastRayAllIntoWorld(Vector3 screenPressPosition, int layerMask)
	{
		screenPressPosition.z = Camera.main.nearClipPlane;
		Ray raycast = Camera.main.ScreenPointToRay(screenPressPosition);
    
		return Physics.RaycastAll(raycast, float.PositiveInfinity, layerMask);
	}
}
