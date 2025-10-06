using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class RailFollowingCamera : RailFollowingObjectTracker
{
  /// <summary>
  /// If set, camera will focus on this target exclusively.
  /// </summary>
	public Transform target;

    /// Offsets for camera position. `baseRailFollowPoint` will be offset by these height / z-forward directions.
    public float addFollowOffsetT = 15f;
    public float addLookForwardOffT = 20f;
    public float baseCameraHeight = 5f;
    public float maxCameraHeight = 25f;

    /// <summary>
    /// Animation curves control the smoothness of movement and rotation based on player distances.
    /// t=0 : players are close (movement should be slow), t=1 : players are far (movement should be faster to respond)
    /// </summary>
    public AnimationCurve FollowOffsetCurve;
    public AnimationCurve HeightOffsetCurve;
    public AnimationCurve LookAheadOffsetCurve;

	public bool CameraSway = true;
	public float MaxXSway = 25f;
	public float currentSway = 0f;
	public float swaySensitivity = 2f;
	public float ExtraLookAhead = 45f;

    public override void OnRoundStart()
    {
		currentSway = 0f;
		if(target!=null)
		{
			MaxXSway = 2f;
			swaySensitivity = 0.8f;
			ExtraLookAhead = 4f;
		}

		base.OnRoundStart();		
	}
	

    /// <summary>
    /// Determines base position of the camera
    /// </summary>
    public override Vector3? DetermineBaseFocusPoint()
    {
        Vector3 CentreBetweenFirstAndLast = (GetLosingPlayer().position + GetFurthestPlayer().position) * 0.5f;
        Vector3 ClosestNodePosition = rail.GetNearestNode(CentreBetweenFirstAndLast);

        return target != null ? target.position : GetAveragePlayerPosition();
    }

    /// <summary>
    /// Offsets the camera from base position by lagging it relative to chasing targets.
    /// </summary>
    /// <returns></returns>
    public override Vector3 GetLaggingPoint()
    {
        if (LevelManager.instance.chasingWall == null)
            base.GetLaggingPoint();

		//float CurrentOffsetT = LerpUtility.Clerp(-followOffsetT, -followOffsetT + -addFollowOffsetT, FollowOffsetCurve, LastPlayerWallPercentage());
		float CurrentOffsetT = GetCurrentOffsetT();
		Vector3 ClosestNodeToOffset = currentClosestRailNodeIndex.GetWorldPosition(CurrentOffsetT);

        return ClosestNodeToOffset;
    }

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift))
		{
			CameraSway = !CameraSway;
		}
	}

	/// <summary>
	/// Offsets the camera by adjusting camera's lateral and vertical position, after everything else has been calculated.
	/// </summary>
	/// <returns></returns>
	public override Vector3 FinalPositionOffset()
	{
		Vector3 Offset = Vector3.zero;

		//Vertical Offsets
		float cameraHeightOffset = LerpUtility.Clerp(baseCameraHeight, baseCameraHeight + maxCameraHeight, HeightOffsetCurve, LastPlayerWallPercentage());
		Offset = new Vector3(0f, cameraHeightOffset, 0f);

		// Lateral Offset
		if (CameraSway && GameController.instance.state == GameController.State.Round)
		{
			Vector3 lateralDeltaMove = transform.InverseTransformDirection(currentClosestRailNodeIndex.GetWorldPosition(GetCurrentLookOffsetT() + ExtraLookAhead) - currentClosestRailNodeIndex.GetWorldPosition(GetCurrentOffsetT() + ExtraLookAhead));
			if(target!=null)
			{
				lateralDeltaMove = transform.InverseTransformDirection(currentClosestRailNodeIndex.GetClosestPointOnRailSpline(target.transform.position) - currentClosestRailNodeIndex.GetWorldPosition(0f));

			}

			if (Mathf.Abs(lateralDeltaMove.x) > 2f)
			{
				currentSway = LerpUtility.Clerp(currentSway, (lateralDeltaMove.x > 0 ? -MaxXSway : MaxXSway), FollowOffsetCurve, swaySensitivity * Time.deltaTime);
				currentSway = Mathf.Clamp(currentSway, -MaxXSway, MaxXSway);
			}
			else
			{
				currentSway = LerpUtility.Clerp(currentSway, 0f, FollowOffsetCurve, swaySensitivity * Time.deltaTime);
			}

			Offset += transform.right * currentSway;
		}

		return Offset;
	}

    /// <summary>
    /// where to look at in the world
    /// </summary>
    public override Vector3 WhereToLook()
    {
		Transform losingPlayer = GetLosingPlayer(); // CheckpointManager.instance.GetLosingPlayer();
        Vector3? avPos = target != null ? target.position : GetAveragePlayerPosition();

		float lookForwardT = GetCurrentLookOffsetT(); //LerpUtility.Clerp(lookForwardOffsetT, lookForwardOffsetT + addLookForwardOffT, LookAheadOffsetCurve, LastPlayerWallPercentage());
        Vector3 lookAheadPos = currentClosestRailNodeIndex.GetClosestPointOnRailSpline(currentClosestRailNodeIndex.GetWorldPosition(lookForwardT));

        if (avPos.HasValue)
        {
            if (losingPlayer != null)
            {
                Vector3 lastPlayerPos = losingPlayer.position;
                Vector3 baseLookPos = lookAheadPos; // (lookAheadPos + avPos.Value) * 0.5f;
                //Vector3 LookPos = LerpUtility.Clerp(lookAheadPos, (avPos.Value + lastPlayerPos) * 0.5f, HeightOffsetCurve, LastPlayerWallPercentage());
                Vector3 LookPos = LerpUtility.Clerp(baseLookPos, (avPos.Value + lastPlayerPos) * 0.5f, HeightOffsetCurve, LastPlayerWallPercentage());

                return LookPos;
            }
            else
            {
                Debug.Log("No losing player!");
                return LerpUtility.Clerp(lookAheadPos, avPos.Value, FollowOffsetCurve, LastPlayerWallPercentage()); //avPos.Value;
            }
        }
        else
        {
            //Debug.Log("No average pos!");
            return currentClosestRailNodeIndex.GetWorldPosition(lookForwardOffsetT);
        }
    }

    /// <summary>
    /// Gets a percentage of how close the last player is to the chasing wall,
    /// based on the position inbetween the first player and the wall.
    /// </summary>
    /// <returns>Percentage between 0 (the first player's position) and 1 (the chasing wall's position)</returns>
    public float LastPlayerWallPercentage()
    {
        RailFollowingChasingWall chasingWall = LevelManager.instance.chasingWallController;
        if (chasingWall == null) return 0;

		Transform firstPlayer = GetFurthestPlayer(); // CheckpointManager.instance.GetFurthestPlayer();
		Transform lastPlayer = GetLosingPlayer(); // CheckpointManager.instance.GetLosingPlayer();
        if (firstPlayer == lastPlayer) return 0;

        float DistanceBetweenFirstAndWall = Vector3.Distance(firstPlayer.transform.position, chasingWall.transform.position);
        float DistanceBetweenFirstAndLast = Vector3.Distance(firstPlayer.transform.position, lastPlayer.transform.position);
        if (DistanceBetweenFirstAndLast == 0) return 0;

        float distancePercentage = DistanceBetweenFirstAndLast / DistanceBetweenFirstAndWall;
        return distancePercentage;
    }

	public float GetCurrentOffsetT()
	{
		return LerpUtility.Clerp(-followOffsetT, -followOffsetT + -addFollowOffsetT, FollowOffsetCurve, LastPlayerWallPercentage());
	}

	public float GetCurrentLookOffsetT()
	{
		return LerpUtility.Clerp(lookForwardOffsetT, lookForwardOffsetT + addLookForwardOffT, LookAheadOffsetCurve, LastPlayerWallPercentage());
	}

	private Transform GetLosingPlayer() { return target ? target : CheckpointManager.instance.GetLosingPlayer().transform; }
	private Transform GetFurthestPlayer() { return target ? target : CheckpointManager.instance.GetFurthestPlayer().transform; }
}
