using UnityEngine;

public class SphereGen : MonoBehaviour
{
    public Material material;

    [Header("Shape")]
    public float radius      = 1f;
    public int   latSegments = 12;   // number of horizontal rings  (> 5)
    public int   lonSegments = 16;   // number of vertical arcs     (> 5)

    [Header("Position")]
    public float posX = 3f;
    public float posY = 0f;
    public float posZ = 5f;

    [Header("Rotation (degrees)")]
    public float rotX = 0f;
    public float rotY = 0f;
    public float rotZ = 0f;

    // -----------------------------------------------------------------------
    private void OnPostRender()
    {
        DrawSphere();
    }

    /// <summary>Converts spherical to local Cartesian, rotates, then projects.</summary>
    private Vector2 Project(float theta, float phi)
    {
        Vector3 local = new Vector3(
            radius * Mathf.Sin(theta) * Mathf.Cos(phi),
            radius * Mathf.Cos(theta),
            radius * Mathf.Sin(theta) * Mathf.Sin(phi));

        local = Quaternion.Euler(rotX, rotY, rotZ) * local;

        float worldZ = posZ + local.z;
        float p      = PerspectiveCamera.Instance.GetPerspective(worldZ);
        return new Vector2((posX + local.x) * p, (posY + local.y) * p);
    }

    public void DrawSphere()
    {
        if (material == null)
        {
            Debug.LogError("[SphereGen] Assign a material.");
            return;
        }

        GL.PushMatrix();
        material.SetPass(0);
        GL.Begin(GL.LINES);

        float dTheta = Mathf.PI / latSegments;
        float dPhi   = 2f * Mathf.PI / lonSegments;

        // ---- latitude rings ----
        for (int lat = 1; lat < latSegments; lat++)   // skip poles (degenerate)
        {
            float theta = lat * dTheta;
            for (int lon = 0; lon < lonSegments; lon++)
            {
                float phi0 = lon       * dPhi;
                float phi1 = (lon + 1) * dPhi;
                GL.Vertex(Project(theta, phi0));
                GL.Vertex(Project(theta, phi1));
            }
        }

        // ---- longitude arcs ----
        for (int lon = 0; lon < lonSegments; lon++)
        {
            float phi = lon * dPhi;
            for (int lat = 0; lat < latSegments; lat++)
            {
                float theta0 = lat       * dTheta;
                float theta1 = (lat + 1) * dTheta;
                GL.Vertex(Project(theta0, phi));
                GL.Vertex(Project(theta1, phi));
            }
        }

        GL.End();
        GL.PopMatrix();
    }
}
