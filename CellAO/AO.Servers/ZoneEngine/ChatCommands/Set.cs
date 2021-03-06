﻿#region License

// Copyright (c) 2005-2013, CellAO Team
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of the CellAO Team nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

namespace ZoneEngine.ChatCommands
{
    #region Usings ...

    using System;
    using System.Collections.Generic;

    using SmokeLounge.AOtomation.Messaging.GameData;

    using ZoneEngine.Function;
    using ZoneEngine.GameObject;
    using ZoneEngine.GameObject.Items;
    using ZoneEngine.GameObject.Stats;
    using ZoneEngine.Network;
    using ZoneEngine.Network.InternalBus.InternalMessages;
    using ZoneEngine.Script;

    #endregion

    /// <summary>
    /// </summary>
    public class ChatCommandSet : AOChatCommand
    {
        /// <summary>
        /// </summary>
        /// <param name="client">
        /// </param>
        /// <param name="target">
        /// </param>
        /// <param name="args">
        /// </param>
        public override void ExecuteCommand(Client client, Identity target, string[] args)
        {
            // Fallback to self if no target is selected
            bool fallback = false;
            if (target.Instance == 0)
            {
                target = client.Character.Identity;
                fallback = true;
            }

            int statId = 0;
            try
            {
                statId = StatNamesDefaults.GetStatNumber(args[1]);
            }
            catch (Exception)
            {
                client.SendChatText("Unknown Stat name " + args[1]);
                return;
            }

            uint statNewValue = 1234567890;
            try
            {
                statNewValue = uint.Parse(args[2]);
            }
            catch
            {
                try
                {
                    // For values >= 2^31
                    statNewValue = uint.Parse(args[2]);
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {
                }
            }

            IStats tempch = client.Playfield.FindByIdentity(target);
            if (tempch == null)
            {
                client.SendChatText("Target vanished? This should NOT be reached");
            }

            uint statOldValue;
            try
            {
                statOldValue = tempch.Stats[statId].BaseValue;
                var IM =
                    new IMExecuteFunction(
                        new Functions
                            {
                                Target = Constants.ItemtargetSelectedtarget, 
                                FunctionType = Constants.FunctiontypeSet, 
                                Requirements = new List<Requirements>(), 
                                Arguments =
                                    new FunctionArguments
                                        {
                                            Values =
                                                new List<object> { statId, (int)statNewValue }
                                        }, 
                                TickCount = 1, 
                                TickInterval = 1, 
                                dolocalstats = true
                            }, 
                        ((INamedEntity)tempch).Identity);
                if (fallback)
                {
                    IM.Function.Target = Constants.ItemtargetSelf;
                }

                if (tempch.CheckRequirements(IM.Function, true))
                {
                    ((IInstancedEntity)tempch).Playfield.Publish(IM);
                }
            }
            catch
            {
                client.SendChatText("Unknown StatId " + statId);
                return;
            }

            INamedEntity namedEntity = tempch as INamedEntity;
            string response;
            if (namedEntity != null)
            {
                response = "Character " + namedEntity.Name + " (" + target.Instance + "): Stat "
                           + StatNamesDefaults.GetStatName(statId) + " (" + statId + ") =";
            }
            else
            {
                response = "Dynel (" + ((IInstancedEntity)tempch).Identity.Instance + "): Stat "
                           + StatNamesDefaults.GetStatName(statId) + " (" + statId + ") =";
            }

            response += " Old: " + statOldValue;
            response += " New: " + statNewValue;
            client.SendChatText(response);
        }

        /// <summary>
        /// </summary>
        /// <param name="client">
        /// </param>
        public override void CommandHelp(Client client)
        {
            // No help needed, no arguments can be given
            return;
        }

        /// <summary>
        /// </summary>
        /// <param name="args">
        /// </param>
        /// <returns>
        /// </returns>
        public override bool CheckCommandArguments(string[] args)
        {
            // Two different checks return true: <int> <uint> and <string> <uint>
            List<Type> check = new List<Type>();
            check.Add(typeof(int));
            check.Add(typeof(uint));
            bool check1 = CheckArgumentHelper(check, args);

            check.Clear();
            check.Add(typeof(string));
            check.Add(typeof(uint));
            check1 |= CheckArgumentHelper(check, args);
            return check1;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public override int GMLevelNeeded()
        {
            // Be a GM
            return 1;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public override List<string> ListCommands()
        {
            List<string> temp = new List<string>();
            temp.Add("set");
            return temp;
        }
    }
}