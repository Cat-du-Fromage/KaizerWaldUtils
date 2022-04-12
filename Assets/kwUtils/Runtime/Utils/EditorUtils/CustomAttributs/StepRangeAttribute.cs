using System;
using UnityEditor;
using UnityEngine;
namespace KWUtils
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class StepRange : PropertyAttribute
    {
        public readonly float min = .0f;
        public readonly float max = 100.0f;
        public readonly float step = 1.0f;
        public readonly string label = "";
     
        public StepRange(float min, float max, float step = 1.0f, string label = "")
        {
            this.min = min;
            this.max = max;
            this.step = step;
            this.label = label;
        }
    }
     
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(StepRange))]
    internal sealed class RangeStepDrawer : PropertyDrawer
    {
     
        /**
         * Return exact precision of reel decimal
         * ex :
         * 0.01     = 2 digits
         * 0.02001  = 5 digits
         * 0.02000  = 2 digits
         */
        private int Precision(float value)
        {
            if (value == .0f) return 0;
            int precision = value.ToString().Length - (((int)value).ToString().Length + 1);
            // Math.Round function get only precision between 0 to 15
            return Mathf.Clamp(precision, 0, 15);
        }
     
        /**
         * Return new float value with step calcul (and step decimal precision)
         */
        private float Step(float value, float min, float step)
        {
            if (step == 0) return value;
            float newValue = min + Mathf.Round((value - min) / step) * step;
            return (float)Math.Round(newValue, Precision(step));
        }
     
        /**
         * Return new integer value with step calcul
         * (It's more simple ^^)
         */
        private int Step(int value, int min, int step)
        {
            if (step == 0) return value;
            int newValue = min + (value - min) / step * step;
            return newValue;
        }
     
        //
        // Methods
        //
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            StepRange rangeAttribute = (StepRange)base.attribute;
     
            if (rangeAttribute.label != "")
                label.text = rangeAttribute.label;
     
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    float floatValue = EditorGUI.Slider(position, label, property.floatValue, rangeAttribute.min, rangeAttribute.max);
                    property.floatValue = Step(floatValue, rangeAttribute.min, rangeAttribute.step);
                    break;
                case SerializedPropertyType.Integer:
                    int intValue = EditorGUI.IntSlider(position, label, property.intValue, (int)rangeAttribute.min, (int)rangeAttribute.max);
                    property.intValue = Step(intValue, (int)rangeAttribute.min, (int)rangeAttribute.step);
                    break;
                default:
                    EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
                    break;
            }
        }
    }
    #endif
}
