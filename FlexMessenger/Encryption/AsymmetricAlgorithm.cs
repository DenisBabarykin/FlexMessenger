namespace Encryption
{
    public abstract class AsymmetricAlgorithm
    {
        public abstract void getPrepared();
        public abstract byte[] getParametrs(bool flag);
        public abstract byte[] Encrypt(byte[] data, byte[] Parametrs);
        public abstract byte[] Decrypt(byte[] data, byte[] Parametrs);
    }
}
