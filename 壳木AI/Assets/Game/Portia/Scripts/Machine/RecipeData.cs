using System;
using UnityEngine;

namespace Game.Portia
{
    public enum MachineType { Sawmill, Furnace, Assembly }

    [Serializable]
    public struct RecipeInput
    {
        public int gid;
        public int count;
    }

    [CreateAssetMenu(fileName = "Recipe_", menuName = "Game/Recipe")]
    public class RecipeData : ScriptableObject
    {
        public MachineType   machineType;
        public string        recipeName;
        public RecipeInput[] inputs;
        public int           outputGid;
        public int           outputCount = 1;
        public float         processTime = 5f;
    }
}
