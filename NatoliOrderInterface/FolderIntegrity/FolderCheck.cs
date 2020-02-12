using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NatoliOrderInterface.Models.NEC;

namespace NatoliOrderInterface.FolderIntegrity
{
    public static class FolderCheck
    {
        public static string FixDirectoryName(string directoryName)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            string correctCustomerFolder = directoryName.Trim();
            foreach (char c in invalid)
            {
                string replacement = "";
                if (c == '/' || c == '\\')
                {
                    replacement = "-";
                }
                correctCustomerFolder = correctCustomerFolder.Replace(c.ToString(), replacement);
            }
            while (correctCustomerFolder.Contains("  "))
            {
                correctCustomerFolder = correctCustomerFolder.Replace("  ", " ");
            }
            while (correctCustomerFolder.Last() == '.' || correctCustomerFolder.Last() == ' ')
            {
                correctCustomerFolder = correctCustomerFolder.Trim('.');
                correctCustomerFolder = correctCustomerFolder.Trim();
            }
            return correctCustomerFolder;
        }
        /// <summary>
        /// Renames customer folders to match the database. If it finds folders that do not match any customer number, it returns them as a list.
        /// </summary>
        /// <returns></returns>
        public static (List<string> foldersWithoutANumber, List<Tuple<string,string>> foldersRenamed) CustomerFolderCheck()
        {
            List<string> foldersWithoutANumber = new List<string>();
            List<Tuple<string, string>> foldersRenamed = new List<Tuple<string, string>>();
            try
            {
                string customersDirectory = @"\\engserver\workstations\tools\Customers";
                string[] customerFolders = Directory.GetDirectories(customersDirectory).Select(Path.GetFileName).ToArray(); // Just the names of the folders, not full paths.
                string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                using var _necContext = new NECContext();
                foreach (string customerFolder in customerFolders)
                {
                    if (customerFolder.Contains(" - "))
                    {
                        string customerNumber = customerFolder.Remove(customerFolder.IndexOf(" - "));
                        string customerName = customerFolder.Substring(customerNumber.Length - 1 + 3);
                        // Folder number matches database
                        if (_necContext.Rm00101.Any(c => c.Custnmbr.Trim() == customerNumber.Trim()))
                        {
                            var customer = _necContext.Rm00101.First(c => c.Custnmbr.Trim() == customerNumber.Trim());
                            string correctCustomerName = customer.Custname.Trim();
                            correctCustomerName = FixDirectoryName(correctCustomerName);
                            // Folder's customer name doesn't match the database's customer name
                            if (correctCustomerName.Trim() != customerName.Trim())
                            {
                                string correctCustomerFolder = customer.Custnmbr.Trim() + " - " + correctCustomerName.Trim();
                                MoveFolders(customersDirectory + "\\" + customerFolder, customersDirectory + "\\" + correctCustomerFolder);
                                foldersRenamed.Add(new Tuple<string, string>(customersDirectory + "\\" + customerFolder, customersDirectory + "\\" + correctCustomerFolder));
                            }
                        }
                        // No customer number match
                        else
                        {
                            foldersWithoutANumber.Add(customersDirectory + "\\" + customerFolder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return (foldersWithoutANumber, foldersRenamed);
        }
        /// <summary>
        /// Moves folders and contents of folders to new folder. Moves files if folder is duplicate. Renames duplicate files.
        /// </summary>
        /// <param name="directoryFrom"></param>
        /// <param name="directoryTo"></param>
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
        /// <summary>
        /// Moves files and renames duplicates [filename] + "_OLD" + [.ext].
        /// </summary>
        /// <param name="fileFrom"></param>
        /// <param name="fileTo"></param>
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
