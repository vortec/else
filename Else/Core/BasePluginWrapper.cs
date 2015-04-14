﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Else.Extensibility;

namespace Else.Core
{
    public abstract class BasePluginWrapper
    {
        public List<Plugin> Loaded = new List<Plugin>();
        public abstract void Load(string path);

        /// <summary>
        /// Failed to load any plugins from the assembly.
        /// </summary>
        public class PluginLoadException : Exception
        {
            public PluginLoadException()
            {
            }

            public PluginLoadException(string message) : base(message)
            {
            }

            public PluginLoadException(string message, Exception inner) : base(message, inner)
            {
            }

            protected PluginLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}