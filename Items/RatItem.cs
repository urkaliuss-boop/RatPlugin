using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using MEC;
using RatPlugin.Components;
using UnityEngine;

namespace RatPlugin.Items
{
    [CustomItem(ItemType.GrenadeHE)]
    public class RatItem : CustomGrenade
    {
        public override uint Id { get; set; } = 30;
        public override string Name { get; set; } = "ручной Джерри";
        public override string Description { get; set; } = "Странная крыса-мутант! Бросьте её, и она побежит за ближайшим игроком.";
        public override float Weight { get; set; } = 1f;
        public override ItemType Type { get; set; } = ItemType.GrenadeHE;
        
        // Время до взрыва - большое, чтобы граната не взрывалась сама, а служила источником писка
        public override float FuseTime { get; set; } = 600f; 

        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 3,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
            {
                new DynamicSpawnPoint
                {
                    Chance = 50,
                    Location = SpawnLocationType.InsideLczArmory
                },
                new DynamicSpawnPoint
                {
                    Chance = 100, // Высокий шанс для тестов
                    Location = SpawnLocationType.InsideHczArmory
                }
            }
        };

        public override bool ExplodeOnCollision { get; set; } = false;

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ThrownProjectile += OnThrownProjectile;
            Timing.RunCoroutine(PickupVisualizer(), "RatVisualizer");
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ThrownProjectile -= OnThrownProjectile;
            Timing.KillCoroutines("RatVisualizer");
            base.UnsubscribeEvents();
        }

        // Корутина для замены всех лежащих на полу "Крыс" на зеленые квадраты
        private IEnumerator<float> PickupVisualizer()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);
                foreach (var pickup in Exiled.API.Features.Pickups.Pickup.List)
                {
                    if (Check(pickup) && pickup != null && pickup.GameObject != null && pickup.GameObject.GetComponent<RatVisualizer>() == null)
                    {
                        pickup.GameObject.AddComponent<RatVisualizer>();
                    }
                }
            }
        }

        protected override void OnThrownProjectile(ThrownProjectileEventArgs ev)
        {
            if (Check(ev.Item))
            {
                if (ev.Projectile != null && ev.Projectile.GameObject != null)
                {
                    // Не делаем гранату нулевого размера, иначе привязанный к ней куб тоже станет нулевым!
                    // Куб достаточно большой (0.3), чтобы просто полностью скрыть саму гранату внутри себя.

                    // Создаём схемат (примитив) - зеленый куб
                    var primitive = Exiled.API.Features.Toys.Primitive.Create(
                        UnityEngine.PrimitiveType.Cube, 
                        ev.Projectile.Position
                    );
                    primitive.Scale = new Vector3(0.3f, 0.3f, 0.3f);
                    primitive.Color = Color.green;

                    // Привязываем примитив куба к летящей гранате
                    primitive.Base.transform.SetParent(ev.Projectile.Base.transform);
                    primitive.Base.transform.localPosition = Vector3.zero;

                    // Навешиваем наш мозг "крысы" на САМУ ГРАНАТУ
                    var controller = ev.Projectile.GameObject.AddComponent<RatController>();
                    controller.Init(ev.Player);
                }
            }
            
            base.OnThrownProjectile(ev);
        }

        protected override void OnExploding(ExplodingGrenadeEventArgs ev)
        {
            if (Check(ev.Projectile))
            {
                ev.IsAllowed = false; 
            }
            
            base.OnExploding(ev);
        }
    }

    // Компонент для визуального изменения лежащего на полу предмета
    public class RatVisualizer : MonoBehaviour
    {
        private Exiled.API.Features.Toys.Primitive _primitive;

        private void Start()
        {
            // Не прячем оригинальную модель через scale, иначе потомки спрячутся.
            // Примитив скроет её своей текстурой.

            _primitive = Exiled.API.Features.Toys.Primitive.Create(
                PrimitiveType.Cube,
                transform.position
            );
            _primitive.Scale = new Vector3(0.3f, 0.3f, 0.3f);
            _primitive.Color = Color.green;

            _primitive.Base.transform.SetParent(transform);
            _primitive.Base.transform.localPosition = Vector3.zero;
        }

        private void OnDestroy()
        {
            if (_primitive != null)
                _primitive.Destroy();
        }
    }
}
