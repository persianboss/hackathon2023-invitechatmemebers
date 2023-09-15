using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Models;
using AdaptiveCards.Templating;
using Newtonsoft.Json;
using Attachment = Microsoft.Bot.Schema.Attachment;
using InviteChatMembers.Models;

namespace InviteChatMembers.Bot;

public class TeamsMessageExtension : TeamsActivityHandler
{
    private readonly string adaptiveCardForFrom = Path.Combine(".", "Resources", "SchedulingFormCard.json");

    protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
    {
        var chatId = turnContext.Activity.Conversation.Id;
        var users = await GraphHelper.GetUsersAsync(chatId);
        var adaptiveCardJson = File.ReadAllText(adaptiveCardForFrom);
        AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);

        var myData = new { users = users.Value.Select(u => new { FullName = u.DisplayName, u.Id, IsOrganizer = turnContext.Activity.From.AadObjectId == ((AadUserConversationMember)u).UserId, email = ((AadUserConversationMember)u).Email }).ToList() };

        string cardJson = template.Expand(myData);
        var cardAttachment = CreateAdaptiveCardAttachment(cardJson);


        return new MessagingExtensionActionResponse
        {
            Task = new TaskModuleContinueResponse
            {
                Value = new TaskModuleTaskInfo
                {
                    Card = cardAttachment,
                    Title = "Meeting scheduling form",
                },
            },
        };
    }

    // Message Extension Code
    // Action.
    protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
    {
        switch (action.CommandId)
        {
            case "scheduleMeeting":
                return await ScheduleMeetingCommand(turnContext, action);
        }
        return await Task.FromResult(new MessagingExtensionActionResponse());
    }

    private async Task<MessagingExtensionActionResponse> ScheduleMeetingCommand(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
    {
        var createCardData = ((JObject)action.Data).ToObject<CardResponse>();
        var onlineEvent = await GraphHelper.CreateEvent(createCardData);
        var card = new HeroCard
        {
            Title = onlineEvent.Subject,
            Subtitle = string.Join(",", onlineEvent.Attendees.Select(a => a.EmailAddress.Name)),
            Text = string.Join("-", $"From : {onlineEvent.Start.ToDateTime()}, To: {onlineEvent.End.ToDateTime()}"),
        };

        var attachments = new List<MessagingExtensionAttachment>
        {
            new MessagingExtensionAttachment
            {
                Content = card,
                ContentType = HeroCard.ContentType,
                Preview = card.ToAttachment(),
            }
        };

        return new MessagingExtensionActionResponse
        {
            ComposeExtension = new MessagingExtensionResult
            {
                AttachmentLayout = "list",
                Type = "result",
                Attachments = attachments,
            },
        };
    }

    // Generate a set of substrings to illustrate the idea of a set of results coming back from a query.
    private async Task<IEnumerable<(string, string, string, string, string)>> FindPackages(string text)
    {
        var obj = JObject.Parse(await (new HttpClient()).GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{text}"));
        return obj["data"].Select(item => (item["id"].ToString(), item["version"].ToString(), item["description"].ToString(), item["projectUrl"]?.ToString(), item["iconUrl"]?.ToString()));
    }

    private static Attachment CreateAdaptiveCardAttachment(string _card)
    {
        var adaptiveCardAttachment = new Attachment()
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = JsonConvert.DeserializeObject(_card),
        };
        return adaptiveCardAttachment;
    }


}

