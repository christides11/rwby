﻿using System;

namespace ModIO
{
    /// <summary>
    /// Struct used to represent a mod that already exists on the current device. You can view the
    /// subscribed users to this mod as well as the directory and modprofile associated to it.
    /// </summary>
    public struct UserInstalledMod
    {
        /// <summary>
        /// Whether or not the mod has been marked for an update
        /// </summary>
        public bool updatePending;
        
        /// <summary>
        /// the directory of where this mod is installed
        /// </summary>
        public string directory;
        
        /// <summary>
        /// The metadata for the version of the mod that is currently installed (Not to be mistaken
        /// with the metadata located inside of ModProfile.cs)
        /// </summary>
        public string metadata;
        
        /// <summary>
        /// the version of this installed mod
        /// </summary>
        public string version;
        
        /// <summary>
        /// the change log for this version of the installed mod
        /// </summary>
        public string changeLog;

        /// <summary>
        /// The date that this version of the mod was submitted to mod.io
        /// </summary>
        public DateTime dateAdded;
        
        /// <summary>
        /// The profile of this mod, including the summary and name
        /// </summary>
        public ModProfile modProfile;
    }
}
