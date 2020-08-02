using System;
using System.Diagnostics;
using System.Linq;
using PCSC;
using PCSC.Iso7816;

namespace MIfare1kTest_3
{
    public class MifareCard
    {
        private const byte CUSTOM_CLA = 0xFF;
        private readonly IIsoReader _isoReader;

        public MifareCard(IIsoReader isoReader)
        {
            _isoReader = isoReader ?? throw new ArgumentNullException(nameof(isoReader));
        }

        public bool LoadKey(KeyStructure keyStructure, byte keyNumber, byte[] key)
        {
            var loadKeyCmd = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
            {
                CLA =  CUSTOM_CLA,
                Instruction = InstructionCode.ExternalAuthenticate,
                P1 = (byte)keyStructure,
                P2 = keyNumber,
                Data = key
            };

            Debug.WriteLine($"Load Authentication Keys: {BitConverter.ToString(loadKeyCmd.ToArray())}");
            var response = _isoReader.Transmit(loadKeyCmd);
            Debug.WriteLine($"SW1 SW2 = {response.SW1:X2} {response.SW2:X2}");

            return IsSuccess(response);
        }

        public bool Authenticate(byte msb, byte lsb, KeyType keyType, byte keyNumber)
        {
            var authBlock = new GeneralAuthenticate
            {
                KeyNumber =  keyNumber,
                KeyType = keyType,
                Lsb = lsb,
                Msb = msb
            };

            var authKeyCmd = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
            {
                CLA = CUSTOM_CLA,
                Instruction = InstructionCode.InternalAuthenticate,
                P1 = 0x00,
                P2 = 0x00,
                Data = authBlock.ToArray()
            };

            Debug.WriteLine($"General Authenticate: {BitConverter.ToString(authKeyCmd.ToArray())}");
            var respon = _isoReader.Transmit(authKeyCmd);
            Debug.WriteLine($"SW1 SW2 = {respon.SW1:X2} {respon.SW1:X2}");

            return IsSuccess(respon);
        }

        public byte[] ReadBinary(byte msb, byte lsb, int size)
        {
            unchecked
            {
                var readBinaryCmd = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Any)
                {
                    CLA = CUSTOM_CLA,
                    Instruction = InstructionCode.ReadBinary,
                    P1 = msb,
                    P2 = lsb,
                    Le = size
                };

                Debug.WriteLine($"Read Binary: {BitConverter.ToString(readBinaryCmd.ToArray())}");
                var response = _isoReader.Transmit(readBinaryCmd);
                Debug.WriteLine($"SW1 SW2 = {response.SW1:X2} {response.SW2:X2} \nData = {BitConverter.ToString(response.GetData().ToArray())}.");

                return IsSuccess(response)
                    ? response.GetData() ?? new byte[0]
                    : null;
            }
        }

        public bool UpdateBinary(byte msb, byte lsb, byte[] data)
        {
            var updateBinaryCmd = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
            {
                CLA = CUSTOM_CLA,
                Instruction = InstructionCode.UpdateBinary,
                P1 = msb,
                P2 = lsb,
                Data = data
            };

            Debug.WriteLine($"Update Binary: {BitConverter.ToString(updateBinaryCmd.ToArray())}");
            var response = _isoReader.Transmit(updateBinaryCmd);
            Debug.WriteLine($"SW1 SW2 = {response.SW1:X2} {response.SW2:X2}");

            return IsSuccess(response);
        }

        private static bool IsSuccess(Response response) => (response.SW1 == (byte) SW1Code.Normal) && (response.SW2 == 0x00);
    }
}
