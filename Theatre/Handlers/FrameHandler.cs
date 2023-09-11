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
using System.Text.Json.Nodes;
using System.Text.Json;
using Newtonsoft.Json;

namespace Theatre.Handlers
{
    internal class FrameHandler
    {
        static bool wiiu = false;
        static bool swch = true;
        static int currTabIndex = 0;
        static FInfo temp = new FInfo();
        static string tempFile = "";
        public static void RunFrame(IWindow window, ref List<string> selectedFiles, string ryuModsPath)
        {
            if (ImGui.BeginTabBar("tabs"))
            {
                if (ImGui.BeginTabItem("My mods"))
                {
                    currTabIndex = 0;
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Get Mods"))
                {
                    currTabIndex = 1;
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            TextColoured(window, new Colour(1, 0, 0), "Theatre");

            ImGui.GetFont().FontSize *= 1.4f;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() * 1.05f);
            TextColoured(window, new Colour(0, 0, 1), "3DW Mod Manager");
            ImGui.GetFont().FontSize /= 1.4f;

            ImGui.GetStyle().FrameRounding = 12f;

            var tempPath = Path.GetTempPath() + "Theatre";

            ImGui.ShowDemoWindow();

            switch (currTabIndex)
            {
                case 0:
                    {
                        var moddir = new FileInfo(Path.Join(new FileInfo(Environment.ProcessPath!).Directory!.FullName, "Mods"));
                        var files = Directory.GetDirectories(moddir.FullName);
                        for (int i = 0; i < files.Length; i++)
                        {
                            DirectoryInfo dinfo = new(Path.Join(moddir.FullName,
                                Path.GetFileNameWithoutExtension(new DirectoryInfo(files[i]).Name)));
                            if (dinfo.GetFiles().Count() > 0)
                            {
                                var info = JsonConvert.DeserializeObject<FInfo>(File.ReadAllText(Path.Join(dinfo.FullName, "FileInfo.json")));
                                ImGui.Text(info.Name);
                            }
                                
                        }
                        return;
                    }
                case 1:
                    {
                        ImGui.Checkbox("View All Switch", ref swch);
                        ImGui.Checkbox("View All WiiU", ref wiiu);
                        if (swch)
                        {
                            TextColoured(window, new Colour(1, 0, 0), "SWITCH MODS");
                            ImGui.BeginChild("mod_list_switch");
                            foreach (var keyValuePair in CachedGBMods.AllSwitchModsCached)
                            {
                                Text(window, keyValuePair.Value.Name.LimitChars(15).FormatModList(keyValuePair.Value.Owner));
                                if (Button("[DOWNLOAD]"))
                                {
                                    keyValuePair.Value.DownloadSwitchMod();
                                }

                            }
                            wiiu = false;
                        }

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

                        if (Button("Import Local Mod"))
                        {
                            var filePick = Nfd.FileOpen(new[] { NfdFilterPresets.ZipFiles }, "Downloads");
                            if (filePick.Status == NfdStatus.Ok)
                            {
                                tempFile = filePick.Path;
                                ImGui.OpenPopup("Mod Info");
                            } else
                            {
                                ImGui.OpenPopup("Error Importing Mod");
                            }
                        }

                        if (ImGui.BeginPopupModal("Mod Info"))
                        {
                            ImGui.InputTextWithHint("Name", "The name of the mod", ref temp.Name, 35);
                            ImGui.InputTextWithHint("Owner", "The owner of the mod", ref temp.Owner, 15);
                            ImGui.InputTextWithHint("Description", "The description of the mod", ref temp.Description, 50);
                            if (ImGui.Button("OK"))
                            {
                                var info = temp;
                                if (info.Owner == "")
                                {
                                    return;
                                }
                                FileUtils.DownloadSwitchMod(tempFile, info);
                                tempFile = "";
                                ImGui.CloseCurrentPopup();
                            }
                            ImGui.EndPopup();
                        }
                        return;
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
        }
    }


    public class FInfo
    {
        public string Name = string.Empty;
        public string Owner = string.Empty;
        public string Description = string.Empty;

        public static FInfo FromJson(JsonArray array)
        {
            return new()
            {
                Name = array[0]?.Deserialize<string>() ?? string.Empty,
                Owner = array[1]?.Deserialize<string>() ?? string.Empty,
                Description = array[2]?.Deserialize<string>() ?? string.Empty
            };
        }
    }
}
