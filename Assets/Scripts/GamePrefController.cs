using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GamePrefController {
    private const string PREF_COINS = "Coins_";

    public static int LoadCoinsForLevel(int level) {
        return PlayerPrefs.GetInt(PREF_COINS + level, 0);
    }

    public static void ClearCoinsForLevels(int maxLevel) {
        for (int i = 1; i <= maxLevel; i++) {
            PlayerPrefs.DeleteKey(PREF_COINS + i);
        }
    }

    public static int GetTotalCoinsForLevels(int maxLevel) {
        int totalCoins = 0;
        for (int i = 1; i <= maxLevel; i++) {
            totalCoins += PlayerPrefs.GetInt(PREF_COINS + i, 0);
        }
        return totalCoins;
    }

    public static void SaveCoinsForLevel(int level, int currentCoins) {
        PlayerPrefs.SetInt(PREF_COINS + level, currentCoins);
    }
}
