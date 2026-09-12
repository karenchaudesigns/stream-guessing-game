using System;
using System.IO;

public class CPHInline
{
    public bool Execute()
    {
        // 1. Get the Goose's username from your command (e.g., !giveword goosename)

        // 0. Load leaderboard configuration
        double leaderboardScrollSpeedMultiplier = 1.0;
        int leaderboardScrollGapPx = 50;
        try {
            string configPath = "config.json";
            if (System.IO.File.Exists(configPath)) {
                string configText = System.IO.File.ReadAllText(configPath);

                string searchString = "\"leaderboardScrollSpeedMultiplier\"";
                int idx = configText.IndexOf(searchString);
                if (idx != -1) {
                    int colonIdx = configText.IndexOf(':', idx + searchString.Length);
                    if (colonIdx != -1) {
                        int endIdx = configText.IndexOfAny(new char[] { ',', '}', '\n', '\r' }, colonIdx + 1);
                        if (endIdx == -1) endIdx = configText.Length;
                        string valString = configText.Substring(colonIdx + 1, endIdx - (colonIdx + 1)).Trim();
                        if (double.TryParse(valString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedVal)) {
                            leaderboardScrollSpeedMultiplier = parsedVal;
                        }
                    }
                }

                string searchStringGap = "\"leaderboardScrollGapPx\"";
                int idxGap = configText.IndexOf(searchStringGap);
                if (idxGap != -1) {
                    int colonIdx = configText.IndexOf(':', idxGap + searchStringGap.Length);
                    if (colonIdx != -1) {
                        int endIdx = configText.IndexOfAny(new char[] { ',', '}', '\n', '\r' }, colonIdx + 1);
                        if (endIdx == -1) endIdx = configText.Length;
                        string valString = configText.Substring(colonIdx + 1, endIdx - (colonIdx + 1)).Trim();
                        if (int.TryParse(valString, out int parsedVal)) {
                            leaderboardScrollGapPx = parsedVal;
                        }
                    }
                }
            }
        } catch (Exception ex) {
            CPH.LogInfo("Config Read Error (Leaderboard): " + ex.Message);
        }

        CPH.SetGlobalVar("guessing-game_leaderboardScrollSpeedMultiplier", leaderboardScrollSpeedMultiplier, true);
        CPH.SetGlobalVar("guessing-game_leaderboardScrollGapPx", leaderboardScrollGapPx, true);
        CPH.TryGetArg("rawInput", out string rawInput);
        string guestUser = string.IsNullOrEmpty(rawInput) ? "" : rawInput.Trim().Replace("@", "").ToLower();
        
        if (string.IsNullOrEmpty(guestUser)) {
            CPH.SendMessage("Please specify a Goose: !giveword username");
            return false;
        }

        try 
        {
            // 2. Fetch words from the local words.txt file (or custom path)
            CPH.TryGetArg("wordsFilePath", out string customPath);
            string wordsFilePath = string.IsNullOrEmpty(customPath) ? "words.txt" : customPath;
            string[] allWords = File.ReadAllLines(wordsFilePath);

            if(allWords.Length >= 3)
            {
                // Select 3 random distinct words
                Random rnd = new Random();
                string[] words = new string[3];
                for (int i = 0; i < 3; i++)
                {
                    int index = rnd.Next(allWords.Length);
                    words[i] = allWords[index].Trim().ToUpper();
                    // Basic safeguard to avoid duplicates (could be optimized, but fine for n=3)
                    while (i > 0 && Array.IndexOf(words, words[i], 0, i) != -1)
                    {
                        index = rnd.Next(allWords.Length);
                        words[i] = allWords[index].Trim().ToUpper();
                    }
                }

                if(words.Length >= 3) 
                {
                    // 3. Save options globally. Capitalizing for visual clarity and comparison later.
                    CPH.SetGlobalVar("guessing-game_guestOptions_A", words[0], true);
                    CPH.SetGlobalVar("guessing-game_guestOptions_B", words[1], true);
                    CPH.SetGlobalVar("guessing-game_guestOptions_C", words[2], true);
                    
                    CPH.SetGlobalVar("guessing-game_currentGuest", guestUser, true);
                    CPH.SetGlobalVar("guessing-game_secretWord", "", true); // Clear any old word out
                    CPH.SetGlobalVar("guessing-game_state", "waiting", true); // Set game state to waiting
                    CPH.SetGlobalVar("guessing-game_timerStatus", "stopped", true);

                    // 4. Send the whisper
                    string message = $"Duck, Duck, Guess! Reply to this whisper with A, B, or C to lock in the secret word. a) {words[0]}, b) {words[1]}, c) {words[2]}";
                    CPH.SendWhisper(guestUser, message, true);
                    
                    // 5. Let chat know we are waiting
                    CPH.SendMessage($"Sent 3 secret options to our Goose @{guestUser}. Waiting for them to lock in their choice...");

                    // 6. Broadcast state change to overlay
                    string jsonPayload = "{\"event\":\"state_change\",\"state\":\"waiting\"}";
                    CPH.WebsocketBroadcastString(jsonPayload);
                }
            }
        }
        catch (Exception ex) 
        {
            CPH.LogInfo("Word Fetch Error: " + ex.Message);
            CPH.SendMessage("Error fetching words from words.txt.");
        }
        
        return true;
    }
}