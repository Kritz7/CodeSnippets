/* Author: Nicholas Blackburn
*
* A small example of designer-friendly variables, created to look nice in Unity's inspector
* and provide helpful tooltips as to functionality.
*
* AnimationBehaviours was a very common class contained on all Items within the game, and
* while this snippet contains no functionality, AnimationBehaviours was the foundation for 90%
* of the game's player<->item hooks.
*/

[System.Serializable]
public class ActionBehaviours
{
	public enum AnimationType
	{
		None,
		PickupCrouch,
		PickupStanding
	}
	
	[Header("Action Behaviours")]
	[Tooltip("Makes GameObject disappear once action has been performed.")]
	public bool HideObject = false;
	[Tooltip("Determines what animation the player should do when this action is performed.")]
	public AnimationType PlayerAnimation;
	[Tooltip("Runs a Yarn node action is activated.")]
	[FormerlySerializedAs("DialogueNodeStart")]
	public string YarnNode;
	[Tooltip("Launches the Inspection window.")]
	[FormerlySerializedAs("InspectName")]
	public string InspectWindow;
	[Tooltip("Loads a scene when action is activated.")]
	public string SceneToLoad;

	[Header("Events")]
	public UnityEvent OnUse;

	[Header("Advanced Settings")]
	[Tooltip("Only perform this action if these action names have already been performed. NOTE: Keep action names unique!")]
	public List<string> PrerequisiteActions = new List<string>();   // ActionNames of other actions that need to be found in ActionTrackingManager before this action is useable.
	[Tooltip("Allows action to be performed only once")]
	public bool ActionIsOnceOff = false;								// Defines if this action can only be called once (e.g., smashing a vase) or multiple times (e.g., talking to someone)
	[Tooltip("Hides action from appearing in game UI menus.")]
	public bool ActionIsHidden = false;
	[Tooltip("How long does this object take to hide after the action is called?")]
	public float HideDelay = 2f;

	public ActionBehaviours(){}
}
