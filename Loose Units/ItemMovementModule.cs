using System;
using System.Threading.Tasks;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace LooseUnits
{
    public class ItemMovementModule : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField, AutoProperty(AutoPropertyMode.Parent)] private Rigidbody rb;
        [FormerlySerializedAs("carriableBase")] [SerializeField, AutoProperty(AutoPropertyMode.Parent)] private CarriableBase carriable;
        [SerializeField] private Transform physicsColliderParent;

        [Header("Pickup Settings")]
        [SerializeField] private float pickupMass = 0.0001f; 
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float externalForceMulti = 10f;

        [Header("Drop Settings")]
        [SerializeField] private float dropForce = 200f;
        [SerializeField] private float moveSpeedDropForceMulti = 2f;
        
        // Non-serialized
        private Agent carryingAgent;
        private Transform followTarget;
        private Vector3 externalMovementVelocity; // resets every frame
        private float originalPickupMass;
        private bool isBeingPlaced;

        // Properties
        public bool AllowInteraction => !isBeingPlaced && followTarget == null;

        public async void DropRestorePosition(Vector3 goalPosition, Quaternion goalRotation)
        {
            await RestoreOriginalPositionTask(goalPosition, goalRotation);
        }

        public void Explode()
        {
            float movementMulti = Mathf.Max(1, carryingAgent.GetCurrentMoveSpeed() * moveSpeedDropForceMulti);
            Vector3 direction = (transform.position - carryingAgent.transform.position);
            direction += Vector3.up;

            rb.AddForce(direction.normalized * dropForce * movementMulti);
        }

        public bool TryFollow(Agent agent, Transform target)
        {
            Debug.Assert(target != null, "Target should not be null!");
            Debug.Assert(followTarget == null,$"Already following a target {followTarget}!");
            
            if (followTarget != null)
                return false;

            followTarget = target;
            carryingAgent = agent;

            originalPickupMass = rb.mass;
            rb.mass = pickupMass;
            rb.useGravity = false;
            
            foreach (var collider in physicsColliderParent.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            return true;
        }

        public void StopFollowing()
        {
            rb.mass = originalPickupMass;
            rb.useGravity = true;
            
            foreach (var collider in physicsColliderParent.GetComponentsInChildren<Collider>())
            {
                collider.enabled = true;
            }

            followTarget = null;
        }

        /// <summary>
        /// stored external velocity only gets cleared when a fixedupdate runs,
        /// which may take a few frames (or happen multiple times in a single frame)
        /// so if the value has already been set, do a lazy average to make sure the number doesn't get too bonkers
        /// </summary>
        /// <param name="externalVelocity"></param>
        public void AddExternalMovementVelocity(Vector3 externalVelocity)
        {
            externalMovementVelocity = Mathf.Approximately(externalMovementVelocity.magnitude, 0) ?
                externalVelocity : (externalMovementVelocity + externalVelocity) * 0.5f; // lazy averaging
        }

        private void LateUpdate()
        {
            if (followTarget == null)
                return;

            if (isBeingPlaced)
                return;

            Vector3 offset = rb.transform.position - carriable.GetPickupPivotCentre();
            Vector3 goalPosition = followTarget.position + offset;
            Vector3 flatGoalPosition = goalPosition;
            flatGoalPosition.y = carryingAgent.transform.position.y;
            
            Vector3 rotationDirection = (carryingAgent.transform.position - flatGoalPosition).normalized;
            Quaternion goalRotation = Quaternion.LookRotation(rotationDirection, Vector3.up);

            float currentMoveSpeed = Mathf.Max(moveSpeed, carryingAgent.GetCurrentMoveSpeed());

            rb.transform.position = Vector3.Lerp(rb.transform.position, goalPosition, currentMoveSpeed * Time.deltaTime);
            rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, goalRotation, currentMoveSpeed * Time.deltaTime);
            
            externalMovementVelocity = Vector3.zero;
        }

        /// <summary>
        /// Move carriable object to home position
        /// </summary>
        public async Task RestoreOriginalPositionTask(Vector3 goalPosition, Quaternion goalRotation,
            float duration = 0.5f, float heightOffset = 0.2f)
        {
            isBeingPlaced = true;

            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;

            float moveDuration = duration;
            float rotationDuration = duration * 0.5f;
            float elapsedTime = 0f;

            while (elapsedTime < moveDuration || elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float tMove = Mathf.Clamp01(elapsedTime / moveDuration);
                float tRotate = Mathf.Clamp01(elapsedTime / rotationDuration);

                rb.MovePosition(Vector3.Lerp(startPosition, goalPosition + Vector3.up * heightOffset, tMove));
                rb.MoveRotation(Quaternion.Slerp(startRotation, goalRotation, tRotate));

                await Task.Yield();
            }

            rb.MovePosition(goalPosition);
            rb.MoveRotation(goalRotation);

            isBeingPlaced = false;
        }
    }
}