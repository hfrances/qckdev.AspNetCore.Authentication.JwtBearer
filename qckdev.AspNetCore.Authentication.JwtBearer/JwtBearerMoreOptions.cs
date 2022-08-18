using System;
using System.Collections.Generic;
using System.Text;

namespace qckdev.AspNetCore.Authentication.JwtBearer
{

    /// <summary>
    /// Additional options class provides information needed to control Bearer Authentication handler behavior.
    /// </summary>
    public class JwtBearerMoreOptions
    {

        /// <summary>
        /// Gets or sets the amount of time a generated token remains valid. Defaults to 1 day.
        /// </summary>
        /// <value>
        /// The amount of time a generated token remains valid.
        /// </value>
        public TimeSpan? TokenLifeTimespan { get; set; }
        
    }
}
