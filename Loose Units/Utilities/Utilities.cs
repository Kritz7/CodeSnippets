using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.GraphDesigner.Runtime.Variables;
using UnityEngine;

namespace LooseUnits
{
    public class Utilities
    {
        /// <summary>
        /// Finds the object of type T, given some predicate, which is closest to the target.
        /// </summary>
        public static T GetClosestMatching<T>(Transform target, T[] objectsOfType, Func<T, bool> evaluation) where T : MonoBehaviour
        {
            objectsOfType = objectsOfType.ToList().Where(obj => evaluation.Invoke(obj)).ToArray();
            
            T closest = null;
            float closestDistance = 9999;

            if (objectsOfType.Length == 1)
                return objectsOfType.First();
            
            objectsOfType.ForEach(p =>
            {
                var distance = Vector3.Distance(target.position, p.transform.position);
                if (!(distance < closestDistance))
                    return;

                closestDistance = distance;
                closest = p;
            });

            return closest;
        }

        public static T GetClosest<T>(Transform target, List<T> list) where T : MonoBehaviour
        {
            T closest = null;
            float closestDistance = 9999;
            
            if (list.Count == 1)
                return list.First();

            foreach (var item in list)
            {
                var distance = Vector3.Distance(target.position, item.transform.position);
                if (!(distance < closestDistance))
                    continue;

                closestDistance = distance;
                closest = item;
            }
            
            return closest;
        }
        
        public static List<MonoBehaviour> GetAllWithinRange(Transform target, Type t, float range)
        {
            List<MonoBehaviour> closest = new List<MonoBehaviour>();
            var AllOfType = GameObject.FindObjectsByType(t, FindObjectsSortMode.None).Select(x=>(MonoBehaviour)x).ToList();
            if (AllOfType.Count == 1)
                return AllOfType;

            foreach (var item in AllOfType)
            {
                var distance = Vector3.Distance(target.position, item.transform.position);
                if (distance > range)
                    continue;
                
                closest.Add(item);
            }

            return closest;
        }
        
        public static void ReloadGOSharedVariable<T>(ref SharedVariable<T> variable, BehaviorTree behaviorTree)
        {
            if (variable.Value == null)
            {
                variable = behaviorTree.GetVariable<T>(variable.Name, SharedVariable.SharingScope.GameObject);
            }
        }
            
        public static void ReloadGOSharedVariable<T>(ref SharedVariable<T> variable, string name, BehaviorTree behaviorTree)
        {
            if (variable.Value == null)
            {
                variable = behaviorTree.GetVariable<T>(name, SharedVariable.SharingScope.GameObject);
            }
        }
    }
}