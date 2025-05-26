using CgmscHO_API.DTO;
using CgmscHO_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace CgmscHO_API.Utility
{
  
    public class master
    {
        private readonly OraDbContext _context;
        public master(OraDbContext context)
        {
            _context = context;
        }


        public string getOPReceiptItemidID(string facilityId, string itemId)
        {
            string retvalue = "0";
            string strQuery = @"  select facreceiptitemid from tbfacilityreceiptitems  where facreceiptid=" + facilityId + " and itemid=" + itemId;
            //DataTable dt = DBHelper.GetDataTable(strQuery);
            var myList = _context.GetfacreceiptitemidDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(strQuery)).ToList();

            if (myList.Count > 0)
            {
                retvalue = myList[0].FACRECEIPTID.ToString();
            }
            return retvalue;

        }


        public bool getSameBatchInItems(string batchno, string facreceiptitemid,string itemId)
        {
            bool value = true;
            string strQuery = @" select INWNO from tbFacilityReceiptBatches where itemid=" + itemId + " and batchno='" + batchno + "' and FACreceiptitemid=" + facreceiptitemid;
            var myList = _context.GetInwardNoDbSet
       .FromSqlInterpolated(FormattableStringFactory.Create(strQuery)).ToList();
            if (myList.Count > 0)
            {
                value = false;
            }
            return value;

        }

        public bool isFacilityReceipt(string facReceiptId)
        {
            bool value = false;


            string strQuery = @" select  r.FACRECEIPTID from tbfacilityreceipts r
                    inner join tbfacilityreceiptitems ri on ri.facreceiptid = r.facreceiptid
                    inner join tbfacilityreceiptbatches rb on rb.facreceiptitemid = ri.facreceiptitemid
                    where r.facreceiptid = " + facReceiptId + " ";

            var myList = _context.GetFacilityReceiptIdDbSet
       .FromSqlInterpolated(FormattableStringFactory.Create(strQuery)).ToList();

            if (myList.Count > 0)
            {
                value = true;
            }

            return value;
        }


       


        public string checkstock(string recid)
        {
            string drugcode = "";

            string checkinitems = "select FACRECEIPTITEMID from tbfacilityreceiptitems where FACreceiptid=" + recid;

   
            var myListdtnilrow = _context.GetFacilityReceiptItemIdDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(checkinitems)).ToList();

            string qry = " select  nvl(i.absrqty,0) opening_qty,sum(nvl(bt.absrqty,0)) as batchqty,i.itemid,mi.itemcode from tbfacilityreceipts r  " +
     " inner join tbfacilityreceiptitems i on r.FACreceiptid=i.FACreceiptid  " +
     " left outer join tbFacilityReceiptBatches bt on bt.FACreceiptitemid=i.FACreceiptitemid  " +
     " inner join vmasitems mi on mi.itemid=i.itemid  where  FACreceipttype='FC' and r.FACreceiptid=" + recid + " " +
      " group by i.absrqty,i.itemid,mi.itemcode having sum(nvl(bt.absrqty,0)) <>i.absrqty ";

            var myList = _context.GetItemCodeDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

          

            if (myListdtnilrow.Count > 0)
            {
                if (myList.Count > 0)
                {
                    drugcode = myList[0].ITEMCODE.ToString();
                }

                else
                {
                    drugcode = "True";
                }
            }
            else
            {
                drugcode = "True";
            }
            return drugcode;
        }





        public string FacAutoGenerateNumbers(string FacilityID, bool IsReceipt, string mType)
        {
            string mGenNo = "";
            string mFacID = FacilityID;
            string strSQL = "Select FacilityCode from masFacilities Where FacilityID = " + mFacID;



            var myList = _context.GetFacilityCodeDbSet
          .FromSqlInterpolated(FormattableStringFactory.Create(strSQL)).ToList();


            if (myList.Count > 0)
            {
                string mToday = DateTime.Now.ToString();
                //GenFunctions.CheckDate(ref mToday, "Today", false);
                string strSQL1 = "Select SHAccYear from masAccYearSettings Where " + mToday + " between StartDate and EndDate";


                var myList1 = _context.GetHSccDbSet
          .FromSqlInterpolated(FormattableStringFactory.Create(strSQL1)).ToList();

                if (myList1.Count > 0)
                {
                    string mSHAccYear = myList1[0].SHACCYEAR.ToString();
                    string mWHPrefix = myList[0].FACILITYCODE.ToString();
                    if (IsReceipt)
                        strSQL = "Select Lpad(NVL(Max(To_Number(SubStr(FacReceiptNo, -11, 5))), 0) + 1, 5, '0') as WHSLNO from tbFacilityReceipts Where FacReceiptNo Like '" + mWHPrefix + "/" + mType + "/%/" + mSHAccYear + "'";
                    else
                        strSQL = "Select Lpad(NVL(Max(To_Number(SubStr(IssueNo, -11, 5))), 0) + 1, 5, '0') as WHSLNO from tbFacilityIssues Where IssueNo Like '" + mWHPrefix + "/" + mType + "/%/" + mSHAccYear + "'";


                    var myList2 = _context.GetWHsSerialNoDbSet
       .FromSqlInterpolated(FormattableStringFactory.Create(strSQL)).ToList();


                    if (myList2.Count > 0)
                        mGenNo = mWHPrefix + "/" + mType + "/" + myList2[0].WHSLNO.ToString() + "/" + mSHAccYear;
                    else
                        mGenNo = mWHPrefix + "/" + mType + "/" + "00001" + "/" + mSHAccYear;
                }
            }
            return mGenNo;
        }


        public bool checkAlreadyFC(Int64 usrFacilityID)
        {
            bool data = false;
            string checkFC = " select FACRECEIPTID from tbfacilityreceipts  s  where  s.facreceipttype='FC' and s.facilityid=" + usrFacilityID + " ";

            var myList = _context.GetFacReceiptIdDbSet
       .FromSqlInterpolated(FormattableStringFactory.Create(checkFC)).ToList();

           
            if (myList.Count > 0)
            {
                data = false;
            }
            else
            {
                data = true;
            }


            return data;
        }





        //public string getFacCodeForIndent(Int64 facId)
        //{
        //    string yearid = getACCYRSETID();
        //    string qry = " Select a.FACILITYID, Lpad(NVL(Max(To_Number(a.AUTO_NCCODE)), 0) + 1, 5, '0') as FACILITYCODE" +
        //       " From MASCGMSCNOC a" +
        //       " where a.FACILITYID=" + facId + " and a.accyrsetid=" + yearid + " group by a.FACILITYID";

        //    var myList = _context.GenWhIndentNoDbSet
        //    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //    if (myList.Count > 0)
        //    {
        //        string facCode = myList[0].FACILITYCODE; // Assuming IssueItemID is an integer
        //        return facCode.ToString();
        //    }
        //    else
        //    {
        //        return "00001"; // Or any other appropriate indication
        //    }
        //}

    }
}
