using System;

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
        }

        return true;
    }
}
