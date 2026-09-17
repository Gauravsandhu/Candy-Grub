using UnityEngine;

public class CannonMovement : MonoBehaviour
{
    public Transform barrelTransform;

    public float maxRightAngle = -90f;
    public float maxLeftAngle = 90f;
    public float desiredPeriod = 4f; // seconds for one full swing, right-left-right

    private float baseAngle;
    private float center;
    private float amplitude;
    private float speedFactor;
    private float elapsedTime;

    void Start()
    {
        baseAngle = barrelTransform.localEulerAngles.z;
        if (baseAngle > 180f)
            baseAngle -= 360f;

        center = (maxLeftAngle + maxRightAngle) / 2f;
        amplitude = (maxLeftAngle - maxRightAngle) / 2f;
        speedFactor = 2f * Mathf.PI / desiredPeriod;

        elapsedTime = 0f;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        float offsetAngle = center + amplitude * Mathf.Sin(elapsedTime * speedFactor);

        barrelTransform.localRotation = Quaternion.Euler(0f, 0f, baseAngle + offsetAngle);
    }
}