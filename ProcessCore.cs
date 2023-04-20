using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading;
using Scb.Framework;
using System.Configuration;
using System.IO;
using System.Data;

namespace ResponseMonitorService
{
   public class ProcessCore
    {
        #region variables
          private volatile bool m_canStop = false;
          private volatile bool m_canStopProcessMar = false;
          private volatile bool m_canStopMarReminderEmail = false;
          private int m_period = 2;//Unit: second
          private int m_periodProcessMar = 2;//Unit: second
          private int m_periodMarReminderEmail = 2;//Unit: second
          private string _link = ConfigurationManager.AppSettings["Link"];
        #endregion

        #region Public Methods
        public void DoWork()
        {
            try
            {
              SendReminder();
              SendResponse();
            }
            catch (Exception ex)
            {
                Logger.Error("ReminderProcessor::DoWork::", ex);
            }
        }

        public void DoReminder()
        {
            try
            {
                MarReminderEmail();
                Thread.Sleep(m_periodMarReminderEmail * 1000);
            }
            catch (Exception ex)
            {
                Logger.Error("DoMarReminderEmail::DoWork::", ex);
            }
        }

        public void RequestStop()
        {
            m_canStop = true;
            m_canStopProcessMar = true;
            m_canStopMarReminderEmail = true;
        }
        #endregion

        #region Private Methods
        // send remind for first time
        void SendReminder()
        {
          try
          {
            string cs = ConfigurationManager.ConnectionStrings["DefaultDatabase"].ConnectionString;

            SqlConnection conn = new SqlConnection(cs);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Exercise WHERE (ExerStatus = 'Open')", conn);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
              try
              {
                // Exerlist.Add(Convert.ToString(reader["ExerID"]));//
                string ExerID = reader["ExerID"].ToString();
                string ExerSubject = reader["ExerSubject"].ToString();
                string ExerBody = reader["ExerBody"].ToString();
                string ExerPath = reader["ExerPath"].ToString();
                string Email = reader["Email"].ToString();
                DateTime FRDate = DateTime.Parse(reader["FReDate"].ToString());
                DateTime SRDate = DateTime.Parse(reader["SReDate"].ToString());
                DateTime TRDate = DateTime.Parse(reader["TReDate"].ToString());

                SqlConnection conn1 = new SqlConnection(cs);
                conn1.Open();
                SqlCommand cmd1 = new SqlCommand("SELECT * FROM ResponseLog WHERE (ExerID = @ExerID) and (ResponseStatus is null)and (BSent is null)", conn1);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@ExerID";
                param.Value = ExerID;
                cmd1.Parameters.Add(param);

                SqlDataReader reader1 = cmd1.ExecuteReader();
                while (reader1.Read())
                {
                  string To = reader1["Email"].ToString();
                  string From = Email;
                  string Name = reader1["Name"].ToString();
                  string[] Attachment = new string[10];
                  int ID = Int32.Parse(reader1["ID"].ToString());

                  DirectoryInfo d = new DirectoryInfo(ExerPath);
                  FileInfo[] r = d.GetFiles();

                  for (int i = 0; i < r.Length; i++)
                  {
                    Attachment[i] = ExerPath + "\\" + r[i].Name;
                  }

                  string line = "Please access below link to update the status: \n";
                  //NEED TO UPDATE LINK WHEN DEPLOY
                  string link = _link + ID;
                  string tempExerSubject = "";
                  string tempExerBody = "";

                  tempExerSubject = ExerSubject + " - " + Name;
                  tempExerBody = ExerBody + "\n\n" + line + link + "\n";

                  if (EmailHelper.SendMail(From, To, null, tempExerSubject, tempExerBody, Attachment))
                  {
                    //write log mail was sent
                    Logger.Info("Sent to" + To + "at:" + DateTime.Now);
                    SqlConnection conn2 = new SqlConnection(cs);

                    SqlCommand cmd2 = new SqlCommand("UPDATE ResponseLog SET BSent = @BSent WHERE ID = @ID", conn2);
                    cmd2.Parameters.Add("@BSent", SqlDbType.DateTime);
                    cmd2.Parameters["@BSent"].Value = DateTime.Now.ToString();
                    cmd2.Parameters.Add("@ID", SqlDbType.Int);
                    cmd2.Parameters["@ID"].Value = ID;

                    conn2.Open();
                    cmd2.ExecuteNonQuery();
                    conn2.Close();
                  }
                }
                reader1.Close();
                conn1.Close();


              }
              catch (Exception ex)
              {
                throw ex;
              }
            }
            reader.Close();
            conn.Close();
          }
          catch (Exception ex)
          {
            Logger.Error("ERROR SENDREMIND::",ex);
          }
        }

        // send remind with due date
        void MarReminderEmail()
        {
          try
          {
            string cs = ConfigurationManager.ConnectionStrings["DefaultDatabase"].ConnectionString;

            //**Get remind message**//
            SqlConnection conn3 = new SqlConnection(cs);
            conn3.Open();

            SqlCommand cmd3 = new SqlCommand("SELECT * FROM RemindMessage", conn3);
            SqlDataReader reader3 = cmd3.ExecuteReader();
            reader3.Read();
            string RemindMessage1 = reader3["RemindMessage1"].ToString();
            string RemindMessage2 = reader3["RemindMessage2"].ToString();
            string RemindMessage3 = reader3["RemindMessage3"].ToString();
            reader3.Close();
            conn3.Close();
            //**Get remind message done**//

            //** Start scan data base for reminder needed **//
            SqlConnection conn = new SqlConnection(cs);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT ex.*,rp.FSent,rp.SSent,rp.TSent,rp.Email as RPEmail,rp.Name,rp.ID FROM Exercise ex  join ResponseLog rp on ex.ExerID = rp.ExerID WHERE ExerStatus = 'Open' and rp.ResponseStatus is null", conn);

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
              try
              {
                // Exerlist.Add(Convert.ToString(reader["ExerID"]));//
                string ExerID = reader["ExerID"].ToString();
                string ExerSubject = reader["ExerSubject"].ToString();
                string ExerBody = reader["ExerBody"].ToString();
                string ExerPath = reader["ExerPath"].ToString();
                string mail = reader["Email"].ToString();
                DateTime FRDate = DateTime.Parse(reader["FReDate"].ToString());
                DateTime SRDate = DateTime.Parse(reader["SReDate"].ToString());
                DateTime TRDate = DateTime.Parse(reader["TReDate"].ToString());
                string FSent = reader["FSent"].ToString();
                string SSent = reader["SSent"].ToString();
                string TSent = reader["TSent"].ToString();

                Boolean Remind = false;
                int TimeRemind = 0;

                if (DateTime.Now.Date == FRDate.Date && string.IsNullOrEmpty(FSent))
                {
                  Remind = true;
                  TimeRemind = 1;
                  ExerBody = RemindMessage1 + "\n\n" + ExerBody;
                }

                if (DateTime.Now.Date == SRDate.Date && string.IsNullOrEmpty(SSent))
                {
                  Remind = true;
                  TimeRemind = 2;
                  ExerBody = RemindMessage2 + "\n\n" + ExerBody;
                }

                if (DateTime.Now.Date == TRDate.Date && string.IsNullOrEmpty(TSent))
                {
                  Remind = true;
                  TimeRemind = 3;
                  ExerBody = RemindMessage3 + "\n\n" + ExerBody;
                }

                if (Remind)
                {
                  string Email = reader["RPEmail"].ToString();
                  string From = mail;
                  string Name = reader["Name"].ToString();
                  string[] Attachment = new string[10];
                  int ID = Int32.Parse(reader["ID"].ToString());


                  DirectoryInfo d = new DirectoryInfo(ExerPath);
                  FileInfo[] r = d.GetFiles();

                  for (int i = 0; i < r.Length; i++)
                  {
                    Attachment[i] = ExerPath + "\\" + r[i].Name;
                  }

                  ExerSubject = ExerSubject + " - " + Name;

                  string tempExerBody = "";
                  string line = "Please access below link to update the status: \n";
                  //NEED TO UPDATE LINK WHEN DEPLOY
                  string link = _link + ID;
                  tempExerBody = ExerBody + "\n\n" + line + link + "\n";

                  if (EmailHelper.SendMail(From, Email, null, ExerSubject, tempExerBody, Attachment))
                  {
                    Logger.Info("Sent to" + Email + "at:" + DateTime.Now);
                    SqlConnection conn2 = new SqlConnection(cs);

                    if (TimeRemind == 1)
                    {
                      SqlCommand cmd2 = new SqlCommand("UPDATE ResponseLog SET FSent = @FSent WHERE ID = @ID", conn2);
                      cmd2.Parameters.Add("@FSent", SqlDbType.DateTime);
                      cmd2.Parameters["@FSent"].Value = DateTime.Now.ToString();
                      cmd2.Parameters.Add("@ID", SqlDbType.Int);
                      cmd2.Parameters["@ID"].Value = ID;
                      conn2.Open();
                      cmd2.ExecuteNonQuery();
                      conn2.Close();
                    }

                    if (TimeRemind == 2)
                    {
                      SqlCommand cmd2 = new SqlCommand("UPDATE ResponseLog SET SSent = @SSent WHERE ID = @ID", conn2);
                      cmd2.Parameters.Add("@SSent", SqlDbType.DateTime);
                      cmd2.Parameters["@SSent"].Value = DateTime.Now.ToString();
                      cmd2.Parameters.Add("@ID", SqlDbType.Int);
                      cmd2.Parameters["@ID"].Value = ID;
                      conn2.Open();
                      cmd2.ExecuteNonQuery();
                      conn2.Close();
                    }

                    if (TimeRemind == 3)
                    {
                      SqlCommand cmd2 = new SqlCommand("UPDATE ResponseLog SET TSent = @TSent WHERE ID = @ID", conn2);
                      cmd2.Parameters.Add("@TSent", SqlDbType.DateTime);
                      cmd2.Parameters["@TSent"].Value = DateTime.Now.ToString();
                      cmd2.Parameters.Add("@ID", SqlDbType.Int);
                      cmd2.Parameters["@ID"].Value = ID;
                      conn2.Open();
                      cmd2.ExecuteNonQuery();
                      conn2.Close();
                    }
                  }
                }
                // Update status Excercise after Third remind
                if (TimeRemind == 3)
                {
                  SqlConnection con = new SqlConnection(cs);
                  SqlCommand cm = new SqlCommand("Update Exercise Set ExerStatus = 'Close' Where ExerID =" + ExerID, con);

                  con.Open();
                  cm.ExecuteNonQuery();
                  con.Close();
                }
              }
              catch (Exception ex)
              {
                throw ex;
              }
            }
            reader.Close();
            conn.Close();
          }
          catch (Exception ex)
          {
            Logger.Error("FAIL WHEN SEND REMIND WITH DUE DATE::", ex);
          }
        }

      //send response from user.
     protected void SendResponse()
     {
       try
       {
         string cs = ConfigurationManager.ConnectionStrings["DefaultDatabase"].ConnectionString;

         SqlConnection conn = new SqlConnection(cs);
         conn.Open();

         SqlCommand cmd = new SqlCommand("SELECT ex.ExerSubject,ex.Email as MailTo,rp.* FROM Exercise ex,ResponseLog rp WHERE ex.exerID = rp.exerID and rp.Responsestatus is not null and  rp.isSent is null", conn);
         SqlDataReader reader = cmd.ExecuteReader();

         while (reader.Read())
         {
           try
           {
             string ExerSubject = reader["ExerSubject"].ToString();
             string ExerPath = reader["ExerPath"].ToString();
             string ExerName = reader["ExerName"].ToString();
             string ReviewerName = reader["Name"].ToString();
             string ReviewerBankID = reader["BankID"].ToString();
             string ExerciseResponseStatus = reader["ResponseStatus"].ToString();
             string ReviewerComment = reader["Comment"].ToString();
             int ID = Int32.Parse(reader["ID"].ToString());

             string To = reader["MailTo"].ToString();
             string From = reader["Email"].ToString();
             string Name = ReviewerName;
             string[] Attachment = new string[10];

             if (!string.IsNullOrEmpty(ExerPath))
             {
               DirectoryInfo d = new DirectoryInfo(ExerPath);
               FileInfo[] r = d.GetFiles();

               for (int i = 0; i < r.Length; i++)
               {
                 Attachment[i] = ExerPath + "\\" + r[i].Name;
               }

             }

             string tempExerSubject = "";
             string tempExerBody = "";

             tempExerSubject = ExerSubject + " - " + Name;
             tempExerBody = string.Format(" Exercise Name:{0}\n\n Reviewer Name:{1}\n\n Reviewer Bank ID:{2}\n\n Exercise Response Status:{3}\n\n Reviewer Comment:{4}", ExerName, ReviewerName, ReviewerBankID, ExerciseResponseStatus, ReviewerComment);

             if (EmailHelper.SendMail(From, To, null, tempExerSubject, tempExerBody, Attachment))
             {
               //write log mail was sent
               Logger.Info("Sent to" + To + "at:" + DateTime.Now);
               SqlConnection conn1 = new SqlConnection(cs);

               SqlCommand cmd2 = new SqlCommand(string.Format("UPDATE ResponseLog SET isSent = 1 WHERE ID = {0}", ID), conn1);
               conn1.Open();
               cmd2.ExecuteNonQuery();
               conn1.Close();
             }

           }
           catch (Exception ex)
           {
             throw ex;
           }
         }
         reader.Close();
         conn.Close();
       }
       catch (Exception ex)
       {
         Logger.Error("FAIL WHEN SEND SENDRESPONSE::",ex);
       }
     }
        #endregion

    }
}
