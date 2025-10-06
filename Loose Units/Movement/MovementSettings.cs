using MyBox;
using UnityEngine;

namespace LooseUnits
{
    public class MovementSettings : ScriptableObject
    {
        // Constants
        private const float maxTurnAngle = 180; // half a circle
        
        [Header("Input Sampling")]
        [SerializeField] private float sampleDuration = 0.5f;
        [SerializeField] private AnimationCurve weightedSamplingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Speed Values")]
        [SerializeField] private float baseMoveSpeed = 10f;

        [Header("Turning")]
        [SerializeField] private float minTurnSlowdownAngle = 15f;
        [SerializeField, MaxValue(maxTurnAngle)] private float maxTurnSlowdownAngle = 110f;

        [Header("Time Values")]
        [SerializeField] private float timeUntilMaxMoveSpeed = 1f;
        [SerializeField] private float inactiveInputDecreaseMulti = 5f;

        [Header("Deadzone")]
        [SerializeField] private float movementDeadzoneMagnitude = 0.01f;

        [Header("Movement Curves")]
        [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve turnSlowdownCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // Properties
        public AnimationCurve WeightedSamplingCurve => weightedSamplingCurve;
        public float MovementDeadzoneMagnitude => movementDeadzoneMagnitude;
        public float TimeUntilMaxMoveSpeed => timeUntilMaxMoveSpeed;
        public float InactiveInputDecreaseMulti => inactiveInputDecreaseMulti;
        public float SampleDuration => sampleDuration;

        public float CalculateMoveSpeed(float currentDirectionalMoveDuration)
        {
            return baseMoveSpeed * accelerationCurve.Evaluate(currentDirectionalMoveDuration / TimeUntilMaxMoveSpeed);
        }

        public float CalculateTurnMultiplier(float turnAngleDifference)
        {
            float t = Mathf.InverseLerp(minTurnSlowdownAngle, maxTurnSlowdownAngle, turnAngleDifference);
            return turnSlowdownCurve.Evaluate(t);
        }
    }
}