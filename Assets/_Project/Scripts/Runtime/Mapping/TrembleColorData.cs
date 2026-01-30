using System;
using System.Collections.Generic;
using GGJ.Core.Interfaces;
using UnityEngine;

namespace GGJ.Mapping
{
    [CreateAssetMenu(fileName = "TrembleColorData", menuName = "GGJ/Tremble/ColorData", order = 0)]
    public class TrembleColorData : ScriptableObject
    {
        [SerializeField] public List<DataPair> pairs = new();
        
        [Serializable]
        public class DataPair
        {
            public SerializableType Type;
            public Color Color;

            public DataPair(Type type, Color color)
            {
                Type = type;
                Color = color;
            }
        }
    }
}