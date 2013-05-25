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

namespace ZoneEngine.Functions
{
    #region Usings ...

    using ZoneEngine.GameObject;

    #endregion

    /// <summary>
    /// </summary>
    internal class Function_set : FunctionPrototype
    {
        /// <summary>
        /// </summary>
        public new int FunctionNumber = 53026;

        /// <summary>
        /// </summary>
        public new string FunctionName = "set";

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public override int ReturnNumber()
        {
            return this.FunctionNumber;
        }

        /// <summary>
        /// </summary>
        /// <param name="self">
        /// </param>
        /// <param name="caller">
        /// </param>
        /// <param name="target">
        /// </param>
        /// <param name="arguments">
        /// </param>
        /// <returns>
        /// </returns>
        public override bool Execute(
            INamedEntity self, INamedEntity caller, IInstancedEntity target, object[] arguments)
        {
            lock (target)
            {
                return this.FunctionExecute(self, caller, target, arguments);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public override string ReturnName()
        {
            return this.FunctionName;
        }

        /// <summary>
        /// </summary>
        /// <param name="Self">
        /// </param>
        /// <param name="Caller">
        /// </param>
        /// <param name="Target">
        /// </param>
        /// <param name="Arguments">
        /// </param>
        /// <returns>
        /// </returns>
        public bool FunctionExecute(INamedEntity Self, INamedEntity Caller, IInstancedEntity Target, object[] Arguments)
        {
            int statNumber = (int)Arguments[0];
            int statValue = (int)Arguments[1];
            IStats temp = Target;
            if (temp != null)
            {
                temp.Stats.GetStatbyNumber(statNumber).Set(statValue);
                return true;
            }

            return false;
        }
    }
}