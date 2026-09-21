using UnityEngine;
using System.Collections.Generic;

public class BoidCluster : MonoBehaviour
{
    List<Boid> boids;
    public int amount = 10; // amount of boids
    public float radius = 2f; // radius they spawn in
    public float avoidDistance = 0.5f; // how close they get before avoid each other
    public float speed = 0.25f; // speed of the boids

    public Boid boidPrefab;

    void Start() {
        boids = new List<Boid>();

        for (int i = 0; i<amount; i++) {
            Boid b = Instantiate(boidPrefab, transform.position+Random.onUnitSphere*radius, Quaternion.identity);
            b.speed = speed;
            boids.Add(b);
        }
    }

    void Update()
    {

        foreach (Boid b in boids){

            Vector3 clusterDir = Vector3.zero;
            Vector3 alignmentDir = Vector3.zero;
            Vector3 avoidDir = Vector3.zero;

            foreach (Boid boidOther in boids){
                if (boidOther != b)
                {
                    // go toward middle of local cluster
                    Vector3 positionDelta = boidOther.transform.position - b.transform.position;
                    clusterDir += positionDelta / (positionDelta.sqrMagnitude + 1);

                    // align velocity with nearby boids
                    Vector3 velocityDelta = boidOther.velocity - b.velocity;
                    alignmentDir += velocityDelta / (positionDelta.sqrMagnitude + 1);

                    // avoid boids that are very close
                    if (avoidDistance > positionDelta.sqrMagnitude) {
                        avoidDir -= positionDelta / (positionDelta.sqrMagnitude + 1);
                    }
                }
            }

            b.velocity += (clusterDir.normalized + alignmentDir.normalized + avoidDir.normalized) / 3;

        }
    }
}