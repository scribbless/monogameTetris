using System;
using System.Collections.Generic;

namespace MonogameTetris.TetrisLib
{
    public class MovePermutations
    {
        public readonly List<string> MovePermutationsList = new List<string>();

        public MovePermutations(int digitCount)
        {
            MovePermutationsList.Clear();
            for (var i = 0; i < Math.Pow(5, digitCount); i++)
                MovePermutationsList.Add(DecimalToArbitrarySystem(i, 5, digitCount));
        }

        /// <summary>
        ///     Converts the given decimal number to the numeral system with the
        ///     specified radix (in the range [2, 36]).
        /// </summary>
        /// <param name="decimalNumber">The number to convert.</param>
        /// <param name="radix">The radix of the destination numeral system (in the range [2, 36]).</param>
        /// <param name="digitCount">The number of leading zeros to use</param>
        /// <returns></returns>
        private static string DecimalToArbitrarySystem(long decimalNumber, int radix, int digitCount)
        {
            const int bitsInLong = 64;
            const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2 || radix > digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " + digits.Length);

            if (decimalNumber == 0)
                return 0.ToString($"D{digitCount}");

            var index = bitsInLong - 1;
            var currentNumber = Math.Abs(decimalNumber);
            var charArray = new char[bitsInLong];

            while (currentNumber != 0)
            {
                var remainder = (int) (currentNumber % radix);
                charArray[index--] = digits[remainder];
                currentNumber = currentNumber / radix;
            }

            var result = new string(charArray, index + 1, bitsInLong - index - 1);
            if (decimalNumber < 0) result = "-" + result;

            return int.Parse(result).ToString($"D{digitCount}");
        }
    }
}