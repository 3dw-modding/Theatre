using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Theatre.Handlers;
using Theatre.Utils;

namespace Theatre
{
    // TODO: THIS IS A DEV PUSH, DO NOT USE THIS, ITS REALLY FUCKING BAD, IT DOESNT EVEN WORK, THIS IS ONLY FOR LORD TO HELP FIX MY PROBLEMS

    // TODO: Add comments to as much as possible, unless someone does it for me, ill do that next push (maybe probably)
    // TODO: Make the code a lot more readable

    public static class Program 
    {
        static void Main()
        {
            // Why the everliving fuck is this here??? like why the fuck are we not just putting the contents of Run into Main????
            // WHO THE FUCK DID THIS!!!!
            Run();
        }

        private static void Run()
        {
            using var window = Window.Create(WindowOptions.Default);

            List<string> selectedFiles = new();

            ImGuiController? controller = null;
            GL? gl = null;
            IInputContext? inputContext = null;


            window.Load += () =>
            {
                static void SetupFonts()
                {
                    var minecraftFontPath = "Theatre.Fonts.Monocraft.ttf";
                    var fontPath = "Fonts\\Mariosans.ttf";

                    //GCHandle pinnedArray = GCHandle.Alloc(FileUtils.ExtractResource(fontPath), GCHandleType.Pinned);
                    //IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                    //ImGui.GetIO().Fonts.AddFontFromMemoryTTF(pointer, 18, 32);
                    ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 32);

                    //pinnedArray.Free();
                }

                gl = window.CreateOpenGL();
                inputContext = window.CreateInput();

                controller = new ImGuiController(gl, window, inputContext, SetupFonts);

                selectedFiles = new List<string>();

                window.Center();
            };


            window.FramebufferResize += s =>
            {
                gl?.Viewport(s);
            };
            
            window.Render += delta =>
            {
                controller?.Update((float)delta);
                
                gl?.ClearColor(.45f, .55f, .60f, 1f);
                gl?.Clear((uint)ClearBufferMask.ColorBufferBit);

                ImGui.SetNextWindowPos(new Vector2(0, 0));
                ImGui.SetNextWindowSize(new Vector2(window.Size.X, window.Size.Y));
                ImGui.Begin("MainWindow", ImGuiWindowFlags.NoDecoration);

                FrameHandler.RunFrame(window, ref selectedFiles, 
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx", "mods", "contents", "010028600EBDA000"),
                    "mods");


                ImGui.End();

                controller?.Render();

            };
            
            window.Closing += () =>
            {
                controller?.Dispose();
                inputContext?.Dispose();
                gl?.Dispose();
            };
            
            window.Run();

            window.Dispose();
        }
    }

    
}