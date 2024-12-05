﻿using System.Net;
using System.Net.Mail;

namespace BL.Services;

// Service voor het verzenden van emails met SMPT
public class EmailService : IEmailService
{
    // TODO: gebruik echte gegevens 
    public async Task SendInvitationEmailAsync(string email, string token, string invitedUser, string inviterName)
    {
        var smtpClient = new SmtpClient(Environment.GetEnvironmentVariable("SMTP_CLIENT"))
        {
            Port = 587,
            Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("SMTP_USERNAME"), Environment.GetEnvironmentVariable("SMTP_PASSWORD")),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("testculcode@gmail.com"),
            Subject = $"Invitation to join {inviterName}'s Culinary Code Group",
            Body = $@"<!DOCTYPE html>
                        <html>
                            <head>
                                <style>
                                    /* Global styles */
                                    body {{
                                        font-family: Arial, sans-serif;
                                        background-color: #f4f4f9;
                                        margin: 0;
                                        padding: 0;
                                        color: #333;
                                    }}

                                    h3 {{
                                        font-style: italic;
                                    }}

                                    p {{
                                        font-size: 14px;
                                        color: #555;
                                        line-height: 1.6;
                                    }}
                                    
                                    /* Header section */
                                    .header {{
                                        background-color: #4CAF50;
                                        color: white;
                                        text-align: center;
                                        padding: 20px 0;
                                    }}
                                    
                                    .header h1 {{
                                        margin: 0;
                                    }}
                                    
                                    /* Button styles */
                                    .button {{
                                        background-color: #4CAF50;
                                        border: none;
                                        color: white;
                                        padding: 15px 32px;
                                        text-align: center;
                                        text-decoration: none;
                                        display: inline-block;
                                        font-size: 16px;
                                        margin: 20px 0;
                                        cursor: pointer;
                                        border-radius: 5px;
                                    }}
                                    .button:hover {{
                                        background-color: #45a049;
                                    }}
                                    
                                    /* Footer section */
                                    .footer {{
                                        background-color: #222222;
                                        color: #fff;
                                        text-align: center;
                                        padding: 15px;
                                        font-size: 12px;
                                    }}
                                    .footer a {{
                                        color: #4CAF50;
                                        text-decoration: none;
                                    }}
                                    
                                    /* Container to center content */
                                    .container {{
                                        max-width: 600px;
                                        margin: 20px auto;
                                        background-color: #ffffff;
                                        padding: 20px;
                                        border-radius: 8px;
                                        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                                    }}
                                    
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <!-- Header Section -->
                                    <div class='header'>
                                        <h1>Culinary Code</h1>
                                        <h3>Meer dan alleen recepten</h3>
                                    </div>

                                    <!-- Body Section -->
                                    <p>Beste {invitedUser},</p>
                                    <p>Je bent uitgenodigd om deel te nemen aan de Culinary Code groep van {inviterName}! Om je uitnodiging te accepteren, klik je op de onderstaande knop:</p>
                                    
                                    <!-- Button Section -->
                                    <a href='https://localhost:7098/api/Invitation/acceptInvitation/{token}' class='button'>Uitnodiging Accepteren</a>
                                    
                                    <!-- Footer Section -->
                                    <div class='footer'>
                                        <p>Bedankt dat je deel uitmaakt van de Culinary Code familie!</p>
                                        <p>&copy; 2024 Culinary Code. Alle rechten voorbehouden.</p>
                                    </div>
                                </div>
                            </body>
                        </html>",
            IsBodyHtml = true
        };
        
        mailMessage.To.Add(email);
        await smtpClient.SendMailAsync(mailMessage);
    }
}