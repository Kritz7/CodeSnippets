using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LooseUnits.Vehicles
{
    public class VehiclePassengerModule : MonoBehaviour, IVehicleModule
    {
        [Header("Car Seats")]
        [SerializeField] private List<Transform> carSeatTransforms;
        
        // Non-serialized
        private Vehicle currentVehicle;
        private IPlayer driver;
        private List<IPlayer> passengers;
        
        // Properties
        public IPlayer CurrentDriver => driver;
        public Transform SeatDriver => carSeatTransforms[0];
        public bool HasDriver => driver != null;

        public void Initialise(Vehicle vehicle)
        {
            passengers = new List<IPlayer>();
            currentVehicle = vehicle;
        }

        public void EnterVehicle(IPlayer passenger, Action<BaseInteractionSuccessData> onSuccess)
        {
            Debug.Assert(passenger != null && !passengers.Contains(passenger),
                $"Car already contains passenger {passenger}!");
            
            Debug.Log($"got a passenger {passenger}!");

            bool success = TryAssignOccupant(passenger, onSuccess);
            
            Debug.Log($"passenger: {HasDriver}");
        }

        public void ExitVehicle(IPlayer passenger, Action<BaseInteractionSuccessData> onSuccess)
        {
            Debug.Assert(passengers != null && passengers.Contains(passenger),
                "Car doesn't contain passenger!");
            
            Debug.Log("Exited vehicle!");

            if (driver == passenger)
                driver = null;

            passengers?.Remove(passenger);
            AutoAssignDriver();
            
            onSuccess?.Invoke(new VehicleEnteredSuccessData(currentVehicle, null));
        }
        
        public bool ContainsPlayer(IPlayer player) => passengers.Contains(player);

        private bool TryAssignOccupant(IPlayer passenger, Action<VehicleEnteredSuccessData> onSuccess, bool forcePromoteToDriver = false)
        {
            // Vehicle is full
            if (passengers.Count == carSeatTransforms.Count)
                return false;
            
            passengers.Add(passenger);
            // TODO: Seats should be a class that can be filled, and this should find the first unavailable seat (or the closest seat to where the player is entering?)
            Transform occupantSeat = carSeatTransforms[passengers.Count - 1];
            
            if (driver == null || forcePromoteToDriver)
            {
                driver = passenger;
            }
            
            onSuccess?.Invoke(new VehicleEnteredSuccessData(currentVehicle, occupantSeat));
            return true;
        }

        private void AutoAssignDriver()
        {
            if (driver != null)
                return;

            IPlayer firstPassenger = passengers.FirstOrDefault();
            driver = firstPassenger;
        }
    }
}