using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevKacper.ObjectPooler;

public class Planet : MonoBehaviour
{
    public static List<Planet> planets = new List<Planet>(); 

    private const float GravityMultiplier = 10f;
    private const float MaxMass = 5;
    private const float BaseRadius = 0.075f;
    private const float BaseMass = 0.05f;
    private const int MaximumExplosionSize = 100;

    private const float VelocityMultiplier = 0.975f;
    private const float AttractMultiplier = 0.5f;

    [SerializeField] private Rigidbody2D planetRigidbody;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private SpriteRenderer sprite;

    private bool isMerged;

    private void OnEnable()
    {
        planets.Add(this);

        transform.localScale = new Vector3(BaseRadius / 2f, BaseRadius / 2f, 1f);
        planetRigidbody.mass = BaseMass;

        isMerged = false;
        circleCollider.enabled = false;

        sprite.color = Random.ColorHSV();

        planetRigidbody.AddForce(GetRandomDirection());

        Invoke(nameof(EnableCollider), 0.5f);
    }

    private void OnDisable()
    {
        planets.Remove(this);
        CancelInvoke();
    }

    private void FixedUpdate()
    {
        if(planetRigidbody.velocity != Vector2.zero)
        {
            planetRigidbody.velocity *= VelocityMultiplier;
        }
    }

    private void EnableCollider()
    {
        circleCollider.enabled = true;
        InvokeRepeating(nameof(AttractPlanets), 5f, 0.05f);
        InvokeRepeating(nameof(CheckPosition), 0f, 1f);
    }

    private void CheckPosition()
    {
        if(!PlanetSpawner.IsPlanetInBounds(transform.position))
        {
            DestroyPlanet();
        }
    }

    private void AttractPlanets()
    {
        if(planetRigidbody.mass != BaseMass)
        {
            float attractRange = Mathf.Sqrt(planetRigidbody.mass) * AttractMultiplier;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attractRange);
            foreach (Collider2D collider in colliders)
            {
                var planet = collider.GetComponent<Planet>();
                if (planet != null)
                {
                    AttractPlanet(planet);
                }
            }
        }
    }

    private void AttractPlanet(Planet ball)
    {
        Vector3 direction = planetRigidbody.position - ball.planetRigidbody.position;
        float distance = direction.magnitude;

        if (distance == 0)
            return;

        float forceMagnitude = GravityMultiplier * (planetRigidbody.mass * ball.planetRigidbody.mass) / Mathf.Pow(distance, 2);
        Vector3 force = direction.normalized * forceMagnitude;

        ball.planetRigidbody.AddForce(force);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!isMerged)
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
        if(collision.GetComponent<Planet>() == null)
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

        if(planetRigidbody.mass >= MaxMass)
        {
            for(int i = 0; i < (int)planetRigidbody.mass / BaseMass; ++i)
            {
                if(i > MaximumExplosionSize)
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
        gameObject.SetActive(false);
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
