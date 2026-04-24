using UnityEngine;

namespace Game.Portia
{
    [CreateAssetMenu(fileName = "Crop_", menuName = "Game/Crop")]
    public class CropData : ScriptableObject
    {
        public string     cropName;
        public int        outputGid;
        public int        outputCount = 1;
        public float      growTime    = 60f;
        public GameObject seedlingPrefab;
        public GameObject maturePrefab;
    }
}
