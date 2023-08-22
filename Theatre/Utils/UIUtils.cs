using System.Numerics;
using ImGuiNET;
using Silk.NET.Windowing;

namespace Theatre.Utils
{
    internal static class UIUtils
    {
        public static Vector2 CalcTextSize(string text)
        {
            return ImGui.CalcTextSize(text) * 1.2f;
        }

        public static Vector2 CenterCursorWithText(IWindow window, string text)
        {
            return new Vector2((float)window.Size.X / 2 - ImGui.CalcTextSize(text).X / 2, ImGui.GetCursorPosY());
        }

        public static bool Button(string label)
        {
            Vector2 size = CalcTextSize(label);
            return ImGui.Button(label, size);
        }

        public static void Text(IWindow window, string text, bool centered = false)
        {
            if (centered)
                ImGui.SetCursorPos(CenterCursorWithText(window, text));
            ImGui.Text(text);
        }

        public static void TextColoured(IWindow window, Colour colour, string text, bool centered = true)
        {
            if (centered)
                ImGui.SetCursorPos(CenterCursorWithText(window, text));
            try
            {
                ImGui.TextColored(colour.ToVector4(), text);
            }
            catch (AccessViolationException _)
            {
                Console.WriteLine(_.StackTrace);
            }
        }
    }
}
