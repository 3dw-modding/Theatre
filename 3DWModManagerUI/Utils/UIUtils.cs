using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace _3DWModManagerUI.Utils
{
    internal class UIUtils
    {
        public static bool Button(string label)
        {
            Vector2 size = ImGui.CalcTextSize(label) + ImGui.GetStyle().FramePadding * 2.0f;
            return ImGui.Button(label, size);
        }
    }
}
