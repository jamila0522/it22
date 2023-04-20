using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.Timers;
using Scb.Framework;

namespace ResponseMonitorService
{
    public partial class RMService : ServiceBase
    {
      private int _TimeSend = 0;
      private Timer _timer = new Timer();
      private int _interval = 0;
      ProcessCore process = new ProcessCore();

        public RMService()
        {
            InitializeComponent();

            //ProcessCore core = new ProcessCore();
            //core.DoWork();
            //core.DoReminder();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
          _timer.Start(); 
          Init();
          
          Logger.Info("ResponeMonitor:: Start at" + DateTime.Now);
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
          _timer.Stop();
          Logger.Info("ResponeMonitor:: Stop at" + DateTime.Now);
        }

      protected void Init()
      {
       // Logger.Info("Da vao Innit");
        _TimeSend = Int32.Parse(ConfigurationManager.AppSettings["TimeSend"]);
        _interval = Int32.Parse(ConfigurationManager.AppSettings["Interval"]);

        _timer.Start();
        _timer.Interval = _interval * 60 * 1000;// 
        _timer.Enabled = true;
        _timer.Elapsed += new ElapsedEventHandler(_timer_Tick);
        
        //Logger.Info("End Innit");
      }

      void _timer_Tick(object sender, ElapsedEventArgs e)
      {
        try
        {
          //Send first remind
          process.DoWork();
          // check time and send
          if (DateTime.Now.Hour == _TimeSend)
          {
            //Logger.Info("Da vao _timer_Tick");
            process.DoReminder();
          }
        }
        catch (Exception ex)
        {
          //
          Logger.Error("ResponeMonitor:: Fail when DoRemind");
        }
      }
    }
}
