using System;
using System.Threading.Tasks;

public class CPHInline
{
    public bool Execute()
    {
        Task.Run(async () => {
            string[] clues = {
                "First", "Second", "Third", "Fourth",
                "Fifth", "Sixth", "Seventh", "Eighth",
                "Ninth", "Tenth", "Eleventh", "Twelfth"
            };

            double clueSpeedMultiplier = 1.5; // default
            try {
                string configPath = "config.json";
                if (System.IO.File.Exists(configPath)) {
                    string configText = System.IO.File.ReadAllText(configPath);
                    string searchString = "\"clueFloatSpeedMultiplier\"";
                    int idx = configText.IndexOf(searchString);
                    if (idx != -1) {
                        int colonIdx = configText.IndexOf(':', idx + searchString.Length);
                        if (colonIdx != -1) {
                            int endIdx = configText.IndexOfAny(new char[] { ',', '}', '\n', '\r' }, colonIdx + 1);
                            if (endIdx == -1) endIdx = configText.Length;
                            string valString = configText.Substring(colonIdx + 1, endIdx - (colonIdx + 1)).Trim();
                            if (double.TryParse(valString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedVal)) {
                                clueSpeedMultiplier = parsedVal;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                CPH.LogInfo("Config Read Error (Speed): " + ex.Message);
            }

            foreach (string clue in clues)
            {
                string payload = $"{{\"event\":\"goose_clue_msg\",\"user\":\"SimUser\",\"color\":\"#000000\",\"message\":\"{clue}\",\"speed\":{clueSpeedMultiplier.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}";
                CPH.WebsocketBroadcastString(payload);
                await Task.Delay(1000); // 1 second delay between clues
            }
        });
        return true;
    }
}
