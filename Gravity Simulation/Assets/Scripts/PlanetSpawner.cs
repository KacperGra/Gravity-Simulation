using DevKacper.ObjectPooler;
using DevKacper.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpawner : MonoBehaviour
{
    public const string PlanetTag = "Planet";
    public static int planetsToSpawn = 0;

    [Header("References")]
    [SerializeField] private PlanetSystem _planetSystem;
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("Settings")]
    [SerializeField] private int _maximumPlanetsPerFrame = 0;
    [SerializeField] private int _maximumPlanetNumber = 2000;

    private readonly Collider2D[] colliderBuffer = new Collider2D[1000];
    private bool _isSpawning;

    public static PlanetSpawner Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
    }

    private void Start()
    {
        TagObjectPooler.Instance.Initialize();

        StartSpawner();
    }

    private void StartSpawner()
    {
        if (!_isSpawning)
        {
            _isSpawning = true;
            StartCoroutine(SpawnPlanets());
        }
    }

    private void StopSpawner()
    {
        if (_isSpawning)
        {
            _isSpawning = false;
            StopCoroutine(SpawnPlanets());
        }
    }

    private IEnumerator SpawnPlanets()
    {
        while (true)
        {
            for (int i = 0; i < _maximumPlanetsPerFrame; ++i)
            {
                SpawnPlanet();
            }

            yield return null;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var planet = SpawnPlanet();
            planet.transform.position = UtilityClass.GetMousePosition();
        }
        else if (Input.GetMouseButton(1))
        {
            var planet = SpawnPlanet();
            planet.transform.position = UtilityClass.GetMousePosition();
        }

        if (Planet.Planets.Count > _maximumPlanetNumber)
        {
            StopSpawner();
        }
        else
        {
            StartSpawner();
        }

        Debug.Log($"Number of planets {Planet.Planets.Count}");
    }

    public Collider2D[] GetColliders(Vector3 position, float range, out int amount)
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        amount = Physics2D.OverlapCircle(position, range, contactFilter, colliderBuffer);
        return colliderBuffer;
    }

    private float GetBiggestPlanetSize()
    {
        float biggestPlanetSize = 0f;
        foreach (Planet planet in Planet.Planets)
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
        var planet = TagObjectPooler.Spawn(PlanetTag, UtilityClass.RandomPointInBounds(spawnArea.bounds));
        return planet;
    }

    public static bool IsPlanetInBounds(Vector3 position)
    {
        return UtilityClass.IsPointInBounds(position, Instance.spawnArea.bounds);
    }
}