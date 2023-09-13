using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevKacper.ObjectPooler;

public class Planet : MonoBehaviour
{
    public static List<Planet> Planets = new List<Planet>();

    [Header("References")]
    [SerializeField] private Rigidbody2D planetRigidbody;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private SpriteRenderer sprite;

    [Header("Settings")]
    [SerializeField] private float _baseRadius = 0.075f;
    [SerializeField] private float _baseMass = 0.05f;
    [SerializeField] private float _gravityMultiplier = 10f;
    [SerializeField] private float _maxMass = 5;
    [SerializeField] private int _maximumExplosionSize = 100;
    [SerializeField] private float _velocityMultiplier = 0.975f;
    [SerializeField] private float _attractMultiplier = 0.5f;

    private bool isMerged;

    private void OnEnable()
    {
        Planets.Add(this);

        transform.localScale = new Vector3(_baseRadius / 2f, _baseRadius / 2f, 1f);
        planetRigidbody.mass = _baseMass;

        isMerged = false;
        circleCollider.enabled = false;

        sprite.color = Random.ColorHSV();
        planetRigidbody.AddForce(GetRandomDirection());

        Invoke(nameof(EnableCollider), 0.5f);
    }

    private void OnDisable()
    {
        Planets.Remove(this);
        CancelInvoke();
    }

    public void UpdateVelocity()
    {
        if (planetRigidbody.velocity != Vector2.zero)
        {
            planetRigidbody.velocity *= _velocityMultiplier;
        }
    }

    public void AttractPlanets()
    {
        if (planetRigidbody.mass != _baseMass)
        {
            float attractRange = Mathf.Sqrt(planetRigidbody.mass) * _attractMultiplier;

            var colliders = PlanetSpawner.Instance.GetColliders(transform.position, attractRange, out int amount);
            for (int i = 0; i < amount; ++i)
            {
                var collider = colliders[i];
                if (collider.TryGetComponent<Planet>(out var planet))
                {
                    AttractPlanet(planet);
                }
            }
        }
    }

    private void EnableCollider()
    {
        circleCollider.enabled = true;
        InvokeRepeating(nameof(CheckPosition), 0f, 1f);
    }

    private void CheckPosition()
    {
        if (!PlanetSpawner.IsPlanetInBounds(transform.position))
        {
            //DestroyPlanet();
        }
    }

    private void AttractPlanet(Planet ball)
    {
        Vector3 direction = planetRigidbody.position - ball.planetRigidbody.position;
        float distance = direction.magnitude;

        if (distance == 0)
            return;

        float forceMagnitude = _gravityMultiplier * (planetRigidbody.mass * ball.planetRigidbody.mass) / Mathf.Pow(distance, 2);
        Vector3 force = direction.normalized * forceMagnitude;

        ball.planetRigidbody.AddForce(force);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isMerged)
        {
            var planet = collision.collider.GetComponent<Planet>();
            if (planet != null && !planet.isMerged)
            {
                if (planet.planetRigidbody.mass > planetRigidbody.mass)
                {
                    return;
                }

                planet.isMerged = true;
                MergePlanets(planet);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Planet>() == null)
        {
            CancelInvoke(nameof(EnableCollider));
            EnableCollider();
        }
    }

    private void MergePlanets(Planet planet)
    {
        planetRigidbody.mass += planet.planetRigidbody.mass;

        float newR = Mathf.Sqrt((planet.GetField() + GetField()) / Mathf.PI);
        transform.localScale = new Vector3(newR * 2, newR * 2, 1f);

        planet.DestroyPlanet();

        if (planetRigidbody.mass >= _maxMass)
        {
            for (int i = 0; i < (int)planetRigidbody.mass / _baseMass; ++i)
            {
                if (i > _maximumExplosionSize)
                {
                    break;
                }

                var newPlanet = TagObjectPooler.Spawn(PlanetSpawner.PlanetTag, (Vector2)transform.position + GetRandomDirection());
                newPlanet.GetComponent<Planet>().planetRigidbody.AddForce(GetRandomDirection() * 0.25f, ForceMode2D.Impulse);
            }
            DestroyPlanet();
        }
    }

    private void DestroyPlanet()
    {
        TagObjectPooler.DestroyObject("Planet", gameObject);
    }

    private Vector2 GetRandomDirection()
    {
        return UnityEngine.Random.insideUnitCircle.normalized;
    }

    private float GetField()
    {
        return Mathf.Pow(GetRadius(), 2f) * Mathf.PI;
    }

    private float GetRadius()
    {
        return transform.localScale.x / 2f;
    }
}