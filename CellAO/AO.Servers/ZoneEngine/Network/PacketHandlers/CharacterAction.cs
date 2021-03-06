﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CharacterAction.cs" company="CellAO Team">
//   Copyright © 2005-2013 CellAO Team.
//   
//   All rights reserved.
//   
//   Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//   
//       * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//       * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//       * Neither the name of the CellAO Team nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//   
//   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//   A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//   NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// <summary>
//   Defines the CharacterAction type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ZoneEngine.PacketHandlers
{
    using System;
    using System.Text;

    using AO.Core;
    using AO.Database.Dao;

    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

    using ZoneEngine.GameObject;
    using ZoneEngine.GameObject.Items;
    using ZoneEngine.Network;
    using ZoneEngine.Network.PacketHandlers;
    using ZoneEngine.Network.Packets;

    public static class CharacterAction
    {
        #region Public Methods and Operators

        public static void Read(CharacterActionMessage packet, Client client)
        {
            var mys = new SqlWrapper();

            var actionNum = (int)packet.Action;
            var unknown1 = packet.Unknown1;
            var args1 = packet.Parameter1;
            var args2 = packet.Parameter2;
            var unknown2 = packet.Unknown2;

            switch (actionNum)
            {
                case 19:
                    {
                        // Cast nano
                        // CastNanoSpell
                        var msg = new CastNanoSpellMessage
                        {
                            Identity = client.Character.Identity,
                            Unknown = 0x00,
                            NanoId = args2,
                            Target = packet.Target,
                            Unknown1 = 0x00000000,
                            Caster = client.Character.Identity
                        };

                        client.Character.Playfield.Announce(msg);

                        // CharacterAction 107
                        var characterAction107 = new CharacterActionMessage
                        {
                            Identity = client.Character.Identity,
                            Unknown = 0x00,
                            Action = CharacterActionType.Unknown1,
                            Unknown1 = 0x00000000,
                            Target = Identity.None,
                            Parameter1 = 1,
                            Parameter2 = args2,
                            Unknown2 = 0x0000
                        };
                        client.Character.Playfield.Announce(characterAction107);

                        // CharacterAction 98
                        var characterAction98 = new CharacterActionMessage
                        {
                            Identity = packet.Target,
                            Unknown = 0x00,
                            Action = CharacterActionType.Unknown2,
                            Unknown1 = 0x00000000,
                            Target =
                                new Identity
                                {
                                    Type =
                                        IdentityType
                                        .NanoProgram,
                                    Instance = args2
                                },
                            Parameter1 = client.Character.Identity.Instance,
                            Parameter2 = 0x249F0, // duration?
                            Unknown2 = 0x0000
                        };
                        client.Character.Playfield.Announce(characterAction98);
                    }

                    break;

                /* this is here to prevent server crash that is caused by
             * search action if server doesn't reply if something is
             * found or not */
                case 66:
                    {
                        // If action == search
                        /* Msg 110:136744723 = "No hidden objects found." */
                        // TODO: SEARCH!!
                        SendFeedback.Send(client, 110, 136744723);
                    }

                    break;

                case 105:
                    {
                        // If action == Info Request
                        IInstancedEntity tPlayer = client.Playfield.FindByIdentity(packet.Target);
                        var tChar = tPlayer as Character;
                        if (tChar != null)
                        {
                            var LegacyScore = tChar.Stats["PvP_Rating"].BaseValue;
                            string LegacyTitle = null;
                            if (LegacyScore < 1400)
                            {
                                LegacyTitle = string.Empty;
                            }
                            else if (LegacyScore < 1500)
                            {
                                LegacyTitle = "Freshman";
                            }
                            else if (LegacyScore < 1600)
                            {
                                LegacyTitle = "Rookie";
                            }
                            else if (LegacyScore < 1700)
                            {
                                LegacyTitle = "Apprentice";
                            }
                            else if (LegacyScore < 1800)
                            {
                                LegacyTitle = "Novice";
                            }
                            else if (LegacyScore < 1900)
                            {
                                LegacyTitle = "Neophyte";
                            }
                            else if (LegacyScore < 2000)
                            {
                                LegacyTitle = "Experienced";
                            }
                            else if (LegacyScore < 2100)
                            {
                                LegacyTitle = "Expert";
                            }
                            else if (LegacyScore < 2300)
                            {
                                LegacyTitle = "Master";
                            }
                            else if (LegacyScore < 2500)
                            {
                                LegacyTitle = "Champion";
                            }
                            else
                            {
                                LegacyTitle = "Grand Master";
                            }

                            var orgGoverningForm = 0;
                            var ms = new SqlWrapper();
                            var dt =
                                ms.ReadDatatable(
                                    "SELECT `GovernmentForm` FROM organizations WHERE ID=" + tChar.Stats["Clan"].BaseValue);

                            if (dt.Rows.Count > 0)
                            {
                                orgGoverningForm = (Int32)dt.Rows[0][0];
                            }

                            // Uses methods in ZoneEngine\PacketHandlers\OrgClient.cs
                            /* Known packetFlags--
                             * 0x40 - No org | 0x41 - Org | 0x43 - Org and towers | 0x47 - Org, towers, player has personal towers | 0x50 - No pvp data shown
                             * Bitflags--
                             * Bit0 = hasOrg, Bit1 = orgTowers, Bit2 = personalTowers, Bit3 = (Int32) time until supression changes (Byte) type of supression level?, Bit4 = noPvpDataShown, Bit5 = hasFaction, Bit6 = ?, Bit 7 = null.
                            */

                            int? orgId;
                            string orgRank;
                            InfoPacketType type;
                            if (tPlayer.Stats["Clan"].BaseValue == 0)
                            {
                                type = InfoPacketType.Character;
                                orgId = null;
                                orgRank = null;
                            }
                            else
                            {
                                type = InfoPacketType.CharacterOrg;
                                orgId = (int?)tPlayer.Stats["Clan"].BaseValue;
                                if (client.Character.Stats["Clan"].BaseValue == tPlayer.Stats["Clan"].BaseValue)
                                {
                                    orgRank = OrgClient.GetRank(
                                        orgGoverningForm, tPlayer.Stats["ClanLevel"].BaseValue);
                                }
                                else
                                {
                                    orgRank = string.Empty;
                                }
                            }

                            var info = new CharacterInfoPacket
                            {
                                Unknown1 = 0x01,
                                Profession =
                                    (Profession)
                                    tPlayer.Stats["Profession"].Value,
                                Level = (byte)tPlayer.Stats["Level"].Value,
                                TitleLevel =
                                    (byte)tPlayer.Stats["TitleLevel"].Value,
                                VisualProfession =
                                    (Profession)
                                    tPlayer.Stats["VisualProfession"].Value,
                                SideXp = 0,
                                Health = tPlayer.Stats["Health"].Value,
                                MaxHealth = tPlayer.Stats["Life"].Value,
                                BreedHostility = 0x00000000,
                                OrganizationId = orgId,
                                FirstName = tChar.FirstName,
                                LastName = tChar.LastName,
                                LegacyTitle = LegacyTitle,
                                Unknown2 = 0x0000,
                                OrganizationRank = orgRank,
                                TowerFields = null,
                                CityPlayfieldId = 0x00000000,
                                Towers = null,
                                InvadersKilled = tPlayer.Stats["InvadersKilled"].Value,
                                KilledByInvaders = tPlayer.Stats["KilledByInvaders"].Value,
                                AiLevel = tPlayer.Stats["AlienLevel"].Value,
                                PvpDuelWins = tPlayer.Stats["PvpDuelKills"].Value,
                                PvpDuelLoses = tPlayer.Stats["PvpDuelDeaths"].Value,
                                PvpProfessionDuelLoses = tPlayer.Stats["PvpProfessionDuelDeaths"].Value,
                                PvpSoloKills = tPlayer.Stats["PvpRankedSoloKills"].Value,
                                PvpTeamKills = tPlayer.Stats["PvpRankedTeamKills"].Value,
                                PvpSoloScore = tPlayer.Stats["PvpSoloScore"].Value,
                                PvpTeamScore = tPlayer.Stats["PvpTeamScore"].Value,
                                PvpDuelScore = tPlayer.Stats["PvpDuelScore"].Value
                            };

                            var infoPacketMessage = new InfoPacketMessage
                            {
                                Identity = tPlayer.Identity,
                                Unknown = 0x00,
                                Type = type,
                                Info = info
                            };

                            client.SendCompressed(infoPacketMessage);
                        }
                        else
                            // TODO: NPC's
                        {/*
                            var npc =
                                (NonPlayerCharacterClass)
                                FindDynel.FindDynelById(packet.Target);
                            if (npc != null)
                            {
                                var infoPacket = new PacketWriter();

                                // Start packet header
                                infoPacket.PushByte(0xDF);
                                infoPacket.PushByte(0xDF);
                                infoPacket.PushShort(10);
                                infoPacket.PushShort(1);
                                infoPacket.PushShort(0);
                                infoPacket.PushInt(3086); // sender (server ID)
                                infoPacket.PushInt(client.Character.Id.Instance); // receiver 
                                infoPacket.PushInt(0x4D38242E); // packet ID
                                infoPacket.PushIdentity(npc.Id); // affected identity
                                infoPacket.PushByte(0); // ?

                                // End packet header
                                infoPacket.PushByte(0x50); // npc's just have 0x50
                                infoPacket.PushByte(1); // esi_001?
                                infoPacket.PushByte((byte)npc.Stats.Profession.Value); // Profession
                                infoPacket.PushByte((byte)npc.Stats.Level.Value); // Level
                                infoPacket.PushByte((byte)npc.Stats.TitleLevel.Value); // Titlelevel
                                infoPacket.PushByte((byte)npc.Stats.VisualProfession.Value); // Visual Profession

                                infoPacket.PushShort(0); // no idea for npc's
                                infoPacket.PushUInt(npc.Stats.Health.Value); // Current Health (Health)
                                infoPacket.PushUInt(npc.Stats.Life.Value); // Max Health (Life)
                                infoPacket.PushInt(0); // BreedHostility?
                                infoPacket.PushUInt(0); // org ID
                                infoPacket.PushShort(0);
                                infoPacket.PushShort(0);
                                infoPacket.PushShort(0);
                                infoPacket.PushShort(0);
                                infoPacket.PushInt(0x499602d2);
                                infoPacket.PushInt(0x499602d2);
                                infoPacket.PushInt(0x499602d2);
                                var infoPacketA = infoPacket.Finish();
                                client.SendCompressed(infoPacketA);
                            }*/
                        }
                    }

                    break;

                case 120:
                    {
                        // If action == Logout
                        // Start 30 second logout timer if client is not a GM (statid 215)
                        if (client.Character.Stats["GMLevel"].Value == 0)
                        {
                            // client.startLogoutTimer();
                        }
                        else
                        {
                            // If client is a GM, disconnect without timer
                            client.Server.DisconnectClient(client);
                        }
                    }

                    break;
                case 121:
                    {
                        // If action == Stop Logout
                        // Stop current logout timer and send stop logout packet
                        client.Character.UpdateMoveType((byte)client.Character.PreviousMoveMode);
                        //client.CancelLogOut();
                    }

                    break;

                case 87:
                    {
                        // If action == Stand
                        client.Character.UpdateMoveType(37);

                        // Send stand up packet, and cancel timer/send stop logout packet if timer is enabled
                        //client.StandCancelLogout();
                    }

                    break;

                case 22:
                    {
                        // Kick Team Member
                    }

                    break;
                case 24:
                    {
                        // Leave Team
                        /*
                        var team = new TeamClass();
                        team.LeaveTeam(client);
                         */
                    }

                    break;
                case 25:
                    {
                        // Transfer Team Leadership
                    }

                    break;
                case 26:
                    {
                        // Team Join Request
                        // Send Team Invite Request To Target Player
                        /*
                        var team = new TeamClass();
                        team.SendTeamRequest(client, packet.Target);
                         */
                    }

                    break;
                case 28:
                    {
                        /*
                        // Request Reply
                        // Check if positive or negative response

                        // if positive
                        var team = new TeamClass();
                        var teamID = TeamClass.GenerateNewTeamId(client, packet.Target);

                        // Destination Client 0 = Sender, 1 = Reciever

                        // Reciever Packets
                        ///////////////////

                        // CharAction 15
                        team.TeamRequestReply(client, packet.Target);

                        // CharAction 23
                        team.TeamRequestReplyCharacterAction23(client, packet.Target);

                        // TeamMember Packet
                        team.TeamReplyPacketTeamMember(1, client, packet.Target, "Member1");

                        // TeamMemberInfo Packet
                        team.TeamReplyPacketTeamMemberInfo(1, client, packet.Target);

                        // TeamMember Packet
                        team.TeamReplyPacketTeamMember(1, client, packet.Target, "Member2");

                        // Sender Packets
                        /////////////////

                        // TeamMember Packet
                        team.TeamReplyPacketTeamMember(0, client, packet.Target, "Member1");

                        // TeamMemberInfo Packet
                        team.TeamReplyPacketTeamMemberInfo(0, client, packet.Target);

                        // TeamMember Packet
                        team.TeamReplyPacketTeamMember(0, client, packet.Target, "Member2");
                         */
                    }

                    break;

                case 0x70: // Remove/Delete item
                    ItemDao.RemoveItem((int)packet.Target.Type, client.Character.Identity.Instance, packet.Target.Instance);
                    client.Character.BaseInventory.RemoveItem((int)packet.Target.Type, packet.Target.Instance);
                    client.SendCompressed(packet);
                    break;



                case 0x34: // Split?
                    IItem it = client.Character.BaseInventory.Pages[(int)packet.Target.Type][packet.Target.Instance];
                    it.MultipleCount -= args2;
                    Item newItem = new Item(it.Quality, it.LowID, it.HighID);
                    newItem.MultipleCount = args2;

                    client.Character.BaseInventory.Pages[(int)packet.Target.Type].Add(
                        client.Character.BaseInventory.Pages[(int)packet.Target.Type].FindFreeSlot(), newItem);
                    client.Character.BaseInventory.Pages[(int)packet.Target.Type].Write();
                    break;



                #region Join item

                case 0x35:
                    client.Character.BaseInventory.Pages[(int)packet.Target.Type][packet.Target.Instance].MultipleCount
                        += client.Character.BaseInventory.Pages[(int)packet.Target.Type][args2].MultipleCount;
                    client.Character.BaseInventory.Pages[(int)packet.Target.Type].Remove(args2);
                    client.Character.BaseInventory.Pages[(int)packet.Target.Type].Write();
                    client.SendCompressed(packet);
                    break;

                #endregion

                #region Sneak Action

                // ###################################################################################
                // Spandexpants: This is all i have done so far as to make sneak turn on and off, 
                // currently i cannot find a missing packet or link which tells the server the player
                // has stopped sneaking, hidden packet or something, will come back to later.
                // ###################################################################################

                // Sneak Packet Received
                case 163:
                    {
                        // TODO: IF SNEAKING IS ALLOWED RUN THIS CODE.
                        // Send Action 162 : Enable Sneak
                        var sneak = new CharacterActionMessage
                        {
                            Identity = client.Character.Identity,
                            Unknown = 0x00,
                            Action = CharacterActionType.StartedSneaking,
                            Unknown1 = 0x00000000,
                            Target = Identity.None,
                            Parameter1 = 0,
                            Parameter2 = 0,
                            Unknown2 = 0x0000
                        };

                        client.SendCompressed(sneak);

                        // End of Enable sneak
                        // TODO: IF SNEAKING IS NOT ALLOWED SEND REJECTION PACKET
                    }

                    break;

                #endregion

                #region Use Item on Item
                /*
                case 81:
                    {
                        var item1 = packet.Target;
                        var item2 = new Identity { Type = (IdentityType)args1, Instance = args2 };

                        var cts = new Tradeskill(client, item1.Instance, item2.Instance);
                        cts.ClickBuild();
                        break;
                    }
                    */
                #endregion

                #region Change Visual Flag

                case 166:
                    {
                        client.Character.Stats["VisualFlags"].Value = args2;

                        // client.SendChatText("Setting Visual Flag to "+unknown3.ToString());
                        AppearanceUpdate.AnnounceAppearanceUpdate(client.Character);
                        break;
                    }

                #endregion
                /*
                #region Tradeskill Source Changed
                    
                case 0xdc:
                    TradeSkillReceiver.TradeSkillSourceChanged(client, args1, args2);
                    break;

                #endregion

                #region Tradeskill Target Changed

                case 0xdd:
                    TradeSkillReceiver.TradeSkillTargetChanged(client, args1, args2);
                    break;

                #endregion

                #region Tradeskill Build Pressed

                case 0xde:
                    TradeSkillReceiver.TradeSkillBuildPressed(client, packet.Target.Instance);
                    break;

                #endregion
                    */
                #region default

                default:
                    {
                        client.Playfield.Announce(packet);
                    }

                    break;

                #endregion

            }
        }

        #endregion
    }
}