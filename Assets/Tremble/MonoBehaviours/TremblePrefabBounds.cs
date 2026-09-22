using UnityEngine;

namespace TinyGoose.Tremble
{
    public class TremblePrefabBounds : MonoBehaviour
    {
        [SerializeField] private Bounds bounds;
        [SerializeField] private bool overrideBounds;
        
        public Bounds Bounds => bounds;
        public bool OverrideBounds => overrideBounds;

        public void ApplyToBounds(ref Bounds inputBounds)
        {
            if (OverrideBounds)
                inputBounds.SetMinMax(Bounds.min, Bounds.max);
            else
                inputBounds.Encapsulate(Bounds);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + Bounds.center, Bounds.size);
        }
    }
}