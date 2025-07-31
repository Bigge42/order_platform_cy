using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Utilities
{
    public class EncryptUtil
    {

        /// <summary>
        /// md5加密
        /// </summary>
        /// <returns></returns>
        public static string Md5Encrypt(string value,bool isLower=false)
        {

            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(value));
                var strResult = BitConverter.ToString(result);
                if (isLower)
                {
                    strResult= strResult.ToLower();

                }
                return strResult.Replace("-", "");
            }
        }
    }
}
