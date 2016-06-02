using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdviceBot.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Bot.Connector;

namespace AdviceBot.Controllers
{
    [Serializable]
    public class AdviceChainDialog:IDialog<object>
    {


        static string adviceTopic = string.Empty;
        static string aLuis = string.Empty;

        //DocumentDB connection Info
        private const string EndpointUri = "https://advicebotdb.documents.azure.com:443/";
        private const string PrimaryKey = "DIjubQfc8c96jrUPnmxTDRfe2jcG6iJrXtZZ0XzyGem3hAGmCYu8GEj0NAiYN1mWIZPsh4RAMuoTDShxsx0cMg==";
        private const string databaseName = "AdviceBotDB";
        private const string collectionName = "AdviceCollection";
        private const string defaultResponse = @"Sorry, I don't think I can advise you on that. ¯\\_(ツ)_/¯  Try asking me a question about a topic, like: Give me advice on people";
        private const string defualtGreeting = "Hi there and welcome to Advice Bot!  Ask me anything, like this:  \"Give me advice on people.\"";
        static DocumentClient client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;
            string returnReply = string.Empty;

            //Understand what the user is really asking here:
            AdviceLUIS aLuis = await LUISAdviceClient.ParseUserInput(message.Text);
            List<Advice> aReturn = null;
            string intent = aLuis.intents[0].intent;


            switch (intent)
            {
                case "GiveAdvice":
                    if(aLuis.entities.Count()>0)
                    {
                        aReturn = GetAdvice(aLuis.entities[0].entity);
                    }                    
                    break;
                default:
                    break;

            }

            //If the intent is to give advice there but there are no results, fall through to the default answer
            if (intent == "GiveAdvice" && aReturn.Count() == 0)
            {
                await SendMessages(context, defaultResponse);
            }
            //If the intent is to give advice and there are results, send messages
            else if (intent == "GiveAdvice" && aReturn.Count() > 0)
            {
                await SendMessages(context, "I found " + aReturn.Count().ToString() + " peices of advice...");
                await SendMessages(context, aReturn, null);
                await SendMessages(context, "Hope that was helpful!");
            }
            //If the intent is to greet, then send messages
            else if (intent == "Greeting")
            {
                await SendMessages(context, defualtGreeting);
            }
            else if (intent == "YourWelcome")
            {
                await SendMessages(context, "I'm here anytime!");
            }

            context.Wait(MessageReceivedAsync);


        }

        private static async Task SendMessages(IDialogContext context, string message)
        {
            await SendMessages(context, null, message);
        }
        private static async Task SendMessages(IDialogContext context, List<Advice> aReturn, string message)
        {
            if (aReturn != null)
            {
                foreach (Advice adviceItem in aReturn)
                {
                    await context.PostAsync(adviceItem.AdviceTitle);
                }
            }
            else
            {
                await context.PostAsync(message);
            }
        }

        private static List<Advice> GetAdvice(string adviceEntity)
        {

            string adviceMessage = string.Empty;

            //Query the document DB and return a line of advice to the user.
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            // Here we find the advice based on the user's message
            IQueryable<Advice> adviceQuery = client.CreateDocumentQuery<Advice>(
                   UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                   .Where(f => f.AdviceTags.Contains(adviceEntity));

            return adviceQuery.ToList();

            //// The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
            //foreach (Advice adviceItem in adviceQuery)
            //{

            //    //Find out a means of provding a response when we come up with a lot of responses
            //    adviceMessage += adviceItem.AdviceTitle + "\r\n" + "\r\n" + "------------------------------------------" + "\r\n";
            //}

            //if (adviceMessage == null || adviceMessage == "")
            //{
            //    adviceMessage = @"Sorry, I don't think I can advise you on that. ¯\\_(ツ)_/¯  But good luck!  Feel free to ask me anything else though.";
            //}

            //return adviceMessage;
        }

    }
}