using UnityEngine;

public sealed class KonohaTreeSway : MonoBehaviour
{
    Vector3 baseRotation;
    float phase;

    void Start()
    {
        baseRotation = transform.localEulerAngles;
        phase = Mathf.Sin(transform.position.x * 0.17f + transform.position.z * 0.11f) * 3f;
    }

    void Update()
    {
        float breeze = Time.time * 0.58f + phase;
        transform.localRotation = Quaternion.Euler(
            baseRotation.x + Mathf.Sin(breeze * 0.8f) * 0.45f,
            baseRotation.y,
            baseRotation.z + Mathf.Sin(breeze) * 1.15f);
    }
}
