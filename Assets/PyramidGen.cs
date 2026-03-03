using UnityEngine;

/// <summary>
/// Draws a wireframe pyramid using GL.LINES in screen-space with perspective.
/// The pyramid has a square base and four triangular faces meeting at a single apex.
/// </summary>
public class PyramidGen : MonoBehaviour
{
    public Material material;

    [Header("Shape")]
    public float baseSize = 1f;   // half-width of the square base
    public float height   = 2f;   // total height from base to apex

    [Header("Position")]
    public float posX  = 0f;
    public float posY  = 0f;
    public float posZ  = 5f;     // depth into scene (fed into PerspectiveCamera)

    [Header("Rotation (degrees)")]
    public float rotX = 0f;
    public float rotY = 0f;
    public float rotZ = 0f;

    // -----------------------------------------------------------------------
    private void OnPostRender()
    {
        DrawPyramid();
    }

    public void DrawPyramid()
    {
        if (material == null)
        {
            Debug.LogError("[PyramidGen] Assign a material.");
            return;
        }

        GL.PushMatrix();
        material.SetPass(0);
        GL.Begin(GL.LINES);

        // ---- define local-space vertices ----
        // Square base corners (Y = 0, centered at origin)
        Vector3[] baseVerts = new Vector3[]
        {
            new Vector3(-baseSize,  0f, -baseSize),
            new Vector3( baseSize,  0f, -baseSize),
            new Vector3( baseSize,  0f,  baseSize),
            new Vector3(-baseSize,  0f,  baseSize),
        };
        Vector3 apex = new Vector3(0f, height, 0f);

        // ---- apply rotation ----
        Quaternion rot = Quaternion.Euler(rotX, rotY, rotZ);
        for (int i = 0; i < 4; i++)
            baseVerts[i] = rot * baseVerts[i];
        apex = rot * apex;

        // ---- project to screen ----
        Vector2[] baseProj = new Vector2[4];
        float[] basePersp  = new float[4];
        for (int i = 0; i < 4; i++)
        {
            float worldZ = posZ + baseVerts[i].z;
            float p = PerspectiveCamera.Instance.GetPerspective(worldZ);
            basePersp[i] = p;
            baseProj[i]  = new Vector2(
                (posX + baseVerts[i].x) * p,
                (posY + baseVerts[i].y) * p);
        }
        float   apexP    = PerspectiveCamera.Instance.GetPerspective(posZ + apex.z);
        Vector2 apexProj = new Vector2(
            (posX + apex.x) * apexP,
            (posY + apex.y) * apexP);

        // ---- draw base square ----
        for (int i = 0; i < 4; i++)
        {
            GL.Vertex(baseProj[i]);
            GL.Vertex(baseProj[(i + 1) % 4]);
        }

        // ---- draw lateral edges (base corner → apex) ----
        for (int i = 0; i < 4; i++)
        {
            GL.Vertex(baseProj[i]);
            GL.Vertex(apexProj);
        }

        GL.End();
        GL.PopMatrix();
    }
}
