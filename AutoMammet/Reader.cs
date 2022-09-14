using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud;
using Dalamud.Plugin;
using System.Reflection;
using Dalamud.Utility.Signatures;
using Lumina.Excel.GeneratedSheets;
using Lumina.Excel;

namespace AutoMammet
{
    // Code from Otter, just slight changes to fix warnings/get it plugin ready.
    public class Reader
    {
        [Signature("E8 ?? ?? ?? ?? 8B 50 10")]
        private readonly unsafe delegate* unmanaged<IntPtr> readerInstance = null!;

        private IReadOnlyList<string> items;
        private IReadOnlyList<string> popularities;
        private IReadOnlyList<string> supplies;
        private IReadOnlyList<string> shifts;

        private IReadOnlyList<string> supplies_english;
        private IReadOnlyList<string> shifts_english;

        private ExcelSheet<MJICraftworksPopularity> sheet;

        private Configuration config;

        public Reader(DalamudPluginInterface pluginInterface, Plugin plugin)
        {
            Dalamud.Initialize(pluginInterface);
            SignatureHelper.Initialise(this);

            this.config = plugin.Configuration;

            UpdateTable();
        }

        private void UpdateTable()
        {
            items = Dalamud.GameData.GetExcelSheet<MJICraftworksObject>((ClientLanguage)this.config.ExportLanguage)!.Select(o => o.Item.Value?.Name.ToString() ?? string.Empty)
               .Where(s => s.Length > 0).Prepend(string.Empty).ToArray();

            var addon = Dalamud.GameData.GetExcelSheet<Addon>((ClientLanguage)this.config.ExportLanguage)!;
            var addon_english = Dalamud.GameData.GetExcelSheet<Addon>(ClientLanguage.English)!;

            shifts = Enumerable.Range(15186, 5).Select(i => addon.GetRow((uint)i)!.Text.ToString()).ToArray();
            supplies = Enumerable.Range(15181, 5).Reverse().Select(i => addon.GetRow((uint)i)!.Text.ToString()).ToArray();

            shifts_english = Enumerable.Range(15186, 5).Select(i => addon_english.GetRow((uint)i)!.Text.ToString()).ToArray();
            supplies_english = Enumerable.Range(15181, 5).Reverse().Select(i => addon_english.GetRow((uint)i)!.Text.ToString()).ToArray();

            popularities = Enumerable.Range(15177, 4).Select(i => addon.GetRow((uint)i)!.Text.ToString()).Prepend(string.Empty).ToArray();
            sheet = Dalamud.GameData.GetExcelSheet<MJICraftworksPopularity>()!;
        }

        public unsafe string ExportIsleData()
        {
            UpdateTable();
            var instance = readerInstance();
            if (instance == IntPtr.Zero)
                return string.Empty;

            var currentPopularity = sheet.GetRow(*(byte*)(instance + 0x2E8))!;
            var nextPopularity = sheet.GetRow(*(byte*)(instance + 0x2E9))!;

            var sb = new StringBuilder(64 * 128);
            for (var i = 1; i < items.Count; ++i)
            {
                sb.Append(items[i]); // 0
                sb.Append('\t');
                sb.Append(GetPopularity(currentPopularity, i)); // 1
                sb.Append('\t');
                var supply = *(byte*)(instance + 0x2EA + i);
                var shift = supply & 0x7;
                supply = (byte)(supply >> 4);
                sb.Append(supplies[supply]); // 2
                sb.Append('\t');
                sb.Append(shifts[shift]); // 3
                sb.Append('\t');
                sb.Append(GetPopularity(nextPopularity, i)); // 4
                sb.Append('\t');
                sb.Append(supplies_english[supply]); // 5
                sb.Append('\t');
                sb.Append(shifts_english[shift]); // 6
                sb.Append('\n');
            }

            return sb.ToString();
        }

        private string GetPopularity(MJICraftworksPopularity pop, int idx)
        {
            var val = (byte?)pop.GetType().GetProperty($"Unknown{idx}", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)?.GetValue(pop, null);
            return val == null ? string.Empty : popularities[val.Value];
        }
    }
}
