using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevKacper.ObjectPooler;

public class Planet : MonoBehaviour
{
    private const float GravityMultiplier = 10f;
    private const float MaxMass = 50;
    private const float BaseRadius = 0.1f;
    private const float BaseMass = 0.05f;

    [SerializeField] private Rigidbody2D planetRigidbody;
    [SerializeField] private CircleCollider2D circleCollider;

    private bool isMerged;

    private void OnEnable()
    {
        isMerged = false;
        transform.localScale = new Vector3(BaseRadius / 2f, BaseRadius / 2f, 1f);
        planetRigidbody.mass = BaseMass;
        circleCollider.enabled = false;

        planetRigidbody.AddForce(GetRandomDirection());

        Invoke(nameof(EnableCollider), 0.75f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void FixedUpdate()
    {
        if(planetRigidbody.velocity != Vector2.zero)
        {
            planetRigidbody.velocity *= 0.99f;
        }
    }

    private void EnableCollider()
    {
        circleCollider.enabled = true;
        InvokeRepeating(nameof(AttractPlanets), 0f, 0.1f);
        InvokeRepeating(nameof(CheckPosition), 0f, 1f);
    }

    private void CheckPosition()
    {
        if(!PlanetSpawner.IsPlanetInBounds(transform.position))
        {
            gameObject.SetActive(false);
        }
    }

    private void AttractPlanets()
    {
        float attractRange = Mathf.Sqrt(planetRigidbody.mass);
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
            if (planet != null)
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


        planet.gameObject.SetActive(false);

        if(planetRigidbody.mass >= MaxMass)
        {
            for(int i = 0; i < (int)planetRigidbody.mass; ++i)
            {
                var newPlanet = TagObjectPooler.Spawn("Planet", (Vector2)transform.position + GetRandomDirection());
                newPlanet.GetComponent<Planet>().planetRigidbody.AddForce(GetRandomDirection() * 3f, ForceMode2D.Impulse);
            }
            gameObject.SetActive(false);
        }
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
