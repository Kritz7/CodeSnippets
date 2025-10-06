using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace LooseUnits
{
    public class BaseMoveable : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] protected InputActionReference movementInput;
        
        // Non-serialized
        protected Vector2 unweightedMoveInput;
        protected float currentHeldDuration;
        
        private List<InputSample> inputSamples = new();
        private Vector2 storedWeightedMoveInput;
        
        // Properties
        protected MovementSettings moveSettings { get; private set; }
        public float CurrentMoveSpeed => moveSettings.CalculateMoveSpeed(currentHeldDuration);

        public void AssignMovementSettings(MovementSettings settings)
        {
            moveSettings = settings;
        }
        
        protected virtual void MovementUpdate() { }
        protected virtual void FixedMovementUpdate() { }
        
        public virtual void ResetMovementValues()
        {
            inputSamples.Clear();
            unweightedMoveInput = Vector2.zero;
            currentHeldDuration = 0;
        }
        
        private void Update()
        {
            CollectInput();
            MovementHeldUpdate();
            MovementUpdate();
        }

        private void FixedUpdate()
        {
            FixedMovementUpdate();
        }

        protected virtual void CollectInput()
        {
            unweightedMoveInput = movementInput.action.ReadValue<Vector2>();
            
            float turnSpeedMulti = TurnSpeedMultiplier(storedWeightedMoveInput);
            
            inputSamples.Add(new InputSample
            {
                direction = unweightedMoveInput.normalized,
                turnMulti = turnSpeedMulti,
                timestamp = Time.time
            });
            
            inputSamples.RemoveAll(sample => Time.time - sample.timestamp > moveSettings.SampleDuration);
        }

        private void MovementHeldUpdate()
        {
            // I (probably?) only care about this frame's input, not the input average, for this calc.
            Vector2 currentMovementInput = unweightedMoveInput;
            
            bool aboveThreshold = currentMovementInput.magnitude > moveSettings.MovementDeadzoneMagnitude;

            currentHeldDuration += aboveThreshold ? Time.deltaTime : -Time.deltaTime * moveSettings.InactiveInputDecreaseMulti;
            currentHeldDuration = Mathf.Clamp(currentHeldDuration, 0, moveSettings.TimeUntilMaxMoveSpeed);
        }

        protected float TurnSpeedMultiplier(Vector3 weightedMoveInput)
        {
            float angle = Vector2.Angle(unweightedMoveInput, new Vector2(weightedMoveInput.x, weightedMoveInput.z));
            float turnSpeedMultiplier = moveSettings.CalculateTurnMultiplier(angle);
            
            return turnSpeedMultiplier;
        }
        
        protected Vector2 GetWeightedMoveInput()
        {
            if (inputSamples.Count == 0)
                return Vector2.zero;

            Vector2 weightedSum = Vector2.zero;
            float totalWeight = 0f;

            foreach (var sample in inputSamples)
            {
                float timeSinceSample = Time.time - sample.timestamp;
                float normalizedTime = Mathf.Clamp01(timeSinceSample / moveSettings.SampleDuration);
                float weight = moveSettings.WeightedSamplingCurve.Evaluate(normalizedTime);

                weightedSum += sample.direction * (weight * sample.turnMulti);
                totalWeight += weight;
            }

            Vector2 weightedMoveInput = totalWeight > 0 ? weightedSum / totalWeight : Vector2.zero;
            storedWeightedMoveInput = weightedMoveInput;
            
            return weightedMoveInput;
        }
        
        private struct InputSample
        {
            public Vector2 direction;
            public float turnMulti;
            public float timestamp;
        }
    }
}