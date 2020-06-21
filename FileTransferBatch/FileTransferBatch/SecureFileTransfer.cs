using Chilkat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace FileTransferBatch
{
    public class SecureFileTransfer
    {
        private readonly string _username;
        private readonly string _password;
        private string _hostName;
        private int _port;

        public SecureFileTransfer(string username, string password)
        {
            _username = username;
            _password = password;
        }

        // Download from Book Library server
        public List<string> BookLibrarySftpDownload(string sourceFolderPath, string destinationFolderPath, List<string> sourceFileNames = null)
        {
            List<string> downloadedFiles = new List<string>();

            try
            {
                LogWriteLine("Starting Book Library SFTP download");
                SetBookLibrarySftpInfo();
                downloadedFiles = SftpDownload(sourceFolderPath, destinationFolderPath, sourceFileNames);
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on Book Library SFTP download - " + ex.Message);
                throw ex;
            }

            return downloadedFiles;
        }

        public List<string> BookLibrarySftpUpload(string sourceFolderPath, string destinationFolderPath)
        {
            List<string> uploadedFiles = new List<string>();

            try
            {
                LogWriteLine("Starting Book Library SFTP upload");
                SetBookLibrarySftpInfo();
                uploadedFiles = SftpUpload(sourceFolderPath, destinationFolderPath);
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on Book Library SFTP upload - " + ex.Message);
                throw ex;
            }

            return uploadedFiles;
        }

        public List<string> BookLibrarySftpDelete(string sourceFolderPath, List<string> sourceFileNames = null)
        {
            List<string> deletedFiles = new List<string>();

            try
            {
                LogWriteLine("Starting Book Library SFTP delete");
                SetBookLibrarySftpInfo();
                deletedFiles = SftpDelete(sourceFolderPath, sourceFileNames);
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on Book Library SFTP delete - " + ex.Message);
                throw ex;
            }

            return deletedFiles;
        }

        // Download from Bookstore server
        public List<string> BookstoreSftpDownload(string sourceFolderPath, string destinationFolderPath, List<string> sourceFileNames = null)
        {
            List<string> downloadedFiles = new List<string>();

            try
            {
                LogWriteLine("Starting Bookstore SFTP download");
                SetBookstoreSftpInfo();
                downloadedFiles = SftpDownload(sourceFolderPath, destinationFolderPath, sourceFileNames);
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on Bookstore SFTP download - " + ex.Message);
                throw ex;
            }

            return downloadedFiles;
        }

        public List<string> BookstoreSftpUpload(string sourceFolderPath, string destinationFolderPath)
        {
            List<string> uploadedFiles = new List<string>();

            try
            {
                LogWriteLine("Starting Bookstore SFTP upload");
                SetBookstoreSftpInfo();
                uploadedFiles = SftpUpload(sourceFolderPath, destinationFolderPath);
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on Bookstore SFTP upload - " + ex.Message);
                throw ex;
            }

            return uploadedFiles;
        }

        public List<string> BookstoreSftpDelete(string sourceFolderPath, List<string> sourceFileNames = null)
        {
            List<string> deletedFiles = new List<string>();

            try
            {
                LogWriteLine("Starting Bookstore SFTP delete");
                SetBookstoreSftpInfo();
                deletedFiles = SftpDelete(sourceFolderPath, sourceFileNames);
            }
            catch (Exception ex)
            {
                LogWriteLine("Error on Bookstore SFTP delete - " + ex.Message);
                throw ex;
            }

            return deletedFiles;
        }

        private void SetBookLibrarySftpInfo()
        {
            _hostName = ConfigurationManager.AppSettings["SftBookLibraryHostName"];
            _port = Convert.ToInt32(ConfigurationManager.AppSettings["SftBookLibraryPort"]);
        }

        private void SetBookstoreSftpInfo()
        {
            _hostName = ConfigurationManager.AppSettings["SftBookstoreHostName"];
            _port = Convert.ToInt32(ConfigurationManager.AppSettings["SftBookstorePort"]);
        }

        private List<string> SftpDownload(string sourceFolderPath, string destinationFolderPath, List<string> sourceFileNames = null)
        {
            SFtp sftpInstance = OpenSftpConnection(_hostName, _port);
            InitializeSftp(sftpInstance);

            return DownloadFiles(sftpInstance, sourceFolderPath, destinationFolderPath, sourceFileNames);
        }

        private List<string> SftpUpload(string sourceFolderPath, string destinationFolderPath)
        {
            SFtp sftpInstance = OpenSftpConnection(_hostName, _port);
            InitializeSftp(sftpInstance);

            return UploadFiles(sftpInstance, sourceFolderPath, destinationFolderPath);
        }

        private List<string> SftpDelete(string sourceFolderPath, List<string> sourceFileNames = null)
        {
            SFtp sftpInstance = OpenSftpConnection(_hostName, _port);
            InitializeSftp(sftpInstance);

            return DeleteFiles(sftpInstance, sourceFolderPath, sourceFileNames);
        }

        private SFtp OpenSftpConnection(string hostName, int port)
        {
            // Uses 3rd party tool by Chilkat for SFTP
            SFtp sftpInstance = new SFtp();

            bool result = Convert.ToBoolean(sftpInstance.UnlockComponent("AddLicenseKeyHere"));

            if (!result)
                throw new Exception(sftpInstance.LastErrorText);

            // Set some timeouts, in milliseconds: (15000 mS = 15 seconds)
            sftpInstance.ConnectTimeoutMs = 15000;
            sftpInstance.IdleTimeoutMs = 15000;

            // Connect to the server
            LogWriteLine("Connecting to server - host name: " + hostName);
            result = Convert.ToBoolean(sftpInstance.Connect(hostName, port));

            // No connection error message popup
            if (!result)
                throw new Exception(sftpInstance.LastErrorText);

            // Authenticate with the server using password authentication
            LogWriteLine("Authenticating with server");
            result = Convert.ToBoolean(sftpInstance.AuthenticatePw(_username, _password));

            if (result)
                LogWriteLine("Authentication successful");
            else
                throw new Exception(sftpInstance.LastErrorText);

            return sftpInstance;
        }

        private void InitializeSftp(SFtp sftpInstance)
        {
            LogWriteLine("Initializing subsystem");
            if (!sftpInstance.InitializeSftp()) throw new Exception(sftpInstance.LastErrorText);
        }

        private SFtpDir GetDirectoryListing(SFtp sftpInstance, string sourceFolderPath)
        {
            // Open a directory on the server...
            // Paths starting with a slash are "absolute", and are relative
            // to the root of the file system. Names starting with any other
            // character are relative to the user's default directory(home directory).
            // A path component of ".." refers to the parent directory,
            // and "." refers to the current directory.
            LogWriteLine("Opening directory '" + sourceFolderPath + "'");
            string handle = sftpInstance.OpenDir(sourceFolderPath);

            if (string.IsNullOrWhiteSpace(handle))
                throw new Exception(sftpInstance.LastErrorText);

            LogWriteLine("Getting directory listing");
            SFtpDir dirListing = sftpInstance.ReadDir(handle);

            if (dirListing == null)
                throw new Exception(sftpInstance.LastErrorText);

            if (dirListing.NumFilesAndDirs == 0)
                sftpInstance.CloseHandle(handle);

            return dirListing;
        }

        private List<string> DownloadFiles(SFtp sftpInstance, string sourceFolderPath, string destinationFolderPath, List<string> sourceFileNames = null)
        {
            List<string> downloadedFiles = new List<string>();

            SFtpDir dirListing = GetDirectoryListing(sftpInstance, sourceFolderPath);

            if (dirListing.NumFilesAndDirs > 0)
            {
                LogWriteLine("Going to loop through files to download");

                for (int i = 0; i < dirListing.NumFilesAndDirs; i++)
                {
                    SFtpFile fileObj = dirListing.GetFileObject(i);

                    // Skip the file if it wasn't specified in the list of source file names
                    if (sourceFileNames != null && sourceFileNames.Count() > 0)
                    {
                        if (!sourceFileNames.Contains(fileObj.Filename)) continue;
                    }

                    LogWriteLine("Found file '" + fileObj.Filename + "' - Last Modified Time " + fileObj.LastModifiedTime);

                    string sourceFilePath = sourceFolderPath + "/" + fileObj.Filename;
                    string destinationFilePath = destinationFolderPath + fileObj.Filename;

                    bool result = Convert.ToBoolean(sftpInstance.DownloadFileByName(sourceFilePath, destinationFilePath));

                    if (result)
                    {
                        LogWriteLine("Downloaded file '" + fileObj.Filename + "'");
                        downloadedFiles.Add(fileObj.Filename);
                    }
                    else
                    {
                        throw new Exception(sftpInstance.LastErrorText);
                    }
                }
            }
            else
            {
                LogWriteLine("No files found");
            }

            return downloadedFiles;
        }

        private List<string> UploadFiles(SFtp sftpInstance, string sourceFolderPath, string destinationFolderPath)
        {
            List<string> uploadedFiles = new List<string>();

            LogWriteLine("Going to loop through files to upload");

            foreach (string item in Directory.GetFiles(sourceFolderPath))
            {
                FileInfo currentFile = new FileInfo(item);
                string destinationFilePath = destinationFolderPath + "/" + currentFile.Name;

                // Upload from the local file to the SSH server.
                // Important -- the remote filepath is the 1st argument,
                // the local filepath is the 2nd argument;
                bool result = sftpInstance.UploadFileByName(destinationFilePath, item);

                if (result)
                {
                    LogWriteLine("Uploaded file '" + currentFile.Name + "' - Last Modified Time " + currentFile.LastWriteTime);
                    uploadedFiles.Add(currentFile.Name);
                }
                else
                {
                    throw new Exception(sftpInstance.LastErrorText);
                }
            }

            return uploadedFiles;
        }

        private List<string> DeleteFiles(SFtp sftpInstance, string sourceFolderPath, List<string> sourceFileNames = null)
        {
            List<string> deletedFiles = new List<string>();

            SFtpDir dirListing = GetDirectoryListing(sftpInstance, sourceFolderPath);

            if (dirListing.NumFilesAndDirs > 0)
            {
                LogWriteLine("Going to loop through files to delete");

                for (int i = 0; i < dirListing.NumFilesAndDirs; i++)
                {
                    SFtpFile fileObj = dirListing.GetFileObject(i);

                    // Skip the file if it wasn't specified in the list of source file names
                    if (sourceFileNames != null && sourceFileNames.Count() > 0)
                    {
                        if (!sourceFileNames.Contains(fileObj.Filename)) continue;
                    }

                    LogWriteLine("Found file '" + fileObj.Filename + "' - Last Modified Time " + fileObj.LastModifiedTime);

                    bool result = sftpInstance.RemoveFile(sourceFolderPath + "/" + fileObj.Filename);

                    if (result)
                    {
                        LogWriteLine("Deleted file '" + fileObj.Filename + "'");
                        deletedFiles.Add(fileObj.Filename);
                    }
                    else
                    {
                        throw new Exception(sftpInstance.LastErrorText);
                    }
                }
            }
            else
            {
                LogWriteLine("No files found");
            }

            return deletedFiles;
        }

        private void LogWriteLine(string logMessage)
        {
            Utilities.LogWriteLine(logMessage);
        }
    }
}
