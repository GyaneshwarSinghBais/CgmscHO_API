using CgmscHO_API.Models;
using CgmscHO_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourierController : ControllerBase
    {
        private readonly OraDbContext _context;

        public CourierController(OraDbContext context)
        {
            _context = context;
        }


        string qry = "";
        string whselectedDocket = "";

        [HttpGet("getPickDocketDetails")]
        public async Task<ActionResult<IEnumerable<PickDockets>>> getPickDocketDetails(string userid, string usertype, string indentid)
        {
            if (indentid != "0")
            {
                whselectedDocket = " and tb.indentid=" + indentid;
            }

            if (usertype == "WHU")
            {
                qry = @"  select   w.warehouseid,w.warehousename,tb.QDOCKETNO,tb.indentid
                                 ,tb.indentno ,tb.indentdate ,count(distinct tbi.itemid) as nousitems
                                 ,tb.QDOCKETNO||'-('||to_char(count(distinct tbi.itemid))||')-'||tb.indentno as details
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 where tb.Status = 'C' and tb.issuetype in ('QA','QS')             
                                 and tb.indentdate between '04-May-2024' and SYSDATE and tb.CTID is null
                                 and tb.isCourierPickQC is null
                                 and tb.QDOCKETNO not in ('0000000000')
                                 and  w.warehouseid =" + userid + @" " + whselectedDocket + @"
                                 group by  w.warehousename,tb.QDOCKETNO,tb.indentid,tb.indentno ,tb.indentdate,w.warehouseid ";
            }
            else
            {
                //change 2931 QC-1 id in ddl 
                qry = @"  select   w.warehouseid,w.warehousename,tb.QDOCKETNO,tb.indentid
                                 ,tb.indentno ,tb.indentdate ,count(distinct tbi.itemid) as nousitems
                                 ,tb.QDOCKETNO||'-('||to_char(count(distinct tbi.itemid))||')-'||tb.indentno as details
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 where tb.Status = 'C' and tb.issuetype in ('QA','QS')              
                                 and tb.indentdate between '04-May-2024' and SYSDATE and tb.CTID is null
                                 and tb.isCourierPickQC is null
                                 and tb.QDOCKETNO not in ('0000000000')
                                 and  w.warehouseid =" + userid + @"
                                 group by  w.warehousename,tb.QDOCKETNO,tb.indentid,tb.indentno ,tb.indentdate,w.warehouseid ";
            }
            var myList = _context.PickDocketDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpPut("InsertCourierTransaction")]
        public IActionResult InsertCourierTransaction(Int64 SourceID, String SourceType, Int64 DestinationID, String DestinationType, Double WEIGHT, String Unit, string docketNo, Int64 indentid)
        {
            string dt1 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
            bool isCTIDexist = false;


            string qry = @" insert into CourierTransaction(SourceID ,SourceType,EntryDatePick, PickDate,DestinationID,DestinationType,WEIGHT,UNIT,indentid)
                        values(" + SourceID + " ,'" + SourceType + "',sysdate, '" + dt1 + "'," + DestinationID + ",'" + DestinationType + "'," + WEIGHT + ",'" + Unit + "'," + indentid + ")  ";
            _context.Database.ExecuteSqlRaw(qry);

            isCTIDexist = getCTID(SourceID, indentid, out Int64 ctid);

            string qryUpdate = @" update tbindents set ctid = " + ctid + " where QDOCKETNO = '" + docketNo + "'   ";
            _context.Database.ExecuteSqlRaw(qryUpdate);


            return Ok("Successfully Saved");

        }


        private bool getCTID(Int64 SourceID, Int64 indentid, out Int64 ctid)
        {
            ctid = 0;


            string qry = @" select  CTID from CourierTransaction where sourceid = " + SourceID + " and indentid = '" + indentid + "' order by ctid desc ";

            var result = _context.GetCourierTransactionDBSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList().FirstOrDefault();

            if (result == null)
            {
                ctid = 0;
                return false;

            }
            ctid = Convert.ToInt64(result.CTID);
            return true;

        }


        [HttpGet("pickedCourierToBeDrop")]
        public async Task<ActionResult<IEnumerable<pickedCourierToBeDropModel>>> pickedCourierToBeDrop(string userid, string usertype, string indentid)
        {
            string qry = "";
            string whselectedDocket = "";
            if (indentid != "0")
            {
                whselectedDocket = " and tb.ctid=" + indentid;
            }

            if (usertype == "WHU")
            {
                qry = @"  select distinct  w.warehouseid,w.warehousename,tb.QDOCKETNO                             
                                 ,tb.QDOCKETNO as details, tb.ctid
                                 ,ct.entrydatepick,TO_CHAR(ct.pickdate, 'DD-MON-YY HH:MI:SS PM') AS pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit
                                 ,hi.status,TO_CHAR(hi.entrydate, 'DD-MON-YY HH:MI:SS PM') AS entrydate
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
                                 left outer join COURIER_STATUS_HISTORY hi on hi.ctid = ct.ctid and hi.islatest = 'Y'
                                  where tb.Status = 'C' 
                                 and tb.CTID is not null                              
                                 and isdrop is null " + whselectedDocket + @"
                                 group by  w.warehousename,tb.QDOCKETNO,w.warehouseid, tb.ctid
                                 ,ct.entrydatepick,ct.pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit 
                                 ,hi.status,hi.entrydate  ";
            }
            else
            {
                //change 2931 QC-1 id in ddl 
                qry = @"   select distinct  w.warehouseid,w.warehousename,tb.QDOCKETNO                             
                                 ,tb.QDOCKETNO as details, tb.ctid
                                 ,ct.entrydatepick,TO_CHAR(ct.pickdate, 'DD-MON-YY HH:MI:SS PM') AS pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit
                                 ,hi.status,TO_CHAR(hi.entrydate, 'DD-MON-YY HH:MI:SS PM') AS entrydate
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
                                 left outer join COURIER_STATUS_HISTORY hi on hi.ctid = ct.ctid and hi.islatest = 'Y'
                                  where tb.Status = 'C' 
                                 and tb.CTID is not null                              
                                 and isdrop is null " + whselectedDocket + @"
                                 group by  w.warehousename,tb.QDOCKETNO,w.warehouseid, tb.ctid
                                 ,ct.entrydatepick,ct.pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit 
                                 ,hi.status,hi.entrydate  ";
            }
            var myList = _context.pickedCourierToBeDropDBSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("pickedCourierToBeDropStatus")]
        public async Task<ActionResult<IEnumerable<pickedCourierToBeDropModel>>> pickedCourierToBeDropStatus(string userid, string usertype, string indentid)
        {
            string qry = "";
            string whselectedDocket = "";
            if (indentid != "0")
            {
                whselectedDocket = " and tb.ctid=" + indentid;
            }

            if (usertype == "WHU")
            {
                qry = @"  select distinct  w.warehouseid,w.warehousename,tb.QDOCKETNO                             
                                 ,tb.QDOCKETNO as details, tb.ctid
                                 ,ct.entrydatepick,TO_CHAR(ct.pickdate, 'DD-MON-YY HH:MI:SS PM') AS pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit
                                 ,hi.status,TO_CHAR(hi.entrydate, 'DD-MON-YY HH:MI:SS PM') AS entrydate
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
                                 left outer join COURIER_STATUS_HISTORY hi on hi.ctid = ct.ctid and hi.islatest = 'Y'
                                  where tb.Status = 'C' 
                                 and tb.CTID is not null                              
                                 " + whselectedDocket + @"
                                 and w.warehouseid = " + userid + @"
                                 group by  w.warehousename,tb.QDOCKETNO,w.warehouseid, tb.ctid
                                 ,ct.entrydatepick,ct.pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit 
                                 ,hi.status,hi.entrydate  ";
            }
            else
            {
                //change 2931 QC-1 id in ddl 
                qry = @"   select distinct  w.warehouseid,w.warehousename,tb.QDOCKETNO                             
                                 ,tb.QDOCKETNO as details, tb.ctid
                                 ,ct.entrydatepick,TO_CHAR(ct.pickdate, 'DD-MON-YY HH:MI:SS PM') AS pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit
                                 ,hi.status,TO_CHAR(hi.entrydate, 'DD-MON-YY HH:MI:SS PM') AS entrydate
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
                                 left outer join COURIER_STATUS_HISTORY hi on hi.ctid = ct.ctid and hi.islatest = 'Y'
                                  where tb.Status = 'C' 
                                 and tb.CTID is not null                              
                                  " + whselectedDocket + @"
                                 group by  w.warehousename,tb.QDOCKETNO,w.warehouseid, tb.ctid
                                 ,ct.entrydatepick,ct.pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit 
                                 ,hi.status,hi.entrydate  ";
            }
            var myList = _context.pickedCourierToBeDropDBSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpPut("UpdateCourierTransaction")]
        public IActionResult UpdateCourierTransaction(String droplongitude, String droplatitude, Int64 ctid)
        {
            string dt1 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");

            string qry = @"  update CourierTransaction set  isdrop = 'Y', dropdate = '" + dt1 + "', droplongitude = '" + droplongitude + "',droplatitude = '" + droplatitude + "' where ctid = " + ctid + "  ";
            _context.Database.ExecuteSqlRaw(qry);

            return Ok("Successfully Updated");

        }

        [HttpPut("UpdateCOURIER_STATUS_HISTORY")]
        public IActionResult UpdateCOURIER_STATUS_HISTORY(Int64 ctid, String status)
        {
            string dt1 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");

            string qryUpdate = @"  update COURIER_STATUS_HISTORY set islatest = 'N' where ctid = " + ctid + " ";
            _context.Database.ExecuteSqlRaw(qryUpdate);

            string qryInsert = @"  insert into COURIER_STATUS_HISTORY(ctid,status,entrydate,islatest) values(" + ctid + ",'" + status + "','" + dt1 + "','Y') ";
            _context.Database.ExecuteSqlRaw(qryInsert);

            return Ok("Successfully Updated");

        }



        [HttpGet("CourierStatus")]
        public async Task<ActionResult<IEnumerable<CourierStatusDTO>>> CourierStatus(Int64 ctid)
        {
            string qry = "";


            qry = @"  select  ROW_NUMBER() OVER (ORDER BY statusdate) as id, statusdate, status from(
                            select   TO_CHAR(dropdate, 'DD-MON-YY HH:MI PM')  as statusDate , 'Drop' as status from CourierTransaction where ctid = " + ctid + @"  and dropdate is not null
                            UNION
                            select  TO_CHAR(entrydate, 'DD-MON-YY HH:MI PM') as statusDate ,  status from COURIER_STATUS_HISTORY where ctid = " + ctid + @"
                            UNION
                            select  TO_CHAR(pickdate, 'DD-MON-YY HH:MI PM') as statusDate , 'Pick' as status from CourierTransaction where ctid = " + ctid + @"
                            )
                             order by statusdate desc ";

            var myList = _context.CourierStatusDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("CourierPerformance")]
        public async Task<ActionResult<IEnumerable<CourierPerformanceDTO>>> CourierPerformance(String monthFlag)
        {
            string qry = "";
            Int32 myMonth = 0;

            if (monthFlag == "3M")
            {
                myMonth = 90;
            }
            else if (monthFlag == "6M")
            {
                myMonth = 180;
            }
            else
            {
                myMonth = 360;
            }

            qry = @" SELECT
    WAREHOUSEID,
    WAREHOUSENAME,
    COUNT(DISTINCT CTID) AS NoOfDocket,
    SUM(NoOFItems) AS NoOFItems,
    SUM(DaysTaken) AS DaysTaken,
    ROUND(SUM(DaysTaken) / COUNT(DISTINCT CTID), 2) AS AVGDays
FROM (
    SELECT DISTINCT
        w.warehouseid,getUndroppedDocket
        w.warehousename,
        tb.QDOCKETNO,
        tb.QDOCKETNO AS details,
        tb.ctid,
        ct.entrydatepick,
        ct.destinationtype,
        ct.destinationid,
        ct.weight,
        ct.unit,
        hi.status,
        COUNT(DISTINCT tbi.itemid) AS NoOFItems,
        TO_DATE(TO_CHAR(ct.DROPDATE, 'dd-MM-yyyy'), 'dd-MM-yyyy') AS chard,
        TO_DATE('01-APR-2024', 'dd-MM-yyyy') AS sdsd,
        TO_DATE(TO_CHAR(ct.DROPDATE, 'DD-MON-YYYY'), 'DD-MM-YYYY') AS DROPDATE,
        TO_DATE(TO_CHAR(ct.pickdate, 'DD-MON-YYYY'), 'DD-MM-YYYY') AS pickdate,
        TO_DATE(TO_CHAR(ct.DROPDATE, 'DD-MON-YYYY'), 'DD-MM-YYYY') - TO_DATE(TO_CHAR(ct.pickdate, 'DD-MON-YYYY'), 'DD-MM-YYYY') AS DaysTaken
    FROM
        tbindents tb
        INNER JOIN tbindentitems tbi ON tbi.indentid = tb.indentid
        INNER JOIN tboutwards tbo ON tbo.indentitemid = tbi.indentitemid
        INNER JOIN tbreceiptbatches rb ON rb.inwno = tbo.inwno
        INNER JOIN masitems m ON m.itemid = tbi.itemid
        INNER JOIN maswarehouses w ON w.warehouseid = tb.warehouseid
        INNER JOIN CourierTransaction ct ON ct.ctid = tb.ctid
        LEFT OUTER JOIN COURIER_STATUS_HISTORY hi ON hi.ctid = ct.ctid AND hi.islatest = 'Y'
    WHERE
        tb.Status = 'C'
        AND tb.CTID IS NOT NULL
        AND isdrop IS NOT NULL
        AND ct.DROPDATE >= TRUNC(SYSDATE) - " + myMonth + @"
        AND ct.DROPDATE < TRUNC(SYSDATE) + 1
    GROUP BY
        w.warehousename,
        tb.QDOCKETNO,
        w.warehouseid,
        tb.ctid,
        ct.entrydatepick,
        ct.pickdate,
        ct.destinationtype,
        ct.destinationid,
        ct.weight,
        ct.unit,
        hi.status,
        hi.entrydate,
        ct.DROPDATE
) 
GROUP BY
    WAREHOUSEID,
    WAREHOUSENAME;
 ";

            //            qry = @"  select WAREHOUSEID, WAREHOUSENAME,count(distinct CTID) as NoOfDocket,sum(NoOFItems) as NoOFItems,sum(DaysTaken) as DaysTaken,round(sum(DaysTaken)/count(distinct CTID),2) as AVGDays
            //from (
            //select distinct  w.warehouseid,w.warehousename,tb.QDOCKETNO                             
            //                                 ,tb.QDOCKETNO as details, tb.ctid
            //                                 ,ct.entrydatepick                                 
            //                                 --,TO_date(ct.pickdate, 'DD-MON-YY HH:MI:SS PM') AS pickdate                                 
            //                                 ,ct.destinationtype,ct.destinationid,ct.weight,ct.unit
            //                                 ,hi.status --,TO_CHAR(hi.entrydate, 'DD-MON-YY HH:MI:SS PM') AS entrydate
            //                                 ,count(distinct tbi.itemid) as NoOFItems, to_date(to_char(ct.DROPDATE,'dd-MM-yyyy'),'dd-MM-yyyy') as chard,to_date('01-APR-2024','dd-MM-yyyy') sdsd
            //                                 ,to_date( to_char(ct.DROPDATE,'DD-MON-YYYY'),'DD-MM-YYYY') as DROPDATE,to_date(to_char(ct.pickdate,'DD-MON-YYYY'),'DD-MM-YYYY') as pickdate
            //                                 ,to_date( to_char(ct.DROPDATE,'DD-MON-YYYY'),'DD-MM-YYYY') -to_date(to_char(ct.pickdate,'DD-MON-YYYY'),'DD-MM-YYYY')  as DaysTaken
            //                                 from tbindents tb
            //                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
            //                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
            //                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
            //                                 inner join masitems m on m.itemid = tbi.itemid
            //                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
            //                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
            //                                 left outer join COURIER_STATUS_HISTORY hi on hi.ctid = ct.ctid and hi.islatest = 'Y'
            //                                  where tb.Status = 'C' 
            //                                 and tb.CTID is not null                              
            //                                 and isdrop is not null  and  to_date(to_char(ct.DROPDATE,'dd-MM-yyyy'),'dd-MM-yyyy') between to_date('01-APR-2024','dd-MM-yyyy') and to_date('30-APR-2024','dd-MM-yyyy')
            //                                 group by  w.warehousename,tb.QDOCKETNO,w.warehouseid, tb.ctid
            //                                 ,ct.entrydatepick,ct.pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit 
            //                                 ,hi.status,hi.entrydate,ct.DROPDATE
            //                                 ) group by WAREHOUSEID, WAREHOUSENAME ";

            var myList = _context.CourierPerformanceDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("getUndroppedDocket")]
        public async Task<ActionResult<IEnumerable<getUndroppedDocketDTO>>> getUndroppedDocket(String monthFlag)
        {
            string qry = "";

            string whPickDate = "";

            if (monthFlag == "3M")
            {
                whPickDate = "  AND ct.pickdate >= TRUNC(SYSDATE) - 90  AND ct.pickdate < TRUNC(SYSDATE) + 1 ";
            }
            else if (monthFlag == "6M")
            {
                whPickDate = "  AND ct.pickdate >= TRUNC(SYSDATE) - 180  AND ct.pickdate < TRUNC(SYSDATE) + 1 ";
            }
            else
            {
                whPickDate = "  AND ct.pickdate >= TRUNC(SYSDATE) - 360  AND ct.pickdate < TRUNC(SYSDATE) + 1 ";
            }

            qry = @" select distinct  w.warehouseid,w.warehousename,tb.QDOCKETNO                             
                                 ,tb.QDOCKETNO as details, tb.ctid
                                 ,ct.entrydatepick,TO_CHAR(ct.pickdate, 'DD-MON-YY HH:MI') AS pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit
                                 ,hi.status,TO_CHAR(hi.entrydate, 'DD-MON-YY HH:MI:SS PM') AS entrydate
                                 ,  TRUNC(SYSDATE) - TRUNC(ct.pickdate) AS days_since_pickdate
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
                                 left outer join COURIER_STATUS_HISTORY hi on hi.ctid = ct.ctid and hi.islatest = 'Y'
                                  where tb.Status = 'C' 
                                 and tb.CTID is not null                              
                                 and isdrop is null
                                    " + whPickDate + @"
                                 group by  w.warehousename,tb.QDOCKETNO,w.warehouseid, tb.ctid
                                 ,ct.entrydatepick,ct.pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit 
                                 ,hi.status,hi.entrydate  ";



            var myList = _context.getUndroppedDocketDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



        [HttpGet("getPickDocketDetailsLab")]
        public async Task<ActionResult<IEnumerable<PickDocketDetailsLabDTO>>> getPickDocketDetailsLab(string docketNo, string labid)
        {
            string whDocketLab = "";

            if (docketNo != "0" && labid != "0")
            {
                whDocketLab = " and qt.docketno = '" + docketNo + "'  and ql.labid = " + labid + "";
            }


            qry = @"  select ROW_NUMBER() OVER (ORDER BY CTID) as id, '2931' as SourceID , CTID, (DOCKETNO || '(' || to_char(DOCKETDATE,'dd-MM-yyyy') || ')' ) as DOCKET ,DOCKETNO,DOCKETDATE,LABID,LABNAME,ADDRESS1,  ADDRESS2,  ADDRESS3, CITY,  ZIP,  PHONE1,count(qctestid) cntItems from (

                        select distinct qt.CTID,qt.qctestid,qt.sampleno ,qt.DOCKETNO,qt.DOCKETDATE,ql.LABID, ql.LABNAME, ql.ADDRESS1,  ql.ADDRESS2,  ql.ADDRESS3, ql. CITY,  ql.ZIP,  ql.PHONE1 
                        from qctests qt
                        inner join qcsamples qs on qs.sampleid=qt.sampleid
                        inner join qclabs ql on ql.labid=qt.labid
                        
                        where DOCKETNO is not null and  DOCKETDATE is not null and CTID is null
                        and DOCKETDATE>'13-May-2024' and qt.isCourierPickQC is null and qt.labid not in (398)
                        " + whDocketLab + @"
                        ) group by CTID,DOCKETNO,DOCKETDATE,LABID,LABNAME,ADDRESS1,  ADDRESS2,  ADDRESS3, CITY,  ZIP,  PHONE1 ";


            var myList = _context.PickDocketDetailsLabDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpPut("InsertCourierTransactionForLab")]
        public IActionResult InsertCourierTransactionForLab(Int64 SourceID, String SourceType, Int64 DestinationID, String DestinationType, Double WEIGHT, String Unit, string docketNo, Int64 labid)
        {
            string dt1 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
            bool isCTIDexist = false;


            string qry = @" insert into CourierTransaction(SourceID ,SourceType,EntryDatePick, PickDate,DestinationID,DestinationType,WEIGHT,UNIT,DOCKETNO)
                        values(" + SourceID + " ,'" + SourceType + "',sysdate, '" + dt1 + "'," + DestinationID + ",'" + DestinationType + "'," + WEIGHT + ",'" + Unit + "','" + docketNo + "')  ";
            _context.Database.ExecuteSqlRaw(qry);

            isCTIDexist = getCTIDForLab(docketNo, labid, out Int64 ctid);

            string qryUpdate = @" update qctests set ctid = " + ctid + " where DOCKETNO = '" + docketNo + "'   ";
            _context.Database.ExecuteSqlRaw(qryUpdate);


            return Ok("Successfully Saved");

        }


        private bool getCTIDForLab(string docketNo, Int64 labid, out Int64 ctid)
        {
            ctid = 0;


            string qry = @" select  CTID from CourierTransaction where DOCKETNO = '" + docketNo + "' and DESTINATIONID = " + labid + " order by ctid desc ";

            var result = _context.GetCourierTransactionDBSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList().FirstOrDefault();

            if (result == null)
            {
                ctid = 0;
                return false;

            }
            ctid = Convert.ToInt64(result.CTID);
            return true;

        }


        [HttpGet("pickedCourierToBeDropForLab")]
        public async Task<ActionResult<IEnumerable<pickedCourierToBeDropForLabDTO>>> pickedCourierToBeDropForLab(string docketNo, string labid)
        {
            string whDocketLab = "";

            if (docketNo != "0" && labid != "0")
            {
                whDocketLab = " and qt.docketno = '" + docketNo + "'  and ql.labid = " + labid + "";
            }

            qry = @" 
        select ROW_NUMBER() OVER (ORDER BY CTID) as id, '2931' as SourceID , CTID, (DOCKETNO || '(' || to_char(DOCKETDATE,'dd-MM-yyyy') || ')' ) as DOCKET ,DOCKETNO,DOCKETDATE,LABID,LABNAME,ADDRESS1,  ADDRESS2,  ADDRESS3, CITY,  ZIP,  PHONE1,count(qctestid) cntItems,weight from (

                        select distinct qt.CTID,qt.qctestid,qt.sampleno ,qt.DOCKETNO,qt.DOCKETDATE,ql.LABID, ql.LABNAME, ql.ADDRESS1,  ql.ADDRESS2,  ql.ADDRESS3, ql. CITY,  ql.ZIP,  ql.PHONE1 ,ct.weight
                        from qctests qt
                        inner join qcsamples qs on qs.sampleid=qt.sampleid
                        inner join qclabs ql on ql.labid=qt.labid
                        inner join couriertransaction ct on ct.ctid = qt.ctid and ct.docketno = qt.docketno                        
                        where qt.DOCKETNO is not null and  qt.DOCKETDATE is not null and qt.CTID is not null and isdrop is null
                        and qt.DOCKETDATE>'13-May-2024' and qt.isCourierPickQC is null
                         " + whDocketLab + @"                     
                        ) group by CTID,DOCKETNO,DOCKETDATE,LABID,LABNAME,ADDRESS1,  ADDRESS2,  ADDRESS3, CITY,  ZIP,  PHONE1,weight  ";

            var myList = _context.pickedCourierToBeDropForLabDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }






        [HttpGet("getPendingToDropInLab")]
        public async Task<ActionResult<IEnumerable<PendingToDropInLabDTO>>> getPendingToDropInLab(String monthFlag)
        {
            string qry = "";

            string whPickDate = "";

            if (monthFlag == "GREATER5")
            {
                whPickDate = "  and  (TRUNC(SYSDATE) - TRUNC(ct.pickdate))  > 5 ";
            }
            else if (monthFlag == "LESS2")
            {
                whPickDate = "  and  (TRUNC(SYSDATE) - TRUNC(ct.pickdate))  < 2 ";
            }
            else if (monthFlag == "BETWEEN3AND5")
            {
                whPickDate = "  and  (TRUNC(SYSDATE) - TRUNC(ct.pickdate))  between 3 and 5 ";
            }

            qry = @"  select ROW_NUMBER() OVER (ORDER BY CTID) as id, '2931' as SourceID , CTID, (DOCKETNO || '(' || to_char(DOCKETDATE,'dd-MM-yyyy') || ')' ) as DOCKET ,DOCKETNO,DOCKETDATE,LABID,LABNAME,ADDRESS1,  ADDRESS2,  ADDRESS3, CITY,  ZIP,  PHONE1,count(qctestid) cntItems,weight
,days_since_pickdate
 from (

                        select distinct qt.CTID,qt.qctestid,qt.sampleno ,qt.DOCKETNO,qt.DOCKETDATE,ql.LABID, ql.LABNAME, ql.ADDRESS1,  ql.ADDRESS2,  ql.ADDRESS3, ql. CITY,  ql.ZIP,  ql.PHONE1 ,ct.weight
                         , TRUNC(SYSDATE) - TRUNC(ct.pickdate) AS days_since_pickdate
                        from qctests qt
                        inner join qcsamples qs on qs.sampleid=qt.sampleid
                        inner join qclabs ql on ql.labid=qt.labid
                        inner join couriertransaction ct on ct.ctid = qt.ctid and ct.docketno = qt.docketno                        
                        where qt.DOCKETNO is not null and  qt.DOCKETDATE is not null and qt.CTID is not null and isdrop is null
                        and qt.DOCKETDATE>'13-May-2024'   and qt.isCourierPickQC is null  
                        " + whPickDate + @"
                        ) group by CTID,DOCKETNO,DOCKETDATE,LABID,LABNAME,ADDRESS1,  ADDRESS2,  ADDRESS3, CITY,  ZIP,  PHONE1,weight
                        ,days_since_pickdate ";



            var myList = _context.PendingToDropInLabDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpPut("ChangePwdCourier")]
        public IActionResult ChangePwdCourier(string NewPassword, string userid)
        {
            FacOperations ob = new FacOperations(_context);
            //return Ok("Test");

            bool isChange = ob.ChangePasswordCourier(NewPassword, userid);

            if (isChange)
            {
                return Ok("Successfully Saved");
            }

            return BadRequest();

        }


        [HttpGet("GetRaisedPicks")]
        public async Task<ActionResult<IEnumerable<PickRaisedDTO>>> GetRaisedPicks()
        {

            qry = @" select   w.warehousename,tb.QDOCKETNO ,tb.indentdate ,count(distinct tbi.itemid) as nousitems
                               
                                 ,w.warehouseid,tb.indentid,tb.indentno 
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 where tb.Status = 'C' and tb.issuetype  in ('QA','QS')             
                                 and tb.indentdate between '04-May-2024' and SYSDATE and tb.CTID is null and tb.isCourierPickQC is null
                                 and tb.QDOCKETNO not in ('0000000000')
                                 group by  w.warehousename,tb.QDOCKETNO,tb.indentid,tb.indentno ,tb.indentdate,w.warehouseid 
                                 order by w.warehouseid
                                  ";

            var myList = _context.PickRaisedDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("GetPendingToPick")]
        public async Task<ActionResult<IEnumerable<PendingToPickAndDropDTO>>> GetPendingToPick(Int32 warehouseid)
        {
            string whWarehouse = "";

            if (warehouseid != 0)
            {
                whWarehouse = @" and w.warehouseid = " + warehouseid + "";
            }

            qry = @" select ROW_NUMBER() OVER (ORDER BY w.warehousename) as id,   w.warehousename,tb.QDOCKETNO ,tb.indentdate ,count(distinct tbi.itemid) as nousitems                               
                                 ,w.warehouseid,tb.indentid,tb.indentno , m.itemname || '(' || m.itemcode || ')' as itemname, rb.batchno
                                 ,TO_DATE(TO_CHAR(sysdate, 'DD-MON-YYYY'), 'DD-MM-YYYY') - TO_DATE(TO_CHAR(tb.issuedate, 'DD-MON-YYYY'), 'DD-MM-YYYY') as PendingDays
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 where tb.Status = 'C' and tb.issuetype  in ('QA','QS')             
                                 and tb.indentdate between '04-May-2024' and SYSDATE and tb.CTID is null and tb.isCourierPickQC is null
                                 and tb.QDOCKETNO not in ('0000000000')
                                 " + whWarehouse + @"
                                 group by  w.warehousename,tb.QDOCKETNO,tb.indentid,tb.indentno ,tb.indentdate,w.warehouseid , m.itemname, m.itemcode
                                 , rb.batchno,tb.issuedate
                                 order by w.warehouseid ";

            var myList = _context.PendingToPickAndDropDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



        [HttpGet("GetPendingToDrop")]
        public async Task<ActionResult<IEnumerable<PendingToPickAndDropDTO>>> GetPendingToDrop(Int32 warehouseid)
        {
            string whWarehouse = "";

            if (warehouseid != 0)
            {
                whWarehouse = @" and w.warehouseid = " + warehouseid + "";
            }

            qry = @" select  distinct  w.warehouseid,w.warehousename,tb.QDOCKETNO                             
                                 ,tb.QDOCKETNO as details, tb.ctid
                                 ,ct.entrydatepick,TO_CHAR(ct.pickdate, 'DD-MON-YY HH:MI') AS pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit
                                 ,hi.status,TO_CHAR(hi.entrydate, 'DD-MON-YY HH:MI:SS PM') AS entrydate
                                 ,  TRUNC(SYSDATE) - TRUNC(ct.pickdate) AS PendingDays
                                 ,  m.itemname || '(' || m.itemcode || ')' as itemname, rb.batchno
                                 ,ROW_NUMBER() OVER (ORDER BY w.warehouseid) as id
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
                                 left outer join COURIER_STATUS_HISTORY hi on hi.ctid = ct.ctid and hi.islatest = 'Y'
                                  where tb.Status = 'C' 
                                 and tb.CTID is not null                              
                                 and isdrop is null
                                  " + whWarehouse + @"
                                 group by  w.warehousename,tb.QDOCKETNO,w.warehouseid, tb.ctid
                                 ,ct.entrydatepick,ct.pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit 
                                 ,hi.status,hi.entrydate, m.itemname, m.itemcode, rb.batchno ";

            var myList = _context.PendingToPickAndDropDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }




        //https://localhost:7247/api/Courier/ItemDetailDDL
        [HttpGet("ItemDetailDDL")]
        public async Task<ActionResult<IEnumerable<ItemDetailDDLDTO>>> ItemDetailDDL()
        {

            qry = @"select mc.MCATEGORY,m.itemname,m.strength1,m.unit,A.itemid,count(distinct A.warehouseid) as nosWH,nvl(ReadyForIssue,0) as ReadyForIssue
, sum(nvl(Pending,0)) as UQCQTY,count(distinct BATCHNO) nosBatches,max(A.MRCDATE) as LastMRCDT
,m.itemname || '-' || m.itemcode || '-' || ',Ready:' || to_char(nvl(ReadyForIssue,0)) || ',UQC:' ||    to_char(sum(nvl(Pending,0)))   || ',Batches:' || to_char(count(distinct BATCHNO))   
|| ',No of WH:' || to_char(count(distinct A.warehouseid)) || ',MRC:' || to_char(max(A.MRCDATE),'dd-MM-yyyy') as detailId  
from
                 (
                 select t.warehouseid,  b.inwno,  
                   case when m.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending,m.itemid,b.batchno
                   ,t.MRCDATE
                  from tbreceiptbatches b
                  inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner join tbreceipts t on t.receiptid = i.receiptid                 
                    inner join masitems m on m.itemid = b.itemid
                 left outer join
                (
                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where 1=1 and  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   group by tbi.itemid,tb.warehouseid,tbo.inwno
                 ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
                 Where T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate)
                 And(b.Whissueblock = 0 or b.Whissueblock is null)
                 and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null and m.qctest='Y'
                 and  (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end)>0
                --  and b.itemid=9996
                 ) A 
                 
             
                 
                inner join masitems m on m.itemid = A.itemid
                inner join masitemcategories c on c.categoryid=m.categoryid
                 inner join masitemmaincategory mc on mc.MCID = c.MCID  
                 
                 left outer join 
                 (
                  
 select     itemid,nvl(sum(ReadyForIssue),0) as ReadyForIssue
 from 
 (
 select t.warehouseid,  b.inwno, 
               (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when m.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue
,m.itemid
                  from tbreceiptbatches b
                  inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner join tbreceipts t on t.receiptid = i.receiptid                 
                  inner join masitems m on m.itemid = i.itemid
                 left outer join
                (
                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where 1=1 and  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   group by tbi.itemid,tb.warehouseid,tbo.inwno
                 ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
                 Where T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate)
                 And(b.Whissueblock = 0 or b.Whissueblock is null)
                 and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null and m.qctest='Y'
                 and (nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))>0
) group by itemid
 ) st on st.itemid=m.itemid
                 
                 group by m.itemname,m.strength1,m.unit,A.itemid,mc.MCATEGORY,ReadyForIssue,A.MRCDATE,m.itemcode
                 having (sum(nvl(A.Pending,0)))>0 order by nvl(sum(ReadyForIssue),0) ";

            var myList = _context.ItemDetailDDLDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



        //https://localhost:7247/api/Courier/GetPendingToPickByItem?itemId=13591
        [HttpGet("GetPendingToPickByItem")]
        public async Task<ActionResult<IEnumerable<PendingToPickByItemDTO>>> GetPendingToPickByItem(Int32 itemId)
        {
            string whItemId = "";
            itemId = 10708;
            if (itemId != 0)
            {
                whItemId = @" and m.itemid = " + itemId + "";
            }

            qry = @" select ROW_NUMBER() OVER (ORDER BY w.warehousename) as id,   w.warehousename,tb.QDOCKETNO ,tb.indentdate ,count(distinct tbi.itemid) as nousitems                               
                                 ,w.warehouseid,tb.indentid,tb.indentno , m.itemname || '(' || m.itemcode || ')' as itemname, rb.batchno
                                 ,TO_DATE(TO_CHAR(sysdate, 'DD-MON-YYYY'), 'DD-MM-YYYY') - TO_DATE(TO_CHAR(tb.issuedate, 'DD-MON-YYYY'), 'DD-MM-YYYY') as PendingDays
                                 ,m.itemid,tb.issuedate
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 where tb.Status = 'C' and tb.issuetype  in ('QA','QS')             
                                 and tb.indentdate between '04-May-2024' and SYSDATE and tb.CTID is null and tb.isCourierPickQC is null
                                 and tb.QDOCKETNO not in ('0000000000')
                                 " + whItemId + @"
                                 group by  w.warehousename,tb.QDOCKETNO,tb.indentid,tb.indentno ,tb.indentdate,w.warehouseid , m.itemname, m.itemcode,m.itemid
                                 , rb.batchno,tb.issuedate
                                 order by w.warehouseid ";

            var myList = _context.PendingToPickByItemDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        //https://localhost:7247/api/Courier/GetPendingToDropByItem?itemId=12678
        [HttpGet("GetPendingToDropByItem")]
        public async Task<ActionResult<IEnumerable<PendingToDropByItemDTO>>> GetPendingToDropByItem(Int32 itemId)
        {
            string whItemId = "";
            itemId = 10423;
            if (itemId != 0)
            {
                whItemId = @" and m.itemid = " + itemId + "";
            }

            qry = @" select  distinct  w.warehouseid,w.warehousename,tb.QDOCKETNO                             
                                 ,tb.QDOCKETNO as details, tb.ctid
                                 ,ct.entrydatepick,TO_CHAR(ct.pickdate, 'DD-MON-YY HH:MI') AS pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit
                                 ,hi.status,TO_CHAR(hi.entrydate, 'DD-MON-YY HH:MI:SS PM') AS entrydate
                                 ,  TRUNC(SYSDATE) - TRUNC(ct.pickdate) AS PendingDays
                                 ,  m.itemname || '(' || m.itemcode || ')' as itemname, rb.batchno
                                 ,ROW_NUMBER() OVER (ORDER BY w.warehouseid) as id, m.itemid,  TO_CHAR(ct.pickdate, 'DD-MON-YYYY') as pickdate1
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
                                 left outer join COURIER_STATUS_HISTORY hi on hi.ctid = ct.ctid and hi.islatest = 'Y'
                                  where tb.Status = 'C' 
                                 and tb.CTID is not null                              
                                 and isdrop is null
                                 " + whItemId + @"
                                 group by  w.warehousename,tb.QDOCKETNO,w.warehouseid, tb.ctid
                                 ,ct.entrydatepick,ct.pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit 
                                 ,hi.status,hi.entrydate, m.itemname, m.itemcode, rb.batchno, m.itemid,ct.pickdate ";

            var myList = _context.PendingToDropByItemDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        //https://localhost:7247/api/Courier/GetPendingToSendToLab?itemId=10826
        [HttpGet("GetPendingToSendToLab")]
        public async Task<ActionResult<IEnumerable<PendingToSendToLabDTO>>> GetPendingToSendToLab(Int32 itemId)
        {
            string whItemId = "";
            itemId = 0;
            if (itemId != 0)
            {
                whItemId = @" and m.itemid = " + itemId + "";
            }

            qry = @" select  row_number() over (Order by mcid) as id, mcid,mcategory,noofdays,count(distinct itemid) noofitems,count(distinct sampleid) noofsamples,count(distinct DOCKETNO) noofDOCKETS,count(distinct labid) nooflabs
from
(
select mc.mcid,mc.mcategory,m.itemid,m.itemcode,m.itemname,t.sampleid,t.sampleno,t.labid,t.labissuedate,t.DOCKETNO, t.DOCKETDATE,t.ctid,
round((sysdate-1)-t.DOCKETDATE,0) noofdays from qctests t
inner join qcsamples s on s.sampleid = t.sampleid or s.refsampleid = t.sampleid
inner join masitems m on m.itemid = s.itemid and m.qctest = 'Y'
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID 
where t.labid not in (398) and t.SAMPLERECEIVEDDATE is null
and t.DOCKETNO is not null
and t.CTID is null
--and t.labissuedate between '01-MAY-2024' and SYSDATE
" + whItemId + @"
) group by mcid,mcategory,noofdays order by noofdays desc ";

            var myList = _context.PendingToSendToLabDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        //https://localhost:7247/api/Courier/GetPendingToReceiptInHO?itemId=9448
        [HttpGet("GetPendingToReceiptInHO")]
        public async Task<ActionResult<IEnumerable<PendingToReceiptInHODTO>>> GetPendingToReceiptInHO(Int32 itemId)
        {
            string whItemId = "";
            itemId = 9448;
            if (itemId != 0)
            {
                whItemId = @" and m.itemid = " + itemId + "";
            }

            qry = @" 

select row_number() over (Order by mc.mcid) as id, mc.mcid,mc.mcategory,m.itemid,m.itemcode,m.itemname,tb.warehouseid,tb.indentid,tb.indentdate,tb.QDOCKETNO, tb.QDOCKETDT,rb.batchno,
rb.expdate,tbo.issueqty,ct.ctid,s.outwno,round((sysdate-1)-tb.QDOCKETDT,0) noofdays,TO_CHAR(ct.pickdate, 'DD-MON-YYYY') as pickdate, TO_CHAR(ct.dropdate, 'DD-MON-YYYY') as dropdate
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid and m.qctest = 'Y'
                                 inner join masitemcategories c on c.categoryid=m.categoryid
                                 inner join masitemmaincategory mc on mc.MCID = c.MCID 
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 left outer join qcsamples s on s.outwno = tbo.outwno
                                 left outer join couriertransaction ct on ct.ctid = tb.ctid
                                 where tb.Status = 'C' and tb.issuetype  in ('QA','QS') --and m.itemid = 17816            
                                 and s.outwno is null 
                                 and tb.indentdate between '01-MAY-2024' and SYSDATE 
                                 and ct.DESTINATIONTYPE = 'HO' and ct.CTID is not null and ct.ISDROP is not null
                                " + whItemId + @"
                         
 ";

            var myList = _context.PendingToReceiptInHODbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        //https://localhost:7247/api/Courier/GetPendingToReceiptInLab?itemId=9548
        [HttpGet("GetPendingToReceiptInLab")]
        public async Task<ActionResult<IEnumerable<PendingToReceiptInLabDTO>>> GetPendingToReceiptInLab(Int64 itemId)
        {
            string whItemId = "";
            itemId = 9548;
            if (itemId != 0)
            {
                whItemId = @" and m.itemid = " + itemId + "";
            }

            qry = @" select  w.warehouseid,w.warehousename,tb.QDOCKETNO   , tb.ctid
                                 ,ct.entrydatepick,TO_CHAR(ct.pickdate, 'DD-MON-YY HH:MI') AS pickdate,ct.destinationtype,ct.destinationid,ct.weight,ct.unit
                                 ,  m.itemname || '(' || m.itemcode || ')' as itemname, rb.batchno
                                 ,ROW_NUMBER() OVER (ORDER BY w.warehouseid) as id, m.itemid
                                 , TO_CHAR(ct.DROPDATE, 'DD-MON-YYYY') as dropdate,tbo.outwno
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join masitems m on m.itemid = tbi.itemid
                                 inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
                                 left outer join
                                 (
                                 select outwno from qcsamples 
                                 ) s on s.outwno = tbo.outwno
                                 where 1=1 
                                 --and tb.Status = 'C' and ct.isdrop = 'Y' 
                                 --and s.outwno is null 
                                 --and DESTINATIONTYPE = 'HO' order by ct.DROPDATE desc
                                " + whItemId + @"
                                 
                         
 ";

            var myList = _context.PendingToReceiptInLabDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        //https://localhost:7247/api/Courier/GetUnderLabSinceXdaysWithinTimeline?itemId=9646
        [HttpGet("GetUnderLabSinceXdaysWithinTimeline")]
        public async Task<ActionResult<IEnumerable<UnderLabSinceXdaysWithinTimelineDTO>>> GetUnderLabSinceXdaysWithinTimeline(Int32 itemId)
        {
            string whItemId = "";
            itemId = 9646;
            if (itemId != 0)
            {
                whItemId = @" and m.itemid = " + itemId + "";
            }

            qry = @"  select ROW_NUMBER() OVER (ORDER BY mcid) AS ID, mcid,mcategory,noofdays,count(distinct itemid) noofitems,count(distinct sampleid) noofsamples,count(distinct DOCKETNO) noofDOCKETS,count(distinct labid) nooflabs
from
(
select mc.mcid,mc.mcategory,m.itemid,m.itemcode,m.itemname,s.warehouseid,s.batchno,
t.sampleid,t.labid,t.labissuedate,t.DOCKETNO, t.DOCKETDATE,t.SAMPLERECEIVEDDATE,t.LABRESULT,t.testRESULT,
s.testRESULT hotresult,t.ctid,
round((sysdate)-t.SAMPLERECEIVEDDATE,0) noofdays,mt.qcdayslab from qctests t
inner join qcsamples s on s.sampleid = t.sampleid --or s.refsampleid = t.sampleid
inner join masitems m on m.itemid = s.itemid and m.qctest = 'Y'
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join masitemtypes mt on mt.itemtypeid = m.itemtypeid
where t.SAMPLERECEIVEDDATE between '01-MAY-2024' and SYSDATE
and t.LABUPLOADEDDATE is null and round((sysdate)-t.SAMPLERECEIVEDDATE,0)<mt.qcdayslab
" + whItemId + @"
) group by mcid,mcategory,noofdays order by noofdays desc
 ";

            var myList = _context.UnderLabSinceXdaysWithinTimelineDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



        //https://localhost:7247/api/Courier/GetUnderLabSinceXdaysOutOfTimeline?itemId=13591
        [HttpGet("GetUnderLabSinceXdaysOutOfTimeline")]
        public async Task<ActionResult<IEnumerable<UnderLabSinceXdaysOutOfTimelineDTO>>> GetUnderLabSinceXdaysOutOfTimeline(Int32 itemId)
        {
            string whItemId = "";
            itemId = 13591;
            if (itemId != 0)
            {
                whItemId = @" and m.itemid = " + itemId + "";
            }

            qry = @" select row_number() over (order by mcid) as id, mcid,mcategory,noofdays,count(distinct itemid) noofitems,count(distinct sampleid) noofsamples,count(distinct DOCKETNO) noofDOCKETS,count(distinct labid) nooflabs
from
(
select mc.mcid,mc.mcategory,m.itemid,m.itemcode,m.itemname,s.warehouseid,s.batchno,
t.sampleid,t.labid,t.labissuedate,t.DOCKETNO, t.DOCKETDATE,t.SAMPLERECEIVEDDATE,t.LABRESULT,t.testRESULT,
s.testRESULT hotresult,t.ctid,
round((sysdate)-t.SAMPLERECEIVEDDATE,0) noofdays,mt.qcdayslab from qctests t
inner join qcsamples s on s.sampleid = t.sampleid --or s.refsampleid = t.sampleid
inner join masitems m on m.itemid = s.itemid and m.qctest = 'Y'
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join masitemtypes mt on mt.itemtypeid = m.itemtypeid
where t.SAMPLERECEIVEDDATE between '01-MAY-2024' and SYSDATE
and t.LABUPLOADEDDATE is null and round((sysdate)-t.SAMPLERECEIVEDDATE,0)>mt.qcdayslab
" + whItemId + @"
) group by mcid,mcategory,noofdays order by noofdays desc
  ";

            var myList = _context.UnderLabSinceXdaysOutOfTimelineDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        //https://localhost:7247/api/Courier/GetFinalStatusPendingInHOQC?itemId=12686
        [HttpGet("GetFinalStatusPendingInHOQC")]
        public async Task<ActionResult<IEnumerable<FinalStatusPendingInHOQCDTO>>> GetFinalStatusPendingInHOQC(Int32 itemId)
        {
            string whItemId = "";
            itemId = 12686;
            if (itemId != 0)
            {
                whItemId = @" and m.itemid = " + itemId + "";
            }

            qry = @" select row_number() over (order by mcid) as id, mcid,mcategory,noofdays,count(distinct itemid) noofitems,count(distinct sampleid) noofsamples,count(distinct labid) nooflabs
from
(
select mc.mcid,mc.mcategory,m.itemid,m.itemcode,m.itemname,s.warehouseid,s.batchno,
t.sampleid,t.labid,t.labissuedate,t.DOCKETNO, t.DOCKETDATE,t.SAMPLERECEIVEDDATE,t.LABRESULT,
s.testRESULT hotestresult,t.ctid,s.testresult,s.newtestresult,
round((sysdate)-t.SAMPLERECEIVEDDATE,0) noofdays from qctests t
inner join qcsamples s on s.sampleid = t.sampleid --or s.refsampleid = t.sampleid
inner join masitems m on m.itemid = s.itemid and m.qctest = 'Y'
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
where t.REPORTRECEIVEDDATE between '01-MAY-2024' and SYSDATE
and nvl(s.newtestresult,s.testresult) is null
" + whItemId + @"
) group by mcid,mcategory,noofdays order by noofdays desc

  ";

            var myList = _context.FinalStatusPendingInHOQCDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("getUndroppedDocketDetailsLab")]
        public async Task<ActionResult<IEnumerable<UndroppedDocketDetailsLabDTO>>> getUndroppedDocketDetailsLab()
        {
            //string whDocketLab = "";

            //if (docketNo != "0" && labid != "0")
            //{
            //    whDocketLab = " and qt.docketno = '" + docketNo + "'  and ql.labid = " + labid + "";
            //}


            qry = @" select ROW_NUMBER() OVER (ORDER BY CTID) as id, '2931' as SourceID , CTID, (DOCKETNO || '(' || to_char(DOCKETDATE,'dd-MM-yyyy') || ')' ) as DOCKET ,DOCKETNO,DOCKETDATE,LABID,LABNAME,ADDRESS1,  ADDRESS2,  ADDRESS3, CITY,  ZIP,  PHONE1,count(qctestid) cntItems
,weight, unit
from (

                        select distinct qt.CTID,qt.qctestid,qt.sampleno ,qt.DOCKETNO,qt.DOCKETDATE,ql.LABID, ql.LABNAME, ql.ADDRESS1,  ql.ADDRESS2,  ql.ADDRESS3, ql. CITY,  ql.ZIP,  ql.PHONE1 
                        ,ct.weight, ct.unit
                        from qctests qt
                        inner join qcsamples qs on qs.sampleid=qt.sampleid
                        inner join qclabs ql on ql.labid=qt.labid
                        inner join couriertransaction ct on ct.ctid = qt.ctid
                        where qt.DOCKETNO is not null and  qt.DOCKETDATE is not null and qt.CTID is not null
                        and qt.DOCKETDATE>'13-May-2024' and qt.isCourierPickQC is null and qt.labid not in (398)
                      
                        ) group by CTID,DOCKETNO,DOCKETDATE,LABID,LABNAME,ADDRESS1,  ADDRESS2,  ADDRESS3, CITY,  ZIP,  PHONE1,weight, unit ";


            var myList = _context.UndroppedDocketDetailsLabDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



    }
}
