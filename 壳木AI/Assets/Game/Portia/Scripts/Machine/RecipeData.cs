using UnityEngine;

namespace Game.Portia
{
    public enum MachineType { Sawmill, Furnace }

    [CreateAssetMenu(fileName = "Recipe_", menuName = "Game/Recipe")]
    public class RecipeData : ScriptableObject
    {
        public MachineType machineType;
        public string      recipeName;
        public int         inputGid;
        public int         inputCount  = 1;
        public int         outputGid;
        public int         outputCount = 1;
        public float       processTime = 5f;
    }
}
