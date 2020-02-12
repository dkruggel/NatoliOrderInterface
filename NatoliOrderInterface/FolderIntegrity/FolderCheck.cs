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
        /// <summary>
        /// Renames customer folders to match the database. If it finds folders that do not match any customer number, it returns them as a list.
        /// </summary>
        /// <returns></returns>
        public static List<string> MoveCustomerFolders()
        {
            List<string> foldersWithoutANumber = new List<string>();
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
                            foreach (char c in invalid)
                            {
                                correctCustomerName = correctCustomerName.Replace(c.ToString(), "");
                            }
                            while (correctCustomerName.Last() == '.' || correctCustomerName.Last() == ' ')
                            {
                                correctCustomerName = correctCustomerName.Trim('.');
                                correctCustomerName = correctCustomerName.Trim();
                            }
                            // Folder's customer name doesn't match the database's customer name
                            if (_necContext.Rm00101.Any(c => c.Custnmbr.Trim() == customerNumber.Trim() && !string.IsNullOrWhiteSpace(c.Custname) && !string.IsNullOrEmpty(c.Custname) && c.Custname.Trim() != customerName.Trim()))
                            {
                                string correctCustomerFolder = customer.Custnmbr.Trim() + " - " + customer.Custname.Trim();
                                MoveFolders(customersDirectory + "\\" + customerFolder, customersDirectory + "\\" + correctCustomerFolder);
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
            return foldersWithoutANumber;
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
