using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Modularity;
using Prism.Regions;

using BotanyEV3Library.Views;

namespace BotanyEV3Library
{
    public class BotanyEV3Module : IModule
    {
        private readonly IRegionManager regionManager;

        public BotanyEV3Module(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void Initialize()
        {
            regionManager.RegisterViewWithRegion("MainRegion", typeof(EV3View));
        }
    }
}
