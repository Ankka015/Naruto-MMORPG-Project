using UnityEngine;

public sealed class KonohaNpcPatrol : MonoBehaviour
{
    Vector3[] route;
    int next;
    public float speed = 2.2f;
    public void SetRoute(Vector3[] points, int start) { route = points; next = (start + 1) % points.Length; }
    void Update()
    {
        if (route == null || route.Length == 0) return;
        Vector3 target = route[next]; target.y = transform.position.y;
        Vector3 delta = target - transform.position;
        if (delta.sqrMagnitude < 1.5f) next = (next + 1) % route.Length;
        else { transform.position += delta.normalized * speed * Time.deltaTime; transform.rotation = Quaternion.LookRotation(delta); }
    }
}
