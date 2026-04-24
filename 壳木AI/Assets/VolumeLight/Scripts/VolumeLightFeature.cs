using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeLightFeature : ScriptableRendererFeature
{
    public enum DownSample { off = 1, half = 2, third = 3, quarter = 4 };
    [System.Serializable]
    public class Settings
    {
        public DownSample downsampling = DownSample.half;
        public float intensity = 1;
        public float scattering = 0;
        public float steps = 24;
        public float maxDistance = 75;
        public float jitter = 250;
        public int blurIteration = 1;
        
        public Material material;
        public Material materialAdd;
        public Material materialBlur;
        public Light light;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public static Settings settings = new Settings();

    class Pass : ScriptableRenderPass
    {
        private RenderTargetIdentifier source;
        RenderTargetHandle volumeLightTex;
        RenderTargetHandle blurTemp;
        RenderTargetHandle tempTexture;

        private string profilerTag;

        public void Setup()
        {
            renderPassEvent = settings.renderPassEvent;
        }

        public Pass(string profilerTag)
        {
            this.profilerTag = profilerTag;

            volumeLightTex.Init("LightRT");
            tempTexture.Init("Temp");
            blurTemp.Init("BlurTemp");
        }


        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var original = cameraTextureDescriptor;
            int divider = (int)settings.downsampling;

            if (Camera.current != null) //This is necessary so it uses the proper resolution in the scene window
            {
                cameraTextureDescriptor.width = (int)Camera.current.pixelRect.width / divider;
                cameraTextureDescriptor.height = (int)Camera.current.pixelRect.height / divider;
                original.width = (int)Camera.current.pixelRect.width;
                original.height = (int)Camera.current.pixelRect.height;
            }
            else //regular game window
            {
                cameraTextureDescriptor.width /= divider;
                cameraTextureDescriptor.height /= divider;
            }
           
            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
            cmd.GetTemporaryRT(blurTemp.id, cameraTextureDescriptor);
            cmd.GetTemporaryRT(volumeLightTex.id, cameraTextureDescriptor);
            ConfigureTarget(volumeLightTex.Identifier());

            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            if(settings.material != null)
            {
                settings.material.SetVector("_LightDirection", settings.light.transform.forward);
                settings.material.SetVector("_LightColor", settings.light.color);
                settings.material.SetFloat("_Scattering", settings.scattering);
                settings.material.SetFloat("_Steps", settings.steps);
                settings.material.SetFloat("_JitterVolumetric", settings.jitter);
                settings.material.SetFloat("_MaxDistance", settings.maxDistance);
                settings.material.SetFloat("_Intensity", settings.intensity);

                cmd.Blit(source, volumeLightTex.Identifier(), settings.material);
                for (int i = 0; i < settings.blurIteration; i++)
                {
                    cmd.Blit(volumeLightTex.id, blurTemp.id, settings.materialBlur, 0);
                    cmd.Blit(blurTemp.id, volumeLightTex.id, settings.materialBlur, 1);
                }

                cmd.Blit(source, tempTexture.id);
                cmd.SetGlobalTexture("_Source", tempTexture.id);
                cmd.Blit(volumeLightTex.Identifier(), source, settings.materialAdd);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }

    Pass pass;
    RenderTargetHandle renderTextureHandle;
    public override void Create()
    {
        pass = new Pass("Volumetric Light");
        name = "Volumetric Light";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(settings != null)
        {
            pass.Setup();
            renderer.EnqueuePass(pass);
        }
    }
}