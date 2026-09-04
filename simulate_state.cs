using System;

public class CPHInline
{
    public bool Execute()
    {
        // Get the command to simulate from rawInput (e.g., !simulate win)
        CPH.TryGetArg("rawInput", out string rawInput);
        string command = string.IsNullOrEmpty(rawInput) ? "" : rawInput.Trim().ToLower();

        if (string.IsNullOrEmpty(command))
        {
            CPH.SendMessage("Please specify a simulation command: win, timeout, forfeit, active, inactive, waiting");
            return false;
        }

        if (command == "win")
        {
            CPH.SetGlobalVar("guessing-game_state", "recap", true);
            CPH.SetGlobalVar("guessing-game_winner", "MockWinner", true);
            CPH.SetGlobalVar("guessing-game_winningWord", "SIMULATED", true);
            CPH.SetGlobalVar("guessing-game_timerStatus", "stopped", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"recap\"}");
            CPH.SendMessage("Simulated game state: WIN");
        }
        else if (command == "timeout")
        {
            CPH.SetGlobalVar("guessing-game_state", "recap", true);
            CPH.SetGlobalVar("guessing-game_winner", "Time's up", true);
            CPH.SetGlobalVar("guessing-game_winningWord", "SIMULATED", true);
            CPH.SetGlobalVar("guessing-game_timerStatus", "stopped", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"recap\"}");
            CPH.SendMessage("Simulated game state: TIMEOUT");
        }
        else if (command == "forfeit")
        {
            string currentGuest = CPH.GetGlobalVar<string>("guessing-game_currentGuest", true);
            if (string.IsNullOrEmpty(currentGuest)) currentGuest = "MockGoose";

            CPH.SetGlobalVar("guessing-game_state", "recap", true);
            CPH.SetGlobalVar("guessing-game_winner", $"{currentGuest} forfeited", true);
            CPH.SetGlobalVar("guessing-game_winningWord", "SIMULATED", true);
            CPH.SetGlobalVar("guessing-game_timerStatus", "stopped", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"recap\"}");
            CPH.SendMessage("Simulated game state: FORFEIT");
        }
        else if (command == "active")
        {
            CPH.SetGlobalVar("guessing-game_state", "active", true);
            CPH.SetGlobalVar("guessing-game_timerStatus", "running", true);
            CPH.SetGlobalVar("guessing-game_secretWord", "SIMULATED", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"active\"}");
            CPH.SendMessage("Simulated game state: ACTIVE");
        }
        else if (command == "waiting")
        {
            CPH.SetGlobalVar("guessing-game_state", "waiting", true);
            CPH.SetGlobalVar("guessing-game_currentGuest", "MockGoose", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"waiting\"}");
            CPH.SendMessage("Simulated game state: WAITING");
        }
        else if (command == "inactive")
        {
            CPH.SetGlobalVar("guessing-game_state", "inactive", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"inactive\"}");
            CPH.SendMessage("Simulated game state: INACTIVE");
        }
        else
        {
            CPH.SendMessage($"Unknown simulation command: {command}");
            return false;
        }

        return true;
    }
}
