using System.Collections.Generic;
using UnityEngine;

// This script is attached to the parent fence GameObject for each fence
// and is what the user interacts with in the inspector when working on
// fence layout and generating the fence
//
// If creating a custom fence style the matching pylon prefab must be
// assigned in the inspector window of this class
namespace EnergyFenceTool
{
    [ExecuteAlways]
    public class EnergyFence : MonoBehaviour
    {
        [Header("Corner Points")]
        // The checkbox for addPylon acts as a button to add more pylons to
        // a fence after it has been built with the tool's editor window
        [Tooltip("Add a fence post/point to line")]
        public bool addPylon = false;
        // Selecting these points in the inspector will highlight the
        // corresponding pylon, this helps place them in the scene
        [Tooltip("Points for fence posts - Use button above to add")]
        [SerializeField]
        private List<EnergyPylon> points = new List<EnergyPylon>(0);
        // If creating a custom fence style the prefab for the pylon must be
        // set here. The pylon prefab must have EnergyPylon.cs attached
        [Tooltip("Set prefab GameObject for fence posts")]
        [SerializeField]
        private GameObject energyPylonPrefab;

        [Space]

        [Header("Line Stuff")]
        // If the material is set here it will be set in the line renderer when
        // 'generate fence' is clicked. If changed afterwards it must be manually
        // changed in the field below as well
        [Tooltip("Material for energy beam/fence wire")]
        [SerializeField]
        private Material beamMaterial;
        // If this is not checked the fence will not be an enclosed loop, if
        // there are less than three pylons this will default to false
        [Tooltip("Connect last fence post/point to first")]
        [SerializeField]
        private bool closeLoop = true;
        // This checkbox will trigger a call to 'generate fence' from Update
        [Tooltip("Generate a new fence based on these parameters")]
        public bool generateFence = false;
        LineRenderer energyBeam;

        [Space]

        // Different fence styles and applications may require a different width of wall
        [Header("Collision Boxes")]
        [SerializeField]
        private float widthMultiplier;

        void Awake()
        {
            energyBeam = gameObject.GetComponent<LineRenderer>();
        }

        void Update()
        {
            // Manually add a new pylon after the fence has been added to the scene
            if (addPylon)
            {
                AddNewPylon();
                addPylon = false;
            }
            // Generate a new fence based on the above settings. Using this button
            // again will destroy existing lines and collision boxes before
            // creating new ones - user does not need to start over after making changes
            if (generateFence)
            {
                GenerateFence();
                generateFence = false;
            }
        }

        // Creates a new pylon and adds it to the list of points used to draw the line
        public void AddNewPylon()
        {
            GameObject newPylon = Instantiate(energyPylonPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
            newPylon.transform.parent = gameObject.transform;
            points.Add(newPylon.GetComponent<EnergyPylon>());
        }

        void GenerateFence()
        {
            // Clear any existing colliders
            BoxCollider[] preExistingBoxColliders = GetComponentsInChildren<BoxCollider>();
            foreach(BoxCollider boxCollider in preExistingBoxColliders)
            {
                DestroyImmediate(boxCollider.gameObject);
            }
            // Transfers pylon data into points for line renderer and builds box collisers between them
            int pylonCounter = 0;
            energyBeam.positionCount = points.Count;
            foreach(EnergyPylon point in points)
            {
                energyBeam.SetPosition(pylonCounter, points[pylonCounter].transform.position);
                pylonCounter++;
                if (pylonCounter < points.Count)
                {
                    BuildCollider(point.transform.position, points[pylonCounter].transform.position);
                } else if (pylonCounter >= points.Count && closeLoop)
                {
                    BuildCollider(point.transform.position, points[0].transform.position);
                }
            }
            energyBeam.loop = closeLoop;
            energyBeam.material = beamMaterial;
        }

        // Called from GenerateFence(), builds colliders along line/fence wall
        void BuildCollider(Vector3 startPoint, Vector3 endPoint)
        {
            GameObject wallObject = new GameObject();
            wallObject.name = "wall";
            wallObject.transform.parent = gameObject.transform;
            wallObject.transform.position = (startPoint + endPoint) * 0.5f;
            BoxCollider newBoxCollider = wallObject.AddComponent<BoxCollider>();
            float bcLength = Vector3.Distance(startPoint, endPoint);
            float bcWidth = (energyBeam.startWidth + energyBeam.endWidth) * 0.5f * widthMultiplier;
            float bcHeight = 4.0f;
            newBoxCollider.size = new Vector3(bcWidth, bcHeight, bcLength);
            float deltaX = endPoint.x - startPoint.x;
            float deltaZ = endPoint.z - startPoint.z;
            float newRotationDegrees = Mathf.Atan(deltaX / deltaZ);
            newRotationDegrees *= Mathf.Rad2Deg;
            if (newRotationDegrees > 180.0f)
            {
                newRotationDegrees -= 360.0f;
            } 
            wallObject.transform.rotation = Quaternion.Euler(0.0f, newRotationDegrees, 0.0f);
        }

        // Used to start new fence with close loop option defaulted to false, used for 2 pylon fences
        public void DisableCloseLoop()
        {
            closeLoop = false;
        }
    }
}