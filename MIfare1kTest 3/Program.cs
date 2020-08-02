using System;
using System.Collections.Generic;
using PCSC;
using PCSC.Iso7816;

namespace MIfare1kTest_3
{
    class Program
    {
        private const byte Msb = 0x00;
        private const byte BlockFrom = 0x08;
        private const byte BlockTo = 0x0A;

        private static MifareCard card;

        static void Main(string[] args)
        {
            var contextFactory = ContextFactory.Instance;
            using (var context = contextFactory.Establish(SCardScope.System))
            {
                var readerNames = context.GetReaders();
                if (NoReaderAvailabe(readerNames))
                {
                    Console.WriteLine("You need at least one reader in order to run this example.");
                    Console.ReadLine();
                    return;
                }

                var readerName = ChooseReader(readerNames);
                if (readerName == null) return;

                using (var isoReader = new IsoReader(
                    context,
                    (string)readerName,
                    SCardShareMode.Shared,
                    SCardProtocol.Any,
                    false))
                {
                    card = new MifareCard(isoReader);

                    var loadKeySuccess = card.LoadKey(KeyStructure.VolatileMemory, 0x00,
                        new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                    if (!loadKeySuccess) throw new Exception("LOAD KEY Failed");

                    for (byte i = BlockFrom; i <= BlockTo; i++)
                    {
                        if ((i + 1) % 4 == 0) continue;
                        var authSuccess = card.Authenticate(Msb, i, KeyType.KeyA, 0x00);
                        if (authSuccess)
                        {
                            var result = card.ReadBinary(Msb, i, 16);
                            Console.WriteLine("Result(Before Update): {0}",
                                (result != null)
                                    ? Util.ToASCII(result, 0, 16, true)
                                    : null);
                        }
                    }

                    byte[] data = new byte[16];
                    Array.Clear(data, 0, 16);

                    //var updateBinnarySuccess = card.UpdateBinary(Msb, BlockFrom, data);
                    //                    var updateBinnarySuccess = card.UpdateBinary(Msb, BlockFrom, Util.ToArrayByte16("Hello"));
                    //                    if(!updateBinnarySuccess) throw new Exception("UPDATE BINARY Failed");

                    if (!WriteBlockRange(Msb, BlockFrom, BlockTo, Util.ToArrayByte48("Fiqri khoirul Muttaqin")))
                        throw new Exception("UPDATE BINARY Failed");

                    Console.WriteLine("===================================================");

                    Console.WriteLine(BitConverter.ToString(ReadBlockRange(Msb, BlockFrom, BlockTo)));

                    Console.WriteLine("===================================================");
                    byte[] dataNama = ReadBlockRange(Msb, BlockFrom, BlockTo);
                    Console.WriteLine(Util.ToASCII(dataNama, 0, dataNama.Length, false));
                    Console.WriteLine(dataNama.Length.ToString());
                    //                    for (byte i = BlockFrom; i <= BlockTo; i++)
                    //                    {
                    //                        if ((i + 1) % 4 == 0) continue;
                    //                        else
                    //                        {
                    //                            var authSuccess = card.Authenticate(Msb, i, KeyType.KeyA, 0x00);
                    //                            if (authSuccess)
                    //                            {
                    //                                var result = card.ReadBinary(Msb, i, 16);
                    //                                Console.WriteLine("Result(After Update): {0}",
                    //                                    (result != null)
                    //                                        ? Util.ToASCII(result, 0, 16, true)
                    //                                        : null);
                    //                            }
                    //                        }
                    //                    }

                    //                    var authSuccess = card.Authenticate(Msb, BlockFrom, KeyType.KeyA, 0x00);
                    //                    if (!authSuccess) throw new Exception("AUTHENTICATE Failed");
                    //
                    //                    var result = card.ReadBinary(Msb, BlockFrom, 16);
                    //                    Console.WriteLine("Result(Before Update): {0}", 
                    //                        (result != null)
                    //                        ?BitConverter.ToString(result)
                    //                        :null);
                    //
                    //                    byte[] data = new byte[16];
                    //                    Array.Clear(data, 0, 16);
                    //
                    //                    var updateSuccess = card.UpdateBinary(Msb, BlockFrom, data);
                    //                    if(!updateSuccess) throw new Exception("UPDATE BINARY Failed");
                    //
                    //                    result = card.ReadBinary(Msb, BlockFrom, 16);
                    //                    Console.WriteLine("Result(After Update): {0}",
                    //                        (result != null)
                    //                            ? BitConverter.ToString(result)
                    //                            : null);
                }
            }

            Console.ReadLine();
        }

        private static object ChooseReader(IList<string> readerNames)
        {
            Console.WriteLine(new string('=', 79));
            Console.WriteLine("WARNING!! This will overwrite data in MSB {0:X2} LSB {1:X2} using the default key.", Msb,
                BlockFrom);
            Console.WriteLine(new string('=', 79));

            // Show available readers.
            Console.WriteLine("Available readers: ");
            for (var i = 0; i < readerNames.Count; i++)
            {
                Console.WriteLine($"[{i}] {readerNames[i]}");
            }

            // Ask the user which one to choose.
            Console.Write("Which reader has an inserted Mifare 1k/4k card? ");

            var line = Console.ReadLine();

            if (int.TryParse(line, out var choice) && (choice >= 0) && (choice <= readerNames.Count))
            {
                return readerNames[choice];
            }

            Console.WriteLine("An invalid number has been entered.");
            Console.ReadKey();

            return null;
        }

        private static bool NoReaderAvailabe(ICollection<string> readerNames)
        {
            return readerNames == null || readerNames.Count < 1;
        }

        private static bool WriteBlock(byte msb, byte lsb, byte[] data)
        {
            var authSuccess = card.Authenticate(Msb, lsb, KeyType.KeyA, 0x00);
            if (authSuccess)
            {
                //var updateBinarySuccess = card.UpdateBinary(Msb, i, data);
                var updateBinarySuccess = card.UpdateBinary(Msb, lsb, data);
                if (updateBinarySuccess) return true;
                return false;
            }

            return false;
        }

        private static bool WriteBlockRange(byte msb, byte blockFrom, byte blockTo, byte[] data)
        {
            byte i;
            int count = 0;
            byte[] blockdata = new byte[16];

            for (i = blockFrom; i <= blockTo; i++)
            {
                if ((i + 1) % 4 == 0) continue;
                else
                {
                    Array.Copy(data, count * 16, blockdata, 0, 16);
                    if (WriteBlock(msb, i, blockdata)) count++;
                    else return false;
                }
            }

            return true;
        }

        public static byte[] ReadBlock(byte msb, byte lsb)
        {
            byte[] readBinary = new byte[] { };
            var authSuccess = card.Authenticate(msb, lsb, KeyType.KeyA, 0x00);
            if (authSuccess)
            {
                readBinary = card.ReadBinary(msb, lsb, 16);
            }

            return readBinary;
        }

        private static byte[] ReadBlockRange(byte msb, byte blockFrom, byte blockTo)
        {
            byte i;
            int nBlock = 0;
            int count = 0;
            byte[] blockData = new byte[16];
            byte[] dataOut;

            for (i = blockFrom; i <= blockTo; i++)
            {
                if (((i + 1) % 4) == 0) continue;
                else nBlock++;
            }

            dataOut = new byte[nBlock * 16];
            for (i = blockFrom; i <= blockTo; i++)
            {
                if (((i + 1) % 4) == 0) continue;
                else
                {
                    Array.Copy(ReadBlock(msb, i), 0, dataOut, count * 16, 16);
                    count++;
                }
            }

            return dataOut;
        }
    }
}
