using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LooseUnits.Vehicles
{
    public class VehiclePhysicsController : BaseMoveable, IVehicleModule
    {
        [Header("Setup")]
        [SerializeField, AutoProperty] private Rigidbody rb;
        
        // Non-serialized
        private Vehicle currentVehicle;

        // Properties
        private VehicleMovementSettings vehicleSettings => moveSettings as VehicleMovementSettings;
        public Vector2 StoredMoveInput => unweightedMoveInput;
        public Vector3 CurrentVelocity => rb.linearVelocity;
        
        public void Initialise(Vehicle vehicle)
        {
            currentVehicle = vehicle;
            
            Debug.Log($"Initialised with {vehicle.gameObject}", vehicle.gameObject);
            
            ResetMovementValues();
            movementInput.action.Enable();
            
            AssignMovementSettings(vehicle.Settings.MovementSettings);
        }
        
        protected override void CollectInput()
        {
            unweightedMoveInput = movementInput.action.ReadValue<Vector2>();
        }
        
        protected override void FixedMovementUpdate()
        {
            Debug.Log($"driver? {currentVehicle.VehiclePassengerModule.HasDriver}");
            
            if (!currentVehicle.VehiclePassengerModule.HasDriver)
                return;
            
            FixedMovement();
            FixedMovementSlowdown();
            
            base.FixedMovementUpdate();
        }

        private void FixedMovement()
        {
            Debug.Log($"alright! {currentVehicle.VehiclePassengerModule.HasDriver} and {unweightedMoveInput.magnitude}");
            
            if (!currentVehicle.VehiclePassengerModule.HasDriver)
                return;
            
            if (unweightedMoveInput.magnitude < moveSettings.MovementDeadzoneMagnitude)
                return;

            Vector3 moveInput = unweightedMoveInput.normalized;
            Vector3 worldMoveDirection = new(moveInput.x, 0f, moveInput.y);
            Vector3 forwardBaseMoveDirection = currentVehicle.transform.forward;
            Vector3 force = (worldMoveDirection * CurrentMoveSpeed) +
                            forwardBaseMoveDirection * vehicleSettings.GetForwardBias(rb.linearVelocity.magnitude);
            
            // PFX
            if(force.magnitude / rb.linearVelocity.magnitude > vehicleSettings.ForceSmokeThreshold)
            {
                currentVehicle.VehicleEffectsModule.ActivateTyreSmokePFX();
            }
            
            // Note: ForceMode.Acceleration doesn't need DeltaTime
            rb.AddForce(force, ForceMode.Acceleration);
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, vehicleSettings.MaxVelocity);
        }

        private void FixedMovementSlowdown()
        {
            if (unweightedMoveInput.magnitude > moveSettings.MovementDeadzoneMagnitude)
                return;

            Vector3 deaccelerateForce = -rb.linearVelocity.normalized * vehicleSettings.Deceleration;
            
            // Note: ForceMode.Acceleration doesn't need DeltaTime
            rb.AddForce(deaccelerateForce, ForceMode.Acceleration);
        }
        
        public Vector3 GetGroundNormal()
        {
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,20f);
            return hit.normal;
        }
    }
}
