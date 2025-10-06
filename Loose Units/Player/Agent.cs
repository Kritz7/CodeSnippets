using System;
using System.Collections.Generic;
using System.Linq;
using LooseUnits.AI;
using MyBox;
using UnityEngine;

namespace LooseUnits
{
    public class Agent : MonoBehaviour, IAgent
    {
        // Non-serialized
        protected readonly List<IAgentModule> agentModules = new();
        protected NPCNavAgentModule navAgentModule;
        private CarriableBase holdingCarriable;
        private BaseMoveable moveModule;
        private AgentHandIKModule ikModule;
        
        // Properties
        public bool IsHolding => holdingCarriable != null;
        public CarriableBase HoldingCarriable => holdingCarriable;
        public AgentHandIKModule GetIKModule() => ikModule ??= GetComponentInChildren<AgentHandIKModule>();

        private void OnEnable()
        {
            moveModule = GetComponentInChildren<BaseMoveable>();
            navAgentModule = GetComponentInChildren<NPCNavAgentModule>();
        }
        
        private void Awake()
        {
            RefreshModulesList();
            InitialiseModules();
            Initialise();
        }

        protected virtual void Initialise() { }
        
        public float GetCurrentMoveSpeed()
        {
            if (moveModule)
                return moveModule.CurrentMoveSpeed;

            if (navAgentModule)
                return navAgentModule.NavMeshAgent.velocity.magnitude;

            return 0;
        }
        
        public T GetModule<T>() where T : IAgentModule
        {
            foreach (T module in agentModules.OfType<T>())
            {
                return module;
            }

            return default;
        }
        
        public void AssignHoldingCarriable(ICarriable carriable)
        {
            if(holdingCarriable)
                UnassignHoldingCarriable(holdingCarriable);
            
            holdingCarriable = (CarriableBase)carriable;
        }

        public void UnassignHoldingCarriable(ICarriable carriable)
        {
            holdingCarriable = null;
        }
        
        private void InitialiseModules()
        {
            agentModules.ForEach(pm => pm.Initialise(this));
        }

        
        private void RefreshModulesList()
        {
            GetComponentsInChildren<IAgentModule>(true).ForEach(x => agentModules.Add(x));
        }
    }
}