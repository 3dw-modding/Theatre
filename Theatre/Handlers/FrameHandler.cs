using System.IO.Compression;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.Windowing;
using Theatre.Utils;

namespace Theatre.Handlers
{
    internal class FrameHandler
    {
        public static void RunFrame(IWindow window, ref List<string> selectedFiles, string ryuModsPath, string cacheModsPath)
        {
            UIUtils.TextColoured(window, new Colour(1, 0, 0), "3DW Mod Manager");

            ImGui.GetFont().FontSize *= 1.4f;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() * 1.05f);
            UIUtils.TextColoured(window, new Colour(0, 0, 1), "Theatre");
            ImGui.GetFont().FontSize /= 1.4f;
            

            var tempPath = Path.GetTempPath() + "ModManagerUI";



            if (UIUtils.Button("Import Mod"))
            {
                var filePick = Nfd.FileOpen(new []{NfdFilterPresets.ZipFiles}, "Downloads");
                if (filePick.Status == NfdStatus.Ok)
                {

                    Console.WriteLine(filePick.Path);
                    selectedFiles.Add(filePick.Path);
                    Directory.CreateDirectory(tempPath);


                    ZipArchive zip = ZipFile.Open(filePick.Path, ZipArchiveMode.Update);
                    zip.ExtractToDirectory(tempPath);
                    zip.Dispose();

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
