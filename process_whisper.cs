using System;

public class CPHInline
{
    public bool Execute()
    {
        // 1. Check who sent the whisper and what they said
        CPH.TryGetArg("userName", out string userArg);
        string whisperSender = string.IsNullOrEmpty(userArg) ? "" : userArg.ToLower();

        CPH.TryGetArg("message", out string msgArg);
        string message = string.IsNullOrEmpty(msgArg) ? "" : msgArg.Trim().ToUpper();
        
        // 2. Check who our current Goose is supposed to be
        string currentGuest = CPH.GetGlobalVar<string>("guessing-game_currentGuest");

        // If no game is pending, or someone else whispered the bot, ignore it
        if (string.IsNullOrEmpty(currentGuest) || whisperSender != currentGuest.ToLower()) return true;

        string lockedWord = "";

        // 3. Match their reply to the saved options
        if (message.StartsWith("A")) lockedWord = CPH.GetGlobalVar<string>("guessing-game_guestOptions_A");
        else if (message.StartsWith("B")) lockedWord = CPH.GetGlobalVar<string>("guessing-game_guestOptions_B");
        else if (message.StartsWith("C")) lockedWord = CPH.GetGlobalVar<string>("guessing-game_guestOptions_C");
        else 
        {
            CPH.SendWhisper(whisperSender, "Invalid choice. Please reply with exactly A, B, or C.", true);
            return false;
        }

        // 4. Lock the final word into the variable for chat to guess!
        CPH.SetGlobalVar("guessing-game_secretWord", lockedWord, true);
        CPH.SetGlobalVar("guessing-game_secretWord", lockedWord, false);
        
        // 5. Update state and broadcast
        CPH.SetGlobalVar("guessing-game_state", "active", true);
        CPH.SetGlobalVar("guessing-game_state", "active", false);
        CPH.SetGlobalVar("guessing-game_timerStatus", "running", true);
        CPH.SetGlobalVar("guessing-game_timerStatus", "running", false);

        // 6. Confirm with the Goose & announce to stream
        CPH.SendWhisper(whisperSender, $"Locked in! Your word is: {lockedWord}. Start creating!", true);
        CPH.SendMessage($"The secret word is locked! The game has started. First duck to guess it in chat wins!");
        
        string jsonPayload = "{\"event\":\"state_change\",\"state\":\"active\"}";
        CPH.WebsocketBroadcastString(jsonPayload);

        // 7. Broadcast custom GameStart event for timer
        int timerDuration = 2; // Default to 2 minutes

        string timerDurationStr = CPH.GetGlobalVar<string>("guessing-game_timerDuration", true);
        if (string.IsNullOrEmpty(timerDurationStr))
        {
            timerDurationStr = CPH.GetGlobalVar<string>("guessing-game_timerDuration", false);
        }

        if (!string.IsNullOrEmpty(timerDurationStr) && int.TryParse(timerDurationStr, out int parsedStrDuration))
        {
            timerDuration = parsedStrDuration;
        }
        else
        {
            int timerDurationInt = CPH.GetGlobalVar<int>("guessing-game_timerDuration", true);
            if (timerDurationInt == 0)
            {
                timerDurationInt = CPH.GetGlobalVar<int>("guessing-game_timerDuration", false);
            }
            if (timerDurationInt > 0)
            {
                timerDuration = timerDurationInt;
            }
        }

        string gameStartPayload = $"{{\"event\":{{\"source\":\"General\",\"type\":\"Custom\"}},\"data\":{{\"name\":\"GameStart\",\"arguments\":{{\"guessing-game_timerDuration\":{timerDuration},\"guessing-game_secretWord\":\"{lockedWord}\"}}}}}}";
        CPH.WebsocketBroadcastString(gameStartPayload);

        return true;
    }
}