using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class RangePow2 : PropertyAttribute
    {
        public readonly int min = 1;
        public readonly int max = 512;
        public readonly string label = "";
     
        public RangePow2(int min, int max, string label = "")
        {
            this.min = math.max(ceilpow2(min),1);
            this.max = ceilpow2(max);
            this.label = label;
        }
    }
     
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RangePow2))]
    internal sealed class RangePow2Drawer : PropertyDrawer
    {
        /**
         * Return new integer value with step calcul
         * (It's more simple ^^)
         */
        private int Step(int value, int min)
        {
            if (floorlog2(value) == 0) value = 2;
            int newValue = floorlog2(value) + 1;
            return newValue;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RangePow2 rangeAttribute = (RangePow2)base.attribute;
     
            if (rangeAttribute.label != "")
                label.text = rangeAttribute.label;
     
            int intValue = EditorGUI.IntSlider(position, label, property.intValue, rangeAttribute.min, rangeAttribute.max);
            property.intValue = Step(intValue, rangeAttribute.min);
        }
    }
    #endif
}
