using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace LooseUnits
{
    public class MaterialVariant : MonoBehaviour
    {
        [Header("Variant Settings")]
        [SerializeField] private Vector4 offset = Vector4.one;
        [SerializeField] private Color tint = Color.white;
        
        [Header("Config")]
        [SerializeField] private MeshRenderer meshRenderer;
        
        private static readonly int MainTexSt = Shader.PropertyToID("_MainTex_ST");
        private static readonly int MainColor = Shader.PropertyToID("_Color");

        private void OnValidate()
        {
            meshRenderer ??= GetComponentInChildren<MeshRenderer>(true);
        }
        

        void Start()
        {
            UpdateMaterial();
        }

        [ButtonMethod]
        private void UpdateMaterial()
        {
            if (!meshRenderer)
                return;
            
            // Create a new MaterialPropertyBlock
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

            // Set a random color in the MaterialPropertyBlock
            propertyBlock.SetVector(MainTexSt, offset);
            propertyBlock.SetColor(MainColor, tint);

            // Apply the MaterialPropertyBlock to the GameObject
            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        [ButtonMethod]
        private void ClearPropertyBlock()
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}