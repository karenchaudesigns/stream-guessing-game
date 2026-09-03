using System;
using System.Threading.Tasks;

public class CPHInline
{
    public bool Execute()
    {
        // 1. Grab chat message and user data
        CPH.TryGetArg("message", out string msgArg);
        string originalMessage = string.IsNullOrEmpty(msgArg) ? "" : msgArg.Trim();
        string chatMessage = originalMessage.ToUpper();

        CPH.TryGetArg("userName", out string userArg);
        string chatUser = string.IsNullOrEmpty(userArg) ? "Someone" : userArg;
        
        string userColor = "#ffffff";
        if (CPH.TryGetArg("userColor", out string uColor) && !string.IsNullOrEmpty(uColor)) userColor = uColor;
        else if (CPH.TryGetArg("color", out string colorArg) && !string.IsNullOrEmpty(colorArg)) userColor = colorArg;

        // 2. Get the current active secret word and goose
        string secretWord = CPH.GetGlobalVar<string>("guessing-game_secretWord", true);
        string currentGoose = CPH.GetGlobalVar<string>("guessing-game_currentGuest", true);

        // If the game isn't active (word is empty or null), do nothing
        if (string.IsNullOrEmpty(secretWord)) return true;

        // 3. Only process as a guess if it's a single word
        if (!originalMessage.Contains(" "))
        {
            // Track participant for active game
            string participantsList = CPH.GetGlobalVar<string>("guessing-game_participants", true);
            if (string.IsNullOrEmpty(participantsList))
            {
                CPH.SetGlobalVar("guessing-game_participants", chatUser, true);
            }
            else
            {
                var participants = new System.Collections.Generic.List<string>(participantsList.Split(','));
                if (!participants.Contains(chatUser))
                {
                    participants.Add(chatUser);
                    CPH.SetGlobalVar("guessing-game_participants", string.Join(",", participants), true);
                }
            }

            bool isCorrect = (chatMessage == secretWord);

            // Broadcast the guess for the overlay
            string guessPayload = $"{{\"event\":\"guess\",\"user\":\"{chatUser}\",\"color\":\"{userColor}\",\"word\":\"{originalMessage}\",\"isCorrect\":{isCorrect.ToString().ToLower()}}}";
            CPH.WebsocketBroadcastString(guessPayload);

            // Check for a match
            if (isCorrect)
            {
                if (chatUser.ToLower() == currentGoose?.ToLower())
                {
                    // GOOSE GAVE UP!
                    CPH.SetGlobalVar("guessing-game_secretWord", "", true);
                    CPH.SetGlobalVar("guessing-game_state", "recap", true);
                    CPH.SetGlobalVar("guessing-game_timerStatus", "stopped", true);
                    CPH.SetGlobalVar("guessing-game_winner", $"{currentGoose} forfeited", true);
                    CPH.SetGlobalVar("guessing-game_winningWord", secretWord, true);

                    CPH.SendMessage($"❌ The Goose (@{chatUser}) gave up! The word was {secretWord}. No one wins this round.");

                    string giveUpPayload = $"{{\"event\":\"give_up\",\"word\":\"{secretWord}\"}}";
                    CPH.WebsocketBroadcastString(giveUpPayload);

                    string statePayload = "{\"event\":\"state_change\",\"state\":\"recap\"}";
                    CPH.WebsocketBroadcastString(statePayload);
                }
                else
                {
                    // WE HAVE A WINNER (DUCK)!
                    CPH.SetGlobalVar("guessing-game_secretWord", "", true);
                    CPH.SetGlobalVar("guessing-game_state", "recap", true);
                    CPH.SetGlobalVar("guessing-game_timerStatus", "stopped", true);
                    CPH.SetGlobalVar("guessing-game_winner", chatUser, true);
                    CPH.SetGlobalVar("guessing-game_winningWord", secretWord, true);

                    // Send success message to chat
                    CPH.SendMessage($"🏆 @{chatUser} GOT IT! The word was {secretWord}! 🏆");

                    // Format a JSON message to broadcast to our OBS HTML overlay
                    int pointsAwarded = 10;
                    int currentPoints = CPH.GetTwitchUserVar<int>(chatUser, "guessing-game_score", true);
                    int newPoints = currentPoints + pointsAwarded;
                    CPH.SetTwitchUserVar(chatUser, "guessing-game_score", newPoints, true);

                    // Update leaderboard global variable
                    string leaderboardData = CPH.GetGlobalVar<string>("guessing-game_leaderboard_data", true);
                    var scores = new System.Collections.Generic.Dictionary<string, int>();
                    var colors = new System.Collections.Generic.Dictionary<string, string>();

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

                    scores[chatUser] = newPoints;
                    colors[chatUser] = userColor;

                    var sortedList = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, int>>(scores);
                    sortedList.Sort((a, b) => b.Value.CompareTo(a.Value));

                    var dataParts = new System.Collections.Generic.List<string>();
                    var jsonParts = new System.Collections.Generic.List<string>();
                    foreach (var kvp in sortedList)
                    {
                        string entryColor = colors.ContainsKey(kvp.Key) ? colors[kvp.Key] : "#ffffff";
                        dataParts.Add($"{kvp.Key}:{kvp.Value}:{entryColor}");
                        jsonParts.Add($"{{\"username\":\"{kvp.Key}\",\"score\":{kvp.Value},\"color\":\"{entryColor}\"}}");
                    }

                    CPH.SetGlobalVar("guessing-game_leaderboard_data", string.Join(",", dataParts), true);
                    CPH.SetGlobalVar("guessing-game_leaderboard", "[" + string.Join(",", jsonParts) + "]", true);

                    string winnerPayload = $"{{\"event\":\"winner\",\"user\":\"{chatUser}\",\"word\":\"{secretWord}\",\"points\":{pointsAwarded}}}";

                    // Broadcast via Streamer.bot WebSocket Server
                    CPH.WebsocketBroadcastString(winnerPayload);

                    string statePayload = "{\"event\":\"state_change\",\"state\":\"recap\"}";
                    CPH.WebsocketBroadcastString(statePayload);
                }

                int recapDuration = 60;
                try {
                    string configPath = "config.json";
                    if (System.IO.File.Exists(configPath)) {
                        string configText = System.IO.File.ReadAllText(configPath);
                        string searchString = "\"recapDurationSeconds\"";
                        int idx = configText.IndexOf(searchString);
                        if (idx != -1) {
                            int colonIdx = configText.IndexOf(':', idx + searchString.Length);
                            if (colonIdx != -1) {
                                int endIdx = configText.IndexOfAny(new char[] { ',', '}', '\n', '\r' }, colonIdx + 1);
                                if (endIdx == -1) endIdx = configText.Length;

                                string valString = configText.Substring(colonIdx + 1, endIdx - (colonIdx + 1)).Trim();
                                if (int.TryParse(valString, out int parsedVal)) {
                                    recapDuration = parsedVal;
                                }
                            }
                        }
                    }
                } catch (Exception ex) {
                    CPH.LogInfo("Config Read Error: " + ex.Message);
                }

                if (recapDuration > 0) {
                    Task.Run(async () => {
                        await Task.Delay(recapDuration * 1000);
                        string currentState = CPH.GetGlobalVar<string>("guessing-game_state", true);
                        if (currentState == "recap") {
                            CPH.SetGlobalVar("guessing-game_state", "inactive", true);
                            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"inactive\"}");
                        }
                    });
                }
            }
        }
        
        return true;
    }
}