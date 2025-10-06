using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace LooseUnits.Vehicles
{
    public class Vehicle : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private VehicleSettings vehicleSettings;

        [Header("Prefabs")]
        [SerializeField] private GameObject carPhysicsPrefab;
        
        [Header("Modules")]
        [SerializeField, AutoProperty] private VehicleMovement vehicleMovement;
        [SerializeField, AutoProperty] private VehiclePassengerModule vehiclePassengerModule;
        [FormerlySerializedAs("effectsModule")] [SerializeField, AutoProperty] private VehicleEffectsModule vehicleEffectsModule;
        [SerializeField, AutoProperty] private InteractionReceiver interactionReceiver;
        
        // Non-serialized
        private readonly List<IVehicleModule> vehicleModules = new();
        private VehiclePhysicsController physicsController;

        // Properties
        public VehicleSettings Settings => vehicleSettings;
        public VehiclePassengerModule VehiclePassengerModule => vehiclePassengerModule;
        public VehicleMovement Movement => vehicleMovement;
        public VehiclePhysicsController PhysicsController => physicsController;
        public VehicleEffectsModule VehicleEffectsModule => vehicleEffectsModule;

        private void Awake()
        {
            InitialisePhysicsController();
            RefreshModulesList();
            InitialiseModules();
        }

        private void OnEnable()
        {
            AssignVehicleActions();
        }

        private void OnDisable()
        {
            UnassignVehicleActions();
        }
        
        public bool TryInteract(InteractionData data)
        {
            interactionReceiver.PerformActions(data);
            return true;
        }

        private void AssignVehicleActions()
        {
            interactionReceiver.AssignAction(OnVehicleInteracted);
        }
        
        private void UnassignVehicleActions()
        {
            interactionReceiver.UnassignAction(OnVehicleInteracted);
        }

        private void OnVehicleInteracted(InteractionData interactionData)
        {
            if (vehiclePassengerModule.ContainsPlayer((IPlayer)interactionData.caller))
            {
                vehiclePassengerModule.ExitVehicle((IPlayer)interactionData.caller, interactionData.success);
            }
            else
            {
                vehiclePassengerModule.EnterVehicle((IPlayer)interactionData.caller, interactionData.success);
            }
        }

        private void InitialisePhysicsController()
        {
            physicsController = Instantiate(carPhysicsPrefab, transform.position, transform.rotation).GetComponent<VehiclePhysicsController>();
            PhysicsController.AssignMovementSettings(Settings.MovementSettings);
        }

        private void InitialiseModules()
        {
            vehicleModules.ForEach(pm => pm.Initialise(this));
        }

        private void RefreshModulesList()
        {
            GetComponentsInChildren<IVehicleModule>(true).ForEach(vm => vehicleModules.Add(vm));
            vehicleModules.Add(physicsController);
        }
    }
}
