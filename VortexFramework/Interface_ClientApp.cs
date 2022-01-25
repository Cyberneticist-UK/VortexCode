using System;
using System.Collections.Generic;


namespace Vortex
{
    public class Interface_ClientApp
    {
        /// <summary>
        /// The Command name - must be a single word, currently not case sensitive.
        /// </summary>
        string Command { get; }
        /// <summary>
        /// Description about the command - used by programs that can list the command data
        /// </summary>
        string Description { get; }

        /// <summary>
        /// When the command was Created
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// When the command was last updated
        /// </summary>
        DateTime LastUpdated { get; }
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

    }
}
