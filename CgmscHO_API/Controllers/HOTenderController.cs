using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CgmscHO_API.Models;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using MessagePack;
using System.Net.NetworkInformation;
using System.IO.Pipelines;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Diagnostics.Contracts;
using CgmscHO_API.HODTO;
//using Broadline.Controls;
//using CgmscHO_API.Utility;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HOTenderController : ControllerBase
    {

        private readonly OraDbContext _context;

        public HOTenderController(OraDbContext context)
        {
            _context = context;
        }
        //HO PO Related
        [HttpGet("TotalRC")]
        public async Task<ActionResult<IEnumerable<TotalRCDTO>>> TotalRC()
        {


            string qry = @" select mcid, MCATEGORY,sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL)+sum(NEDL) as Total
from 
(
select m.itemid,mc.mcid,mc.MCATEGORY,case when m.isedl2021='Y' then 1 else 0 end as EDL, case when m.isedl2021='Y' then 0 else 1 end as NEDL from masitems m
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
inner join 
(
select distinct itemid from v_rcvalid
) r on r.itemid=m.itemid
where m.ISFREEZ_ITPR is null
) group by MCATEGORY,mcid
order by mcid";
            var myList = _context.TotalRCDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }
        //HO PO Related
        [HttpGet("TotalRCMC")]
        public async Task<ActionResult<IEnumerable<TotalRCDTO>>> TotalRCMC(string mcid)
        {

            string whclause = "";
            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }
            string qry = @" select mcid, MCATEGORY,sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL)+sum(NEDL) as Total
from 
(
select m.itemid,mc.mcid,mc.MCATEGORY,case when m.isedl2021='Y' then 1 else 0 end as EDL, case when m.isedl2021='Y' then 0 else 1 end as NEDL from masitems m
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
inner join 
(
select distinct itemid from v_rcvalid
) r on r.itemid=m.itemid
where m.ISFREEZ_ITPR is null " + whclause + @" 
) group by MCATEGORY,mcid
order by mcid";
            var myList = _context.TotalRCDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("TotalAcceptance")]
        public async Task<ActionResult<IEnumerable<TotalTDTO>>> TotalAcceptance()
        {


            //            string qry = @" select mcid, MCATEGORY,sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL)+sum(NEDL) as Total,count(distinct SCHEMEID) as nosTender
            //from 
            //(
            //select mc.mcid, mc.MCATEGORY,case when m.isedl2021='Y' then 1 else 0 end as EDL, case when m.isedl2021='Y' then 0 else 1 end as NEDL
            //,SCHEMEID
            //from v_tenderstatusallnew v
            //inner join masitems m on m.itemid=v.itemid
            //inner join masitemcategories c on c.categoryid=m.categoryid
            //inner join masitemmaincategory mc on mc.MCID=c.MCID
            //where v.actioncode='Accepted in' 
            //and v.RCSCHEMEID is null and m.ISFREEZ_ITPR is null
            //) group by MCATEGORY,mcid
            //order by mcid ";


            string qry = @" select mcid, MCATEGORY, sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL) + sum(NEDL) as Total,count(distinct SCHEMEID) as nosTender
from
(
select l.SUPPLIERID, s.suppliername, m.itemcode, t.SCHEMEID, sch.schemename, mxtenderid, m.itemid, rc.contractitemid, mc.mcid,
mc.MCATEGORY,
case when m.isedl2021 = 'Y' then 1 else 0 end as EDL, case when m.isedl2021 = 'Y' then 0 else 1 end as NEDL
,case when rc.contractitemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end as Flag,l.fdate
from live_tender_price l
inner join live_tenders t on t.tid = l.tid
inner join massuppliers s on s.supplierid = l.supplierid
inner join masschemes sch on sch.schemeid = t.schemeid
inner join masitems m on m.itemcode = t.drug_code
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join
(
select ac.schemeid, aci.itemid, aci.contractitemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
) rc on rc.schemeid = t.SCHEMEID  and rc.itemid = m.itemid

left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode = t.drug_code
group by m.itemid
) mt on mt.itemid = m.itemid

where m.ISFREEZ_ITPR is null and l.isaccept = 'Y' and rc.contractitemid is null
and t.rejectdate is null and (case when rc.contractitemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end)= 1
and l.fdate > '1-Jan-2022'
) group by MCATEGORY,mcid
order by mcid ";

            var myList = _context.TotalTDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("TotalPriceBid")]
        public async Task<ActionResult<IEnumerable<TotalTDTO>>> TotalPriceBid(string RCValidInt)
        {
            string whRCClause = "";
            if (RCValidInt == "2")
            {
                whRCClause = " and rc.itemid is not null";
            }
            else if (RCValidInt == "3")
            {
                whRCClause = " and rc.itemid is null";
            }
            else
            {

            }
            string qry = @"  select mcid, MCATEGORY, sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL) + sum(NEDL) as Total,count(distinct SCHEMEID) as nosTender
from
(
select m.itemname, sch.schemename, l.TID, rc.itemid, mxtenderid, t.SCHEMEID, mc.mcid, m.itemcode,
mc.MCATEGORY,case when m.isedl2021 = 'Y' then 1 else 0 end as EDL, case when m.isedl2021 = 'Y' then 0 else 1 end as NEDL
,case when mxtenderid> t.SCHEMEID then 0 else 1 end as Flag from live_tenders t
inner join masschemes sch on sch.schemeid = t.schemeid
inner join masitems m on m.itemcode = t.drug_code
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
inner
join
(

select distinct  TID from live_tender_price l
inner join massuppliers s on s.supplierid = l.supplierid
where 1 = 1
and l.RANKID = 1 and l.isaccept is null
) l on l.tid = t.tid
left outer join
(
select distinct aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid = m.itemid
left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode = t.drug_code
group by m.itemid
) mt on mt.itemid = m.itemid
where m.ISFREEZ_ITPR is null and t.rejectdate is null " + whRCClause + @"
and (case when rc.itemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end)= 1
) group by MCATEGORY,mcid
order by mcid ";
            var myList = _context.TotalTDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("TotalPriceBidDril")]
        public async Task<ActionResult<IEnumerable<PriceBidDrilDTO>>> TotalPriceBidDril(string RCValidInt, string mcid, string edltype)
        {
            string whRCClause = "";
            if (RCValidInt == "2")
            {
                whRCClause = " and rc.itemid is not null";
            }
            else if (RCValidInt == "3")
            {
                whRCClause = " and rc.itemid is null";
            }
            else
            {

            }
            string edlcaluse = "";
            if (edltype == "1")
            {
                edlcaluse = " and (case when m.isedl2021= 'Y' then 1 else 0 end)=1 ";
            }
            else if (edltype == "2")
            {
                //Non edl
                edlcaluse = " and (case when m.isedl2021= 'Y' then 1 else 0 end)=0 ";
            }
            else
            {

            }



            string qry = @" select m.itemid, sch.schemeid, m.itemcode, m.itemname, sch.schemecode, sc.PRICEBIDDATE from live_tenders t
inner join masschemes sch on sch.schemeid= t.schemeid
inner join masschemesstatus sc on sc.schemeid= sch.schemeid
inner join masitems m on m.itemcode= t.drug_code
inner join masitemcategories c on c.categoryid= m.categoryid
inner join masitemmaincategory mc on mc.MCID= c.MCID
inner join
(

select distinct  TID, BASICRATE from live_tender_price l
inner join massuppliers s on s.supplierid= l.supplierid
where 1=1  
and l.RANKID= 1 and l.isaccept is null
) l on l.tid=t.tid
left outer join
(
select distinct aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid= aci.contractid
where ac.status= 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid=m.itemid
left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode= t.drug_code
group by m.itemid
) mt on mt.itemid=m.itemid
where m.ISFREEZ_ITPR is null and t.rejectdate is null " + whRCClause + @"
and (case when rc.itemid is null and mxtenderid>t.SCHEMEID then 0 else 1 end)=1
and mc.mcid=" + mcid + @"  " + edlcaluse + @"
order by sc.PRICEBIDDATE,sch.schemeid ";
            var myList = _context.PriceBidDrilDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("CovBAObjeClaimLiveDrillDown")]
        public async Task<ActionResult<IEnumerable<PriceBidDrilDTO>>> CovBAObjeClaimLiveDrillDown(string RCValidInt, string mcid, string edltype, string statusCOVID)
        {
            string strwhclause = "";
            if (statusCOVID == "3")
            {
                strwhclause = " and ms.iselibible_B = 'Y'";
            }

            string whRCClause = "";
            if (RCValidInt == "2")
            {
                whRCClause = " and rc.itemid is not null";
            }
            else if (RCValidInt == "3")
            {
                whRCClause = " and rc.itemid is null";
            }
            else
            {

            }
            string edlcaluse = "";
            if (edltype == "1")
            {
                edlcaluse = " and (case when m.isedl2021= 'Y' then 1 else 0 end)=1 ";
            }
            else if (edltype == "2")
            {
                //Non edl
                edlcaluse = " and (case when m.isedl2021= 'Y' then 1 else 0 end)=0 ";
            }
            else
            {

            }

            string qry = "";
            if (statusCOVID == "1")
            {
                qry = @"  select m.itemid,ms.schemeid, m.itemcode,m.itemname,ms.schemecode,st.ENDDT as PRICEBIDDATE,
case when m.isedl2021='Y' then 1 else 0 end as EDL, case when m.isedl2021='Y' then 0 else 1 end as NEDL 
,mc.MCATEGORY, mc.mcid
from live_tenders l
inner join masitems m  on m.itemcode=l.drug_code
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
inner join masschemes ms on ms.schemeid = l.schemeid
inner join masschemesstatus st on st.schemeid = l.schemeid
left outer join
(
select distinct aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid = m.itemid
where  m.ISFREEZ_ITPR is null and st.status =1
" + whRCClause + @" and mc.mcid=" + mcid + @" " + edlcaluse + @"
order by st.ENDDT ";

            }
            else
            {
                qry = @" select  a.itemid, a.schemeid, a.itemcode,a.itemname,sch.schemecode,case when sc.status=3 then sc.COV_B_OPDATE else case when sc.status=2 then sc.COV_A_OPDATE else sc.DA_DATE end end  as PRICEBIDDATE,
case when m.isedl2021 = 'Y' then 1 else 0 end as EDL, case when m.isedl2021 = 'Y' then 0 else 1 end as NEDL from
(
select  l.tid, c.schemeid, c.itemid, m.itemcode,case when mxtenderid> ms.SCHEMEID then 0 else 1 end as Flag,m.itemname,st.status
from schemestatusdetailschild c
inner join masitems m on m.itemid = c.itemid
inner join masschemesstatusdetails ms on ms.schstatusdid = c.schstatusdid
inner join masschemesstatus st on st.schemeid = ms.schemeid
inner join live_tenders l on l.schemeid = c.schemeid and l.drug_code = m.itemcode
left outer join
(
select distinct aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid = m.itemid
left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode = t.drug_code
group by m.itemid
) mt on mt.itemid = m.itemid
where m.ISFREEZ_ITPR is null " + strwhclause + @" and st.status = " + statusCOVID + @" " + whRCClause + @"
and(case when mxtenderid> ms.SCHEMEID then 0 else 1 end)= 1
)a
inner join masschemes sch on sch.schemeid = a.schemeid
inner join masschemesstatus sc on sc.schemeid= sch.schemeid
inner join masitems m on m.itemid = a.itemid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
and mc.mcid=" + mcid + @" " + edlcaluse + @"
group by a.itemid,a.schemeid,m.isedl2021,mc.mcid,mc.MCATEGORY, sch.schemecode,a.itemcode,a.itemname,sc.COV_B_OPDATE,
sc.status,sc.COV_A_OPDATE ,sc.DA_DATE
order by sc.COV_B_OPDATE,a.schemeid ";
            }
            var myList = _context.PriceBidDrilDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("TotalObjClaim")]
        public async Task<ActionResult<IEnumerable<TotalTDTO>>> TotalObjClaim()
        {


            string qry = @" select mcid, MCATEGORY,sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL)+sum(NEDL) as Total,count(distinct SCHEMEID) as nosTender
from 
(
select mc.mcid, mc.MCATEGORY,case when m.isedl2021='Y' then 1 else 0 end as EDL, case when m.isedl2021='Y' then 0 else 1 end as NEDL
,SCHEMEID
from v_tenderstatusallnew v
inner join masitems m on m.itemid=v.itemid
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
where v.actioncode='Claim Objection in' 
and v.RCSCHEMEID is null and m.ISFREEZ_ITPR is null
) group by MCATEGORY,mcid
order by mcid ";
            var myList = _context.TotalTDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("TotalCovA")]
        public async Task<ActionResult<IEnumerable<TotalTDTO>>> TotalCovA()
        {


            string qry = @" select mcid, MCATEGORY,sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL)+sum(NEDL) as Total,count(distinct SCHEMEID) as nosTender
from 
(
select mc.mcid, mc.MCATEGORY,case when m.isedl2021='Y' then 1 else 0 end as EDL, case when m.isedl2021='Y' then 0 else 1 end as NEDL
,SCHEMEID
from v_tenderstatusallnew v
inner join masitems m on m.itemid=v.itemid
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
where v.actioncode='Cover-A in' 
and v.RCSCHEMEID is null and m.ISFREEZ_ITPR is null
) group by MCATEGORY,mcid
order by mcid ";
            var myList = _context.TotalTDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("TotalCovB")]
        public async Task<ActionResult<IEnumerable<TotalTDTO>>> TotalCovB(string RCValidInt, string statusCOVID)
        {


            string strwhclause = "";
            if (statusCOVID == "3")
            {
                strwhclause = " and ms.iselibible_B = 'Y'";
            }

            string whRCClause = "";
            if (RCValidInt == "2")
            {
                whRCClause = " and rc.itemid is not null";
            }
            else if (RCValidInt == "3")
            {
                whRCClause = " and rc.itemid is null";
            }
            else
            {

            }

            string qry = @"   select mcid, MCATEGORY, sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL) + sum(NEDL) as Total,count(distinct SCHEMEID) as nosTender
from
(

select  a.itemid, a.schemeid, mc.mcid,
mc.MCATEGORY,
case when m.isedl2021 = 'Y' then 1 else 0 end as EDL, case when m.isedl2021 = 'Y' then 0 else 1 end as NEDL from
(
select  l.tid, c.schemeid, c.itemid, m.itemcode,case when mxtenderid> ms.SCHEMEID then 0 else 1 end as Flag
from schemestatusdetailschild c
inner join masitems m on m.itemid = c.itemid
inner join masschemesstatusdetails ms on ms.schstatusdid = c.schstatusdid
inner join masschemesstatus st on st.schemeid = ms.schemeid
inner join live_tenders l on l.schemeid = c.schemeid and l.drug_code = m.itemcode
left outer join
(
select distinct aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid = m.itemid
left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode = t.drug_code
group by m.itemid
) mt on mt.itemid = m.itemid
where m.ISFREEZ_ITPR is null " + strwhclause + @" and st.status = " + statusCOVID + @" " + whRCClause + @"
and(case when mxtenderid> ms.SCHEMEID then 0 else 1 end)= 1
)a
inner join masschemes sch on sch.schemeid = a.schemeid
inner join masitems m on m.itemid = a.itemid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
group by a.itemid,a.schemeid,m.isedl2021,mc.mcid,mc.MCATEGORY, sch.schemecode
) group by MCATEGORY,mcid
order by mcid ";

            var myList = _context.TotalTDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("TotalLive")]
        public async Task<ActionResult<IEnumerable<TotalTDTO>>> TotalLive(string RCValidInt)
        {


            string whRCClause = "";
            if (RCValidInt == "2")
            {
                whRCClause = " and rc.itemid is not null";
            }
            else if (RCValidInt == "3")
            {
                whRCClause = " and rc.itemid is null";
            }
            else
            {

            }
            string qry = @"  select mcid, MCATEGORY, sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL) + sum(NEDL) as Total,count(distinct SCHEMEID) as nosTender
from
(
select st.NITDATE, ms.schemeid, mc.mcid, m.itemcode, mc.MCATEGORY,case when m.isedl2021 = 'Y' then 1 else 0 end as EDL, case when m.isedl2021 = 'Y' then 0 else 1 end as NEDL
from live_tenders l
inner
join masitems m  on m.itemcode = l.drug_code
inner
join masitemcategories c on c.categoryid = m.categoryid
inner
join masitemmaincategory mc on mc.MCID = c.MCID
inner
join masschemes ms on ms.schemeid = l.schemeid
inner
join masschemesstatus st on st.schemeid = l.schemeid
left outer join
(
select distinct aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid = m.itemid
where m.ISFREEZ_ITPR is null and st.status = 1 " + whRCClause + @"
) group by MCATEGORY,mcid
order by mcid";

            var myList = _context.TotalTDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("TotaltoBeTender")]
        public async Task<ActionResult<IEnumerable<TotalTDTO>>> TotaltoBeTender()
        {


            string qry = @" select mcid, MCATEGORY,sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL)+sum(NEDL) as Total,count(distinct SCHEMEID) as nosTender
from 
(
select mc.mcid, mc.MCATEGORY,case when m.isedl2021='Y' then 1 else 0 end as EDL, case when m.isedl2021='Y' then 0 else 1 end as NEDL
,SCHEMEID
from v_tenderstatusallnew v
inner join masitems m on m.itemid=v.itemid
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
where v.actioncode='To be Retender' and m.ISFREEZ_ITPR is null
and v.RCSCHEMEID is null
) group by MCATEGORY,mcid
order by mcid ";
            var myList = _context.TotalTDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("RCValidTenderPostion")]
        public async Task<ActionResult<IEnumerable<TenderEveDTO>>> RCValidTenderPostion(string mcid, string edl, string rc_NoRC, string agIndent)
        {
            string whEDLNonEDL = "";
            if (edl == "1")
            {
                //
                whEDLNonEDL = " and  (case when m.isedl2021 = 'Y' then 1 else 0 end)=1";
            }
            else if (edl == "2")
            {
                whEDLNonEDL = " and  (case when m.isedl2021 = 'Y' then 1 else 0 end)=0";
            }
            else
            {

            }

            string indJoin = " left outer join ";
            if (agIndent == "1")
            {
                indJoin = " inner join ";
            }

            string qry = "";
            if (rc_NoRC == "1")
            {
                qry = @"   select Criteria,sum(EDL) +sum(NEDL) as total ,sum(Accept) as Accept
 ,sum(PriceBid) as PriceBid,sum(COVB) as COVB,sum(COVAOC) as COVAOC,sum(COVA) as COVA,sum(LiveT) as LiveT 
 ,case when Criteria in  ('<2 Months','<3 Months') then  (sum(EDL) +sum(NEDL)-(sum(Accept)+sum(PriceBid)+sum(COVB)+sum(COVAOC)+sum(COVA)+sum(LiveT)))
 else 0 end
 as tobeTender,
 Criteriaor
from 
(

select m.itemid, rc.schemeid,mt.mxtenderid,case when mt.mxtenderid>rc.schemeid then 1 else 0 end as IsNewTemder  ,
sc.status,
case when sc.status=1 then 1 else 0 end as LiveT,
case when (acc.tid is not null and acc.schemeid>rc.schemeid) then 1 else 0 end as Accept,
case when p.BASICRATE is not null then 1 else 0 end as PriceBid,
case when cov.itemid is not null then 1 else 0 end  as COVA,nvl(nosCOVA,0)nosCOVA
,case when covObj.itemid is not null then 1 else 0 end as COVAOC,nvl(nosCOVAObj,0) as nosCOVAOC
,case when covb.itemid is not null then 1 else 0 end  as COVB,nvl(nosCOVB,0) as nosCOVB
,case when (round(ac.ContractEndDate-(sysdate)+1))<=60 then '<2 M'
else case when (round(ac.ContractEndDate-(sysdate)+1))>60 and (round(ac.ContractEndDate-(sysdate)+1))<=90  then '<3 M'
else case when (round(ac.ContractEndDate-(sysdate)+1))>90 and (round(ac.ContractEndDate-(sysdate)+1))<=180  then '<6 M'
else '>6 M' end end end as Criteria
,case when (round(ac.ContractEndDate-(sysdate)+1))<=60 then '1.<2 Months'
else case when (round(ac.ContractEndDate-(sysdate)+1))>60 and (round(ac.ContractEndDate-(sysdate)+1))<=90  then '2.<3 Months'
else case when (round(ac.ContractEndDate-(sysdate)+1))>90 and (round(ac.ContractEndDate-(sysdate)+1))<=180  then '3.<6 Months'
else '4.>6 Months' end end end as Criteriaor
,to_char(ac.ContractStartDate,'dd-MM-yyyy') rcstart,
ac.ContractEndDate,round(ac.ContractEndDate-(sysdate)+1) as nosdayspending
,case when m.isedl2021 = 'Y' then 1 else 0 end as EDL, case when m.isedl2021 = 'Y' then 0 else 1 end as NEDL 
from masitems m
inner join 
(
select distinct itemid from v_rcvalid
) r on r.itemid=m.itemid
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID

inner join
(
select min(contractitemid) as CID,itemid from v_rcvalid
group by itemid
) rcc on  rcc.itemid = m.itemid
inner join aoccontractitems aci on aci.contractitemid=rcc.cid
inner join aoccontracts ac on ac.contractid = aci.contractid


left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode= t.drug_code
group by m.itemid
) mt on mt.itemid=m.itemid

left outer join
(
select distinct ac.schemeid,aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid= aci.contractid
where ac.status= 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid=m.itemid
inner join masschemesstatus sc on sc.schemeid= mt.mxtenderid
inner join live_tenders l on l.schemeid = mt.mxtenderid and l.drug_code = m.itemcode
left outer join 
(

select distinct  l.TID, l.BASICRATE from live_tender_price l
inner join live_tenders s on s.tid= l.tid
inner join masschemesstatus sc on sc.schemeid= s.schemeid
where 1=1  
and l.RANKID= 1 and l.isaccept is null and s.REJECTDATE is null and sc.status=4
) p on p.tid=l.tid

left outer join 
(
select ms.schemeid,count(distinct c.supplierid) as nosCOVA,c.itemid from masschemesstatusdetails ms
inner join schemestatusdetailschild c on c.schstatusdid=ms.schstatusdid 
inner join masschemesstatus sc on sc.schemeid= ms.schemeid
where FLAGCOA='Y' and  sc.status='2'
group by ms.schemeid,c.itemid
) cov on cov.schemeid= mt.mxtenderid and cov.itemid = m.itemid
left outer join 
(
select ms.schemeid,count(distinct c.supplierid) as nosCOVAObj,c.itemid from masschemesstatusdetails ms
inner join schemestatusdetailschild c on c.schstatusdid=ms.schstatusdid 
inner join masschemesstatus sc on sc.schemeid= ms.schemeid
where FLAGCOA='Y' and  sc.status='7'
group by ms.schemeid,c.itemid
) covObj on covObj.schemeid= mt.mxtenderid and covObj.itemid = m.itemid
left outer join 
(
select ms.schemeid,count(distinct c.supplierid) as nosCOVB,c.itemid from masschemesstatusdetails ms
inner join schemestatusdetailschild c on c.schstatusdid=ms.schstatusdid 
inner join masschemesstatus sc on sc.schemeid= ms.schemeid
where FLAGCOA='Y' and   ms.iselibible_B='Y' and sc.status='3' 
group by ms.schemeid,c.itemid
) covb on covb.schemeid= mt.mxtenderid and covb.itemid = m.itemid

left outer join 
(

select distinct  l.TID, s.schemeid from live_tender_price l
inner join live_tenders s on s.tid= l.tid
inner join masschemesstatus sc on sc.schemeid= s.schemeid
where 1=1  
and l.RANKID= 1 and l.isaccept ='Y' and s.REJECTDATE is null and sc.status=4
) acc on acc.tid=l.tid and acc.schemeid=mt.mxtenderid

where m.ISFREEZ_ITPR is null and mc.mcid=" + mcid + @" " + whEDLNonEDL + @"
)
group by Criteria,Criteriaor
order by Criteriaor ";
            }
            else
            {
                qry = @" select Criteria,sum(EDL) +sum(NEDL) as total ,sum(Accept) as Accept
 ,sum(PriceBid) as PriceBid,sum(COVB) as COVB,sum(COVAOC) as COVAOC,sum(COVA) as COVA,sum(LiveT) as LiveT
 , (sum(EDL) +sum(NEDL)-(sum(Accept)+sum(PriceBid)+sum(COVB)+sum(COVAOC)+sum(COVA)+sum(LiveT))) tobeTender
 ,Criteria as Criteriaor
from 
(

select m.itemid,m.itemcode,m.itemname,mt.mxtenderid ,case when acc.tid is not null  then 1 else 0 end as Accept
,case when acc.tid is null and p.TID is not null then 1 else 0 end as PriceBid
,case when covb.itemid is not null then 1 else 0 end as COVB,nvl(nosCOVB, 0) as nosCOVB
,case when covObj.itemid is not null then 1 else 0 end as COVAOC,nvl(nosCOVAObj, 0) as nosCOVAOC,
case when cov.itemid is not null then 1 else 0 end as COVA,nvl(nosCOVA, 0)nosCOVA
,case when sc.status = 1  and l.tid is not null then 1 else 0 end as LiveT
, case when mt.mxtenderid is null and l.tid is null then 1 else 0 end as tobeTender
,case when m.isedl2021 = 'Y' then 1 else 0 end as EDL, case when m.isedl2021 = 'Y' then 0 else 1 end as NEDL
,case when m.isedl2021 = 'Y' then 'EDL' else 'Non EDL' end as Criteria
from masitems m
" + indJoin + @"
(
select distinct itemid from itemindent i  where 1 = 1 and
nvl(i.DHS_INDENTQTY, 0) + nvl(DME_INDENTQTY, 0) > 0 and
accyrsetid = 544
) ai on ai.itemid = m.itemid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join
(
select distinct itemid from v_rcvalid
) r on r.itemid = m.itemid
left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode = t.drug_code
group by m.itemid
) mt on mt.itemid = m.itemid

left outer join masschemesstatus sc on sc.schemeid = mt.mxtenderid
left outer join live_tenders l on l.schemeid = mt.mxtenderid and l.drug_code = m.itemcode
left outer join
(

select distinct  l.TID, s.schemeid from live_tender_price l
inner join live_tenders s on s.tid = l.tid
inner join masschemesstatus sc on sc.schemeid = s.schemeid
where 1 = 1
and l.RANKID = 1 and l.isaccept is null and s.REJECTDATE is null and sc.status = 4
) p on p.tid = l.tid  and p.schemeid = mt.mxtenderid
left outer join
(

select distinct  l.TID, s.schemeid from live_tender_price l
inner join live_tenders s on s.tid = l.tid
inner join masschemesstatus sc on sc.schemeid = s.schemeid
where 1 = 1
and l.RANKID = 1 and l.isaccept = 'Y' and s.REJECTDATE is null and sc.status = 4
) acc on acc.tid = l.tid  and acc.schemeid = mt.mxtenderid

left outer join
(
select ms.schemeid, count(distinct c.supplierid) as nosCOVA, c.itemid from masschemesstatusdetails ms
inner join schemestatusdetailschild c on c.schstatusdid = ms.schstatusdid
inner join masschemesstatus sc on sc.schemeid = ms.schemeid
where FLAGCOA = 'Y' and  sc.status = '2'
group by ms.schemeid, c.itemid
) cov on cov.schemeid = mt.mxtenderid and cov.itemid = m.itemid
left outer join
(
select ms.schemeid, count(distinct c.supplierid) as nosCOVAObj, c.itemid from masschemesstatusdetails ms
inner join schemestatusdetailschild c on c.schstatusdid = ms.schstatusdid
inner join masschemesstatus sc on sc.schemeid = ms.schemeid
where FLAGCOA = 'Y' and  sc.status = '7'
group by ms.schemeid, c.itemid
) covObj on covObj.schemeid = mt.mxtenderid and covObj.itemid = m.itemid
left outer join
(
select ms.schemeid, count(distinct c.supplierid) as nosCOVB, c.itemid from masschemesstatusdetails ms
inner join schemestatusdetailschild c on c.schstatusdid = ms.schstatusdid
inner join masschemesstatus sc on sc.schemeid = ms.schemeid
where FLAGCOA = 'Y' and   ms.iselibible_B = 'Y' and sc.status = '3'
group by ms.schemeid, c.itemid
) covb on covb.schemeid = mt.mxtenderid and covb.itemid = m.itemid
where m.ISFREEZ_ITPR is null and r.itemid is null and mc.mcid=" + mcid + @" " + whEDLNonEDL + @"
)group by Criteria
order by Criteria ";

            }
            var myList = _context.TenderEveDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }




        [HttpGet("YearWiseRC")]
        public async Task<ActionResult<IEnumerable<YearWiseRCDTO>>> YearWiseRC(string mcatid)
        {
            string whMCatID = "";



            string qry = @" select accyrsetid,ACCYEAR,EDL,NONEDL,EDL+NONEDL as AllRC from (
select accyrsetid,SHACCYEAR as ACCYEAR,GetYearWiseRCStatus(ENDDATE,'Y'," + mcatid + @") EDL,
GetYearWiseRCStatus(ENDDATE,'N'," + mcatid + @") NONEDL 
from masaccyearsettings where accyrsetid <=(select ACCYRSETID from  masaccyearsettings where sysdate between STARTDATE and ENDDATE) ) 
group by accyrsetid,ACCYEAR,EDL,NONEDL
having (EDL+NONEDL)>0
order by accyrsetid ";
            var myList = _context.YearWiseRCDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;



        }

        [HttpGet("TenderStagePending")]
        public async Task<ActionResult<IEnumerable<TenderstageDTO>>> TenderStagePending(string mcatid, string covstatus)
        {
            string whMCatID = "";
            if (mcatid != "0")
            {
                whMCatID = "  and  mc1.mcid  =" + mcatid;
            }
            //string whcovstatus = " and sc.Status  in (1,2,3,4,7,8)";
            string whcovstatus = "";
            if (covstatus != "0")
            {
                whcovstatus = "  and Status ='" + covstatus + "'";
            }


            string qry = @" select categoryname,schemecode,schemename,systenderno as EprocNo,
to_char(startDt,'dd-MM-yyyy') as startDt,to_char(ActClosingDT,'dd-MM-yyyy') as ActClosingDT,
NoofItems,ItemAEDL,
nvl(to_char(Cov_A_Opdate,'dd-MM-yyyy'),'NA') as Cov_A_Opdate,
case when DA_DATE is null then 'NA' else DA_DATE end as DA_DATE ,nvl(to_char(COV_B_OPDATE,'dd-MM-yyyy'),'NA') as COV_B_OPDATE,nvl(to_char(PRICEBIDDATE,'dd-MM-yyyy'),'NA') as PRICEBIDDATE,

Noof_Bid_A,
NoofItemsCountA,
NoofItemsCountAEDL,webid,Status
,nvl(to_char(pendingforACRej),'NA') as PriceNotAccpt_Reject
,schemeid,statusid
,case when statusid=1 then (case when statusid='1' and Round(sysdate-ActClosingDT,0)>0
 then to_char((Round(sysdate-ActClosingDT,0)-1)||' days before Closed') else
 case when  statusid='1' and Round(sysdate-ActClosingDT,0)<0 then  to_char(Round(ActClosingDT-sysdate,0)||' days to go')  else '-' end end)
 else case when statusid in (2,7)   then remarks  
 else  case when statusid in (3) then 'Under Cov-B' 
 else case when statusid in (4) then 'Price Bid Opened,Accept/Reject Pending '||to_char(round(sysdate-PRICEBIDDATE))||' days' else 'Cancelled'
 end end end end  as remarksdata
 ,prc,nvl(to_char(PREBIDENDDT,'dd-MM-yyyy'),'NA') as PREBIDENDDT 
from
(
select  s.schemecode,s.schemename,sc.schemeid,sc.systenderno,NITDate as startDt,to_char(sc.enDDT,'dd-MM-yyyy') as EndDate,
case when mxDt.MaxDate is null then sc.enDDT else  mxDt.MaxDate end as ActClosingDT,
nvl(ext.noext,0) as noofExtension
,nvl(ItemLive,0) as NoofItems,nvl(ItemAEDL,0) as ItemAEDL,
Cov_A_Opdate,COV_B_OPDATE,
case when covA.cn is null then nvl(sc.biddersa,0) else nvl(covA.cn,0)  end as  Noof_Bid_A, 
nvl(CovA4.cnt,0) as NoofItemsCountA,nvl(CovA4E.cntEDL,0) as NoofItemsCountAEDL,
(nvl(ItemLive,0)- nvl(CovA4.cnt,0)) NoofItemsBidNotFoundA,
round(sysdate-Cov_A_Opdate) as CoverAUnderProcess,
round(sysdate-NITDate) totaldays,mc1.MCATEGORY as categoryname,sc.webid,round(Cov_A_Opdate-NITDate) as Livedays
,to_char(DA_DATE,'dd-MM-yyyy') as DA_DATE 
, case when DA_DATE is null then 'Under Cov-A,Since '||to_char(round(sysdate-Cov_A_Opdate))||' days'
else 'Under Claim Objection Since '||to_char(round(sysdate-DA_DATE))||' days' end as remarks
, 
(case when sc.Status='1' then 'Live' when sc.Status='2' then 'Cover A Opened' when sc.Status='3' then 'Cover B Opened' 
 when sc.Status='4' then 'Price Bid Opened' When sc.Status='5' then 'Cancelled' When sc.Status='7' then 'Claim Objection' When sc.Status='8' then 'Acceptance'  When sc.Status='9' then 'RC Created' else '' end) Status 
 ,sc.Status as statusid,PRICEBIDDATE
 ,case when sc.Status!=4 then 1 else  case when (sc.Status=4 and prc.schemeid is not null) then 1 else 0 end end as prc
 ,pendingforACRej,PREBIDENDDT
 from masschemes s 
inner join masschemesstatus sc on s.schemeid=sc.schemeid
inner join masitemcategories mc on mc.categoryid=sc.categoryid
 inner join masitemmaincategory mc1 on mc1.MCID = mc.MCID
 left outer join
 (
select count(distinct Drug_Code) as ItemLive,schemeid from live_tenders a  
group by schemeid
)  itm on itm.schemeid=s.schemeid 

 left outer join
 (
select count(distinct Drug_Code) as ItemAEDL,schemeid from live_tenders a  
inner join masitems m on m.itemcode=a.Drug_Code
where 1=1 and m.isedl2021='Y'
group by schemeid
)  itmE on itmE.schemeid=s.schemeid 

left outer join
 (
select count(enddt) as noext,schemeid from MASSCHEMESTATUSEXT a  
group by schemeid
)  ext on ext.schemeid=s.schemeid 

left outer join
 (
select max(enddt) as MaxDate,schemeid from MASSCHEMESTATUSEXT
group by schemeid
)  mxDT on mxDT.schemeid=s.schemeid

left outer join 
(
select count(supplierid) as cn,schemeid from masschemesstatusdetails c 
group by schemeid
) covA on covA.schemeid=s.schemeid
left outer join 
(
select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
where  flagcoa='Y' group by schemeid
) CovA4 on CovA4.schemeid=s.schemeid


left outer join 
(
select count(distinct sc.itemid) as cntEDL,schemeid from schemestatusdetailschild sc
inner join masitems m on m.itemid=sc.itemid
where  flagcoa='Y' and  m.isedl2021='Y' group by schemeid
) CovA4E on CovA4E.schemeid=s.schemeid

left outer join 
(
select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
where  flagcob='Y'  group by schemeid
) CovB4 on CovB4.schemeid=s.schemeid

left outer join 
(
select count(supplierid) as cnB,schemeid from masschemesstatusdetails c where  iselibible_b='Y'
group by schemeid
) covAShort on covAShort.schemeid=s.schemeid

left outer join 
(
 select  schemeid,count(distinct itemid) pendingforACRej from
 (
 select m.itemid, sch.schemeid from live_tenders t
inner join masschemes sch on sch.schemeid= t.schemeid
inner join masitems m on m.itemcode= t.drug_code
inner join masitemcategories c on c.categoryid= m.categoryid
inner join masitemmaincategory mc on mc.MCID= c.MCID
inner join
(

select distinct  TID, BASICRATE from live_tender_price l
inner join massuppliers s on s.supplierid= l.supplierid
where 1=1  
and l.RANKID= 1 and l.isaccept is null
) l on l.tid=t.tid

left outer join
(
select distinct aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid = m.itemid
left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode = t.drug_code
group by m.itemid
) mt on mt.itemid = m.itemid
where m.ISFREEZ_ITPR is null and t.rejectdate is null  and t.rejectdate is null  and (case when rc.itemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end)= 1
and mc.mcid=1
) a group by schemeid
) prc on prc.schemeid=s.schemeid 

where 1=1  and  sc.Status  in (1,2,3,4,7,8)   " + whMCatID + @"
and nvl(ItemLive,0)>0 
) where prc=1 " + whcovstatus + @"
order by startDt,statusid desc ";
            var myList = _context.TenderstageDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;



        }


        [HttpGet("TenderStagesTotal")]
        public async Task<ActionResult<IEnumerable<TenderStagesTotalDTO>>> TenderStagesTotal(string mcatid)
        {
            string whMCatID = "";
            string whMCatID1 = "";

            if (mcatid != "0")
            {
                whMCatID = "  and mc.mcid = " + mcatid;
                whMCatID1 = "  and  mc1.mcid  = " + mcatid;
            }


            string qry = @"  select y.STATUS,nvl(noTenders,0) as noTenders,nvl(OnOfItems,0) as OnOfItems,nvl(TenderValue,0) as TenderValue,orderdp from 
(
select 'Price Bid Opened' as Status,4 orderdp from dual
union
select 'Cover B Opened' as Status,6 orderdp from dual
union
select 'Cover A Opened' as Status,2 orderdp from dual
union
select 'Live' as Status, 1 orderdp from dual
union
select 'Claim Objection' as Status,3 orderdp from dual
union
select 'Acceptance' as Status,5 orderdp from dual

)y
left outer join
(
select STATUS,count(SCHEMEID) as noTenders,sum(NoofItems) as OnOfItems ,sum(TenderValue) as TenderValue
from
(
 
 select categoryname,schemecode,schemename,systenderno as EprocNo,
to_char(startDt,'dd-MM-yyyy') as startDt,to_char(ActClosingDT,'dd-MM-yyyy') as ActClosingDT,
NoofItems,ItemAEDL,TenderValue,
nvl(to_char(Cov_A_Opdate,'dd-MM-yyyy'),'NA') as Cov_A_Opdate,
case when DA_DATE is null then 'NA' else DA_DATE end as DA_DATE ,nvl(to_char(COV_B_OPDATE,'dd-MM-yyyy'),'NA') as COV_B_OPDATE,nvl(to_char(PRICEBIDDATE,'dd-MM-yyyy'),'NA') as PRICEBIDDATE,

Noof_Bid_A,
NoofItemsCountA,
NoofItemsCountAEDL,webid,Status
,nvl(to_char(pendingforACRej),'NA') as PriceNotAccpt_Reject
,schemeid,statusid
,case when statusid=1 then (case when statusid='1' and Round(sysdate-ActClosingDT,0)>0
 then to_char((Round(sysdate-ActClosingDT,0)-1)||' days before Closed') else
 case when  statusid='1' and Round(sysdate-ActClosingDT,0)<0 then  to_char(Round(ActClosingDT-sysdate,0)||' days to go')  else '-' end end)
 else case when statusid in (2,7)   then remarks  
 else  case when statusid in (3) then 'Under Cov-B' 
 else case when statusid in (4) then 'Price Bid Opened,Accept/Reject Pending '||to_char(round(sysdate-PRICEBIDDATE))||' days' else 'Cancelled'
 end end end end  as remarksdata
 ,prc,nvl(to_char(PREBIDENDDT,'dd-MM-yyyy'),'NA') as PREBIDENDDT 
from
(
select  s.schemecode,s.schemename,sc.schemeid,sc.systenderno,NITDate as startDt,to_char(sc.enDDT,'dd-MM-yyyy') as EndDate,
case when mxDt.MaxDate is null then sc.enDDT else  mxDt.MaxDate end as ActClosingDT,
nvl(ext.noext,0) as noofExtension
,nvl(ItemLive,0) as NoofItems
,nvl(itm.IndValue,0) TenderValue
,nvl(ItemAEDL,0) as ItemAEDL,
Cov_A_Opdate,COV_B_OPDATE,
case when covA.cn is null then nvl(sc.biddersa,0) else nvl(covA.cn,0)  end as  Noof_Bid_A, 
nvl(CovA4.cnt,0) as NoofItemsCountA,nvl(CovA4E.cntEDL,0) as NoofItemsCountAEDL,
(nvl(ItemLive,0)- nvl(CovA4.cnt,0)) NoofItemsBidNotFoundA,
round(sysdate-Cov_A_Opdate) as CoverAUnderProcess,
round(sysdate-NITDate) totaldays,mc1.MCATEGORY as categoryname,sc.webid,round(Cov_A_Opdate-NITDate) as Livedays
,to_char(DA_DATE,'dd-MM-yyyy') as DA_DATE 
, case when DA_DATE is null then 'Under Cov-A,Since '||to_char(round(sysdate-Cov_A_Opdate))||' days'
else 'Under Claim Objection Since '||to_char(round(sysdate-DA_DATE))||' days' end as remarks
, 
(case when sc.Status='1' then 'Live' when sc.Status='2' then 'Cover A Opened' when sc.Status='3' then 'Cover B Opened' 
 when sc.Status='4' then 'Price Bid Opened' When sc.Status='5' then 'Cancelled' When sc.Status='7' then 'Claim Objection' When sc.Status='8' then 'Acceptance'  When sc.Status='9' then 'RC Created' else '' end) Status 
 ,sc.Status as statusid,PRICEBIDDATE
 ,case when sc.Status!=4 then 1 else  case when (sc.Status=4 and prc.schemeid is not null) then 1 else 0 end end as prc
 ,pendingforACRej,PREBIDENDDT
 from masschemes s 
inner join masschemesstatus sc on s.schemeid=sc.schemeid
inner join masitemcategories mc on mc.categoryid=sc.categoryid
 inner join masitemmaincategory mc1 on mc1.MCID = mc.MCID
 left outer join
 (
select schemeid,count(distinct  Drug_Code) as ItemLive,round(sum(IndValue)/10000000,2) as IndValue   from (
select distinct Drug_Code ,schemeid
,nvl(ai.DHS_INDENTQTY,0)+ nvl(ai.DME_INDENTQTY,0)+ nvl(ai.MITANIN,0) as TotIndnet
,(nvl(SKUFINALRATE,0)*(nvl(ai.DHS_INDENTQTY,0)+ nvl(ai.DME_INDENTQTY,0)+ nvl(ai.MITANIN,0))) as IndValue
from live_tenders a  
inner join masitems m on m.itemcode=a.Drug_Code
left outer join itemindent ai on ai.itemid=m.itemid and accyrsetid=(select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate)
left outer join v_itemrate v on v.itemid=m.itemid
)
group by schemeid
)  itm on itm.schemeid=s.schemeid 

 left outer join
 (
select count(distinct Drug_Code) as ItemAEDL,schemeid from live_tenders a  
inner join masitems m on m.itemcode=a.Drug_Code
where 1=1 and m.isedl2021='Y'
group by schemeid
)  itmE on itmE.schemeid=s.schemeid 

left outer join
 (
select count(enddt) as noext,schemeid from MASSCHEMESTATUSEXT a  
group by schemeid
)  ext on ext.schemeid=s.schemeid 

left outer join
 (
select max(enddt) as MaxDate,schemeid from MASSCHEMESTATUSEXT
group by schemeid
)  mxDT on mxDT.schemeid=s.schemeid

left outer join 
(
select count(supplierid) as cn,schemeid from masschemesstatusdetails c 
group by schemeid
) covA on covA.schemeid=s.schemeid
left outer join 
(
select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
where  flagcoa='Y' group by schemeid
) CovA4 on CovA4.schemeid=s.schemeid


left outer join 
(
select count(distinct sc.itemid) as cntEDL,schemeid from schemestatusdetailschild sc
inner join masitems m on m.itemid=sc.itemid
where  flagcoa='Y' and  m.isedl2021='Y' group by schemeid
) CovA4E on CovA4E.schemeid=s.schemeid

left outer join 
(
select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
where  flagcob='Y'  group by schemeid
) CovB4 on CovB4.schemeid=s.schemeid

left outer join 
(
select count(supplierid) as cnB,schemeid from masschemesstatusdetails c where  iselibible_b='Y'
group by schemeid
) covAShort on covAShort.schemeid=s.schemeid

left outer join 
(
 select  schemeid,count(distinct itemid) pendingforACRej from
 (
 select m.itemid, sch.schemeid from live_tenders t
inner join masschemes sch on sch.schemeid= t.schemeid
inner join masitems m on m.itemcode= t.drug_code
inner join masitemcategories c on c.categoryid= m.categoryid
inner join masitemmaincategory mc on mc.MCID= c.MCID
inner join
(

select distinct  TID, BASICRATE from live_tender_price l
inner join massuppliers s on s.supplierid= l.supplierid
where 1=1  
and l.RANKID= 1 and l.isaccept is null
) l on l.tid=t.tid

left outer join
(
select distinct aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid = m.itemid
left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode = t.drug_code
group by m.itemid
) mt on mt.itemid = m.itemid
where m.ISFREEZ_ITPR is null and t.rejectdate is null  and t.rejectdate is null  and (case when rc.itemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end)= 1
   "+ whMCatID + @"   
) a group by schemeid
) prc on prc.schemeid=s.schemeid 

where 1=1  and sc.Status  in (1,2,3,4,8,7)       "+ whMCatID1 + @"  
and nvl(ItemLive,0)>0 
) where prc=1
order by startDt,statusid desc

)group by STATUS

)x on x.STATUS=y.STATUS

order by orderdp   ";









//            string qry = @"  select y.STATUS,nvl(noTenders,0) as noTenders,nvl(OnOfItems,0) as OnOfItems,orderdp from 
//(
//select 'Price Bid Opened' as Status,4 orderdp from dual
//union
//select 'Cover B Opened' as Status,6 orderdp from dual
//union
//select 'Cover A Opened' as Status,2 orderdp from dual
//union
//select 'Live' as Status, 1 orderdp from dual
//union
//select 'Claim Objection' as Status,3 orderdp from dual
//union
//select 'Acceptance' as Status,5 orderdp from dual

//)y
//left outer join
//(
//select STATUS,count(SCHEMEID) as noTenders,sum(NoofItems) as OnOfItems 
//from
//(
 
// select categoryname,schemecode,schemename,systenderno as EprocNo,
//to_char(startDt,'dd-MM-yyyy') as startDt,to_char(ActClosingDT,'dd-MM-yyyy') as ActClosingDT,
//NoofItems,ItemAEDL,
//nvl(to_char(Cov_A_Opdate,'dd-MM-yyyy'),'NA') as Cov_A_Opdate,
//case when DA_DATE is null then 'NA' else DA_DATE end as DA_DATE ,nvl(to_char(COV_B_OPDATE,'dd-MM-yyyy'),'NA') as COV_B_OPDATE,nvl(to_char(PRICEBIDDATE,'dd-MM-yyyy'),'NA') as PRICEBIDDATE,

//Noof_Bid_A,
//NoofItemsCountA,
//NoofItemsCountAEDL,webid,Status
//,nvl(to_char(pendingforACRej),'NA') as PriceNotAccpt_Reject
//,schemeid,statusid
//,case when statusid=1 then (case when statusid='1' and Round(sysdate-ActClosingDT,0)>0
// then to_char((Round(sysdate-ActClosingDT,0)-1)||' days before Closed') else
// case when  statusid='1' and Round(sysdate-ActClosingDT,0)<0 then  to_char(Round(ActClosingDT-sysdate,0)||' days to go')  else '-' end end)
// else case when statusid in (2,7)   then remarks  
// else  case when statusid in (3) then 'Under Cov-B' 
// else case when statusid in (4) then 'Price Bid Opened,Accept/Reject Pending '||to_char(round(sysdate-PRICEBIDDATE))||' days' else 'Cancelled'
// end end end end  as remarksdata
// ,prc,nvl(to_char(PREBIDENDDT,'dd-MM-yyyy'),'NA') as PREBIDENDDT 
//from
//(
//select  s.schemecode,s.schemename,sc.schemeid,sc.systenderno,NITDate as startDt,to_char(sc.enDDT,'dd-MM-yyyy') as EndDate,
//case when mxDt.MaxDate is null then sc.enDDT else  mxDt.MaxDate end as ActClosingDT,
//nvl(ext.noext,0) as noofExtension
//,nvl(ItemLive,0) as NoofItems,nvl(ItemAEDL,0) as ItemAEDL,
//Cov_A_Opdate,COV_B_OPDATE,
//case when covA.cn is null then nvl(sc.biddersa,0) else nvl(covA.cn,0)  end as  Noof_Bid_A, 
//nvl(CovA4.cnt,0) as NoofItemsCountA,nvl(CovA4E.cntEDL,0) as NoofItemsCountAEDL,
//(nvl(ItemLive,0)- nvl(CovA4.cnt,0)) NoofItemsBidNotFoundA,
//round(sysdate-Cov_A_Opdate) as CoverAUnderProcess,
//round(sysdate-NITDate) totaldays,mc1.MCATEGORY as categoryname,sc.webid,round(Cov_A_Opdate-NITDate) as Livedays
//,to_char(DA_DATE,'dd-MM-yyyy') as DA_DATE 
//, case when DA_DATE is null then 'Under Cov-A,Since '||to_char(round(sysdate-Cov_A_Opdate))||' days'
//else 'Under Claim Objection Since '||to_char(round(sysdate-DA_DATE))||' days' end as remarks
//, 
//(case when sc.Status='1' then 'Live' when sc.Status='2' then 'Cover A Opened' when sc.Status='3' then 'Cover B Opened' 
// when sc.Status='4' then 'Price Bid Opened' When sc.Status='5' then 'Cancelled' When sc.Status='7' then 'Claim Objection' When sc.Status='8' then 'Acceptance'  When sc.Status='9' then 'RC Created' else '' end) Status 
// ,sc.Status as statusid,PRICEBIDDATE
// ,case when sc.Status!=4 then 1 else  case when (sc.Status=4 and prc.schemeid is not null) then 1 else 0 end end as prc
// ,pendingforACRej,PREBIDENDDT
// from masschemes s 
//inner join masschemesstatus sc on s.schemeid=sc.schemeid
//inner join masitemcategories mc on mc.categoryid=sc.categoryid
// inner join masitemmaincategory mc1 on mc1.MCID = mc.MCID
// left outer join
// (
//select count(distinct Drug_Code) as ItemLive,schemeid from live_tenders a  
//group by schemeid
//)  itm on itm.schemeid=s.schemeid 

// left outer join
// (
//select count(distinct Drug_Code) as ItemAEDL,schemeid from live_tenders a  
//inner join masitems m on m.itemcode=a.Drug_Code
//where 1=1 and m.isedl2021='Y'
//group by schemeid
//)  itmE on itmE.schemeid=s.schemeid 

//left outer join
// (
//select count(enddt) as noext,schemeid from MASSCHEMESTATUSEXT a  
//group by schemeid
//)  ext on ext.schemeid=s.schemeid 

//left outer join
// (
//select max(enddt) as MaxDate,schemeid from MASSCHEMESTATUSEXT
//group by schemeid
//)  mxDT on mxDT.schemeid=s.schemeid

//left outer join 
//(
//select count(supplierid) as cn,schemeid from masschemesstatusdetails c 
//group by schemeid
//) covA on covA.schemeid=s.schemeid
//left outer join 
//(
//select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
//where  flagcoa='Y' group by schemeid
//) CovA4 on CovA4.schemeid=s.schemeid


//left outer join 
//(
//select count(distinct sc.itemid) as cntEDL,schemeid from schemestatusdetailschild sc
//inner join masitems m on m.itemid=sc.itemid
//where  flagcoa='Y' and  m.isedl2021='Y' group by schemeid
//) CovA4E on CovA4E.schemeid=s.schemeid

//left outer join 
//(
//select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
//where  flagcob='Y'  group by schemeid
//) CovB4 on CovB4.schemeid=s.schemeid

//left outer join 
//(
//select count(supplierid) as cnB,schemeid from masschemesstatusdetails c where  iselibible_b='Y'
//group by schemeid
//) covAShort on covAShort.schemeid=s.schemeid

//left outer join 
//(
// select  schemeid,count(distinct itemid) pendingforACRej from
// (
// select m.itemid, sch.schemeid from live_tenders t
//inner join masschemes sch on sch.schemeid= t.schemeid
//inner join masitems m on m.itemcode= t.drug_code
//inner join masitemcategories c on c.categoryid= m.categoryid
//inner join masitemmaincategory mc on mc.MCID= c.MCID
//inner join
//(

//select distinct  TID, BASICRATE from live_tender_price l
//inner join massuppliers s on s.supplierid= l.supplierid
//where 1=1  
//and l.RANKID= 1 and l.isaccept is null
//) l on l.tid=t.tid

//left outer join
//(
//select distinct aci.itemid from aoccontractitems aci
//inner join aoccontracts ac on ac.contractid = aci.contractid
//where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
//) rc on  rc.itemid = m.itemid
//left outer join
//(
//select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
//inner join masitems m on m.itemcode = t.drug_code
//group by m.itemid
//) mt on mt.itemid = m.itemid
//where m.ISFREEZ_ITPR is null and t.rejectdate is null  and t.rejectdate is null  and (case when rc.itemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end)= 1
// "+ whMCatID + @"   
//) a group by schemeid
//) prc on prc.schemeid=s.schemeid 

//where 1=1  and sc.Status  in (1,2,3,4,8,7)     "+ whMCatID1 + @"  
//and nvl(ItemLive,0)>0 
//) where prc=1
//order by startDt,statusid desc

//)group by STATUS


//)x on x.STATUS=y.STATUS

//order by orderdp 
// ";


            var myList = _context.TenderStagesTotalDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;



        }



        [HttpGet("StatusDetail")]
        public async Task<ActionResult<IEnumerable<StatusDetailDTO>>> StatusDetail(string status, string mcatid)
        {
            string whStatus = "";
            string whMCatID = "";
            string whMCatID1 = "";
            string qry = "";

            if (mcatid != "0")
            {
                whMCatID = "  and mc.mcid = " + mcatid;
                whMCatID1 = "  and  mc1.mcid  = " + mcatid;
            }


            if (status != "0")
            {
                whStatus = "and  STATUS='" + status + "' ";

            }


            if (status != "0")
            {


                qry = @"  select categoryname,schemecode,schemename,systenderno as EprocNo,
to_char(startDt,'dd-MM-yyyy') as startDt,to_char(ActClosingDT,'dd-MM-yyyy') as ActClosingDT,
NoofItems,TenderValue,ItemAEDL,
nvl(to_char(Cov_A_Opdate,'dd-MM-yyyy'),'NA') as Cov_A_Opdate,
case when DA_DATE is null then 'NA' else DA_DATE end as DA_DATE ,nvl(to_char(COV_B_OPDATE,'dd-MM-yyyy'),'NA') as COV_B_OPDATE,nvl(to_char(PRICEBIDDATE,'dd-MM-yyyy'),'NA') as PRICEBIDDATE,

Noof_Bid_A,
NoofItemsCountA,
NoofItemsCountAEDL,webid,Status
,nvl(to_char(pendingforACRej),'NA') as PriceNotAccpt_Reject
,x.schemeid,statusid
,

case when statusid=1 then (case when statusid='1' and Round(sysdate-ActClosingDT,0)>0
 then to_char((Round(sysdate-ActClosingDT,0)-1)||' days before Closed') else
 case when  statusid='1' and Round(sysdate-ActClosingDT,0)<0 then  to_char(Round(ActClosingDT-sysdate,0)||' days to go')  else '-' end end)
 else case when statusid in (2,7)   then remarks  
 else  case when statusid in (3) then 'Under Cov-B' 
 else case when statusid in (4) then 'Price Bid Opened,Accept/Reject Pending '||to_char(round(sysdate-PRICEBIDDATE))||' days' else 'Cancelled'
 end end end end  as remarksdataOLD
 ,
 

 nvl(tenderstatus,(
 case when statusid=1 then (case when statusid='1' and Round(sysdate-ActClosingDT,0)>0
 then 'Tender Closed and Cover A To Be Opened'  else
 case when  statusid='1' and Round(sysdate-ActClosingDT,0)<0 then  to_char(Round(ActClosingDT-sysdate,0)||' days to go')  else '-' end end)
 else case when statusid in (2,7)   then remarks  
 else  case when statusid in (3) then 'Under Cov-B' 
 else case when statusid in (4) then 'Price Bid Opened,Accept/Reject Pending '||to_char(round(sysdate-PRICEBIDDATE))||' days' else 'Cancelled'
 end end end end))  as remarksdata
 ,tenderremark,entrydate as RemarkEntryDate
 ,prc,nvl(to_char(PREBIDENDDT,'dd-MM-yyyy'),'NA') as PREBIDENDDT 
from
(


select  s.schemecode,s.schemename,sc.schemeid,sc.systenderno,NITDate as startDt,to_char(sc.enDDT,'dd-MM-yyyy') as EndDate,
case when mxDt.MaxDate is null then sc.enDDT else  mxDt.MaxDate end as ActClosingDT,
nvl(ext.noext,0) as noofExtension
,nvl(ItemLive,0) as NoofItems,nvl(TenderValue,0) as TenderValue,nvl(ItemAEDL,0) as ItemAEDL,
Cov_A_Opdate,COV_B_OPDATE,
case when covA.cn is null then nvl(sc.biddersa,0) else nvl(covA.cn,0)  end as  Noof_Bid_A, 
nvl(CovA4.cnt,0) as NoofItemsCountA,nvl(CovA4E.cntEDL,0) as NoofItemsCountAEDL,
(nvl(ItemLive,0)- nvl(CovA4.cnt,0)) NoofItemsBidNotFoundA,
round(sysdate-Cov_A_Opdate) as CoverAUnderProcess,
round(sysdate-NITDate) totaldays,mc1.MCATEGORY as categoryname,sc.webid,round(Cov_A_Opdate-NITDate) as Livedays
,to_char(DA_DATE,'dd-MM-yyyy') as DA_DATE 
, case when DA_DATE is null then 'Under Cov-A,Since '||to_char(round(sysdate-Cov_A_Opdate))||' days'
else 'Under Claim Objection Since '||to_char(round(sysdate-DA_DATE))||' days' end as remarks
, 
(case when sc.Status='1' then 'Live' when sc.Status='2' then 'Cover A Opened' when sc.Status='3' then 'Cover B Opened' 
 when sc.Status='4' then 'Price Bid Opened' When sc.Status='5' then 'Cancelled' When sc.Status='7' then 'Claim Objection' else '' end) Status 
 ,sc.Status as statusid,PRICEBIDDATE
 ,case when sc.Status!=4 then 1 else  case when (sc.Status=4 and prc.schemeid is not null) then 1 else 0 end end as prc
 ,pendingforACRej,PREBIDENDDT
 from masschemes s 
inner join masschemesstatus sc on s.schemeid=sc.schemeid
inner join masitemcategories mc on mc.categoryid=sc.categoryid
 inner join masitemmaincategory mc1 on mc1.MCID = mc.MCID
 left outer join
 (
 
select schemeid,count(distinct  Drug_Code) as ItemLive,round(sum(IndValue)/10000000,2) as TenderValue   from (
select distinct Drug_Code ,schemeid
,nvl(ai.DHS_INDENTQTY,0)+ nvl(ai.DME_INDENTQTY,0)+ nvl(ai.MITANIN,0) as TotIndnet
,(nvl(SKUFINALRATE,0)*(nvl(ai.DHS_INDENTQTY,0)+ nvl(ai.DME_INDENTQTY,0)+ nvl(ai.MITANIN,0))) as IndValue
from live_tenders a  
inner join masitems m on m.itemcode=a.Drug_Code
left outer join itemindent ai on ai.itemid=m.itemid and accyrsetid=546
left outer join v_itemrate v on v.itemid=m.itemid
)
group by schemeid

) itm on itm.schemeid=s.schemeid 

 left outer join
 (
select count(distinct Drug_Code) as ItemAEDL,schemeid from live_tenders a  
inner join masitems m on m.itemcode=a.Drug_Code
where 1=1 and m.isedl2021='Y'
group by schemeid
)  itmE on itmE.schemeid=s.schemeid 

left outer join
 (
select count(enddt) as noext,schemeid from MASSCHEMESTATUSEXT a  
group by schemeid
)  ext on ext.schemeid=s.schemeid 

left outer join
 (
select max(enddt) as MaxDate,schemeid from MASSCHEMESTATUSEXT
group by schemeid
)  mxDT on mxDT.schemeid=s.schemeid

left outer join 
(
select count(supplierid) as cn,schemeid from masschemesstatusdetails c 
group by schemeid
) covA on covA.schemeid=s.schemeid
left outer join 
(
select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
where  flagcoa='Y' group by schemeid
) CovA4 on CovA4.schemeid=s.schemeid


left outer join 
(
select count(distinct sc.itemid) as cntEDL,schemeid from schemestatusdetailschild sc
inner join masitems m on m.itemid=sc.itemid
where  flagcoa='Y' and  m.isedl2021='Y' group by schemeid
) CovA4E on CovA4E.schemeid=s.schemeid

left outer join 
(
select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
where  flagcob='Y'  group by schemeid
) CovB4 on CovB4.schemeid=s.schemeid

left outer join 
(
select count(supplierid) as cnB,schemeid from masschemesstatusdetails c where  iselibible_b='Y'
group by schemeid
) covAShort on covAShort.schemeid=s.schemeid

left outer join 
(
 select  schemeid,count(distinct itemid) pendingforACRej from
 (
 select m.itemid, sch.schemeid from live_tenders t
inner join masschemes sch on sch.schemeid= t.schemeid
inner join masitems m on m.itemcode= t.drug_code
inner join masitemcategories c on c.categoryid= m.categoryid
inner join masitemmaincategory mc on mc.MCID= c.MCID
inner join
(

select distinct  TID, BASICRATE from live_tender_price l
inner join massuppliers s on s.supplierid= l.supplierid
where 1=1  
and l.RANKID= 1 and l.isaccept is null
) l on l.tid=t.tid

left outer join
(
select distinct aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid = m.itemid
left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode = t.drug_code
group by m.itemid
) mt on mt.itemid = m.itemid
where m.ISFREEZ_ITPR is null and t.rejectdate is null  and t.rejectdate is null  and (case when rc.itemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end)= 1
  "+ whMCatID + @"
) a group by schemeid
) prc on prc.schemeid=s.schemeid 

where 1=1  and sc.Status  in (1,2,3,4,7)      "+ whMCatID1 + @"
and nvl(ItemLive,0)>0 


)x
left outer join
(

select tr.SCHEMEID,  tr.tenderremark,
to_char( tr.entrydate,'dd-MM-yyyy') as entrydate
,t.tenderstatus,tr.TSID 
from
TENDERSTATUSREMARK tr 
inner join TENDERSTATUSMASTER t  on t.tsid=tr.tsid
where tr.ISNEW='Y'  order by  TSID desc

)tsr on  tsr.schemeid=x.schemeid

where prc=1 "+ whStatus + @" 
order by startDt,statusid desc  ";

            }

            var myList = _context.StatusDetailDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;



        }


        //        [HttpGet("StatusDetail")]
        //        public async Task<ActionResult<IEnumerable<StatusDetailDTO>>> StatusDetail(string status, string mcatid)
        //        {
        //            string whStatus = "";
        //            string whMCatID = "";
        //            string whMCatID1 = "";
        //            string qry = "";

        //            if (mcatid != "0")
        //            {
        //                whMCatID = "  and mc.mcid = " + mcatid;
        //                whMCatID1 = "  and  mc1.mcid  = " + mcatid;
        //            }


        //            if (status != "0")
        //            {
        //                whStatus = "and  STATUS='" + status + "' ";

        //            }


        //            if (status != "0")
        //            {
        //                if (status != "Acceptance")
        //                {

        //                    qry = @"  SELECT 
        //  TO_CHAR(categoryname) AS categoryname,
        //  TO_CHAR(schemecode) AS schemecode,
        //  TO_CHAR(schemename) AS schemename,
        //  TO_CHAR(systenderno) AS EprocNo,
        //  TO_CHAR(startDt, 'dd-MM-yyyy') AS startDt,
        //  TO_CHAR(ActClosingDT, 'dd-MM-yyyy') AS ActClosingDT,
        //  TO_CHAR(NoofItems) AS NoofItems,
        //  TO_CHAR(ItemAEDL) AS ItemAEDL,
        //  NVL(TO_CHAR(Cov_A_Opdate, 'dd-MM-yyyy'), 'NA') AS Cov_A_Opdate,
        //  CASE 
        //    WHEN DA_DATE IS NULL THEN 'NA' 
        //    ELSE TO_CHAR(DA_DATE, 'dd-MM-yyyy') 
        //  END AS DA_DATE,
        //  NVL(TO_CHAR(COV_B_OPDATE, 'dd-MM-yyyy'), 'NA') AS COV_B_OPDATE,
        //  NVL(TO_CHAR(PRICEBIDDATE, 'dd-MM-yyyy'), 'NA') AS PRICEBIDDATE,
        //  TO_CHAR(Noof_Bid_A) AS Noof_Bid_A,
        //  TO_CHAR(NoofItemsCountA) AS NoofItemsCountA,
        //  TO_CHAR(NoofItemsCountAEDL) AS NoofItemsCountAEDL,
        //  TO_CHAR(webid) AS webid,
        //  TO_CHAR(Status) AS Status,
        //  NVL(TO_CHAR(pendingforACRej), 'NA') AS PriceNotAccpt_Reject,
        //  TO_CHAR(schemeid) AS schemeid,
        //  TO_CHAR(statusid) AS statusid,
        //  CASE 
        //    WHEN statusid = 1 THEN 
        //      CASE 
        //        WHEN ROUND(SYSDATE - ActClosingDT, 0) > 0 THEN 
        //          TO_CHAR(ROUND(SYSDATE - ActClosingDT, 0) - 1) || ' days before Closed'
        //        WHEN ROUND(SYSDATE - ActClosingDT, 0) < 0 THEN 
        //          TO_CHAR(ROUND(ActClosingDT - SYSDATE, 0)) || ' days to go'
        //        ELSE '-' 
        //      END
        //    WHEN statusid IN (2, 7) THEN TO_CHAR(remarks)
        //    WHEN statusid = 3 THEN 'Under Cov-B'
        //    WHEN statusid = 4 THEN 'Price Bid Opened,Accept/Reject Pending ' || TO_CHAR(ROUND(SYSDATE - PRICEBIDDATE)) || ' days'
        //    ELSE 'Cancelled'
        //  END AS remarksdata,
        //  TO_CHAR(prc) AS prc,
        //  NVL(TO_CHAR(PREBIDENDDT, 'dd-MM-yyyy'), 'NA') AS PREBIDENDDT
        //from
        //(
        //select  s.schemecode,s.schemename,sc.schemeid,sc.systenderno,NITDate as startDt,to_char(sc.enDDT,'dd-MM-yyyy') as EndDate,
        //case when mxDt.MaxDate is null then sc.enDDT else  mxDt.MaxDate end as ActClosingDT,
        //nvl(ext.noext,0) as noofExtension
        //,nvl(ItemLive,0) as NoofItems,nvl(ItemAEDL,0) as ItemAEDL,
        //Cov_A_Opdate,COV_B_OPDATE,
        //case when covA.cn is null then nvl(sc.biddersa,0) else nvl(covA.cn,0)  end as  Noof_Bid_A, 
        //nvl(CovA4.cnt,0) as NoofItemsCountA,nvl(CovA4E.cntEDL,0) as NoofItemsCountAEDL,
        //(nvl(ItemLive,0)- nvl(CovA4.cnt,0)) NoofItemsBidNotFoundA,
        //round(sysdate-Cov_A_Opdate) as CoverAUnderProcess,
        //round(sysdate-NITDate) totaldays,mc1.MCATEGORY as categoryname,sc.webid,round(Cov_A_Opdate-NITDate) as Livedays
        //,to_char(DA_DATE,'dd-MM-yyyy') as DA_DATE 
        //, case when DA_DATE is null then 'Under Cov-A,Since '||to_char(round(sysdate-Cov_A_Opdate))||' days'
        //else 'Under Claim Objection Since '||to_char(round(sysdate-DA_DATE))||' days' end as remarks
        //, 
        //(case when sc.Status='1' then 'Live' when sc.Status='2' then 'Cover A Opened' when sc.Status='3' then 'Cover B Opened' 
        // when sc.Status='4' then 'Price Bid Opened' When sc.Status='5' then 'Cancelled' When sc.Status='7' then 'Claim Objection' else '' end) Status 
        // ,sc.Status as statusid,PRICEBIDDATE
        // ,case when sc.Status!=4 then 1 else  case when (sc.Status=4 and prc.schemeid is not null) then 1 else 0 end end as prc
        // ,pendingforACRej,PREBIDENDDT
        // from masschemes s 
        //inner join masschemesstatus sc on s.schemeid=sc.schemeid
        //inner join masitemcategories mc on mc.categoryid=sc.categoryid
        // inner join masitemmaincategory mc1 on mc1.MCID = mc.MCID
        // left outer join
        // (
        //select count(distinct Drug_Code) as ItemLive,schemeid from live_tenders a  
        //group by schemeid
        //)  itm on itm.schemeid=s.schemeid 

        // left outer join
        // (
        //select count(distinct Drug_Code) as ItemAEDL,schemeid from live_tenders a  
        //inner join masitems m on m.itemcode=a.Drug_Code
        //where 1=1 and m.isedl2021='Y'
        //group by schemeid
        //)  itmE on itmE.schemeid=s.schemeid 

        //left outer join
        // (
        //select count(enddt) as noext,schemeid from MASSCHEMESTATUSEXT a  
        //group by schemeid
        //)  ext on ext.schemeid=s.schemeid 

        //left outer join
        // (
        //select max(enddt) as MaxDate,schemeid from MASSCHEMESTATUSEXT
        //group by schemeid
        //)  mxDT on mxDT.schemeid=s.schemeid

        //left outer join 
        //(
        //select count(supplierid) as cn,schemeid from masschemesstatusdetails c 
        //group by schemeid
        //) covA on covA.schemeid=s.schemeid
        //left outer join 
        //(
        //select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
        //where  flagcoa='Y' group by schemeid
        //) CovA4 on CovA4.schemeid=s.schemeid


        //left outer join 
        //(
        //select count(distinct sc.itemid) as cntEDL,schemeid from schemestatusdetailschild sc
        //inner join masitems m on m.itemid=sc.itemid
        //where  flagcoa='Y' and  m.isedl2021='Y' group by schemeid
        //) CovA4E on CovA4E.schemeid=s.schemeid

        //left outer join 
        //(
        //select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
        //where  flagcob='Y'  group by schemeid
        //) CovB4 on CovB4.schemeid=s.schemeid

        //left outer join 
        //(
        //select count(supplierid) as cnB,schemeid from masschemesstatusdetails c where  iselibible_b='Y'
        //group by schemeid
        //) covAShort on covAShort.schemeid=s.schemeid

        //left outer join 
        //(
        // select  schemeid,count(distinct itemid) pendingforACRej from
        // (
        // select m.itemid, sch.schemeid from live_tenders t
        //inner join masschemes sch on sch.schemeid= t.schemeid
        //inner join masitems m on m.itemcode= t.drug_code
        //inner join masitemcategories c on c.categoryid= m.categoryid
        //inner join masitemmaincategory mc on mc.MCID= c.MCID
        //inner join
        //(

        //select distinct  TID, BASICRATE from live_tender_price l
        //inner join massuppliers s on s.supplierid= l.supplierid
        //where 1=1  
        //and l.RANKID= 1 and l.isaccept is null
        //) l on l.tid=t.tid

        //left outer join
        //(
        //select distinct aci.itemid from aoccontractitems aci
        //inner join aoccontracts ac on ac.contractid = aci.contractid
        //where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
        //) rc on  rc.itemid = m.itemid
        //left outer join
        //(
        //select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
        //inner join masitems m on m.itemcode = t.drug_code
        //group by m.itemid
        //) mt on mt.itemid = m.itemid
        //where m.ISFREEZ_ITPR is null and t.rejectdate is null  and t.rejectdate is null  and (case when rc.itemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end)= 1
        //" + whMCatID + @"
        //) a group by schemeid
        //) prc on prc.schemeid=s.schemeid 

        //where 1=1  and sc.Status  in (1,2,3,4,7)    " + whMCatID1 + @"
        //and nvl(ItemLive,0)>0 
        //) where prc=1 " + whStatus + @"
        //order by startDt,statusid desc  ";
        //                }

        //                else   //if Acceptance
        //                {
        //                    qry = @" ";
        //                }
        //            }

        //            var myList = _context.StatusDetailDbSet
        //           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //            return myList;



        //        }

        [HttpGet("StatusItemDetail")]
        public async Task<ActionResult<IEnumerable<StatusItemDetailDTO>>> StatusItemDetail(string schemeId)
        {
            string whSchemeId = "";
            string whSchemeId1 = "";


            if (schemeId != "0")
            {
                whSchemeId = " and a.schemeid=" + schemeId + "  ";
                whSchemeId1 = " and schemeid=" + schemeId + "  ";

            }



            string qry = @" select a.tid, m.itemcode,itemname,strength1 as strength ,unit,
case when nvl(isedl2021,'N')='Y' then 'EDL' else 'Non EDL'  end as isedl2021 
,PRICEFLAG 
,count(TPRICEID) as toNoOfParticipant,min(BASICRATE ) as L1Basic
,round(nvl(IndValue,0)/100000,2) as IndValue
from live_tenders a 
inner join masitems m on m.itemcode=a.DRUG_CODE
left outer join live_tender_price tp on tp.TID=a.TID
left outer join
(
select distinct Drug_Code,m.itemid ,schemeid
,nvl(ai.DHS_INDENTQTY,0)+ nvl(ai.DME_INDENTQTY,0)+ nvl(ai.MITANIN,0) as TotIndnet
,(nvl(SKUFINALRATE,0)*(nvl(ai.DHS_INDENTQTY,0)+ nvl(ai.DME_INDENTQTY,0)+ nvl(ai.MITANIN,0))) as IndValue
from live_tenders a  
inner join masitems m on m.itemcode=a.Drug_Code
left outer join itemindent ai on ai.itemid=m.itemid and accyrsetid=546
left outer join v_itemrate v on v.itemid=m.itemid
where 1=1 "+ whSchemeId1 + @"
)ten on ten.schemeid=a.schemeid and ten.itemid=m.itemid
where 1=1  "+ whSchemeId + @"  
group by a.tid, m.itemcode,itemname,strength1 ,isedl2021,PRICEFLAG,unit,IndValue
order by nvl(isedl2021,'N') ";
            var myList = _context.StatusItemDetailDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;



        }

        [HttpGet("TotalRC1")]
        public async Task<ActionResult<IEnumerable<TotalRCDTO>>> TotalRC1(string mcatid)
        {

            string whMCatID = "";


            if (mcatid != "0")
            {
                whMCatID = "  and mc.mcid = " + mcatid;
            }


            string qry = @" select mcid, MCATEGORY,sum(EDL) as EDL,sum(NEDL) as NEDL,sum(EDL)+sum(NEDL) as Total
from 
(
select m.itemid,mc.mcid,mc.MCATEGORY,case when m.isedl2021='Y' then 1 else 0 end as EDL, case when m.isedl2021='Y' then 0 else 1 end as NEDL from masitems m
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
inner join 
(
select distinct itemid from v_rcvalid
) r on r.itemid=m.itemid
where m.ISFREEZ_ITPR is null
" + whMCatID + @"
) group by MCATEGORY,mcid
order by mcid";
            var myList = _context.TotalRCDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("TotalTender")]
        public async Task<ActionResult<IEnumerable<TotalTenderDTO>>> TotalTender(string categoryId)
        {
            string whCategoryId = "";
            string whCategoryId1 = "";


            if (categoryId != "0")
            {
                whCategoryId = " and  mc.mcid  ="+ categoryId + @"    ";
                whCategoryId1 = " and  mc1.mcid  ="+ categoryId + @"    ";

            }



            string qry = @" select CATEGORYNAME, SCHEMECODE, SCHEMENAME, STARTDT, ENDDATE, NOOFITEMS,TenderValue, NOOF_BID_A, TENDERSTATUS, TENDERREMARK, STATUSENTRYDATE,schemeid from (
 
 select categoryname,schemecode,schemename,systenderno as EprocNo,
to_char(startDt,'dd-MM-yyyy') as startDt
,x.EndDate
,to_char(ActClosingDT,'dd-MM-yyyy') as ActClosingDT,
NoofItems,ItemAEDL,
nvl(to_char(Cov_A_Opdate,'dd-MM-yyyy'),'NA') as Cov_A_Opdate,
case when DA_DATE is null then 'NA' else DA_DATE end as DA_DATE ,nvl(to_char(COV_B_OPDATE,'dd-MM-yyyy'),'NA') as COV_B_OPDATE,nvl(to_char(PRICEBIDDATE,'dd-MM-yyyy'),'NA') as PRICEBIDDATE,

Noof_Bid_A,
NoofItemsCountA,
NoofItemsCountAEDL,webid,Status
,nvl(to_char(pendingforACRej),'NA') as PriceNotAccpt_Reject
,x.schemeid,x.statusid
,case when statusid=1 then (case when statusid='1' and Round(sysdate-ActClosingDT,0)>0
 then to_char((Round(sysdate-ActClosingDT,0)-1)||' days before Closed') else
 case when  statusid='1' and Round(sysdate-ActClosingDT,0)<0 then  to_char(Round(ActClosingDT-sysdate,0)||' days to go')  else '-' end end)
 else case when statusid in (2,7)   then remarks  
 else  case when statusid in (3) then 'Under Cov-B' 
 else case when statusid in (4) then 'Price Bid Opened,Accept/Reject Pending '||to_char(round(sysdate-PRICEBIDDATE))||' days' else 'Cancelled'
 end end end end  as remarksdata
 ,x.prc,nvl(to_char(PREBIDENDDT,'dd-MM-yyyy'),'NA') as PREBIDENDDT 
 ,  nvl(TENDERSTATUS,STATUS) as TENDERSTATUS, TENDERREMARK, ENTRYDATE as StatusEntrydate,TenderValue
 
from
(


select  s.schemecode,s.schemename,sc.schemeid,sc.systenderno,NITDate as startDt,to_char(sc.enDDT,'dd-MM-yyyy') as EndDate,
case when mxDt.MaxDate is null then sc.enDDT else  mxDt.MaxDate end as ActClosingDT,
nvl(ext.noext,0) as noofExtension
,nvl(ItemLive,0) as NoofItems,nvl(ItemAEDL,0) as ItemAEDL,
Cov_A_Opdate,COV_B_OPDATE,
case when covA.cn is null then nvl(sc.biddersa,0) else nvl(covA.cn,0)  end as  Noof_Bid_A, 
nvl(CovA4.cnt,0) as NoofItemsCountA,nvl(CovA4E.cntEDL,0) as NoofItemsCountAEDL,
(nvl(ItemLive,0)- nvl(CovA4.cnt,0)) NoofItemsBidNotFoundA,
round(sysdate-Cov_A_Opdate) as CoverAUnderProcess,
round(sysdate-NITDate) totaldays,mc1.MCATEGORY as categoryname,sc.webid,round(Cov_A_Opdate-NITDate) as Livedays
,to_char(DA_DATE,'dd-MM-yyyy') as DA_DATE 
, case when DA_DATE is null then 'Under Cov-A,Since '||to_char(round(sysdate-Cov_A_Opdate))||' days'
else 'Under Claim Objection Since '||to_char(round(sysdate-DA_DATE))||' days' end as remarks
, 
(case when sc.Status='1' then 'Live' when sc.Status='2' then 'Cover A Opened' when sc.Status='3' then 'Cover B Opened' 
 when sc.Status='4' then 'Price Bid Opened' When sc.Status='5' then 'Cancelled' When sc.Status='7' then 'Claim Objection' When sc.Status='8' then 'Acceptance'  When sc.Status='9' then 'RC Created' else  '' end) Status 
 ,sc.Status as statusid,PRICEBIDDATE
 ,case when sc.Status!=4 then 1 else  case when (sc.Status=4 and prc.schemeid is not null) then 1 else  case when nvl(accstatus,0)=5 then 1 else   0 end end end as prc
 ,pendingforACRej,PREBIDENDDT,nvl(itm.TenderValue,0)  as TenderValue
 from masschemes s 
inner join masschemesstatus sc on s.schemeid=sc.schemeid
inner join masitemcategories mc on mc.categoryid=sc.categoryid
 inner join masitemmaincategory mc1 on mc1.MCID = mc.MCID
 left outer join
 (
select schemeid,count(distinct  Drug_Code) as ItemLive,round(sum(IndValue)/10000000,2) as TenderValue   from (
select distinct Drug_Code ,schemeid
,nvl(ai.DHS_INDENTQTY,0)+ nvl(ai.DME_INDENTQTY,0)+ nvl(ai.MITANIN,0) as TotIndnet
,(nvl(SKUFINALRATE,0)*(nvl(ai.DHS_INDENTQTY,0)+ nvl(ai.DME_INDENTQTY,0)+ nvl(ai.MITANIN,0))) as IndValue
from live_tenders a  
inner join masitems m on m.itemcode=a.Drug_Code
left outer join itemindent ai on ai.itemid=m.itemid and accyrsetid=546
left outer join v_itemrate v on v.itemid=m.itemid
)
group by schemeid
)  itm on itm.schemeid=s.schemeid 

 left outer join
 (
select count(distinct Drug_Code) as ItemAEDL,schemeid from live_tenders a  
inner join masitems m on m.itemcode=a.Drug_Code
where 1=1 and m.isedl2021='Y'
group by schemeid
)  itmE on itmE.schemeid=s.schemeid 

left outer join
 (
select count(enddt) as noext,schemeid from MASSCHEMESTATUSEXT a  
group by schemeid
)  ext on ext.schemeid=s.schemeid 

left outer join
 (
select max(enddt) as MaxDate,schemeid from MASSCHEMESTATUSEXT
group by schemeid
)  mxDT on mxDT.schemeid=s.schemeid

left outer join 
(
select count(supplierid) as cn,schemeid from masschemesstatusdetails c 
group by schemeid
) covA on covA.schemeid=s.schemeid
left outer join 
(
select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
where  flagcoa='Y' group by schemeid
) CovA4 on CovA4.schemeid=s.schemeid


left outer join 
(
select count(distinct sc.itemid) as cntEDL,schemeid from schemestatusdetailschild sc
inner join masitems m on m.itemid=sc.itemid
where  flagcoa='Y' and  m.isedl2021='Y' group by schemeid
) CovA4E on CovA4E.schemeid=s.schemeid

left outer join 
(
select count(distinct itemid) as cnt,schemeid from schemestatusdetailschild 
where  flagcob='Y'  group by schemeid
) CovB4 on CovB4.schemeid=s.schemeid

left outer join 
(
select count(supplierid) as cnB,schemeid from masschemesstatusdetails c where  iselibible_b='Y'
group by schemeid
) covAShort on covAShort.schemeid=s.schemeid

left outer join 
(

  select  schemeid,count(distinct itemid) pendingforACRej from
 (
 select m.itemid, sch.schemeid
 from live_tenders t
inner join masschemes sch on sch.schemeid= t.schemeid
inner join masitems m on m.itemcode= t.drug_code
inner join masitemcategories c on c.categoryid= m.categoryid
inner join masitemmaincategory mc on mc.MCID= c.MCID
inner join
(

select distinct  TID, BASICRATE from live_tender_price l
inner join massuppliers s on s.supplierid= l.supplierid
where 1=1  
and l.RANKID= 1 and l.isaccept is null
) l on l.tid=t.tid

left outer join
(
select distinct aci.itemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
where ac.status = 'C' and sysdate between ac.contractstartdate and ac.contractenddate
) rc on  rc.itemid = m.itemid
left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode = t.drug_code
group by m.itemid
) mt on mt.itemid = m.itemid
where m.ISFREEZ_ITPR is null and t.rejectdate is null 
and t.rejectdate is null  and (case when rc.itemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end)= 1
 "+ whCategoryId + @"    
) a group by schemeid

) prc on prc.schemeid=s.schemeid 

left outer join
(

select distinct SCHEMEID,'5' as accstatus
from
(

select l.SUPPLIERID, s.suppliername, m.itemcode, t.SCHEMEID, sch.schemename, mxtenderid, m.itemid, rc.contractitemid, mc.mcid,
mc.MCATEGORY,
case when m.isedl2021 = 'Y' then 1 else 0 end as EDL, case when m.isedl2021 = 'Y' then 0 else 1 end as NEDL
,case when rc.contractitemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end as Flag,l.fdate
from live_tender_price l
inner join live_tenders t on t.tid = l.tid
inner join massuppliers s on s.supplierid = l.supplierid
inner join masschemes sch on sch.schemeid = t.schemeid
inner join masitems m on m.itemcode = t.drug_code
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join
(
select ac.schemeid, aci.itemid, aci.contractitemid from aoccontractitems aci
inner join aoccontracts ac on ac.contractid = aci.contractid
) rc on rc.schemeid = t.SCHEMEID  and rc.itemid = m.itemid

left outer join
(
select max(t.SCHEMEID) as mxtenderid, m.itemid from live_tenders t
inner join masitems m on m.itemcode = t.drug_code
group by m.itemid

) mt on mt.itemid = m.itemid

where 1=1  "+ whCategoryId + @"     and  m.ISFREEZ_ITPR is null 
and l.isaccept = 'Y'
and rc.contractitemid is null  "+ whCategoryId + @"    
and t.rejectdate is null and (case when rc.contractitemid is null and mxtenderid > t.SCHEMEID then 0 else 1 end)= 1
and l.fdate > '1-Jan-2022'
)


)ac on ac.SCHEMEID=s.SCHEMEID


where 1=1  and sc.Status  in (1,2,3,7,4,8)    
 "+ whCategoryId1 + @"     
and nvl(ItemLive,0)>0 


)x 
left outer join
(

select tr.SCHEMEID,  tr.tenderremark,
to_char( tr.entrydate,'dd-MM-yyyy') as entrydate
,t.tenderstatus,tr.TSID 
from
TENDERSTATUSREMARK tr 
inner join TENDERSTATUSMASTER t  on t.tsid=tr.tsid
where tr.ISNEW='Y'  order by  TSID desc

)tsr on  tsr.schemeid=x.schemeid
where prc=1
order by startDt,statusid desc

)

 ";
            var myList = _context.TotalTenderDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;



        }


        [HttpGet("NoOfBidders")]
        public async Task<ActionResult<IEnumerable<NoOfBiddersDTO>>> NoOfBidders(string schemeId)
        {

            string whSchemeId = "";


            if (schemeId != "0")
            {
                whSchemeId = " and s.schemeid="+ schemeId + "  ";
            }


            string qry = @" select s.SCHEMEID, s.SCHEMECODE, s.SCHEMENAME,sp.supplierid,sp.suppliername,nvl(sp.CONTACTPERSON,'-') as CONTACTPERSON, sp.ADDRESS1 ||' '||sp.ADDRESS2 ||' '|| sp.ADDRESS3 as ADDRESS, sp.PHONE1, sp.PHONE2, sp.EMAIL
 
 ,itm.NoOFItems from  masschemes s 
inner join masschemesstatus sc on s.schemeid=sc.schemeid
inner join masitemcategories mc on mc.categoryid=sc.categoryid
 inner join masitemmaincategory mc1 on mc1.MCID = mc.MCID
 left outer join
 (
 
select s.schemeid, s.supplierid,count( distinct lp.tid) as NoOFItems from 
live_tenders lt
inner join  masschemesstatusdetails s  on s.schemeid= lt.schemeid
left outer join live_tender_price lp on   lp.SUPPLIERID=s.SUPPLIERID   and lp.TID=lt.TID

group by s.schemeid, s.supplierid

)  itm on itm.schemeid=s.schemeid 
left outer join massuppliers  sp on sp.supplierid=itm.supplierid
where 1=1 "+ whSchemeId + @" ";
            var myList = _context.NoOfBiddersDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("ConversationHodCgmsc")]
        public async Task<ActionResult<IEnumerable<ConversationHodCgmscDTO>>> ConversationHodCgmsc()
        {

           

            string qry = @" select t.SCHEMEID, s.SCHEMENAME ,ft.FACILITYTYPECODE as HOD,  t.letterno,to_char( t.letterdate,'dd-MM-yyyy') as letterdate,t.remarks,to_char( t.senddate,'dd-MM-yyyy') as senddate,t.entrydate,t.filename,
'https://dpdmis.in/' || replace(t.filepath,'~','') as filepath

,t.convid
,rp.RecvDate,rp.LetterNo as ReplyLetterNo,rp.LetterDT as ReplyLetterDT,rp.Remarks ReplyRemarks,rp.FileName as ReplyFileName,
'https://dpdmis.in/' || replace(rp.FilePath,'~','') as ReplyFilePath
from TBHODCONVERSATION t
inner join masschemes  s on s.SCHEMEID=t.SCHEMEID
inner join masfacilitytypes ft on ft.FACILITYTYPEID=t.hodid
left outer join 
(
select CONRID,CONVID,to_char( RecvDate,'dd-MM-yyy')  as RecvDate,LetterNo,to_char( LetterDT,'dd-MM-yyy') as LetterDT,Remarks,FileName,

FilePath,entryby,entrydate 
from TBHODCONVERSATIONREPLY
)rp on rp.CONVID=t.CONVID
where 1=1 order by  s.schemeid desc ";
            var myList = _context.ConversationHodCgmscDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("WhMangerSSODetail")]
        public async Task<ActionResult<IEnumerable<WhMangerSSODetailDTO>>> WhMangerSSODetail()
        {



            string qry = @" SELECT 
    TO_CHAR(w.WAREHOUSEID) AS WAREHOUSEID,
    w.WAREHOUSENAME,
    ADDRESS1 || ' ' || ADDRESS2 || ' ' || NVL(ADDRESS3, CITY) AS ADDRESS,
    TO_CHAR(am.AMID) AS AMID,
    am.AMNAME,
    am.MOB1 AS AMMOBILENO,
    TO_CHAR(so.SOID) AS SOID,
    so.SONAME,
    so.MOB1 AS SOMOBILENO,
    w.LATITUDE,
    W.LONGITUDE
FROM maswarehouses w
LEFT OUTER JOIN masamgr am ON am.AMID = w.AMID
LEFT OUTER JOIN masso so ON so.SOID = w.SOID; ";
            var myList = _context.WhMangerSSODetailDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("ToBeTender")]
        public async Task<ActionResult<IEnumerable<ToBeTenderDTO>>> ToBeTender(string mcid)
        {

            string whMcId = "";


            if (mcid != "0")
            {
                whMcId = " and mc.mcid= " + mcid + "   ";
            }


            string qry = @"  select distinct count(itemcode) cntItems,round(sum(AIValue)/10000000,2) as AIValue  from (

 select m.itemcode,m.itemname,m.strength1,m.unit,m.itemid
,case when m.isedl2021='Y' then 'Yes' else 'No' end as EDL
,lsch.lSchemeid,lsch.schemecode,lsch.status
, case when lsch.status=1 then '6:Live' else 
case when lsch.status=2  and SUBSTR(ts.ACTIONCODE,1,10)='Cover-A in' then '5:Cover A' else  
case when lsch.status=3 and SUBSTR(ts.ACTIONCODE,1,10)='Cover-B in' then '3:Cover B' else 
case when lsch.status=7 and SUBSTR(ts.ACTIONCODE,1,18)='Claim Objection in' then '4:Under Claim Objection' else 
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,11)='Accepted in' then  '1:Accepted' else
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,15)='Price Opened in' then  '2:Price Opened' else 
case when SUBSTR(ts.ACTIONCODE,1,14)='To be Retender' then '8:To be ReTender' else 
case when SUBSTR(ts.ACTIONCODE,1,10)='RC Blocked' then '7:RC Blocked,To be Tender' else 
'9:Fresh Tender'
end end end end end end end end as TStatus,action,ACTIONCODE,
case when skurate is not null then Round((nvl(dme_indentqty,0)+nvl(i.dhs_indentqty,0)+nvl(i.mitanin,0))*skurate,0) 
else 0 end as AIValue,
case when lsch.status=1 then ts.schemeid else 
case when lsch.status=2  and SUBSTR(ts.ACTIONCODE,1,10)='Cover-A in' then ts.schemeid  else  
case when lsch.status=3 and SUBSTR(ts.ACTIONCODE,1,10)='Cover-B in' then ts.schemeid  else 
case when lsch.status=7 and SUBSTR(ts.ACTIONCODE,1,18)='Claim Objection in' then ts.schemeid  else 
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,11)='Accepted in' then  ts.schemeid  else
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,15)='Price Opened in' then  ts.schemeid  else 
case when SUBSTR(ts.ACTIONCODE,1,14)='To be Retender' then ts.schemeid  else 
case when SUBSTR(ts.ACTIONCODE,1,10)='RC Blocked' then BSchemeid else 
NULL
end end end end end end end end as TenderRef
from itemindent i 
inner join masitems m on m.itemid=i.itemid 
inner join masitemcategories c on c.CATEGORYID=m.CATEGORYID
inner join masitemmaincategory mc on mc.mcid=c.mcid
left outer join 
(
select itemid,max(SKUFINALRATE) skurate from v_itemrate
group by itemid
) rt on rt.itemid=m.itemid
left outer join
(
select distinct r.itemid as ritemid from  v_rcvalid r
) r on r.ritemid=m.itemid

left outer join 
(
select schemeid as BSchemeid,itemid from v_rcvalidblocked
) rb on rb.itemid=m.itemid


left outer join 
   (
   select itemid,action,schemeid,ACTIONCODE from v_tenderstatusallnew e
   ) ts on ts.itemid=m.itemid

   left outer join 
   (
   select max(tid) as tid,drug_code from live_tenders l
   inner join masschemesstatus sc on sc.schemeid=l.schemeid
   where 1=1 and sc.nitdate>'01-Apr-2021' 
   group by drug_code
   ) lastT on lastT.drug_code=m.itemcode

   left outer join 
   (
  select tid,drug_code,l.schemeid as lSchemeid,s.schemecode,sc.status from live_tenders l
  inner join masschemes s on s.schemeid=l.schemeid
  inner join masschemesstatus sc on sc.schemeid=l.schemeid
   ) lsch on lsch.tid=lastT.tid

where (nvl(i.ISAIRETURN_DHS,'N')='N' and nvl(i.ISAIRETURN_DME,'N') ='N') and i.accyrsetid=(select accyrsetid from masaccyearsettings where sysdate between startdate and enddate) 
"+ whMcId + @"
and m.isfreez_itpr is null and ritemid is null 
 and   (nvl(dme_indentqty,0)+nvl(i.dhs_indentqty,0)+nvl(i.mitanin,0))>0 
and case when lsch.status=1 then '6:Live' else 
case when lsch.status=2  and SUBSTR(ts.ACTIONCODE,1,10)='Cover-A in' then '5:Cover A' else  
case when lsch.status=3 and SUBSTR(ts.ACTIONCODE,1,10)='Cover-B in' then '3:Cover B' else 
case when lsch.status=7 and SUBSTR(ts.ACTIONCODE,1,18)='Claim Objection in' then '4:Under Claim Objection' else 
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,11)='Accepted in' then  '1:Accepted' else
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,15)='Price Opened in' then  '2:Price Opened' else 
case when SUBSTR(ts.ACTIONCODE,1,14)='To be Retender' then '8:To be ReTender' else 
case when SUBSTR(ts.ACTIONCODE,1,10)='RC Blocked' then '7:RC Blocked,To be Tender' else 
'9:Fresh Tender'
end end end end end end end end = '8:To be ReTender' 
) ";
            var myList = _context.ToBeTenderDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("ToBeTenderDetail")]
        public async Task<ActionResult<IEnumerable<ToBeTenderDetailDTO>>> ToBeTenderDetail(string mcid)
        {

            string whMcid = "";


            if (mcid != "0")
            {
                whMcid = " and mc.MCID= " + mcid + "   ";
            }


            string qry = @"  select ITEMCODE, ITEMNAME, STRENGTH1 as STRENGTH, UNIT, EDL,
DHSIndnetQty,DHSAIValue,DMEIndentQty,DMEAIValue,
(DHSIndnetQty+DMEIndentQty) TotalIndentQty
,TotalAIValue,
SCHEMECODE,SCHEMENAME,  TENDERREF,lSchemeid from (

 select m.itemcode,m.itemname,m.strength1,m.unit,m.itemid
,case when m.isedl2021='Y' then 'Yes' else 'No' end as EDL
,lsch.lSchemeid,lsch.schemecode,lsch.status,lsch.SCHEMENAME
, case when lsch.status=1 then '6:Live' else 
case when lsch.status=2  and SUBSTR(ts.ACTIONCODE,1,10)='Cover-A in' then '5:Cover A' else  
case when lsch.status=3 and SUBSTR(ts.ACTIONCODE,1,10)='Cover-B in' then '3:Cover B' else 
case when lsch.status=7 and SUBSTR(ts.ACTIONCODE,1,18)='Claim Objection in' then '4:Under Claim Objection' else 
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,11)='Accepted in' then  '1:Accepted' else
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,15)='Price Opened in' then  '2:Price Opened' else 
case when SUBSTR(ts.ACTIONCODE,1,14)='To be Retender' then '8:To be ReTender' else 
case when SUBSTR(ts.ACTIONCODE,1,10)='RC Blocked' then '7:RC Blocked,To be Tender' else 
'9:Fresh Tender'
end end end end end end end end as TStatus,action,ACTIONCODE,

nvl(dme_indentqty,0) as DMEIndentQty,
(nvl(i.dhs_indentqty,0)+nvl(i.mitanin,0)) as DHSIndnetQty 
,case when skurate is not null then Round((nvl(i.dhs_indentqty,0)+nvl(i.mitanin,0))*skurate,0) else 0 end as DHSAIValue
,case when skurate is not null then Round((nvl(dme_indentqty,0))*skurate,0) else 0 end as DMEAIValue
,
case when skurate is not null then Round((nvl(dme_indentqty,0)+nvl(i.dhs_indentqty,0)+nvl(i.mitanin,0))*skurate,0) else 0 end as TotalAIValue



,
case when lsch.status=1 then ts.schemeid else 
case when lsch.status=2  and SUBSTR(ts.ACTIONCODE,1,10)='Cover-A in' then ts.schemeid  else  
case when lsch.status=3 and SUBSTR(ts.ACTIONCODE,1,10)='Cover-B in' then ts.schemeid  else 
case when lsch.status=7 and SUBSTR(ts.ACTIONCODE,1,18)='Claim Objection in' then ts.schemeid  else 
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,11)='Accepted in' then  ts.schemeid  else
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,15)='Price Opened in' then  ts.schemeid  else 
case when SUBSTR(ts.ACTIONCODE,1,14)='To be Retender' then ts.schemeid  else 
case when SUBSTR(ts.ACTIONCODE,1,10)='RC Blocked' then BSchemeid else 
NULL
end end end end end end end end as TenderRef
from itemindent i 
inner join masitems m on m.itemid=i.itemid 
inner join masitemcategories c on c.CATEGORYID=m.CATEGORYID
inner join masitemmaincategory mc on mc.mcid=c.mcid
left outer join 
(
select itemid,max(SKUFINALRATE) skurate from v_itemrate
group by itemid
) rt on rt.itemid=m.itemid
left outer join
(
select distinct r.itemid as ritemid from  v_rcvalid r
) r on r.ritemid=m.itemid

left outer join 
(
select schemeid as BSchemeid,itemid from v_rcvalidblocked
) rb on rb.itemid=m.itemid


left outer join 
   (
   select itemid,action,schemeid,ACTIONCODE from v_tenderstatusallnew e
   ) ts on ts.itemid=m.itemid

   left outer join 
   (
   select max(tid) as tid,drug_code from live_tenders l
   inner join masschemesstatus sc on sc.schemeid=l.schemeid
   where 1=1 and sc.nitdate>'01-Apr-2021' 
   group by drug_code
   ) lastT on lastT.drug_code=m.itemcode

   left outer join 
   (
  select tid,drug_code,l.schemeid as lSchemeid,s.schemecode,sc.status,SCHEMENAME from live_tenders l
  inner join masschemes s on s.schemeid=l.schemeid
  inner join masschemesstatus sc on sc.schemeid=l.schemeid
   ) lsch on lsch.tid=lastT.tid

where (nvl(i.ISAIRETURN_DHS,'N')='N' and nvl(i.ISAIRETURN_DME,'N') ='N') and i.accyrsetid=(select accyrsetid from masaccyearsettings where sysdate between startdate and enddate) 
" + whMcid + @"
and m.isfreez_itpr is null and ritemid is null 
 and   (nvl(dme_indentqty,0)+nvl(i.dhs_indentqty,0)+nvl(i.mitanin,0))>0 
and case when lsch.status=1 then '6:Live' else 
case when lsch.status=2  and SUBSTR(ts.ACTIONCODE,1,10)='Cover-A in' then '5:Cover A' else  
case when lsch.status=3 and SUBSTR(ts.ACTIONCODE,1,10)='Cover-B in' then '3:Cover B' else 
case when lsch.status=7 and SUBSTR(ts.ACTIONCODE,1,18)='Claim Objection in' then '4:Under Claim Objection' else 
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,11)='Accepted in' then  '1:Accepted' else
case when lsch.status=4 and SUBSTR(ts.ACTIONCODE,1,15)='Price Opened in' then  '2:Price Opened' else 
case when SUBSTR(ts.ACTIONCODE,1,14)='To be Retender' then '8:To be ReTender' else 
case when SUBSTR(ts.ACTIONCODE,1,10)='RC Blocked' then '7:RC Blocked,To be Tender' else 
'9:Fresh Tender'
end end end end end end end end = '8:To be ReTender' 

)x

where 1=1 -- and TOTALAIVALUE>100000
order by ITEMNAME ";
            var myList = _context.ToBeTenderDetailDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("SchemeReceived")]
        public async Task<ActionResult<IEnumerable<SchemeReceivedDTO>>> SchemeReceived(string schemeid)
        {

            string whSchemeId = "";


            if (schemeid != "0")
            {
                whSchemeId = " and s.schemeid= " + schemeid + "   ";
            }


            string qry = @" select t.SCHEMEID, s.SCHEMENAME ,ft.FACILITYTYPECODE,  t.letterno,to_char( t.letterdate,'dd-MM-yyyy') as letterdate,t.remarks,to_char( t.senddate,'dd-MM-yyyy') as senddate,t.entrydate,t.filename,
replace(t.filepath,'~','dpdmis.in') as filepath,t.convid 
,rp.CONRID, rp.RECVDATE, rp.RECVLETTERNO, rp.RECVLETTERDT, rp.RECVREMARK,rp.RECVFILENAME, replace(rp.RECVFILEPATH,'~','dpdmis.in')  as RECVFILEPATH , rp.RECVENTRYDATE
from TBHODCONVERSATION t
inner join masschemes  s on s.SCHEMEID=t.SCHEMEID
inner join masfacilitytypes ft on ft.FACILITYTYPEID=t.hodid
left outer join 
(
select CONRID,CONVID,to_char( RecvDate,'dd-MM-yyy')  as RecvDate,LetterNo as RecvLetterNo
,to_char( LetterDT,'dd-MM-yyy') as RecvLetterDT,Remarks as RecvRemark,FileName as RecvFileName,FilePath as RecvFilePath,entryby,to_char(entrydate,'dd-MM-yyyy')   as Recventrydate
from TBHODCONVERSATIONREPLY  order by entrydate desc
)rp on rp.convid=t.convid
where 1=1  "+ whSchemeId + @" order by  convid desc ";
            var myList = _context.SchemeReceivedDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("SchemeTenderStatus")]
        public async Task<ActionResult<IEnumerable<SchemeTenderStatusDTO>>> SchemeTenderStatus(string schemeid)
        {

            string whSchemeId = "";


            if (schemeid != "0")
            {
                whSchemeId = " and s.schemeid= " + schemeid + "   ";
            }


            string qry = @" select s.SCHEMEID, s.SCHEMENAME  ,t.tenderstatus, tr.tenderremark,
                            to_char( tr.entrydate,'dd-MM-yyyy') as entrydate
                           ,tr.TSID 
                            from
                            TENDERSTATUSREMARK tr 
                            inner join TENDERSTATUSMASTER t  on t.tsid=tr.tsid
                            inner join masschemes  s on s.SCHEMEID=tr.SCHEMEID
                            where 1=1 "+ whSchemeId + @" order by  TSID desc ";
            var myList = _context.SchemeTenderStatusDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


    }



}
