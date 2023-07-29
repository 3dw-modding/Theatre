using System.Numerics;
using ImGuiNET;
using Silk.NET.Windowing;
using Window = Silk.NET.SDL.Window;

namespace _3DWModManagerUI.Utils
{
    internal class UIUtils
    {
        public static Vector2 CenterCursorWithText(IWindow window, string text)
        {
            return new Vector2(window.Size.X / 2 - ImGui.CalcTextSize(text).X / 2,0);
        }

        public static bool Button(string label)
        {
            Vector2 size = ImGui.CalcTextSize(label) + ImGui.GetStyle().FramePadding * 2.0f;
            return ImGui.Button(label, size);
        }

        public static void TextColoured(Vector4 colour, string text)
        {
            
            ImGui.TextColored(colour, text);
        }
    }
}
