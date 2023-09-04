using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.Windowing;
using Theatre.Utils;
using SharpCompress.Readers;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives;
using System.IO.Compression;
using SharpCompress.Archives.Rar;
using static Theatre.Utils.UIUtils;

namespace Theatre.Handlers
{
    internal class FrameHandler
    {
        static bool wiiu, swch = false;
        public static void RunFrame(IWindow window, ref List<string> selectedFiles, string ryuModsPath)
        {
            TextColoured(window, new Colour(1, 0, 0), "Theatre");

            ImGui.GetFont().FontSize *= 1.4f;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() * 1.05f);
            TextColoured(window, new Colour(0, 0, 1), "3DW Mod Manager");
            ImGui.GetFont().FontSize /= 1.4f;

            ImGui.GetStyle().FrameRounding = 12f;

            var tempPath = Path.GetTempPath() + "Theatre";

            ImGui.ShowDemoWindow();

            ImGui.Checkbox("View All WiiU", ref wiiu);
            ImGui.Checkbox("View All Switch", ref swch);

            if (wiiu)
            {
                TextColoured(window, new Colour(1, 0, 0), "WIIU MODS");
                ImGui.BeginChild("mod_list_wiiu");
                foreach (var keyValuePair in CachedGBMods.AllWiiUModsCached)
                {
                    Text(window, keyValuePair.Value.Name.LimitChars(15).FormatModList(keyValuePair.Value.Owner));
                    if (Button("[DOWNLOAD]"))
                    {

                    }
                }
                swch = false;
            }
            if (swch)
            {
                TextColoured(window, new Colour(1,0,0), "SWITCH MODS");
                ImGui.BeginChild("mod_list_switch");
                foreach (var keyValuePair in CachedGBMods.AllSwitchModsCached)
                {
                    Text(window, keyValuePair.Value.Name.LimitChars(15).FormatModList(keyValuePair.Value.Owner));
                    if (Button("[DOWNLOAD]"))
                    {

                    }
                    
                }
                wiiu = false;
            }

            if (Button("Import Mod"))
            {
                var filePick = Nfd.FileOpen(new []{NfdFilterPresets.ZipFiles}, "Downloads");
                if (filePick.Status == NfdStatus.Ok)
                {
                    
                }
            }

            if (ImGui.Button("Show popup"))
                ImGui.OpenPopup("Error Importing Mod");
            


            if (ImGui.BeginPopupModal("Error Importing Mod"))
            {
                ImGui.SetItemDefaultFocus();
                ImGui.Text("Could not import mod, be sure that the file includes either a romfs, or exefs.");

                ImGui.Separator();

                if (Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            if (Button("Clear Files"))
            {
                foreach (var file in Directory.EnumerateFiles("mods"))
                {
                    File.Delete(file);
                    selectedFiles.Remove(file);
                }
            }

            if (Button("List Current Mods"))
            {
                foreach (var file in selectedFiles)
                {
                    Console.WriteLine(Path.GetFileName(file));
                }
            }
        }
    }
}
