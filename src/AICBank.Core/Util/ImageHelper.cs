using System;
using System.IO;

namespace AICBank.Core.Util;

public static class ImageHelper
{
    public static async Task<string> GetBase64LogoImage()
    {
        var filename = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo.png");
        var fileContents = await File.ReadAllBytesAsync(filename);

        return Convert.ToBase64String(fileContents);
    }
}