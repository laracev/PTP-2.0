using Pantry_To_Plate.mods;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;

namespace Pantry_To_Plate
{
    public static class UserDataService
    {
        private static string filePath = "userinfo.json";

        public static void Save(userinfo user)
        {
            AppLogger.Log("Benutzerdaten werden gespeichert");
            string json = JsonSerializer.Serialize(user, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
            AppLogger.Log("Benutzerdaten erfolgreich gespeichert");
        }

        public static userinfo Load()
        {
            AppLogger.Log("Benutzerdaten werden geladen");
            if (!File.Exists(filePath))
            {
                AppLogger.LogError("Keine Benutzerdaten gefunden");
                return new userinfo();
            }

            string json = File.ReadAllText(filePath);

            userinfo user= JsonSerializer.Deserialize<userinfo>(json) ?? new userinfo();
            AppLogger.Log("Benutzerdaten erfolgreich geladen");
            return user;
        }
    }
}
