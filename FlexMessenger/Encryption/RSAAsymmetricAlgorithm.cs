using System;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;

namespace Encryption
{
    public class RSAAsymmetricAlgorithm: AsymmetricAlgorithm
    {
        RSACryptoServiceProvider RSA;

        public RSAAsymmetricAlgorithm()
        {
            RSA = null;
        }

        public override void getPrepared()
        {
            RSA = new RSACryptoServiceProvider();
        }

        public override byte[] getParametrs(bool flag)
        {
            try
            {
                return ObjectToByteArray(RSA.ExportParameters(flag));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override byte[] Encrypt(byte[] data, byte[] Parametrs)
        {
            try
            {
                byte[] encryptedData;
                RSAParameters param = (RSAParameters)ByteArrayToObject(Parametrs);
                RSA.ImportParameters(param);

                bool flag = false;
                encryptedData = RSA.Encrypt(data, flag);
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                throw e;
            }
        }

        public override byte[] Decrypt(byte[] data, byte[] Parametrs)
        {
            try
            {
                byte[] decryptedData;
                RSAParameters param = (RSAParameters)ByteArrayToObject(Parametrs);
                RSA.ImportParameters(param);
                bool flag = false;
                decryptedData = RSA.Decrypt(data, flag);
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                throw e;
            }

        }

        protected byte[] ObjectToByteArray(RSAParameters param)
        {

            XmlSerializer bf = new XmlSerializer(param.GetType());
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, param);
                ms.Flush();
                return ms.ToArray();
            }
        }

        protected RSAParameters ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            XmlSerializer binForm = new XmlSerializer(typeof(RSAParameters));
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            RSAParameters param = (RSAParameters)binForm.Deserialize(memStream);
            return param;
        }
    }
}
