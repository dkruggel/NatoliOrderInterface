using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NatoliOrderInterface.FolderIntegrity
{
    public static class FolderCheck
    {
        public static List<string> GetDuplicateFolders()
        {
            return null;
        }
        public static void MoveFolders(string directoryFrom, string directoryTo)
        {
            try
            {
                if (Directory.Exists(directoryTo))
                {
                    var files = Directory.GetFiles(directoryFrom);
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        MoveFiles(file, directoryTo + "\\" + fileName);
                    }
                    var directories = Directory.GetDirectories(directoryFrom);
                    foreach (string directory in directories)
                    {
                        string directoryName = new DirectoryInfo(directory).Name;
                        // Call MoveFolders() again to repeat the process.
                        MoveFolders(directory, directoryTo + "\\" + directoryName);
                    }
                    Directory.Delete(directoryFrom);
                }
                else
                {
                    Directory.Move(directoryFrom, directoryTo);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public static void MoveFiles(string fileFrom, string fileTo)
        {
            try
            {
                string directoryTo = Path.GetDirectoryName(fileTo);
                if (File.Exists(fileTo))
                {
                    string fileToExt = Path.GetExtension(fileTo);
                    string fileToNameOnly = Path.GetFileNameWithoutExtension(fileTo);
                    // Call MoveFiles() again to repeat the process (could be a duplicate).
                    MoveFiles(fileFrom, directoryTo + "\\" + fileToNameOnly + "_OLD" + fileToExt);
                }
                else
                {
                    File.Move(fileFrom, fileTo);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
