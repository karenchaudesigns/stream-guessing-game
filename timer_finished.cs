using System;
using System.Threading.Tasks;

public class CPHInline
{
    public bool Execute()
    {
        string secretWord = CPH.GetGlobalVar<string>("guessing-game_secretWord", true);

        if (!string.IsNullOrEmpty(secretWord))
        {
            CPH.SetGlobalVar("guessing-game_state", "recap", true);
            CPH.SetGlobalVar("guessing-game_winner", "Time's up", true);
            CPH.SetGlobalVar("guessing-game_winningWord", secretWord, true);
            CPH.SetGlobalVar("guessing-game_secretWord", "", true);

            CPH.SendMessage($"⏳ Time's up! No one guessed the word. The secret word was {secretWord}.");

            string statePayload = "{\"event\":\"state_change\",\"state\":\"recap\"}";
            CPH.WebsocketBroadcastString(statePayload);

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

        return true;
    }
}
