using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Communication.Email;

public class EmailUtils
{
    public string ConnectionString { get; set; }
    public string DefaultFromAddress { get; set; }

    private readonly List<string> templates = new List<string>
    {
        "<html><body><h1>Hello, {salutation} {fullName}!</h1><p>Welcome to our service.</p></body></html>",
        "<html><body><h1>Dear {firstName},</h1><p>Your order #{projectName} has been shipped.</p></body></html>",
        "<html><body><h1>Hi {firstName},</h1><p>We have received your request and will respond shortly.</p></body></html>"
    };

    private readonly List<string> templates2 = new List<string>
    {
        "<html><body><h1>Hello, {0} {1}!</h1><p>Welcome to our service.</p></body></html>",
        "<html><body><h1>Dear {0},</h1><p>Your order #{1} has been shipped.</p></body></html>",
        "<html><body><h1>Hi {0},</h1><p>We have received your request and will respond shortly.</p></body></html>"
    };

    public EmailUtils(string connectionString, string defaultFromAddress)
    {
        ConnectionString = connectionString;
        DefaultFromAddress = defaultFromAddress;
    }

    /// <summary>
    /// Formats a template with the given replacement values.
    /// </summary>
    /// <param name="templateId">The ID of the template to format.</param>
    /// <param name="replacementValues">The list of values to replace the placeholders in the template.</param>
    /// <returns>The formatted template.</returns>
    public string FormatTemplate(int templateId, List<string> replacementValues)
    {
        if (templateId < 0 || templateId >= templates2.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(templateId), "Invalid template ID.");
        }

        var template = templates2[templateId];
        var placeholderCount = template.Count(c => c == '{') / 2; // Count {0}, {1}, etc.

        if (replacementValues.Count != placeholderCount)
        {
            throw new ArgumentException($"Template {templateId} requires {placeholderCount} placeholders, but {replacementValues.Count} were provided.", nameof(replacementValues));
        }

        return string.Format(template, replacementValues.ToArray());
    }

    /// <summary>
    /// Replaces placeholders in a template with values from the provided dictionary.
    /// </summary>
    /// <param name="templateId">The ID of the template.</param>
    /// <param name="data">A dictionary containing placeholder keys and their corresponding values.</param>
    /// <returns>The formatted template.</returns>
    public string GetFullTemplate(int templateId, Dictionary<string, string> data)
    {
        if (templateId < 0 || templateId >= templates.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(templateId), "Invalid template ID.");
        }

        var template = templates[templateId];

        foreach (var key in data.Keys)
        {
            template = template.Replace($"{{{key}}}", data[key]);
        }

        // Check for any unreplaced placeholders
        if (template.Contains("{"))
        {
            throw new ArgumentException("Not all placeholders were replaced. Check the provided dictionary.", nameof(data));
        }

        return template;
    }

    private async Task SendEmailInternal(string from, string to, string subject, string emailBody)
    {
        var client = new EmailClient(ConnectionString);

        var recipients = new EmailRecipients(new List<EmailAddress> { new EmailAddress(to) });

        var emailContent = new EmailContent(subject)
        {
            Html = emailBody
        };

        var emailMessage = new EmailMessage(from, recipients, emailContent);

        try
        {
            var response = await client.SendAsync(WaitUntil.Completed, emailMessage);
            if (response == null || response.Value == null || response.Value.Status != EmailSendStatus.Succeeded)
            {
                throw new Exception("Failed to send email.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error occurred while sending email.", ex);
        }
    }

    /// <summary>
    /// Sends an email using the Azure Communication Email Service.
    /// </summary>
    /// <param name="from">The sender's email address.</param>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="templateId">The ID of the email template.</param>
    /// <param name="templatePlaceholders">A dictionary of placeholders to replace in the template.</param>
    public async Task SendEmail(string from, string to, string subject, int templateId, Dictionary<string, string> templatePlaceholders)
    {
        if (string.IsNullOrWhiteSpace(from))
        {
            from = DefaultFromAddress;
        }

        if (string.IsNullOrWhiteSpace(from))
        {
            throw new ArgumentException("The sender's email address must be provided.", nameof(from));
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            throw new ArgumentException("The recipient's email address must be provided.", nameof(to));
        }

        var emailBody = GetFullTemplate(templateId, templatePlaceholders);
        await SendEmailInternal(from, to, subject, emailBody);
    }

    /// <summary>
    /// Sends an email using the Azure Communication Email Service with ordered placeholders.
    /// </summary>
    /// <param name="from">The sender's email address.</param>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="templateId">The ID of the email template.</param>
    /// <param name="replacementValues">A list of values to replace placeholders in the template.</param>
    public async Task SendEmail(string from, string to, string subject, int templateId, List<string> replacementValues)
    {
        if (string.IsNullOrWhiteSpace(from))
        {
            from = DefaultFromAddress;
        }

        if (string.IsNullOrWhiteSpace(from))
        {
            throw new ArgumentException("The sender's email address must be provided.", nameof(from));
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            throw new ArgumentException("The recipient's email address must be provided.", nameof(to));
        }

        var emailBody = FormatTemplate(templateId, replacementValues);
        await SendEmailInternal(from, to, subject, emailBody);
    }
}
