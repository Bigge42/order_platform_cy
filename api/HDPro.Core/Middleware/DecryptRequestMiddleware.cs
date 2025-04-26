using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.Configuration;
using HDPro.Core.Const;
using HDPro.Core.Enums;
using Microsoft.Extensions.Hosting;
using HDPro.Core.Extensions;
using HDPro.Core.Services;

namespace HDPro.Core.Middleware
{
    public class DecryptRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string key = "5ABA9E202D94C43A";// AppSetting.GetSection("ParamEncryption")["kv"]; 
        //private static readonly string iv = "5ABA9E202D94C43A";// AppSetting.GetSection("ParamEncryption")["iv"];

        public DecryptRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);
            return;

            // "".DecryptDES("R7tFg2i9L4P3e1Q0");
            if (context.Request.Method == "POST")
            {
                if (context.Request.ContentLength > 0)
                {
                    context.Request.EnableBuffering();
                    (context.RequestServices.GetService(typeof(ActionObserver)) as ActionObserver).RequestDate = DateTime.Now;
                    string body = null;
                    try
                    {
                        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true);
                        body = await reader.ReadToEndAsync();
                        body= body.Trim('\"');
                        var decryptedBody = DecryptData(body);
                        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(decryptedBody));
                    }
                    catch (Exception exception)
                    {
                        var env = context.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) as Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
                        string message = exception.Message + exception.StackTrace + exception.InnerException;
                        Logger.Error(LoggerType.Exception, "加密参数不正确:" + message);
                        if (!env.IsDevelopment())
                        {
                            message = "加密参数不正确".Translator();
                        }
                        else
                        {
                            Console.WriteLine($"加密参数不正确,请求参数:{body},异常信息{message}");
                        }
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = ApplicationContentType.JSON;
                        await context.Response.WriteAsync(new { message, status = false }.Serialize(), Encoding.UTF8);
                        return;
                    }
                }
            }
            await _next(context);
        }

        private string DecryptData(string ciphertext)
        {

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            // 将加密后的字符串转换为字节数组
            byte[] cipherBytes = Convert.FromBase64String(ciphertext);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None; // 设置填充方式为PKCS7 PaddingMode.Zeros;// 

                // 创建解密器
                ICryptoTransform decryptor = aes.CreateDecryptor();

                // 解密数据
                byte[] plaintextBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                // 将解密后的字节数组转换为字符串
                string plaintext = Encoding.UTF8.GetString(plaintextBytes);

                return plaintext;
            }
            //using Aes aesAlg = Aes.Create();
            //aesAlg.Key = Encoding.UTF8.GetBytes(key);
            //aesAlg.IV = Encoding.UTF8.GetBytes(iv); // new byte[16]; // 如果使用了初始向量，也需要设置

            //ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            //byte[] encryptedBytes = Convert.FromBase64String(data);

            //using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
            //{
            //    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            //    {
            //        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            //        {
            //            return srDecrypt.ReadToEnd();
            //        }
            //    }
            //}

        }
    }
}