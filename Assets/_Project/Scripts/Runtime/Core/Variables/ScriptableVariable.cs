using UnityEngine;

namespace GGJ.Core.Variables
{
    public abstract class ScriptableVariable<T> : ScriptableObject
    {
        public T Value;

        public virtual T GetValue => Value;
    }
}
