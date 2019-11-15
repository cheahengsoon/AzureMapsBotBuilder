// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
           // await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
            var lineid = turnContext.Activity.Text;
            if(lineid=="206")
            {
               // await turnContext.SendActivityAsync(MessageFactory.Text("Bus Number 206"));
                await ProcessMAP(turnContext, cancellationToken);
            }
            else
            {
                //await turnContext.SendActivityAsync(MessageFactory.Text("Sorry Bus Number not Found"));

                await ProcessArrivalTime(turnContext, cancellationToken);
            }
      

        }

        private async Task ProcessArrivalTime(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //await turnContext.SendActivityAsync("Arrival Time: "+turnContext.Activity.Text);
            var stopId = turnContext.Activity.Text;
            var client = new RestClient("https://atlas.microsoft.com/mobility/realtime/arrivals/json?api-version=1.0&metroId=5390&query="+stopId+"&subscription-key=UTihEd648nTqRjRqFXfKY-g0PbnZM_fDcEotOoROyS4");
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Host", "atlas.microsoft.com");
            request.AddHeader("Postman-Token", "850a5f2a-e3aa-4c9e-b2c9-1433018c1f4d,938371e3-26cb-4055-be6f-a3c8c60c9f57");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("User-Agent", "PostmanRuntime/7.19.0");
            IRestResponse response = client.Execute(request);

   
            var content = response.Content;

            JObject jObj = JObject.Parse(content);

            JArray jarr = (JArray)jObj["results"];

            var attachments = new List<Attachment>();
            foreach(JToken item in jarr)
            {
                var arrivalMinutes = (string)item.SelectToken("arrivalMinutes");
                var linenumber = (string)item.SelectToken("line.lineNumber");
                var linedest = (string)item.SelectToken("line.lineDestination");

                //await turnContext.SendActivityAsync(MessageFactory.Text("Minutes Arrival: "+arrivalMinutes +"\r\n Line Number: "+linenumber+"\r\n Destination: "+linedest));

                var herocard = new HeroCard(
                  // title:linenumber+"Destination: "+linedest,
                  // subtitle:"Minute Arrival"+arrivalMinutes
                  title:"Est. Arrivals "+arrivalMinutes +"mins",
                  subtitle:linenumber+"\r\n" +linedest

                   ).ToAttachment();

                attachments.Add(herocard);
            }
            var reply = MessageFactory.Carousel(attachments);
            await turnContext.SendActivityAsync(reply);

        }

        private async Task ProcessMAP(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
             await turnContext.SendActivityAsync(MessageFactory.Text("Please Select your Current Bus stop location"));


            var client = new RestClient("https://atlas.microsoft.com/mobility/transit/itinerary/json?subscription-key=UTihEd648nTqRjRqFXfKY-g0PbnZM_fDcEotOoROyS4&api-version=1.0&query=ec9ae150-5825-4e81-bc66-82676b10e9e5---20191115579B4CC7E9084F86B851A0FE4FFBA9E4:0---5390");
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Host", "atlas.microsoft.com");
            request.AddHeader("Postman-Token", "e42c6725-b865-40e5-93c6-d1559826ab04,142ab14e-1160-43d0-9f5e-53617adfac24");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("User-Agent", "PostmanRuntime/7.19.0");
            IRestResponse response = client.Execute(request);

            var content = response.Content; // raw content as string
                                            // await turnContext.SendActivityAsync(content.ToString());

            JObject jObj = JObject.Parse(content);
            var stopNames = jObj.SelectToken("legs[2].stops.stopName");
            var stopIds = jObj.SelectToken("legs[2].stops.stopId");
            JArray stops = (JArray)jObj.SelectToken("legs[2].stops");

            var attachments = new List<Attachment>();
            foreach (JToken stop in stops)
            {
                stopIds = (string)stop.SelectToken("stopId");
                stopNames = (string)stop.SelectToken("stopName");


                var herocard = new HeroCard(
                     //title: stopIds.ToString()+ stopNames.ToString(),
                     buttons: new CardAction[]
                     {
                    new CardAction(ActionTypes.ImBack, title: stopNames.ToString() , value: stopIds.ToString())
                     }

                     ).ToAttachment();

                attachments.Add(herocard);


            }
            var reply = MessageFactory.Carousel(attachments);

            await turnContext.SendActivityAsync(reply);




        }

  







        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
}
