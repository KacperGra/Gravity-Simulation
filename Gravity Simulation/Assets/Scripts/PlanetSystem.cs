using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSystem : MonoBehaviour
{
    [SerializeField] private int _maxPlanetUpdate;

    private int _currentIndex = 0;

    private void FixedUpdate()
    {
        for (int i = 0; i < _maxPlanetUpdate; ++i)
        {
            if (_currentIndex >= Planet.Planets.Count)
            {
                _currentIndex = 0;
                return;
            }

            var planet = Planet.Planets[_currentIndex];
            planet.UpdateVelocity();
            planet.AttractPlanets();

            ++_currentIndex;
        }
    }
}