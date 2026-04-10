using Exiled.API.Features;
using PlayerRoles;
using UnityEngine;
using Mirror;

namespace RatPlugin.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class RatController : MonoBehaviour
    {
        // === ПЕРЕМЕННЫЕ ===
        private Player _thrower;
        private Player _target;
        private Rigidbody _rb;
        private float _activationTimer;
        private float _searchTimer;
        private bool _isActive;
        
        // Кэш конфига
        private float _speed;
        private float _killRadiusSq;
        private float _searchRadiusSq;
        private float _damage;
        private string _deathReason;

        // === ИНИЦИАЛИЗАЦИЯ ===
        public void Init(Player thrower)
        {
            _thrower = thrower;
            _rb = GetComponent<Rigidbody>();
            
            var config = Plugin.Instance.Config;
            
            _activationTimer = config.RatActivationDelay;
            _speed = config.RatSpeed;
            _damage = config.RatDamage;
            _deathReason = config.RatDeathReason;
            
            // Вычисляем квадраты радиусов
            _killRadiusSq = config.RatKillRadius * config.RatKillRadius;
            _searchRadiusSq = config.RatSearchRadius * config.RatSearchRadius;
        }

        // === ОСНОВНОЙ ЦИКЛ ===
        private void FixedUpdate()
        {
            // 1. Процесс активации
            if (!_isActive)
            {
                _activationTimer -= Time.fixedDeltaTime;
                if (_activationTimer > 0)
                    return; // Ждём таймер
                    
                _isActive = true;
                FindNewTarget();
            }

            // 2. Валидация текущей цели (не померла ли, не стала ли SCP)
            if (_target != null && (!_target.IsAlive || _target.Role.Team == Team.SCPs))
            {
                _target = null;
            }

            // 3. Поиск новой цели, если текущая потеряна
            if (_target == null)
            {
                _searchTimer -= Time.fixedDeltaTime;
                if (_searchTimer <= 0)
                {
                    FindNewTarget();
                }
                
                if (_target == null)
                    return; // Если так никого и не нашли - стоим
            }

            // 4. Движение и детонация
            Vector3 myPos = transform.position;
            Vector3 diff = _target.Position - myPos;
            float sqrDistance = diff.sqrMagnitude; // 3D-дистанция для взрыва

            MoveTowardsTarget(diff);

            if (sqrDistance <= _killRadiusSq)
            {
                Detonate();
            }
        }

        // === ПОИСК ЦЕЛИ ===
        private void FindNewTarget()
        {
            _searchTimer = 0.5f; 

            float minDistanceSq = _searchRadiusSq;
            Player closestTarget = null;
            Vector3 myPos = transform.position;

            foreach (Player p in Player.List)
            {
                if (!p.IsAlive || p == _thrower || p.Role.Team == Team.SCPs) 
                    continue;

                float distSq = (p.Position - myPos).sqrMagnitude;
                if (distSq <= minDistanceSq)
                {
                    minDistanceSq = distSq;
                    closestTarget = p;
                }
            }
            
            _target = closestTarget;
        }

        // === ЛОГИКА ДВИЖЕНИЯ ===
        private void MoveTowardsTarget(Vector3 diff)
        {
            // Обнуляем высоту для расчета 2D вектора движения
            diff.y = 0;

            float horizontalSqrMag = diff.sqrMagnitude;
            
            // Если мы прямо под/над игроком - двигаться по горизонтали некуда (защита от деления на 0)
            if (horizontalSqrMag <= 0.0001f)
                return;

            // Нормализуем по горизонтали, извлекая корень только 1 раз
            Vector3 direction = diff / Mathf.Sqrt(horizontalSqrMag);
            Vector3 targetVelocity = direction * _speed;
            
            // Гравитацию берем от движка (используем linearVelocity для новых версий Unity)
            targetVelocity.y = _rb.linearVelocity.y; 
            
            _rb.linearVelocity = targetVelocity;
        }

        // === ВЗРЫВ ===
        private void Detonate()
        {
            _target.Hurt(_damage, _deathReason);
            
            // EXILED работает только на выделенных серверах, проверки NetworkServer.active не нужны
            NetworkServer.Destroy(gameObject);
        }
    }
}
