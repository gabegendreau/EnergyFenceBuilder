using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;


// If you are adding a custom fence style it must also be added to PrefabStore.cs
// There are further instructions in the comments in that file
//
// To add a custom fence style to this file:
//      - Add the name of the style to the enum variable just below (marked with **)
//      - Add a case for your style to switch statement in OnGUI() below, there is
//        a template for you code commented out in the place where it should go
//      - The name given to the style in the enum variable needs to match the case
//        in the switch statement
//      - The prefab assigned to thisFence needs to match the variable name listed
//        in PrefabStore.cs
//
// Full instructions on creating your own prefab are distributed with this tool

// This is the main editor window code for the tool
namespace EnergyFenceTool
{
    public class EnergyFenceBuilder : EditorWindow
    {
        GameObject thisFence;
        GameObject passingFence;
        // ** Add custom fence style name to list below (before 'Custom') **
        public enum FenceStyles {GreenEnergy, TechFence, YellowPurpleSparks, Custom}
        FenceStyles fenceStyle;
        int numPylons = 4;
        public PrefabStore prefabData;
        bool startLoopClosed = true;
        int minPylons = 2;
        // You can increase the max number of pylons if you really need to
        int maxPylons = 40;
        GameObject customPrefab;
        Vector3 fenceSpawnLocation = Vector3.zero;

        [MenuItem("Tools/Energy Fence Builder")]
        public static void AddNewFence()
        {
            GetWindow<EnergyFenceBuilder>("Energy Fence Builder");
        }

        public void OnGUI()
        {
            if (!EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 212;
            }
            EditorGUILayout.Space(20.0f);
            // Slider for number of pylons sets variable
            numPylons = EditorGUILayout.IntSlider("Number of pylons", numPylons, minPylons, maxPylons);
            EditorGUILayout.Space(5.0f);
            // Sets the location at which the fence's center will be
            fenceSpawnLocation = EditorGUILayout.Vector3Field("Fence center location: ", fenceSpawnLocation);
            EditorGUILayout.Space(5.0f);
            // Get location of GameObject in scene
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Capture position of scene object", GUILayout.Width(200)))
            {
                fenceSpawnLocation = Selection.activeTransform.position;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10.0f);
            // Popup menu of styles listed in enum sets variable
            fenceStyle = (FenceStyles)EditorGUILayout.EnumPopup("Fence style:", fenceStyle);
            // If the style is set to custom it will show a field to select your prefab
            if (fenceStyle == FenceStyles.Custom)
            {
                customPrefab = EditorGUILayout.ObjectField("Custom Prefab", customPrefab, typeof(GameObject), false) as GameObject;
            }
            EditorGUILayout.Space(30.0f);
            // Button places pylons based on information above
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Fence", GUILayout.Width(80), GUILayout.Height(34)))
            {
                // Sets proper fence prefab based on style selected
                switch(fenceStyle) 
                {
                    case FenceStyles.GreenEnergy:
                    {
                        thisFence = prefabData.greenEnergyPrefab;
                        break;
                    }
                    case FenceStyles.YellowPurpleSparks:
                    {
                        thisFence = prefabData.yellowPurplePrefab;
                        break;
                    }
                    case FenceStyles.TechFence:
                    {
                        thisFence = prefabData.techFencePrefab;
                        break;
                    }
                    // Follow the below code sample to add your custom fence style
                    /* 
                    case FenceStyles.YourFenceStyle;  ** Custom fence style template
                    {
                        thisFence = prefabData.yourPrefabName;
                        break;
                    }                                 ** End custom fence style template
                    */
                    case FenceStyles.Custom:
                    {
                        thisFence = customPrefab;
                        break;
                    }
                }
                // Create new fence of chosen style
                GameObject newFence = Instantiate(thisFence, fenceSpawnLocation, Quaternion.identity) as GameObject;
                // Create int to count remaining pylons to place
                int pylonsToAdd = numPylons;
                // Count down and add pylons
                while (pylonsToAdd > 0)
                {
                    newFence.GetComponent<EnergyFence>().AddNewPylon();
                    pylonsToAdd--;
                }
                // Set default close loop option to false if only 2 pylons
                if (numPylons == 2)
                {
                    startLoopClosed = false;
                }
                // Get all transforms in fence
                Transform[] allTransforms = newFence.GetComponentsInChildren<Transform>();
                // Convert to list so it can be adjusted
                List<Transform> transformList = new(allTransforms);
                // Create a list of transforms not needed (parent object, models in prefabs)
                List<Transform> transformsToRemove = new List<Transform>();
                int k = 0;
                foreach(Transform transform in transformList)
                {
                    if (!transform.gameObject.GetComponent<EnergyPylon>())
                    {
                    transformsToRemove.Add(transformList[k]);
                    }
                    k++;
                }
                // Create an array of only the pylon transforms and distribute them evenly around center of fence parent object
                Transform[] pylonTransforms = transformList.Except(transformsToRemove).ToArray();
                int radius = 10;
                int i = 1;
                foreach(Transform pylonTransform in pylonTransforms)
                {
                    float radsToRotate = Mathf.PI * 2.0f / numPylons * i;
                    float posX = Mathf.Sin(radsToRotate);
                    float posZ = Mathf.Cos(radsToRotate);
                    Vector3 moveDirection = new(posX, 0, posZ);
                    pylonTransform.localPosition = moveDirection * radius;
                    i++;
                }
                // Take loop closed variable and set for fence
                if (!startLoopClosed)
                {
                    newFence.GetComponent<EnergyFence>().DisableCloseLoop();
                }
                // Select each pylon so any particles are activated in viewport
                int j = 0;
                GameObject[] objectsToSelect = new GameObject[pylonTransforms.Length];
                foreach(Transform transform in pylonTransforms)
                {
                    objectsToSelect[j] = transform.gameObject;
                    j++;
                }
                Selection.objects = objectsToSelect;
                // Set variable with larger scope to new fence so it can be used outside this function
                passingFence = newFence;
                // After the inspector updates call function to select fence and switch to inspector for it
                EditorApplication.delayCall += SelectFence();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public EditorApplication.CallbackFunction SelectFence()
        {
            EditorApplication.CallbackFunction callback = null;
            callback = new EditorApplication.CallbackFunction(() =>
            {
                Selection.objects = new GameObject[] {passingFence};
                // If floating window close it, docked windows stay docked in background
                if (!docked)
                {
                    Close();
                }
                // Focus inspector for fence so pylons can be placed
                EditorApplication.ExecuteMenuItem("Window/General/Inspector");
            }
            );
            return callback;
        }
    }
}