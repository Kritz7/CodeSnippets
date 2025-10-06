using System;
using System.Linq;
using MoreMountains.Feedbacks;
using MyBox;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace LooseUnits
{
    public class CarriableBase : MonoBehaviour, ICarriable, IInteractable
    {
        [Header("Setup")]
        [SerializeField, AutoProperty] private InteractionReceiver receiver;
        [SerializeField, AutoProperty] private NavMeshObstacle navMeshObstacle;
        [SerializeField, AutoProperty] private ItemMovementModule movementModule;
        [SerializeField] private Transform[] pickupHandlePivots;

        [Header("Feedback")]
        [SerializeField] private MMF_Player pickupFeedback;
        
        [Header("Physics Setup")]
        [SerializeField] private Collider triggerCollider;

        [Header("Pickup Settings")]
        [SerializeField] private bool oneHanded;
        
        [Header("AI Settings")]
        [SerializeField] private float homeDistanceAllowance = 0.5f;

        // Non-serialized
        private IAgent targetAgent;
        private Vector3 storedHomePosition;
        private Quaternion storedHomeRotation;
        private bool shouldCarveNavMesh = false;
        
        // Properties
        public IAgent HoldingAgent => targetAgent;
        public Vector3 StoredHomePosition => storedHomePosition;
        public float DistanceFromHome => Vector3.Distance(transform.position, StoredHomePosition);
        public bool IsHeld => targetAgent != null;
        public bool IsHome => DistanceFromHome <= homeDistanceAllowance;
        public bool AllowInteraction => movementModule.AllowInteraction && !IsHeld;
        public bool OneHanded => oneHanded;

        private void OnEnable()
        {
#if UNITY_EDITOR
            Editor_VerifyInteractable();
#endif
            AssignActions();

            shouldCarveNavMesh = navMeshObstacle.carving;
            storedHomePosition = transform.position;
            storedHomeRotation = transform.rotation;
        }

        private void OnDisable()
        {
            UnassignActions();
        }

        private void FixedUpdate()
        {
            navMeshObstacle.carving = shouldCarveNavMesh && !IsHeld;
        }
        
        public bool TryInteract(InteractionData data)
        {
            receiver.PerformActions(data);
            return true;
        }
        
        public Transform[] GetPickupPivots() => pickupHandlePivots;
        public Vector3 GetPickupPivotCentre()
        {
            Vector3 sumPositions = pickupHandlePivots.Aggregate(Vector3.zero,
                (sum, t) => sum + t.position);
            return sumPositions / pickupHandlePivots.Length;
        }

        public void OnCarriableInteracted(InteractionData data)
        {
            if (IsHeld)
            {
                Drop(data.success, ()=>movementModule.Explode());
            }
            else
            {
                Carry(data.caller, data.success);
            }
        }

        public virtual bool Carry(IAgent target, Action<CarriableCarriedSuccessData> onSuccess = null)
        {
            if (IsHeld)
                return false;
            
            targetAgent = target;
            navMeshObstacle.enabled = false;

            Transform carryTransform = target.GetIKModule().GetCarryFollowTarget(oneHanded);
            if (!movementModule.TryFollow(target as Agent, carryTransform))
                return false;
            
            pickupFeedback?.PlayFeedbacks();
            
            target.AssignHoldingCarriable(this);
            onSuccess?.Invoke(new CarriableCarriedSuccessData(this));
            return true;
        }

        public async void DropRestorePosition()
        {
            Drop(null, ()=>
            {
                movementModule.DropRestorePosition(storedHomePosition, storedHomeRotation);
            });
        }

        public virtual void Drop(Action<CarriableDroppedSuccessData> onSuccess = null, Action onComplete = null)
        {
            if (!IsHeld)
                return;
            
            movementModule.StopFollowing(); 
            targetAgent?.UnassignHoldingCarriable(this);
            
            targetAgent = null;
            navMeshObstacle.enabled = true;
            
            pickupFeedback?.PlayFeedbacks();
            
            onSuccess?.Invoke(new CarriableDroppedSuccessData());
            onComplete?.Invoke();
        }
        
        private void AssignActions()
        {
            receiver.AssignAction(OnCarriableInteracted);
        }
        
        private void UnassignActions()
        {
            receiver.UnassignAction(OnCarriableInteracted);
        }

#if UNITY_EDITOR
        private void Editor_VerifyInteractable()
        {
            bool interactableLayer = false;
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.layer == IInteractable.InteractableLayer)
                {
                    interactableLayer = true;
                    break;
                }
            }
            
            // Make sure object has a `Triggers` child that contains a trigger collider, with the gameObject marked with the correct layer.
            Debug.Assert(interactableLayer, $"There is no interactable layer assigned to object {transform.name}!");
        }
        
        public void Debug_SetPickupHandles(Transform[] handles)
        {
            pickupHandlePivots = handles;
        }

        [ButtonMethod]
        public void Debug_NPCCarry()
        {
            AI.NPC npc = GameObject.FindAnyObjectByType<AI.NPC>(FindObjectsInactive.Exclude);
            Carry(npc, (x) => Debug.Log($"{npc} carrying {name} {x.carriable.name}"));
        }
#endif
    }
}
