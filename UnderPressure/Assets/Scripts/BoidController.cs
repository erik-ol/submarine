using UnityEngine;
using System.Collections.Generic;

public class BoidCluster : MonoBehaviour
{
    List<Boid> boids;
    public int amount = 10; // amount of boids
    public float radius = 2f; // radius they spawn in
    public float avoidDistance = 0.5f; // how close they get before avoid each other
    float avoidDistanceSquared;
    public float speed = 0.25f; // speed of the boids
    public float acceleration = 0.5f;
    public float speedDecay = 0.9f;

    public Boid boidPrefab;

    // spawn in boids randomly in sphere
    void Start() {

        avoidDistanceSquared = avoidDistance*avoidDistance;

        boids = new List<Boid>();

        for (int i = 0; i<amount; i++) {
            Boid b = Instantiate(boidPrefab, transform.position+Random.onUnitSphere*Random.Range(0.0f, radius), Quaternion.identity);
            b.speed = speed;
            b.velocity = Random.onUnitSphere*5;
            boids.Add(b);
        }
    }

    // update velocity of all boids
    void Update()
    {

        foreach (Boid b in boids){

            // the direction different impulses want to go, and the strength each impulse has
            Vector3 clusterDir = Vector3.zero;
            float clusterStrength = 0.75f;
            Vector3 alignmentDir = Vector3.zero;
            float alignmentStrength = 1;
            Vector3 avoidDir = Vector3.zero;
            float avoidStrength = 0;

            foreach (Boid boidOther in boids){
                if (boidOther != b)
                {
                    // go toward middle of local cluster
                    Vector3 positionDelta = boidOther.transform.position - b.transform.position;
                    clusterDir += positionDelta.normalized / ((positionDelta.sqrMagnitude * positionDelta.sqrMagnitude) + 0.1f);

                    // align velocity with nearby boids
                    alignmentDir += boidOther.velocity / ((positionDelta.sqrMagnitude * positionDelta.sqrMagnitude) + 0.1f);

                    // avoid boids that are very close
                    if (avoidDistanceSquared > positionDelta.sqrMagnitude) {
                        avoidDir -= positionDelta.normalized / (positionDelta.sqrMagnitude + 0.1f);
                        avoidStrength += 1-(positionDelta.sqrMagnitude/avoidDistanceSquared);
                    }
                }
            }

            // don't go too far from cluster spawn
            Vector3 homeDir = transform.position - b.transform.position;
            float homeStrength = System.Math.Max(0.1f, homeDir.sqrMagnitude - (radius*radius));

            float totalStrength = clusterStrength + alignmentStrength + avoidStrength + homeStrength;

            // velocity is updated mid loop, so boids further in the loop will check this instead of the old velocity, should that be changed?
            // each impulse affects velocity proportionally to its strength
            b.velocity += acceleration * ((clusterDir.normalized * clusterStrength + alignmentDir.normalized * alignmentStrength + avoidDir.normalized * avoidStrength + homeDir.normalized * homeStrength) / totalStrength);
            b.velocity *= speedDecay;

        }
    }
}