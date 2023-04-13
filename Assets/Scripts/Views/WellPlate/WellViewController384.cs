using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

public class WellViewController384 : WellViewController
{
    public override WellViewController GetNextInRow()
    {
        int wellNum;
        string nextWellId;

        if (wellId.Length == 2)
        {
            wellNum = Int32.Parse(wellId[1].ToString());
        }
        else
        {
            char[] chars = { wellId[1], wellId[2] };
            wellNum = Int32.Parse(new string(chars));
        }

        if ((wellNum + 1) < maxRowNum)
        {
            wellNum += 2;
            nextWellId = new string(wellId[0] + wellNum.ToString());
            return transform.parent.Find(nextWellId).GetComponent<WellViewController>();
        }
        return null;
    }

    public override WellViewController GetNextInCol()
    {
        string nextWellId;

        char nextRowId = (char)(((int)wellId[0]) + 2);
        string columnNum = wellId.Substring(1);

        if (((int)wellId[0] + 1) - 64 < maxColNum)
        {
            nextWellId = new string(nextRowId.ToString() + columnNum);
            return transform.parent.Find(nextWellId).GetComponent<WellViewController>();
        }
        return null;
    }
}
