using System;

namespace PKHeX.Core
{
    public sealed class BlockInfo3DS : BlockInfo
    {
        public ushort Checksum;
        private int BlockInfoOffset;
        private readonly Func<byte[], int, int, ushort> CheckFunc;

        private BlockInfo3DS(Func<byte[], int, int, ushort> func) => CheckFunc = func;
        private int ChecksumOffset => BlockInfoOffset + 6 + (int)ID * 8;
        private ushort GetChecksum(byte[] data) => CheckFunc(data, Offset, Length);

        protected override bool ChecksumValid(byte[] data)
        {
            ushort chk = GetChecksum(data);
            var old = BitConverter.ToUInt16(data, ChecksumOffset);
            return chk == old;
        }
        protected override void SetChecksum(byte[] data)
        {
            ushort chk = GetChecksum(data);
            BitConverter.GetBytes(chk).CopyTo(data, ChecksumOffset);
        }

        /// <summary>
        /// Gets the <see cref="BlockInfo"/> table for the input <see cref="data"/>.
        /// </summary>
        /// <param name="data">Complete data array</param>
        /// <param name="blockInfoOffset">Offset the <see cref="BlockInfo"/> starts at.</param>
        /// <param name="CheckFunc">Checksum method for validating each block.</param>
        public static BlockInfo[] GetBlockInfoData(byte[] data, out int blockInfoOffset, Func<byte[], int, int, ushort> CheckFunc)
        {
            blockInfoOffset = data.Length - 0x200 + 0x10;
            if (BitConverter.ToUInt32(data, blockInfoOffset) != SaveUtil.BEEF)
                blockInfoOffset -= 0x200; // No savegames have more than 0x3D blocks, maybe in the future?
            int count = (data.Length - blockInfoOffset - 0x8) / 8;
            blockInfoOffset += 4;

            var Blocks = new BlockInfo[count];
            int CurrentPosition = 0;
            for (int i = 0; i < Blocks.Length; i++)
            {
                Blocks[i] = new BlockInfo3DS(CheckFunc)
                {
                    Offset = CurrentPosition,
                    Length = BitConverter.ToInt32(data, blockInfoOffset + 0 + 8 * i),
                    ID = BitConverter.ToUInt16(data, blockInfoOffset + 4 + 8 * i),
                    Checksum = BitConverter.ToUInt16(data, blockInfoOffset + 6 + 8 * i),

                    BlockInfoOffset = blockInfoOffset
                };

                // Expand out to nearest 0x200
                var remainder = Blocks[i].Length & 0x1FF;
                CurrentPosition += remainder == 0 ? Blocks[i].Length : Blocks[i].Length + 0x200 - remainder;

                if (Blocks[i].ID != 0 || i == 0)
                    continue;
                count = i;
                break;
            }
            // Fix Final Array Lengths
            Array.Resize(ref Blocks, count);
            return Blocks;
        }
    }
}