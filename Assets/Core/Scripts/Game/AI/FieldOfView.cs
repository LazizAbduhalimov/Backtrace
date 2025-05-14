using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FieldOfView : MonoBehaviour
{

    public float MinAutoDetectRadius = 2f;
    public float ViewRadius = 10f;
    [Range(0, 360)] public float ViewAngle = 90f;
    public int RayCount = 100;

    public LayerMask ObstacleMask;
    public LayerMask TargetMask;
    public List<Transform> VisibleTargets = new();

    private Mesh _mesh;
    private MeshRenderer _meshRenderer;
    
    public Color WarningColor;
    public Color EngageColor;
    private Color _defaultColor;
 
    private void Start() {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        _meshRenderer = GetComponent<MeshRenderer>();
        StartCoroutine(FindTargetsWithDelay(0.1f));
        _defaultColor = _meshRenderer.material.color;
    }

    private void LateUpdate() {
        DrawFieldOfView();
    }

    IEnumerator<WaitForSeconds> FindTargetsWithDelay(float delay) {
        while (true) {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
            AdaptColor();
        }
    }

    private void AdaptColor()
    {
        var color = VisibleTargets.Count > 0 ? EngageColor : _defaultColor;
        _meshRenderer.material.color = color;
    }

    private void FindVisibleTargets() {
        VisibleTargets.Clear();

        var targetsInViewRadius = Physics.OverlapSphere(transform.position, ViewRadius, TargetMask);
        foreach (var target in targetsInViewRadius) {
            var dirToTarget = (target.transform.position - transform.position).normalized;
            var angle = Vector3.Angle(transform.forward, dirToTarget.WithY(0));

            var distance = Vector3.Distance(transform.position, target.transform.position);

            if (angle < ViewAngle / 2f) {
                if (!Physics.Raycast(transform.position, dirToTarget, distance, ObstacleMask)) {
                    VisibleTargets.Add(target.transform);
                }
            }

        }
    }

    private void DrawFieldOfView() {
        var stepAngle = ViewAngle / RayCount;
        var viewPoints = new List<Vector3>();
        var origin = Vector3.zero;

        viewPoints.Add(origin);

        for (var i = 0; i <= RayCount; i++) {
            var angle = -ViewAngle / 2 + stepAngle * i;
            var dir = DirFromAngle(angle, true);
            var worldDir = transform.rotation * dir;

            var didHit = Physics.Raycast(transform.position, worldDir, out var rayHit, ViewRadius, ObstacleMask);
            var worldDirection = transform.rotation * dir;
            var worldPoint = didHit
                ? rayHit.point
                : transform.position + worldDirection * ViewRadius;

            var localPoint = transform.InverseTransformPoint(worldPoint);
            viewPoints.Add(localPoint);
        }

        var vertices = viewPoints.ToArray();
        var triangles = new int[(viewPoints.Count - 2) * 3];

        for (int i = 0; i < viewPoints.Count - 2; i++) {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
    }

    Vector3 DirFromAngle(float angleInDegrees, bool global) {
        if (!global) angleInDegrees += transform.eulerAngles.y;
        return Quaternion.Euler(0, angleInDegrees, 0) * Vector3.forward;
    }
    
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;

        var leftBoundary = DirFromAngle(-ViewAngle / 2, false);
        var rightBoundary = DirFromAngle(ViewAngle / 2, false);
        var origin = transform.position;

        Gizmos.DrawLine(origin, origin + leftBoundary * ViewRadius);
        Gizmos.DrawLine(origin, origin + rightBoundary * ViewRadius);
        Gizmos.DrawWireSphere(origin, ViewRadius); // просто круг обзора для наглядности
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(origin, MinAutoDetectRadius);
    }

}
