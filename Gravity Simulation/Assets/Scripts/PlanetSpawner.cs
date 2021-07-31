using DevKacper.ObjectPooler;
using DevKacper.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpawner : MonoBehaviour
{
    public const string PlanetTag = "Planet";

    private const int MaximumPlanetNumber = 2000;

    private const int MaximumPlanetsPerFrame = 20;
    public static int planetsToSpawn = 0;

    [SerializeField] private BoxCollider2D spawnArea;

    public static PlanetSpawner Instance;
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
    }

    private void Start()
    { 
        StartSpawner();
    }

    private void StartSpawner()
    {
        if (!IsInvoking(nameof(SpawnPlanet)))
        {
            InvokeRepeating(nameof(SpawnPlanet), 0f, 0.025f);
        }
    }

    private void StopSpawner()
    {
        if(IsInvoking(nameof(SpawnPlanet)))
        {
            CancelInvoke(nameof(SpawnPlanet));
        }
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var planet = SpawnPlanet();
            planet.transform.position = UtilityClass.GetMousePosition();
        }
        else if(Input.GetMouseButton(1))
        {
            var planet = SpawnPlanet();
            planet.transform.position = UtilityClass.GetMousePosition();
        }


        if(Planet.planets.Count > MaximumPlanetNumber)
        {
            StopSpawner();
        }
        else
        {
            StartSpawner();
        }

        Debug.Log($"Number of planets {Planet.planets.Count}");
        Debug.Log($"Biggest planet size {GetBiggestPlanetSize()}");
    }

    private float GetBiggestPlanetSize()
    {
        float biggestPlanetSize = 0f;
        foreach (Planet planet in Planet.planets)
        {
            if (planet.transform.localScale.x > biggestPlanetSize)
            {
                biggestPlanetSize = planet.transform.localScale.x;
            }
        }
        return biggestPlanetSize;
    }

    private GameObject SpawnPlanet()
    {
        return TagObjectPooler.Spawn(PlanetTag, UtilityClass.RandomPointInBounds(spawnArea.bounds));
    }

    public static bool IsPlanetInBounds(Vector3 position)
    {
        return UtilityClass.IsPointInBounds(position, Instance.spawnArea.bounds);
    }
}
