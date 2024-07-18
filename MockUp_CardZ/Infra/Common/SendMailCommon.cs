using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Infra.Common
{
    public class SendMailCommon
    {
        //private readonly HttpClient _httpClient;
        //private readonly IEnumerable<IHttpResponseContentReader> _parsers;

        //public QueuingEmailWithTemplate(
        //    IHttpClientFactory httpClientFactory,
        //    IEnumerable<IHttpResponseContentReader> parsers)
        //{
        //    _httpClient = httpClientFactory.CreateClient(nameof(SendInterServiceRequest));
        //    _parsers = parsers;
        //}

        //[ActivityInput(
        //    Hint = "The recipients email addresses.",
        //    SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        //    )]
        //public string To { get; set; } = String.Empty;

        //[ActivityInput(
        //    Hint = "The cc recipients email addresses.",
        //    SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid },
        //    Category = "More")]
        //public string Cc { get; set; } = String.Empty;

        //[ActivityInput(Hint = "Transaction code to get the template (leave blank if using current transaction code)", SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        //public string? CustomTransactionCode { get; set; }

        //[ActivityInput(Hint = "Param to get the template (eg: SENDER, RECEIVER...)", UIHint = ActivityInputUIHints.MultiLine, SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        //public string? TemplateParam { get; set; }

        //[ActivityInput(Hint = "Custom language of the template (leave blank to use user's language)", UIHint = ActivityInputUIHints.MultiLine, SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        //public string? CustomLang { get; set; }

        //[ActivityInput(
        //    Hint = "The jobject param get from transaction info to fill in to the template.",
        //    UIHint = ActivityInputUIHints.MultiLine,
        //    DefaultSyntax = SyntaxNames.Json,
        //    SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript })]
        //public JObject InputParams { get; set; }
        //[ActivityOutput] public object? ResponseContent { get; set; }

        //private class AttachmentStructure
        //{
        //    public string FileName { get; set; }
        //    public string Content { get; set; }
        //    public string Password { get; set; }
        //}

        //protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        //{
        //    if (string.IsNullOrEmpty(CustomTransactionCode))
        //    {
        //        CustomTransactionCode = context.GetVariable(VariableKey.TransactionCode).ToString();
        //    }
        //    if (string.IsNullOrEmpty(CustomLang))
        //    {
        //        CustomLang = context.GetVariable(VariableKey.Lang).ToString();
        //    }
        //    if (string.IsNullOrEmpty(TemplateParam))
        //    {
        //        TemplateParam = "DEFAULT";
        //    }

        //    var msg = await TemplateBuilder.BuildTemplate(CustomTransactionCode, "EMAIL", TemplateParam, CustomLang, InputParams);

        //    List<AttachmentStructure> attachmens = new List<AttachmentStructure>();
        //    if (!string.IsNullOrEmpty(msg.Attachment1) && !string.IsNullOrEmpty(msg.AttachmentName1))
        //    {
        //        attachmens.Add(new AttachmentStructure
        //        {
        //            FileName = msg.AttachmentName1,
        //            Password = msg.Password,
        //            Content = msg.Attachment1
        //        });
        //    }

        //    if (!string.IsNullOrEmpty(msg.Attachment2) && !string.IsNullOrEmpty(msg.AttachmentName2))
        //    {
        //        attachmens.Add(new AttachmentStructure
        //        {
        //            FileName = msg.AttachmentName2,
        //            Password = msg.Password,
        //            Content = msg.Attachment2
        //        });
        //    }

        //    if (!string.IsNullOrEmpty(msg.Attachment3) && !string.IsNullOrEmpty(msg.AttachmentName3))
        //    {
        //        attachmens.Add(new AttachmentStructure
        //        {
        //            FileName = msg.AttachmentName3,
        //            Password = msg.Password,
        //            Content = msg.Attachment3
        //        });
        //    }

        //    var outcomes = new List<string> { OutcomeNames.Done };
        //    try
        //    {
        //        var bodyObject = new
        //        {
        //            IdpHeader = new
        //            {
        //                ChannelId = context.GetVariable(VariableKey.ChannelId),
        //                UserId = context.GetVariable(VariableKey.UserId),
        //                ClientInfo = context.GetVariable(VariableKey.ClientInfo),
        //                Lang = context.GetVariable(VariableKey.Lang),
        //            },
        //            sourceId = context.WorkflowInstance.Id,
        //            to = To,
        //            cc = Cc,
        //            title = msg.Title,
        //            body = msg.Body,
        //            attachment = attachmens
        //        };

        //        string requestBody = JObject.FromObject(bodyObject).ToString();

        //        InterServiceExtensions.InterServiceRequestBase request = new InterServiceExtensions.InterServiceRequestBase
        //        {
        //            Destination = "ESP",
        //            Url = "/email/send",
        //            Method = "POST",
        //            Authorization = "",
        //            RequestHeaders = new Models.HttpRequestHeaders() { { "X-Correlation-ID", context.WorkflowInstance.CorrelationId } },
        //            ContentType = "application/json",
        //            Content = requestBody,
        //            context = context,
        //            httpClient = _httpClient,
        //            ReadContent = true,
        //            parsers = _parsers,
        //            ResponseContentParserName = "",
        //            SupportedStatusCodes = new HashSet<int>(new[] { 200 })
        //        };

        //        InterServiceExtensions.InterServiceResponseBase result = await (new InterServiceExtensions()).SendInterRequest(request);

        //        if (result.Response.StatusCode.Equals(HttpStatusCode.OK))
        //        {
        //            return Outcomes(OutcomeNames.Done, "Success");
        //        }
        //        else
        //        {
        //            ResponseContent = result.ResponseContent;
        //            context.JournalData.Add("Error", result.ResponseContent);
        //            return Outcomes(OutcomeNames.Done, "Unexpected Error");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        outcomes.Add("Unexpected Error");
        //        ResponseContent = ex.ToString();
        //        context.JournalData.Add("Error", ex.Message);
        //    }

        //    return Outcomes(outcomes);
        //}
    }
}
