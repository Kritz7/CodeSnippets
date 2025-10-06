/* Author: Nicholas Blackburn
*
* Small snippet of everyday code. This snippet is as-is, demonstrating my usual code style
* on routine methods.
*
* The 0.52f magic number is perhaps a little egregious. :P
*/

/// <summary>
/// Ball has collided with a wall and needs to bounce away from it. Ball becomes smaller after each bounce.
/// </summary>
/// <param name="hit">The last raycast the ball performed before hitting the wall.</param>
protected void Bounce(RaycastHit hit)
{		
	float prevY = transform.position.y;

	// Calculate ball's next facing angle.
	Vector3 dirToHit = (transform.position - hit.point);
	Vector3 nextDirection = Vector3.Reflect(dirToHit, hit.normal);

	// Position and rotate!
	// Move the ball a bit further than half its radius from the wall.
	Vector3 nextPos = hit.point - nextDirection * transform.lossyScale.x * 0.52f;
	nextPos.y = prevY;
	transform.position = nextPos;
	transform.rotation = Quaternion.LookRotation((transform.position - hit.point));

	// Ball should resize after each bounce.
	transform.localScale *= 1-bounceScaleLoss;
	if (CurrentBouncyHitpoints <= 0)
		Explode(transform.position);
	
	mainParticleSystemModule.startColor = colorRamp.Evaluate(CurrentBouncyHitpoints / maxPossibleHitpoints);

	// Balls that bounce can now friendly-fire.
	explosion.ignoresOwner = IgnoreMode.Nothing;

	hasBounced = true;
	CurrentBouncyHitpoints--;
	_bounceFrame = Time.frameCount;		
}
