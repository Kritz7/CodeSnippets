using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LooseUnits
{
    // Interface for agents - players, humanoid NPCs
    public interface IAgent
    {
        public void AssignHoldingCarriable(ICarriable carriable);
        public void UnassignHoldingCarriable(ICarriable carriable);
        public AgentHandIKModule GetIKModule();
    }
}
