﻿using Engine.Card;
using Engine.Client;
using System;
using System.Collections.Generic;

namespace Engine.Effect
{
    public interface IAtomicEffect
    {
        /// <summary>
        /// 对方复原操作
        /// </summary>
        /// <param name="game"></param>
        /// <param name="actField"></param>
        void ReRunEffect(ClientPlayerInfo game, String[] actField);
        /// <summary>
        /// 对英雄动作
        /// </summary>
        /// <param name="game"></param>
        /// <param name="PlayInfo"></param>
        /// <returns></returns>
        String DealHero(Client.ClientPlayerInfo game, PublicInfo PlayInfo);
        /// <summary>
        /// 对随从动作
        /// </summary>
        /// <param name="game"></param>
        /// <param name="Minion"></param>
        /// <returns></returns>
        String DealMinion(Client.ClientPlayerInfo game, MinionCard Minion);
        /// <summary>
        /// 获得效果信息
        /// </summary>
        /// <param name="InfoArray"></param>
        void GetField(List<String> InfoArray);
    }
}
