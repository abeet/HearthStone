﻿using Engine.Client;
using Engine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Server
{
    /// <summary>
    /// 服务器 - 客户端 通信协议
    /// </summary>
    public static class ServerResponse
    {
        /// <summary>
        /// 处理Request
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="requestType"></param>
        /// <returns></returns>
        public static string ProcessRequest(string Request, RequestType requestType)
        {
            string Response = string.Empty;
            MinimizeBattleInfo info = new MinimizeBattleInfo();
            int GameId;
            string IsHostStr;
            string IsFirstStr;
            bool IsHost;
            switch (requestType)
            {
                case RequestType.新建游戏:
                    //返回GameId
                    Response = GameServer.CreateNewGame_CS(Request.Substring(3)).ToString(GameServer.GameIdFormat);
                    break;
                case RequestType.加入游戏:
                    Response = GameServer.JoinGame_CS(int.Parse(Request.Substring(3, 5)), Request.Substring(8)).ToString();
                    break;
                case RequestType.开始游戏:
                    //[BS]
                    if (GameServer.GameWaitGuest_BS.Count == 0)
                    {
                        GameId = GameServer.CreateNewGame_BS(Request.Substring(3));
                        IsHostStr = CardUtility.strTrue;
                        IsFirstStr = GameServer.GameWaitGuest_BS[GameId].HostAsFirst ? CardUtility.strTrue : CardUtility.strFalse;
                    }
                    else
                    {
                        GameId = GameServer.JoinGame_BS(GameServer.GameWaitGuest_BS.Keys.ToList()[0], string.Empty);
                        IsHostStr = CardUtility.strFalse;
                        IsFirstStr = GameServer.GameRunning_BS[GameId].HostAsFirst ? CardUtility.strFalse : CardUtility.strTrue;
                    }
                    // GameId + IsHost + IsFirst
                    Response = GameId.ToString(GameServer.GameIdFormat) + IsHostStr + IsFirstStr;
                    break;
                case RequestType.传送套牌:
                    //[BS/CS]
                    Stack<string> Deck = new Stack<string>();
                    foreach (var card in Request.Substring(9).Split(CardUtility.strSplitArrayMark.ToCharArray()))
                    {
                        Deck.Push(card);
                    }
                    GameServer.SetCardStack(int.Parse(Request.Substring(3, 5)), Request.Substring(8, 1) == CardUtility.strTrue, Deck);
                    Response = CardUtility.strTrue;
                    break;
                case RequestType.初始化状态:
                    //[BS]
                    GameId = int.Parse(Request.Substring(3, 5));
                    GameServer.GameRunning_BS[GameId].InitPlayInfo();
                    Response = CardUtility.strTrue;
                    break;
                case RequestType.等待游戏列表:
                    Response = GameServer.GetWaitGameList();
                    break;
                case RequestType.游戏启动状态:
                    Response = GameServer.IsGameStart(int.Parse(Request.Substring(3, 5))).ToString();
                    break;
                case RequestType.先后手状态:
                    Response = GameServer.IsFirst(int.Parse(Request.Substring(3, 5)), Request.Substring(8, 1) == CardUtility.strTrue) ? CardUtility.strTrue : CardUtility.strFalse;
                    break;
                case RequestType.抽牌:
                    var Cardlist = GameServer.DrawCard(int.Parse(Request.Substring(3, 5)), Request.Substring(8, 1) == CardUtility.strTrue, int.Parse(Request.Substring(9, 1)));
                    Response = string.Join(CardUtility.strSplitArrayMark, Cardlist.ToArray());
                    break;
                case RequestType.回合结束:
                    if (SystemManager.游戏类型 == SystemManager.GameType.HTML版)
                    {
                        GameId = int.Parse(Request.Substring(3, 5));
                        IsHost = Request.Substring(8, 1) == CardUtility.strTrue;
                        GameServer.GameRunning_BS[GameId].TurnEnd(IsHost);
                        Response = IsHost ? CardUtility.strTrue : CardUtility.strFalse;
                    }
                    else
                    {
                        GameServer.WriteAction(int.Parse(Request.Substring(3, 5)), Request.Substring(8));
                    }
                    break;
                case RequestType.写入行动:
                    GameServer.WriteAction(int.Parse(Request.Substring(3, 5)), Request.Substring(8));
                    break;
                case RequestType.读取行动:
                    Response = GameServer.ReadAction(int.Parse(Request.Substring(3, 5)));
                    break;
                case RequestType.奥秘判定:
                    Response = GameServer.SecretHit(int.Parse(Request.Substring(3, 5)), Request.Substring(8, 1) == CardUtility.strTrue, Request.Substring(9));
                    break;
                case RequestType.使用手牌:
                    GameId = int.Parse(Request.Substring(3, 5));
                    IsHost = Request.Substring(8, 1) == CardUtility.strTrue;
                    //这里可能产生中断
                    var interrput = GameServer.UseHandCard(GameId, IsHost, Request.Substring(9), 1, string.Empty);
                    Response = interrput.ToJson();
                    if (interrput.ActionName == CardUtility.strOK)
                    {
                        Response = GetOkResponse(Request.Substring(9), interrput);
                    }
                    else
                    {
                        Response = interrput.ToJson();
                    }
                    break;
                case RequestType.战场状态:
                    GameId = int.Parse(Request.Substring(3, 5));
                    IsHost = Request.Substring(8, 1) == CardUtility.strTrue;
                    //WebSocket将会同时将信息发送给双方，所以这里发送以HOST为主视角的战场信息
                    info.Init(GameServer.GameRunning_BS[GameId].gameStatus(IsHost));
                    Response = info.ToJson();
                    break;
                case RequestType.中断续行:
                    GameId = int.Parse(Request.Substring(3, 5));
                    IsHost = Request.Substring(8, 1) == CardUtility.strTrue;
                    RequestType ResumeType = (RequestType)Enum.Parse(typeof(RequestType), Request.Substring(9, 3));
                    int Step = int.Parse(Request.Substring(12, 1));
                    string CardSN = Request.Substring(13, 7);
                    if (ResumeType == RequestType.使用手牌)
                    {
                        var resume = GameServer.UseHandCard(GameId, IsHost, CardSN, Step, Request.Substring(20));
                        if (resume.ActionName == CardUtility.strOK)
                        {
                            Response = GetOkResponse(CardSN, resume);
                        }
                        else
                        {
                            Response = resume.ToJson();
                        }
                    }
                    requestType = ResumeType;
                    break;
                case RequestType.攻击行为:
                    GameId = int.Parse(Request.Substring(3, 5));
                    IsHost = Request.Substring(8, 1) == CardUtility.strTrue;
                    //017000010ME#1|YOU#2|
                    int MyPos = int.Parse(Request.Substring(12, 1));
                    int YourPos = int.Parse(Request.Substring(18, 1));
                    GameServer.Fight(GameId, IsHost, MyPos, YourPos);
                    Response = Request.Substring(12, 1) + Request.Substring(18, 1);
                    break;
                case RequestType.可攻击对象:
                    GameId = int.Parse(Request.Substring(3, 5));
                    IsHost = Request.Substring(8, 1) == CardUtility.strTrue;
                    Response = GameServer.GetFightTargetList(GameId, IsHost);
                    break;
                default:
                    //结束游戏
                    break;
            }
            if (SystemManager.游戏类型 == SystemManager.GameType.HTML版) Response = requestType.GetHashCode().ToString("D3") + Response;
            return Response;
        }
        /// <summary>
        /// 移除游戏
        /// </summary>
        /// <param name="gameId"></param>
        internal static void RemoveGame(string gameId)
        {
            GameServer.GameRunning_BS.Remove(int.Parse(gameId));
        }

        /// <summary>
        /// GetOkResponse
        /// </summary>
        /// <param name="CardSN"></param>
        /// <param name="resume"></param>
        /// <returns></returns>
        private static string GetOkResponse(string CardSN, Control.FullServerManager.Interrupt resume)
        {
            string Response;
            switch (CardUtility.GetCardInfoBySN(CardSN).卡牌种类)
            {
                case Card.CardBasicInfo.卡牌类型枚举.随从:
                    resume.ExternalInfo = "MINION";
                    break;
                case Card.CardBasicInfo.卡牌类型枚举.法术:
                    resume.ExternalInfo = "SPELL";
                    break;
                case Card.CardBasicInfo.卡牌类型枚举.武器:
                    resume.ExternalInfo = "WEAPON";
                    break;
                case Card.CardBasicInfo.卡牌类型枚举.奥秘:
                    resume.ExternalInfo = "SECRET";
                    break;
                case Card.CardBasicInfo.卡牌类型枚举.其他:
                    break;
                default:
                    break;
            }
            Response = resume.ToJson();
            Response = CardUtility.strOK + Response;
            return Response;
        }
        /// <summary>
        /// 获取运行中游戏列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetRunningList()
        {
            List<String> running = new List<string>();
            foreach (var game in GameServer.GameRunning_BS)
            {
                running.Add(game.Key.ToString("D5"));
            }
            return running;
        }
        /// <summary>
        /// 消息类型(3位)
        /// </summary>
        public enum RequestType
        {
            /// <summary>
            /// 新建一个游戏
            /// </summary>
            新建游戏,
            /// <summary>
            /// 传送套牌
            /// </summary>
            传送套牌,
            /// <summary>
            /// 获得等待中游戏列表
            /// </summary>
            等待游戏列表,
            /// <summary>
            /// 加入一个游戏
            /// </summary>
            加入游戏,
            /// <summary>
            /// 主机询问是否游戏已经启动
            /// </summary>
            游戏启动状态,
            /// <summary>
            /// 确认先后手状态
            /// </summary>
            先后手状态,
            /// <summary>
            /// 认输，退出一个游戏
            /// </summary>
            认输,
            /// <summary>
            /// 抽牌
            /// </summary>
            抽牌,
            /// <summary>
            /// 回合结束
            /// </summary>
            回合结束,
            /// <summary>
            /// 写入行动
            /// </summary>
            写入行动,
            /// <summary>
            /// 读取行动
            /// </summary>
            读取行动,
            /// <summary>
            /// 奥秘判定
            /// </summary>
            奥秘判定,
            /// <summary>
            /// 使用手牌
            /// </summary>
            使用手牌,
            /// <summary>
            /// 战场状态
            /// </summary>
            战场状态,
            /// <summary>
            /// 开始一个游戏[BS]
            /// 如果没有等待中的游戏，则新建
            /// 不然就加入一个游戏
            /// </summary>
            开始游戏,
            /// <summary>
            /// 初始化状态[BS]
            /// </summary>
            初始化状态,
            /// <summary>
            /// 中断续行[BS]
            /// </summary>
            中断续行,
            /// <summary>
            /// 攻击行为
            /// </summary>
            攻击行为,
            /// <summary>
            /// 可攻击对象
            /// </summary>
            可攻击对象,
            /// <summary>
            /// 结束游戏
            /// </summary>
            结束游戏
        }
    }
}
