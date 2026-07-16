using System;

public class CPHInline
{
    public bool Execute()
    {
        // 1. Grab chat message and user data
        string chatMessage = args.ContainsKey("message") ? args["message"].ToString().Trim().ToUpper() : "";
        string chatUser = args.ContainsKey("userName") ? args["userName"].ToString() : "Someone";
        
        // 2. Get the current active secret word
        string secretWord = CPH.GetGlobalVar<string>("secretWord");

        // If the game isn't active (word is empty or null), do nothing
        if (string.IsNullOrEmpty(secretWord)) return true;

        // 3. Check for a match
        if (chatMessage == secretWord)
        {
            // WE HAVE A WINNER!
            
            // Immediately clear the word so nobody else wins this round
            CPH.SetGlobalVar("secretWord", "", true); 

            // Send success message to chat
            CPH.SendMessage($"🏆 @{chatUser} GOT IT! The word was {secretWord}! 🏆");

            // Format a JSON message to broadcast to our OBS HTML overlay
            int pointsAwarded = 10;
            string jsonPayload = $"{{\"event\":\"winner\",\"user\":\"{chatUser}\",\"word\":\"{secretWord}\",\"points\":{pointsAwarded}}}";
            
            // Broadcast via Streamer.bot WebSocket Server
            CPH.WebsocketBroadcastJson(jsonPayload);
        }
        
        return true;
    }
}