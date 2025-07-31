using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Utilities
{
    public class RSAFromPkcs8
    {
        public static string sign(byte[] content, string privateKey)
        {
            RSACryptoServiceProvider rsa = RSAFromPkcs8.DecodePemPrivateKey(privateKey);
            SHA1 sh = new SHA1CryptoServiceProvider();
            byte[] signData = rsa.SignData(content, sh);
            return Convert.ToBase64String(signData);
        }

        // Token: 0x06000A8A RID: 2698 RVA: 0x0002B948 File Offset: 0x00029B48
        public static string sign(string content, string privateKey)
        {
            byte[] Data = Encoding.UTF8.GetBytes(content);
            RSACryptoServiceProvider rsa = RSAFromPkcs8.DecodePemPrivateKey(privateKey);
            SHA1 sh = new SHA1CryptoServiceProvider();
            byte[] signData = rsa.SignData(Data, sh);
            return Convert.ToBase64String(signData);
        }

        // Token: 0x06000A8B RID: 2699 RVA: 0x0002B984 File Offset: 0x00029B84
        public static string signDotNET(string content, string privateKey)
        {
            byte[] Data = Encoding.UTF8.GetBytes(content);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(RSAFromPkcs8.ConvertFromPrivateKey(privateKey));
            SHA1 sh = new SHA1CryptoServiceProvider();
            byte[] signData = rsa.SignData(Data, sh);
            return Convert.ToBase64String(signData);
        }

        // Token: 0x06000A8C RID: 2700 RVA: 0x0002B9CC File Offset: 0x00029BCC
        public static bool verifyXML(string content, string signedString, string publicKeyXML)
        {
            byte[] Data = Encoding.UTF8.GetBytes(content);
            byte[] data = Convert.FromBase64String(signedString);
            RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
            rsaPub.FromXmlString(publicKeyXML);
            SHA1 sh = new SHA1CryptoServiceProvider();
            return rsaPub.VerifyData(Data, sh, data);
        }

        // Token: 0x06000A8D RID: 2701 RVA: 0x0002BA18 File Offset: 0x00029C18
        public static bool verify(string content, string signedString, string publicKey)
        {
            byte[] Data = Encoding.UTF8.GetBytes(content);
            byte[] data = Convert.FromBase64String(signedString);
            RSAParameters paraPub = RSAFromPkcs8.ConvertFromPublicKey(publicKey);
            RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
            rsaPub.ImportParameters(paraPub);
            SHA1 sh = new SHA1CryptoServiceProvider();
            return rsaPub.VerifyData(Data, sh, data);
        }

        // Token: 0x06000A8E RID: 2702 RVA: 0x0002BA6C File Offset: 0x00029C6C
        public static string decrypt(byte[] data, string privateKey)
        {
            RSACryptoServiceProvider rsa = RSAFromPkcs8.DecodePemPrivateKey(privateKey);
            SHA1 sh = new SHA1CryptoServiceProvider();
            byte[] source = rsa.Decrypt(data, false);
            Encoding utf8 = Encoding.UTF8;
            char[] asciiChars = new char[utf8.GetCharCount(source, 0, source.Length)];
            utf8.GetChars(source, 0, source.Length, asciiChars, 0);
            return new string(asciiChars);
        }

        // Token: 0x06000A8F RID: 2703 RVA: 0x0002BAD0 File Offset: 0x00029CD0
        public static string decryptData(string resData, string privateKey)
        {
            byte[] DataToDecrypt = Convert.FromBase64String(resData);
            string result = "";
            for (int i = 0; i < DataToDecrypt.Length / 128; i++)
            {
                byte[] buf = new byte[128];
                for (int j = 0; j < 128; j++)
                {
                    buf[j] = DataToDecrypt[j + 128 * i];
                }
                result += RSAFromPkcs8.decrypt(buf, privateKey);
            }
            return result;
        }

        // Token: 0x06000A90 RID: 2704 RVA: 0x0002BB54 File Offset: 0x00029D54
        private static RSACryptoServiceProvider DecodePemPrivateKey(string pemstr)
        {
            byte[] pkcs8privatekey = Convert.FromBase64String(pemstr);
            bool flag = pkcs8privatekey != null;
            RSACryptoServiceProvider result;
            if (flag)
            {
                RSACryptoServiceProvider rsa = RSAFromPkcs8.DecodePrivateKeyInfo(pkcs8privatekey);
                result = rsa;
            }
            else
            {
                result = null;
            }
            return result;
        }

        // Token: 0x06000A91 RID: 2705 RVA: 0x0002BB84 File Offset: 0x00029D84
        private static RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8)
        {
            byte[] SeqOID = new byte[]
            {
            48,
            13,
            6,
            9,
            42,
            134,
            72,
            134,
            247,
            13,
            1,
            1,
            1,
            5,
            0
            };
            byte[] seq = new byte[15];
            MemoryStream mem = new MemoryStream(pkcs8);
            int lenstream = (int)mem.Length;
            BinaryReader binr = new BinaryReader(mem);
            RSACryptoServiceProvider result;
            try
            {
                ushort twobytes = binr.ReadUInt16();
                bool flag = twobytes == 33072;
                if (flag)
                {
                    binr.ReadByte();
                }
                else
                {
                    bool flag2 = twobytes == 33328;
                    if (!flag2)
                    {
                        return null;
                    }
                    binr.ReadInt16();
                }
                byte bt = binr.ReadByte();
                bool flag3 = bt != 2;
                if (flag3)
                {
                    result = null;
                }
                else
                {
                    twobytes = binr.ReadUInt16();
                    bool flag4 = twobytes != 1;
                    if (flag4)
                    {
                        result = null;
                    }
                    else
                    {
                        seq = binr.ReadBytes(15);
                        bool flag5 = !RSAFromPkcs8.CompareBytearrays(seq, SeqOID);
                        if (flag5)
                        {
                            result = null;
                        }
                        else
                        {
                            bt = binr.ReadByte();
                            bool flag6 = bt != 4;
                            if (flag6)
                            {
                                result = null;
                            }
                            else
                            {
                                bt = binr.ReadByte();
                                bool flag7 = bt == 129;
                                if (flag7)
                                {
                                    binr.ReadByte();
                                }
                                else
                                {
                                    bool flag8 = bt == 130;
                                    if (flag8)
                                    {
                                        binr.ReadUInt16();
                                    }
                                }
                                byte[] rsaprivkey = binr.ReadBytes((int)((long)lenstream - mem.Position));
                                RSACryptoServiceProvider rsacsp = RSAFromPkcs8.DecodeRSAPrivateKey(rsaprivkey);
                                result = rsacsp;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                result = null;
            }
            finally
            {
                binr.Close();
            }
            return result;
        }

        // Token: 0x06000A92 RID: 2706 RVA: 0x0002BD30 File Offset: 0x00029F30
        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            bool flag = a.Length != b.Length;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                int i = 0;
                foreach (byte c in a)
                {
                    bool flag2 = c != b[i];
                    if (flag2)
                    {
                        return false;
                    }
                    i++;
                }
                result = true;
            }
            return result;
        }

        // Token: 0x06000A93 RID: 2707 RVA: 0x0002BD90 File Offset: 0x00029F90
        private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);
            RSACryptoServiceProvider result;
            try
            {
                ushort twobytes = binr.ReadUInt16();
                bool flag = twobytes == 33072;
                if (flag)
                {
                    binr.ReadByte();
                }
                else
                {
                    bool flag2 = twobytes == 33328;
                    if (!flag2)
                    {
                        return null;
                    }
                    binr.ReadInt16();
                }
                twobytes = binr.ReadUInt16();
                bool flag3 = twobytes != 258;
                if (flag3)
                {
                    result = null;
                }
                else
                {
                    byte bt = binr.ReadByte();
                    bool flag4 = bt > 0;
                    if (flag4)
                    {
                        result = null;
                    }
                    else
                    {
                        int elems = RSAFromPkcs8.GetIntegerSize(binr);
                        byte[] MODULUS = binr.ReadBytes(elems);
                        elems = RSAFromPkcs8.GetIntegerSize(binr);
                        byte[] E = binr.ReadBytes(elems);
                        elems = RSAFromPkcs8.GetIntegerSize(binr);
                        byte[] D = binr.ReadBytes(elems);
                        elems = RSAFromPkcs8.GetIntegerSize(binr);
                        byte[] P = binr.ReadBytes(elems);
                        elems = RSAFromPkcs8.GetIntegerSize(binr);
                        byte[] Q = binr.ReadBytes(elems);
                        elems = RSAFromPkcs8.GetIntegerSize(binr);
                        byte[] DP = binr.ReadBytes(elems);
                        elems = RSAFromPkcs8.GetIntegerSize(binr);
                        byte[] DQ = binr.ReadBytes(elems);
                        elems = RSAFromPkcs8.GetIntegerSize(binr);
                        byte[] IQ = binr.ReadBytes(elems);
                        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                        RSA.ImportParameters(new RSAParameters
                        {
                            Modulus = MODULUS,
                            Exponent = E,
                            D = D,
                            P = P,
                            Q = Q,
                            DP = DP,
                            DQ = DQ,
                            InverseQ = IQ
                        });
                        result = RSA;
                    }
                }
            }
            catch (Exception)
            {
                result = null;
            }
            finally
            {
                binr.Close();
            }
            return result;
        }

        // Token: 0x06000A94 RID: 2708 RVA: 0x0002BF84 File Offset: 0x0002A184
        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = binr.ReadByte();
            bool flag = bt != 2;
            int result;
            if (flag)
            {
                result = 0;
            }
            else
            {
                bt = binr.ReadByte();
                bool flag2 = bt == 129;
                int count;
                if (flag2)
                {
                    count = (int)binr.ReadByte();
                }
                else
                {
                    bool flag3 = bt == 130;
                    if (flag3)
                    {
                        byte highbyte = binr.ReadByte();
                        byte lowbyte = binr.ReadByte();
                        byte[] array = new byte[4];
                        array[0] = lowbyte;
                        array[1] = highbyte;
                        byte[] modint = array;
                        count = BitConverter.ToInt32(modint, 0);
                    }
                    else
                    {
                        count = (int)bt;
                    }
                }
                while (binr.ReadByte() == 0)
                {
                    count--;
                }
                binr.BaseStream.Seek(-1L, SeekOrigin.Current);
                result = count;
            }
            return result;
        }

        // Token: 0x06000A95 RID: 2709 RVA: 0x0002C040 File Offset: 0x0002A240
        private static RSAParameters ConvertFromPublicKey(string pemFileConent)
        {
            byte[] keyData = Convert.FromBase64String(pemFileConent);
            bool flag = keyData.Length < 162;
            if (flag)
            {
                throw new ArgumentException("pem file content is incorrect.");
            }
            byte[] pemModulus = new byte[128];
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, 29, pemModulus, 0, 128);
            Array.Copy(keyData, 159, pemPublicExponent, 0, 3);
            return new RSAParameters
            {
                Modulus = pemModulus,
                Exponent = pemPublicExponent
            };
        }

        // Token: 0x06000A96 RID: 2710 RVA: 0x0002C0C4 File Offset: 0x0002A2C4
        private static RSAParameters ConvertFromPrivateKey(string pemFileConent)
        {
            byte[] keyData = Convert.FromBase64String(pemFileConent);
            bool flag = keyData.Length < 609;
            if (flag)
            {
                throw new ArgumentException("pem file content is incorrect.");
            }
            int index = 11;
            byte[] pemModulus = new byte[128];
            Array.Copy(keyData, index, pemModulus, 0, 128);
            index += 128;
            index += 2;
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, index, pemPublicExponent, 0, 3);
            index += 3;
            index += 4;
            byte[] pemPrivateExponent = new byte[128];
            Array.Copy(keyData, index, pemPrivateExponent, 0, 128);
            index += 128;
            index += ((keyData[index + 1] == 64) ? 2 : 3);
            byte[] pemPrime = new byte[64];
            Array.Copy(keyData, index, pemPrime, 0, 64);
            index += 64;
            index += ((keyData[index + 1] == 64) ? 2 : 3);
            byte[] pemPrime2 = new byte[64];
            Array.Copy(keyData, index, pemPrime2, 0, 64);
            index += 64;
            index += ((keyData[index + 1] == 64) ? 2 : 3);
            byte[] pemExponent = new byte[64];
            Array.Copy(keyData, index, pemExponent, 0, 64);
            index += 64;
            index += ((keyData[index + 1] == 64) ? 2 : 3);
            byte[] pemExponent2 = new byte[64];
            Array.Copy(keyData, index, pemExponent2, 0, 64);
            index += 64;
            index += ((keyData[index + 1] == 64) ? 2 : 3);
            byte[] pemCoefficient = new byte[64];
            Array.Copy(keyData, index, pemCoefficient, 0, 64);
            return new RSAParameters
            {
                Modulus = pemModulus,
                Exponent = pemPublicExponent,
                D = pemPrivateExponent,
                P = pemPrime,
                Q = pemPrime2,
                DP = pemExponent,
                DQ = pemExponent2,
                InverseQ = pemCoefficient
            };
        }
    }
}
