using BotBuilderOpenAi;
using BotBuilderOpenAi.Models;
using GptBotApi.Extensions;
using GptBotApi.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;

namespace GptBotApi.Dialogs;

public class MainDialog : ComponentDialog
{
    private readonly IChatService chatService;
    private readonly ConversationState conversationState;
    private readonly OpenAIConfig config;

    public MainDialog(IChatService chatService, ConversationState conversationState,
        IOptions<OpenAIConfig> options)
    {
        this.chatService = chatService;
        this.conversationState = conversationState;
        config = options.Value;

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
        {
            AksOpenAIStepAsync
        }));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> AksOpenAIStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var conversationData = await conversationState.CreateProperty<ConversationData>(nameof(ConversationData))
            .GetAsync(stepContext.Context, () => new ConversationData(), cancellationToken);

        var history = conversationData.ChatTurns.Select(x => new ChatTurn(x.User, x.Bot)).ToList();
        history.Add(new ChatTurn(stepContext.Context.Activity.Text));
        var request = new ChatRequest(history.ToArray(), new RequestOverrides());

        var response = await chatService.GetResponse(request);


        conversationData.ChatTurns.Add(new ChatTurnData { User = stepContext.Context.Activity.Text, Bot = response.Answer });

        var extractedAnswer = response.Answer.ParseResponse(response.CitationBaseUrl);


        var reply = stepContext.Context.Activity.CreateReply(extractedAnswer.Answer);
        reply.Attachments = extractedAnswer.Citations.ToAttachments();

        await stepContext.Context.SendActivityAsync(reply, cancellationToken);
        var followUpQuestions = extractedAnswer.FollowUpQuestions.ToAttachment();
        if (followUpQuestions != null)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(followUpQuestions), cancellationToken);
        }
        return await stepContext.NextAsync(stepContext.Context.Activity, cancellationToken);
    }

}
