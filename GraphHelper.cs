using System.Net.Http.Headers;
using System.Text;
using Azure.Core;
using Azure.Identity;
using InviteChatMembers.Extensions;
using InviteChatMembers.Models;
using Microsoft.Graph;
using Newtonsoft.Json;
using ConversationMemberCollectionResponse = Microsoft.Graph.Models.ConversationMemberCollectionResponse;
using Event = Microsoft.Graph.Models.Event;

namespace InviteChatMembers
{
    class GraphHelper
    {
        private static Settings _settings;
        private static ClientSecretCredential _clientSecretCredential;
        private static GraphServiceClient _appClient;

        public static void InitializeGraphForAppOnlyAuth(Settings settings)
        {
            _settings = settings;

            // Ensure settings isn't null
            _ = settings ??
                throw new System.NullReferenceException("Settings cannot be null");

            _settings = settings;

            if (_clientSecretCredential == null)
            {
                _clientSecretCredential = new ClientSecretCredential(
                    _settings.TenantId, _settings.ClientId, _settings.ClientSecret);
            }

            if (_appClient == null)
            {
                _appClient = new GraphServiceClient(_clientSecretCredential,
                    // Use the default scope, which will request the scopes
                    // configured on the app registration
                    new[] { "https://graph.microsoft.com/.default" });
            }
        }

        public static async Task<string> GetAppOnlyTokenAsync()
        {
            _ = _clientSecretCredential ??
                throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

            // Request token with given scopes
            var context = new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" });
            var response = await _clientSecretCredential.GetTokenAsync(context);
            return response.Token;
        }

        /// <summary>  
        /// Get Token for User.  
        /// </summary>  
        /// <returns>Token for user.</returns>  
        public static async Task<string> GetTokenForUserAsync()
        {
            // Todo : implement user token retrieval.
            return await Task.FromResult("");
        }

        public static async Task<ConversationMemberCollectionResponse> GetUsersAsync(string chatId)
        {
            _ = _appClient ??
                throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

            var users = await _appClient.Chats[chatId].Members.GetAsync();
            return users;
        }

        public static async Task<Event> CreateEvent(CardResponse cardResponse)
        {
            _ = _appClient ??
            throw new System.NullReferenceException("Graph has not been initialized for app-only auth");
            var token = await GetTokenForUserAsync();
            var onlineEvent = cardResponse.GetOnlineEvent();

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(cardResponse.GetOnlineEventJason(), Encoding.UTF8, "application/json");
            var respose = await httpClient.PostAsync("me/events", content);
            if (respose.IsSuccessStatusCode)
            {
                var responseContent = await respose.Content.ReadAsStringAsync();
                var newEvent = JsonConvert.DeserializeObject<Event>(responseContent);
                return newEvent;
            } else
            {
                throw new Exception("Failed to schedule a meeting");
            }
        }
    }
}
