using System;
using System.Collections.Generic;

public class CPH {
    public static string GetGlobalVar<T>(string name, bool persisted) {
        return "";
    }
    public static T GetTwitchUserVar<T>(string user, string name, bool persisted) {
        return default(T);
    }
    public static void SetTwitchUserVar(string user, string name, object value, bool persisted) {
    }
    public static void SetGlobalVar(string name, object value, bool persisted) {
    }
    public static void SendMessage(string msg) {
    }
    public static void WebsocketBroadcastString(string data) {
    }
    public static void LogInfo(string msg) {
    }
}
