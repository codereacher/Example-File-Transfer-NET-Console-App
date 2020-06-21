using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;

namespace FileTransferBatch
{
    public static class Utilities
    {
        public static readonly string env = ConfigurationManager.AppSettings["Environment"];
        private static readonly string logFolderPath = ConfigurationManager.AppSettings["LogFolder"];
        private static readonly string logFilePath = logFolderPath + "FileTransferBatchLog" + DateTime.Today.ToString("yyyyMMdd") + ".txt";

        public static void LogWriteLine(string logMessage)
        {
            string logDateTime = DateTime.Today.ToString("MM/dd/yyyy") + " " + DateTime.Now.ToString("HH:mm:ss.fff");

            try
            {
                // Check for log folder and create if doesn't exist
                CreateDirectory(logFolderPath);

                // Write log message to console and log file
                Console.WriteLine(logDateTime + ": " + logMessage);
                File.AppendAllText(logFilePath, logDateTime + ": " + logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(logDateTime + ": " + ex.Message);
            }
        }

        // Check for folder and create if it doesn't exist
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        public static void EmailMessage(string subject, string body)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["SmtpServer"]))
                {
                    string from = ConfigurationManager.AppSettings["MailFromAddress"];
                    string to = ConfigurationManager.AppSettings["MailToAddress"];
                    string fullSubject = ConfigurationManager.AppSettings["MailSubject"];

                    fullSubject += string.IsNullOrWhiteSpace(subject) ? string.Empty : " - " + subject;

                    client.Send(from, to, fullSubject, body);
                }
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on sending email: " + ex.Message);
                throw ex;
            }
        }

        public static void ArchiveFiles(string sourceFolderPath)
        {
            LogWriteLine("Starting archive process");

            BackUpFiles(sourceFolderPath);
            DeleteFiles(sourceFolderPath);

            LogWriteLine("Finished archive process");
        }

        public static void BackUpFiles(string sourceFolderPath)
        {
            try
            {
                string backupFolder = ConfigurationManager.AppSettings["BackupFolder"] + Path.GetFileName(Path.GetDirectoryName(sourceFolderPath));
                string destinationFolderPath = backupFolder + "\\" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Create folder if doesn't exist
                CreateDirectory(destinationFolderPath);

                LogWriteLine("Going to loop through files to back up from " + sourceFolderPath);

                // Loop through the files and back them up
                foreach (string sourceFile in Directory.GetFiles(sourceFolderPath))
                {
                    FileInfo currentFile = new FileInfo(sourceFile);
                    string destinationFilePath = destinationFolderPath + "\\" + currentFile.Name;

                    File.Copy(sourceFile, destinationFilePath);

                    if (File.Exists(destinationFilePath))
                        LogWriteLine("Backed up file " + currentFile.Name + " to " + destinationFilePath);
                    else
                        throw new Exception("Could not back up file " + currentFile.Name);
                }
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on backing up files: " + ex.Message);
                throw ex;
            }
        }

        private static void DeleteFiles(string sourceFolderPath)
        {
            try
            {
                LogWriteLine("Going to loop through files to delete from " + sourceFolderPath);

                // Loop through the files and delete them
                foreach (string sourceFile in Directory.GetFiles(sourceFolderPath))
                {
                    FileInfo currentFile = new FileInfo(sourceFile);
                    File.Delete(sourceFile);

                    if (!File.Exists(sourceFile))
                        LogWriteLine("Deleted file " + currentFile.Name);
                    else
                        throw new Exception("Could not delete file " + currentFile.Name);
                }
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on deleting files: " + ex.Message);
                throw ex;
            }
        }
    }
}
