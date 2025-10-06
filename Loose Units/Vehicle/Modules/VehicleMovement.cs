using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace LooseUnits.Vehicles
{
    public class VehicleMovement : MonoBehaviour, IVehicleModule
    {
        [Header("Transform Config")]
        [SerializeField] private Transform carModel;
        [SerializeField] private Transform[] frontWheelPivots;
        [SerializeField] private Transform[] frontWheels;

        [Header("Positioning Settings")]
        [SerializeField] private Vector3 baseOffset;

        [Header("Rotation Settings")]
        [SerializeField] private float modelRotationYMax = 200f;
        [SerializeField] private float modelRotationSpeed = 5f;
        [SerializeField] private float minAngleForModelRotation = 15f;
        
        // Non-serialized
        private Vehicle currentVehicle;
        private VehiclePhysicsController physicsController;
        private Vector3 goalModelRotation;
        
        // Properties
        private VehicleMovementSettings vehicleMoveSettings => currentVehicle.Settings.MovementSettings;

        public void Initialise(Vehicle vehicle)
        {
            currentVehicle = vehicle;
            physicsController = currentVehicle.PhysicsController;
        }

        private void LateUpdate()
        {
            LateMovementUpdate();
        }

        protected void LateMovementUpdate()
        {
            UpdateCarBasePosition();
            UpdateModelLocalRotation();
            
            if (physicsController.StoredMoveInput.magnitude < 0.1f)
            {
                goalModelRotation = Vector3.Lerp(goalModelRotation, Vector3.zero, modelRotationSpeed * 5f * Time.deltaTime);
                return;
            }

            if (!currentVehicle.VehiclePassengerModule.HasDriver)
                return;

            UpdateTargetLocalRotation();
            UpdateCarFacing();
        }
        
        private void UpdateCarFacing()
        {
            Vector3 pivot = physicsController.transform.position;
            Vector3 targetLookDirection = physicsController.CurrentVelocity.normalized;
            pivot.y = targetLookDirection.y = 0;

            Vector3 currentDirection = currentVehicle.transform.forward;

            if (Vector3.Angle(targetLookDirection, currentDirection) < 0.5f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(targetLookDirection);
            Quaternion smoothedRotation = Quaternion.RotateTowards(currentVehicle.transform.rotation, targetRotation, vehicleMoveSettings.MaxWheelDegrees);
            Quaternion rotationDelta = smoothedRotation * Quaternion.Inverse(currentVehicle.transform.rotation);

            rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);

            if (angle < 1f)
                return;

            float turnAngle = angle * vehicleMoveSettings.TurnSpeed * Time.deltaTime;

            currentVehicle.transform.RotateAround(pivot, axis, turnAngle);
        }

        private void UpdateCarBasePosition()
        {
            currentVehicle.transform.position = physicsController.transform.position + 
                                                transform.forward * baseOffset.z +
                                                transform.up * baseOffset.y;
        }

        private void UpdateModelLocalRotation()
        {
            carModel.transform.localRotation = Quaternion.Slerp(carModel.localRotation,
                Quaternion.Euler(goalModelRotation),
                modelRotationSpeed * Time.deltaTime);
        }

        private void UpdateTargetLocalRotation()
        {
            Vector2 moveInput = physicsController.StoredMoveInput;
            Vector3 worldDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 vehicleDirection = currentVehicle.transform.forward;
            vehicleDirection.y = 0;
            
            float angleDelta = Vector3.SignedAngle(vehicleDirection, worldDirection, Vector3.up);

            goalModelRotation = Mathf.Abs(angleDelta) > minAngleForModelRotation
                ? new Vector3(0f, angleDelta > 0 ? modelRotationYMax : -modelRotationYMax, 0f)
                : Vector3.zero;

            float rotationT = Mathf.InverseLerp(minAngleForModelRotation, 90f, Mathf.Abs(angleDelta));
            goalModelRotation *= rotationT;
        }
        
        Vector3 ClampDirectionAngle(Vector3 inputDir, Vector3 refDir, float minAngle, float maxAngle)
        {
            inputDir.Normalize();
            refDir.Normalize();
            
            float angle = Vector3.Angle(refDir, inputDir);

            if (angle >= minAngle && angle <= maxAngle)
            {
                return inputDir;
            }
            
            float clampedAngle = Mathf.Clamp(angle, minAngle, maxAngle);
            float t = clampedAngle / angle;
            
            return Vector3.Slerp(refDir, inputDir, t).normalized;
        }

        private Vector3 GetAverageFrontWheelPosition()
        {
            return frontWheels.Select(x => x.position).
                Aggregate(Vector3.zero, (sum, val) => sum + val)
                   / frontWheels.Length;
        }
        
        private Vector3 GetAverageFrontWheelDirection()
        {
            return frontWheels.Select(x => x.forward).
                       Aggregate(Vector3.zero, (sum, val) => sum + val)
                   / frontWheels.Length;
        }
    }
}