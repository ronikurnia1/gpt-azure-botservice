// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;


namespace GptBotApi.Bot;

public class ChatBot<T> : TeamsActivityHandler where T : Dialog
{
    private readonly ConversationState conversationState;
    private readonly UserState userState;
    private readonly Dialog dialog;

    private readonly ILogger<ChatBot<T>> logger;

    public ChatBot(ConversationState conversationState, UserState userState,
        T dialog, ILogger<ChatBot<T>> logger)
    {
        this.conversationState = conversationState;
        this.userState = userState;
        this.logger = logger;
        this.dialog = dialog;
    }


    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        await base.OnTurnAsync(turnContext, cancellationToken);

        // Save any state changes that might have occurred during the turn.
        await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await userState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        // Run the Dialog with the new message Activity.
        await dialog.RunAsync(turnContext, conversationState.CreateProperty<DialogState>
            (nameof(DialogState)), cancellationToken);
    }

    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        var suggested = new List<CardAction>()
        {
            new CardAction
            {
                Title = "Insurance Plan and Benefit",
                Type = ActionTypes.ImBack,
                Value = "Please explain about insurance packages?"},
            new CardAction
            {
                Title = "Employee Handbook",
                Type = ActionTypes.ImBack,
                Value = "Do you have any information about employee handbook?"
                },
            new CardAction
            {
                Title = "Workplace Safety",
                Type = ActionTypes.ImBack,
                Value = "Is there any workplace safety program"
            }
        };

        var welcome = new HeroCard
        {
            Title = "Welcome to the GPT-powered SmartBot!",
            Text = "You can ask me around HR related things, like:",
            Buttons = suggested
        }.ToAttachment();

        foreach (var member in membersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(welcome), cancellationToken);
            }
        }
    }
}
