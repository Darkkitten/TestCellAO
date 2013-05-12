﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoneEngine.GameObject
{
    public interface INamedEntity : IInstancedEntity, IStats
    {
        string Name
        {
            get;
            set;
        }
    }
}