using System.Net;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Theatre.Handlers;
using Theatre.Utils;

namespace Theatre
{
    // TODO: Add comments to as much as possible, unless someone does it for me, ill do that next push (maybe probably)

    public static class Program 
    {
        static void Main()
        {
            string cacheDirectory = Path.Join(
                new FileInfo(Environment.ProcessPath!).Directory!.FullName,
                "cache");
            string databaseFolderUrl = "https://github.com/Scyye/Theatre/raw/main/Databases/";

            Directory.CreateDirectory(cacheDirectory);

            using var client = new HttpClient();


            if (!File.Exists(Path.Join(cacheDirectory, "switch.bin")))
                File.WriteAllBytes(Path.Join(cacheDirectory, "switch.bin"), 
                    client.GetByteArrayAsync(databaseFolderUrl + "switch.bin").GetAwaiter().GetResult());


            if (!File.Exists(Path.Join(cacheDirectory, "wiiu.bin")))
                File.WriteAllBytes(Path.Join(cacheDirectory, "wiiu.bin"),
                    client.GetByteArrayAsync(databaseFolderUrl + "wiiu.bin").GetAwaiter().GetResult());


            CachedGBMods.AllWiiUModsCached = FileUtils.FromBytes(File.ReadAllBytes(Path.Join(cacheDirectory, "wiiu.bin")));
            CachedGBMods.AllSwitchModsCached = FileUtils.FromBytes(File.ReadAllBytes(Path.Join(cacheDirectory, "switch.bin")));

            /*
            foreach (var tuple in CachedGBMods.AllSwitchModsCached)
                Console.WriteLine("Switch:  " + tuple.Value.Item1);

            foreach (var tuple in CachedGBMods.AllWiiUModsCached)
                Console.WriteLine("WiiU:  " + tuple.Value.Item1);
            */


            using var window = Window.Create(WindowOptions.Default);

            List<string> selectedFiles = new();

            ImGuiController? controller = null;
            GL? gl = null;
            IInputContext? inputContext = null;


            window.Load += () =>
            {
                gl = window.CreateOpenGL();
                inputContext = window.CreateInput();

                controller = new ImGuiController(gl, window, inputContext,
                    () =>
                    {
                        ImGui.GetIO().Fonts.AddFontFromFileTTF("Fonts\\Mariosans.ttf", 32);
                    });

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
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx", "mods", "contents", "010028600EBDA000"));


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