using System;
using System.Net.Http;

public class CPHInline
{
    public bool Execute()
    {
        // 1. Get the guest's username from your command (e.g., !giveword guestname)
        string guestUser = args.ContainsKey("rawInput") ? args["rawInput"].ToString().Trim().Replace("@", "").ToLower() : "";
        
        if (string.IsNullOrEmpty(guestUser)) {
            CPH.SendMessage("Please specify a guest: !giveword username");
            return false;
        }

        try 
        {
            using (HttpClient client = new HttpClient())
            {
                // 2. Fetch 3 random words from a free API
                string response = client.GetStringAsync("https://random-word-api.herokuapp.com/word?number=3").Result;
                
                // Clean up the JSON array formatting
                response = response.Replace("[", "").Replace("]", "").Replace("\"", "");
                string[] words = response.Split(',');

                if(words.Length >= 3) 
                {
                    // 3. Save options globally. Capitalizing for visual clarity and comparison later.
                    CPH.SetGlobalVar("guestOptions_A", words[0].ToUpper(), true);
                    CPH.SetGlobalVar("guestOptions_B", words[1].ToUpper(), true);
                    CPH.SetGlobalVar("guestOptions_C", words[2].ToUpper(), true);
                    
                    CPH.SetGlobalVar("currentGuest", guestUser, true);
                    CPH.SetGlobalVar("secretWord", "", true); // Clear any old word out

                    // 4. Send the whisper
                    string message = $"Guessing Game! Reply to this whisper with A, B, or C.  A) {words[0].ToUpper()}   B) {words[1].ToUpper()}   C) {words[2].ToUpper()}";
                    CPH.SendWhisper(guestUser, message, true);
                    
                    // 5. Let chat know we are waiting
                    CPH.SendMessage($"Sent 3 secret options to @{guestUser}. Waiting for them to lock in their choice...");
                }
            }
        }
        catch (Exception ex) 
        {
            CPH.LogInfo("Word Fetch Error: " + ex.Message);
            CPH.SendMessage("Error fetching words. Check Streamer.bot logs.");
        }
        
        return true;
    }
}