﻿namespace stubby {

    /// <summary>
    /// The options container class used by Stubby constructors.
    /// </summary>
    public class Arguments : IArguments {
        ///<summary>
        /// Port for admin portal. Defaults to 8889.
        ///</summary>
        public uint Admin { get; set; }

        ///<summary>
        /// Port for stubs portal. Defaults to 8882.
        ///</summary>
        public uint Stubs { get; set; }

        ///<summary>
        /// Port for stubs https portal. Defaults to 7443.
        ///</summary>
        public uint Tls { get; set; }

        ///<summary>
        /// Hostname at which to bind stubby. Defaults to localhost.
        ///</summary>
        public string Location { get; set; }

        ///<summary>
        /// Data file location to pre-load endpoints. YAML format.
        ///</summary>
        public string Data { get; set; }

        ///<summary>
        /// Monitor supplied data file for changes and reload endpoints if necessary. Defaults to false.
        ///</summary>
        public bool Watch { get; set; }

        ///<summary>
        /// The site requests to proxy and record
        ///</summary>
        public string Record { get; set; }

        ///<summary>
        /// The physical location to download a site to
        ///</summary>
        public string LocationToDownloadSite { get; set; }

        ///<summary>
        /// Prevent stubby from logging to the console. Muted by default.
        ///</summary>
        public bool Mute { get; set; }

        public Arguments() {
            Admin = 8889;
            Stubs = 8882;
            Tls = 7443;
            Location = "localhost";
            Data = null;
            Mute = true;
            Watch = false;
        }
    }
}
