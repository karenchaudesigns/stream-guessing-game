using System;
using System.Collections.Generic;

public class CPH {
    public static string GetGlobalVar<T>(string name, bool persisted) { return ""; }
    public static void SetGlobalVar(string name, object value, bool persisted) { }
    public static T GetTwitchUserVar<T>(string user, string name, bool persisted) { return default(T); }
    public static void SetTwitchUserVar(string user, string name, object value, bool persisted) { }
}

public class Test {
    public static void Main() {
        string leaderboardData = "someuser:10:#ffffff";
        var scores = new Dictionary<string, int>();
        var colors = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(leaderboardData))
        {
            foreach (var entry in leaderboardData.Split(','))
            {
                var parts = entry.Split(':');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int score))
                {
                    scores[parts[0]] = score;
                    if (parts.Length >= 3)
                    {
                        colors[parts[0]] = parts[2];
                    }
                }
            }
        }

        string chatUser = "someuser";
        int pointsAwarded = 10;
        int currentPoints = scores.ContainsKey(chatUser) ? scores[chatUser] : 0;
        int newPoints = currentPoints + pointsAwarded;

        CPH.SetTwitchUserVar(chatUser, "guessing-game_score", newPoints, true);
        scores[chatUser] = newPoints;
    }
}
