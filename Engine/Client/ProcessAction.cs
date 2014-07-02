﻿using Engine.Card;
using Engine.Effect;
using Engine.Server;
using Engine.Utility;

namespace Engine.Client
{
    /// <summary>
    /// 处理对方的动作
    /// </summary>
    public static class ProcessAction
    {
        /// <summary>
        /// 处理对方的动作
        /// </summary>
        /// <param name="ActionCommand"></param>
        /// <param name="game"></param>
        public static void Process(string ActionCommand, ClientPlayerInfo game)
        {
            string[] actField = ActionCommand.Split(CardUtility.strSplitMark.ToCharArray());
            switch (Engine.Server.ActionCode.GetActionType(ActionCommand))
            {
                case ActionCode.ActionType.Card:
                    CardEffect.ReRunEffect(1,game, actField);
                    break;
                case ActionCode.ActionType.UseMinion:
                    int Pos = int.Parse(actField[2]);
                    var minion = (Engine.Card.MinionCard)Engine.Utility.CardUtility.GetCardInfoBySN(actField[1]);
                    minion.初始化();
                    game.YourInfo.BattleField.PutToBattle(Pos, minion);
                    game.YourInfo.BattleField.ResetBuff();
                    break;
                case ActionCode.ActionType.UseWeapon:
                    game.YourInfo.Weapon = (Engine.Card.WeaponCard)Engine.Utility.CardUtility.GetCardInfoBySN(actField[1]);
                    break;
                case ActionCode.ActionType.UseSecret:
                    game.YourInfo.SecretCount++; ;
                    break;
                case ActionCode.ActionType.UseAbility:
                    break;
                case ActionCode.ActionType.Fight:
                    //FIGHT#1#2
                    FightHandler.Fight(int.Parse(actField[2]), int.Parse(actField[1]), game, false);
                    break;
                case ActionCode.ActionType.Point:
                    IAtomicEffect point = new PointEffect();
                    point.ReRunEffect(game, actField);
                    break;
                case ActionCode.ActionType.Health:
                    IAtomicEffect health = new HealthEffect();
                    health.ReRunEffect(game, actField);
                    break;
                case ActionCode.ActionType.Status:
                    IAtomicEffect status = new StatusEffect();
                    status.ReRunEffect(game, actField);
                    break;
                case ActionCode.ActionType.Transform:
                    IAtomicEffect transform = new TransformEffect();
                    transform.ReRunEffect(game, actField);
                    break;
                case ActionCode.ActionType.Attack:
                    IAtomicEffect attack = new AttackEffect();
                    attack.ReRunEffect(game, actField);
                    break;
                case ActionCode.ActionType.HitSecret:
                    SecretCard.ReRunSecret(game, actField);
                    break;
                case ActionCode.ActionType.Control:
                    ControlEffect.ReRunEffect(game, actField);
                    break;
                case ActionCode.ActionType.Summon:
                    SummonEffect.ReRunEffect(game, actField);
                    break;
                case ActionCode.ActionType.Crystal:
                    CrystalEffect.ReRunEffect(game, actField);
                    break;
                case ActionCode.ActionType.WeaponPoint:
                    WeaponPointEffect.ReRunEffect(game, actField);
                    break;
                case ActionCode.ActionType.Settle:
                    ClientManager.Settle(game);
                    break;
            }
        }
    }
}
