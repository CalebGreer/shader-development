using UnityEngine;

[ExecuteInEditMode]
public class Clouds : MonoBehaviour
{
    [SerializeField] private Shader m_cloudShader;
    [SerializeField] private float m_minHeight = 0.0f;
    [SerializeField] private float m_maxHeight = 5.0f;
    [SerializeField] private float m_fadeDistance = 2.0f;
    [SerializeField] private float m_scale = 5.0f;
    [SerializeField] private float m_steps = 50.0f;
    [SerializeField] private Texture m_valueNoiseImage;
    [SerializeField] private Transform m_sun;

    private Camera m_camera;
    [SerializeField] private Material m_material;

    public Material Material { 
        get
        {
             if(m_material == null && m_cloudShader != null)
            {
                m_material = new Material(m_cloudShader);
            }

            if (m_material != null && m_cloudShader == null)
            {
                DestroyImmediate(m_material);
            }

            if(m_material != null && m_cloudShader && m_cloudShader != m_material.shader)
            {
                DestroyImmediate(m_material);
                m_material = new Material(m_cloudShader);
            }

            return m_material;
        }
    }

    private void Start()
    {
        if (m_material.shader == null)
            DestroyImmediate(m_material);
        m_material = Material;
    }

    private Matrix4x4 GetFrustumCorners()
    {
        Matrix4x4 frustumCorners = Matrix4x4.identity;
        Vector3[] fCorners = new Vector3[4];

        m_camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), m_camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, fCorners);

        frustumCorners.SetRow(0, Vector3.Scale(fCorners[1], new Vector3(1,1,-1)));
        frustumCorners.SetRow(1, Vector3.Scale(fCorners[2], new Vector3(1, 1, -1)));
        frustumCorners.SetRow(2, Vector3.Scale(fCorners[3], new Vector3(1, 1, -1)));
        frustumCorners.SetRow(3, Vector3.Scale(fCorners[0], new Vector3(1, 1, -1)));

        return frustumCorners;
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Material == null || m_valueNoiseImage == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (m_camera == null)
        {
            m_camera = GetComponent<Camera>();
        }

        Material.SetTexture("_ValueNoise", m_valueNoiseImage);
        if (m_sun != null)
        {
            Material.SetVector("_SunDir", -m_sun.forward);
        }
        else;
        {
            Material.SetVector("_SunDir", Vector3.up);
        }

        Material.SetFloat("_MinHeight", m_minHeight);
        Material.SetFloat("_MaxHeight", m_maxHeight);
        Material.SetFloat("_FadeDist", m_fadeDistance);
        Material.SetFloat("_Scale", m_scale);
        Material.SetFloat("_Steps", m_steps);

        Material.SetMatrix("_FrustumCornersWS", GetFrustumCorners());
        Material.SetMatrix("_CameraInvViewMatrix", m_camera.cameraToWorldMatrix);
        Material.SetVector("_CameraPosWS", m_camera.transform.position);

        CustomGraphicsBlit(source, destination, Material, 0);
    }

    private void CustomGraphicsBlit(RenderTexture source, RenderTexture destination, Material fxMaterial, int passNumber)
    {
        RenderTexture.active = destination;

        fxMaterial.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();

        fxMaterial.SetPass(passNumber);

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

        GL.End();
        GL.PopMatrix();
    }

    protected virtual void OnDisable()
    {
        if(m_material)
        {
            DestroyImmediate(m_material);
        }
    }
}
