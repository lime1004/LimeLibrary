using UnityEngine;

namespace LimeLibrary.Utility {

public static class MathUtility {
  public const float PerlinNoisePeriod = 256f;

  public static int SeparateBit32(int n) {
    n = (n | (n << 8)) & 0x00ff00ff;
    n = (n | (n << 4)) & 0x0f0f0f0f;
    n = (n | (n << 2)) & 0x33333333;
    return (n | (n << 1)) & 0x55555555;
  }

  public static int GetMortonNumberAABB(int minNumber, int maxNumber) {
    int xor = minNumber ^ maxNumber;
    int shift = 0;
    int i = 0;
    while (xor != 0) {
      if ((xor & 0x3) != 0) {
        int spaceIndex = i + 1;
        shift = spaceIndex * 2;
      }

      xor >>= 2;
      i++;
    }

    return maxNumber >> shift;
  }

  public static bool IsSameMortonNumber(int number1, int number2) {
    int level1 = GetMortonNumberLevel(number1);
    int level2 = GetMortonNumberLevel(number2);
    int lowerLevel = Mathf.Min(level1, level2);
    return GetMortonNumber(number1, lowerLevel) == GetMortonNumber(number2, lowerLevel);
  }

  private static int GetMortonNumberLevel(int number) {
    int level = 0;
    while (number != 0) {
      number >>= 2;
      level++;
    }
    return level;
  }

  private static int GetMortonNumber(int number, int level) {
    if (level == 0) return 0;
    int nowLevel = GetMortonNumberLevel(number);
    if (nowLevel <= level) return number;
    for (int i = 0; i < nowLevel - level; i++) {
      number >>= 2;
    }
    return number;
  }
}

}