﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Unifiedban.Terminal.Bot.Command
{
    public class Kick : ICommand
    {
        public void Execute(Message message)
        {
            Manager.BotClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);

            if (!Utils.BotTools.IsUserOperator(message.From.Id) &&
                !Utils.ChatTools.IsUserAdmin(message.Chat.Id, message.From.Id))
            {
                MessageQueueManager.EnqueueMessage(
                   new ChatMessage()
                   {
                       Timestamp = DateTime.UtcNow,
                       Chat = message.Chat,
                       ReplyToMessageId = message.MessageId,
                       Text = CacheData.GetTranslation("en", "error_not_auth_command")
                   });
                return;
            }
            int userToKick;

            if (message.ReplyToMessage == null)
            {
                if (message.Text.Split(" ")[1].StartsWith("@"))
                {
                    if (!CacheData.Usernames.Keys.Contains(message.Text.Split(" ")[1].Remove(0, 1)))
                    {
                        MessageQueueManager.EnqueueMessage(
                            new ChatMessage()
                            {
                                Timestamp = DateTime.UtcNow,
                                Chat = message.Chat,
                                Text = CacheData.GetTranslation("en", "kick_command_error_invalidUsername")
                            });
                        return;
                    }
                    userToKick = CacheData.Usernames[message.Text.Split(" ")[1].Remove(0, 1)];
                }
                else
                {
                    bool isValid = int.TryParse(message.Text.Split(" ")[1], out userToKick);
                    if (!isValid)
                    {
                        MessageQueueManager.EnqueueMessage(
                            new ChatMessage()
                            {
                                Timestamp = DateTime.UtcNow,
                                Chat = message.Chat,
                                Text = CacheData.GetTranslation("en", "kick_command_error_invalidUserId")
                            });
                        return;
                    }
                }
            }
            else
                userToKick = message.ReplyToMessage.From.Id;

            try
            {
                Manager.BotClient.KickChatMemberAsync(message.Chat.Id, userToKick);
                if (message.Chat.Type == ChatType.Supergroup)
                    Manager.BotClient.UnbanChatMemberAsync(message.Chat.Id, userToKick);
            }
            catch
            {
                MessageQueueManager.EnqueueMessage(
                   new ChatMessage()
                   {
                       Timestamp = DateTime.UtcNow,
                       Chat = message.Chat,
                       ParseMode = ParseMode.Markdown,
                       Text = CacheData.GetTranslation("en", "command_kick_error")
                   });
            }
        }

        public void Execute(CallbackQuery callbackQuery) { }
    }
}
