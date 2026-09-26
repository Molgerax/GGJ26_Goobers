using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Rendering.Portals
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Light))]
    public class LightEnforcer : MonoBehaviour
    {
        public readonly static HashSet<LightEnforcer> ActiveLightEnforcers = new();
        
        private Light _light;

        public Light Light => _light;

        [NonSerialized] public bool ForceVisible;

        public void SetForcedVisible(bool value)
        {
            _light.useBoundingSphereOverride = value;
            _light.forceVisible = value;

            if (value)
                _light.boundingSphereOverride = new(0, 0, 0, 1000000);
        }

        private void OnEnable()
        {
            _light = GetComponent<Light>();
            ActiveLightEnforcers.Add(this);
        }

        private void OnDisable()
        {
            ActiveLightEnforcers.Remove(this);
        }
    }
}