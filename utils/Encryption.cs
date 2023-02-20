using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace BUAALogin.Encryption
{
    public class Base64Encoder
    {
        private const string _PADCHAR = "=";
        private const string _ALPHA = "LVoJPiCN2R8G90yg+hmFHuacZ1OWMnrsSTXkYpUq/3dlbfKwv6xztjI7DeBE45QA";

        public byte GetByte(string s, int i)
        {
            return Encoding.ASCII.GetBytes(s)[i];
        }

        public string GetBase64(byte[] byteArray)
        {
            string base64 = "";
            if (byteArray.Count() == 0)
                throw new Exception("GetBase64()异常，传入参数为空");
            int imax = byteArray.Count() - byteArray.Count() % 3;
            for(int i = 0; i<imax;i+=3)
            {
                int b10 = (byteArray[i] << 16) | (byteArray[i+1] << 8) | byteArray[i+2];
                base64 += "" + _ALPHA[(b10 >> 18)] + _ALPHA[((b10 >> 12) & 63)] + _ALPHA[((b10 >> 6) & 63)] + _ALPHA[(b10 & 63)];
            }
            if (byteArray.Count() % 3 == 1)
            {
                int b10 = byteArray[imax] << 16;
                base64 += "" + _ALPHA[(b10 >> 18)] + _ALPHA[((b10 >> 12) & 63)] + _PADCHAR + _PADCHAR;
            }
            else if(byteArray.Count() % 3 == 2)
            {
                int b10 = (byteArray[imax] << 16) | (byteArray[imax+1] << 8);
                base64 += "" +_ALPHA[(b10 >> 18)] + _ALPHA[((b10 >> 12) & 63)] + _ALPHA[((b10 >> 6) & 63)] + _PADCHAR;
            }
            return base64;
        }
    }

    public class MD5Encoder
    {
        public string GetMD5(string? password, string? token)
        {
            if(string.IsNullOrEmpty(password) || string.IsNullOrEmpty(token))
                throw new Exception("GetMD5()异常，传入参数为空");
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

            var hmac = new HMACMD5(tokenBytes);
            byte[] hashBytes = hmac.ComputeHash(passwordBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
    
    public class SHA1Encoder
    {
        public string GetSHA1(string value)
        {
            var sha1 = SHA1.Create();
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            byte[] hashBytes = sha1.ComputeHash(valueBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public class XencodeEncoder
    {
        public byte Ordat(string msg, int id)
        {
            return (msg.Length > id) ? Encoding.UTF8.GetBytes(msg)[id] : (byte)0;
        }

        public List<long> SEncode(string msg, bool key)
        {
            List<long> pwd = new List<long>();
            for(int i = 0;i < msg.Length; i += 4)
            {
                var code = Ordat(msg, i) | Ordat(msg, i + 1) << 8 | Ordat(msg, i + 2) << 16 | Ordat(msg, i + 3) << 24;
                pwd.Add(code);
            }
            if(key)
                pwd.Add(msg.Length);
            return pwd;
        }

        public byte[] LEncode(List<long> msg, bool key)
        {
            int len = (msg.Count-1) << 2;
            if (key)
            {
                char m = (char)msg[-1];
                if (m < len-3 || m > len)
                    throw new Exception("LEncode异常");
                len = m;
            }
            string[] msg_string = new string[msg.Count];

            List<byte> byteList = new List<byte>();
            for (int i = 0; i < msg.Count; i++)
            {
                byteList.Add((byte)(msg[i] & 0xff));
                byteList.Add((byte)(msg[i] >> 8  & 0xff));
                byteList.Add((byte)(msg[i] >> 16 & 0xff));
                byteList.Add((byte)(msg[i] >> 24 & 0xff));
            }
            byte[] byteArray = byteList.ToArray();
            if (key)
                return byteArray.Take(len).ToArray();
                // return string.Join("",msg_string.Take(len));
            // string res = string.Join("", msg_string);
            return byteArray;
        }

        public byte[] GetXEncode(string? msg, string? key)
        {
            if(string.IsNullOrEmpty(msg) || string.IsNullOrEmpty(key))
                throw new Exception("GetXEncode()异常，传入参数为空");
            var pwd = SEncode(msg, true);
            var pwdk = SEncode(key, false);
            if (pwdk.Count < 4)
            {
                int numZeros = 4 - pwdk.Count;
                pwdk.AddRange(Enumerable.Repeat((long)0, numZeros));
            }
            int n = pwd.Count - 1;
            long z = pwd[n];
            long y = pwd[0];
            long c = 0x86014019 | 0x183639A0; 
            long m = 0;
            long e = 0;
            int p = 0;
            long q = Convert.ToInt64(Math.Floor(6 + 52.0 / (n + 1)));
            long d = 0;

            while (q > 0)
            {
                d = (d + c) & (0x8CE0D9BF | 0x731F2640); 
                e = d >> 2 & 3;
                p = 0;

                while (p < n)
                {
                    y = pwd[(int)(p + 1)];
                    m = (long)(z >> 5 ^ y << 2);
                    m += (long)((y >> 3 ^ z << 4) ^ (d ^ y));
                    m += (long)(pwdk[(int)((p & 3) ^ e)] ^ z);
                    pwd[p] = (pwd[p] + m) & (0xEFB8D130 | 0x10472ECF); 
                    z = pwd[p];
                    p = p + 1;
                }

                y = pwd[0];
                m = z >> 5 ^ y << 2;
                m = m + ((y >> 3 ^ z << 4) ^ (d ^ y));
                m = m + (pwdk[(int)((p & 3) ^ e)] ^ z);
                pwd[n] = (pwd[n] + m) & (0xBB390742 | 0x44C6F8BD); 
                z = pwd[n];
                q = q - 1;
            }
            return LEncode(pwd, false);
        }
    }
    public static class AesEncryption
    {
        private static byte[] Key = Encoding.UTF8.GetBytes("YG5^v%bb&FX6YS!Y");
        private static byte[] IV = Encoding.UTF8.GetBytes("36JGQ%bYssG$k*v#");

        public static string EncryptString(string plainText)
        {
            byte[] encrypted;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        encrypted = ms.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        public static string DecryptString(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

}



