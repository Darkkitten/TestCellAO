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

    using System.Diagnostics.Contracts;

    using SmokeLounge.AOtomation.Messaging.GameData;

    using ZoneEngine.GameObject.Stats;

    #endregion

    /// <summary>
    /// </summary>
    [ContractClass(typeof(INamedEntityContract))]
    public interface INamedEntity : IInstancedEntity, IStats
    {
        #region Public Properties

        /// <summary>
        /// </summary>
        string Name { get; set; }

        #endregion
    }

    /// <summary>
    /// </summary>
    [ContractClassFor(typeof(INamedEntity))]
    internal abstract class INamedEntityContract : INamedEntity
    {
        /// <summary>
        /// </summary>
        public Identity Identity { get; private set; }

        /// <summary>
        /// </summary>
        public Identity Playfield { get; set; }

        public Vector3 Coordinates
        {
            get
            {
                return default(Vector3);
            }
            set { }
        }

        public Quaternion Heading
        {
            get
            {
                return default(Quaternion);
            }
            set { }
        }

        /// <summary>
        /// </summary>
        public DynelStats Stats { get; private set; }

        /// <summary>
        /// </summary>
        public string Name
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                Contract.Ensures(Contract.Result<string>() != "");
                return default(string);
            }

            set
            {
                Contract.Requires(value != null);
                Contract.Requires(value != "");
            }
        }
    }
}