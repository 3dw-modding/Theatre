using System.IO.Compression;
using Theatre.Exceptions;
using Theatre.Utils;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.Windowing;
using SearchOption = System.IO.SearchOption;

namespace Theatre.Handlers
{
    internal class FrameHandler
    {
        public static void RunFrame(IWindow window, ref List<string> selectedFiles, string ryuModsPath, string cacheModsPath) 
        {
            ImGui.SetCursorPos(UIUtils.CenterCursorWithText(window, "3DW Mod Manager"));
            UIUtils.TextColoured(new Colour(1, 0, 0), "3DW Mod Manager");

            var tempPath = Path.Join(Path.GetTempPath(), "Theatre");

            ImGui.GetStyle().FrameRounding = 25f;



            ImGui.ShowDemoWindow();



            if (UIUtils.Button("Import Mod"))
            {
                var filePick = Nfd.FileOpen(new []{NfdFilterPresets.ZipFiles}, "Downloads");
                if (filePick.Status == NfdStatus.Ok)
                {
                    string path = filePick.Path;

                    Console.WriteLine(path);
                    selectedFiles.Add(path);

                    if (Path.GetExtension(path).Equals("rar"))
                    {
                        var archive = FileUtils.ConvertFromRar(path);
                        path = path.Replace(".rar", ".zip");
                    }

                    // TODO: Actually make this fucking work properly, because why the hell would you be able to get files in a zip without it being a pain in the ass.
                    ZipFile.ExtractToDirectory(path, tempPath);

                    foreach (var file in Directory.EnumerateDirectories(tempPath))
                    {
                        if (!Directory.GetDirectories(file).Contains("romfs")||!Directory.GetDirectories(file).Contains("exefs"))
                        {
                            ImGui.OpenPopup("Error Importing Mod");
                            FileUtils.ReloadDirectory(tempPath);
                            return;
                        }
                    }

                    var thisTempPath = Path.Join(tempPath, Path.GetFileNameWithoutExtension(path));

                    try
                    {
                        var romfsPath = Directory.GetDirectories(thisTempPath, "romfs", SearchOption.AllDirectories)[0];
                        var exefsPath = Directory.GetDirectories(thisTempPath, "exefs", SearchOption.AllDirectories)[0];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("It got to this point somehow");
                        ImGui.OpenPopup("Error Importing Mod");
                        throw new InvalidModFileException("Mod file must contain either \"romfs\" or \"exefs\"", ex);
                    }
                    
                    
                    


                    File.Copy(path, Path.Combine(cacheModsPath, Path.GetFileName(path)));

                    // FileUtils.CopyDirectory(romfsPath, Path.Combine(ryuModsPath, "romfs"));
                    // FileUtils.CopyDirectory(exefsPath, Path.Combine(ryuModsPath, "exefs"));
                    // FileUtils.CreateDirectorySafe(cacheModsPath);
                }
            }

            if (ImGui.BeginPopupModal("Error Importing Mod"))
            {
                UIUtils.TextColoured(new Colour(125f, 36, 22), "There was an error importing your mod! See console for details.");
                if (UIUtils.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            if (UIUtils.Button("Show popup"))
                ImGui.OpenPopup("Error Importing Mod");


            
            

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
