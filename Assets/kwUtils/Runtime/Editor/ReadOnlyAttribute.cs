using System;
using UnityEngine;

namespace KWUtils
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyFieldAttribute : PropertyAttribute
    {

    }
}
