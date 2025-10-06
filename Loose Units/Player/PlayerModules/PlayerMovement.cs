using DG.Tweening;
using LooseUnits.Vehicles;
using MyBox;
using UnityEngine;

namespace LooseUnits
{
    public class PlayerMovement : BaseMoveable, IAgentModule
    {
        [Header("Player Model")]
        [SerializeField] private Transform playerModel;
        [SerializeField] private float rotationSpeed = 20f;
        
        [Header("PFX")]
        [SerializeField] private float skidPFXSpeed = 0.8f;
        
        // Non-serialized
        private Player currentPlayer;
        private Vehicle currentVehicle;
        private Transform currentVehicleSeat;
        private float temp_lerpSpeed = 50f;
        private Vector3 storedMoveDirection = Vector3.zero;
        private bool isDiving = false;
        private float diveDuration = 1f;
        
        // Properties
        public Vector3 MoveVelocity => storedMoveDirection * CurrentMoveSpeed;
        public bool IsDiving => isDiving;
        private bool isInVehicle => currentVehicleSeat != null;

        public void Initialise(IAgent player)   
        {
            currentPlayer = (Player)player;

            ResetMovementValues();
            movementInput.action.Enable();
            
            AssignMovementSettings(currentPlayer.Settings.MovementSettings);
        }

        public void SetCurrentVehicle(Vehicle vehicle, Transform seat)
        {
            currentVehicle = seat != null ? vehicle : null;
            currentVehicleSeat = seat;
        }
        
        protected override void MovementUpdate()
        {
            if (isInVehicle)
            {
                VehicleMovementUpdate();
            }
            else
            {
                Vector2 currentMovementInput = GetWeightedMoveInput();
                Vector3 goalPlayerMoveDirection = new(currentMovementInput.x, 0f, currentMovementInput.y);

                if(CurrentMoveSpeed > 0 && goalPlayerMoveDirection.magnitude < skidPFXSpeed)
                    currentPlayer.EffectsModule.ActivateSkidPFX();
            
                Move(goalPlayerMoveDirection);
                ModelRotationUpdate();
            }
        }

        private void ModelRotationUpdate()
        {
            if (storedMoveDirection.magnitude < 0.1f)
                return;
            
            Quaternion targetRotation = Quaternion.LookRotation(storedMoveDirection.normalized * 2f);
            playerModel.localRotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void VehicleMovementUpdate()
        {
            currentPlayer.transform.position = Vector3.Lerp(currentPlayer.transform.position, currentVehicleSeat.transform.position,
                temp_lerpSpeed * Time.deltaTime);
        }

        private void Move(Vector3 movementDirection)
        {
            storedMoveDirection = movementDirection * CurrentMoveSpeed;
            currentPlayer.transform.position += storedMoveDirection * Time.deltaTime;
        }

        public void Dive()
        {
            if (isDiving)
                return;
            
            currentPlayer.GetModule<MovementAnimatorModule>().PlayDiveAnimation();

            Sequence diveSequence = DOTween.Sequence();
            diveSequence.AppendCallback(() => isDiving = true);
            diveSequence.AppendInterval(diveDuration);
            diveSequence.AppendCallback(() => isDiving = false);

            diveSequence.Play();
        }
        
        #if UNITY_EDITOR
        [ButtonMethod]
        public void Debug_PingMovementSettings()
        {
            ScriptableObjectHelper.PingAsset<PlayerMovementSettings>();
        }
        #endif
    }
}