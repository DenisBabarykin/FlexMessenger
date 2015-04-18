using System;
using System.Security.Cryptography;
using System.IO;

namespace Encryption
{
    public class TripleDesSymmetricAlgorithm: SymmetricAlgorithm
    {
        TripleDESCryptoServiceProvider tDESalg;

        public TripleDesSymmetricAlgorithm()
        {
            tDESalg = null;
        }

        public override void getPrepared()
        {
            tDESalg = new TripleDESCryptoServiceProvider(); 
        }

        public override byte[] getKey()
        {
            try
            {
                return tDESalg.Key;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override byte[] getIV()
        {
            try
            {
                return tDESalg.IV;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override byte[] Encrypt(byte[] data, byte[] Key, byte[] IV)
        {
            try
            {
                MemoryStream mStream = new MemoryStream();

                CryptoStream cStream = new CryptoStream(mStream, tDESalg.CreateEncryptor(Key, IV), CryptoStreamMode.Write);

                cStream.Write(data, 0, data.Length);
                cStream.FlushFinalBlock();

                byte[] ret = mStream.ToArray();

                cStream.Close();
                mStream.Close();

                return ret;
            }
            catch (CryptographicException e)
            {
                throw e;
            }
        }

        public override byte[] Decrypt(byte[] data, byte[] Key, byte[] IV)
        {
            try
            {
                MemoryStream msDecrypt = new MemoryStream(data);
                CryptoStream csDecrypt = new CryptoStream(msDecrypt,tDESalg.CreateDecryptor(Key, IV),CryptoStreamMode.Read);
                byte[] fromEncrypt = new byte[data.Length];

                csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);
                return fromEncrypt;
            }
            catch (CryptographicException e)
            {
                throw e;
            }

        }

    }
}
