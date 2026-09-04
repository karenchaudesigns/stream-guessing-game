using System;
using System.Threading.Tasks;

public class CPHInline
{
    public bool Execute()
    {
        // Start the demo background task
        Task.Run(async () => {

            // Check if global state is inactive before starting to avoid overlapping cycles.
            // But since this is a demo, we will just force it.

            // 1. Inactive
            CPH.SetGlobalVar("guessing-game_state", "inactive", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"inactive\"}");
            CPH.SendMessage("Demo: State is INACTIVE");

            await Task.Delay(10000);

            // 2. Waiting
            CPH.SetGlobalVar("guessing-game_state", "waiting", true);
            CPH.SetGlobalVar("guessing-game_currentGuest", "DemoGoose", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"waiting\"}");
            CPH.SendMessage("Demo: State is WAITING");

            await Task.Delay(10000);

            // 3. Active
            CPH.SetGlobalVar("guessing-game_state", "active", true);
            CPH.SetGlobalVar("guessing-game_timerStatus", "running", true);
            CPH.SetGlobalVar("guessing-game_secretWord", "DEMOWORD", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"active\"}");
            CPH.WebsocketBroadcastString("{\"event\":{\"source\":\"General\",\"type\":\"Custom\"},\"data\":{\"name\":\"GameStart\",\"arguments\":{}}}");
            CPH.SendMessage("Demo: State is ACTIVE");

            await Task.Delay(10000);

            // 4. Recap
            CPH.SetGlobalVar("guessing-game_state", "recap", true);
            CPH.SetGlobalVar("guessing-game_winner", "DemoWinner", true);
            CPH.SetGlobalVar("guessing-game_winningWord", "DEMOWORD", true);
            CPH.SetGlobalVar("guessing-game_timerStatus", "stopped", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"recap\"}");
            CPH.SendMessage("Demo: State is RECAP");

            await Task.Delay(10000);

            // 5. Back to Inactive
            CPH.SetGlobalVar("guessing-game_state", "inactive", true);
            CPH.WebsocketBroadcastString("{\"event\":\"state_change\",\"state\":\"inactive\"}");
            CPH.SendMessage("Demo: Cycle Complete. State is INACTIVE");
        });

        return true;
    }
}
