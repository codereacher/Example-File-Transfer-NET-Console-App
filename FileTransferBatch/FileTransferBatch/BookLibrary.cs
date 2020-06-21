using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace FileTransferBatch
{
    public class BookLibrary
    {
        private static readonly string encyclopediaBookSourceFolder = ConfigurationManager.AppSettings["EncyclopediaBookSourceFolder"];
        private static readonly string encyclopediaBookDownloadFolder = ConfigurationManager.AppSettings["EncyclopediaBookDownloadFolder"];
        private static readonly string encyclopediaBookDestinationFolder = ConfigurationManager.AppSettings["EncyclopediaBookDestinationFolder"];

        public static void TransferEncyclopediaDataToBookstore()
        {
            try
            {
                LogWriteLine("Starting process: Encyclopedia Data Transfer to Bookstore");
                string emailBody = "Start time: " + DateTime.Now + Environment.NewLine + Environment.NewLine;

                List<string> downloadedFiles = GetEncyclopediaFiles();

                if (downloadedFiles.Count() > 0)
                {
                    LogWriteLine("Going to delete Encyclopedia files from Bookstore SFT since they are downloaded");
                    DeleteFromBookLibrarySft(encyclopediaBookSourceFolder, downloadedFiles);

                    List<string> uploadedFiles = SendEncyclopediaFilesToBookstore();

                    if (uploadedFiles.Count() > 0)
                    {
                        emailBody += "Uploaded Encyclopedia files for Bookstore at " + encyclopediaBookDestinationFolder +
                            Environment.NewLine + Environment.NewLine;

                        foreach (string item in uploadedFiles)
                            emailBody += item + Environment.NewLine + Environment.NewLine;

                        // Archive the downloaded files
                        Utilities.ArchiveFiles(encyclopediaBookDownloadFolder);

                        emailBody += "End time: " + DateTime.Now;
                        Utilities.EmailMessage("Encyclopedia Data for Bookstore", emailBody.TrimEnd());
                    }
                }

                LogWriteLine("End of process: Encyclopedia Data Transfer to Bookstore");
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on Encyclopedia Data Transfer to Bookstore: " + ex);
            }
        }

        public static List<string> GetEncyclopediaFiles()
        {
            List<string> downloadedFiles = new List<string>();

            try
            {
                LogWriteLine("Going to get Encyclopedia files");

                // Check for folder and create if doesn't exist
                Utilities.CreateDirectory(encyclopediaBookDownloadFolder);

                // Download Encyclopedia files from Book Library SFT location
                downloadedFiles = DownloadFromBookLibrarySft(encyclopediaBookSourceFolder, encyclopediaBookDownloadFolder, GetEncyclopediaFileNames());
                LogWriteLine(downloadedFiles.Count() + " files downloaded");
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on getting Encyclopedia files: " + ex.Message);
                throw ex;
            }

            return downloadedFiles;
        }

        private static List<string> GetEncyclopediaFileNames()
        {
            return new List<string>()
            {
                "BlueEncyclopedia.csv",
                "GreenEncyclopedia.csv",
                "OrangeEncyclopedia.csv",
                "SilverEncyclopedia.csv",
                "GoldEncyclopedia.csv"
            };
        }

        private static List<string> DownloadFromBookLibrarySft(string sourceFolderPath, string destinationFolderPath, List<string> sourceFileNames = null)
        {
            SecureFileTransfer sftInstance = new SecureFileTransfer(
                ConfigurationManager.AppSettings["SftBookLibraryUsername"], ConfigurationManager.AppSettings["SftBookLibraryPassword"]);

            return sftInstance.BookLibrarySftpDownload(sourceFolderPath, destinationFolderPath, sourceFileNames);
        }

        private static List<string> DeleteFromBookLibrarySft(string sourceFolderPath, List<string> sourceFileNames = null)
        {
            SecureFileTransfer sftInstance = new SecureFileTransfer(
                ConfigurationManager.AppSettings["SftBookLibraryUsername"], ConfigurationManager.AppSettings["SftBookLibraryPassword"]);

            return sftInstance.BookLibrarySftpDelete(sourceFolderPath, sourceFileNames);
        }

        private static List<string> SendEncyclopediaFilesToBookstore()
        {
            List<string> uploadedFiles = new List<string>();

            try
            {
                LogWriteLine("Going to send Encyclopedia files to Bookstore");
                uploadedFiles = SendToBookstore(encyclopediaBookDownloadFolder, encyclopediaBookDestinationFolder);
                LogWriteLine(uploadedFiles.Count() + " files uploaded");
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on sending Encyclopedia files to Bookstore: " + ex.Message);
                throw ex;
            }

            return uploadedFiles;
        }

        private static List<string> SendToBookstore(string sourceFolderPath, string destinationFolderPath)
        {
            SecureFileTransfer sftInstance = new SecureFileTransfer(
                ConfigurationManager.AppSettings["SftBookstoreUsername"], ConfigurationManager.AppSettings["SftBookstorePassword"]);

            return sftInstance.BookstoreSftpUpload(sourceFolderPath, destinationFolderPath);
        }

        private static void LogWriteLine(string logMessage)
        {
            Utilities.LogWriteLine(logMessage);
        }
    }
}
