using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace LooseUnits
{
    public class Player : Agent, IPlayer
    {
        [Header("Settings")]
        [SerializeField] private PlayerSettings playerSettings;
        
        [Header("Modules")]
        [SerializeField, AutoProperty] private PlayerMovement playerMovement;
        [SerializeField, AutoProperty] private PlayerEffectsModule effectsModule;
        [SerializeField, AutoProperty] private PlayerInteractionModule playerInteraction;
        
        // Non-serialized
        private readonly List<IPlayerInteractionListener> interactionListeners = new();

        // Properties
        public PlayerSettings Settings => playerSettings;
        public PlayerInteractionModule PlayerInteraction => playerInteraction;
        public PlayerEffectsModule EffectsModule => effectsModule;
        public PlayerMovement Movement => playerMovement;

        protected override void Initialise()
        {
            RefreshListenersList();
            
            base.Initialise();
        }

        public void SendListenersEvent(BaseInteractionSuccessData data)
        {
            interactionListeners.ForEach(x => x.OnInteractionReceived(data));
        }
        
        private void RefreshListenersList()
        {
            GetComponentsInChildren<IPlayerInteractionListener>(true).ForEach(x => interactionListeners.Add(x));
        }
    }
}