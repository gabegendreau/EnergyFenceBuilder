using UnityEngine;

// This file contains references to the prefabs listed in the tool editor window
// 
// If you are wanting to add a custom fence style to the editor window you need
// to create a GameObject variable here and assign it in the inspector window for
// the PrefabStore scriptable object located at /EnergyFenceBuilder/Resources
//
// Your custom fence style must also be added in 2 places in EnergyFenceBuilder.cs
// See comments at top of that file for further information
//
// Full instructions on creating your own prefab are distributed with this tool
namespace EnergyFenceTool
{
    [CreateAssetMenu]
    public class PrefabStore : ScriptableObject
    {
        public GameObject greenEnergyPrefab;
        public GameObject yellowPurplePrefab;
        public GameObject techFencePrefab;
    }
}