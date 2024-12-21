using System;

namespace Affinity_manager.Model
{
    public static class AffinityConverter
    {
        public static bool[] ConvertAffinityMaskToBoolArray(uint affinityMask)
        {
            int size = sizeof(uint) * 8; // Number of bits in a uint
            bool[] boolArray = new bool[size];

            for (int i = 0; i < size; i++)
            {
                boolArray[i] = (affinityMask & (1u << i)) != 0;
            }

            return boolArray;
        }

        public static uint ConvertBoolArrayToAffinityMask(bool[] boolArray)
        {
            if (boolArray.Length > sizeof(uint) * 8)
            {
                throw new ArgumentException("Boolean array is too long to fit in a uint.", nameof(boolArray));
            }

            uint affinityMask = 0;

            for (int i = 0; i < boolArray.Length; i++)
            {
                if (boolArray[i])
                {
                    affinityMask |= (1u << i);
                }
            }

            return affinityMask;
        }

    }
}
