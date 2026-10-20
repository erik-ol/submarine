using UnityEngine;
using System.Collections.Generic;

public class BoidCluster : MonoBehaviour
{
    List<Boid> boids;
    public int amount = 200; // amount of boids
    public float radius = 20f; // radius they spawn in and stay in - make them follow submarine position?
    public float viewDistance = 5f; // radius in which they see and follow other boids
    float viewDistanceSquared;
    public float avoidDistance = 0.5f; // how close they get before avoid each other
    float avoidDistanceSquared;
    public float speed = 1f; // speed factor of the boids
    public float acceleration = 1f;
    public float speedDecay = 1f; // friction coefficient
    public float maxSpeed = 4f; // max speed of boids
    public float clusterStrength = 1f;
    public float alignmentStrength = 2f;
    public float submarineAvoidDistance = 7f;
    float submarineAvoidDistanceSquared;

    public Boid boidPrefab;

    public ArduinoConnector submarine;

    // spawn in boids randomly in sphere
    void Start() {

        viewDistanceSquared = viewDistance*viewDistance;
        avoidDistanceSquared = avoidDistance*avoidDistance;
        submarineAvoidDistanceSquared = submarineAvoidDistance*submarineAvoidDistance;

        boids = new List<Boid>();

        for (int i = 0; i<amount; i++) {
            Boid b = Instantiate(boidPrefab, transform.position+Random.onUnitSphere*Random.Range(0.0f, radius), Quaternion.identity);
            b.speed = speed;
            b.velocity = Random.onUnitSphere*maxSpeed/2f;
            boids.Add(b);
        }
    }

    // update velocity of all boids
    void Update()
    {

        foreach (Boid b in boids){

            // add: avoid the submarine

            // the direction different impulses want to go, and the strength each impulse has
            Vector3 clusterDir = Vector3.zero;
            Vector3 alignmentDir = Vector3.zero;
            Vector3 avoidDir = Vector3.zero;
            Vector3 submarineDir = Vector3.zero;
            float avoidStrength = 0;

            foreach (Boid boidOther in boids){
                if (boidOther != b)
                {
                    // go toward middle of local cluster
                    Vector3 positionDelta = boidOther.transform.position - b.transform.position;

                    if (positionDelta.sqrMagnitude<viewDistanceSquared) {
                        clusterDir += positionDelta;

                        // align velocity with nearby boids
                        alignmentDir += boidOther.velocity;

                        // avoid boids that are very close
                        if (positionDelta.sqrMagnitude < avoidDistanceSquared) {
                            avoidDir -= positionDelta / (positionDelta.sqrMagnitude + 0.1f);
                            avoidStrength = System.Math.Max(avoidStrength, 5-positionDelta.sqrMagnitude/avoidDistanceSquared);
                        }
                    }
                }
            }

            float submarineStrength = 0f;
            Vector3 submarineDelta = submarine.transform.position - b.transform.position;
            if (submarineDelta.sqrMagnitude < submarineAvoidDistanceSquared)
            {
                submarineStrength = 10f;
                submarineDir = - submarineDelta;
            }

            // don't go too far from cluster spawn
            Vector3 homeDir = transform.position - b.transform.position;
            float homeStrength = System.Math.Max(0.1f, (homeDir.sqrMagnitude / radius) - radius);

            float totalStrength = clusterStrength + alignmentStrength + avoidStrength + homeStrength + submarineStrength;

            // velocity is updated mid loop, so boids further in the loop will check this instead of the old velocity, should that be changed?
            // each impulse affects velocity proportionally to its strength
            b.velocity += acceleration * Time.deltaTime * ((clusterDir.normalized * clusterStrength + alignmentDir.normalized * alignmentStrength + avoidDir.normalized * avoidStrength + homeDir.normalized * homeStrength + submarineDir.normalized * submarineStrength) / totalStrength);
            // speed decays from friciton?
            b.velocity *= speedDecay;
            // clamp on max speed
            if (b.velocity.sqrMagnitude > maxSpeed*maxSpeed)
            {
                b.velocity = b.velocity.normalized * maxSpeed;
            }
        }
    }
}