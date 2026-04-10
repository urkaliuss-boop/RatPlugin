using System;
using Exiled.API.Features;
using Exiled.CustomItems.API;
using RatPlugin.Items;

namespace RatPlugin
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Developer";
        public override string Name => "RatPlugin";
        public override string Prefix => "rat_plugin";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(8, 0, 0); // Убедитесь, что версия EXILED подходит вашему серверу

        public static Plugin Instance { get; private set; }

        public RatItem CustomRatItem { get; private set; }

        public override void OnEnabled()
        {
            Instance = this;

            // Инициализация кастомного предмета (Крысы)
            CustomRatItem = new RatItem();
            CustomRatItem.Register();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            CustomRatItem.Unregister();
            CustomRatItem = null;

            Instance = null;
            base.OnDisabled();
        }
    }
}
