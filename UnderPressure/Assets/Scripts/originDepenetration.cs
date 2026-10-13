using UnityEngine;

public class OriginDepenetration : MonoBehaviour
{
    public Collider bodyCollider;        
    public LayerMask obstacleMask = ~0;    // walls, props, etc.
    public bool horizontalOnly = true;
    public int iterations = 4;

    readonly Collider[] hits = new Collider[16];

    void LateUpdate()   // make sure this runs after locomotion
    {
        for (int i = 0; i < iterations; i++)
        {
            var b = bodyCollider.bounds;
            int n = Physics.OverlapBoxNonAlloc(b.center, b.extents, hits,
                        Quaternion.identity, obstacleMask, QueryTriggerInteraction.Ignore);
            bool moved = false;

            for (int j = 0; j < n; j++)
            {
                var other = hits[j];
                if (other == bodyCollider || other.transform.IsChildOf(transform)) continue;

                if (Physics.ComputePenetration(
                        bodyCollider, bodyCollider.transform.position, bodyCollider.transform.rotation,
                        other, other.transform.position, other.transform.rotation,
                        out Vector3 dir, out float dist))
                {
                    if (horizontalOnly) { dir.y = 0; dir.Normalize(); }
                    transform.position += dir * dist;
                    moved = true;
                }
            }
            if (!moved) break;
        }
    }
}
