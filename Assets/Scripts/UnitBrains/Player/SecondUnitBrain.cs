using System.Collections.Generic;
using Codice.CM.Common.Tree.Partial;
using GluonGui.Dialog;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        public List<Vector2Int> TargetOutOfRange;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float temp = GetTemperature();

            if (temp >= overheatTemperature) return;
            
            for (int i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
        }
  
        public override Vector2Int GetNextStep()
        {
           if (TargetOutOfRange.Count > 0)
            {
                var targetOutOfRange = TargetOutOfRange[0];
                return unit.Pos.CalcNextStepTowards(targetOutOfRange);
            }
           else
            {
                return unit.Pos;
            }
        }

        protected override List<Vector2Int> SelectTargets()             // тут домашка
        {
            List<Vector2Int> result = GetReachableTargets();

            float min = float.MaxValue;
            var rightTarget = Vector2Int.zero;
            TargetOutOfRange = new();

            if (result.Count > 0)
            {
                foreach (var target in GetAllTargets())
                {
                    var minDistance = DistanceToOwnBase(target);
                    if (minDistance < min)
                    {
                        min = minDistance;
                        rightTarget = target;
                    }
                }
                if (min != float.MaxValue && IsTargetInRange(rightTarget))
                {
                    result.Clear();
                    result.Add(rightTarget);
                }
                if (min != float.MaxValue && !IsTargetInRange(rightTarget))
                {
                    TargetOutOfRange.Clear();
                    TargetOutOfRange.Add(rightTarget);
                }
            }
            else
            {
                var enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                if (IsTargetInRange(enemyBase))
                {
                    result.Clear();
                    result.Add(enemyBase);
                }
                else
                {
                    TargetOutOfRange.Clear();
                    TargetOutOfRange.Add(enemyBase);
                }
                
            }



            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}