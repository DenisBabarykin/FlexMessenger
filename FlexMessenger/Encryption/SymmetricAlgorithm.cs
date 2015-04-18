namespace Encryption
{
    public abstract class SymmetricAlgorithm
    {
        public abstract void getPrepared();
        public abstract byte[] getKey();
        public abstract byte[] getIV();
        public abstract byte[] Encrypt(byte[] data, byte[] Key, byte[] IV);
        public abstract byte[] Decrypt(byte[] data, byte[] Key, byte[] IV);
    }
}
