﻿using System;

namespace PKHeX.Core
{
    public sealed class Bank7 : BulkStorage
    {
        public Bank7(byte[] data, Type t, int start, int slotsPerBox = 30) : base(data, t, start, slotsPerBox)
        {
            Personal = PersonalTable.USUM;
            Version = GameVersion.USUM;
            HeldItems = Legal.HeldItems_USUM;
        }

        protected override string PlayTimeString => $"{Year:00}{Month:00}{Day:00}_{Hours:00}ː{Minutes:00}";
        public override string BAKName => $"{FileName} [{PlayTimeString}].bak";
        private const int GroupNameSize = 0x20;
        private const int BankNameSize = 0x24;
        private const int GroupNameSpacing = GroupNameSize + 2;
        private const int BankNameSpacing = BankNameSize + 2;

        public ulong UID => BitConverter.ToUInt64(Data, 0);
        public string GetGroupName(int group)
        {
            if (group < 0 || group > 10)
                throw new ArgumentException($"{nameof(group)} must be 1-10.");
            int offset = 0x8 + GroupNameSpacing * group + 2; // skip over " "
            return GetString(offset, GroupNameSize / 2);
        }

        public override int BoxCount => BankCount;
        private int BankCount
        {
            get => Data[0x15E];
            set => Data[0x15E] = (byte)value;
        }
        private int Year => BitConverter.ToUInt16(Data, 0x160);
        private int Month => Data[0x162];
        private int Day => Data[0x163];
        private int Hours => Data[0x164];
        private int Minutes => Data[0x165];

        private int BoxDataSize => 30 * SIZE_STORED + BankNameSpacing;
        public override int GetBoxOffset(int box) => Box + BoxDataSize * box;
        public override string GetBoxName(int box) => GetString(GetBoxNameOffset(box), BankNameSize / 2);
        public int GetBoxNameOffset(int box) => GetBoxOffset(box) + 30 * SIZE_STORED;
        public int GetBoxIndex(int box) => BitConverter.ToUInt16(Data, GetBoxNameOffset(box) + BankNameSize);

        private const int BoxStart = 0x17C;
        public static Bank7 GetBank7(byte[] data) => new Bank7(data, typeof(PK7), BoxStart);
    }
}
