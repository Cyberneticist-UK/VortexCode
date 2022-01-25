using System;
using System.Collections.Generic;


namespace Vortex
{
    /// <summary>
    /// This is the interface for a NetScript command.
    /// </summary>
    public interface Interface_NetScriptCommand
    {
        /// <summary>
        /// The Command name - must be a single word, currently not case sensitive.
        /// </summary>
        string Command { get; }

        /// <summary>
        /// The Command Unique Identifier, to allow commands to be built in different languages and converted between them
        /// </summary>
        Guid CommandID { get; }

        /// <summary>
        /// Description about the command - used by programs that can list the command data
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Example using the command
        /// </summary>
        string Example { get; }
        
        /// <summary>
        /// When the command was Created
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// When the command was last updated
        /// </summary>
        DateTime LastUpdated { get; }

        /// <summary>
        /// Which version are we currently on
        /// </summary>
        decimal Version { get; }

        /// <summary>
        /// Examples on how the command can be used
        /// </summary>
        List<string> Usage { get; }

        /// <summary>
        /// Who created this
        /// </summary>
        string CreatedBy { get; }

        /// <summary>
        /// An update history for the command
        /// </summary>
        List<string> UpdateHistory { get; }

        Token Process(Token command, Vortex_Memory CurrentMemory, Vortex.Node_AccessLayer NodeCache, NetScript_Command CommandProcessor);
    }
}
