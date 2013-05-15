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

namespace ZoneEngine.CoreClient
{
    #region Usings ...

    using System;
    using System.Globalization;
    using System.Net.Sockets;

    using AO.Core.Components;
    using AO.Core.Events;

    using Cell.Core;

    using ComponentAce.Compression.Libs.zlib;

    using NiceHexOutput;

    using SmokeLounge.AOtomation.Messaging.Messages;
    using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

    using ZoneEngine.CoreServer;
    using ZoneEngine.GameObject;

    #endregion

    /// <summary>
    /// </summary>
    public class Client : ClientBase, IZoneClient
    {
        #region Fields

        /// <summary>
        /// </summary>
        private readonly IBus bus;

        /// <summary>
        /// </summary>
        private readonly Character character = new Character();

        /// <summary>
        /// </summary>
        private readonly IMessageSerializer messageSerializer;

        /// <summary>
        /// </summary>
        private string accountName = string.Empty;

        /// <summary>
        /// </summary>
        private bool zStreamSetup;

        /// <summary>
        /// </summary>
        private NetworkStream netStream;

        /// <summary>
        /// </summary>
        private ZOutputStream zStream;

        /// <summary>
        /// </summary>
        private string clientVersion = string.Empty;

        /// <summary>
        /// </summary>
        private ushort packetNumber = 1;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        /// <param name="server">
        /// </param>
        /// <param name="messageSerializer">
        /// </param>
        /// <param name="bus">
        /// </param>
        public Client(ServerBase server, IMessageSerializer messageSerializer, IBus bus)
            : base(server)
        {
            this.messageSerializer = messageSerializer;
            this.bus = bus;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public string AccountName
        {
            get
            {
                return this.accountName;
            }

            set
            {
                this.accountName = value;
            }
        }

        /// <summary>
        /// </summary>
        public Character Character
        {
            get
            {
                return this.character;
            }
        }

        /// <summary>
        /// </summary>
        public string ClientVersion
        {
            get
            {
                return this.clientVersion;
            }

            set
            {
                this.clientVersion = value;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        private ZoneServer server
        {
            get
            {
                return (ZoneServer)this._server;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <param name="receiver">
        /// </param>
        /// <param name="messageBody">
        /// </param>
        public void Send(int receiver, MessageBody messageBody)
        {
            // TODO: Investigate if reciever is a timestamp
            var message = new Message
                              {
                                  Body = messageBody, 
                                  Header =
                                      new Header
                                          {
                                              MessageId = this.packetNumber, 
                                              PacketType = messageBody.PacketType, 
                                              Unknown = 0x0001, 
                                              Sender = 0x00000001, 
                                              Receiver = receiver
                                          }
                              };
            this.packetNumber++;
            byte[] buffer = this.messageSerializer.Serialize(message);

            /* Uncomment for Debug outgoing Messages
            */
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(NiceHexOutput.Output(buffer));
            Console.ResetColor();

            if (buffer.Length % 4 > 0)
            {
                Array.Resize(ref buffer, buffer.Length + (4 - (buffer.Length % 4)));
            }

            this.Send(buffer);
        }

        /// <summary>
        /// </summary>
        /// <param name="packet">
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void SendCompressed(byte[] packet)
        {
            int tries = 0;
            bool done = false;

            // 18.1 Fix
            byte[] pn = BitConverter.GetBytes(this.packetNumber++);
            packet[0] = pn[1];
            packet[1] = pn[0];
            while ((!done) && (tries < 3))
            {
                try
                {
                    done = true;
                    if (!this.zStreamSetup)
                    {
                        // Create the zStream
                        this.netStream = new NetworkStream(this.TcpSocket);
                        this.zStream = new ZOutputStream(this.netStream, zlibConst.Z_BEST_COMPRESSION);
                        this.zStream.FlushMode = zlibConst.Z_SYNC_FLUSH;
                        this.zStreamSetup = true;
                    }

                    this.zStream.Write(packet, 0, packet.Length);
                    this.zStream.Flush();
                }
                catch (Exception)
                {
                    tries++;
                    done = false;
                    this.Server.DisconnectClient(this);
                    return;
                }
            }
        }

        // <summary>
        // </summary>
        /// <summary>
        /// </summary>
        /// <param name="text">
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void SendChatText(string text)
        {
            ChatTextMessage chatTextMessage = new ChatTextMessage();
            chatTextMessage.Text = text;
            this.Send(this.character.Identity.Instance, chatTextMessage);

            throw new NotImplementedException("SendChatText not implemented yet");
        }

        // <summary>
        // </summary>
        /// <summary>
        /// </summary>
        /// <param name="message">
        /// </param>
        /// <param name="announceToPlayfield">
        /// </param>
        public void SendToPlayfield(Message message, bool announceToPlayfield)
        {
            if (message == null)
            {
                return;
            }

            foreach (Client client in this.server.Clients)
            {
                if ((client.Character.Playfield != this.Character.Playfield)
                    || (client.Character.Identity.Instance == this.Character.Identity.Instance))
                {
                    continue;
                }

                message.Header.Receiver = client.character.Identity.Instance;
                byte[] packet = this.messageSerializer.Serialize(message);
                client.SendCompressed(packet);
            }
        }

        #endregion

        /// <summary>
        /// </summary>
        /// <param name="segment">
        /// </param>
        /// <returns>
        /// </returns>
        protected uint GetMessageNumber(BufferSegment segment)
        {
            var messageNumberArray = new byte[4];
            messageNumberArray[3] = segment.SegmentData[16];
            messageNumberArray[2] = segment.SegmentData[17];
            messageNumberArray[1] = segment.SegmentData[18];
            messageNumberArray[0] = segment.SegmentData[19];
            uint reply = BitConverter.ToUInt32(messageNumberArray, 0);
            return reply;
        }

        /// <summary>
        /// </summary>
        /// <param name="segment">
        /// </param>
        /// <returns>
        /// </returns>
        protected uint GetMessageNumber(byte[] segment)
        {
            var messageNumberArray = new byte[4];
            messageNumberArray[3] = segment[16];
            messageNumberArray[2] = segment[17];
            messageNumberArray[1] = segment[18];
            messageNumberArray[0] = segment[19];
            uint reply = BitConverter.ToUInt32(messageNumberArray, 0);
            return reply;
        }

        /// <summary>
        /// </summary>
        /// <param name="buffer">
        /// </param>
        /// <returns>
        /// </returns>
        protected override bool OnReceive(BufferSegment buffer)
        {
            Message message = null;

            var packet = new byte[this._remainingLength];
            Array.Copy(buffer.SegmentData, packet, this._remainingLength);

            /* Uncomment for Incoming Messages
                */
            Console.WriteLine("Offset: " + buffer.Offset.ToString() + " -- RemainingLength: " + this._remainingLength);
            Console.WriteLine(NiceHexOutput.Output(packet));

            this._remainingLength = 0;
            try
            {
                message = this.messageSerializer.Deserialize(packet);
            }
            catch (Exception)
            {
                uint messageNumber = this.GetMessageNumber(packet);
                this.Server.Warning(
                    this, "Client sent malformed message {0}", messageNumber.ToString(CultureInfo.InvariantCulture));
                return false;
            }

            buffer.IncrementUsage();

            if (message == null)
            {
                uint messageNumber = this.GetMessageNumber(packet);
                this.Server.Warning(
                    this, "Client sent unknown message {0}", messageNumber.ToString(CultureInfo.InvariantCulture));
                return false;
            }

            this.bus.Publish(new MessageReceivedEvent(this, message));

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="messageBody">
        /// </param>
        /// <param name="receiver">
        /// </param>
        /// <param name="announceToPlayfield">
        /// </param>
        public void SendCompressed(MessageBody messageBody, int receiver, bool announceToPlayfield)
        {
            var message = new Message
                              {
                                  Body = messageBody, 
                                  Header =
                                      new Header
                                          {
                                              MessageId = this.packetNumber, 
                                              PacketType = messageBody.PacketType, 
                                              Unknown = 0x0001, 
                                              Sender = 0x00000001, 
                                              Receiver = receiver
                                          }
                              };
            this.packetNumber++;
            byte[] buffer = this.messageSerializer.Serialize(message);

            this.SendCompressed(buffer);
        }
    }
}