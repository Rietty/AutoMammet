using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using AutoMammet.Windows;

namespace AutoMammet
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "AutoMammet";
        private const string CommandName = "/mammet";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("AutoMammet");

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            Reader reader = new Reader(this.PluginInterface);
            WindowSystem.AddWindow(new MainWindow(this, reader));

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Shows a simple pop-up window for our plugin!"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;

        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            WindowSystem.GetWindow("AutoMammet - Felicitous Furball!").IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            WindowSystem.GetWindow("AutoMammet Configuration Window").IsOpen = true;
        }
    }
}
