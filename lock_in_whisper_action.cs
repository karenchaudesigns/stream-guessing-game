using System;

public class CPHInline
{
    public bool Execute()
    {
        // 1. Check who sent the whisper and what they said
        string whisperSender = args.ContainsKey("user") ? args["user"].ToString().ToLower() : "";
        string message = args.ContainsKey("message") ? args["message"].ToString().Trim().ToUpper() : "";
        
        // 2. Check who our current guest is supposed to be
        string currentGuest = CPH.GetGlobalVar<string>("currentGuest");

        // If no game is pending, or someone else whispered the bot, ignore it
        if (currentGuest == null || whisperSender != currentGuest.ToLower()) return true;

        string lockedWord = "";

        // 3. Match their reply to the saved options
        if (message == "A") lockedWord = CPH.GetGlobalVar<string>("guestOptions_A");
        else if (message == "B") lockedWord = CPH.GetGlobalVar<string>("guestOptions_B");
        else if (message == "C") lockedWord = CPH.GetGlobalVar<string>("guestOptions_C");
        else 
        {
            CPH.SendWhisper(whisperSender, "Invalid choice. Please reply with exactly A, B, or C.", true);
            return false;
        }

        // 4. Lock the final word into the variable for chat to guess!
        CPH.SetGlobalVar("secretWord", lockedWord, true);
        
        // 5. Clear the guest variable so they can't change it
        CPH.SetGlobalVar("currentGuest", "", true);

        // 6. Confirm with the guest & announce to stream
        CPH.SendWhisper(whisperSender, $"Locked in! Your word is: {lockedWord}. Start creating!", true);
        CPH.SendMessage($"The secret word is locked! The game has started. First person to guess it in chat wins!");
        
        return true;
    }
}