using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using BotanyEV3Library.Models;

namespace BotanyEV3Controller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            EV3Model ev3Model = EV3Model.GetInstance();
            if (ev3Model == null)
                return;

            ev3Model.Shutdown();
        }
    }
}
