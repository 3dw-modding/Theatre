using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeFileDialogExtendedSharp;

namespace _3DWModManagerUI.Utils
{
    public class NfdFilterPresets
    {
        public static NfdFilter ImageFiles = new NfdFilter
            { Description = "Image files", Specification = "png,jpg,jpeg,bmp" };


    }
}
