using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using Scb.Framework;
/**using ProcessCore.Entities;**/

namespace ResponseMonitorService
{
  public static class EmailHelper
  {
    public static bool SendNotifyEmail(string content, string from, string to, string toname, string note)
    {
      if (SendMail(from, to, null, "Investigation Controlling System - Reminder", content, null))
        return true;

      return false;
    }

    public static bool SendMail(string from, string to, string cc, string subject, string content,
      string[] attachfiles)
    {
      try
      {
        // TODO: Add error handling for invalid arguments
        // To
        MailMessage mailMsg = new MailMessage();
        mailMsg.To.Add(to);
        if (!string.IsNullOrEmpty(cc))
          mailMsg.CC.Add(cc);

        // From
        MailAddress mailAddress = new MailAddress(from);
        mailMsg.From = mailAddress;


        // Subject and Body
        mailMsg.Subject = subject;
        mailMsg.Body = content;
        if (attachfiles != null)
          foreach (string attachfile in attachfiles)
          {
            if (attachfile != null)
            {
              mailMsg.Attachments.Add(new Attachment(attachfile));
            }
          }

        // Init SmtpClient and send
        SmtpClient smtpClient = new SmtpClient(GlobalCache.GetInstance().SmtpHost);
        smtpClient.Port = 25;

        smtpClient.UseDefaultCredentials = true;
        smtpClient.Send(mailMsg);

        return true;
      }
      catch (Exception ex)
      {
        Logger.Error("Investigation::SendingMailToSMTP::", ex);
        return false;
      }

    }
  }
}
