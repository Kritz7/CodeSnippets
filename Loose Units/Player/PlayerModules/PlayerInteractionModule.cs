using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LooseUnits
{
    public class PlayerInteractionModule : MonoBehaviour, IAgentModule
    {
        [Header("Input")]
        [SerializeField] protected InputActionReference interactionInput;
        [SerializeField] protected InputActionReference moveActionInput;

        [Header("Triggers")]
        [SerializeField] private InteractionTriggerSensor trigger;
        
        private Player currentPlayer;

        public void Initialise(IAgent player)
        {
            currentPlayer = (Player)player;
            
            interactionInput.action.Enable();
            moveActionInput.action.Enable();
        }

        private void OnEnable()
        {
            interactionInput.action.started += OnInteractionPressed;
            moveActionInput.action.started += OnMovementActionPressed;
        }

        private void OnDisable()
        {
            interactionInput.action.started -= OnInteractionPressed;
            moveActionInput.action.started -= OnMovementActionPressed;
        }
        
        private void OnMovementActionPressed(InputAction.CallbackContext obj)
        {
            currentPlayer.Movement.Dive();
        }
        
        private void OnInteractionPressed(InputAction.CallbackContext obj)
        {
            var interactable = trigger.GetClosestInteractable();
            interactable?.TryInteract(new InteractionData(currentPlayer, null, InteractionSuccess));
            
            // TODO: play some interaction success/fail noise?
        }

        private void InteractionSuccess(BaseInteractionSuccessData data)
        {
            // Funnel this event into all other interaction listeners
            currentPlayer.SendListenersEvent(data);
        }
    }
}
