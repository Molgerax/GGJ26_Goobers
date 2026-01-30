using UnityEngine;

namespace GGJ.Core.Variables.Types
{
    [CreateAssetMenu(menuName = "Beakstorm/ScriptableVariable/Float Range")]
    public class RangeVariable : FloatVariable
    {
        public float Min;
        public float Max;

        public float Get01 => Max - Min == 0 ? 0 : Mathf.Clamp01((Value - Min) / (Max - Min));

        public override float GetValue => Get01;
    }
}
