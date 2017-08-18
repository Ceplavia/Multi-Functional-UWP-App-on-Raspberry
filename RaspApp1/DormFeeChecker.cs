using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace RaspApp1
{

    public class DormFeeChecker
    {
        private string roomNumber;
        public DormFeeChecker(string roomNumber)
        {
            this.roomNumber = roomNumber;
        }
        public async Task<string> DataUpdate()
        {
            Tuple<CookieContainer, dynamic> tuple = await GetIdAsync(roomNumber);
            Dictionary<string, dynamic> dictionary = await GetBalanceAsync(tuple);
            if (dictionary["success"])
            {
                string result = "￥" + dictionary["balance"];
                return result;
            }
            else return "Offline :(";
        }
        public async Task<Tuple<CookieContainer, dynamic>> GetIdAsync(string account)
        {
            string url = "http://10.136.2.5/jnuweb/WebService/JNUService.asmx/Login";
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            string dateTime = DateTime.Now.ToString();
            httpRequest.Method = "POST";
            httpRequest.Headers[HttpRequestHeader.KeepAlive] = "true";
            httpRequest.Accept = "*/*";
            httpRequest.ContentType = "application/json";
            httpRequest.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
            httpRequest.Headers["Token"] = GetToken(out dateTime);
            httpRequest.Headers["DateTime"] = dateTime;
            CookieContainer cookieJar = new CookieContainer();
            httpRequest.CookieContainer = cookieJar;
            string json = "{\"user\":\"" + account + "\",\"password\":\"2ay/7lGoIrXLc9KeacM7sg==\"}";
            httpRequest.Headers[HttpRequestHeader.ContentLength] = Encoding.UTF8.GetBytes(json).Length.ToString();
            Stream requestStream = await httpRequest.GetRequestStreamAsync();
            using (var streamWriter = new StreamWriter(requestStream))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }
            WebResponse response = await httpRequest.GetResponseAsync();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            dynamic data = JValue.Parse(streamReader.ReadToEnd());
            return new Tuple<CookieContainer, dynamic>(cookieJar, data.d.ResultList[0].customerId.Value);
        }
        public async Task<Dictionary<string, dynamic>> GetBalanceAsync(Tuple<CookieContainer, dynamic> loginData)
        {
            string dateTime = DateTime.Now.ToString();
            string url = "http://10.136.2.5/jnuweb/WebService/JNUService.asmx/GetUserInfo";
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.Headers[HttpRequestHeader.KeepAlive] = "true";
            httpRequest.Accept = "*/*";
            httpRequest.ContentType = "application/json";
            httpRequest.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
            httpRequest.Headers["Token"] = GetToken(out dateTime);
            httpRequest.Headers["DateTime"] = dateTime;
            httpRequest.CookieContainer = loginData.Item1;
            httpRequest.Headers[HttpRequestHeader.ContentLength] = "0";
            WebResponse response = await httpRequest.GetResponseAsync();
            Stream responseStream = response.GetResponseStream();
            using (responseStream)
            {
                using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    string jsonString = streamReader.ReadToEnd();
                    var rootObject = JsonObject.Parse(jsonString);
                    Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>()
                    {
                        {"success",Convert.ToBoolean(data.d.Success.Value)},
                        {"balance",rootObject["propertyName"]},
                        /*
                            ！！！！！！！！！！！！！！！！！！！！！！
                         */
                    };
                    return dict;
                }
            }
        }
        public static string GetToken(out string dateTime)
        {
            /*
                GetId.
             */
            dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string plainText = "{\"userID\":0,\"tokenTime\":\"" + dateTime + "\"}";
            
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.Key = StringToByteArray("436574536f667445454d537973576562");
            aes.IV = StringToByteArray("1934577290ABCDEF1264147890ACAE45");
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray()) + "%0A";
                }
            }
        }
        private static string GetToken(dynamic id, out string dateTime)
        {
            /*
                GetBalance.
             */
            dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string plainText = "{\"userID\":" + id + ",\"tokenTime\":\"" + dateTime + "\"}";
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.Key = StringToByteArray("436574536f667445454d537973576562");
            aes.IV = StringToByteArray("1934577290ABCDEF1264147890ACAE45");
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray()).Insert(64, "%0A");
                }
            }
        }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
