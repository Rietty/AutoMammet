using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace AutoMammet
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool ViewDataInTable { get; set; } = false;
        public bool ExportTextVersion { get; set; } = false;

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
