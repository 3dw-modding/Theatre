using System.Collections;
using System.Net;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Theatre.Utils;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Theatre.Handlers;

namespace Theatre
{
    public class Program
    {
        // TODO: Todo in FrameHandler.cs@37
        // TODO: Add comments to as much as possible, unless someone does it for me, ill do that next push (maybe probably)
        // TODO: Make the code a lot more readable
        // TODO: Rename ModManagerUI to Theatre
        public static Program Instance { get; private set; }
        public const string ModsCacheDirectory = "";

        static void Main(string[] args)
        {
            Instance = new Program();
            if (Directory.Exists(Path.GetTempPath() + "Theatre\\"))
            {
                FileUtils.ReloadDirectory(Path.GetTempPath() + "Theatre\\");
            }

            Instance.Run();
        }
        

        public void Run()
        {
            using var window = Window.Create(WindowOptions.Default);

            List<string> selectedFiles = new List<string>();

            ImGuiController controller = null;
            GL gl = null;
            IInputContext inputContext = null;


            window.Load += () =>
            {
                static void SetupFonts()
                {
                    var minecraftFontPath = "Theatre.Fonts.Monocraft.ttf";
                    var fontPath = "Theatre.Fonts.Mariosans.ttf";
                    
                    GCHandle pinnedArray = GCHandle.Alloc(FileUtils.ExtractResource(fontPath), GCHandleType.Pinned);
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

                try
                {
                    FrameHandler.RunFrame(window, ref selectedFiles,
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx",
                            "mods", "contents", "010028600EBDA000"),
                        "mods");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message+"\n"+ex.StackTrace);
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