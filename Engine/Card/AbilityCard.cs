﻿using Engine.Client;
using Engine.Effect;
using Engine.Utility;
using System;
using System.Collections.Generic;

namespace Engine.Card
{
    /// <summary>
    /// 法术卡牌
    /// </summary>
    [Serializable]
    public class AbilityCard : CardBasicInfo
    {
        /// <summary>
        /// 原生法术
        /// </summary>
        public const String 原生法术 = "0";
        /// <summary>
        /// 幸运币
        /// </summary>
        public const String SN幸运币 = "A900001";
        /// <summary>
        /// 效果选择类型枚举
        /// </summary>
        public enum 效果选择类型枚举
        {
            /// <summary>
            /// 无需选择
            /// </summary>
            无需选择,
            /// <summary>
            /// 用户主动
            /// </summary>
            主动选择,
            /// <summary>
            /// 自动判定
            /// </summary>
            自动判定
        }
        /// <summary>
        /// 效果选择类型枚举
        /// </summary>
        public 效果选择类型枚举 效果选择类型 = 效果选择类型枚举.无需选择;
        /// <summary>
        /// 效果选择条件
        /// </summary>
        public String 效果选择条件 = String.Empty;
        /// <summary>
        /// 效果回数表达式
        /// </summary>
        public String 效果回数表达式 = String.Empty;
        /// <summary>
        /// 第一效果
        /// </summary>
        public AbilityDefine FirstAbilityDefine = new AbilityDefine();
        /// <summary>
        /// 第二效果
        /// </summary>
        public AbilityDefine SecondAbilityDefine = new AbilityDefine();
        /// <summary>
        /// 效果定义
        /// </summary>
        [Serializable]
        public struct AbilityDefine
        {
            /// <summary>
            /// 描述
            /// </summary>
            public String 描述;
            /// <summary>
            /// 主效果定义
            /// </summary>
            public EffectDefine MainAbilityDefine;
            /// <summary>
            /// 追加效果定义
            /// </summary>
            public EffectDefine AppendAbilityDefine;
            /// <summary>
            /// 追加效果启动条件
            /// </summary>
            public String AppendEffectCondition;
            /// <summary>
            /// 初始化
            /// </summary>
            public void Init()
            {
                MainAbilityDefine = new EffectDefine();
                AppendAbilityDefine = new EffectDefine();
            }
        }
        /// <summary>
        /// 使用法术
        /// </summary>
        /// <param name="game"></param>
        /// <param name="ConvertPosDirect">对象方向转换</param>
        public List<String> UseAbility(GameManager game,
                                       Boolean ConvertPosDirect)
        {
            List<String> Result = new List<string>();
            Engine.Utility.CardUtility.PickEffect PickEffectResult = CardUtility.PickEffect.第一效果;
            switch (效果选择类型)
            {
                case 效果选择类型枚举.无需选择:
                    break;
                case 效果选择类型枚举.主动选择:
                    PickEffectResult = game.PickEffect(FirstAbilityDefine.描述, SecondAbilityDefine.描述);
                    if (PickEffectResult == CardUtility.PickEffect.取消) return new List<string>();
                    break;
                case 效果选择类型枚举.自动判定:
                    if (!ExpressHandler.AbilityPickCondition(game, 效果选择条件)) PickEffectResult = CardUtility.PickEffect.第二效果;
                    break;
                default:
                    break;
            }
            List<EffectDefine> SingleEffectList = new List<EffectDefine>();
            AbilityCard.AbilityDefine ability;
            if (PickEffectResult == CardUtility.PickEffect.第一效果)
            {
                ability = FirstAbilityDefine;
            }
            else
            {
                ability = SecondAbilityDefine;
            }
            Result.AddRange(RunAbilityEffect(game, ConvertPosDirect, ability));
            return Result;
        }
        /// <summary>
        /// 运行法术
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="ConvertPosDirect"></param>
        /// <param name="Ability"></param>
        /// <param name="TargetPosInfo"></param>
        /// <returns></returns>
        private List<String> RunAbilityEffect(GameManager game,
                                              Boolean ConvertPosDirect,
                                              AbilityCard.AbilityDefine Ability)
        {
            List<String> Result = new List<string>();

            //对象选择处理
            if (Ability.MainAbilityDefine.AbliltyPosPicker.EffictTargetSelectMode == CardUtility.TargetSelectModeEnum.指定 ||
                Ability.MainAbilityDefine.AbliltyPosPicker.EffictTargetSelectMode == CardUtility.TargetSelectModeEnum.横扫 ||
                Ability.MainAbilityDefine.AbliltyPosPicker.EffictTargetSelectMode == CardUtility.TargetSelectModeEnum.相邻)
            {
                Ability.MainAbilityDefine.AbliltyPosPicker.SelectedPos = game.GetSelectTarget(Ability.MainAbilityDefine.AbliltyPosPicker);
            }
            else
            {
                if (ConvertPosDirect)
                {
                    switch (Ability.MainAbilityDefine.AbliltyPosPicker.EffectTargetSelectDirect)
                    {
                        case CardUtility.TargetSelectDirectEnum.本方:
                            Ability.MainAbilityDefine.AbliltyPosPicker.EffectTargetSelectDirect = CardUtility.TargetSelectDirectEnum.对方;
                            break;
                        case CardUtility.TargetSelectDirectEnum.对方:
                            Ability.MainAbilityDefine.AbliltyPosPicker.EffectTargetSelectDirect = CardUtility.TargetSelectDirectEnum.本方;
                            break;
                        case CardUtility.TargetSelectDirectEnum.双方:
                            break;
                        default:
                            break;
                    }
                }
            }

            //取消处理
            if (Ability.MainAbilityDefine.AbliltyPosPicker.SelectedPos.Postion == -1)
            {
                Result.Clear();
                return Result;
            }

            //法术伤害对于攻击型效果的加成
            if (Ability.MainAbilityDefine.效果条件 == CardUtility.strIgnore && Ability.MainAbilityDefine.EffectCount > 1)
            {
                Ability.MainAbilityDefine.EffectCount += game.MyInfo.BattleField.AbilityDamagePlus;
            }
            //按照回数执行效果
            for (int cnt = 0; cnt < Ability.MainAbilityDefine.EffectCount; cnt++)
            {
                //系统法术
                switch (Ability.MainAbilityDefine.TrueAtomicEffect.AtomicEffectType)
                {
                    case AtomicEffectDefine.AtomicEffectEnum.卡牌:
                    case AtomicEffectDefine.AtomicEffectEnum.水晶:
                    case AtomicEffectDefine.AtomicEffectEnum.奥秘:
                    case AtomicEffectDefine.AtomicEffectEnum.召唤:
                    case AtomicEffectDefine.AtomicEffectEnum.武器:
                    case AtomicEffectDefine.AtomicEffectEnum.控制:
                        Result.AddRange(RunGameSystemEffect(game, ConvertPosDirect, Ability.MainAbilityDefine.TrueAtomicEffect, Ability.MainAbilityDefine.AbliltyPosPicker));
                        break;
                    default:
                        Result.AddRange(Effecthandler.RunSingleEffect(Ability.MainAbilityDefine, game, GameManager.RandomSeed));
                        break;
                }

                GameManager.RandomSeed++;
                Result.AddRange(game.Settle());
            }
            return Result;
        }
        /// <summary>
        /// 针对系统的法术效果
        /// </summary>
        /// <param name="game"></param>
        /// <param name="ConvertPosDirect"></param>
        /// <param name="Ability"></param>
        /// <returns></returns>
        private List<string> RunGameSystemEffect(GameManager game, bool ConvertPosDirect, AtomicEffectDefine effect, CardUtility.PositionSelectOption Option)
        {
            List<string> Result = new List<string>();
            switch (effect.AtomicEffectType)
            {
                case AtomicEffectDefine.AtomicEffectEnum.卡牌:
                    CardEffect cardatomic = new CardEffect();
                    cardatomic.GetField(effect.InfoArray);
                    return cardatomic.RunEffect(game, Option.EffectTargetSelectDirect);
                case AtomicEffectDefine.AtomicEffectEnum.水晶:
                    CrystalEffect crystalatomic = new CrystalEffect();
                    crystalatomic.GetField(effect.InfoArray);
                    return crystalatomic.RunEffect(game, Option.EffectTargetSelectDirect);
                case AtomicEffectDefine.AtomicEffectEnum.奥秘:
                    break;
                case AtomicEffectDefine.AtomicEffectEnum.武器:
                    break;
                case AtomicEffectDefine.AtomicEffectEnum.召唤:
                    break;
                case AtomicEffectDefine.AtomicEffectEnum.控制:
                    break;
            }
            return Result;
        }
    }
}