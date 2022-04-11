using UnityEngine;

namespace KWUtils.KwTerrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider colliderMesh;
        
        [SerializeField] private MapSettings mapSettings;
        
        public MapSettings GetMapSettings() => mapSettings;

        public void NewGameSettings(TerrainType[] regions)
        {
            mapSettings.NewGame();
            SetPositionToZero();
            SetMesh();
        }

        public void SetPositionToZero()
        {
            gameObject.transform.position =
                Vector3.zero - new Vector3(mapSettings.mapSize / 2f, 2, mapSettings.mapSize / 2f);
        }

        private void SetMesh()
        {
            meshFilter.sharedMesh = colliderMesh.sharedMesh = MeshGenerator.GetTerrainMesh(mapSettings);
        }

    }
}