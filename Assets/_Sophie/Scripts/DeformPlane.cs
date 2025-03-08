using UnityEngine;

public class DeformPlane : MonoBehaviour
{
    public float hillHeight = 1.0f; // Max height of the hill
    public float hillSmoothness = 5.0f; // How smoothly the hill transitions
    public float slopeDirection = 1.0f; // 1 = X direction, 0 = Z direction
    public float curveStrength = 2.0f; // Adjusts how curved the slope is
    public float slopeBias = 0.5f; // Controls if one side goes down

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null) return;

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            // Blend between X and Z to control slope direction
            float slopeEffect = Mathf.Lerp(vertices[i].z, vertices[i].x, slopeDirection) / hillSmoothness;

            // Apply a curve instead of a linear slope
            float curvedSlope = Mathf.Pow(slopeEffect, curveStrength);

            // Modify one side to go down
            float directionModifier = (vertices[i].x > 0) ? 1 : -slopeBias; 

            vertices[i].y = Mathf.Lerp(0, hillHeight * directionModifier, curvedSlope);
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals(); // Fix lighting
        mesh.RecalculateBounds(); // Fix collisions
    }
}
