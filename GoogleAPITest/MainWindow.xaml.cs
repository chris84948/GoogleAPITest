using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
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
        private string[] Scopes = { DriveService.Scope.DriveAppdata };
        private string AppName = "GoogleAPITest";
        private UserCredential credentials;
        
        public MainWindow()
        {
            InitializeComponent();

            string credPath = AppDomain.CurrentDomain.BaseDirectory + AppName;

            credentials = GetCredentials(credPath);
        }

        private UserCredential GetCredentials(string credPath)
        {
            try
            {
                using (var stream = new System.IO.FileStream("client_secret.json",
                                                             System.IO.FileMode.Open,
                                                             System.IO.FileAccess.Read))
                {
                    return GoogleWebAuthorizationBroker.AuthorizeAsync(stream,
                                                                       Scopes,
                                                                       "user",
                                                                       CancellationToken.None,
                                                                       new FileDataStore(credPath, true)).Result;
                }
            }
            catch (AggregateException ae)
            {
                var googleEx = ae.InnerException as TokenResponseException;
                System.Diagnostics.Debug.WriteLine("Message: " + googleEx.Message);
            }

            return null;
        }

        /// <summary>
        /// Insert new file in the Application Data folder.
        /// </summary>
        /// <param name="service">Drive API service instance.</param>
        /// <param name="title">Title of the file to insert, including the extension.</param>
        /// <param name="description">Description of the file to insert.</param>
        /// <param name="mimeType">MIME type of the file to insert.</param>
        /// <param name="filename">Filename of the file to insert.</param>
        /// <returns>Inserted file metadata, null is returned if an API error occurred.</returns>
        private File InsertFile(DriveService service, string title, string description, string mimeType, string filename)
        {
            // File's metadata.
            var body = new File();
            body.Title = title;
            body.Description = description;
            body.MimeType = mimeType;
            body.Parents = new List<ParentReference>() { new ParentReference() { Id = "appfolder" } };

            // File's content.
            byte[] byteArray = System.IO.File.ReadAllBytes(filename);
            MemoryStream stream = new MemoryStream(byteArray);

            try
            {
                FilesResource.CreateMediaUpload request = service.Files.Create(body, stream, mimeType);
                request.Upload();

                File file = request.ResponseBody;
                return file;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }

    }
}
