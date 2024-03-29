﻿using System;
using System.Net;

namespace TracertAs.Whois
{
    internal class ApnicRirClient : RipeRirClient
    {
        private static readonly string[] generalRecordsFeatures =
        {
            "is not allocated to APNIC",
            "address range is not administered by APNIC",
            "Not allocated by APNIC"
        };

        public override string RirName
        {
            get { return "apnic"; }
        }

        protected override string[] GeneralRecordsFeatures
        {
            get { return generalRecordsFeatures; }
        }
    }
}