using System;
using System.Collections.Generic;
using System.Linq;
using MinecraftClient.Dialogs;
using MinecraftClient.Scripting;
using Tomlet.Attributes;

namespace MinecraftClient.ChatBots
{
    /// <summary>
    /// Fills in and submits server dialogs, such as login forms, that match a configured title
    /// </summary>
    public class AutoDialog : ChatBot
    {
        public static Configs Config = new();

        [TomlDoNotInlineObject]
        public class Configs
        {
            [NonSerialized]
            private const string BotName = "AutoDialog";

            public bool Enabled = false;

            [TomlPrecedingComment("$ChatBot.AutoDialog.Rules$")]
            public Rule[] Rules = [new()];

            public void OnSettingUpdate()
            {
                Rules ??= [];
                foreach (Rule rule in Rules)
                    rule.Inputs ??= [];
            }

            public class Rule
            {
                public string Title = "Login";
                public string[] Inputs = ["password=your_password"];
                public int Click = 1;
            }
        }

        private readonly HashSet<Configs.Rule> usedRules = [];

        public override void OnDialogShown(DialogInstance dialog)
        {
            string title = GetVerbatim(dialog.Definition.DisplayTitle());
            Configs.Rule? rule = Config.Rules.FirstOrDefault(rule => !string.IsNullOrEmpty(rule.Title)
                && title.Contains(rule.Title, StringComparison.OrdinalIgnoreCase));
            if (rule is null)
                return;

            // Fill each rule once per connection so a rejected answer is never resubmitted in a loop.
            if (!usedRules.Add(rule))
            {
                LogToConsole(string.Format(Translations.bot_autoDialog_repeated, title));
                return;
            }

            foreach (string input in rule.Inputs)
            {
                int separator = input.IndexOf('=');
                if (separator <= 0)
                    continue;

                DialogActionResult result = Handler.Dialogs.SetInput(input[..separator].Trim(), input[(separator + 1)..]);
                if (!result.Success)
                {
                    LogToConsole(result.Message);
                    return;
                }
            }

            DialogActionResult click = Handler.Dialogs.Click(rule.Click);
            LogToConsole(click.Success ? string.Format(Translations.bot_autoDialog_submitted, title, rule.Click) : click.Message);
        }

        public override bool OnDisconnect(DisconnectReason reason, string message)
        {
            // MCC keeps bot instances across reconnects, so reset per-connection state here.
            usedRules.Clear();
            return false;
        }
    }
}
