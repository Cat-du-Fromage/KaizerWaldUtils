using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KWUtils.KWGenericGrid
{
    public abstract class AbstractGridHandler<T1, T2, T3> : MonoBehaviour
    where T1 : struct
    where T2 : GenericGrid<T1>
    where T3 : Enum
    {
        public T2 Grid { get; private set; }
        public T3 GridType { get; private set; }

        public abstract void GetThings();
    }
}
