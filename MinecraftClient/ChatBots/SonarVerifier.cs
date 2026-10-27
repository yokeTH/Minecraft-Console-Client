using System;
using System.Collections.Generic;
using System.Linq;
using MinecraftClient.Mapping;
using MinecraftClient.Scripting;
using MinecraftClient.Tui;
using Tomlet.Attributes;

namespace MinecraftClient.ChatBots
{
    /// <summary>
    /// Shows Sonar anti-bot map CAPTCHAs to the user and reconnects after Sonar's verification kick
    /// </summary>
    public class SonarVerifier : ChatBot
    {
        public static Configs Config = new();

        [TomlDoNotInlineObject]
        public class Configs
        {
            [NonSerialized]
            private const string BotName = "SonarVerifier";

            public bool Enabled = false;

            [TomlInlineComment("$ChatBot.SonarVerifier.Retries$")]
            public int Retries = 1;

            [TomlInlineComment("$ChatBot.SonarVerifier.Reconnect_Delay$")]
            public int Reconnect_Delay = 6;

            [TomlPrecedingComment("$ChatBot.SonarVerifier.Captcha_Messages$")]
            public string[] Captcha_Messages = ["enter the text in chat that is displayed on the map"];

            [TomlPrecedingComment("$ChatBot.SonarVerifier.Success_Messages$")]
            public string[] Success_Messages = ["successfully passed the verification"];

            [TomlPrecedingComment("$ChatBot.SonarVerifier.Fail_Messages$")]
            public string[] Fail_Messages = ["failed the bot verification", "reconnected too fast"];

            public void OnSettingUpdate()
            {
                Captcha_Messages ??= [];
                Success_Messages ??= [];
                Fail_Messages ??= [];
                Retries = Math.Max(0, Retries);
                Reconnect_Delay = Math.Max(0, Reconnect_Delay);
            }
        }

        // Sonar spawns its check vehicles at Y 3000-3499, above any world's build limit.
        private const double SonarVehicleMinY = 3000;
        // Sessions that stay outside Sonar this long (ticks) count as passed.
        private const int PassedSessionTicks = 600;

        private static int failedAttempts;
        private McMap? captchaMap;
        private bool captchaShown;
        private bool inVerification;
        private int sessionTicks;

        public override void Initialize()
        {
            if (!GetTerrainEnabled() || !GetEntityHandlingEnabled())
                LogToConsole(Translations.bot_sonarVerifier_needs_movement);
        }

        public override void Update()
        {
            if (!inVerification && ++sessionTicks == PassedSessionTicks)
                failedAttempts = 0;
        }

        public override void OnEntitySpawn(Entity entity)
        {
            if (entity.Location.Y >= SonarVehicleMinY && (entity.Type.IsBoat() || entity.Type == EntityType.Minecart))
                inVerification = true;
        }

        public override void GetText(string text)
        {
            if (!ContainsAny(GetVerbatim(text), Config.Captcha_Messages))
                return;

            inVerification = true;
            ShowCaptcha();
        }

        public override void OnMapData(int mapid, byte scale, bool trackingPosition, bool locked, List<MapIcon> icons, byte columnsUpdated, byte rowsUpdated, byte mapCoulmnX, byte mapRowZ, byte[]? colors)
        {
            if (colors is null || columnsUpdated == 0 || rowsUpdated == 0)
                return;

            captchaMap = new McMap
            {
                MapId = mapid,
                Width = columnsUpdated,
                Height = rowsUpdated,
                Colors = colors,
                LastUpdated = DateTime.Now
            };

            // Sonar only sends a map during verification for its CAPTCHA.
            if (inVerification)
                ShowCaptcha();
        }

        public override bool OnDisconnect(DisconnectReason reason, string message)
        {
            // MCC keeps bot instances across reconnects, so reset per-session state here.
            bool wasVerifying = inVerification;
            captchaMap = null;
            captchaShown = false;
            inVerification = false;
            sessionTicks = 0;

            if (reason != DisconnectReason.InGameKick)
                return false;

            message = GetVerbatim(message);
            if (ContainsAny(message, Config.Success_Messages))
            {
                failedAttempts = 0;
                LogToConsole(string.Format(Translations.bot_sonarVerifier_passed, Config.Reconnect_Delay));
                ReconnectToTheServer(delaySeconds: Config.Reconnect_Delay, keepAccountAndServerSettings: true);
                return true;
            }

            if (!wasVerifying && !ContainsAny(message, Config.Fail_Messages))
                return false;

            // Kicks without a known success message count as failures so retries stay below Sonar's blacklist threshold.
            if (failedAttempts >= Config.Retries)
            {
                failedAttempts = 0;
                LogToConsole(Translations.bot_sonarVerifier_giving_up);
                return false;
            }

            failedAttempts++;
            LogToConsole(string.Format(Translations.bot_sonarVerifier_retry, Config.Reconnect_Delay, Config.Retries - failedAttempts));
            ReconnectToTheServer(delaySeconds: Config.Reconnect_Delay, keepAccountAndServerSettings: true);
            return true;
        }

        private void ShowCaptcha()
        {
            if (captchaMap is null || captchaShown)
                return;

            captchaShown = true;
            if (ConsoleIO.Backend is TuiConsoleBackend)
                Map.RenderInTui(captchaMap);
            else
                Map.RenderInConsole(captchaMap);
            LogToConsole(Translations.bot_sonarVerifier_captcha);
        }

        private static bool ContainsAny(string text, string[] patterns)
        {
            return patterns.Any(pattern => !string.IsNullOrEmpty(pattern)
                && text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }
    }
}
