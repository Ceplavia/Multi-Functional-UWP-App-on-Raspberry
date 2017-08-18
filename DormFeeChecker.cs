using System;

public class DormFeeChecker
{
    public Dictionary<string, dynamic> GetBalance(Tuple<CookieContainer, dynamic> loginData)
    {
        string DateTime;
        string url = "http://10.136.2.5/jnuweb/WebService/JNUService.asmx/GetUserInfo";
        HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
        httpRequest.Method = "POST";
        httpRequest.KeepAlive = true;
        httpRequest.Accept = "*/*";
        httpRequest.ContentType = "application/json";
        httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
        httpRequest.Headers.Add("Token", GetToken(loginData.Item2, out DateTime));
        httpRequest.Headers.Add("DateTime", DateTime);
        httpRequest.CookieContainer = loginData.Item1;
        httpRequest.ContentLength = 0;
        HttpWebResponse httpresponse = (HttpWebResponse)httpRequest.GetResponse();
        using (Stream responseStream = httpresponse.GetResponseStream())
        {
            using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
            {
                dynamic data = JValue.Parse(streamReader.ReadToEnd());
                Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>()
                    {
                        {"success",Convert.ToBoolean(data.d.Success.Value)},
                        {"balance",data.d.ResultList[0].roomInfo[1].keyValue.Value},
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
