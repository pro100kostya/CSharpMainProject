using System.Collections.Generic;
using System.Linq;
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
        private List<Vector2Int> TargetsOutOfRange = new();

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
            Vector2Int target = TargetsOutOfRange.Count > 0 ? TargetsOutOfRange[0] : unit.Pos;
            if (IsTargetInRange(target))
            {
                return unit.Pos;
            }
            else
            {
                return unit.Pos.CalcNextStepTowards(target);
            }
            
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            float min = float.MaxValue;
            var rightTarget = Vector2Int.zero;
           
                foreach (var target in GetAllTargets())
                {
                    var minDistance = DistanceToOwnBase(target);
                    if (minDistance < min)
                    {
                        min = minDistance;
                        rightTarget = target;
                    }
                }

                TargetsOutOfRange.Clear();

                if (min != float.MaxValue)
                {
                    TargetsOutOfRange.Add(rightTarget);
                    if(IsTargetInRange(rightTarget)) result.Add(rightTarget);
                }
                else
                {
                    var enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                    TargetsOutOfRange.Add(enemyBase);
                }

            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown / 10);
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
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}