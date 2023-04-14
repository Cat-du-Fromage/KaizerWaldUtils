using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    public struct ChunkComponent
    {
        public bool4 OpenSides;
        
        // Il faudra générer les portes à l'externe, les portes ici seront des références
        public List<Gate> topGates;
        public List<Gate> rightGates;
        public List<Gate> bottomGates;
        public List<Gate> leftGates;
        
    }
}
