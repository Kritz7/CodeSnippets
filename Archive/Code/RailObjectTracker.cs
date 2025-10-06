using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public abstract class RailObjectTracker : IListenToMainGameEvents
{
    public Rail rail;  //reference to associated rail
    public NodeTraversalTracker currentClosestRailNodeIndex;

    /// <summary>
    /// Point that this transform should look at
    /// </summary>
    public Vector3 baseRailFollowPoint;
    public float followOffsetT = 100f;
    public float lookForwardOffsetT = 4f;

    public float MoveLerpSpeed = 4f;
    public float LookLerpSpeed = 3.5f;

    protected Vector3 PreviousPosition;

    public override void OnRoundStart()
    {
        if (rail == null)
            rail = LevelManager.instance.trackRail;

        base.OnRoundStart();
        DetermineBaseFocusPoint();
        SetStartPosition();
    }

    /// <summary>
    /// Sets the start position of this object.
    /// </summary>
    public void SetStartPosition()
    {
        currentClosestRailNodeIndex = new NodeTraversalTracker(rail, CheckpointManager.instance.GetNextRespawnCheckpoint().transform.position);
        transform.position = currentClosestRailNodeIndex.GetWorldPosition(-followOffsetT);
        var newRotation = Quaternion.LookRotation(WhereToLook() - transform.position);
        transform.rotation = newRotation;

		PreviousPosition = transform.position;
	}

    void LateUpdate()
    {
        if (rail == null || rail.totalNodes == 0)
            return;

        // work out where the point is we should be following
        Vector3? newFocusPoint = DetermineBaseFocusPoint(); //NOTE: may or may not update the focus

        if (newFocusPoint.HasValue)
            baseRailFollowPoint = newFocusPoint.Value;

        // move/rotate the object with respect to the focusPoint and the nodeIndex
        UpdatePosition(baseRailFollowPoint);
    }

    /// <summary>
    /// Updates the position of this object using the referenced rail.
    /// </summary>
    /// <param name="baseTrackingPoint">Point to track.</param>
    /// <param name="maxJumpAllowed">Maximum jump allowed at a time.</param>
    /// <param name="force">If set to <c>true</c> force.</param>
    public virtual void UpdatePosition(Vector3 baseTrackingPoint, bool force = true)
    {
		// update position of tracked point with respect to the rail
		if (force)
			currentClosestRailNodeIndex.Update(baseTrackingPoint);
		if(!force)
			currentClosestRailNodeIndex.UpdateLerped(baseTrackingPoint, MoveLerpSpeed * Time.deltaTime);

		// calculate new position to lag behind focuspoint with respect to the rail
		Vector3 newTrackingPoint = GetLaggingPoint();
        newTrackingPoint += FinalPositionOffset();

		// smoothly move to new position
		PreviousPosition = transform.position;
		transform.position = Vector3.Lerp(transform.position, newTrackingPoint, MoveLerpSpeed * Time.deltaTime);

        // work out what to look at
        Vector3 whereToLook = WhereToLook();

        // smoothly transition from old rotation to new one
        if (whereToLook - transform.position != Vector3.zero)
        {
            var newRotation = Quaternion.LookRotation(whereToLook - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, LookLerpSpeed * Time.deltaTime);
        }

        //lindsay hooking in the car canvases here
        CarCanvas.LateUpdateAllPositions();
    }

    /// <summary>
    /// where on the rail (in world space) to offset the object from
    /// </summary>
    public abstract Vector3? DetermineBaseFocusPoint();

    public virtual Vector3 GetLaggingPoint()
    {
        // Current pos snapped to rail, lagging behind by an offset.
        return currentClosestRailNodeIndex.GetWorldPosition(-followOffsetT);
    }

    /// <summary>
    /// offset to apply to the calculated position (along the rail) of the following object
    /// </summary>
    public virtual Vector3 FinalPositionOffset() { return Vector3.zero; }

    /// <summary>
    /// where to look at in the world
    /// </summary>
    public virtual Vector3 WhereToLook()
    {
        PlayerObj leadingPlayer = CheckpointManager.instance.GetFurthestPlayer();

        if (leadingPlayer != null)
        {
            Vector3 LookPos = (CheckpointManager.instance.GetFurthestPlayer().transform.position + CheckpointManager.instance.GetLosingPlayer().transform.position) * 0.5f;
            LookPos = rail.GetNearestNode(LookPos);

            return LookPos;
        }
        else
        {
            return currentClosestRailNodeIndex.GetWorldPosition(-followOffsetT + 1);
        }
    }

    /// <summary>
    /// Returns avg player position.
    /// </summary>
    /// <returns>Avg player position. Null if there are no players.</returns>
    public static Vector3? GetAveragePlayerPosition()
    {
        Vector3 averagePlayerPosition = Vector3.zero;
        int count = 0;

        foreach (PlayerObj player in GameController.instance.playersEnabled)
        {
            if ((player.vehicleController.isAlive && player.gameObject.activeSelf && player.vehicleController.aboveTrack) ||
                GameController.instance.state == GameController.State.RoundIntro)
            {
                averagePlayerPosition += player.transform.position;
                count++;
            }
        }

        // average out the sum of the positions
        averagePlayerPosition /= count;

        // bail if no players fit the bill
        if (count == 0) return null;

        return averagePlayerPosition;
    }
}
