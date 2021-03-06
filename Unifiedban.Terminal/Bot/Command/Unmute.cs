﻿using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Unifiedban.Terminal.Bot.Command
{
    public class Unmute : ICommand
    {
        public void Execute(Message message)
        {
            Manager.BotClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);

            if (!Utils.BotTools.IsUserOperator(message.From.Id) &&
                !Utils.ChatTools.IsUserAdmin(message.Chat.Id, message.From.Id))
            {
                MessageQueueManager.EnqueueMessage(
                   new Models.ChatMessage()
                   {
                       Timestamp = DateTime.UtcNow,
                       Chat = message.Chat,
                       ReplyToMessageId = message.MessageId,
                       Text = CacheData.GetTranslation("en", "error_not_auth_command")
                   });
                return;
            }

            int userId = 0;
            if (message.ReplyToMessage != null)
            {
                userId = message.ReplyToMessage.From.Id;
            }
            else if (message.Text.Contains(" "))
            {
                int.TryParse(message.Text.Split(" ")[1], out userId);
            }

            if (userId == 0)
            {
                MessageQueueManager.EnqueueMessage(
               new Models.ChatMessage()
               {
                   Timestamp = DateTime.UtcNow,
                   Chat = message.Chat,
                   ParseMode = ParseMode.Markdown,
                   Text = CacheData.GetTranslation("en", "command_unmute_missingMessage")
               });
                return;
            }

            Manager.BotClient.RestrictChatMemberAsync(
                    message.Chat.Id,
                    userId,
                    new ChatPermissions()
                    {
                        CanSendMessages = true,
                        CanAddWebPagePreviews = true,
                        CanChangeInfo = true,
                        CanInviteUsers = true,
                        CanPinMessages = true,
                        CanSendMediaMessages = true,
                        CanSendOtherMessages = true,
                        CanSendPolls = true
                    });
        }

        public void Execute(CallbackQuery callbackQuery) { }
    }
}
