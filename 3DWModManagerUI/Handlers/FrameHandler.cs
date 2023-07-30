using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using _3DWModManagerUI.Utils;
using ImGuiNET;
using Microsoft.Win32.SafeHandles;
using NativeFileDialogExtendedSharp;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace _3DWModManagerUI.Handlers
{
    internal class FrameHandler
    {
        public static void RunFrame(IWindow window, ref List<string> selectedFiles, string ryuModsPath, string cacheModsPath)
        {
            ImGui.SetCursorPos(UIUtils.CenterCursorWithText(window, "3DW Mod Manager"));
            UIUtils.TextColoured(new Colour(1, 0, 0, 1), "3DW Mod Manager");

            var tempPath = Path.GetTempPath() + "ModManagerUI";



            if (UIUtils.Button("Import Mod"))
            {
                var filePick = Nfd.FileOpen(new []{NfdFilterPresets.ZipFiles}, "Downloads");
                if (filePick.Status == NfdStatus.Ok)
                {

                    Console.WriteLine(filePick.Path);
                    selectedFiles.Add(filePick.Path);

                    // TODO: Actually make this fucking work properly, because why the hell would you be able to get files in a zip without it being a pain in the ass.
                    ZipArchive zip = ZipFile.Open(filePick.Path, ZipArchiveMode.Update);
                    foreach (var entry in zip.Entries)
                    {
                        if (entry.FullName.Equals(string.Empty))
                        {
                            continue;
                        }
                        entry.ExtractToFile(tempPath, true);
                    }

                    foreach (var file in Directory.EnumerateFiles(tempPath))
                    {
                        if (!Directory.GetFiles(file).Contains("romfs")||!Directory.GetFiles(file).Contains("exefs"))
                        {
                            ImGui.OpenPopup("Error Importing Mod");
                            FileUtils.ReloadDirectory(tempPath);
                            return;
                        }
                    }

                    

                    string romfsPath = Directory.GetDirectories(filePick.Path, "romfs", SearchOption.AllDirectories)[0];
                    string exefsPath = Directory.GetDirectories(filePick.Path, "exefs", SearchOption.AllDirectories)[0];
                    


                    File.Copy(filePick.Path, cacheModsPath);

                    FileUtils.CopyDirectory(romfsPath, ryuModsPath + "\\romfs");
                    FileUtils.CopyDirectory(exefsPath, ryuModsPath + "\\exefs");
                    FileUtils.CreateDirectorySafe(cacheModsPath);
                }
            }

            if (ImGui.Button("Show popup"))
                ImGui.OpenPopup("Error Importing Mod");
            


            if (ImGui.BeginPopupModal("Error Importing Mod"))
            {
                ImGui.SetItemDefaultFocus();
                ImGui.Text("Could not import mod, be sure that the file includes either a romfs, or exefs.");

                ImGui.Separator();

                if (UIUtils.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            if (UIUtils.Button("Clear Files"))
            {
                foreach (var file in Directory.EnumerateFiles(cacheModsPath))
                {
                    File.Delete(file);
                    selectedFiles.Remove(file);
                }
            }

            if (UIUtils.Button("List Current Mods"))
            {
                foreach (var file in selectedFiles)
                {
                    Console.WriteLine(Path.GetFileName(file));
                }
            }
        }
    }
}
