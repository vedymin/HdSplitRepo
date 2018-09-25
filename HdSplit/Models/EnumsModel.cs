using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdSplit.Models {
    public enum Lines {
        LINE_10,
        LINE_15,
        LINE_30,
        LINE_20,
        LINE_71,
        LINE_80,
        LINE_12,
        LINE_11,
        LINE_40,
        LINE_41,
        LINE_98

    }

    public enum States
    {
        firstScanOfHd,
        itemScan,
        newHdScan,
        checkHdAfterItemScan
    }
}
