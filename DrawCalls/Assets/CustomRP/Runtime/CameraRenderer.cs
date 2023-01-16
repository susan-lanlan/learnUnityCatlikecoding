using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{

    ScriptableRenderContext context;
    Camera camera;
    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };
    CullingResults cullingResults;
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull())
        {
            return;
        }

        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }
    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
    void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        //����������״̬
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,  //�Ƿ�Ҫ�����Ȼ���
            flags == CameraClearFlags.Color,  //�Ƿ�Ҫ�����ɫ����
            flags == CameraClearFlags.Color ? //���������������������ɫֵ
                camera.backgroundColor.linear : Color.clear
        );
        //buffer.ClearRenderTarget(true, true, Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();

    }
    void DrawVisibleGeometry()
    {
        //���û���˳���ָ����Ⱦ���
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        //������Ⱦ��Shader Pass����Ⱦ����
        var drawingSettings = new DrawingSettings(unlitShaderTagId,sortingSettings);
        //ֻ����RenderQueueΪopaque��͸��������
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        //1.���Ʋ�͸������
        context.DrawRenderers(cullingResults,ref drawingSettings,ref filteringSettings);
        //2.������պ�
        context.DrawSkybox(camera);
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        //ֻ����RenderQueueΪtransparent͸��������
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        //3.����͸������
        context.DrawRenderers(cullingResults,ref drawingSettings,ref filteringSettings);
    }
    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
}

