using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PptReader.Models.Common
{
    static class BinaryUtil
    {
        public static IEnumerable<string> ToLines(this string text)
        {
            return text.Replace("\r", string.Empty).Split('\n');
        }

        public static unsafe byte[] GetBytes(float[] data)
        {
            var resultLength = data.Length * sizeof(float);
            byte[] result = new byte[resultLength];

            fixed (float* pFloat = data)
            {
                var pByte = (byte*)pFloat;
                for (int i = 0; i < resultLength; i++)
                {
                    result[i] = pByte[i];
                }
            }

            return result;
        }

        public static unsafe float[] GetFloats(byte[] data)
        {
            var resultLength = data.Length / sizeof(float);
            float[] result = new float[resultLength];

            fixed (byte* pByte = data)
            {
                var pFloat = (float*)pByte;
                for (int i = 0; i < resultLength; i++)
                {
                    result[i] = pFloat[i];
                }
            }

            return result;
        }
    }
}
