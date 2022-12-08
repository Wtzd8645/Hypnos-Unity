using Morpheus.Core.Encryption;
using NUnit.Framework;

namespace Morpheus.Test
{
    public class EncryptionTest
    {
        [Test]
        public void Aes256Passes()
        {
            Aes256 encryption = new Aes256();
            byte[] data = new byte[96];
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = (byte)i;
            }
            byte[] result = encryption.Encrypt(data, 0, data.Length);
            result = encryption.Decrypt(result, 0, result.Length);

            if (result.Length != data.Length)
            {
                Assert.Fail();
                return;
            }

            bool isEqual = true;
            for (int i = 0; i < result.Length; ++i)
            {
                if (result[i] != i)
                {
                    isEqual = false;
                    break;
                }
            }
            Assert.AreEqual(true, isEqual);
        }
    }
}