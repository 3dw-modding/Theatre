using NativeFileDialogExtendedSharp;

namespace Theatre.Utils
{
    public static class NfdFilterPresets
    {
        public static readonly NfdFilter ImageFiles = new() 
        { Description = "Image files", Specification = "png,jpg,jpeg,bmp" };

        public static readonly NfdFilter ZipFiles = new() 
        { Description = "Zip Files", Specification = "zip,7z,rar" };
    }
}
