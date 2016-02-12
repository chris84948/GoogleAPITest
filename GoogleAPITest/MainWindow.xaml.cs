using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GoogleAPITest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// This permission only allows viewing of files I have created or opened with this app
        /// </summary>
        private string[] Scopes = { DriveService.Scope.DriveFile };
        private string AppName = "GoogleAPITest";
        private UserCredential credentials;
        private DriveService service;
        
        public MainWindow()
        {
            InitializeComponent();

            string credPath = AppDomain.CurrentDomain.BaseDirectory + AppName;

            credentials = GetCredentials(credPath);

            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = AppName,
            });
        }
                
        private UserCredential GetCredentials(string credPath)
        {
            try
            {
                UserCredential userCredential;
                using (var stream = new System.IO.FileStream("client_secret.json",
                                                             System.IO.FileMode.Open,
                                                             System.IO.FileAccess.Read))
                {
                    userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(stream,
                                                                                 Scopes,
                                                                                 "user",
                                                                                 CancellationToken.None,
                                                                                 new FileDataStore(credPath, true)).Result;
                }

                if (userCredential.IsExpired())
                {
                    bool b = userCredential.RefreshTokenAsync(CancellationToken.None).Result;
                }

                return userCredential;
            }
            catch (AggregateException ae)
            {
                var googleEx = ae.InnerException as TokenResponseException;
                System.Diagnostics.Debug.WriteLine("Message: " + googleEx.Message);
            }

            return null;
        }

        public static IList<File> GetFiles(DriveService service, string search)
        {
            try
            {
                //List all of the files and directories for the current user.  
                // Documentation: https://developers.google.com/drive/v2/reference/files/list
                FilesResource.ListRequest list = service.Files.List();

                FilesResource.ListRequest request = service.Files.List();
                FileList files = request.Execute();

                return files.Files;
            }
            catch (Exception ex)
            {
                // In the event there is an error with the request.
                Console.WriteLine(ex.Message);
            }
            return new List<File>();
        }

        /// <summary>
        /// Create a new Directory.
        /// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        /// </summary>
        /// <param name="service">a Valid authenticated DriveService</param>
        /// <param name="title">The title of the file. Used to identify file or folder name.</param>
        /// <param name="description">A short description of the file.</param>
        /// <param name="parent"></param>
        /// <returns>
        /// Collection of parent folders which contain this file. 
        /// Setting this field will put the file in all of the provided folders. root folder.
        /// </returns>
        public static File CreateDirectory(DriveService service, string title, string description, string parent = "root")
        {
            File NewDirectory = null;

            // Create metaData for a new Directory
            File body = new File();
            body.Name = title;
            body.Description = description;
            body.MimeType = "application/vnd.google-apps.folder";
            body.Parents = new List<string>() { parent };

            try
            {
                FilesResource.CreateRequest request = service.Files.Create(body);
                NewDirectory = request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return NewDirectory;
        }

        /// <summary>
        /// Uploads a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        /// </summary>
        /// <param name="service">a Valid authenticated DriveService</param>
        /// <param name="uploadFile">path to the file to upload</param>
        /// <param name="parent">
        /// Collection of parent folders which contain this file. 
        /// Setting this field will put the file in all of the provided folders. root folder.
        /// </param>
        /// <returns>
        /// If upload succeeded returns the File resource of the uploaded file 
        /// If the upload fails returns null
        /// </returns>
        public static File UploadFile(DriveService service, string uploadFile, string content, string parent)
        {
            File body = new File();
            body.Name = System.IO.Path.GetFileName(uploadFile);
            body.Description = "Test File 1";
            body.Parents = new List<string>() { parent };

            // File's content.
            var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(content ?? ""));
            try
            {
                FilesResource.CreateMediaUpload request = service.Files.Create(body, stream, String.Empty);
                request.Upload();
                return request.ResponseBody;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Updates a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/update
        /// </summary>
        /// <param name="service">a Valid authenticated DriveService</param>
        /// <param name="uploadFile">path to the file to upload</param>
        /// <param name="parent">
        /// Collection of parent folders which contain this file. 
        /// Setting this field will put the file in all of the provided folders. root folder.
        /// </param>
        /// <param name="fileId">the resource id for the file we would like to update</param>
        /// <returns>
        /// If upload succeeded returns the File resource of the uploaded file 
        /// If the upload fails returns null
        /// </returns>
        public static File UpdateFile(DriveService service, string uploadFile, string parent, string fileId)
        {
            File body = new File();
            body.Name = System.IO.Path.GetFileName(uploadFile);
            body.Parents = new List<string>() { parent };

            // File's content.
            byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
            try
            {
                FilesResource.UpdateMediaUpload request = service.Files.Update(body, fileId, stream, String.Empty);
                request.Upload();
                return request.ResponseBody;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }

        public static File TrashFile(DriveService service, string uploadFile,  string fileId)
        {
            File body = new File();
            body.Trashed = true;

            // File's content.
            byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
            try
            {
                FilesResource.UpdateRequest request = service.Files.Update(body, fileId);
                return request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }

        public static File UntrashFile(DriveService service, string uploadFile, string fileId)
        {
            File body = new File();
            body.Trashed = false;

            // File's content.
            byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
            try
            {
                FilesResource.UpdateRequest request = service.Files.Update(body, fileId);
                return request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Download a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/get
        /// </summary>
        /// <param name="service">a Valid authenticated DriveService</param>
        /// <param name="fileResource">File resource of the file to download</param>
        /// <param name="saveTo">location of where to save the file including the file name to save it as.</param>
        /// <returns></returns>
        public void DownloadFile(DriveService service, File fileResource)
        {
            var fileId = fileResource.Id;
            var request = service.Files.Get(fileId);
            var stream = new System.IO.MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged +=
                (IDownloadProgress progress) =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            {
                                Console.WriteLine(progress.BytesDownloaded);
                                break;
                            }
                        case DownloadStatus.Completed:
                            {
                                try
                                {
                                    Console.WriteLine("Download complete.");
                                    stream.Position = 0;
                                    var sr = new System.IO.StreamReader(stream);
                                    Dispatcher.BeginInvoke((Action) (() =>
                                    {
                                        TextBox.Text = sr.ReadToEnd();
                                    }));
                                }
                                catch (Exception ex)
                                {

                                }
                                break;
                            }
                        case DownloadStatus.Failed:
                            {
                                Console.WriteLine("Download failed.");
                                break;
                            }
                    }
                };
            request.Download(stream);
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            var directory = CreateDirectory(service, "DriveAPITest", "Drive API Test Application");
            var file = UploadFile(service, "File1.txt", TextBox.Text, directory.Id);

            var allFiles = GetFiles(service, "");
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            var allFiles = GetFiles(service, "");
            
            DownloadFile(service, allFiles[0]);
            
        }

    }
}
