using Exiled.API.Interfaces;

namespace RatPlugin
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public float RatDamage { get; set; } = 200f;
        public float RatSpeed { get; set; } = 4.5f;
        public float RatKillRadius { get; set; } = 1.2f;
        public float RatSearchRadius { get; set; } = 50f;
        public float RatActivationDelay { get; set; } = 1f;
        public string RatDeathReason { get; set; } = "Загрызен насмерть неопознанной крысой-мутантом.";
    }
}
