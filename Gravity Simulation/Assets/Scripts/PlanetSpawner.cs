using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevKacper.ObjectPooler;
using DevKacper.Utility;
using System;

public class PlanetSpawner : MonoBehaviour
{
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
        InvokeRepeating(nameof(SpawnPlanet), 0f, 0.1f);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var planet = SpawnPlanet();
            planet.transform.position = UtilityClass.GetMousePosition();
        }
    }

    private GameObject SpawnPlanet()
    {
        return TagObjectPooler.Spawn("Planet", UtilityClass.RandomPointInBounds(spawnArea.bounds));
    }

    public static bool IsPlanetInBounds(Vector3 position)
    {
        return UtilityClass.IsPointInBounds(position, Instance.spawnArea.bounds);
    }
}
