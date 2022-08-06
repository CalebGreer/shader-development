using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIGradientEffect : BaseMeshEffect
{
    private const string SHADER_PATH = "Shaders/UIGradient";
    private const string MATERIAL_INSTANCE_NAME = "UIGradient_InstanceEffect";
    private const string ORIENTATION_NAME = "_IsVertical";
    private const string ALPHACLIP_NAME = "Use Alpha Clip";

    public enum GradientOrientations
    {
        Horizontal,
        Vertical
    }

    [Header("Graphic")]
    [SerializeField] private Graphic m_graphic;
    [SerializeField] private GradientOrientations m_Orientation;
    [SerializeField] private Gradient m_gradient = new Gradient();
    [SerializeField] private bool m_useAlphaClip = false;
    // The colors are applied per-vertex so if you have multiple points on your gradient where the color changes 
    // and there aren't enough vertices, you won't see all of the colors.
    [SerializeField] private bool isText = false;

    //Just for testing in Editor easily
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (graphic != null)
        {
            if (isText)
                graphic.SetVerticesDirty();

            Initialize();
        }
        base.OnValidate();
    }

    protected override void Reset()
    {
        base.Reset();

        if (m_graphic == null)
        {
            m_graphic = GetComponent<Graphic>();
            
        }
    }
#endif

    public void Initialize()
    {
        if (m_graphic == null)
        {
#if UNITY_EDITOR
            Debug.LogError(string.Format("[{0}][{1}] Initialization failed, Graphic component is null", gameObject.name, GetType()));
#endif
            return;
        }

        if (isText)
        {
            m_graphic.material = null;
        }
        else
        {
            // Create an instance of the gradient material if we're not using the default (shared material)
            if (m_graphic.material == null || !string.Equals(m_graphic.material.name, MATERIAL_INSTANCE_NAME))
                CreateMaterialInstance();

            InitializeMaterialValues();
        }
    }

    private void CreateMaterialInstance()
    {
        Shader shader = Resources.Load<Shader>(SHADER_PATH);
        if (shader == null)
        {
#if !UKEN_PROD
            Debug.LogError(string.Format("[{0}] Failed to load shader at: {1}", gameObject.name, SHADER_PATH));
#endif
            return;
        }

        m_graphic.material = new Material(shader);
        m_graphic.material.name = MATERIAL_INSTANCE_NAME;
    }

    // Used for setting up the Shader when the graphic is not a text object
    private void InitializeMaterialValues()
    {
        switch (m_Orientation)
        {
            case GradientOrientations.Vertical:
                m_graphic.materialForRendering.SetFloat(ORIENTATION_NAME, 1);
                break;
            case GradientOrientations.Horizontal:
                m_graphic.materialForRendering.SetFloat(ORIENTATION_NAME, 0);
                break;
        }

        if (m_useAlphaClip)
        {
            m_graphic.materialForRendering.SetFloat(ALPHACLIP_NAME, 1);
        }
        else
        {
            m_graphic.materialForRendering.SetFloat(ALPHACLIP_NAME, 0);
        }

        int arrSize = m_graphic.materialForRendering.GetInt("_Colors");

        GradientColorKey[] colorKeys = new GradientColorKey[arrSize];
        for (int i = 0; i < arrSize; i++)
        {
            colorKeys[i] = new GradientColorKey(m_graphic.materialForRendering.GetColor($"_Color{i}"), m_graphic.materialForRendering.GetFloat($"_ColorTime{i}"));
        }

        arrSize = m_graphic.materialForRendering.GetInt("_Alphas");
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[arrSize];
        for (int i = 0; i < arrSize; i++)
        {
            alphaKeys[i] = new GradientAlphaKey(m_graphic.materialForRendering.GetFloat($"_Alpha{i}"), m_graphic.materialForRendering.GetFloat($"_AlphaTime{i}"));
        }

        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);

        for (int i = 0; i < 8; i++)
        {
            m_graphic.materialForRendering.SetColor($"_Color{i}", Color.magenta);
            m_graphic.materialForRendering.SetFloat($"_ColorTime{i}", -1.0f);
            m_graphic.materialForRendering.SetFloat($"_Alpha{i}", -1.0f);
            m_graphic.materialForRendering.SetFloat($"_AlphaTime{i}", -1.0f);
        }

        for (int i = 0; i < m_gradient.colorKeys.Length; i++)
        {
            m_graphic.materialForRendering.SetColor($"_Color{i}", m_gradient.colorKeys[i].color);
            m_graphic.materialForRendering.SetFloat($"_ColorTime{i}", m_gradient.colorKeys[i].time);
        }

        for (int i = 0; i < m_gradient.alphaKeys.Length; i++)
        {
            m_graphic.materialForRendering.SetFloat($"_Alpha{i}", m_gradient.alphaKeys[i].alpha);
            m_graphic.materialForRendering.SetFloat($"_AlphaTime{i}", m_gradient.alphaKeys[i].time);
        }

        m_graphic.materialForRendering.SetInt("_Colors", m_gradient.colorKeys.Length);

        m_graphic.materialForRendering.SetInt("_Alphas", m_gradient.alphaKeys.Length);
    }

    // Used for when the graphic is a text object
    public override void ModifyMesh(VertexHelper helper)
    {
        if (!IsActive() || helper.currentVertCount == 0 || !isText)
            return;

        List<UIVertex> _vertexList = new List<UIVertex>();

        helper.GetUIVertexStream(_vertexList);

        int nCount = _vertexList.Count;
        switch (m_Orientation)
        {
            case GradientOrientations.Horizontal:
                {
                    float left = _vertexList[0].position.x;
                    float right = _vertexList[0].position.x;
                    float x = 0f;

                    for (int i = nCount - 1; i >= 1; --i)
                    {
                        x = _vertexList[i].position.x;

                        if (x > right) right = x;
                        else if (x < left) left = x;
                    }

                    float width = 1f / (right - left);
                    UIVertex vertex = new UIVertex();

                    for (int i = 0; i < helper.currentVertCount; i++)
                    {
                        helper.PopulateUIVertex(ref vertex, i);

                        vertex.color = m_gradient.Evaluate((vertex.position.x - left) * width);

                        helper.SetUIVertex(vertex, i);
                    }
                }
                break;

            case GradientOrientations.Vertical:
                {
                    float bottom = _vertexList[0].position.y;
                    float top = _vertexList[0].position.y;
                    float y = 0f;

                    for (int i = nCount - 1; i >= 1; --i)
                    {
                        y = _vertexList[i].position.y;

                        if (y > top) top = y;
                        else if (y < bottom) bottom = y;
                    }

                    float height = 1f / (top - bottom);
                    UIVertex vertex = new UIVertex();

                    for (int i = 0; i < helper.currentVertCount; i++)
                    {
                        helper.PopulateUIVertex(ref vertex, i);

                        vertex.color = m_gradient.Evaluate((vertex.position.y - bottom) * height);

                        helper.SetUIVertex(vertex, i);
                    }
                }
                break;
        }
    }
}
