using System.IO.Compression;

namespace KNX_Virtual_Integrator.Model.Interfaces;

/// <summary>
/// Defines methods for creating and managing ZIP archives.
/// </summary>
public interface IZipArchiveManager
{
    /// <summary>
    /// Creates a ZIP archive at the specified path, adding files and/or directories to it.
    /// If the specified path is a directory, all files and subdirectories within it are included in the archive.
    /// If the path is a file, only that file is added to the archive.
    /// If the ZIP file already exists, it will be overwritten.
    /// </summary>
    /// <param name="zipFilePath">The path where the ZIP archive will be created.</param>
    /// <param name="paths">An array of file and/or directory paths to include in the archive.</param>
    void CreateZipArchive(string zipFilePath, params string[] paths);

    /// <summary>
    /// Recursively adds all files and subdirectories from the specified directory to the ZIP archive.
    /// Only the contents of the directory are included in the archive, not the directory itself.
    /// </summary>
    /// <param name="archive">The ZIP archive to which files and subdirectories will be added.</param>
    /// <param name="directoryPath">The path of the directory whose contents will be added to the archive.</param>
    /// <param name="entryName">The relative path within the ZIP archive where the contents of the directory will be placed.</param>
    void AddDirectoryToArchive(ZipArchive archive, string directoryPath, string entryName);
}