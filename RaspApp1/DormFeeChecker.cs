using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

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
            Tuple<CookieContainer, string> tuplea = await GetIdAsync(roomNumber);
            Tuple<string, string> tupleab = await GetBalanceAsync(tuplea);
            if (tupleab.Item1 == "true")
            {
                return "￥" + tupleab.Item2;
            }
            else return "Offline :(";
        }
        public async Task<Tuple<CookieContainer, string>> GetIdAsync(string account)
        {
            string url = "http://10.136.2.5/jnuweb/WebService/JNUService.asmx/Login";
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            string dateTime;
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
            string jsonString = streamReader.ReadToEnd();
            Regex regex = new Regex("customerId.{2}([0-9]+)");
            MatchCollection matches = regex.Matches(jsonString);
            var data = new Tuple<CookieContainer, string>(cookieJar, matches[0].Groups[1].Value);
            return data;
        }
        public async Task<Tuple<string, string>> GetBalanceAsync(Tuple<CookieContainer, string> loginData)
        {
            string dateTime;
            string url = "http://10.136.2.5/jnuweb/WebService/JNUService.asmx/GetUserInfo";
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.Headers[HttpRequestHeader.KeepAlive] = "true";
            httpRequest.Accept = "*/*";
            httpRequest.ContentType = "application/json";
            httpRequest.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
            httpRequest.Headers["Token"] = GetToken(loginData.Item2, out dateTime);
            httpRequest.Headers["DateTime"] = dateTime;
            httpRequest.CookieContainer = loginData.Item1;
            httpRequest.Headers[HttpRequestHeader.ContentLength] = "0";
            Stream requestStream = await httpRequest.GetRequestStreamAsync();
            WebResponse response = await httpRequest.GetResponseAsync();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            string jsonString = streamReader.ReadToEnd();
            Regex regex = new Regex("Success.+(true|false).+账户余额.{14}([0-9.]+)");
            MatchCollection matches = regex.Matches(jsonString);
            return new Tuple<string, string>(matches[0].Groups[1].Value, matches[0].Groups[2].Value);
        }
        public string GetToken(out string dateTime)
        {
            /*
                GetId.
             */
            dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string plainText = "{\"userID\":0,\"tokenTime\":\"" + dateTime + "\"}";
            Crypt crypt = new Crypt();
            byte[] encryptedText = crypt.Encrypt(Encoding.ASCII.GetBytes(plainText));
            return Convert.ToBase64String(encryptedText) + "%0A";
        }
        public string GetToken(dynamic id, out string dateTime)
        {
            /*
                GetBalance.
             */
            dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string plainText = "{\"userID\":" + id + ",\"tokenTime\":\"" + dateTime + "\"}";
            Crypt crypt = new Crypt();
            byte[] encryptedText = crypt.Encrypt(Encoding.ASCII.GetBytes(plainText));
            return Convert.ToBase64String(encryptedText).Insert(64, "%0A");
        }
    }
    public class Crypt
    {
        byte[] AES_Key = StringToByteArray("436574536f667445454d537973576562");
        byte[] AES_IV = StringToByteArray("1934577290ABCDEF1264147890ACAE45");
        private IBuffer m_iv = null;
        private CryptographicKey m_key;
        public Crypt()
        {
            IBuffer key = AES_Key.AsBuffer();
            m_iv = AES_IV.AsBuffer();
            SymmetricKeyAlgorithmProvider provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            m_key = provider.CreateSymmetricKey(key);
        }
        public byte[] Encrypt(byte[] input)
        {
            IBuffer bufferMessage = CryptographicBuffer.ConvertStringToBinary(Encoding.ASCII.GetString(input), BinaryStringEncoding.Utf8);
            IBuffer bufferEncrypt = CryptographicEngine.Encrypt(m_key, bufferMessage, m_iv);
            return bufferEncrypt.ToArray();
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
