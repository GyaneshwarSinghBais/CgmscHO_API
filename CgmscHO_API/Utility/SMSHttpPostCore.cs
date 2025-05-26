using System;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Security.Cryptography;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CgmscHO_API.Utility
{
    public class SMSHttpPostCore
    {
        // Method for sending single SMS.
        public string sendSingleSMS(string mobileNo, string message)
        {
            string username = "cgmscl";
            string password = "Cgmscl#321$";
            string senderid = "CGMSCL";
            string secureKey = "ecb45a42-32b6-4087-9128-ecaee9d570dc";
            Stream dataStream;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequest");
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;
            request.UserAgent = ".NET Framework";
            request.Method = "POST";

            string encryptedPassword = encryptedPasswod(password);
            string newSecureKey = hashGenerator(username.Trim(), senderid.Trim(), message.Trim(), secureKey.Trim());
            string smsservicetype = "singlemsg";

            string query = $"username={HttpUtility.UrlEncode(username.Trim())}" +
                           $"&password={HttpUtility.UrlEncode(encryptedPassword)}" +
                           $"&smsservicetype={HttpUtility.UrlEncode(smsservicetype)}" +
                           $"&content={HttpUtility.UrlEncode(message.Trim())}" +
                           $"&mobileno={HttpUtility.UrlEncode(mobileNo)}" +
                           $"&senderid={HttpUtility.UrlEncode(senderid.Trim())}" +
                           $"&key={HttpUtility.UrlEncode(newSecureKey.Trim())}";

            byte[] byteArray = Encoding.ASCII.GetBytes(query);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            string status = ((HttpWebResponse)response).StatusDescription;

            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }

        // Method for sending bulk SMS.
        public string sendBulkSMS(string username, string password, string senderid, string mobileNos, string message, string secureKey)
        {
            Stream dataStream;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequest");
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";
            request.Method = "POST";

            string encryptedPassword = encryptedPasswod(password);
            string newSecureKey = hashGenerator(username.Trim(), senderid.Trim(), message.Trim(), secureKey.Trim());
            string smsservicetype = "bulkmsg";

            string query = $"username={HttpUtility.UrlEncode(username.Trim())}" +
                           $"&password={HttpUtility.UrlEncode(encryptedPassword)}" +
                           $"&smsservicetype={HttpUtility.UrlEncode(smsservicetype)}" +
                           $"&content={HttpUtility.UrlEncode(message.Trim())}" +
                           $"&bulkmobno={HttpUtility.UrlEncode(mobileNos)}" +
                           $"&senderid={HttpUtility.UrlEncode(senderid.Trim())}" +
                           $"&key={HttpUtility.UrlEncode(newSecureKey.Trim())}";

            byte[] byteArray = Encoding.ASCII.GetBytes(query);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            string status = ((HttpWebResponse)response).StatusDescription;

            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }

        // Method for sending Unicode SMS.
        public string sendUnicodeSMS(string username, string password, string senderid, string mobileNos, string unicodeMessage, string secureKey)
        {
            Stream dataStream;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequest");
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";
            request.Method = "POST";

            string uConvertedMessage = string.Empty;
            foreach (char c in unicodeMessage)
            {
                int j = (int)c;
                string sss = "&#" + j + ";";
                uConvertedMessage += sss;
            }

            string encryptedPassword = encryptedPasswod(password);
            string newSecureKey = hashGenerator(username.Trim(), senderid.Trim(), uConvertedMessage.Trim(), secureKey.Trim());
            string smsservicetype = "unicodemsg";

            string query = $"username={HttpUtility.UrlEncode(username.Trim())}" +
                           $"&password={HttpUtility.UrlEncode(encryptedPassword)}" +
                           $"&smsservicetype={HttpUtility.UrlEncode(smsservicetype)}" +
                           $"&content={HttpUtility.UrlEncode(uConvertedMessage.Trim())}" +
                           $"&bulkmobno={HttpUtility.UrlEncode(mobileNos)}" +
                           $"&senderid={HttpUtility.UrlEncode(senderid.Trim())}" +
                           $"&key={HttpUtility.UrlEncode(newSecureKey.Trim())}";

            byte[] byteArray = Encoding.ASCII.GetBytes(query);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            string status = ((HttpWebResponse)response).StatusDescription;

            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }

        // Method for sending OTP message.
        public string sendOTPMSG(string username, string senderId, string content, string secureKey, string mobileNo, string templateId)
        {
            string password = "CGMS123#cgmsc$";
            Stream dataStream;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequestDLT");
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";
            request.Method = "POST";

            string encryptedPassword = encryptedPasswod(password);
            string key = hashGenerator(username.Trim(), senderId.Trim(), content.Trim(), secureKey.Trim());
            string smsservicetype = "otpmsg";

            string query = $"username={HttpUtility.UrlEncode(username.Trim())}" +
                           $"&password={HttpUtility.UrlEncode(encryptedPassword)}" +
                           $"&smsservicetype={HttpUtility.UrlEncode(smsservicetype)}" +
                           $"&content={HttpUtility.UrlEncode(content.Trim())}" +
                           $"&mobileno={HttpUtility.UrlEncode(mobileNo)}" +
                           $"&senderid={HttpUtility.UrlEncode(senderId.Trim())}" +
                           $"&key={HttpUtility.UrlEncode(key.Trim())}" +
                           $"&templateid={HttpUtility.UrlEncode(templateId.Trim())}";

            byte[] byteArray = Encoding.ASCII.GetBytes(query);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            string status = ((HttpWebResponse)response).StatusDescription;

            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }

        // Method for sending Unicode OTP SMS.
        public string sendUnicodeOTPSMS(string username, string password, string senderid, string mobileNos, string unicodeMessage, string secureKey, string templateId)
        {
            Stream dataStream;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequest");
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";
            request.Method = "POST";

            string uConvertedMessage = string.Empty;
            foreach (char c in unicodeMessage)
            {
                int j = (int)c;
                string sss = "&#" + j + ";";
                uConvertedMessage += sss;
            }

            string encryptedPassword = encryptedPasswod(password);
            string newSecureKey = hashGenerator(username.Trim(), senderid.Trim(), uConvertedMessage.Trim(), secureKey.Trim());
            string smsservicetype = "unicodeotpmsg";

            string query = $"username={HttpUtility.UrlEncode(username.Trim())}" +
                           $"&password={HttpUtility.UrlEncode(encryptedPassword)}" +
                           $"&smsservicetype={HttpUtility.UrlEncode(smsservicetype)}" +
                           $"&content={HttpUtility.UrlEncode(uConvertedMessage.Trim())}" +
                           $"&bulkmobno={HttpUtility.UrlEncode(mobileNos)}" +
                           $"&senderid={HttpUtility.UrlEncode(senderid.Trim())}" +
                           $"&key={HttpUtility.UrlEncode(newSecureKey.Trim())}" +
                           $"&templateid={HttpUtility.UrlEncode(templateId.Trim())}";

            byte[] byteArray = Encoding.ASCII.GetBytes(query);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            string status = ((HttpWebResponse)response).StatusDescription;

            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }

        // Method to generate encrypted password.
        public string encryptedPasswod(string password)
        {
            string encPwd = "";
            byte[] encode = new byte[password.Length];
            encode = Encoding.UTF8.GetBytes(password);
            encPwd = Convert.ToBase64String(encode);
            return encPwd;
        }

        // Method to generate hash key.
        public string hashGenerator(string user, string sid, string msg, string key)
        {
            string hash = "";
            string input = user + sid + msg + key;
            byte[] data = Encoding.UTF8.GetBytes(input);
            using (SHA512 shaM = new SHA512Managed())
            {
                var result = shaM.ComputeHash(data);
                hash = string.Concat(Array.ConvertAll(result, x => x.ToString("x2")));
            }
            return hash;
        }

        // Callback method for validating the SSL certificate.
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }


        //public String sendOTPMSG (string username, string senderId1, string content1, string secureKey, string mobileno1, string smsservicetype1,string templateid)
        //   {
           

        //    Stream dataStream;
        //   // String password = "Cgmscl#321$";
        //    //String password = "321#dpdmiscgmsc";

        //    String password = "CGMS123#cgmsc$";
        //    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //forcing .Net framework to use TLSv1.2

        //    //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequest");
        //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequestDLT");
        //    request.ProtocolVersion = HttpVersion.Version10;
        //    request.KeepAlive = false;
        //    request.ServicePoint.ConnectionLimit = 1;

        //    //((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
        //    ((HttpWebRequest)request).UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";

        //    request.Method = "POST";
        //    System.Net.ServicePointManager.CertificatePolicy = new MyPolicy();



        //    String encryptedPassword = encryptedPasswod(password);
        //    String key = hashGenerator(username.Trim(), senderId1.Trim(), content1.Trim(), secureKey.Trim());

        // String smsservicetype = "otpmsg";
        ////   String smsservicetype = "Login";//For OTP message.
        //  // String smsservicetype = "Transactional";//For OTP message
        ////    String smsservicetype = smsservicetype1.ToString();
        //    String query = "username=" + HttpUtility.UrlEncode(username.Trim()) +
        //        "&password=" + HttpUtility.UrlEncode(encryptedPassword) +

        //        "&smsservicetype=" + HttpUtility.UrlEncode(smsservicetype) +

        //        "&content=" + HttpUtility.UrlEncode(content1.Trim()) +

        //        "&mobileno=" + HttpUtility.UrlEncode(mobileno1) +

        //        "&senderid=" + HttpUtility.UrlEncode(senderId1.Trim()) +
                  
        //      "&key=" + HttpUtility.UrlEncode(key.Trim()) +

        //    "&templateid=" + HttpUtility.UrlEncode(templateid.Trim());



        //    byte[] byteArray = Encoding.ASCII.GetBytes(query);

        //    request.ContentType = "application/x-www-form-urlencoded";

        //    request.ContentLength = byteArray.Length;



        //    dataStream = request.GetRequestStream();

        //    dataStream.Write(byteArray, 0, byteArray.Length);

        //    dataStream.Close();

        //    WebResponse response = request.GetResponse();

        //    String Status = ((HttpWebResponse)response).StatusDescription;

        //    dataStream = response.GetResponseStream();

        //    StreamReader reader = new StreamReader(dataStream);

        //    String responseFromServer = reader.ReadToEnd();

        //    reader.Close();

        //    dataStream.Close();

        //    response.Close();
        //    return responseFromServer;

        //}

        //class MyPolicy : ICertificatePolicy
        //{
        //    public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
        //    {
        //        return true;
        //    }
        //}
    }
}
