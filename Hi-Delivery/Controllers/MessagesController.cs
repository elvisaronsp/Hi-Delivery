using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Hi_Delivery
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>

        private static async Task<Hi_Delivery_luis> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            Hi_Delivery_luis Data = new Hi_Delivery_luis();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=a6a30012-657c-4696-8fcd-810a284a1afa&subscription-key=4cd1c7147c9e4a8a82ec3f5814b45ce2&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<Hi_Delivery_luis>(JsonDataResponse);
                }
            }
            return Data;
        }

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                string StockRateString;

                Hi_Delivery_luis StLUIS = await GetEntityFromLUIS(activity.Text);

                if (StLUIS.intents.Count() > 0)
                {
                    switch (StLUIS.intents[0].intent)
                    {
                        case "Mostrar info de pizza":
                            StockRateString = "Estás solicitando info de una pizza ";
                            for (int i=0; i < StLUIS.entities.Length; i++)
                            {
                                switch (StLUIS.entities[i].type)
                                {
                                    case "Sabor":
                                        StockRateString += "sabor " + StLUIS.entities[i].entity + " ";
                                        break;

                                    case "Tamaño":
                                        StockRateString += "de tamaño " + StLUIS.entities[i].entity + " ";
                                        break;

                                }
                            }
                            break;
                        case "Pedir pizza":
                            StockRateString = "Estás pidiendo una pizza ";
                            for (int i = 0; i < StLUIS.entities.Length; i++)
                            {
                                switch (StLUIS.entities[i].type)
                                {
                                    case "Sabor":
                                        StockRateString += "sabor " + StLUIS.entities[i].entity + " ";
                                        break;

                                    case "Tamaño":
                                        StockRateString += "de tamaño " + StLUIS.entities[i].entity + " ";
                                        break;

                                }
                            }
                            break;
                        default:
                            StockRateString = "No entiendo lo que dices";
                            break;
                    }
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    Activity reply = activity.CreateReply(StockRateString);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                Activity reply = message.CreateReply("Hola");
                connector.Conversations.ReplyToActivityAsync(reply);
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}