using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using AdviceBot.Models;
using System;
using AdviceBot.Controllers;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;

namespace AdviceBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        
        private const string EndpointUri = "https://advicebotdb.documents.azure.com:443/";
        private const string PrimaryKey = "DIjubQfc8c96jrUPnmxTDRfe2jcG6iJrXtZZ0XzyGem3hAGmCYu8GEj0NAiYN1mWIZPsh4RAMuoTDShxsx0cMg==";
        private const string databaseName = "AdviceBotDB";
        private const string collectionName = "AdviceCollection";
        DocumentClient client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

        public async Task<Message> Post([FromBody]Message message)
        {

                if (message.Type == "Message")
                {


                    try
                    {
                        return await Conversation.SendAsync(message, () => new AdviceChainDialog());
                    }
                    catch (Exception ex)
                        {
                            return message.CreateReplyMessage(@"Sorry, I don't think I can advise you on that. ¯\\_(ツ)_/¯  Try asking me a question about a topic, like: Give me advice on people");
                        }

                 }
                else
                {
                    return HandleSystemMessage(message);
                }
            }
            

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
                return message.CreateReplyMessage("Hi there and welcome to Advice Bot!  Ask me anything, like this:  \"Give me advice on people.\"");
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }

        
    }
}