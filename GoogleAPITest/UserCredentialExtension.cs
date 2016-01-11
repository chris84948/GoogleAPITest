using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Responses;

namespace GoogleAPITest
{
    public static class UserCredentialExtension
    {
        public static bool IsExpired(this UserCredential userCredential)
        {
            return DateTime.Now >= userCredential.Token.Issued.AddSeconds((double)userCredential.Token.ExpiresInSeconds);
        }
    }
}
