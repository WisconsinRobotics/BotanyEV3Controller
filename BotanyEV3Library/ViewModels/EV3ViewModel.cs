using System.Windows.Input;

using Prism.Commands;
using Prism.Mvvm;

using BotanyEV3Library.Models;

namespace BotanyEV3Library.ViewModels
{
    class EV3ViewModel : BindableBase
    {
        EV3Model ev3Model;

        public EV3ViewModel()
        {
            ev3Model = new EV3Model();
            ExecuteCommand = new DelegateCommand<object>(OnExecute);
            InitializeCommand = new DelegateCommand<object>(OnInitialize);
            ResetCommand = new DelegateCommand<object>(OnReset);
            StopCommand = new DelegateCommand<object>(OnStop);
        }

        public ICommand ExecuteCommand
        {
            get;
            private set;
        }

        public ICommand InitializeCommand
        {
            get;
            private set;
        }

        public ICommand ResetCommand
        {
            get;
            private set;
        }

        public ICommand StopCommand
        {
            get;
            private set;
        }

        private void OnExecute(object arg)
        {
            ev3Model.Execute();
        }

        private void OnInitialize(object arg)
        {
            ev3Model.Initialize();
        }

        public void OnReset(object arg)
        {
            ev3Model.Reset();
        }

        private void OnStop(object arg)
        {
            ev3Model.Stop();
        }
    }
}
