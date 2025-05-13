using UnityEngine;

public class TransformSwayer : MonoBehaviour
{
    [Header("Swaying rotation")]
    public Vector3 SwayAmount;
    public Vector3 SwaySpeed;

    [Space]
    [Header("Shifting position")]
    public Vector3 ShiftAmount;
    public Vector3 ShiftSpeed;

    private Vector3 _lastSwayRotation;
    private Vector3 _lastSwatPosition;

    void Update()
    {
        SwayRotation();
        SwayPosition();
    }

    private void SwayPosition()
    {
        var initialPosition = transform.localPosition - _lastSwatPosition;
        _lastSwatPosition = GetChangedSin(ShiftSpeed, ShiftAmount);
        var swayRotation = initialPosition + _lastSwatPosition;
        transform.localPosition = swayRotation;
    }

    private void SwayRotation()
    {
        var initialRotation = transform.localEulerAngles - _lastSwayRotation;
        _lastSwayRotation = GetChangedSin(SwaySpeed, SwayAmount);
        var swayRotation = initialRotation + _lastSwayRotation;
        transform.localRotation = Quaternion.Euler(swayRotation);
    }

    private Vector3 GetChangedSin(Vector3 speed, Vector3 amount)
    {
        // Расчёт раскачивания по синусоиде
        return new Vector3 {
            x = Mathf.Sin(Time.time * speed.x) * amount.x,
            y = Mathf.Sin(Time.time * speed.y) * amount.y,
            z = Mathf.Sin(Time.time * speed.z) * amount.z
        };
    }
}