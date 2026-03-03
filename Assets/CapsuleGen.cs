using UnityEngine;

/// <summary>
/// BONUS: Draws a wireframe capsule using GL.LINES in screen-space with perspective.
/// A capsule = cylinder body + two hemisphere caps.
/// </summary>
public class CapsuleGen : MonoBehaviour
{
    public Material material;

    [Header("Shape")]
    public float radius   = 0.8f;
    public float height   = 2f;    // length of the cylindrical middle section
    public int   segments = 16;    // longitude divisions (> 5)
    public int   rings    = 8;     // latitude divisions per hemisphere (> 5)

    [Header("Position")]
    public float posX = -3f;
    public float posY =  0f;
    public float posZ =  5f;

    [Header("Rotation (degrees)")]
    public float rotX = 0f;
    public float rotY = 0f;
    public float rotZ = 0f;

    // -----------------------------------------------------------------------
    private Quaternion Rot => Quaternion.Euler(rotX, rotY, rotZ);

    private Vector2 Project(Vector3 local)
    {
        local = Rot * local;
        float worldZ = posZ + local.z;
        float p      = PerspectiveCamera.Instance.GetPerspective(worldZ);
        return new Vector2((posX + local.x) * p, (posY + local.y) * p);
    }

    private void OnPostRender()
    {
        DrawCapsule();
    }

    public void DrawCapsule()
    {
        if (material == null)
        {
            Debug.LogError("[CapsuleGen] Assign a material.");
            return;
        }

        GL.PushMatrix();
        material.SetPass(0);
        GL.Begin(GL.LINES);

        float dPhi = 2f * Mathf.PI / segments;
        float halfH = height * 0.5f;
        float dTheta = (Mathf.PI * 0.5f) / rings;  // 90° per hemisphere

        // ---------- cylinder body ----------
        // Two end rings + vertical lines
        for (int s = 0; s < segments; s++)
        {
            float phi0 = s       * dPhi;
            float phi1 = (s + 1) * dPhi;
            float x0 = radius * Mathf.Cos(phi0);
            float z0 = radius * Mathf.Sin(phi0);
            float x1 = radius * Mathf.Cos(phi1);
            float z1 = radius * Mathf.Sin(phi1);

            // top ring
            GL.Vertex(Project(new Vector3(x0, halfH, z0)));
            GL.Vertex(Project(new Vector3(x1, halfH, z1)));
            // bottom ring
            GL.Vertex(Project(new Vector3(x0, -halfH, z0)));
            GL.Vertex(Project(new Vector3(x1, -halfH, z1)));
            // vertical edges
            GL.Vertex(Project(new Vector3(x0,  halfH, z0)));
            GL.Vertex(Project(new Vector3(x0, -halfH, z0)));
        }

        // ---------- top hemisphere ----------
        for (int r = 0; r < rings; r++)
        {
            float theta0 = r       * dTheta;   // 0 = equator, PI/2 = pole
            float theta1 = (r + 1) * dTheta;

            for (int s = 0; s < segments; s++)
            {
                float phi0 = s       * dPhi;
                float phi1 = (s + 1) * dPhi;

                // latitude ring at theta0
                Vector3 a = HemiPoint(theta0, phi0, +halfH);
                Vector3 b = HemiPoint(theta0, phi1, +halfH);
                // latitude ring at theta1
                Vector3 c = HemiPoint(theta1, phi0, +halfH);

                GL.Vertex(Project(a)); GL.Vertex(Project(b));   // ring arc
                GL.Vertex(Project(a)); GL.Vertex(Project(c));   // meridian
            }
        }

        // ---------- bottom hemisphere ----------
        for (int r = 0; r < rings; r++)
        {
            float theta0 = r       * dTheta;
            float theta1 = (r + 1) * dTheta;

            for (int s = 0; s < segments; s++)
            {
                float phi0 = s       * dPhi;
                float phi1 = (s + 1) * dPhi;

                Vector3 a = HemiPoint(theta0, phi0, -halfH);
                Vector3 b = HemiPoint(theta0, phi1, -halfH);
                Vector3 c = HemiPoint(theta1, phi0, -halfH);

                GL.Vertex(Project(a)); GL.Vertex(Project(b));
                GL.Vertex(Project(a)); GL.Vertex(Project(c));
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    /// <summary>
    /// Returns a point on a hemisphere.
    /// theta=0 is the equator, theta=PI/2 is the pole.
    /// yOffset shifts the hemisphere up (+halfH) or down (-halfH).
    /// </summary>
    private Vector3 HemiPoint(float theta, float phi, float yOffset)
    {
        float sinT = Mathf.Sin(theta);
        float cosT = Mathf.Cos(theta);
        float sign = Mathf.Sign(yOffset);

        return new Vector3(
            radius * cosT * Mathf.Cos(phi),
            yOffset + sign * radius * sinT,
            radius * cosT * Mathf.Sin(phi));
    }
}
