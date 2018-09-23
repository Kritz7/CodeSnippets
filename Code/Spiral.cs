  /// <summary>
  /// Creates a spiral pattern on the provided LineRenderer.
  /// Spiral verticies are in local space.
  /// </summary>
  /// <param name="lineRenderer">LineRenderer to be modified</param>
  /// <param name="numberOfLoops">Number of times the spiral loops. Recommended range is 1 loop per metre.</param>
  protected virtual void SetSpiralOnLine(LineRenderer lineRenderer, int numberOfLoops = 1)
	{
		float radius = 0.2f;
		int pointsPerLoop = 6;
		float zDistIncrementPercent = ((float)numberOfLoops) / ((float)pointsPerLoop);
		Vector3[] positions = new Vector3[pointsPerLoop * numberOfLoops];
		Vector3 endPointLocalSpace = lineRenderer.transform.InverseTransformPoint(rayStart);
		int maxNumberIndexiesApprox = 500;

		for (int i=0; i<numberOfLoops; i++)
		{
			if (i * pointsPerLoop > maxNumberIndexiesApprox)
				break;

			for(int j=0; j<pointsPerLoop; j++)
			{			
				int index = (i * pointsPerLoop) + j;
				Vector3 PositionOnLineAtIndex = Vector3.Lerp(endPointLocalSpace, Vector3.zero, ((float)index) / ((float)(pointsPerLoop * numberOfLoops)));
				float xPosOffset = Mathf.Sin(Mathf.Deg2Rad * ((360 / pointsPerLoop) * j)) * radius;
				float yPosOffset = Mathf.Cos(Mathf.Deg2Rad * ((360 / pointsPerLoop) * j)) * radius;
				positions[index] = new Vector3(xPosOffset, yPosOffset, PositionOnLineAtIndex.z);
			}
		}

		lineRenderer.positionCount = pointsPerLoop * numberOfLoops;
		lineRenderer.SetPositions(positions);
	}
