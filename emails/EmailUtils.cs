using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Communication.Email;
using Azure.Communication.Email.Models;

public class EmailUtils
{
    public string ConnectionString { get; set; }
    public string DefaultFromAddress { get; set; }

    private readonly List<string> templates = new List<string>
    {
        "<html><body><h1>Hello, {0}!</h1><p>Welcome to our service.</p></body></html>",
        "<html><body><h1>Dear {0},</h1><p>Your order #{1} has been shipped.</p></body></html>",
        "<html><body><h1>Hi {0},</h1><p>We have received your request and will respond shortly.</p></body></html>"
    };

    public EmailUtils(string connectionString, string defaultFromAddress)
    {
        ConnectionString = connectionString;
        DefaultFromAddress = defaultFromAddress;
    }

    /// <summary>
    /// Gets the email template by ID.
    /// </summary>
    /// <param name="templateId">The ID of the template to retrieve.</param>
    /// <returns>The HTML string of the template.</returns>
    public string GetTemplate(int templateId)
    {
        if (templateId < 0 || templateId >= templates.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(templateId), "Invalid template ID.");
        }
        return templates[templateId];
    }

    /// <summary>
    /// Sends an email using the Azure Communication Email Service.
    /// </summary>
    /// <param name="from">The sender's email address.</param>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="templateId">The ID of the email template.</param>
    /// <param name="templatePlaceholders">A list of placeholders to replace in the template.</param>
    public async Task SendEmail(string from, string to, string subject, int templateId, List<string> templatePlaceholders)
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

        var template = GetTemplate(templateId);
        var emailBody = string.Format(template, templatePlaceholders.ToArray());

        var client = new EmailClient(ConnectionString);

        var emailMessage = new EmailMessage(from, to)
        {
            Subject = subject,
            Content = new EmailContent(emailBody)
            {
                Html = emailBody
            }
        };

        try
        {
            var response = await client.SendAsync(emailMessage);
            if (response.Status != EmailSendStatus.Succeeded)
            {
                throw new Exception($"Failed to send email. Status: {response.Status}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error occurred while sending email.", ex);
        }
    }
}
