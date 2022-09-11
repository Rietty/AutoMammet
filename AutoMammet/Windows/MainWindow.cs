using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;

namespace AutoMammet.Windows;

public class MainWindow : Window, IDisposable
{
    private Reader reader;
    private Plugin plugin;
    private Configuration config;
    Dictionary<string, int> supplyMapping;
    Dictionary<string, int> shiftMapping;

    public MainWindow(Plugin plugin, Reader reader) : base(
        "AutoMammet - Felicitous Furball!", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 50),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.reader = reader;
        this.plugin = plugin;
        this.config = plugin.Configuration;
        this.supplyMapping = new Dictionary<string, int>();
        this.shiftMapping = new Dictionary<string, int>();

        // Add onto dictionary for our values.
        supplyMapping["Overflowing"]    = 2;
        supplyMapping["Surplus"]        = 2;
        supplyMapping["Sufficient"]     = 2;
        supplyMapping["Insufficient"]   = 1;
        supplyMapping["Nonexistent"]    = 0;

        shiftMapping["Skyrocketing"]    = 2;
        shiftMapping["Increasing"]      = 1;
        shiftMapping["None"]            = 0;
        shiftMapping["Decreasing"]      = -1;
        shiftMapping["Plummeting"]      = -2;
    }

    public void Dispose()
    {
        // this.GoatImage.Dispose();
    }

    public override void Draw()
    {
        ImGui.Text("Please open the supply and demand window in order to load the current supply and demand for export.");
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        string[] products = reader.ExportIsleData().Split('\n', StringSplitOptions.None);

        var viewTableValue = this.config.ViewDataInTable;

        if (ImGui.Checkbox("Display Extracted Table? ", ref viewTableValue))
        {
            this.config.ViewDataInTable = viewTableValue;
            this.config.Save();
        }

        ImGui.SameLine();
        var exportTextVersionValue = this.config.ExportTextVersion;

        if (ImGui.Checkbox("Export Supply/Shift w/ Text? ", ref exportTextVersionValue))
        {
            this.config.ExportTextVersion = exportTextVersionValue;
            this.config.Save();
        }

        ImGui.Spacing();

        if (ImGui.Button("Export: Popularity"))
        {
            var sb = new StringBuilder();

            foreach (string product in products)
            {
                int index = 0;
                foreach (string info in product.Split('\t'))
                {
                    if (index == 1)
                    {
                        sb.Append(info + "\n");
                    }
                    index += 1;
                }
            }
            ImGui.SetClipboardText(sb.ToString());
        }

        ImGui.SameLine();

        // Add in the stuff for exporting..

        if (ImGui.Button("Export: Supply/Shift"))
        {
            if(exportTextVersionValue == true)
            {
                var sb = new StringBuilder();

                foreach (string product in products)
                {
                    int index = 0;
                    foreach (string info in product.Split('\t'))
                    {
                        if (index == 2)
                        {
                            sb.Append(info + ",");
                        } 
                        else if (index == 3)
                        {
                            sb.Append(info + "\n");
                        }

                        index += 1;
                    }
                }
                ImGui.SetClipboardText(sb.ToString());
            } 
            else
            {
                var sb = new StringBuilder();

                foreach (string product in products)
                {
                    int index = 0;
                    foreach (string info in product.Split('\t'))
                    {
                        if (index == 2)
                        {
                            sb.Append(supplyMapping[info] + ",");
                        }
                        else if (index == 3)
                        {
                            sb.Append(shiftMapping[info] + "\n");
                        }

                        index += 1;
                    }
                }

                ImGui.SetClipboardText(sb.ToString());
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Export: Predicted Popularity"))
        {
            var sb = new StringBuilder();

            foreach (string product in products)
            {
                int index = 0;
                foreach (string info in product.Split('\t'))
                {
                    if(index == 4)
                    {
                        sb.Append(info + "\n");
                    }
                    index += 1;
                }
            }
            ImGui.SetClipboardText(sb.ToString());
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Create a new table to show relevant data.
        if (this.config.ViewDataInTable == true && ImGui.BeginTable("Supply & Demand", 5))
        {
            ImGui.TableSetupColumn("Product");
            ImGui.TableSetupColumn("Popularity");
            ImGui.TableSetupColumn("Supply");
            ImGui.TableSetupColumn("Demand Shift");
            ImGui.TableSetupColumn("Predicted Popularity");
            ImGui.TableHeadersRow();

            foreach (string product in products)
            {
                ImGui.TableNextRow();
                int colIndex = 0;
                foreach (string info in product.Split('\t'))
                {
                    ImGui.TableSetColumnIndex(colIndex);
                    ImGui.Text(info);
                    colIndex++;
                }
            }
            ImGui.EndTable();
            ImGui.Separator();
        }
    }
}
