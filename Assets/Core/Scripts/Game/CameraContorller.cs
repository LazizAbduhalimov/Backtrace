using UnityEngine;

public class SmoothCameraMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float smoothTime = 0.1f;

    Vector3 targetPosition;
    Vector3 velocity = Vector3.zero;

    // private Vector2 limitX;
    // private Vector2 limitY;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 0f)
        {
            input = input.normalized;

            // Движение относительно поворота камеры
            var moveDirection = transform.right * input.x + transform.up * input.z;
            targetPosition += moveDirection * moveSpeed * Time.deltaTime;
            
            // targetPosition.x = Mathf.Clamp(targetPosition.x, limitX.x, limitX.y);
            // targetPosition.y = Mathf.Clamp(targetPosition.y, limitY.x, limitY.y);
        }

        // Плавно тянем камеру к целевой позиции
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}