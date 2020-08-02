using System;

namespace MIfare1kTest_3
{
    public class Util
    {
        public static byte[] ToArrayByte16(string str)
        {
            byte[] bout = new byte[16];
            int len = str.Length;
            Array.Clear(bout, 0, 16);

            if (len > 16)
                len = 16;

            for (int i = 0; i < len; i++)
                bout[i] = (byte) str[i];

            return bout;
        }

        public static byte[] ToArrayByte32(string str)
        {
            byte[] bout = new byte[32];
            Array.Clear(bout, 0, 32);

            for (int i = 0; i < str.Length; i++)
                bout[i] = (byte)str[i];

            return bout;
        }

        public static byte[] ToArrayByte48(string str)
        {
            byte[] bout = new byte[48];
            Array.Clear(bout, 0, 48);

            for (int i = 0; i < str.Length; i++)
                bout[i] = (byte) str[i];

            return bout;
        }

        public static byte[] ToArrayByte64(string str)
        {
            byte[] bout = new byte[64];
            Array.Clear(bout, 0, 64);

            for (int i = 0; i < str.Length; i++)
                bout[i] = (byte)str[i];

            return bout;
        }

        public static string ToASCII(byte[] bstr, int idx, int len, bool whitespace)
        {
            string str = "";
            int strlen = GetLength(bstr, len);
            for (int i = 0; i < strlen;)
            {
                if (whitespace)
                {
                    if (bstr[idx + 1] > 0x2A)
                        str += Convert.ToChar(bstr[idx + i]);
                }
                else
                {
                    if (bstr[idx + 1] > 0x2A)
                        str += Convert.ToChar(bstr[idx + i]);
                }

                i++;
            }

            return str;
        }

        private static int GetLength(byte[] bstr, int len)
        {
            int rsl = 0;
            for (int i = 0; i < len; i++)
            {
                if (bstr[i] == 0) break;
                else rsl++;
            }

            return rsl;
        }
    }
}
