using System.Windows;

using Prism.Modularity;
using Prism.Unity;

using Microsoft.Practices.Unity;

using BotanyEV3Library;

namespace BotanyEV3Controller
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            //base.InitializeShell();

            //Application.Current.MainWindow = (Window)this.Shell;
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureModuleCatalog()
        {
            //base.ConfigureModuleCatalog();

            ModuleCatalog moduleCatalog = (ModuleCatalog)this.ModuleCatalog;
            moduleCatalog.AddModule(typeof(BotanyEV3Module));
        }
    }
}
