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
        private string[] Scopes = { DriveService.Scope.DriveFile };
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

        private void CreateFile()
        {
            var driveService = new DriveService();

            // Upload a new file
            File body = new File();
            body.Name = "Test1.txt";
            body.Description = "Test File";
            body.MimeType = "text/plain";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(TextBox.Text);
            var stream = new System.IO.MemoryStream(byteArray);
            FilesResource.CreateMediaUpload request = driveService.Files.Create(body, stream, "text/plain");
            request.Upload();
            File file = request.ResponseBody;

            // Show all files
            var list2 = driveService.Files.List().Execute();
            if (list2.Files != null)
                foreach (var fileItem in list2.Files)
                    Console.WriteLine(fileItem.Name + " - " + fileItem.Description);
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            CreateFile();
        }

    }
}
