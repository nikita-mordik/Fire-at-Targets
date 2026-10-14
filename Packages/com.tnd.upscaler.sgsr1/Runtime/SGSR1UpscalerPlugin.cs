using TND.Upscaling.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]

namespace TND.Upscaling.SGSR1
{
    public class SGSR1UpscalerPlugin: UpscalerPlugin<SGSR1Upscaler, SGSR1UpscalerSettings>
    {
        public override UpscalerName Name => UpscalerName.SGSR1;
        public override string DisplayName => SGSR1Upscaler.DisplayName;
        public override int Priority => (int)UpscalerName.SGSR1 + 11;
        public override bool IsSupported => SystemInfo.graphicsShaderLevel >= 35;
        public override bool IsTemporalUpscaler => false;

        protected override bool TryCreateUpscaler(CommandBuffer commandBuffer, SGSR1UpscalerSettings settings, in UpscalerInitParams initParams, out SGSR1Upscaler upscaler)
        {
            upscaler = new SGSR1Upscaler(settings);
            if (!upscaler.Initialize(commandBuffer, initParams))
            {
                upscaler = null;
                return false;
            }

            return true;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void RegisterUpscalerPlugin()
        {
            RegisterUpscalerPlugin<SGSR1UpscalerPlugin>();
        }
    }
}
