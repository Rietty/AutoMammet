using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Dalamud;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Newtonsoft.Json;

namespace AutoMammet.Windows;

public class MainWindow : Window, IDisposable
{
    private Reader reader;
    private Plugin plugin;
    private Configuration config;
    Dictionary<string, int> wordMapping = null;

    public MainWindow(Plugin plugin, Reader reader, string valueMappingPath) : base(
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

        this.wordMapping = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(valueMappingPath));
    }

    public void Dispose()
    {
        Dalamud.ClientState.ClientLanguage.ToString();
    }

    public String ColumnBuilder(String[] input, int idx, char delimiter, char sep, bool lookup)
    {
        var sb = new StringBuilder();
        foreach (string words in input)
        {
            int index = 0;
            foreach (string word in words.Split(delimiter))
            {
                if (index == idx)
                {
                    if(lookup == true)
                    {
                        sb.Append(wordMapping[word].ToString() + sep);
                    } 
                    else
                    {
                        sb.Append(word + sep);
                    }
                    
                }
                index += 1;
            }
        }

        return sb.ToString();
    }

    public override void Draw()
    {
        ImGui.Text("Please open the supply and demand window in order to load the current supply and demand for export.");
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        string[] products = reader.ExportIsleData().Split('\n', StringSplitOptions.None);

        var viewTableValue = this.config.ViewDataInTable;

        if (ImGui.Checkbox("Display Extracted Table?", ref viewTableValue))
        {
            this.config.ViewDataInTable = viewTableValue;
            this.config.Save();
        }

        ImGui.SameLine();

        var exportTextVersionValue = this.config.ExportTextVersion;

        if (ImGui.Checkbox("Export Supply/Shift w/ Text?", ref exportTextVersionValue))
        {
            this.config.ExportTextVersion = exportTextVersionValue;
            this.config.Save();
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(87);

        string[] languages = { "ja", "en", "de", "fr" };

        if (ImGui.BeginCombo("Language", languages[this.config.ExportLanguage]))
        {
            for (int i = 0; i < languages.Length; i++)
            {
                if (ImGui.Selectable(languages[i]))
                {
                    this.config.ExportLanguage = i;
                    this.config.Save();
                }
            }
            ImGui.EndCombo();
        }

        ImGui.Spacing();

        if (ImGui.Button("Export: Popularity"))
        {
            ImGui.SetClipboardText(ColumnBuilder(products, 1, '\t', '\n', false));
        }

        ImGui.SameLine();

        // Add in the stuff for exporting..

        if (ImGui.Button("Export: Supply"))
        {
            if (!this.config.ExportTextVersion)
            {
                ImGui.SetClipboardText(ColumnBuilder(products, 5, '\t', '\n', true));
            } 
            else
            {
                ImGui.SetClipboardText(ColumnBuilder(products, 2, '\t', '\n', false));
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Export: Demand Shift"))
        {
            if (!this.config.ExportTextVersion)
            {
                ImGui.SetClipboardText(ColumnBuilder(products, 6, '\t', '\n', true));
            }
            else
            {
                ImGui.SetClipboardText(ColumnBuilder(products, 3, '\t', '\n', false));
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Export: Predicted Popularity"))
        {
            ImGui.SetClipboardText(ColumnBuilder(products, 4, '\t', '\n', false));
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
                    if (colIndex < 5)
                    {
                        ImGui.TableSetColumnIndex(colIndex);
                        ImGui.Text(info);
                        colIndex++;
                    }
                }
            }
            ImGui.EndTable();
            ImGui.Separator();
        }
    }
}
