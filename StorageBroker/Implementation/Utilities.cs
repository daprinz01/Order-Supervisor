using StorageBroker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StorageBroker.Implementation
{
    public class Utilities: IUtilities
    {

        public int GenerateRandomNumber()
        {
            RNGCryptoServiceProvider provider = new();
            var byteArray = new byte[4];
            provider.GetBytes(byteArray);

            //convert 4 bytes to an integer
            var randomInteger = BitConverter.ToInt32(byteArray, 0);
            provider.Dispose();
            return randomInteger;
        }
        public int GenerateMagicNumber()
        {
            return new Random().Next(1, 10);
        }
    }
}
