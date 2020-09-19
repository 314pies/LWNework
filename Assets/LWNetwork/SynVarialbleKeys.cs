using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace LWNet
{
    /// <summary>
    /// SynVariablesKeys
    /// (All in string format)
    /// </summary>
    public static class SVK
    {
        #region PUN Only
        /// <summary>
        /// (PUN Only)Ping, ping to photon server
        /// </summary>
        public const string PNG = "PNG";
        /// <summary>
        /// (PUN Only)Last master client Updated Time
        /// </summary>
        public const string LMUT = "LMUT";
        #endregion
        #region LWNetwork
        /// <summary>
        /// SceneObjectList.
        /// A list of existing scene objects' ID
        /// </summary>
        public const string SOL = "SOL";
        /// <summary>
        /// PlayerObjectList.
        /// A list of existing player objects' ID
        /// </summary>
        public const string POL = "POL";
        /// <summary>
        /// Network Game Object
        /// </summary>
        public const string NGO = "NGO";
        #endregion

        #region Lobby(LW LAN)
        /// <summary>
        /// lobbyPlayerProfile
        /// Key for getting lobby player profile from synchronized variables
        /// </summary>
        public const string LPP = "LPP";
        /// <summary>
        /// Lobby Mode
        /// </summary>
        public const string LMd = "LMd";
        /// <summary>
        /// Lobby Map
        /// </summary>
        public const string LMp = "LMp";

        #endregion

        #region Player's profile
        /// <summary>
        /// Player's Level
        /// </summary>
        public const string PLV = "PLV";
        /// <summary>
        /// Name. 
        /// Player's name
        /// </summary>
        public const string NM = "NM";
        #endregion

        /// <summary>
        /// Virtual Player (BOT)
        /// </summary>
        public const string VP = "VP";
        /// <summary>
        /// Team belong
        /// </summary>
        public const string TB = "TB";        
        /// <summary>
        /// Health Point
        /// </summary>
        public const string HP = "HP";
        /// <summary>
        /// Players loudOut
        /// </summary>
        public const string PL = "PL";
        /// <summary>
        /// Player Kills
        /// </summary>
        public const string PK = "PK";
        /// <summary>
        ///Player Deaths
        /// </summary>
        public const string PD = "PD";

        /// <summary>
        /// Team A Last Re-spawn Point
        /// </summary>
        public const string TALRP = "TALRP";
        /// <summary>
        /// Team B Last Re-spawn Point
        /// </summary>
        public const string TBLRP = "TBLRP";

        #region Game Mode
        /// <summary>
        /// Game stage
        /// </summary>
        public const string GSTG = "GSTG";
        /// <summary>
        /// Warm up time
        /// </summary>
        public const string WRMT = "WRMT";
        #endregion

        #region Game Mode- Supply recovery
        /// <summary>
        /// Prepare load out
        /// </summary>
        public const string PLO = "PLO";
        /// <summary>
        /// Supply drop unlock time
        /// </summary>
        public const string SUT = "SUT";
        /// <summary>
        /// Team A captured supply drop
        /// </summary>
        public const string ACSD = "ACSD";
        /// <summary>
        /// Team B captured supply drop
        /// </summary>
        public const string BCSD = "BCSD";
        /// <summary>
        /// Player's captured supply drop
        /// </summary>
        public const string PCSD = "PCSD";
        /// <summary>
        /// Wait time Left(until next supply dropped)
        /// </summary>
        public const string WTL = "WTL";
        /// <summary>
        /// StandardModeGameState
        /// The gaming status for standard mode script
        /// </summary>
        [Obsolete]
        public const string SMGS = "SMGS";
        /// <summary>
        /// LootBoxDeploySecond
        /// </summary>
        [Obsolete]
        public const string LDS = "LDS";
        /// <summary>
        /// LootBoxUnlockedSecond
        /// </summary>
        [Obsolete]
        public const string LUS = "LUS";
        /// <summary>
        /// Loot box locked state
        /// </summary>
        [Obsolete]
        public const string LLS = "LLS";

        /// <summary>
        /// Collected Loot box.The amount of Loot-box already been collected
        /// </summary>
        [Obsolete]
        public const string CLB = "CLB";
        /// <summary>
        /// Team A Collected loot-box
        /// </summary>
        [Obsolete]
        public const string CLB_A = "CLBA";
        /// <summary>
        /// Team B Collected loot-box
        /// </summary>
        [Obsolete]
        public const string CLB_B = "CLBB";
        /// <summary>
        /// How many time left for teams to picking load out(Pick Time Left)
        /// </summary>
        [Obsolete]
        public const string PTL = "PTL";
        
        /// <summary>
        /// Player collected loot boxs
        /// </summary>
        [Obsolete]
        public const string PCL = "PCL";

        #endregion



        #region BOT
        /// <summary>
        /// BOTs' Network ID
        /// </summary>
        [Obsolete]
        public const string BNI = "BNI";
        /// <summary>
        /// BOT List
        /// </summary>
        [Obsolete]
        public const string BL = "BL";
        /// <summary>
        /// Bot Target(Target to move to)
        /// </summary>
        [Obsolete]
        public const string BT = "BT";
        #endregion
    }
}


