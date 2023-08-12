using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeFileDialogExtendedSharp;

namespace _3DWModManagerUI.Utils
{
    public static class NfdFilterPresets
    {
        public readonly static NfdFilter ImageFiles = new() 
        { Description = "Image files", Specification = "png,jpg,jpeg,bmp" };

        public readonly static NfdFilter ZipFiles = new() 
        { Description = "Zip Files", Specification = "zip,7z" };
    }
}
