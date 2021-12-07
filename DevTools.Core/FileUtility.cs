namespace DevTools.Core;

public static class FileUtility
{
    public static void EnsureFolderExists(string parameterName, string folder)
    {
        if (!Directory.Exists(folder)) throw new DirectoryNotFoundException(parameterName + ": " +  folder);
    }
}