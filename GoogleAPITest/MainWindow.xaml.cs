using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
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
        private MemoryDataStore dataStore;
        
        public MainWindow()
        {
            InitializeComponent();

            dataStore = new MemoryDataStore();

            string credPath = @"C:\Google Drive\Programming\Google Drive\.credentials\" + AppName;
            var creds = GetCredentials(credPath, dataStore);

            int i = 0;
        }

        private UserCredential GetCredentials(string credPath, MemoryDataStore dataStore)
        {
            try
            {
                using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    return GoogleWebAuthorizationBroker.AuthorizeAsync(stream,
                                                                       Scopes,
                                                                       "user",
                                                                       CancellationToken.None,
                                                                       dataStore).Result;
                }
            }
            catch (AggregateException ae)
            {
                var googleEx = ae.InnerException as TokenResponseException;
                System.Diagnostics.Debug.WriteLine("Message: " + googleEx.Message);
            }

            return null;
        }

    }
}
