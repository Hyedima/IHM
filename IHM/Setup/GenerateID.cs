using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IHM.Setup
{
    public class GenerateID
    {
        public static string GetID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            var FormNumber = BitConverter.ToUInt32(buffer, 0) ^ BitConverter.ToUInt32(buffer, 4) ^ BitConverter.ToUInt32(buffer, 8) ^ BitConverter.ToUInt32(buffer, 12);
            return FormNumber.ToString("X");
        }
    }
}