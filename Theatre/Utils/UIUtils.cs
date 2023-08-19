using System.Numerics;
using ImGuiNET;
using Silk.NET.Windowing;

namespace Theatre.Utils
{
    internal static class UIUtils
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

        public static void TextColoured(Colour colour, string text)
        {
            ImGui.TextColored(colour.ToVector4(), text);
        }
    }
}
