using Daibitx.Extension.Modularize.EventBus.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daibitx.Extension.Modularize.EventBus
{
    public class CrossEvent
    {
        private static readonly Lazy<CrossEventBusFactory> _factoryInstance = new Lazy<CrossEventBusFactory>();

        /// <summary>
        /// 全局通用事件总线
        /// </summary>
        public static CrossEventBusFactory CrossEventInstance => _factoryInstance.Value;
    }
}
