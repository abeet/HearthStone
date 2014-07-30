﻿using Engine.Card;
using Engine.Client;
using Engine.Effect;
using Engine.Utility;
using System.Collections.Generic;

namespace Engine.Action
{
    /// <summary>
    /// 施法动作
    /// </summary>
    public static class UseSpellAction
    {
        /// <summary>
        /// 使用法术
        /// </summary>
        /// <param name="game"></param>
        /// <param name="IsMyAction">对象方向转换</param>
        public static void RunBS(ActionStatus game, string CardSn)
        {
            List<string> Result = new List<string>();
            SpellCard spell = (SpellCard)CardUtility.GetCardInfoBySN(CardSn);
            //Step1
            CardUtility.抉择枚举 PickAbilityResult = CardUtility.抉择枚举.第一效果;
            SpellCard.AbilityDefine ability = new SpellCard.AbilityDefine();
            if (game.Interrupt.Step == 1)
            {
                switch (spell.效果选择类型)
                {
                    case SpellCard.效果选择类型枚举.无需选择:
                        game.Interrupt.ExternalInfo = "1";
                        break;
                    case SpellCard.效果选择类型枚举.主动选择:
                        game.Interrupt.Step = 2;
                        game.Interrupt.ActionName = "SPELLDECIDE";
                        game.Interrupt.ExternalInfo = spell.FirstAbilityDefine.描述 + CardUtility.strSplitArrayMark + spell.SecondAbilityDefine.描述;
                        return;
                    case SpellCard.效果选择类型枚举.自动判定:
                        game.Interrupt.ExternalInfo = "1";
                        if (!ExpressHandler.AbilityPickCondition(game, spell.效果选择条件))
                        {
                            PickAbilityResult = CardUtility.抉择枚举.第二效果;
                            game.Interrupt.ExternalInfo = "2";
                        }
                        break;
                    default:
                        break;
                }
                game.Interrupt.Step = 2;
            }
            //Step2
            if (game.Interrupt.Step == 2)
            {
                if (spell.效果选择类型 == SpellCard.效果选择类型枚举.主动选择 && SystemManager.游戏类型 == SystemManager.GameType.HTML版)
                {
                    switch (game.Interrupt.SessionDic["SPELLDECIDE"])
                    {
                        case "1":
                            PickAbilityResult = CardUtility.抉择枚举.第一效果;
                            break;
                        case "2":
                            PickAbilityResult = CardUtility.抉择枚举.第二效果;
                            break;
                        default:
                            PickAbilityResult = CardUtility.抉择枚举.取消;
                            break;
                    }
                }
                if (PickAbilityResult != CardUtility.抉择枚举.取消)
                {
                    List<EffectDefine> SingleEffectList = new List<EffectDefine>();
                    if (PickAbilityResult == CardUtility.抉择枚举.第一效果)
                    {
                        ability = spell.FirstAbilityDefine;
                    }
                    else
                    {
                        ability = spell.SecondAbilityDefine;
                    }
                    if (game.ActionName == "USEMINIONCARD")
                    {
                        //如果整个大的动作时随从入场，并且如果这个战吼不需要指定位置，则现在的话，将入场随从设定为指定位置
                        ability.MainAbilityDefine.AbliltyPosPicker.SelectedPos.本方对方标识 = true;
                        ability.MainAbilityDefine.AbliltyPosPicker.SelectedPos.位置 = int.Parse(game.Interrupt.SessionDic["MINIONPOSITION"]);
                    }
                    if (ability.IsNeedTargetSelect())
                    {
                        if (game.ActionName == "USEMINIONCARD" && game.Interrupt.SessionDic.ContainsKey("BATTLECRYPOSITION"))
                        {
                            ability.MainAbilityDefine.AbliltyPosPicker.SelectedPos = CardUtility.指定位置结构体.FromString(game.Interrupt.SessionDic["BATTLECRYPOSITION"]);
                        }
                        else
                        {
                            if (game.ActionName == "USESPELLCARD" && game.Interrupt.SessionDic.ContainsKey("SPELLPOSITION"))
                            {
                                ability.MainAbilityDefine.AbliltyPosPicker.SelectedPos = CardUtility.指定位置结构体.FromString(game.Interrupt.SessionDic["SPELLPOSITION"]);
                            }
                            else
                            {
                                ability.MainAbilityDefine.AbliltyPosPicker.CanNotSelectPos.位置 = BattleFieldInfo.UnknowPos;
                                SelectUtility.SetTargetSelectEnable(ability.MainAbilityDefine.AbliltyPosPicker, game);
                                game.Interrupt.ExternalInfo = SelectUtility.GetTargetListString(game);
                                game.Interrupt.ActionName = "SPELLPOSITION";
                                return;
                            }
                        }
                    }
                    game.Interrupt.Step = 3;
                }
                else
                {
                    game.Interrupt.Step = -1;
                    return;
                }
            }

            if (game.Interrupt.Step == 3)
            {
                SpellCard.RunAbility(game, ability);
                if (spell.法术卡牌类型 == CardBasicInfo.法术卡牌类型枚举.原生卡牌)
                    game.battleEvenetHandler.事件池.Add(new CardUtility.全局事件()
                    {
                        触发事件类型 = CardUtility.事件类型枚举.施法,
                        触发位置 = new CardUtility.指定位置结构体()
                        {
                            位置 = BattleFieldInfo.HeroPos,
                            本方对方标识 = true
                        }
                    });
            }
            game.Interrupt.Step = 99;
            game.Interrupt.ActionName = CardUtility.strOK;
        }
    }
}
