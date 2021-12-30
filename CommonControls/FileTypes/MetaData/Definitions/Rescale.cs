﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CommonControls.FileTypes.MetaData.Definitions
{
    [MetaData("RESCALE", 10)]
    public class ReScale : MetaEntryBase
    {
        [MetaDataTag(5, "")]
        public float Scale { get; set; }
    }
}
