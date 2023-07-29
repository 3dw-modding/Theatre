using System.Collections;
using System.Net;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using _3DWModManagerUI.Utils;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace _3DWModManagerUI
{
    public class Program 
    {
        public static Program Instance { get; private set; }

        private WebClient web = new WebClient();

        static void Main(string[] args)
        {
            Instance = new Program();

            Instance.Run();
        }












        public void Run()
        {
            using var window = Window.Create(WindowOptions.Default);

            List<string> selectedFiles = new List<string>();
            string modsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx", "mods", "contents", "010028600EBDA000");
            string modsCachePath = "mods";

            ImGuiController controller = null;
            GL gl = null;
            IInputContext inputContext = null;


            window.Load += () =>
            {
                static void SetupFonts()
                {
                    var fontPath = "_3DWModManagerUI.Fonts.Monocraft.ttf";
                    
                    GCHandle pinnedArray = GCHandle.Alloc(Utils.FileUtils.ExtractResource(fontPath), GCHandleType.Pinned);
                    IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                    ImGui.GetIO().Fonts.AddFontFromMemoryTTF(pointer, 18, 32);
                    pinnedArray.Free();
                }

                gl = window.CreateOpenGL();
                inputContext = window.CreateInput();

                controller = new ImGuiController(gl, window, inputContext, SetupFonts);

                selectedFiles = new List<string>();

                window.Center();
            };


            window.FramebufferResize += s =>
            {
                gl.Viewport(s);
            };
            
            window.Render += delta =>
            {
                controller.Update((float)delta);
                
                gl.ClearColor(.45f, .55f, .60f, 1f);
                gl.Clear((uint)ClearBufferMask.ColorBufferBit);

                ImGui.SetNextWindowPos(new Vector2(0, 0));
                ImGui.SetNextWindowSize(new Vector2(window.Size.X, window.Size.Y));
                ImGui.Begin("MainWindow", ImGuiWindowFlags.NoDecoration);

                //buttons and stuff here


                ImGui.SetCursorPos(UIUtils.CenterCursorWithText(window, "3DW Mod Manager"));
                UIUtils.TextColoured(new Vector4(1, 0, 0, 1), "3DW Mod Manager");

                

                if (UIUtils.Button("Import Mod"))
                {
                    var filePick = Nfd.PickFolder("Downloads");
                    if (filePick.Status==NfdStatus.Ok)
                    {

                        Console.WriteLine(filePick.Path);
                        selectedFiles.Add(filePick.Path);

                        string romfsPath = Directory.GetDirectories(filePick.Path, "romfs", SearchOption.AllDirectories)[0];
                        string exefsPath = Directory.GetDirectories(filePick.Path, "exefs", SearchOption.AllDirectories)[0];
                        // romfsPath = ;

                        /* Directory.CreateDirectory("files"); */

                        FileUtils.CopyDirectory(romfsPath, modsPath+"\\romfs");
                        FileUtils.CopyDirectory(exefsPath, modsPath+"\\exefs");
                        Directory.CreateDirectory(modsCachePath);

                        /* File.Copy(filePick.Path, modsPath+"\\romfs");*/
                    }
                }

                if (UIUtils.Button("Clear Files"))
                {
                    foreach (var file in Directory.EnumerateFiles(modsPath))
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



                ImGui.End();

                controller.Render();

            };
            
            window.Closing += () =>
            {
                controller.Dispose();
                inputContext.Dispose();
                gl.Dispose();
            };
            
            window.Run();

            window.Dispose();
        }
    }

    
}