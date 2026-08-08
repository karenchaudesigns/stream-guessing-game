using System;

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
        string secretWord = CPH.GetGlobalVar<string>("guessing-game_secretWord");
        string currentGoose = CPH.GetGlobalVar<string>("guessing-game_currentGuest");

        // If the game isn't active (word is empty or null), do nothing
        if (string.IsNullOrEmpty(secretWord)) return true;

        // 3. Only process as a guess if it's a single word
        if (!originalMessage.Contains(" "))
        {
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
                CPH.SetGlobalVar("guessing-game_state", "inactive", true);
                CPH.SetGlobalVar("guessing-game_currentGuest", "", true);

                CPH.SendMessage($"❌ The Goose (@{chatUser}) gave up! The word was {secretWord}. No one wins this round.");

                string giveUpPayload = $"{{\"event\":\"give_up\",\"word\":\"{secretWord}\"}}";
                CPH.WebsocketBroadcastString(giveUpPayload);

                string statePayload = "{\"event\":\"state_change\",\"state\":\"inactive\"}";
                CPH.WebsocketBroadcastString(statePayload);
            }
            else
            {
                // WE HAVE A WINNER (DUCK)!
                CPH.SetGlobalVar("guessing-game_secretWord", "", true);
                CPH.SetGlobalVar("guessing-game_state", "inactive", true);
                CPH.SetGlobalVar("guessing-game_currentGuest", "", true);

                // Send success message to chat
                CPH.SendMessage($"🏆 @{chatUser} GOT IT! The word was {secretWord}! 🏆");

                // Format a JSON message to broadcast to our OBS HTML overlay
                int pointsAwarded = 10;
                int currentPoints = CPH.GetGlobalVar<int>($"guessing-game_score_{chatUser}");
                int newPoints = currentPoints + pointsAwarded;
                CPH.SetGlobalVar($"guessing-game_score_{chatUser}", newPoints, true);

                string winnerPayload = $"{{\"event\":\"winner\",\"user\":\"{chatUser}\",\"word\":\"{secretWord}\",\"points\":{pointsAwarded}}}";

                // Broadcast via Streamer.bot WebSocket Server
                CPH.WebsocketBroadcastString(winnerPayload);

                string statePayload = "{\"event\":\"state_change\",\"state\":\"inactive\"}";
                CPH.WebsocketBroadcastString(statePayload);
            }
            }
        }
        
        return true;
    }
}