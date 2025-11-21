using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        public Shader pixelShader;
        [Range(0.1f, 3.0f)]
        public float lightIntensity = 1.25f;
        [Range(0f, 1f)]
        public float lineAlpha = 0.7f;
        public bool useLighting = true;
        [Range(0f, 1f)]
        public float lineHighlight = 0.2f;
        [Range(0f, 1f)]
        public float lineShadow = 0.55f;
    }

    public Settings settings = new Settings();
    private PixelRenderPass pixelPass;

    public override void Create()
    {
        if (settings.pixelShader != null)
            pixelPass = new PixelRenderPass(settings, new Material(settings.pixelShader));
        pixelPass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.pixelShader == null)
        {
            Debug.LogWarningFormat("Missing Pixel Shader. {0} render pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        // Don't pass the color target here, pass it during execution
        renderer.EnqueuePass(pixelPass);
    }

    class PixelRenderPass : ScriptableRenderPass
    {
        private Settings settings;
        private Material pixelMaterial;
        private RTHandle tempTexture;
        private const string tempTextureName = "_TempPixelTexture";

        // Constructor không đổi
        public PixelRenderPass(Settings settings, Material material)
        {
            this.settings = settings;
            this.pixelMaterial = material;
            // Yêu cầu Depth và Normals là đúng
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        }

        // Configure không đổi
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor desc)
        {
            // Đảm bảo descriptor hợp lệ trước khi cấp phát texture
            if (desc.width > 0 && desc.height > 0)
            {
                RenderingUtils.ReAllocateIfNeeded(ref tempTexture, desc, name: tempTextureName);
            }
        }

        // Execute được cập nhật
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // --- CÁC BƯỚC KIỂM TRA MỚI ---
            if (pixelMaterial == null)
            {
                Debug.LogError("Pixel Material không tồn tại.");
                return;
            }

            if (tempTexture == null)
            {
                // Điều này xảy ra nếu Configure không được gọi với descriptor hợp lệ
                Debug.LogWarning("TempTexture chưa được cấp phát. Bỏ qua render pass.");
                return;
            }
            // --- KẾT THÚC CÁC BƯỚC KIỂM TRA ---

            var renderer = renderingData.cameraData.renderer;
            var colorTarget = renderer.cameraColorTargetHandle;

            // KIỂM TRA QUAN TRỌNG NHẤT: Đảm bảo colorTarget hợp lệ trước khi Blit
            // Chính bước kiểm tra này sẽ ngăn lỗi "Assertion failed"
            if (colorTarget == null)
            {
                Debug.LogWarning("Camera color target is null. Skipping render pass.");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("URP/PixelEffect");

            // Set shader properties
            pixelMaterial.SetFloat("_LightIntensity", settings.lightIntensity);
            pixelMaterial.SetFloat("_LineAlpha", settings.lineAlpha);
            pixelMaterial.SetFloat("_UseLighting", settings.useLighting ? 1f : 0f);
            pixelMaterial.SetFloat("_LineHighlight", settings.lineHighlight);
            pixelMaterial.SetFloat("_LineShadow", settings.lineShadow);

            // Blit
            Blitter.BlitCameraTexture(cmd, colorTarget, tempTexture, pixelMaterial, 0);
            Blitter.BlitCameraTexture(cmd, tempTexture, colorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            tempTexture?.Release();
        }
    }
}