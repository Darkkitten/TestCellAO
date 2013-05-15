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

namespace ZoneEngine.GameObject
{
    #region Usings ...

    using System;
    using System.Collections.Generic;

    using Cell.Core;

    using NiceHexOutput;

    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Messages;

    using ZoneEngine.GameObject.Enums;
    using ZoneEngine.GameObject.Items;

    #endregion

    /// <summary>
    /// </summary>
    public sealed class Character : Dynel, IPacketReceivingEntity, INamedEntity, ISummoner, IAOEvents, IAOActions, IItemContainer
    {
        #region Fields

        /// <summary>
        /// </summary>
        private IZoneClient client;

        /// <summary>
        /// </summary>
        private  MoveModes moveMode = MoveModes.None;

        /// <summary>
        /// </summary>
        private IList<Pet> pets = new List<Pet>();

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public IClient Client
        {
            get
            {
                return this.client;
            }
        }

        /// <summary>
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public MoveModes MoveMode
        {
            get
            {
                return this.moveMode;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public string Name
        {
            get
            {
                // TODO: Implement this property getter
                throw new NotImplementedException();
            }

            set
            {
                // TODO: Implement this property setter
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// </summary>
        public IList<Pet> Pets
        {
            get
            {
                return this.pets;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="message">
        /// </param>
        /// <param name="announceToPlayfield">
        /// </param>
        internal void Send(MessageBody messageBody, bool announceToPlayfield)
        {
            this.client.SendCompressed(messageBody, this.Identity.Instance, announceToPlayfield);
        }

        #endregion

        public BaseInventory BaseInventory { get; private set; }
    }
}