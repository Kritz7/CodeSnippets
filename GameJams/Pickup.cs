using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Example;

public class Pickup : MonoBehaviour
{
    public Transform currentlyHeldObject;
    public Holdable heldObject;
    public Follow heldObjectFollow;

    public float HoldHeight = 1f;
    public float _holdHeight { get { return (heldObject && heldObject.OverrideHoldHeight != -1) ? heldObject.OverrideHoldHeight : HoldHeight; } }
    public float HoldDistance = 0.2f;

    public bool Holding { get { return currentlyHeldObject != null;  } }

    // Update is called once per frame
    void Update()
    {
        if (DialogueRunner.dr !=null && DialogueRunner.dr.isDialogueRunning)
            return;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(Holding)
            {
                Drop();
            }
            else
            {
                // Try to find a holdable object
                Holdable FoundHoldable = GetHoldable();
                
                if(FoundHoldable == null)
                {
                    // Try to find an NPC to speak to instead.
                    CheckForNearbyNPC();
                }
                else
                {
                    Hold(FoundHoldable);
                }
            }
        }
    }

    /// <summary>
    /// While character moves, make held object bounce up and down.
    /// Coroutine terminates if held object is dropped.
    /// </summary>
    public IEnumerator HoldingAnimation()
    {
        Transform currentHold = currentlyHeldObject;
        Rigidbody playerRigidBody = Player.player.GetComponent<Rigidbody>();
        while(currentHold == currentlyHeldObject && currentHold!=null)
        {
            if(playerRigidBody.velocity.magnitude > 0.3f)
            {
                heldObjectFollow.FollowHeight = Mathf.Lerp(heldObjectFollow.FollowHeight, 0.2f + _holdHeight + Mathf.Cos(Time.time * 8f) * 0.3f, 2f * Time.deltaTime);
            }

            yield return 0;
        }
    }

    /// <summary>
    /// Lazy search for nearby holdable objects.
    /// Uses dictionary indexed on scene initialisation
    /// to improve search efficiency.
    /// </summary>
    public Holdable GetHoldable(float radius = 1f)
    {
        Holdable foundHoldable = null;
        float shortestDistance = 999f;

        foreach(Holdable h in Holdable.AllHoldable)
        {
            float distance = Vector3.Distance(h.transform.position, this.transform.position);
            if (distance < radius && distance<shortestDistance)
            {
                shortestDistance = distance;
                foundHoldable = h;
            }
        }

        return foundHoldable;
    }


    /// <summary>
    /// Assigns holdable object to this character.
    /// </summary>
    public void Hold(Holdable h)
    {
        if (h == null || currentlyHeldObject != null)
            return;

        Transform t = h.transform;

        currentlyHeldObject = t;
        heldObjectFollow = currentlyHeldObject.gameObject.AddComponent<Follow>();
        heldObjectFollow.moveTarget = this.transform;
        heldObjectFollow.FollowDistance = HoldDistance;
        heldObjectFollow.FollowHeight = _holdHeight;

        heldObject = h;

        h.OnPickedUp.Invoke();

        StartCoroutine(HoldingAnimation());
    }

    /// <summary>
    /// Unassignes holdable object to this character.
    /// </summary>
    public void Drop()
    {
        if(currentlyHeldObject)
        {
            heldObject.OnDropped.Invoke();
            Destroy(heldObjectFollow);
            Vector3 groundPosition = GetGroundPosition();
            groundPosition.y = heldObject.positionWhenPickedUp.y;

            currentlyHeldObject.transform.position = groundPosition;
            currentlyHeldObject = null;
        }
    }

    public Vector3 GetGroundPosition()
    {
        return transform.position;
    }


    /// <summary>
    /// NOTE: This code is from the Yarn example documentation.
    /// I probably would have used a dictionary index like in my search method
    /// above. Although I think the delegate search is clever
    /// and very readable in its own right.
    /// </summary>
    /// 
    /// 
    /// Find all DialogueParticipants
    /** Filter them to those that have a Yarn start node and are in range; 
     * then start a conversation with the first one
     */
    public void CheckForNearbyNPC(float radius = 2f)
    {
        var allParticipants = new List<NPC>(FindObjectsOfType<NPC>());
        var target = allParticipants.Find(delegate (NPC p) {
            return string.IsNullOrEmpty(p.talkToNode) == false && // has a conversation node?
            (p.transform.position - this.transform.position)// is in range?
            .magnitude <= radius;
        });
        if (target != null)
        {
            // Kick off the dialogue at this node.
            if(DialogueRunner.dr.isDialogueRunning == false)
                FindObjectOfType<DialogueRunner>().StartDialogue(target.talkToNode);
        }
    }
}
