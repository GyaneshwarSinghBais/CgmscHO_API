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
using CgmscHO_API.HODTO;
using CgmscHO_API.Utility;
using Newtonsoft.Json.Linq;
using CgmscHO_API.DTO;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Net;
//using Broadline.Controls;
//using CgmscHO_API.Utility;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HOController : ControllerBase
    {

        private readonly OraDbContext _context;

        public HOController(OraDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            //GenFunctions.Users.WardloginDetails(model.WardId, model.Password, out string message);
            loginDetails(model.emailid, model.pwd, out string message, out UsruserModel user);

            if (message == "Successfully Login")
            {
                //return Ok(message);
                return Ok(new { Message = message, UserInfo = user });
            }

            return BadRequest("Invalid credentials.");
        }

        private bool loginDetails(string email, string password, out string message, out UsruserModel user)
        {
            message = null;

            //var result = _context.MasFacilityWards
            //    .FirstOrDefault(w => w.wardid == wardId);

            var result = _context.Usruser
               .FirstOrDefault(u => u.EMAILID == email);

            user = result;

            if (result == null)
            {
                message = "Invalid ID.";
                return false;
            }


            // Perform password verification
            string salthash = result.PWD;
            string mStart = "salt{";
            string mMid = "}hash{";
            string mEnd = "}";
            string mSalt = salthash.Substring(salthash.IndexOf(mStart) + mStart.Length, salthash.IndexOf(mMid) - (salthash.IndexOf(mStart) + mStart.Length));
            string mHash = salthash.Substring(salthash.IndexOf(mMid) + mMid.Length, salthash.LastIndexOf(mEnd) - (salthash.IndexOf(mMid) + mMid.Length));


            Broadline.Common.SecUtils.SaltedHash ver = Broadline.Common.SecUtils.SaltedHash.Create(mSalt, mHash);
            bool isValid = ver.Verify(password);

            // bool isValid = SaltedHashUtils.VerifySaltedHash(salthash, password);

            if (!isValid)
            {
                message = "The email or password you have entered is incorrect.";
                return false;
            }
            message = "Successfully Login";
            return true;
        }


        //HO PO Related
        [HttpGet("HODYearWisePO")]
        public async Task<ActionResult<IEnumerable<HODYearWisePODTO>>> HODYearWisePO(string yearid, string mcatid, string itemid)
        {
            string whyearid = "";
            if (yearid != "0")
            {
                whyearid = " and  op.ACCYRSETID=" + yearid;
            }
            string whMCatID = "";
            if (mcatid != "0")
            {
                whMCatID = " and  mc.mcid =" + mcatid;
            }
            string whitemid = "";
            if (itemid != "0")
            {
                whitemid = " and mi.itemid =" + itemid;
            }


            string qry = @" select mcid, MCATEGORY, count(distinct itemid) noofitems,
round(sum(dhspovalue) / 10000000, 2) dhspovalue,
round(sum(dhsrvalue) / 10000000, 2) dhsrvalue,
round(sum(dmepovalue) / 10000000, 2) dmepovalue,
round(sum(dmervalue) / 10000000, 2) dmervalue,
round(sum(dhspovalue) / 10000000, 2) + round(sum(dmepovalue) / 10000000, 2) totalpovalue, 
round(sum(dhsrvalue) / 10000000, 2) + round(sum(dmervalue) / 10000000, 2) totalrvalue
,ACCYRSETID,ACCYEAR,to_char(mcid)||to_char(ACCYRSETID) as ID
from(
select mc.mcid, mc.MCATEGORY, op.ponoid,
                            oi.absqty as orderedqty,
                            nvl(sum(od.dhsqty), 0) dhspoqty,
                            nvl(sum(od.dhsqty), 0) * c.finalrategst dhspovalue,
                            nvl(r1.dhsrqty, 0) dhsrqty, nvl(r1.dhsrqty, 0) * c.finalrategst dhsrvalue,
                            nvl(sum(od.dmeqty), 0) dmepoqty,
                            nvl(sum(od.dmeqty), 0) * c.finalrategst dmepovalue,
                            nvl(r2.dmerqty, 0) dmerqty, nvl(r2.dmerqty, 0) * c.finalrategst dmervalue,
                            case when nvl(sum(od.dhsqty), 0) > 0 then 1 else 0 end dhspocnt,
                            case when nvl(sum(od.dmeqty), 0) > 0 then 1 else 0 end dmepocnt
                            , op.ACCYRSETID,mi.itemid,y.SHACCYEAR as ACCYEAR
                            from masItems MI
                                              inner join soOrderedItems OI on(OI.ItemID = MI.ItemID)
                                              inner join soorderplaced op on(op.ponoid = oi.ponoid and op.status not in ('OC', 'WA1', 'I'))
 inner join masaccyearsettings y on y.ACCYRSETID=op.ACCYRSETID                                             
inner join soorderdistribution od on(od.orderitemid = oi.orderitemid)
                                              inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                                              inner join masitemcategories ic on ic.categoryid = mi.categoryid
                                              inner join masitemmaincategory mc on mc.MCID = ic.MCID
                                              left outer join
                                                (
                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty), 0) as dhsrqty
                                                from tbreceipts t
                                                inner
                                                join tbreceiptitems i on (i.receiptid = t.receiptid)
                                                inner
                                                join tbreceiptbatches tb on (i.receiptitemid = tb.receiptitemid)
                                                where T.Status = 'C' and  T.receipttype = 'NO'
                                                and t.ponoid in (select ponoid from soorderplaced where deptid in (367, 371))
                                                and t.receiptid not in (select tr.receiptid
                                                                           from tbindents t
                                                                           inner join tbindentitems i on(i.indentid = t.indentid)
                                                                           inner join tboutwards o on(o.indentitemid = i.indentitemid)
                                                                           inner join tbreceiptbatches tb on(tb.inwno = o.inwno)
                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                                                           where t.status = 'C' and t.issuetype in ('RS') )
                                                group by I.ItemID, T.PoNoID
                                                ) r1 on(r1.itemid = oi.itemid and r1.ponoid = op.ponoid)
                                                left outer join
                                                (
                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty), 0) as dmerqty
                                                from tbreceipts t
                                                inner
                                                join tbreceiptitems i on (i.receiptid = t.receiptid)
                                                inner
                                                join tbreceiptbatches tb on (i.receiptitemid = tb.receiptitemid)
                                                where T.Status = 'C' and  T.receipttype = 'NO'
                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 364)
                                                and t.receiptid not in (select tr.receiptid
                                                                           from tbindents t
                                                                           inner join tbindentitems i on(i.indentid = t.indentid)
                                                                           inner join tboutwards o on(o.indentitemid = i.indentitemid)
                                                                           inner join tbreceiptbatches tb on(tb.inwno = o.inwno)
                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                                                           where t.status = 'C' and t.issuetype in ('RS') )
                                                group by I.ItemID, T.PoNoID
                                                ) r2 on(r2.itemid = oi.itemid and r2.ponoid = op.ponoid)
                                              where 1 = 1 " + whitemid + @"  " + whMCatID + @" and op.ACCYRSETID >= 539 " + whyearid + @"
                                              group by mc.mcid,mc.MCATEGORY,op.ponoid,
                                                  oi.absqty,c.finalrategst,r1.dhsrqty, r2.dmerqty ,op.ACCYRSETID,mi.itemid,y.SHACCYEAR
                                              ) group by mcid,MCATEGORY,ACCYRSETID,ACCYEAR order by mcid
";
            var myList = _context.HODYearWisePODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }
        [HttpGet("HODYearWiseIssuance")]
        public async Task<ActionResult<IEnumerable<HODIssueDTO>>> HODYearWiseIssuance(string yearid, string mcatid, string hodid, string itemid)
        {
            string whyearid = "";
            if (yearid != "0")
            {
                whyearid = " and  y.ACCYRSETID=" + yearid;
            }
            string whMCatID = "";
            if (mcatid != "0")
            {
                whMCatID = " and  mc.mcid =" + mcatid;
            }
            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and t.hodid =" + hodid;
            }
            string whitemid = "";
            if (itemid != "0")
            {
                whitemid = " and m.itemid =" + itemid;
            }
            //string whdistid = "";
            //if (disid != "0")
            //{
            //    whdistid = " and f.districtid="+ disid;
            //}

            string qry = @" select y.SHACCYEAR as ACCYEAR, MCATEGORY,Round(sum(finalvalue)/10000000,2) issuevalue,count(distinct itemid) as noofitems,mcid
    ,y.ACCYRSETID,
    to_char(mcid)||to_char(y.ACCYRSETID) as ID
    from (                       
        select 
        (
        select ACCYEAR from masaccyearsettings where tb.indentdate between startdate and enddate) as Year
        ,f.facilityid,
        tb.indentid,tb.indentdate,tbi.itemid,tbi.indentitemid,mc.mcid,mc.MCATEGORY,m.itemcode,
        rb.batchno iss_batchno,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty ,sb.acibasicratenew skurate,acicst,acgstpvalue,
        acivat,case when rb.ponoid=1111 then 0 else  sb.acisingleunitprice end  finalrate,
        (sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * sb.acibasicratenew) as skuvalue, 
        (sum(tbo.issueqty) * sb.acisingleunitprice) as finalvalue
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid 
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
         left outer join 
                (
                select s.ponoid,tbo.inwno,si.basicrate, a.indentdate,c.batchno,tbo.indentitemid
               ,coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.exciseduty,0))/100) ),2),si.singleunitprice) as supp 
               ,case when  a.indentdate >= '01-Jul-2017' then ( case when  aci.basicratenew is null then aci.basicrate else   aci.basicratenew  end) else aci.basicrate end acibasicratenew
               ,case when  a.indentDate >= '01-Jul-2017' then  (case when aci.gstflag='Y' then aci.finalrategst else coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.ExciseDuty,0))/100) ),2),si.singleunitprice)  end) else coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.ExciseDuty,0))/100) ),2),si.singleunitprice)  end   ACIsingleunitprice
               ,case when aci.cstvat ='CST' then aci.percentvalue  else 0 end ACICST,  
               case when aci.cstvat ='VAT' then aci.percentvalue  else 0 end ACIVAT,  
               case when  a.indentDate >= '01-Jul-2017' then  (case when aci.gstflag='Y' then nvl(aci.percentvaluegst,0) else 0 end) else 0 end  ACGSTPvalue   from tbindents a
               inner join tbindentitems b on a.indentid = b.indentid
               inner join tboutwards tbo on tbo.indentitemid=b.indentitemid
               inner join tbreceiptbatches c on  tbo.inwno = c.inwno 
               inner join tbreceiptitems ri on ri.receiptitemid=c.receiptitemid
               inner join tbreceipts r on r.receiptid=ri.receiptid
               left outer join soorderplaced s on s.ponoid=c.ponoid
               inner join soordereditems si on  si.ponoid=s.ponoid and si.itemid=b.itemid   
               INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
               inner join aoccontracts ac on ac.contractid=aci.contractid
               where a.Status='C' and a.notindpdmis is null 
               and b.notindpdmis is null and c.notindpdmis is null 
               and tbo.notindpdmis is null 
               group by s.ponoid,tbo.inwno,si.basicrate,a.indentDate,tbo.indentitemid,
               c.batchno,si.singleunitprice,si.percentvalue,si.ExciseDuty,aci.basicratenew,aci.basicrate,aci.percentvalue,aci.percentvaluegst,aci.cstvat
               ,r.receiptdate,aci.gstflag,aci.singleunitprice,aci.singleunitprice,aci.finalrategst
                 )  sb on sb.inwno=rb.inwno and sb.indentitemid=tbo.indentitemid
        where 1=1   " + whitemid + @" and tb.status = 'C' " + whMCatID + @" and tb.issuetype='NO' " + whhodid + @"  
        and tb.indentdate between '01-APR-19' and SYSDATE

        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        group by tb.indentid,tb.indentdate,tbi.itemid,tbi.indentitemid,mc.mcid,mc.MCATEGORY,m.itemcode,rb.batchno,
        f.facilityid  ,sb.acisingleunitprice,sb.acibasicratenew,tb.indentdate,rb.ponoid ,acicst,acgstpvalue,acivat
        ) i 
      inner join masaccyearsettings y on y.ACCYEAR=i.Year     
      where 1=1 " + whyearid + @"
        group by mcid,MCATEGORY,y.SHACCYEAR,year,y.ACCYRSETID having sum(finalvalue) > 0     
         order by y.ACCYRSETID desc";

            var myList = _context.HODIssueDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("HODYearWiseIssuanceAng")]
        public async Task<ActionResult<IEnumerable<HODIssueDTO>>> HODYearWiseIssuanceAng(string yearid, string mcatid, string hodid, string itemid, string disid)
        {
            string whyearid = "";
            if (yearid != "0")
            {
                whyearid = " and  y.ACCYRSETID=" + yearid;
            }
            string whMCatID = "";
            if (mcatid != "0")
            {
                whMCatID = " and  mc.mcid =" + mcatid;
            }
            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and t.hodid =" + hodid;
            }
            string whitemid = "";
            if (itemid != "0")
            {
                whitemid = " and m.itemid =" + itemid;
            }
            string whdistid = "";
            if (disid != "0")
            {
                whdistid = " and f.districtid=" + disid;
            }

            string qry = @" select y.SHACCYEAR as ACCYEAR, MCATEGORY,Round(sum(finalvalue)/10000000,2) issuevalue,count(distinct itemid) as noofitems,mcid
    ,y.ACCYRSETID,
    to_char(mcid)||to_char(y.ACCYRSETID) as ID
    from (                       
        select 
        (
        select ACCYEAR from masaccyearsettings where tb.indentdate between startdate and enddate) as Year
        ,f.facilityid,
        tb.indentid,tb.indentdate,tbi.itemid,tbi.indentitemid,mc.mcid,mc.MCATEGORY,m.itemcode,
        rb.batchno iss_batchno,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty ,sb.acibasicratenew skurate,acicst,acgstpvalue,
        acivat,case when rb.ponoid=1111 then 0 else  sb.acisingleunitprice end  finalrate,
        (sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * sb.acibasicratenew) as skuvalue, 
        (sum(tbo.issueqty) * sb.acisingleunitprice) as finalvalue
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid " + whdistid + @"
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
         left outer join 
                (
                select s.ponoid,tbo.inwno,si.basicrate, a.indentdate,c.batchno,tbo.indentitemid
               ,coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.exciseduty,0))/100) ),2),si.singleunitprice) as supp 
               ,case when  a.indentdate >= '01-Jul-2017' then ( case when  aci.basicratenew is null then aci.basicrate else   aci.basicratenew  end) else aci.basicrate end acibasicratenew
               ,case when  a.indentDate >= '01-Jul-2017' then  (case when aci.gstflag='Y' then aci.finalrategst else coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.ExciseDuty,0))/100) ),2),si.singleunitprice)  end) else coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.ExciseDuty,0))/100) ),2),si.singleunitprice)  end   ACIsingleunitprice
               ,case when aci.cstvat ='CST' then aci.percentvalue  else 0 end ACICST,  
               case when aci.cstvat ='VAT' then aci.percentvalue  else 0 end ACIVAT,  
               case when  a.indentDate >= '01-Jul-2017' then  (case when aci.gstflag='Y' then nvl(aci.percentvaluegst,0) else 0 end) else 0 end  ACGSTPvalue   from tbindents a
               inner join tbindentitems b on a.indentid = b.indentid
               inner join tboutwards tbo on tbo.indentitemid=b.indentitemid
               inner join tbreceiptbatches c on  tbo.inwno = c.inwno 
               inner join tbreceiptitems ri on ri.receiptitemid=c.receiptitemid
               inner join tbreceipts r on r.receiptid=ri.receiptid
               left outer join soorderplaced s on s.ponoid=c.ponoid
               inner join soordereditems si on  si.ponoid=s.ponoid and si.itemid=b.itemid   
               INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
               inner join aoccontracts ac on ac.contractid=aci.contractid
               where a.Status='C' and a.notindpdmis is null 
               and b.notindpdmis is null and c.notindpdmis is null 
               and tbo.notindpdmis is null 
               group by s.ponoid,tbo.inwno,si.basicrate,a.indentDate,tbo.indentitemid,
               c.batchno,si.singleunitprice,si.percentvalue,si.ExciseDuty,aci.basicratenew,aci.basicrate,aci.percentvalue,aci.percentvaluegst,aci.cstvat
               ,r.receiptdate,aci.gstflag,aci.singleunitprice,aci.singleunitprice,aci.finalrategst
                 )  sb on sb.inwno=rb.inwno and sb.indentitemid=tbo.indentitemid
        where 1=1  " + whdistid + @" " + whitemid + @" and tb.status = 'C' " + whMCatID + @" and tb.issuetype='NO' " + whhodid + @"  
        and tb.indentdate between '01-APR-19' and SYSDATE

        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        group by tb.indentid,tb.indentdate,tbi.itemid,tbi.indentitemid,mc.mcid,mc.MCATEGORY,m.itemcode,rb.batchno,
        f.facilityid  ,sb.acisingleunitprice,sb.acibasicratenew,tb.indentdate,rb.ponoid ,acicst,acgstpvalue,acivat
        ) i 
      inner join masaccyearsettings y on y.ACCYEAR=i.Year     
      where 1=1 " + whyearid + @"
        group by mcid,MCATEGORY,y.SHACCYEAR,year,y.ACCYRSETID having sum(finalvalue) > 0     
         order by y.ACCYRSETID desc";

            var myList = _context.HODIssueDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("HODYearWiseIssuanceSummary")]
        
        public async Task<ActionResult<IEnumerable<HODIssueSummaryDTO>>> HODYearWiseIssuanceSummary(string mcatid, string hodid)
        {
            FacOperations f = new FacOperations(_context);

            string yeariddata = f.getACCYRSETID();

          
             string whyearid = yeariddata;
            
            string whMCatID = "";
            if (mcatid != "0")
            {
                whMCatID = " and  mc.mcid =" + mcatid;
            }
            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and t.hodid =" + hodid;
            }
            
              
            

            string qry = @" select  to_char(t.mcid)||to_char(y.ACCYRSETID) as ID, y.ACCYRSETID,SHACCYEAR,mc.mcategory,t.mcid,t.TOTALISSUEITEMS,TOTALISSUEVALUE,DHSISSUEVALUE,DHSISSUEITEMS,DMEISSUEVALUE,DMEISSUEITEMS,AYIssueval,AYIssueitems from masaccyearsettings y

left outer join 
(
select TOTALISSUEITEMS,TOTALISSUEVALUE, ACCYRSETID,mcid,case when mcid =4 then 0 else DHSISSUEVALUE end DHSISSUEVALUE ,
case when mcid =4 then 0 else DHSISSUEITEMS end DHSISSUEITEMS ,DMEISSUEVALUE,DMEISSUEITEMS, case when mcid =4 then DHSISSUEVALUE else 0 end as AYIssueval,
case when mcid =4 then DHSISSUEITEMS else 0 end as AYIssueitems   from MasYearDirectorateTransaction
) t on t.ACCYRSETID=y.ACCYRSETID
inner join masitemmaincategory mc on mc.mcid=t.mcid
where 1=1 and y.ACCYRSETID<"+ whyearid + @" "+ whMCatID + @"
union all
select to_char(mcid)||to_char(545) as ID, "+ yeariddata + @" as ACCYRSETID,(select SHACCYEAR from masaccyearsettings  where ACCYRSETID="+ whyearid + @") as SHACCYEAR,mcategory,mcid,count(distinct itemid) as TOTALISSUEITEMS,Round(sum(TotalVal)/10000000,2) as TOTALISSUEVALUE,Round(sum(DHSValue)/10000000,2) DHSISSUEVALUE,
  sum(DHSItem) as DHSISSUEITEMS,Round(sum(DMEVal)/10000000,2) as DMEISSUEVALUE, sum(DMEItem) as DMEISSUEITEMS
 ,Round(sum(AYUSHVal)/10000000,2) AYIssueval,  sum(AYUSHItem) as AYIssueitems
  from 
  (
  select mcid,mcategory,itemid,hodid,
  case when hodid=2 and sum(DHSQTY) >0 then 1 else 0 end DHSItem,  case when  hodid=3 and sum(DMEQTY)>0 then 1 else 0 end DMEItem
  ,case when  hodid=7 and sum(AYUSHQTY)>0 then 1 else 0 end AYUSHItem
  ,sum(TotalVal) as TotalVal,sum(DHSValue) as DHSValue,sum(DMEVal) as DMEVal,sum(AYUSHVal) as AYUSHVal

  from 
  (
  select   t.hodid,mc.mcid,mc.mcategory, m.itemid,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty,
  case when  t.hodid=2 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end DHSQTY,  
    case when  t.hodid=3 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end DMEQTY,  
      case when  t.hodid=7 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end AYUSHQTY,     
     case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end SKURate
      ,round(sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) TotalVal
        ,case when  t.hodid=2 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*(case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end) else 0 end DHSValue,

case when  t.hodid=3 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*(case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end) else 0 end DMEVal,
      case when  t.hodid=7 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*(case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end) else 0 end AYUSHVal
  from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
 inner join tbreceipts r on r.receiptid=ri.receiptid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
           inner join soordereditems si on  si.ponoid=rb.ponoid and si.itemid=rb.itemid   
    INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
       where 1=1  and tb.status = 'C'  " + whMCatID + @"
       and 
       tb.issuetype='NO' "+ whhodid + @"
           and tb.indentdate between (select STARTDATE from masaccyearsettings  where ACCYRSETID="+ yeariddata + @") and  (select ENDDATE from masaccyearsettings  where ACCYRSETID="+ yeariddata + @")  
        group by  mc.mcid,t.hodid,m.itemid,aci.finalrategst,si.singleunitprice,mc.mcategory
        ) group by mcid,mcategory,itemid,hodid
)
        group by mcid,mcategory
order by ACCYRSETID ";

            var myList = _context.GetHODIssueSummaryDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }





        [HttpGet("HODPOYear_AgAI")]


        public async Task<ActionResult<IEnumerable<HODYearWisePODTONew>>> HODPOYear_AgAI(string mcatid, string hodid,string Isall,string IsagainstAI)
        {

            FacOperations f = new FacOperations(_context);

            string yeariddata = f.getACCYRSETID();


            string whyearid = yeariddata;

            string whMCatID = "";
            if (mcatid != "0")
            {
                whMCatID = " and  mc.mcid =" + mcatid;
            }
            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and t.hodid =" + hodid;
            }
            string whIsall = " and y.ACCYRSETID>537 ";
            if (Isall == "Y")
            {
                whIsall = "";
            }
            string qry = "";
            if (IsagainstAI == "Y")
            {
                qry = @" select to_char(AIFINYEAR) as ID,AIFINYEAR as ACCYRSETID,(select SHACCYEAR from masaccyearsettings  where ACCYRSETID=AIFINYEAR) as SHACCYEAR,count(distinct dhsitem) DHSPOITEMS,
round(sum(dhspovalue)/10000000,2) DHSPOVALUE,
count(distinct dmeitem) DMEPOITEMS,
round(sum(dmepovalue)/10000000,2) DMEPOVALUE,
count(distinct itemcode) TOTALPOITEMS,
round(sum(dhspovalue)/10000000,2)+round(sum(dmepovalue)/10000000,2) TOTALPOVALUE, 
round(sum(dhsrvalue)/10000000,2) DHSRECVALUE,
round(sum(dmervalue)/10000000,2) DMERECVALUE,
round(sum(nvl(dhsrvalue,0))/10000000,2)+round(sum(nvl(dmervalue,0))/10000000,2) TOTALRECVALUE
from (
select mc.mcid,mc.MCATEGORY,mi.itemcode,itemname,mi.unit,op.pono pono,op.ponoid,to_char( op.soissuedate,'dd/mm/yyyy') podate,
                            oi.absqty as orderedqty, 
                            nvl(sum(od.dhsqty),0) dhspoqty,
                            nvl(sum(od.dhsqty),0)*c.finalrategst dhspovalue,
                            nvl(r1.dhsrqty,0) dhsrqty,nvl(r1.dhsrqty,0)*c.finalrategst dhsrvalue,
                            nvl(sum(od.dmeqty),0) dmepoqty,
                            nvl(sum(od.dmeqty),0)*c.finalrategst dmepovalue,
                            nvl(r2.dmerqty,0) dmerqty ,nvl(r2.dmerqty,0)*c.finalrategst dmervalue,
                            case when nvl(sum(od.dhsqty),0) > 0 then 1 else 0 end dhspocnt,
                            case when nvl(sum(od.dmeqty),0) > 0 then 1 else 0 end dmepocnt,
                            case when nvl(sum(od.dhsqty),0) > 0 then mi.itemid else null end dhsitem,
                            case when nvl(sum(od.dmeqty),0) > 0 then mi.itemid else null end dmeitem,
                            op.AIFINYEAR
                            from masItems MI
                                              inner join soOrderedItems OI on (OI.ItemID = MI.ItemID) 
                                              inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' )) 
                                              inner join soorderdistribution od on (od.orderitemid = oi.orderitemid) 
                                              inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                                              inner join masitemcategories ic on ic.categoryid = mi.categoryid
                                              inner join masitemmaincategory mc on mc.MCID=ic.MCID
                                              left outer join  
                                                (
                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  
                                                from tbreceipts t 
                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
                                                where T.Status = 'C' and  T.receipttype = 'NO' 
                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 367)
                                                and t.receiptid not in (select tr.receiptid
                                                                           from tbindents t  
                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                                                           where t.status = 'C' and t.issuetype in ('RS') )
                                                group by I.ItemID, T.PoNoID                       
                                                ) r1 on (r1.itemid =oi.itemid and r1.ponoid =op.ponoid) 
                                                left outer join  
                                                (
                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dmerqty                  
                                                from tbreceipts t 
                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
                                                where T.Status = 'C' and  T.receipttype = 'NO'
                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 364)
                                                and t.receiptid not in (select tr.receiptid
                                                                           from tbindents t  
                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                                                           where t.status = 'C' and t.issuetype in ('RS') )
                                                group by I.ItemID, T.PoNoID                       
                                                ) r2 on (r2.itemid =oi.itemid and r2.ponoid =op.ponoid) 
                                              where 1=1 and op.AIFINYEAR>=542   " + whMCatID + @"
                                              group by mc.mcid,mc.MCATEGORY,op.pono,op.ponoid,op.soissuedate,mi.itemcode,mi.itemid,itemname,
                                              oi.absqty,mi.unit,c.finalrategst,r1.dhsrqty, r2.dmerqty ,op.AIFINYEAR
                                              ) group by AIFINYEAR order by AIFINYEAR  ";
            }
            else
            {
                qry = @"    select ID,ACCYRSETID,SHACCYEAR,sum(DHSPOITEMS) as DHSPOITEMS,sum(DHSPOVALUE) as DHSPOVALUE,sum(DMEPOITEMS) as DMEPOITEMS,sum(DMEPOVALUE) as DMEPOVALUE,sum(TOTALPOITEMS) as TOTALPOITEMS,sum(TOTALPOVALUE) as TOTALPOVALUE
  ,sum(DHSRECVALUE) as DHSRECVALUE,sum(DMERECVALUE) as DMERECVALUE,sum(TOTALRECVALUE) as TOTALRECVALUE
  from 
  (
  select  to_char(y.ACCYRSETID) as ID, y.ACCYRSETID,y.SHACCYEAR 
,DHSPOITEMS,DHSPOVALUE,DMEPOITEMS,DMEPOVALUE,TOTALPOITEMS,TOTALPOVALUE,DHSRECVALUE,DMERECVALUE,TOTALRECVALUE
from masaccyearsettings y
left outer join 
(
select  t.ACCYRSETID,t.mcid, DHSPOVALUE , DHSPOITEMS 
,DMEPOITEMS,DMEPOVALUE, 
TOTALPOITEMS,TOTALPOVALUE,DHSRECVALUE,DMERECVALUE,TOTALRECVALUE

from MasYearDirectorateTransaction t
) t on t.ACCYRSETID=y.ACCYRSETID
inner join masitemmaincategory mc on mc.mcid=t.mcid
where 1=1 and y.ACCYRSETID <" + whyearid + @" " + whIsall + @"  " + whMCatID + @"
) group by ID,ACCYRSETID,SHACCYEAR

union all

select to_char(" + whyearid + @") as ID, " + whyearid + @" as ACCYRSETID,SHACCYEAR,count(distinct dhsitem) DHSPOITEMS,
round(sum(dhspovalue)/10000000,2) DHSPOVALUE,
count(distinct dmeitem) DMEPOITEMS,
round(sum(dmepovalue)/10000000,2) DMEPOVALUE,
count(distinct itemcode) TOTALPOITEMS,
round(sum(dhspovalue)/10000000,2)+round(sum(dmepovalue)/10000000,2) TOTALPOVALUE, 
round(sum(dhsrvalue)/10000000,2) DHSRECVALUE,
round(sum(dmervalue)/10000000,2) DMERECVALUE,
round(sum(nvl(dhsrvalue,0))/10000000,2)+round(sum(nvl(dmervalue,0))/10000000,2) TOTALRECVALUE
from (
select (select SHACCYEAR from masaccyearsettings  where ACCYRSETID=545) as SHACCYEAR,mc.mcid,mc.MCATEGORY,mi.itemcode,itemname,mi.unit,op.pono pono,op.ponoid,to_char( op.soissuedate,'dd/mm/yyyy') podate,
                            oi.absqty as orderedqty, 
                            nvl(sum(od.dhsqty),0) dhspoqty,
                            nvl(sum(od.dhsqty),0)*c.finalrategst dhspovalue,
                            nvl(r1.dhsrqty,0) dhsrqty,nvl(r1.dhsrqty,0)*c.finalrategst dhsrvalue,
                            nvl(sum(od.dmeqty),0) dmepoqty,
                            nvl(sum(od.dmeqty),0)*c.finalrategst dmepovalue,
                            nvl(r2.dmerqty,0) dmerqty ,nvl(r2.dmerqty,0)*c.finalrategst dmervalue,
                            case when nvl(sum(od.dhsqty),0) > 0 then 1 else 0 end dhspocnt,
                            case when nvl(sum(od.dmeqty),0) > 0 then 1 else 0 end dmepocnt,
                            case when nvl(sum(od.dhsqty),0) > 0 then mi.itemid else null end dhsitem,
                            case when nvl(sum(od.dmeqty),0) > 0 then mi.itemid else null end dmeitem
                            from masItems MI
                                              inner join soOrderedItems OI on (OI.ItemID = MI.ItemID) 
                                              inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' )) 
                                              inner join soorderdistribution od on (od.orderitemid = oi.orderitemid) 
                                              inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                                              inner join masitemcategories ic on ic.categoryid = mi.categoryid
                                              inner join masitemmaincategory mc on mc.MCID=ic.MCID
                                              left outer join  
                                                (
                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  
                                                from tbreceipts t 
                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
                                                where T.Status = 'C' and  T.receipttype = 'NO' 
                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 367)
                                                and t.receiptid not in (select tr.receiptid
                                                                           from tbindents t  
                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                                                           where t.status = 'C' and t.issuetype in ('RS') )
                                                group by I.ItemID, T.PoNoID                       
                                                ) r1 on (r1.itemid =oi.itemid and r1.ponoid =op.ponoid) 
                                                left outer join  
                                                (
                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dmerqty                  
                                                from tbreceipts t 
                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
                                                where T.Status = 'C' and  T.receipttype = 'NO'
                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 364)
                                                and t.receiptid not in (select tr.receiptid
                                                                           from tbindents t  
                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                                                           where t.status = 'C' and t.issuetype in ('RS') )
                                                group by I.ItemID, T.PoNoID                       
                                                ) r2 on (r2.itemid =oi.itemid and r2.ponoid =op.ponoid) 
                                              where 1=1 and op.soissuedate between (select STARTDATE from masaccyearsettings  where ACCYRSETID=" + whyearid + @") and 
                                              (select ENDDATE from masaccyearsettings  where ACCYRSETID=" + whyearid + @")  " + whMCatID + @"
                                              group by mc.mcid,mc.MCATEGORY,op.pono,op.ponoid,op.soissuedate,mi.itemcode,mi.itemid,itemname,
                                              oi.absqty,mi.unit,c.finalrategst,r1.dhsrqty, r2.dmerqty 
                                              ) group by SHACCYEAR order by ACCYRSETID ";
            }

            var myList = _context.HODYearWisePODTONewDbSet
       .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



            [HttpGet("HODAIvsIssue")]

        public async Task<ActionResult<IEnumerable<HODAIvsIssueDTO>>> HODAIvsIssue(string yearid,string mcatid, string hodid)
        {
            FacOperations f = new FacOperations(_context);

            string yeariddata = f.getACCYRSETID();
            string yid = "";
            string whyearid = "";
            if (yearid != "0")
            {
                whyearid = " and accyrsetid  =" + yearid;
                yid = yearid;
            }
            else
            {
                whyearid = " and accyrsetid ="+yeariddata;
                yid = yeariddata;
            }



            string whMCatID = "";
            if (mcatid != "0")
            {
                whMCatID = " and  mc.mcid =" + mcatid;
            }
            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and t.hodid =" + hodid;
            }

            string returnclause = "";string aiclause = "";
            string pckai = "";
            if (hodid == "2" || hodid == "7")
            {
                returnclause = " i.ISAIRETURN_DHS";
                aiclause = " and (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0)) > 0 ";
                pckai = " (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0)) ";
            }
            else
            {
                returnclause = " i.ISAIRETURN_DME";
                aiclause = " and nvl(DME_INDENTQTY, 0) > 0 ";
                pckai = " nvl(DME_INDENTQTY, 0) ";
            }




            string qry = @" select ROW_NUMBER() OVER ( ORDER BY mcid ) AS ID,mcategory,count(distinct itemid) nosai,sum(AIReturn) airetured,count(distinct itemid)-sum(AIReturn) ActualAI,round(sum(dhsaivalue)/10000000,2) as AIValue
,sum(issuedcount) as issuedcount,round(sum(issuedvalue)/10000000,2) as IssuedValue
from 
(
select m.itemid,m.itemcode,mc.mcid,mc.mcategory,AIReturn,dhsai,case when AIReturn=0 then(case when dhsis.rate is not null then dhsai*dhsis.rate else dhsai*nvl(finalrategst,0) end) else 0 end as dhsaivalue

,nvl(issuedQTY,0) issuedQTY,nvl(issuedvalue,0) as  issuedvalue,dhsis.rate,rc.finalrategst, case when nvl(issuedQTY,0)>0 then 1 else 0 end as issuedcount from masitems m 
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
 inner join 
(
select itemid," + pckai + @" as dhsai,case when "+ returnclause + @" ='Y' then 1 else 0 end as AIReturn from itemindent i  where 1 = 1  "+ whyearid + @"  " + aiclause+@"
) ai on ai.itemid = m.itemid
left outer join
(
  select itemid,sum(DHSQTY)issuedQTY,round(sum(DHSValue),2) as issuedvalue,case when sum(DHSQTY)>0 then round(sum(DHSValue)/sum(DHSQTY),2) else 0 end rate
  from 
  ( select m.itemid,
  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) DHSQTY,  
     case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end SKURate,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*(case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end) as DHSValue

  from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
 inner join tbreceipts r on r.receiptid=ri.receiptid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
           inner join soordereditems si on  si.ponoid=rb.ponoid and si.itemid=rb.itemid   
    INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
       where 1=1  and tb.status = 'C'      and        tb.issuetype='NO'  "+ whhodid + @"
        and tb.indentdate between  (select STARTDATE from masaccyearsettings  where ACCYRSETID="+ yeariddata + @") and  (select ENDDATE from masaccyearsettings  where ACCYRSETID="+ yeariddata + @")    " + whMCatID + @" 
        group by  mc.mcid,t.hodid,m.itemid,aci.finalrategst,si.singleunitprice
        ) group by itemid
      
)dhsis on dhsis.itemid=m.itemid
left outer join 
(
select cid, ci.itemid,rt.finalrategst from 
(
select max(contractitemid) cid,itemid from aoccontractitems
group by itemid
) ci 
inner join aoccontractitems rt on rt.contractitemid=ci.cid and rt.itemid=ci.itemid
) rc on rc.itemid=m.itemid
where 1=1 AND M.ISFREEZ_ITPR is null " + whMCatID + @"
) group by mcategory,mcid ";

            var myList = _context.GetHODAIvsIssueDTODbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        [HttpGet("CGMSCStockValueData")]

        public async Task<ActionResult<IEnumerable<StockVDTO>>> CGMSCStockValueData(string mcatid, string warehouseId)
        {

            string whMCatID = "";
            string mcid = "";
            string whWarehouseId1 = "";
            string whWarehouseId2 = "";
            string whWarehouseId3 = "";

            if (mcatid != "0")
            {
                whMCatID = " and  c.mcid =" + mcatid;
                mcid = " and  mcid =" + mcatid;
            }

            if (warehouseId.Trim() != "0" && warehouseId.Trim() != "1" && warehouseId.Trim() != "" && warehouseId.Trim() != null)
            {
                whWarehouseId1 = "and t.warehouseid=" + warehouseId + "";
                whWarehouseId2 = "and tb.towarehouseid=" + warehouseId + "";
                whWarehouseId3 = "and soi.warehouseid=" + warehouseId + "";
            }


            string qry = @"
        select ROW_NUMBER() OVER ( ORDER BY mcid ) AS ID,edlcat, mcid, MCATEGORY, count (distinct itemid) noofitems,sum(ready_no) noofitemsready,sum(UQC_NO) noofitemsuqc,sum(PIP_NO) noofitemspipeline,
round(sum(ReadyForIssueValue)/10000000,2) ReadyForIssueValue,
round(sum(PendingValue)/10000000,2) QCPendingValue,
round(sum(pipelinevalue)/10000000,2) pipelinevalue
        from
        (
select 0 SlNo, edlcat, mcid, MCATEGORY, x.itemid,case when (sum(nvl(ReadyForIssue,0))+nvl(balance,0))>0 then 1 else 0 end ready_no,
case when(sum(nvl(ReadyForIssue,0))+nvl(balance,0))=0 and sum(nvl(Pending,0))>0  then 1 else 0 end as UQC_NO,
case when nvl(p.pipelineqty,0)>0 then 1 else 0 end as PIP_NO,
round(sum(nvl(ReadyForIssueValue,0)),0)+(nvl(balancevalue,0)) ReadyForIssueValue,
round(sum(nvl(PendingValue,0)),0) PendingValue,
sum(nvl(ReadyForIssue,0))+nvl(balance,0) ReadyForIssue,
sum(nvl(Pending,0)) Pending,
nvl(p.pipelineqty,0) pipelineqty,
nvl(p.pipelinevalue,0) pipelinevalue
 from
 (
 select 0 SlNo,case when mia.isedl2021= 'Y' then 'EDL' else 'NON-EDL' end edlcat, c.mcid, c.MCATEGORY, a.itemid, A.batchno, to_char(A.expdate,'dd-mm-yy') as expdate ,Finalrategst ,
 (case when sum(A.ReadyForIssue)>0 then sum(A.ReadyForIssue) else 0 end)  as ReadyForIssue,
 nvl((case when sum(A.ReadyForIssue)>0 then sum(A.ReadyForIssue) else 0 end),0)* Finalrategst ReadyForIssueValue,
 (case when sum(nvl(Pending, 0)) > 0 then sum(nvl(Pending, 0)) else 0 end) Pending, 
 (case when sum(nvl(Pending,0)) >0 then sum(nvl(Pending,0)) else 0 end)*Finalrategst PendingValue, mc.CategoryName,mc.CategoryID
        from
        (
        select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending,
        ci.Finalrategst
        from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems mi on mi.itemid= i.itemid
 inner join soordereditems odi on odi.ponoid = b.ponoid
 inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C'   And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate
    " + whWarehouseId1 + @"
 ) A
 inner join masitems mia on mia.itemid=A.itemid
 inner join masitemcategories mc on mc.CategoryID=mia.CategoryID
 inner join masitemmaincategory c on c.MCID=mc.MCID
 where 1=1  " + whMCatID + @"
 group by mc.CategoryID, mc.CategoryName, A.batchno, A.expdate , A.itemid
 , Finalrategst , c.MCATEGORY, c.mcid, mia.isedl2021
 having sum(nvl(A.ReadyForIssue,0)) >0 or sum(nvl(A.Pending,0)) >0    
 ) x

  Left outer join
 (

  select itemid, balance, balancevalue from (
  select itemid, itemcode, sum(trasferqty) transferqty, sum(receqty) receqty, sum(trasferqty)-sum(receqty) as balance,
  sum(transfervalue)-sum(recptvalue) balancevalue from
  (
  select m.itemcode, m.itemid, c.finalrategst, sum(tbo.issueqty) trasferqty, sum(tbo.issueqty)* c.finalrategst transfervalue,
  nvl(receqty,0) receqty ,nvl(receqty,0)*c.finalrategst recptvalue, tb.warehouseid,tb.towarehouseid,tb.transferid
  from tbindents tb
  inner join tbindentitems tbi on tbi.indentid=tb.indentid
  inner join tboutwards tbo on tbo.indentitemid= tbi.indentitemid
  inner join tbreceiptbatches rb on rb.inwno= tbo.inwno
  inner join soordereditems oi on oi.ponoid = rb.ponoid
  inner join aoccontractitems c on c.contractitemid = oi.contractitemid
  inner join masitems m on m.itemid = tbi.itemid
  inner join maswarehouses w on w.warehouseid = tb.warehouseid
left outer join
(
  select m.itemcode, tr.ponoid, tri.itemid, sum(rb.absrqty) receqty, receiptdate, nvl(tr.transferid,0) transferid
  from tbreceipts tr
  inner join tbreceiptitems tri on tri.receiptid=tr.receiptid
  inner join tbreceiptbatches rb on rb.receiptitemid= tri.receiptitemid
  inner join masitems m on m.itemid = tri.itemid
  where tr.receipttype= 'SP'
    and tr.status= 'C'
    and tr.notindpdmis is null and tri.notindpdmis is null and rb.notindpdmis is null 
    and tr.receiptdate > '01-Apr-23'
     group by tri.itemid, receiptdate, tr.ponoid, tr.transferid, m.itemcode
) x on  x.transferid=tb.transferid and x.itemid=tbi.itemid
 where   tb.Status = 'C' and tb.issuetype in ('SP')  " + whWarehouseId2 + @"
 and tb.indentdate > '01-Apr-23'
  group by m.itemcode, m.itemid, tb.warehouseid, tb.towarehouseid, tb.transferid, receqty, c.finalrategst


  )   group by itemcode,itemid
  ) where balance>0 
  ) IWHB on IWHB.itemid=x.itemid
  left outer join
        (
  select itemid, sum(pipelineQTY) pipelineQTY, sum(pipelinevalue) pipelinevalue from
(select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid
 where op.status  in ('C','O') " + whWarehouseId3 + @"
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having (case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) group by itemid
) p on p.itemid = x.itemid
  where 1=1 " + mcid + @"
 group by edlcat,mcid,MCATEGORY,IWHB.balance,IWHB.balancevalue,x.itemid,p.pipelineqty,p.pipelinevalue 
 )
 group by edlcat,mcid,MCATEGORY
 order by mcid";


            var myList = _context.CGMSCStockValueDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }
        [HttpGet("CGMSCItemStock")]
        public async Task<ActionResult<IEnumerable<CGMSCStockDTO>>> CGMSCItemStock(string mcatid, string EDLNedl, string mitemid, string WHID, string searchP, string userid, string coll_cmho)
        {
            FacOperations f = new FacOperations(_context);
            string whMCatID = "";
            string mcid = "";
            string whedlnedl = "";
            string whitemid = "";
            string whitemidi = "";
            string soidwhid = "";
            string whidW = ""; string whidT = "";
            string whsearchP = "";
            string districtid = "";

            if (coll_cmho == "Collector")

            {
                districtid = f.getCollectorDisid(userid);
                string cmhoid = f.getCMHOFacFromDistrict(districtid);
                Int64 whiddata = f.getWHID(cmhoid.ToString());

                WHID = Convert.ToString(whiddata);

            }
            if (coll_cmho == "CMHO")
            {
                districtid = Convert.ToString(f.geFACTDistrictid(userid));
                string cmhoid = f.getCMHOFacFromDistrict(districtid);
                Int64 whiddata = f.getWHID(cmhoid.ToString());

                WHID = Convert.ToString(whiddata);

            }
        



            if (mcatid != "0")
            {

                mcid = " and mc.MCID =" + mcatid;
            }
            if (searchP != "0")
            {
                whsearchP = "  and  (m.itemcode like '%" + searchP + "%' or  m.ITEMNAME like '%" + searchP + "%' )";
            }

            if (WHID != "0")
            {
                whidW = " and w.warehouseid =" + WHID;
                whidT = " and t.warehouseid =" + WHID;
                soidwhid = " and soi.warehouseid =" + WHID;
            }

            if (mitemid != "0")
            {
                whitemid = " and m.itemid=" + mitemid;
                whitemidi = " and mi.itemid=" + mitemid;
            }
            else
            {


                if (EDLNedl == "Y")
                {

                    whedlnedl = "  m.isedl2021='Y'";
                }
                else if (EDLNedl == "N")
                {

                    whedlnedl = " and (case when m.isedl2021= 'Y' then 'EDL' else 'Non EDL' end )='Non EDL' ";
                }
                else
                {
                    whedlnedl = "";
                }
            }



            //            string qry = @" select A.itemcode as ITEMCODE, A.ItemName as ITEMNAME, A.strength1 as STRENGTH1, A.SKU, edlcat, edl, (case when sum(A.ReadyForIssue) > 0 then sum(A.ReadyForIssue) else 0 end) as READYFORISSUE,
            //(case when sum(nvl(Pending, 0)) > 0 then sum(nvl(Pending,0)) else 0 end) PENDING,itemid,edltype,groupname,itemtypename
            //                  from
            //                 (
            //                 select case when 1=0 then 'All' else w.WAREHOUSENAME end as WAREHOUSENAME, mi.ITEMCODE, b.inwno, mi.ITEMNAME, mi.strength1, mi.unit as SKU,
            //               (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,  
            //                   case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
            //, e.edlcat,e.edl,mi.itemid,case when mi.isedl2021='Y' then 'EDL' else 'Non EDL' end as edltype,nvl(g.groupname,'NA') as groupname,nvl(ty.itemtypename,'NA') itemtypename

            //                  from tbreceiptbatches b
            //                  inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
            //                  inner join tbreceipts t on t.receiptid = i.receiptid
            //                  inner join masitems mi on mi.itemid = i.itemid
            //                  left outer join masedl e on e.edlcat= mi.edlcat
            //                  inner join masitemcategories c on c.categoryid= mi.categoryid
            //                  inner join masitemmaincategory mc on mc.MCID= c.MCID
            //                  left outer join massubitemcategory msc on msc.categoryid= c.categoryid
            //                 inner join MASWAREHOUSES w  on w.warehouseid = t.warehouseid
            //                 left outer join masitemgroups g on g.groupid=mi.groupid
            //                 left outer join masitemtypes ty on ty.itemtypeid=mi.itemtypeid
            //                 left outer join
            //                (
            //                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
            //                   from tboutwards tbo, tbindentitems tbi , tbindents tb
            //                   where 1=1 and tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
            //                   group by tbi.itemid, tb.warehouseid, tbo.inwno
            //                 ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
            //                 Where 1=1 " + whsearchP + @"  " + mcid + @"  and T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate) And(b.Whissueblock = 0 or b.Whissueblock is null)                
            //                 and t.notindpdmis is null " + whidW + @"  and b.notindpdmis is null  " + whidT + @"  and i.notindpdmis is null " + whitemid + @"
            //                 ) A
            //                 where 1=1 and nvl(READYFORISSUE,0)+nvl(PENDING,0)>0
            //                 group by  A.itemcode,A.ItemName, A.strength1,A.SKU,A.edlcat,edl,itemid,A.edltype,A.groupname,A.itemtypename

            //                 order by A.ItemName ";



            string qry = @" select m.itemid ,m.itemcode as ITEMCODE,m.itemname as ITEMNAME,m.STRENGTH1 as STRENGTH1,nvl(ty.itemtypename, 'NA') as itemtypename,nvl(g.groupname, 'NA') as groupname
,nvl(st.ready, 0) as READYFORISSUE,
nvl(st.UQC, 0) as PENDING,nvl(pip.TOTLPIPELINE, 0) as TOTLPIPELINE
,case when m.isedl2021 = 'Y' then 'EDL' else 'Non-EDL' end as edltype
,m.edlcat,e.edl,m.unit AS SKU, nvl(iss_qty,0) as IssuedFY
from masitems m
  left outer join masedl e on e.edlcat = m.edlcat
left outer join masitemgroups g on g.groupid = m.groupid
left outer join masitemtypes ty on ty.itemtypeid = m.itemtypeid
left outer join
(
select itemid, sum(nvl(ReadyForIssue, 0)) as READY, sum(nvl(Pending, 0)) as UQC from
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))
 else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,    
 case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
 from tbreceiptbatches b
 inner
 join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner
 join tbreceipts t on t.receiptid = i.receiptid
 inner
 join masitems mi on mi.itemid = i.itemid
 inner
 join masitemcategories c on c.categoryid = mi.categoryid
inner
 join masitemmaincategory mc on mc.MCID = c.MCID
 inner
 join soordereditems odi on odi.ponoid = b.ponoid
 inner
 join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null
     and tbi.notindpdmis is null
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid
     Where T.Status = 'C'  " + whitemidi + @"   And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate " + whidT + @"  

     ) group by itemid
) st on st.itemid = m.itemid


left outer join
(
  select itemid, sum(pipelineQTY) TOTLPIPELINE from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty, 0)receiptabsqty,
receiptdelayexception, round(sysdate - op.soissuedate, 0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID = OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid = OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid = tr.receiptid
where tr.receipttype = 'NO' and tr.status = 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid = OP.PoNoID and rec.itemid = OI.itemid
 where op.status  in ('C', 'O') " + whitemid + @" " + soidwhid + @"
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end) > 0
) group by itemid
) pip on pip.itemid = m.itemid



left outer join
(
      select    m.itemid,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty 
  from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
 inner join tbreceipts r on r.receiptid=ri.receiptid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
       where 1=1  and tb.status = 'C' 
       and tb.issuetype='NO' 
           and tb.indentdate between (select STARTDATE from masaccyearsettings  where ACCYRSETID=545) and  (select ENDDATE from masaccyearsettings  where ACCYRSETID=545)  
        group by m.itemid
) iss on iss.itemid = m.itemid

inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
where 1=1 " + whitemid + @"  
"+ mcid + @"
AND m.ISFREEZ_ITPR is null and (nvl(st.ready, 0)+nvl(st.UQC, 0) + nvl(pip.TOTLPIPELINE, 0)) > 0  order by M.ItemName";



            var myList = _context.CGMSCStockDbSet
.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        [HttpGet("WarehouseWiseStock")]
        public async Task<ActionResult<IEnumerable<WHWiseStockDTO>>> WarehouseWiseStock(string mitemid, string whid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();

            string whmitemid = ""; string mwhmitemid = ""; string ihmitemid = "";
            if (mitemid != "0")
            {
                whmitemid = " and mi.itemid=" + mitemid;
                mwhmitemid = " and m.itemid=" + mitemid;
                ihmitemid = " and i.itemid =" + mitemid;


            }
            string whWarehouseid = ""; string toWHID = "";
            string tbwhid = "";
            if (whid != "0")
            {
                whWarehouseid = " and w.warehouseid=" + whid;
                toWHID = " and t.towarehouseid=" + whid;
                tbwhid= " and tb.warehouseid=" + whid;

            }
            string qry = @"   select w.WAREHOUSENAME,w.warehouseid,nvl(READYFORISSUE, 0) as READYFORISSUE,nvl(PENDING, 0) as PENDING,
nvl(pip.pipelineqty, 0) supplierpipeline,nvl(IWHP.transferqty, 0) iwhpipeline,AITEMID,pip.itemid pipitemid, iwhp.itemid iwhitemid

,(case when AITEMID is not null then AITEMID else case when AITEMID is null and pip.itemid is not null then
pip.itemid else iwhp.itemid end end)|| w.warehouseid as ID,nvl(iss_qty,0) as issuedFY
from MASWAREHOUSES w
left outer join
(
select A.itemcode as ITEMCODE,A.ItemName as ITEMNAME,A.strength1 as STRENGTH1, A.SKU,(case when sum(A.ReadyForIssue) > 0 then sum(A.ReadyForIssue) else 0 end) as READYFORISSUE,(case when sum(nvl(Pending, 0)) > 0 then sum(nvl(Pending,0)) else 0 end) PENDING
,edlcat,edl,warehouseid,A.itemid as AITEMID
                  from
                 (
                 select w.warehouseid, mi.ITEMCODE, b.inwno, mi.ITEMNAME, mi.strength1, mi.unit as SKU,
               (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,  
                   case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
, e.edlcat,e.edl,mi.itemid

                  from tbreceiptbatches b
                  inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner join tbreceipts t on t.receiptid = i.receiptid
                  inner join masitems mi on mi.itemid = i.itemid
                   left outer join masedl e on e.edlcat = mi.edlcat
                  inner join masitemcategories c on c.categoryid = mi.categoryid
                  inner join masitemmaincategory mc on mc.MCID = c.MCID
                  left outer join massubitemcategory msc on msc.categoryid = c.categoryid
                 inner join MASWAREHOUSES w  on w.warehouseid = t.warehouseid
                 left outer join
                (
                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty, 0)) issueqty
                   from tboutwards tbo, tbindentitems tbi, tbindents tb
                   where 1 = 1 and  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   group by tbi.itemid, tb.warehouseid, tbo.inwno
                 ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
                 Where T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate) And(b.Whissueblock = 0 or b.Whissueblock is null)
                     " + whmitemid + @"  " + whWarehouseid + @"
                 and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null
                 ) A group by A.itemcode,A.ItemName, A.strength1,A.SKU ,edlcat,edl,warehouseid,A.itemid
 ) b on b.warehouseid = w.warehouseid
 left outer join
 (
 select  warehouseid, itemid, sum(pipelineQTY) pipelineQTY from
(
select w.warehousename, w.warehouseid, m.itemcode, OI.itemid, op.ponoid, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty, 0)receiptabsqty,
case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end as pipelineQTY
from soOrderPlaced OP
inner
join SoOrderedItems OI on OI.PoNoID = OP.PoNoID
inner
join soorderdistribution soi on soi.orderitemid = OI.orderitemid
inner
join masitems m on m.itemid = oi.itemid
inner
join maswarehouses w on w.warehouseid = soi.warehouseid
left outer join
(
select tr.warehouseid, tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid,tri.itemid,tr.warehouseid
) rec on rec.ponoid = OP.PoNoID and rec.itemid = OI.itemid and rec.warehouseid = soi.warehouseid
 where op.status  in ('C', 'O') " + mwhmitemid + @"  " + whWarehouseid + @"
 group by w.warehousename,w.warehouseid,m.itemcode,op.ponoid,OI.itemid ,rec.receiptabsqty,
 op.soissuedate,op.extendeddate ,receiptdelayexception
 having(case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end) > 0
) group by warehouseid,itemid
 ) pip on pip.warehouseid = w.warehouseid


 left outer join
(
select sum(o.issueqty) as transferQTY, t.towarehouseid, tbi.itemid
from stktransfers t
inner
join stktransferitems i on i.transferid = t.transferid
inner
join tbindents ti on ti.transferid = t.transferid
inner
join tbindentitems tbi on tbi.indentid = ti.indentid and tbi.itemid = i.itemid
inner join tboutwards o on o.indentitemid = tbi.indentitemid
where t.status = 'C' " + ihmitemid + @" " + toWHID + @"
and t.transferid in (select transferid from tbindents where status = 'C' and transferid is not null)
and t.transferid not in (select transferid from tbreceipts where status = 'C' and transferid is not null)
and t.transferdate between '01-APR-23' and sysdate
group by t.towarehouseid,tbi.itemid
) IWHP on    IWHP.towarehouseid = w.warehouseid

 left outer join
(
  select    m.itemid,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty , tb.warehouseid
  from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
 inner join tbreceipts r on r.receiptid=ri.receiptid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
       where 1=1  and tb.status = 'C'  "+ tbwhid + @" "+ mwhmitemid + @"
       and 
       tb.issuetype='NO' 
           and tb.indentdate between (select STARTDATE from masaccyearsettings  where ACCYRSETID="+yearid+ @") and  (select ENDDATE from masaccyearsettings  where ACCYRSETID="+yearid+@")  
        group by m.itemid,tb.warehouseid
) whis on    whis.warehouseid = w.warehouseid
 order by w.WAREHOUSENAME ";
            var myList = _context.WHWiseStockDbSet
.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("QCLabOutTime")]
        public async Task<ActionResult<IEnumerable<QCLabOutDTO>>> QCLabOutTime()
        {


            string qry = @"  select ROW_NUMBER() OVER ( ORDER BY mcid ) AS ID,mcid, mcategory,count(distinct itemcode) as nositems,count(distinct batchno) as nosbatch,Round(sum(receiptvalue)/10000000,2) as uqcValue
 ,timeline,ExceddedSincetimeline,ExceddedSincetimeline1
 from 
 (
 select mc.mcid,mc.mcategory,m.itemcode,i.itemid,tb.batchno,tb.absrqty receiptqty,ci.finalrategst,tb.absrqty*ci.finalrategst receiptvalue
 ,qt.labreceiptdate,round(sysdate-qt.labreceiptdate) as nosdays,mt.qcdayslab
 ,case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end as timeline
,case when (case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end)='Out of timeline'
then round(sysdate-qt.labreceiptdate)-mt.qcdayslab else 0 end as Dayover

,case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>15 then '>15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=15 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>6 then '7-15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=6 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>=3 then '3-6 days'
else 'Under 2 days' 
end end end as  ExceddedSincetimeline,

case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>15 then '1.>15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=15 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>6 then '2.7-15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=6 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>=3 then '3.3-6 days'
else '4.Under 2 days' 
end end end as  ExceddedSincetimeline1

,case when (case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end)='Within timeline'
then mt.qcdayslab-round(sysdate-qt.labreceiptdate) else 0 end as PendingDays
 from tbreceipts t
 inner join tbreceiptitems i on (i.receiptid = t.receiptid)
 inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
 left outer join soordereditems oi on oi.ponoid = tb.ponoid
 left outer join aoccontractitems ci on ci.contractitemid = oi.contractitemid
 inner join masitems m on m.itemid = i.itemid
 inner join masitemtypes mt on mt.itemtypeid = m.itemtypeid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
 left outer join
 (
 select tbi.itemid,tbo.outwno,tbo.inwno from tboutwards tbo 
 inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
 inner join tbindents t on t.indentid = tbi.indentid 
 where  t.issuetype in ('QA','QS') and t.status = 'C' 
 ) qi on qi.inwno = tb.inwno 
 
   left outer join
 (
 select s.outwno,s.itemid,s.receiptdate samplereceiptdate,t.samplereceiveddate labreceiptdate 
 from qcsamples s
 inner join qctests t on t.sampleid = s.sampleid or t.sampleid = s.refsampleid 
 ) qt on qt.outwno = qi.outwno 
 
 where t.status='C' and  t.receipttype = 'NO' and qt.labreceiptdate between '01-Apr-23' and sysdate
 and m.qctest = 'Y' and tb.whissueblock is null and tb.qastatus = 0
 and mt.qcdayslab is  not null and round(sysdate-qt.labreceiptdate)-mt.qcdayslab>0
 )
 where timeline='Out of timeline'
 group by mcid, mcategory,timeline,ExceddedSincetimeline,ExceddedSincetimeline1
 order by ExceddedSincetimeline1 ";
            var myList = _context.QCLabOutDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("QCLabOutTimeBatchwise")]
        public async Task<ActionResult<IEnumerable<QCLabTimeLineDetailsDTO>>> QCLabOutTimeBatchwise(string mcid, string dayID)
        {
            string dayclause = "";
            if (dayID == "1.>15 days")
            {
                dayclause = " and  (round(sysdate - qt.labreceiptdate) - mt.qcdayslab) > 15 ";
            }
            else if (dayID == "2.7-15 days")
            {
                dayclause = " and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=15 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>6 ";
            }
            else if (dayID == "3.3-6 days")
            {
                dayclause = " and ( round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=6 and(round(sysdate-qt.labreceiptdate)-mt.qcdayslab )>=3 ";
            }
            else if (dayID == "4.Under 2 days")
            {
                dayclause = " and  (round(sysdate - qt.labreceiptdate) - mt.qcdayslab) <= 2 ";
            }
            else
            {

            }

            string qry = @" select itemcode,mcid,mcategory,itemname||''||itemcode as itemname,itemtype,qcdayslab,batchno,Round(sum(nosdays)/count(batchno),0) as AvgDayInLab,count(distinct labid) noslab ,
sum(receiptqty) as QTY,ExceddedSincetimeline1
from 
(

 select itemcode,itemname,itemtype,batchno,labreceiptdate,qcdayslab,nosdays,LABNAME,receiptqty,labid,mcid,mcategory,ExceddedSincetimeline1 from ( 
 
 select lb.labid,lb.LABNAME,mc.mcid,mc.mcategory,m.itemcode,m.itemname,i.itemid,tb.batchno,tb.absrqty receiptqty,ci.finalrategst,tb.absrqty*ci.finalrategst receiptvalue
 ,qt.labreceiptdate,round(sysdate-qt.labreceiptdate) as nosdays,mt.qcdayslab
 ,case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end as timeline
,case when (case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end)='Out of timeline'
then round(sysdate-qt.labreceiptdate)-mt.qcdayslab else 0 end as Dayover

,case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>15 then '>15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=15 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>6 then '7-15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=6 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>=3 then '3-6 days'
else 'Under 2 days' 
end end end as  ExceddedSincetimeline,

case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>15 then '1.>15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=15 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>6 then '2.7-15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=6 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>=3 then '3.3-6 days'
else '4.Under 2 days' 
end end end as  ExceddedSincetimeline1

,case when (case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end)='Within timeline'
then mt.qcdayslab-round(sysdate-qt.labreceiptdate) else 0 end as PendingDays
,mt.itemtypename as itemtype
 from tbreceipts t
 inner join tbreceiptitems i on (i.receiptid = t.receiptid)
 inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
 left outer join soordereditems oi on oi.ponoid = tb.ponoid
 left outer join aoccontractitems ci on ci.contractitemid = oi.contractitemid
 inner join masitems m on m.itemid = i.itemid
 inner join masitemtypes mt on mt.itemtypeid = m.itemtypeid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
 left outer join
 (
 select tbi.itemid,tbo.outwno,tbo.inwno from tboutwards tbo 
 inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
 inner join tbindents t on t.indentid = tbi.indentid 
 where  t.issuetype in ('QA','QS') and t.status = 'C' 
 ) qi on qi.inwno = tb.inwno 
 
   left outer join
 (
 select s.outwno,s.itemid,s.receiptdate samplereceiptdate,t.samplereceiveddate labreceiptdate ,labid
 from qcsamples s
 inner join qctests t on t.sampleid = s.sampleid or t.sampleid = s.refsampleid 
 ) qt on qt.outwno = qi.outwno 
 inner join qclabs lb on  lb.labid=qt.labid
 
 where 1=1 and mc.MCID=1 and t.status='C' and  t.receipttype = 'NO' and qt.labreceiptdate between '01-Apr-23' and sysdate
 and m.qctest = 'Y' and tb.whissueblock is null and tb.qastatus = 0
 and mt.qcdayslab is  not null and round(sysdate-qt.labreceiptdate)-mt.qcdayslab>0  " + dayclause + @"
 )
 ) group by itemcode,itemname,qcdayslab,batchno,itemtype,mcid,mcategory,ExceddedSincetimeline1
 order by Round(sum(nosdays)/count(batchno),0) desc ";
            var myList = _context.QCLabTimeLineDetailsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("QCLabOutTimeLabDetails")]
        public async Task<ActionResult<IEnumerable<QCLabTimeLineBatchDTO>>> QCLabOutTimeLabDetails(string mcid, string dayID, string batchno)
        {
            string dayclause = "";
            if (dayID == ">15 days")
            {
                dayclause = " and  (round(sysdate - qt.labreceiptdate) - mt.qcdayslab) > 15 ";
            }
            else if (dayID == "7-15 days")
            {
                dayclause = " and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=15 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>6 ";
            }
            else if (dayID == "3-6 days")
            {
                dayclause = " and ( round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=6 and(round(sysdate-qt.labreceiptdate)-mt.qcdayslab )>=3 ";
            }
            else if (dayID == "Under 2 days")
            {
                dayclause = " and  (round(sysdate - qt.labreceiptdate) - mt.qcdayslab) <= 2 ";
            }


            string qry = @"
 select distinct mcid,mcategory, itemcode,itemname,itemtype,qcdayslab,batchno,
 labreceiptdate,nosdays,LABNAME,nvl(PHONE1,0) as PHONE1 ,labid,ExceddedSincetimeline  from ( 
 select lb.labid,lb.LABNAME,lb.PHONE1,mc.mcid,mc.mcategory,m.itemcode,m.itemname,i.itemid,tb.batchno,tb.absrqty receiptqty,ci.finalrategst,tb.absrqty*ci.finalrategst receiptvalue
 ,qt.labreceiptdate,round(sysdate-qt.labreceiptdate) as nosdays,mt.qcdayslab
 ,case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end as timeline
,case when (case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end)='Out of timeline'
then round(sysdate-qt.labreceiptdate)-mt.qcdayslab else 0 end as Dayover

,case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>15 then '>15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=15 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>6 then '7-15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=6 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>=3 then '3-6 days'
else 'Under 2 days' 
end end end as  ExceddedSincetimeline,

case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>15 then '1.>15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=15 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>6 then '2.7-15 days'
else case when (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)<=6 and (round(sysdate-qt.labreceiptdate)-mt.qcdayslab)>=3 then '3.3-6 days'
else '4.Under 2 days' 
end end end as  ExceddedSincetimeline1

,case when (case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end)='Within timeline'
then mt.qcdayslab-round(sysdate-qt.labreceiptdate) else 0 end as PendingDays
,mt.itemtypename as itemtype
 from tbreceipts t
 inner join tbreceiptitems i on (i.receiptid = t.receiptid)
 inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
 left outer join soordereditems oi on oi.ponoid = tb.ponoid
 left outer join aoccontractitems ci on ci.contractitemid = oi.contractitemid
 inner join masitems m on m.itemid = i.itemid
 inner join masitemtypes mt on mt.itemtypeid = m.itemtypeid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
 left outer join
 (
 select tbi.itemid,tbo.outwno,tbo.inwno from tboutwards tbo 
 inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
 inner join tbindents t on t.indentid = tbi.indentid 
 where  t.issuetype in ('QA','QS') and t.status = 'C' 
 ) qi on qi.inwno = tb.inwno 
 
   left outer join
 (
 select s.outwno,s.itemid,s.receiptdate samplereceiptdate,t.samplereceiveddate labreceiptdate ,labid
 from qcsamples s
 inner join qctests t on t.sampleid = s.sampleid or t.sampleid = s.refsampleid 
 ) qt on qt.outwno = qi.outwno 
 inner join qclabs lb on  lb.labid=qt.labid
 
 where  mc.MCID=" + mcid + @" and t.status='C' and  t.receipttype = 'NO' and qt.labreceiptdate between '01-Apr-23' and sysdate
 and m.qctest = 'Y' and tb.whissueblock is null and tb.qastatus = 0
 and mt.qcdayslab is  not null " + dayclause + @"  and batchno='" + batchno + @"'
 ) order by  labreceiptdate ";
            var myList = _context.QCLabTimeLineBatchDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("QCLabWithTime")]
        public async Task<ActionResult<IEnumerable<QCLabWithDTO>>> QCLabWithTime()
        {


            string qry = @"  
 
 select ROW_NUMBER() OVER ( ORDER BY mcid ) AS ID,mcid, mcategory,count(distinct itemcode) as nositems,count(distinct batchno) as nosbatch,Round(sum(receiptvalue)/10000000,2) as uqcValue
 ,timeline,ExceddedSincetimeline,ExceddedSincetimeline1
 from 
 (
 select mc.mcid,mc.mcategory,m.itemcode,i.itemid,tb.batchno,tb.absrqty receiptqty,ci.finalrategst,tb.absrqty*ci.finalrategst receiptvalue
 ,qt.labreceiptdate,round(sysdate-qt.labreceiptdate) as nosdays,mt.qcdayslab
 ,case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end as timeline
,case when (case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end)='Out of timeline'
then round(sysdate-qt.labreceiptdate)-mt.qcdayslab else 0 end as Dayover

,case when (mt.qcdayslab-round(sysdate-qt.labreceiptdate))>15 then '>15 days'
else case when (mt.qcdayslab-round(sysdate-qt.labreceiptdate))<=15 and (mt.qcdayslab-round(sysdate-qt.labreceiptdate))>6 then '7-15 days'
else case when (mt.qcdayslab-round(sysdate-qt.labreceiptdate))<=6 and (mt.qcdayslab-round(sysdate-qt.labreceiptdate))>=3 then '3-6 days'
else 'Under 2 days' 
end end end as  ExceddedSincetimeline,

case when (mt.qcdayslab-round(sysdate-qt.labreceiptdate))>15 then '4.>15 days'
else case when (mt.qcdayslab-round(sysdate-qt.labreceiptdate))<=15 and (mt.qcdayslab-round(sysdate-qt.labreceiptdate))>6 then '3.7-15 days'
else case when (mt.qcdayslab-round(sysdate-qt.labreceiptdate))<=6 and (mt.qcdayslab-round(sysdate-qt.labreceiptdate))>=3 then '2.3-6 days'
else '1.Under 2 days' 
end end end as  ExceddedSincetimeline1


,case when (case when mt.qcdayslab is  null then 'Timeline Not Set'
else case when 
round((sysdate-qt.labreceiptdate),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end)='Within timeline'
then mt.qcdayslab-round(sysdate-qt.labreceiptdate) else 0 end as PendingDays


 from tbreceipts t
 inner join tbreceiptitems i on (i.receiptid = t.receiptid)
 inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
 left outer join soordereditems oi on oi.ponoid = tb.ponoid
 left outer join aoccontractitems ci on ci.contractitemid = oi.contractitemid
 inner join masitems m on m.itemid = i.itemid
 inner join masitemtypes mt on mt.itemtypeid = m.itemtypeid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
 left outer join
 (
 select tbi.itemid,tbo.outwno,tbo.inwno from tboutwards tbo 
 inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
 inner join tbindents t on t.indentid = tbi.indentid 
 where  t.issuetype in ('QA','QS') and t.status = 'C' 
 ) qi on qi.inwno = tb.inwno 
 
   left outer join
 (
 select s.outwno,s.itemid,s.receiptdate samplereceiptdate,t.samplereceiveddate labreceiptdate 
 from qcsamples s
 inner join qctests t on t.sampleid = s.sampleid or t.sampleid = s.refsampleid 
 ) qt on qt.outwno = qi.outwno 
 
 where t.status='C' and  t.receipttype = 'NO' and qt.labreceiptdate between '01-Apr-23' and sysdate
 and m.qctest = 'Y' and tb.whissueblock is null and tb.qastatus = 0
 and mt.qcdayslab is  not null and mt.qcdayslab-round(sysdate-qt.labreceiptdate)>0
 )
 where timeline='Within timeline'
 group by mcid, mcategory,timeline,ExceddedSincetimeline,ExceddedSincetimeline1
 order by ExceddedSincetimeline1 ";
            var myList = _context.QCLabWithDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("QCResultFinalUpdate")]
        public async Task<ActionResult<IEnumerable<QCLabOutDTO>>> QCResultFinalUpdate()
        {


            string qry = @"  select ROW_NUMBER() OVER ( ORDER BY mcid ) AS ID,mcid, mcategory,count(distinct itemid) as nositems
,count(distinct batchno) as nosbatch,Round(sum(receiptvalue)/10000000,2) as uqcValue,'Final Update Pending' as timeline

,daysPendingP as  ExceddedSincetimeline,daysPending as ExceddedSincetimeline1
from 
(
select distinct t.sampleid,mc.mcid, mc.mcategory, ql.labid,ql.labname,ql.email,ql.phone1 mobileno,m.itemcode drugcode,m.itemname drugname,
mt.itemtypename,mt.itemtypeid,s.batchno,t.sampleno,t.samplereceiveddate,t.labuploadeddate,mt.qcdayslab qcdeadlinedays,
round((sysdate-t.labuploadeddate),0) PendinDays,t.labresult

,case when dr.descid is not null then '6:Discrepancy in Report' else case when round((sysdate-t.labuploadeddate),0)=0 then '5:-Todays Pending'
else case when round((sysdate-t.labuploadeddate),0)>0 and  round((sysdate-t.labuploadeddate),0)<=3
then '4:-1 to 3 days Pending'
else case when round((sysdate-t.labuploadeddate),0)>3 and  round((sysdate-t.labuploadeddate),0)<=6
then '3:-3 to 6 days Pending' else case when round((sysdate-t.labuploadeddate),0)>6 and  round((sysdate-t.labuploadeddate),0)<=10
then '2:-7 to 10 days Pending'
else  '1:More than 10 days Pending'
end end end end end as daysPending
,case when dr.descid is not null then 'Discrepancy in Report' else case when round((sysdate-t.labuploadeddate),0)=0 then 'Today'
else case when round((sysdate-t.labuploadeddate),0)>0 and  round((sysdate-t.labuploadeddate),0)<=3
then '1-3 days'
else case when round((sysdate-t.labuploadeddate),0)>3 and  round((sysdate-t.labuploadeddate),0)<=6
then '3-6 days' else case when round((sysdate-t.labuploadeddate),0)>7 and  round((sysdate-t.labuploadeddate),0)<=10
then '7-10 days' 
else  '> 10 days'
end end end end end as daysPendingP

,m.itemid,ci.finalrategst,tb.absrqty*ci.finalrategst receiptvalue
from qctests t
inner join qcsamples s on s.sampleid = t.sampleid
inner join masitems m on m.itemid = s.itemid
inner join masitemcategories c on c.categoryid=m.categoryid
 inner join masitemmaincategory mc on mc.MCID=c.MCID
 Inner Join MasItemVens Ven on (Ven.VenID = m.VenID) 
inner join masitemtypes mt on mt.itemtypeid = m.itemtypeid
inner join qclabs ql on ql.labid = t.labid
inner join usrusers u on u.labid=ql.labid

 left outer join  
 ( 
 select max(descid) as descid,labid,sampleid,testid from qcsamplediscrepancy 
 where 1=1 and  islabdisc='N'  group by labid,sampleid,testid 
 ) dr  on dr.labid=T.LabID and dr.testid=T.QcTestID  and  dr.sampleid=T.Sampleid 
 left outer join qcsamplediscrepancy d on d.descid=dr.descid 
  left outer join
 (
 select tbi.itemid,tbo.outwno,tbo.inwno from tboutwards tbo 
 inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
 inner join tbindents t on t.indentid = tbi.indentid 
 where  t.issuetype in ('QA','QS') and t.status = 'C' 
 ) qi on qi.outwno=s.outwno
  inner join tbreceiptbatches tb on tb.inwno  =qi.inwno
  left outer join soordereditems oi on oi.ponoid = tb.ponoid
 left outer join aoccontractitems ci on ci.contractitemid = oi.contractitemid
where  1=1 and T.LabIssueDate>'01-Apr-2022' 
and   T.SampleReceivedDate is not null   and t.labuploadeddate is not null 
and t.reportreceiveddate is null and t.testresult is  null
) group by mcid, mcategory,daysPending,daysPendingP order by mcid,daysPending ";
            var myList = _context.QCLabOutDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("getUnpaidSupplier")]
        public async Task<ActionResult<IEnumerable<UnpaidSupplierDTO>>> getUnpaidSupplier()
        {


            string qry = @"  select SUPPLIERID,SUPPLIERNAME ||'-' ||to_char(count(distinct PONOID)) as nospo from v_popaymentstatus
                            where PAYMENTSTATUSNEW !='Paid'
                            group by SUPPLIERNAME,SUPPLIERID
                            order by SUPPLIERNAME
                             ";

            var myList = _context.UnpaidSupplierDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("getSupplierLiability")]
        public async Task<ActionResult<IEnumerable<SupplierLiabilityDTO>>> getSupplierLiability(string supplierId)
        {
            string qry = @" select ROW_NUMBER() OVER (ORDER BY FITUNFIT) AS serial_no, FITUNFIT,HODTYPE,count(distinct PONOID) as nosPO
                            ,Round(sum(LIBWITHOUTADM)/10000000,2) LibAmt from v_fitunfit where supplierid=" + supplierId + @"
                            group by FITUNFIT,HODTYPE
                            order by HODTYPE,FITUNFIT";

            var myList = _context.SupplierLiabilityDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getSupplierPO")]
        public async Task<ActionResult<IEnumerable<SupplierPoDTO>>> getSupplierPO(string supplierId, string fitnessStatus, string hodType)
        {
            //string qry = @" select ROW_NUMBER() OVER (ORDER BY LASTMRCDT) AS serial_no, ITEMCODE,ITEMNAME,pono,PODATE,POQTY,
            //,Round(RECEIPTVALUE/100000,2) as RvalueLacs
            //                      ,REASONNAME from v_fitunfit where supplierid="+ supplierId +@"
            //             and FITUNFIT='"+ fitnessStatus + @"' and HODTYPE='"+ hodType +@"'
            //           order by LASTMRCDT ";


            string qry = @" select ROW_NUMBER() OVER (ORDER BY 
(case when SDACKUSERDT is not null and SDACKUSERDT>LASTMRCDT and SDACKUSERDT>LastQCPaadedDT then SDACKUSERDT else case when LastQCPaadedDT>SDACKUSERDT and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end)
) AS serial_no, m.ITEMCODE,m.ITEMNAME,pono,PODATE,POQTY,case when LASTMRCDT is not null then  to_char(LASTMRCDT,'dd-MM-yyyy') else 'Not Received' end as LASTMRCDT,
Round(RECEIPTVALUE/100000,2) as RvalueLacs
                            ,REASONNAME ,case when sdackuserdt is not null then  to_char(sdackuserdt,'dd-MM-yyyy') else 'Not Received' end as SDDate,PRESENTFILE,FILENO,FILEDT,to_char(LastQCPaadedDT,'dd-MM-yyyy') as LastQCPaadedDT
                            ,to_char(CASE when SDACKUSERDT is not null and SDACKUSERDT>LASTMRCDT and SDACKUSERDT>LastQCPaadedDT then SDACKUSERDT else case when LastQCPaadedDT>SDACKUSERDT and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end ,'dd-MM-yyyy')  as FitDate
                            ,FITUNFIT,HODTYPE,
                            case when  nvl(sc.sanctionid,'0')  ='0' and nvl(scP.fpsanctionid,'0')='0' then 'Not Genrated'                      
                            else case when (nvl(sc.sanctionid,0)!='0' and  nvl(scP.fpsanctionid,0)=0 and nvl(scP.PID,0)=0) then 'Sanction Gen. on '||to_char(SancGenDT,'dd-MM-yyyy') 
                            else case when nvl(scP.PID,0)>0 then 'Cheque Prepared on '||to_char(bp.AIDDATE,'dd-MM-yyyy')
                            else 'Forward For Cheque Prep on '||to_char(FPDate,'dd-MM-yyyy')
                            end end end as fileFinStatus
                            ,f.SUPPLIERID,f.SUPPLIERNAME,m.unit,f.RECEIPTQTY,f.QCPASS,case when m.qctest='Y' then 'Required' else 'Not Required' end as QCTest
                            ,f.BUDGETID,b.BUDGETNAME
                            from v_fitunfit f
                            inner join masitems m on m.itemid=f.itemid
                     inner join masbudget b on b.BUDGETID=f.BUDGETID
                             left outer join  
            (
            select s.ponoid,max(reportreceiveddate) as LastQCPaadedDT,s.itemid
            from qcsamples s  
            inner join soOrderPlaced so on so.ponoid=s.ponoid
            inner join masitems mi on mi.itemid=s.itemid
            left outer join qctests q on (s.sampleid=q.sampleid or s.refsampleid=q.sampleid )
            where  q.testresult='SQ' and reportreceiveddate is not null
            group by s.itemid,s.ponoid
            ) qt on qt.ponoid=f.ponoid  and qt.itemid=f.ItemID
             left outer join
 (
 select max(sanctionid) sanctionid ,ponoid,Max(SANCTIONDATE) SancGenDT from blpsanctions
 where STATUS  in ('IN') group by ponoid
 )sc on sc.ponoid=f.ponoid
 left outer join
 (
 select max(sanctionid) fpsanctionid ,ponoid,Max(SANCTIONDATE) FPDate,max(s.paymentid)PID from blpsanctions s
 where s.STATUS  in ('FP') group by s.ponoid
 )scP on scP.ponoid=f.ponoid
 left outer join blppayments bp on bp.paymentid=scp.PID
 where f.supplierid=" + supplierId + @"        and f.FITUNFIT='" + fitnessStatus + @"' and f.HODTYPE='" + hodType + @"'";

            var myList = _context.SupplierPoDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("QCSameBatchWithDiffPOs")]
        public async Task<ActionResult<IEnumerable<QCSameBatchDTO>>> QCSameBatchWithDiffPOs()
        {


            string qry = @" select MCID,MCATEGORY,count(distinct itemid) nosItem,count(distinct tested_batchno) as nosBatches from 
(
select distinct mc.MCID,mc.MCATEGORY,m.itemid,s.batchno tested_batchno,t.sampleno tested_sampleno,t.reportno,to_char( t.reportdate,'dd-MM-yyyy') reportdate,nvl(s.newtestresult,s.testresult) testresult,o.pono pono_qc_done,sp.suppliername,qsp.batchno batchno_qc_pending,qsp.sampleno as sampleno_pending,to_char(qsp.receiptdate,'dd-MM-yyyy') sample_receiptdate,qsp.pono pono_qc_pending from qcsamples s 
left outer join qctests t on t.sampleid = s.sampleid or t.sampleid = s.refsampleid
inner join soorderplaced o on o.ponoid = s.ponoid
inner join massuppliers sp on sp.supplierid = o.supplierid
inner join masitems m on m.itemid = s.itemid and m.qctest = 'Y'
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
left outer join
(
select s.supplierid,s.itemid,batchno,o.pono,s.receiptdate,t.sampleno,nvl(s.newtestresult,s.testresult) testresult from qcsamples s 
left outer join qctests t on t.sampleid = s.sampleid
inner join soorderplaced o on o.ponoid = s.ponoid
where nvl(s.newtestresult,s.testresult) is null
) qsp on qsp.batchno = s.batchno and qsp.itemid = s.itemid and qsp.supplierid = s.supplierid
where   nvl(s.newtestresult,s.testresult) = 'SQ' and qsp.pono is not null 
and  qsp.testresult is null and o.podate>'01-Apr-2019'
) group by MCID,MCATEGORY";
            var myList = _context.QCSameBatchDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("QCSameBatchWithDiffPOsDetails")]
        public async Task<ActionResult<IEnumerable<QCSameBatchDetailsDTO>>> QCSameBatchWithDiffPOsDetails(string mcid)
        {


            string qry = @" 
select ROW_NUMBER() OVER (ORDER BY itemid,tested_batchno,TESTED_SAMPLENO,SAMPLENO_PENDING ) as id,
itemid,itemcode,itemname,suppliername,pono_qc_done,tested_batchno,
tested_sampleno,reportno,testresult,reportdate,sampleno_pending,sample_receiptdate,pono_qc_pending
from 
(

select distinct  mc.MCID,m.itemid,m.itemcode,m.itemname,s.batchno tested_batchno,t.sampleno tested_sampleno,t.reportno,to_char( t.reportdate,'dd-MM-yyyy') reportdate,nvl(s.newtestresult,s.testresult) testresult,o.pono pono_qc_done,sp.suppliername,qsp.batchno batchno_qc_pending,qsp.sampleno as sampleno_pending,to_char(qsp.receiptdate,'dd-MM-yyyy') sample_receiptdate,qsp.pono pono_qc_pending from qcsamples s 
left outer join qctests t on t.sampleid = s.sampleid or t.sampleid = s.refsampleid
inner join soorderplaced o on o.ponoid = s.ponoid
inner join massuppliers sp on sp.supplierid = o.supplierid
inner join masitems m on m.itemid = s.itemid and m.qctest = 'Y'
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
left outer join
(
select s.supplierid,s.itemid,batchno,o.pono,s.receiptdate,t.sampleno,nvl(s.newtestresult,s.testresult) testresult from qcsamples s 
left outer join qctests t on t.sampleid = s.sampleid
inner join soorderplaced o on o.ponoid = s.ponoid
where nvl(s.newtestresult,s.testresult) is null
) qsp on qsp.batchno = s.batchno and qsp.itemid = s.itemid and qsp.supplierid = s.supplierid
where 1=1 and  o.podate>'01-04-2019' and  mc.mcid=" + mcid + @" and nvl(s.newtestresult,s.testresult) = 'SQ' and qsp.pono is not null 
and  qsp.testresult is null
) ";
            var myList = _context.QCSameBatchDetailsDbset
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        [HttpGet("YearWiseExpired")]
        public async Task<ActionResult<IEnumerable<YearWiseExpiredDTO>>> YearWiseExpired(string mcid)
        {


            string qry = @" select Year, mcid,MCATEGORY,count(distinct itemcode) noofitems,round(sum(ExpValueBasic)/10000000,3) ExpValueBasic,round(sum(ExpValueGST)/10000000,3) ExpValueGST from
(
select distinct (select SHACCYEAR as ACCYEAR  from masaccyearsettings where b.expdate between startdate and enddate) as Year, b.ponoid,mc.mcid,mc.MCATEGORY,mi.itemcode,mi.itemname,strength1 strength,b.batchno,b.expdate,w.warehousename, b.inwno,mi.itemid, 
                (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) expqty,nvl(sb.BASICRATENEW,sb.acibasicrate) basicrate,
                nvl(sb.ACICST,0) cst,nvl(sb.ACIVAT,0) vat,nvl(sb.acgstpvalue,0) gst,nvl(sb.SUPP,0) finalrate,
                ((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) *nvl(sb.BASICRATENEW,sb.acibasicrate)) as ExpValueBasic,
                ((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) *sb.SUPP ) as ExpValueGST,
                 round( (Round((b.EXPDate-nvl(case when t.receipttype = 'NO' then t.receiptdate else 
                 case when t.receipttype = 'RF' then nvl(f.receiptdate,mr.receiptdate) else
                 case when t.receipttype = 'RQ' then nvl(q.receiptdate,mr.receiptdate) else nr.norcptdate end end end,nrs.rd)),0)/Round((b.EXPDate-b.MFGDate),0))*100,2) shelflife
                 from tbreceiptbatches b 
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
                 inner join tbreceipts t on t.receiptid=i.receiptid
                 inner join masitems mi on mi.itemid=i.itemid
                 inner join masitemcategories c on c.categoryid=mi.categoryid
                 inner join masitemmaincategory mc on mc.MCID=c.MCID
                 inner join maswarehouses w  on w.warehouseid=t.warehouseid
                 left outer join soorderplaced so on so.ponoid = b.ponoid
               

                 left outer join
                 ( 
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0) + nvl(tbo.reconcile_qty,0)) issueqty  
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' 
                   and tb.issuetype not in ('EX') 
                   and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   group by tbi.itemid,tb.warehouseid,tbo.inwno 
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid 

                  left outer join 
                  (
                    select distinct c.ponoid,ri.itemid,c.inwno,si.basicrate,aci.basicrate acibasicrate,aci.basicratenew,c.batchno
                   ,case when  (r.receiptdate < '01-Jul-2017' ) then coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.exciseduty,0))/100) ),2),si.singleunitprice) else 
                   (case when aci.gstflag='Y' then aci.finalrategst else coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.ExciseDuty,0))/100) ),2),si.singleunitprice) end) end supp ,
                   case when aci.cstvat ='CST' then aci.percentvalue  else 0 end ACICST,  
                   case when aci.cstvat ='VAT' then aci.percentvalue  else 0 end acivat,  
                   case when aci.gstflag='Y' then nvl(aci.percentvaluegst,0) else 0 end acgstpvalue   from tbreceipts r 
                   inner join  tbreceiptitems ri on ri.receiptid = r.receiptid
                   inner join tbreceiptbatches c on  c.receiptitemid =ri.receiptitemid
                   left outer join tboutwards tbo on tbo.inwno = c.inwno
                   left outer join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
                   left outer join tbindents t on t.indentid = tbi.indentid
                   left outer join soorderplaced s on s.ponoid=c.ponoid
                   inner join soordereditems si on  si.ponoid=c.ponoid and si.itemid=ri.itemid   
                   inner join  aoccontractitems aci on aci.contractitemid=si.contractitemid
                   inner join aoccontracts ac on ac.contractid=aci.contractid
                   where r.status='C' 
                   and c.notindpdmis is null 
                   group by c.ponoid,ri.itemid,c.inwno,si.basicrate,t.indentdate,aci.basicrate,
                   c.batchno,si.singleunitprice,si.percentvalue,si.exciseduty,aci.basicratenew,aci.basicrate,aci.percentvalue,aci.percentvaluegst,aci.cstvat,
                   r.receiptdate,aci.gstflag,aci.singleunitprice,aci.singleunitprice,aci.finalrategst       
                  ) sb on sb.inwno=b.inwno and sb.itemid=i.itemid
                  left outer join
                  (
                    select distinct s.receipttype, i.itemid, tb.ponoid, tb.inwno,tb.batchno,t.receiptdate,t.warehouseid ,t.fromwarehouseid,t.transferid ,s.transferid stransferid,s.receipttype norcpttype,s.receiptdate norcptdate               
                                                from tbreceipts t 
                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid) 
                                                inner join
                                                (
                                                   select  tb.warehouseid,tbi.itemid,tbo.inwno,b.batchno,b.ponoid ,tb.transferid,t.receipttype,t.receiptdate
                                                   from tboutwards tbo 
                                                   inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
                                                   inner join tbindents tb on tb.indentid=tbi.indentid
                                                   inner join tbreceiptbatches b on b.inwno = tbo.inwno
                                                   inner join tbreceiptitems i on i.receiptitemid = b.receiptitemid
                                                   inner join tbreceipts t on t.receiptid = i.receiptid
                                                   where  tb.status = 'C'  and t.receipttype = 'NO' 
                                                ) s on s.transferid = t.transferid and s.ponoid = tb.ponoid and s.batchno = tb.batchno 
                                                where t.status = 'C'
                  ) nr on nr.ponoid = b.ponoid and nr.batchno = b.batchno and nr.warehouseid = t.warehouseid and nr.inwno = b.inwno

                  left outer join
                  (
                  select distinct s.receipttype, i.itemid, tb.ponoid, tb.inwno,tb.batchno,t.receiptdate,t.warehouseid ,t.fromwarehouseid,t.transferid ,s.transferid stransferid,
                    s.receipttype norcpttype,s.receiptdate norcptdate,nrs.receipttype rt,max(nrs.receiptdate) rd,  nrs.warehouseid nwid            
                                                from tbreceipts t 
                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid) 
                                                inner join
                                                (
                                                   select  tb.warehouseid,t.fromwarehouseid,tbi.itemid,tbo.inwno,b.batchno,b.ponoid ,tb.transferid,t.receipttype,t.receiptdate
                                                   from tboutwards tbo 
                                                   inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
                                                   inner join tbindents tb on tb.indentid=tbi.indentid
                                                   inner join tbreceiptbatches b on b.inwno = tbo.inwno
                                                   inner join tbreceiptitems i on i.receiptitemid = b.receiptitemid
                                                   inner join tbreceipts t on t.receiptid = i.receiptid
                                                   where  tb.status = 'C'  and t.receipttype = 'SP' 
                                                ) s on s.transferid = t.transferid and s.ponoid = tb.ponoid and s.batchno = tb.batchno 
                                                left outer join
                                                (
                                                select t.warehouseid,i.itemid,b.inwno,b.batchno,b.ponoid ,t.receipttype,t.receiptdate
                                                   from tbreceiptbatches b 
                                                   inner join tbreceiptitems i on i.receiptitemid = b.receiptitemid
                                                   inner join tbreceipts t on t.receiptid = i.receiptid
                                                   where  t.status = 'C'  and t.receipttype = 'NO' 
                                                ) nrs on nrs.ponoid = s.ponoid and nrs.batchno = s.batchno and nrs.warehouseid = s.fromwarehouseid
                                                where t.status = 'C' 
                                                 group by s.receipttype, i.itemid, tb.ponoid, tb.inwno,tb.batchno,t.receiptdate,t.warehouseid ,t.fromwarehouseid,
                                                t.transferid ,s.transferid,s.receipttype ,s.receiptdate ,nrs.receipttype,nrs.warehouseid
                  ) nrs on nrs.inwno = b.inwno

                  left outer join
                 (
                    select distinct t.warehouseid,tr.receipttype,tr.receiptdate,b.batchno,b.ponoid,i.itemid from tbindents t
                    inner join tbindentitems i on i.indentid = t.indentid
                    inner join tboutwards o on o.indentitemid = i.indentitemid
                    inner join tbreceiptbatches b on b.inwno = o.inwno
                    inner join tbreceiptitems ti on ti.receiptitemid = b.receiptitemid
                    inner join tbreceipts tr on tr.receiptid = ti.receiptid
                    where tr.receipttype = 'NO' and t.facilityid is not null 
                 ) f on f.warehouseid = t.warehouseid and f.batchno = b.batchno and f.ponoid = b.ponoid and f.itemid = mi.itemid

                 left outer join
                 (
                   select distinct t.warehouseid,tr.receipttype,tr.receiptdate,b.batchno,b.ponoid,i.itemid from tbindents t
                    inner join tbindentitems i on i.indentid = t.indentid
                    inner join tboutwards o on o.indentitemid = i.indentitemid
                    inner join tbreceiptbatches b on b.inwno = o.inwno
                    inner join tbreceiptitems ti on ti.receiptitemid = b.receiptitemid
                    inner join tbreceipts tr on tr.receiptid = ti.receiptid
                    where t.issuetype in ('QA') and tr.receipttype = 'NO' 
                 ) q on q.ponoid = b.ponoid and q.batchno = b.batchno and q.itemid = mi.itemid and q.warehouseid = t.warehouseid  
                 left outer join
                      (
                        select max(t.warehouseid) warehouseid,i.itemid,b.batchno,b.ponoid ,t.receipttype,max(t.receiptdate) receiptdate
                        from tbreceiptbatches b 
                        inner join tbreceiptitems i on i.receiptitemid = b.receiptitemid
                        inner join tbreceipts t on t.receiptid = i.receiptid
                        where  t.status = 'C'  and t.receipttype = 'NO' 
                        group by i.itemid,b.batchno,b.ponoid ,t.receipttype
                        ) mr on mr.ponoid = b.ponoid and mr.batchno = b.batchno and mr.itemid = mi.itemid
                 where  t.status = 'C'  and mc.mcid = " + mcid + @"
                 and nvl(b.whissueblock,0) in (0) and b.expdate between '01-APR-13' and sysdate
                 and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null
                 and (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) 
                 else (case when mi.qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) >0
                 and round( (Round((b.EXPDate-nvl(case when t.receipttype = 'NO' then t.receiptdate else 
                 case when t.receipttype = 'RF' then nvl(f.receiptdate,mr.receiptdate) else
                 case when t.receipttype = 'RQ' then nvl(q.receiptdate,mr.receiptdate) else nr.norcptdate end end end,nrs.rd)),0)/Round((b.EXPDate-b.MFGDate),0))*100,2)>=80
                 ) group by Year, mcid,MCATEGORY order by Year desc";
            var myList = _context.YearWiseExpiredDbset
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        [HttpGet("getPOAlertBaedOnAI")]
        public async Task<ActionResult<IEnumerable<POAlertAIDTO>>> getPOAlertBaedOnAI(string mcid, string hodid)
        {
            FacOperations f = new FacOperations(_context);

            string yearid = f.getACCYRSETID();
            string whhoid = "";
            string whhodcluase = "";
            if (hodid == "367")
            {
                whhoid = " sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) ";
                whhodcluase = " and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 ";
            }
            else if (hodid == "371")
            {
                whhoid = " sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) ";
                whhodcluase = " and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 ";
            }
            else
            {
                whhoid = " sum(nvl(i.DME_INDENTQTY, 0)) ";
                whhodcluase = " and (nvl(i.DME_INDENTQTY, 0))>0 ";
            }
            string qry = @"  select ROW_NUMBER() OVER ( ORDER BY MCID,RCStatusValue,EDLtypeValue ) AS ID,MCID,RCStatus,EDLtype,sum(EDL)+sum(NEDL) as total,sum(tobePOCount) as tobePOCount,sum(stockout) as stockout,
 sum(OnlyQC) as OnlyQC,sum(OnlyPipeline) as OnlyPipeline,
 EDLtypeValue,RCStatusValue from 
(
select m.itemid,mc.MCID,m.itemcode,m.itemname,case when m.isedl2021 = 'Y' then 'EDL' else 'Non EDL' end as EDLtype,
case when m.isedl2021 = 'Y' then 1 else 0 end as EDLtypeValue,
case when r.itemid is not null then 1 else 0 end as RCStatusValue,

DHSAI,nvl(POQTY,0) as POQTY,nvl(DHSAI-nvl(POQTY,0),0) as BalanceIndentPO,
nvl(ReadyForIssue,0) as ReadyForIssue ,
nvl(Pending,0) as UnderQC,nvl(pip.pipelineQTY,0) as pipelineQTY,
case when m.isedl2021 = 'Y' then 1 else 0 end as EDL, case when m.isedl2021 = 'Y' then 0 else 1 end as NEDL 
,round(DHSAI/4,0) as DHS3MonthAI,round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) as buffavl
,case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then 
round(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end as tobePO,
case when (case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then 
round(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end)>nvl(DHSAI-nvl(POQTY,0),0)
then nvl(DHSAI-nvl(POQTY,0),0) else (case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then 
round(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end) end actualtobePO
,case when (nvl(ReadyForIssue,0)+nvl(Pending,0)+nvl(pip.pipelineQTY,0))=0  then 1 else 0 end as stockout
,case when r.itemid is not null then 'Yes' else 'No' end as RCStatus
,case when (case when (case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then 
round(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end)>nvl(DHSAI-nvl(POQTY,0),0)
then nvl(DHSAI-nvl(POQTY,0),0) else (case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then 
round(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end) end )>0 then 1 else 0 end as tobePOCount
,nvl(AIReturn,0) Return
,case when nvl(ReadyForIssue,0)=0 and nvl(Pending,0) >0 then 1 else 0 end as OnlyQC
,case when nvl(ReadyForIssue,0)=0 and nvl(Pending,0) =0 and nvl(pip.pipelineQTY,0)>0 then 1 else 0 end as OnlyPipeline
from masitems m 
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join 
(
select distinct itemid from v_rcvalid
) r on r.itemid = m.itemid
 inner join 
(
select itemid," + whhoid + @" as DHSAI,case when i.isaireturn ='Y' then 1 else 0 end as AIReturn from itemindent i  where 1 = 1 " + whhodcluase + @" and
accyrsetid = " + yearid + @"
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid

left outer join 
(
select oi.itemid,sum(ABSQTY) as POQTY  from  soOrderedItems oi
inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' )) 
where DEPTID=" + hodid + @" and AIFINYEAR=" + yearid + @"
group by oi.itemid
) p on p.itemid=m.itemid
left outer join 
(
select itemid,sum(nvl(ReadyForIssue,0)) as ReadyForIssue,sum(nvl(Pending,0)) as Pending from 
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending,
 ci.Finalrategst
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems mi on mi.itemid= i.itemid
 inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
 inner join soordereditems odi on odi.ponoid = b.ponoid
 inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C' and mc.mcid=1   And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate
     ) group by itemid
) st on st.itemid=m.itemid


  left outer join
(
  select itemid, sum(pipelineQTY) pipelineQTY from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid
 where op.status  in ('C','O') 
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having (case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) group by itemid
) pip on pip.itemid = m.itemid

where  m.ISFREEZ_ITPR is null  and mc.mcid=" + mcid + @" and nvl(AIReturn,0)=0 
) group by RCStatus,EDLtype,EDLtypeValue,RCStatusValue,MCID
order by EDLtype,(RCStatusValue+EDLtypeValue)  desc  ";

            var myList = _context.POAlertAIDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getPOStockoutQCPipe_Details")]
        public async Task<ActionResult<IEnumerable<POAlertDetailsDTO>>> getPOStockoutQCPipe_Details(string mcid, string hodid, string edlvalue, string rcvalue, string rpttype)
        {
            FacOperations f = new FacOperations(_context);

            string yearid = f.getACCYRSETID();
            string whhoid = "";
            string whhodcluase = "";
            if (hodid == "367")
            {
                whhoid = " sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) ";
                whhodcluase = " and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 ";
            }
            else if (hodid == "371")
            {
                whhoid = " sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) ";
                whhodcluase = " and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 ";
            }
            else
            {
                whhoid = " sum(nvl(i.DME_INDENTQTY, 0)) ";
                whhodcluase = " and (nvl(i.DME_INDENTQTY, 0))>0 ";
            }
            string whedlcaluse = " and(case when m.isedl2021 = 'Y' then 1 else 0 end)= 0";
            if (edlvalue == "1")
            {
                whedlcaluse = " and(case when m.isedl2021 = 'Y' then 1 else 0 end)= 1";
            }
            string whrcclause = " and (case when r.itemid is not null then 1 else 0 end)= 0";
            if (rcvalue == "1")
            {
                whrcclause = " and (case when r.itemid is not null then 1 else 0 end)=1";
            }
            string whrpttypeclause = "";
            if (rpttype == "0")
            {
                //PO alert
                whrpttypeclause = " and (case when (case when (case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then \r\nround(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end)>nvl(DHSAI-nvl(POQTY,0),0)\r\nthen nvl(DHSAI-nvl(POQTY,0),0) else (case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then \r\nround(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end) end )>0 then 1 else 0 end)=1";
            }
            else if (rpttype == "1")
            {
                //stockout
                whrpttypeclause = " and (case when(nvl(ReadyForIssue, 0)+nvl(Pending, 0) + nvl(pip.pipelineQTY, 0))= 0  then 1 else 0 end) =1";

            }
            else if (rpttype == "2")
            {
                //onlyQC
                whrpttypeclause = " and (case when nvl(ReadyForIssue,0)=0 and nvl(Pending,0) >0 then 1 else 0 end) =1";

            }
            else if (rpttype == "3")
            {
                //onlyPipeline
                whrpttypeclause = " and (case when nvl(ReadyForIssue,0)=0 and nvl(Pending,0) =0 and nvl(pip.pipelineQTY,0)>0 then 1 else 0 end) =1";

            }
            else
            {

            }



            string qry = @" select ROW_NUMBER() OVER ( ORDER BY m.itemname ) AS ID,m.itemcode,m.itemname,
DHSAI,nvl(POQTY,0) as POQTY,nvl(DHSAI-POQTY,0) as BalanceIndentPO,
nvl(ReadyForIssue,0) as ReadyForIssue ,nvl(Pending,0) as UnderQC,nvl(pip.pipelineQTY,0) as pipelineQTY
,round(DHSAI/4,0) as DHS3MonthAI,round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) as Qauter1_MStock
,case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then 
round(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end as tobePO,
case when (case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then 
round(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end)>nvl(DHSAI-nvl(POQTY,0),0)
then nvl(DHSAI-nvl(POQTY,0),0) else (case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then 
round(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end) end actualtobePO
,case when (nvl(ReadyForIssue,0)+nvl(Pending,0)+nvl(pip.pipelineQTY,0))=0  then 1 else 0 end as stockout
,case when r.itemid is not null then 'Yes' else 'No' end as RCStatus
,case when (case when (case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then 
round(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end)>nvl(DHSAI-nvl(POQTY,0),0)
then nvl(DHSAI-nvl(POQTY,0),0) else (case when (round(DHSAI/4,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)))>0 then 
round(DHSAI/2,0)-(nvl(ReadyForIssue,0)+ nvl(Pending,0)+nvl(pip.pipelineQTY,0)) else 0 end) end )>0 then 1 else 0 end as tobePOCount
,nvl(AIReturn,0) Return,m.itemid,mc.MCID,case when m.isedl2021 = 'Y' then 1 else 0 end as EDLtypeValue,
case when r.itemid is not null then 1 else 0 end as RCStatusValue,case when m.isedl2021 = 'Y' then 1 else 0 end as EDL, case when m.isedl2021 = 'Y' then 0 else 1 end as NEDL 
,m.strength1,case when m.isedl2021 = 'Y' then 'EDL' else 'Non EDL' end as EDLtype,m.unit
,mt.qcdayslab,mt.itemtypename
, GET_RCSupplier(m.itemid) as sup
,RCRate,RCStartDT,RCEndDT
,case when nvl(ReadyForIssue,0)=0 and nvl(Pending,0) >0 then 1 else 0 end as OnlyQC
,case when nvl(ReadyForIssue,0)=0 and nvl(Pending,0) =0 and nvl(pip.pipelineQTY,0)>0 then 1 else 0 end as OnlyPipeline
from masitems m 
left outer join masitemtypes mt on mt.itemtypeid = m.itemtypeid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join 
(
select itemid,Round(min(FINALRATEGST),2) as RCRate,min(RCSTART) RCStartDT,min(RCENDDT) as RCEndDT from v_rcvalid
group by itemid
) r on r.itemid = m.itemid
inner join 
(
select itemid," + whhoid + @" as DHSAI,case when i.isaireturn ='Y' then 1 else 0 end as AIReturn from 
itemindent i  where 1 = 1 " + whhodcluase + @" and
accyrsetid = " + yearid + @"
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid

left outer join 
(
select oi.itemid,sum(ABSQTY) as POQTY  from  soOrderedItems oi
inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' )) 
where DEPTID=" + hodid + @" and AIFINYEAR=" + yearid + @"
group by oi.itemid
) p on p.itemid=m.itemid
left outer join 
(
select itemid,sum(nvl(ReadyForIssue,0)) as ReadyForIssue,sum(nvl(Pending,0)) as Pending from 
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending,
 ci.Finalrategst
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems mi on mi.itemid= i.itemid
 inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
 inner join soordereditems odi on odi.ponoid = b.ponoid
 inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C' and mc.mcid=1   And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate
     ) group by itemid
) st on st.itemid=m.itemid
  left outer join
(
  select itemid, sum(pipelineQTY) pipelineQTY from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid
 where op.status  in ('C','O') 
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having (case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) group by itemid
) pip on pip.itemid = m.itemid
where  m.ISFREEZ_ITPR is null  " + whedlcaluse + @"
and mc.mcid=" + mcid + @" and nvl(AIReturn,0)=0 
" + whrcclause + @"
" + whrpttypeclause + @"  ";
            var myList = _context.POAlertDetailsDbSet
        .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("PublicCGMSCStockItems")]
        public async Task<ActionResult<IEnumerable<StockitemsDTO>>> PublicCGMSCStockItems(string mcid)
        {
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid=" + mcid;
            }

            string qry = @" select m.itemid as id, 
m.itemname || '-' || m.itemcode || '-' || nvl(ty.itemtypename, 'NA') || '-' || nvl(g.groupname, 'NA') as details
,case when m.isedl2021 = 'Y' then 1 else 0 end as edltype
from masitems m
left outer join masitemgroups g on g.groupid = m.groupid
left outer join masitemtypes ty on ty.itemtypeid = m.itemtypeid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
where m.ISFREEZ_ITPR is null " + whmcid + @"  order by(case when m.isedl2021 = 'Y' then 0 else 1 end),g.groupname desc ";
            var myList = _context.StockitemsDbSet
     .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }





            [HttpGet("PublicCGMSCStockItemsWithSTock")]
        public async Task<ActionResult<IEnumerable<StockitemsDTO>>> PublicCGMSCStockItemsWithSTock(string mcid)
        {
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid=" + mcid;
            }
            string qry = @" select m.itemid as id, 
m.itemname ||'-'||m.itemcode||'-'||nvl(ty.itemtypename,'NA')||'-'||nvl(g.groupname,'NA')||':'||(nvl(st.ready,0)+nvl(st.UQC,0)+nvl(pip.TOTLPIPELINE,0)) as details
,case when m.isedl2021='Y' then 1 else 0 end as edltype
from masitems m
left outer join masitemgroups g on g.groupid=m.groupid
left outer join masitemtypes ty on ty.itemtypeid=m.itemtypeid
left outer join 
(
select itemid,sum(nvl(ReadyForIssue,0)) as READY,sum(nvl(Pending,0)) as UQC from 
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems mi on mi.itemid= i.itemid
 inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
 inner join soordereditems odi on odi.ponoid = b.ponoid
 inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C' and mc.mcid=1   And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate
     ) group by itemid
) st on st.itemid=m.itemid


left outer join
(
  select itemid, sum(pipelineQTY) TOTLPIPELINE from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid
 where op.status  in ('C','O') 
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having (case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) group by itemid
) pip on pip.itemid = m.itemid

inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
where m.ISFREEZ_ITPR is null and (nvl(st.ready,0)+nvl(st.UQC,0)+nvl(pip.TOTLPIPELINE,0))>0  " + whmcid + @"
order by (case when m.isedl2021='Y' then 0 else 1 end),g.groupname desc
 ";
            var myList = _context.StockitemsDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("PublicCGMSCStock")]
        public async Task<ActionResult<IEnumerable<CGMSCPublicStockDTO>>> PublicCGMSCStock(string mcid)
        {
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid=" + mcid;
            }
            string qry = @" select m.itemid as id, m.itemname ||'('||m.itemcode||')' as name,
m.Unit||',Ready:'||nvl(st.ready,0)||',UQC:'||nvl(st.UQC,0)||',In-transit:'||nvl(pip.TOTLPIPELINE,0)||',Group:'||nvl(g.groupname,'NA') as details
,case when m.isedl2021='Y' then 1 else 0 end as edltype
from masitems m
left outer join masitemgroups g on g.groupid=m.groupid
left outer join 
(
select itemid,sum(nvl(ReadyForIssue,0)) as READY,sum(nvl(Pending,0)) as UQC from 
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems mi on mi.itemid= i.itemid
 inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
 inner join soordereditems odi on odi.ponoid = b.ponoid
 inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C' and mc.mcid=1   And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate
     ) group by itemid
) st on st.itemid=m.itemid


left outer join
(
  select itemid, sum(pipelineQTY) TOTLPIPELINE from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid
 where op.status  in ('C','O') 
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having (case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) group by itemid
) pip on pip.itemid = m.itemid

inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
where m.ISFREEZ_ITPR is null and (nvl(st.ready,0)+nvl(st.UQC,0)+nvl(pip.TOTLPIPELINE,0))>0 " + whmcid + @"
order by (case when m.isedl2021='Y' then 0 else 1 end),g.groupname desc ";
            var myList = _context.CGMSCPublicStockDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }




        [HttpGet("SearchRCValid")]
        public async Task<ActionResult<IEnumerable<CGMSCPublicStockDTO>>> SearchRCValid(string mcid)
        {
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid=" + mcid;
            }
            string qry = @" select contractitemid as id,itemname ||'('||itemcode||')'||itemtypename||',SKU:'||Unit||',Rate:'||to_char(finalrategst) as name, 
strength1||','||suppliername||',StartDT:'||RCSTART||',EndDT:'||RCENDDT||',Group:'||nvl(groupname,'NA') as details,case when isedl2021='Y' then 1 else 0 end as edltype
from (
select m.itemcode,ms.schemecode,s.suppliername||' '||case when ci.rankpercetage is not null then ci.rankid else '' end ||' '|| case when ci.rankpercetage is not null then to_char(ci.rankpercetage) else '' end||to_char((case when ci.rankpercetage is not null then '%' else '' end )) as suppliername,
to_char(a.ContractStartDate,'dd-MM-yyyy') rcstart,
a.ContractEndDate,case when (ci.isextended='Y') then to_char(ci.rcextendedupto,'dd-MM-yyyy') else to_char(ContractEndDate,'dd-MM-yyyy') end RCEndDT,
round(Months_Between(to_date(a.ContractEndDate),to_date(a.ContractStartDate)),0) as RCDuration
,round(Months_Between(to_date(sysdate),to_date(a.ContractStartDate)),0) as TotalDurFromStart
,ci.rcextendedfrom,ci.rcextendedupto,ci.isextended ,a.contractid,ci.contractitemid,
 case when ci.basicratenew is null then ci.basicrate else basicratenew end basicrate,
 ci.finalrategst
 ,
                               case when   ci.percentvaluegst is null then   ci.percentvalue  else ci.percentvaluegst end Tax   
                            ,   ci.itemid,ms.schemeid,s.supplierid
                               ,m.categoryid
,case when m.isedl2021= 'Y' then 'Y' else 'N' end as isedl2021   
, case when sysdate between s.blacklistfdate and  s.blacklisttdate then 'Yes' else 'No' end as blacklisted,
case when (ci.isextended='Y') then round(rcextendedupto-sysdate,0) else  round(ContractEndDate-sysdate,0)  end as DayRemaining
,g.groupname,m.unit,m.strength1,m.itemname,ty.itemtypename
from aoccontractitems ci
inner join aoccontracts a on a.contractid=ci.contractid
inner join massuppliers s on s.supplierid=a.supplierid
inner join masschemes ms on ms.schemeid=a.schemeid
inner join masitems m on m.itemid=ci.itemid
left outer join masitemtypes ty on ty.ITEMTYPEID=m.ITEMTYPEID
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join masitemgroups g on g.groupid=m.groupid
where  ForceCloseRC is null  and a.status = 'C' and s.blacklistfdate is null  
and  (sysdate between a.contractstartdate and a.contractenddate or 
sysdate between a.contractstartdate and  (case when ci.isextended='Y' then ci.rcextendedupto else a.contractenddate end))
and round(Months_Between(to_date(a.ContractEndDate),to_date(a.ContractStartDate)),0)<=nvl(( case when m.categoryid=62 then  nvl(ms.ISREAGDURATION,36) else 36 end) ,36)
" + whmcid + @"
) where blacklisted='No' 
order by itemname ";
            var myList = _context.CGMSCPublicStockDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }




        [HttpGet("NearExpReport")]
        public async Task<ActionResult<IEnumerable<NearExpDTO>>> NearExpReport(string mcid, string nexppara)
        {
            string whmcidclause = "";
            if (mcid != "0")
            {

                whmcidclause = " and  mc.mcid =" + mcid;
            }

            FacOperations f = new FacOperations(_context);
            string curryearmonth = f.getCurrYearmonth();
            FacOperations f2 = new FacOperations(_context);
            string toyearmonth = f2.getNextYearMonth(Convert.ToInt64(nexppara));

            string qry = @" select Expirymonth,count(distinct itemcode) noofitems,count(distinct batchno) noofbatches,round(sum(ExValue)/100000,2) nearexpvalue,Expirymonth1 from 
(
select b.batchno,ci.finalrategst,b.expdate, to_char(b.expdate,'MM-YYYY') as Expirymonth,to_char(b.expdate,'YYYY-MM') as Expirymonth1,
w.warehousename,mc.mcid,mc.MCATEGORY, mi.itemcode, mi.itemname, b.inwno,mi.itemid, 
(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) expqty,
((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) *ci.finalrategst ) as ExValue

from tbreceiptbatches b 
inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
inner join tbreceipts t on t.receiptid=i.receiptid
inner join masitems mi on mi.itemid=i.itemid
inner join masitemcategories c on c.categoryid=mi.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
inner join masedl me on me.edlcat=mi.edlcat
inner join masitemcategories mc on mc.categoryid=mi.categoryid
inner join soorderplaced o on o.ponoid = b.ponoid
inner join maswarehouses w  on w.warehouseid=t.warehouseid
left outer join soorderplaced so on so.ponoid = b.ponoid
inner join soordereditems odi on odi.ponoid = so.ponoid
inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
left outer join
( 
select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0) + nvl(tbo.reconcile_qty,0)) issueqty  
from tboutwards tbo, tbindentitems tbi , tbindents tb
where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' 
and tb.issuetype not in ('EX') 
and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
group by tbi.itemid,tb.warehouseid,tbo.inwno 
) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid 
where  t.status = 'C'  " + whmcidclause + @"
and nvl(b.whissueblock,0) in (0) and to_char(b.expdate,'YYYY-MM') between '" + curryearmonth + @"' and '" + toyearmonth + @"'
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null
and (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) >0

) group by Expirymonth,Expirymonth1 order by Expirymonth1 ";
            var myList = _context.NearExpDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("NearExpReportDrugs")]
        public async Task<ActionResult<IEnumerable<NearExpDTOItems>>> NearExpReportDrugs(string mcid, string nexppara, string expmonth)
        {
            string whmcidclause = "";
            if (mcid != "0")
            {

                whmcidclause = " and  mc.mcid =" + mcid;
            }
            //if(expmonth==)

            FacOperations f = new FacOperations(_context);
            string curryearmonth = f.getCurrYearmonth();
            FacOperations f2 = new FacOperations(_context);
            string toyearmonth = f2.getNextYearMonth(Convert.ToInt64(nexppara));



            string qry = @"   select itemid,itemcode, itemname, count(distinct batchno) noofbatches,round(sum(ExValue) / 100000, 2) nearexpvalue,sum(expqty) as QTY from
(
select b.batchno, ci.finalrategst, b.expdate, to_char(b.expdate,'MM-YYYY') as Expirymonth,to_char(b.expdate, 'YYYY-MM') as Expirymonth1,
w.warehousename,mc.mcid,mc.MCATEGORY, mi.itemcode, mi.itemname, b.inwno,mi.itemid, 
(case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) expqty,
((case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) *ci.finalrategst ) as ExValue
from tbreceiptbatches b
inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
inner join tbreceipts t on t.receiptid = i.receiptid
inner join masitems mi on mi.itemid = i.itemid
inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
inner join masedl me on me.edlcat = mi.edlcat
inner join masitemcategories mc on mc.categoryid = mi.categoryid
inner join soorderplaced o on o.ponoid = b.ponoid
inner join maswarehouses w  on w.warehouseid = t.warehouseid
left outer join soorderplaced so on so.ponoid = b.ponoid
inner join soordereditems odi on odi.ponoid = so.ponoid
inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
left outer join
(
select  tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty, 0) + nvl(tbo.reconcile_qty, 0)) issueqty
from tboutwards tbo, tbindentitems tbi, tbindents tb
where  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C'
and tb.issuetype not in ('EX')
and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
group by tbi.itemid, tb.warehouseid, tbo.inwno
) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
where t.status = 'C'  " + whmcidclause + @"
and nvl(b.whissueblock,0) in (0) and to_char(b.expdate,'YYYY-MM') between '" + curryearmonth + @"' and '" + toyearmonth + @"'
and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null
and (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) > 0
and to_char(b.expdate,'MM-YYYY')= '" + expmonth + @"'
) group by itemcode,itemname,itemid
order by round(sum(ExValue)/ 100000,2) desc ";


            var myList = _context.NearExpItemsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }
        [HttpGet("NearExpReportbatch")]
        public async Task<ActionResult<IEnumerable<NearExpBatchWHDTO>>> NearExpReportbatch(string mcid, string nexppara, string expmonth)
        {
            string whmcidclause = "";
            if (mcid != "0")
            {

                whmcidclause = " and  mc.mcid =" + mcid;
            }
            //if(expmonth==)

            FacOperations f = new FacOperations(_context);
            string curryearmonth = f.getCurrYearmonth();
            FacOperations f2 = new FacOperations(_context);
            string toyearmonth = f2.getNextYearMonth(Convert.ToInt64(nexppara));


            string qry = @"   select itemid,itemcode, '' as WH,itemname,batchno,expdate,round(sum(ExValue) / 100000, 2) nearexpvalue,sum(expqty) as QTY from
(
select b.batchno, ci.finalrategst, b.expdate, to_char(b.expdate,'MM-YYYY') as Expirymonth,to_char(b.expdate, 'YYYY-MM') as Expirymonth1,
w.warehousename,mc.mcid,mc.MCATEGORY, mi.itemcode, mi.itemname, b.inwno,mi.itemid, 
(case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) expqty,
((case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) *ci.finalrategst ) as ExValue
from tbreceiptbatches b
inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
inner join tbreceipts t on t.receiptid = i.receiptid
inner join masitems mi on mi.itemid = i.itemid
inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
inner join masedl me on me.edlcat = mi.edlcat
inner join masitemcategories mc on mc.categoryid = mi.categoryid
inner join soorderplaced o on o.ponoid = b.ponoid
inner join maswarehouses w  on w.warehouseid = t.warehouseid
left outer join soorderplaced so on so.ponoid = b.ponoid
inner join soordereditems odi on odi.ponoid = so.ponoid
inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
left outer join
(
select  tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty, 0) + nvl(tbo.reconcile_qty, 0)) issueqty
from tboutwards tbo, tbindentitems tbi, tbindents tb
where  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C'
and tb.issuetype not in ('EX')
and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
group by tbi.itemid, tb.warehouseid, tbo.inwno
) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
where t.status = 'C'  " + whmcidclause + @"
and nvl(b.whissueblock,0) in (0) and to_char(b.expdate,'YYYY-MM') between '" + curryearmonth + @"' and '" + toyearmonth + @"'
and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null
and (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) > 0
and to_char(b.expdate,'MM-YYYY')= '" + expmonth + @"'
) group by itemcode,itemname,itemid,batchno,expdate
order by round(sum(ExValue)/ 100000,2) desc ";
            var myList = _context.NearExpBatchWHDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("NearExpReportbatchWH")]
        public async Task<ActionResult<IEnumerable<NearExpBatchWHDTO>>> NearExpReportbatchWH(string nexppara, string expmonth, string batchno, string itemid)
        {
            string whbatchno = "";
            if (batchno != "0")
            {

                whbatchno = " and  b.batchno= ='" + batchno + "'";
            }
            string whitemid = "";
            if (itemid != "0")
            {
                whitemid = " and mi.itemid=" + itemid;
            }
            //if(expmonth==)

            FacOperations f = new FacOperations(_context);
            string curryearmonth = f.getCurrYearmonth();
            FacOperations f2 = new FacOperations(_context);
            string toyearmonth = f2.getNextYearMonth(Convert.ToInt64(nexppara));


            string qry = @"   select itemid,itemcode, warehousename as WH,itemname,batchno,expdate,round(sum(ExValue) / 100000, 2) nearexpvalue,sum(expqty) as QTY from
(
select b.batchno, ci.finalrategst, b.expdate, to_char(b.expdate,'MM-YYYY') as Expirymonth,to_char(b.expdate, 'YYYY-MM') as Expirymonth1,
w.warehousename,mc.mcid,mc.MCATEGORY, mi.itemcode, mi.itemname, b.inwno,mi.itemid, 
(case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) expqty,
((case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) *ci.finalrategst ) as ExValue
from tbreceiptbatches b
inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
inner join tbreceipts t on t.receiptid = i.receiptid
inner join masitems mi on mi.itemid = i.itemid
inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
inner join masedl me on me.edlcat = mi.edlcat
inner join masitemcategories mc on mc.categoryid = mi.categoryid
inner join soorderplaced o on o.ponoid = b.ponoid
inner join maswarehouses w  on w.warehouseid = t.warehouseid
left outer join soorderplaced so on so.ponoid = b.ponoid
inner join soordereditems odi on odi.ponoid = so.ponoid
inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
left outer join
(
select  tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty, 0) + nvl(tbo.reconcile_qty, 0)) issueqty
from tboutwards tbo, tbindentitems tbi, tbindents tb
where  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C'
and tb.issuetype not in ('EX')
and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
group by tbi.itemid, tb.warehouseid, tbo.inwno
) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
where t.status = 'C'  " + whitemid + @"
and nvl(b.whissueblock,0) in (0) and to_char(b.expdate,'YYYY-MM') between '" + curryearmonth + @"' and '" + toyearmonth + @"'
and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null
and (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) > 0
and to_char(b.expdate,'MM-YYYY')= '" + expmonth + @"' " + whbatchno + @"
) group by itemcode,itemname,itemid,batchno,expdate,warehousename
order by round(sum(ExValue)/ 100000,2) desc ";
            var myList = _context.NearExpBatchWHDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



        [HttpGet("RCDetail")]
        public async Task<ActionResult<IEnumerable<RCDetailDTO>>> RCDetail(string itemid)
        {
            string qry = @"  select m.itemid,nvl(to_char(RCRate),'NA') as RCRate,nvl(RCStartDT,'NA') RCStartDT,nvl(RCEndDT,'NA')RCEndDT , nvl(GET_RCSupplier(m.itemid),'NA') as sup,nvl(s.schemecode,'NA') as schemecode ,
nvl(s.schemename,'NA') as schemename from masitems m
                        left outer join 
                        (
                        select itemid,Round(min(FINALRATEGST),2) as RCRate,min(RCSTART) RCStartDT,min(RCENDDT) as RCEndDT,min(SCHEMEID) as SCHEMEID from v_rcvalid
                        group by itemid
                        ) r on r.itemid = m.itemid
                          left outer join  masschemes s on s.SCHEMEID=r.SCHEMEID
                        where m.itemid=" + itemid + "  ";

            var myList = _context.RCDetailDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("itemIndentQuantity")]
        public async Task<ActionResult<IEnumerable<ItemIndentQtyDTO>>> itemIndentQuantity(string itemid, string fwhid, string distid, string userid, string coll_cmho)
        {
            FacOperations f = new FacOperations(_context);

            string qry = @"";

            string yearid = f.getACCYRSETID();
            string whid = "";
            string districtid = "";

            if (fwhid != "0")
            {
                whid = fwhid;
            }

            //string whDistId = "";
            //if (distid != "0")
            //{
            //    whDistId = " and d.districtid =" + distid;
            //}
        

            if (coll_cmho == "Collector")

            {
                districtid = f.getCollectorDisid(userid);
                string cmhoid = f.getCMHOFacFromDistrict(districtid);
                Int64 whiddata = f.getWHID(cmhoid.ToString());

                whid = Convert.ToString(whiddata);

            }
            if (coll_cmho == "CMHO")
            {
                districtid = Convert.ToString(f.geFACTDistrictid(userid));
                string cmhoid = f.getCMHOFacFromDistrict(districtid);
                Int64 whiddata = f.getWHID(cmhoid.ToString());

                whid = Convert.ToString(whiddata);

            }
            


            if (fwhid!="0" && whid != "0" && distid == "0")
            {
                qry = @" select m.itemname,m.itemcode,m.unit,case when mc.mcid=4 then 0 else nvl(DHSAI,0) end as DHSAI,nvl(iss.DHSIssue,0) as DHSIssue,
case when mc.mcid=4 then 'NA' else (to_char(case when (case when mc.mcid=4 then 0 else dhs.DHSAI end)>0 then Round(iss.DHSIssue/(case when mc.mcid=4 then 0 else dhs.DHSAI end)*100,2) else 100 end)||'%') end as DHSIssueP ,
nvl(dme.dmeai,0) as dmeai,nvl(iss.DMEIssue,0) as DMEIssue,case when mc.mcid=4 then 'NA' else to_char(case when (dme.dmeai)>0 then round(iss.DMEIssue/dme.dmeai*100,2) else 100 end)||'%' end  as DMEIssuePer
,case when mc.mcid=4 then nvl(dhs.DHSAI,0) else 0 end as AYUSHAI,nvl(iss.AYUSHIssue,0) AYUSHIssue
,case when (case when mc.mcid=4 then dhs.DHSAI else 0 end)>0 then to_char(Round(iss.AYUSHIssue/(case when mc.mcid=4 then dhs.DHSAI else 0  end)*100,2))||'%' else 'NA' end as AYUSHIssueper

from masitems m
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID

left outer   join 
(
select sum(AI) as dmeai, a.itemid,a.accyrsetid FROM v_institutionai a
inner join masfacilities f  on f.facilityid=a.facilityid 
inner join masdistricts d on d.districtid=f.districtid
where itemid = "+ itemid + @" and accyrsetid = " + yearid + @" and d.warehouseid = " + whid + @"
group by a.itemid,a.accyrsetid
) dme on dme.itemid=m.itemid

left outer join 
(
select  a.itemid,sum(a.rindentqty) as facilityindentqty
 ,sum(a.DHSAPRQTY) as DHSAIinNos
  ,round(sum(nvl(a.DHSAPRQTY,0))/nvl(m.unitcount,1),0) as DHSAI
from masannualindentdhs dma 
inner join masrevisedannualindent ma on ma.dhsaiid=dma.dhsaiid
inner join revisedannualindentitem a on a.RANNUALINDENTID=ma.RANNUALINDENTID 
inner join masitems m on m.itemid =a.itemid
inner join masfacilities f on f.facilityid=ma.facilityid
inner join masdistricts d on d.districtid=f.districtid
inner join maswarehouses wh on wh.warehouseid=d.warehouseid
where ma.status='C' and   ma.accyrsetid=" + yearid + @"  and dma.accyrsetid=" + yearid + @"  and m.itemid = " + itemid + @"
and nvl(a.rindentqty,0)>0  and ma.dhsaiid is not null and nvl(dma.ISCGMSCRECV,'N')='Y' and d.warehouseid="+ whid + @"
group by a.itemid,m.unitcount
) dhs on  dhs.itemid=m.itemid

left outer join 
(

select itemid,sum(AYUSHIssue) as AYUSHIssue,sum(DMEIssue) as DMEIssue,sum(DHSIssue) as DHSIssue
from 
(
  select  tbi.itemid,case when t.hodid =7 then  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end as AYUSHIssue,
  case when t.hodid =3 then  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end as DMEIssue,
    case when t.hodid not in (3,7) then  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end as DHSIssue
 from tbindents tb
 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  1=1 and tbi.itemid="+ itemid + @" and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + yearid + @" ) 
and (select enddate from masaccyearsettings where accyrsetid=" + yearid + @") 
group by tbi.itemid,t.hodid 
) group by itemid
) iss on iss.itemid=m.itemid
where m.itemid=" + itemid + @" ";

            }


          else if (fwhid == "0" && districtid != "0")
            {
                qry = @" select m.itemname,m.itemcode,m.unit,case when mc.mcid=4 then 0 else nvl(DHSAIinNos,0) end as DHSAI,nvl(iss.DHSIssue,0)*nvl(m.unitcount,1) as DHSIssue,
case when mc.mcid=4 then 'NA' else (to_char(case when (case when mc.mcid=4 then 0 else dhs.DHSAIinNos end)>0 then Round((iss.DHSIssue*nvl(m.unitcount,1))/(case when mc.mcid=4 then 0 else dhs.DHSAIinNos end)*100,2) else 100 end)||'%') end as DHSIssueP ,
nvl(dme.dmeai,0)*nvl(m.unitcount,1) as dmeai,nvl(iss.DMEIssue,0)*nvl(m.unitcount,1) as DMEIssue,case when mc.mcid=4 then 'NA' else to_char(case when (dme.dmeai)*nvl(m.unitcount,1)>0 then round((iss.DMEIssue)*nvl(m.unitcount,1)/((dme.dmeai)*nvl(m.unitcount,1))*100,2) else 100 end)||'%' end  as DMEIssuePer
,case when mc.mcid=4 then nvl(dhs.DHSAIinNos,0) else 0 end as AYUSHAI,nvl(iss.AYUSHIssue,0) AYUSHIssue
,case when (case when mc.mcid=4 then dhs.DHSAIinNos else 0 end)>0 then to_char(Round(iss.AYUSHIssue/(case when mc.mcid=4 then dhs.DHSAIinNos else 0  end)*100,2))||'%' else 'NA' end as AYUSHIssueper

from masitems m
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID

left outer   join 
(
select sum(AI) as dmeai, a.itemid,a.accyrsetid FROM v_institutionai a
inner join masfacilities f  on f.facilityid=a.facilityid 
inner join masdistricts d on d.districtid=f.districtid
where itemid = " + itemid + @" and accyrsetid = " + yearid + @" and d.warehouseid = " + whid + @" and d.districtid="+ districtid + @"
group by a.itemid,a.accyrsetid
) dme on dme.itemid=m.itemid

left outer join 
(
select  a.itemid,sum(a.rindentqty) as facilityindentqty
 ,sum(a.DHSAPRQTY) as DHSAIinNos
  ,round(sum(nvl(a.DHSAPRQTY,0))/nvl(m.unitcount,1),0) as DHSAI
from masannualindentdhs dma 
inner join masrevisedannualindent ma on ma.dhsaiid=dma.dhsaiid
inner join revisedannualindentitem a on a.RANNUALINDENTID=ma.RANNUALINDENTID 
inner join masitems m on m.itemid =a.itemid
inner join masfacilities f on f.facilityid=ma.facilityid
inner join masdistricts d on d.districtid=f.districtid
inner join maswarehouses wh on wh.warehouseid=d.warehouseid
where ma.status='C' and   ma.accyrsetid=" + yearid + @"  and dma.accyrsetid=" + yearid + @"  and m.itemid = " + itemid + @"
and nvl(a.rindentqty,0)>0  and ma.dhsaiid is not null and nvl(dma.ISCGMSCRECV,'N')='Y' and d.warehouseid=" + whid + @" and d.districtid="+ districtid + @"
group by a.itemid,m.unitcount
) dhs on  dhs.itemid=m.itemid

left outer join 
(

select itemid,sum(AYUSHIssue) as AYUSHIssue,sum(DMEIssue) as DMEIssue,sum(DHSIssue) as DHSIssue
from 
(
  select  tbi.itemid,case when t.hodid =7 then  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end as AYUSHIssue,
  case when t.hodid =3 then  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end as DMEIssue,
    case when t.hodid not in (3,7) then  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end as DHSIssue
 from tbindents tb
 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  1=1 and tbi.itemid=" + itemid + @" and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + yearid + @" ) 
and (select enddate from masaccyearsettings where accyrsetid=" + yearid + @") and f.districtid="+ districtid + @"
group by tbi.itemid,t.hodid 
) group by itemid
) iss on iss.itemid=m.itemid
where m.itemid=" + itemid + @" ";

            }
            else if(distid != "0" && whid == "0")
            {
                qry = "";
            }
            else
            {
                qry = @" select m.itemname,m.itemcode,m.unit,
case when mc.mcid=4 then 0 else nvl(i.DHSAI,0) end as DHSAI,DHSIssue, case when mc.mcid=4 then 'NA' else (
to_char(case when (case when mc.mcid=4 then 0 else i.DHSAI end)>0 then Round(DHSIssue/(case when mc.mcid=4 then 0 else i.DHSAI end)*100,2) else 100 end)||'%') end as DHSIssueP
,nvl(i.dmeai,0) dmeai,DMEIssue,case when mc.mcid=4 then 'NA' else to_char(case when (i.dmeai)>0 then round(DMEIssue/i.dmeai*100,2) else 100 end)||'%' end  as DMEIssuePer,
case when mc.mcid=4 then i.DHSAI else 0 end as AYUSHAI,AYUSHIssue
,case when (case when mc.mcid=4 then i.DHSAI else 0 end)>0 then to_char(Round(AYUSHIssue/(case when mc.mcid=4 then i.DHSAI else 0  end)*100,2))||'%' else 'NA' end as AYUSHIssueper

from masitems m
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
left outer join 
(
select nvl(sum(DHS_INDENTQTY),0)+nvl(sum(MITANIN),0) as DHSAI,nvl(sum(DME_INDENTQTY),0) as dmeai,i.itemid from itemindent i where  accyrsetid=" + yearid + @"
group by itemid
) i on i.itemid=m.itemid

left outer join 
(

select itemid,sum(AYUSHIssue) as AYUSHIssue,sum(DMEIssue) as DMEIssue,sum(DHSIssue) as DHSIssue
from 
(
  select  tbi.itemid,case when t.hodid =7 then  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end as AYUSHIssue,
  case when t.hodid =3 then  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end as DMEIssue,
    case when t.hodid not in (3,7) then  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end as DHSIssue
 from tbindents tb
 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + yearid + @" ) and (select enddate from masaccyearsettings where accyrsetid=" + yearid + @") 
group by tbi.itemid,t.hodid 
) group by itemid
) iss on iss.itemid=m.itemid
where m.itemid=  " + itemid + " ";
            }

           

            var myList = _context.ItemIndentQtyDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }




        [HttpGet("DHSIndentvsIssuance")]
        public async Task<ActionResult<IEnumerable<IndentvsIssuanceDTO>>> DHSIndentvsIssuance(string programid, string hodid)
        {
            FacOperations f = new FacOperations(_context);
            string whprogram = "";
            string qry = "";

            string yearid = f.getACCYRSETID();

            if (hodid == "1") //DHS
            {
                if (programid == "1") //Mitanin
                {

                    whprogram = " and nvl(m.ISMITANIN,0)=1";
                }
                if (programid == "2") //EDL
                {

                    whprogram = " and m.isedl2021='Y' ";
                }
                if (programid == "3") //Non EDL
                {

                    whprogram = " and (case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end)='Non EDL' ";
                }
                if (programid == "0") //Overall
                {

                    whprogram = " ";
                }

                qry = @" select mcid,MCATEGORY,count(itemid) as nos,sum(isscount) as isscount,Round(sum(isscount)/count(itemid)*100,2) issuedPer,sum(PhysicalStock) as nosstock,sum(OnlyPipeline) as nospipeline
,sum(RC) as rc,sum(Accepted) as accepted,sum(PriceOpened)as PriceOpened,sum(Evaluation) as Evaluation,sum(live) as live,sum(Tobe) as Tobe
from 
(

select mc.mcid, mc.MCATEGORY,m.itemcode,m.itemname,m.strength1,m.unit,
(nvl(DHS_INDENTQTY,0)+nvl(MITANIN,0)) as DHSAI,RUQC,
pipQty, case when nvl(RUQC,0)>0 then 1 else 0 end as PhysicalStock
, case when nvl(RUQC,0)=0 and nvl(pipQty,0) >0  then 1 else 0 end as OnlyPipeline
,case when r.ritemid is not null then 1 else 0 end as RC 
,case when r.ritemid is  null  and ac.ACCitemid is not null then 1 else 0 end as Accepted
,case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is not null then 1 else 0 end as PriceOpened
, case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is  null and evitemid is not null then 1 else 0 end as Evaluation
,case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is  null and evitemid is  null and litemid is not null then 1 else 0 end as live
,case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is  null and evitemid is  null and litemid is  null and toitemid is not null then 1 else 0 end as Tobe
, case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as EDLSttus
,m.ISDEDL , m.ISRFORLP,m.itemid,case when iss_qty>0 then 1 else 0 end as isscount
from masitems m
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
left outer join 
(
select sum(iss_qty) as iss_qty
,di.itemid 
from 
(
select tbi.itemid,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty 
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes ty on ty.facilitytypeid=f.facilitytypeid 
        where tb.status = 'C' and tb.issuetype='NO' and ty.facilitytypeid not in (371,378,364)
 and  tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + yearid + @" ) and (select enddate from masaccyearsettings where accyrsetid=" + yearid + @") 
        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        group by tbi.itemid

        ) di 
           group by di.itemid 

) iss on iss.itemid=m.itemid

left outer join
(
select distinct itemid as ritemid from v_rcvalid r
) r on r.ritemid=m.itemid

left outer join 
(
select t.itemid as ACCitemid from v_tenderstatusallnew t where t.actioncode like 'Accept%'
) ac on ac.ACCitemid=m.itemid


left outer join 
(
select itemid as pritemid from v_tenderstatusallnew t where t.actioncode like 'Price Op%'
) pr on pr.pritemid=m.itemid


left outer join 
(
select itemid as evitemid from v_tenderstatusallnew t where t.actioncode in ('Claim Objection in','Cover-A in','Cover-B in')
) ev on ev.evitemid=m.itemid

left outer join 
(
select itemid as litemid from v_tenderstatusallnew t where t.actioncode in ('Live in')
) l on l.litemid=m.itemid

left outer join 
(
select itemid as toitemid from v_tenderstatusallnew t where t.actioncode in ('To be Retender')
) ti on ti.toitemid=m.itemid

inner join itemindent i on i.itemid=m.itemid 
left outer join
(
select sum(nvl(READY,0))+sum(nvl(IWHPIPE,0))+sum(nvl(UQC,0)) as RUQC,sum(nvl(TOTLPIPELINE,0)) as pipQty,s.itemid from VWHSTOCKWITHEXP s 
inner join masitems m on m.itemid=s.itemid
group by s.itemid
) s on s.itemid=m.itemid

where 1=1 and i.accyrsetid=" + yearid + @" and (nvl(DHS_INDENTQTY,0)+nvl(MITANIN,0))>0
and IsFreez_ITPR is  null " + whprogram + @" and mc.mcid in (1,2,3)
) group by MCATEGORY,mcid
order by mcid ";

            }



            if (hodid == "2") //DME
            {

                if (programid == "1") //EDL
                {

                    whprogram = " and m.ISRFORLP is  null and m.isedl2021='Y' ";
                }
                if (programid == "2") //Non-EDl
                {

                    whprogram = " and m.ISRFORLP is  null and (case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end)='Non EDL' ";
                }
                if (programid == "3") //Above 1 Lakh
                {

                    whprogram = " and m.ISRFORLP is  null and ((nvl(DME_INDENTQTY,0))*nvl(SKUFINALRATE,0))>=100000 ";
                }
                if (programid == "4") //Below 1 Lakh
                {

                    whprogram = " and m.ISRFORLP is  null and ((nvl(DME_INDENTQTY,0))*nvl(SKUFINALRATE,0))<100000 ";
                }
                if (programid == "5") //Return to DME
                {

                    whprogram = " and m.ISRFORLP is not null  ";
                }
                if (programid == "0") //Overall
                {

                    whprogram = " and m.ISRFORLP is  null ";
                }

                qry = @" 

select  mcid,MCATEGORY,count(distinct itemid ) nos,sum(isscount) isscount,Round(sum(isscount)/count(distinct itemid)*100,2) issuedPer,sum(PhysicalStock) nosstock,sum(ONLYPIPELINE) nospipeline
,sum(RC) rc,sum(ACCEPTED) as accepted
,sum(PRICEOPENED) PriceOpened,sum(EVALUATION) Evaluation, sum(LIVE) LIVE,
sum(TOBE) Tobe from 
(
select distinct  mc.mcid, mc.MCATEGORY ,m.itemcode,m.itemname,m.strength1,m.unit,
(nvl(DME_INDENTQTY,0)) as DMEAI,
(nvl(DME_INDENTQTY,0))*nvl(SKUFINALRATE,0) as DMEIvalue
,case when (nvl(DME_INDENTQTY,0)*nvl(SKUFINALRATE,0))<100000 then 'Upto 1 Lacs'
else 'Above 1 Lac' end as AIValue
,RUQC,
Round((nvl(RUQC,0))*nvl(SKUFINALRATE,0),0) as Stockvalue,
pipQty,Round((nvl(pipQty,0))*nvl(SKUFINALRATE,0),0) as Pipelinevalue
,nvl(iss.iss_qty,0) as iss_qty
,case when nvl(iss.iss_qty,0) >0 then 1 else 0 end as Iss2324
, case when nvl(RUQC,0)>0 then 1 else 0 end as PhysicalStock
, case when nvl(RUQC,0)=0 and nvl(pipQty,0) >0  then 1 else 0 end as OnlyPipeline
,case when r.ritemid is not null then 1 else 0 end as RC 
,case when r.ritemid is  null  and ac.ACCitemid is not null then 1 else 0 end as Accepted
,case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is not null then 1 else 0 end as PriceOpened
, case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is  null and evitemid is not null then 1 else 0 end as Evaluation
,case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is  null and evitemid is  null and litemid is not null then 1 else 0 end as live
,case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is  null and evitemid is  null and litemid is  null and toitemid is not null then 1 else 0 end as Tobe
, case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as EDLSttus
,m.ISDEDL , m.ISRFORLP,m.IsWHReport ,m.itemid,case when iss_qty>0 then 1 else 0 end as isscount
from masitems m
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
left outer join 
(
select sum(iss_qty) as iss_qty
,di.itemid 
from 
(
select tbi.itemid,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty 
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes ty on ty.facilitytypeid=f.facilitytypeid 
        where tb.status = 'C' and tb.issuetype='NO' and ty.facilitytypeid  in (378,364)
 and  tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + yearid + @" ) and (select enddate from masaccyearsettings where accyrsetid=" + yearid + @") 
        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        group by tbi.itemid

        ) di 
           group by di.itemid 

) iss on iss.itemid=m.itemid


left outer join 
(
select ITEMID,SKUFINALRATE from v_itemrate
) vr on vr.itemid=m.itemid

left outer join
(
select distinct itemid as ritemid from v_rcvalid r
) r on r.ritemid=m.itemid

left outer join 
(
select t.itemid as ACCitemid from v_tenderstatusallnew t where t.actioncode like 'Accept%'
) ac on ac.ACCitemid=m.itemid


left outer join 
(
select itemid as pritemid from v_tenderstatusallnew t where t.actioncode like 'Price Op%'
) pr on pr.pritemid=m.itemid


left outer join 
(
select itemid as evitemid from v_tenderstatusallnew t where t.actioncode in ('Claim Objection in','Cover-A in','Cover-B in')
) ev on ev.evitemid=m.itemid

left outer join 
(
select itemid as litemid from v_tenderstatusallnew t where t.actioncode in ('Live in')
) l on l.litemid=m.itemid

left outer join 
(
select itemid as toitemid from v_tenderstatusallnew t where t.actioncode in ('To be Retender')
) ti on ti.toitemid=m.itemid

inner join itemindent i on i.itemid=m.itemid 
left outer join
(
select sum(nvl(READY,0))+sum(nvl(IWHPIPE,0))+sum(nvl(UQC,0)) as RUQC,sum(nvl(TOTLPIPELINE,0)) as pipQty,s.itemid from VWHSTOCKWITHEXP s 
inner join masitems m on m.itemid=s.itemid
group by s.itemid
) s on s.itemid=m.itemid

where 1=1 and i.accyrsetid=" + yearid + @" and nvl(DME_INDENTQTY,0)>0
and mc.mcid in (1,2,3)
" + whprogram + @"
and IsFreez_ITPR is null
)
group by  MCATEGORY,mcid
order by mcid "
                ;
            }





            if (hodid == "3") //AYUSH
            {

                if (programid == "1") //Ayush Classical
                {

                    whprogram = " and mca.SUBCATID=1";
                }
                if (programid == "2") //Ayurvedic Petent
                {

                    whprogram = " and mca.SUBCATID=2";
                }
                if (programid == "3") //Homeo Classical
                {

                    whprogram = " and mca.SUBCATID=3";
                }
                if (programid == "4") //Homeo Petent
                {

                    whprogram = " and mca.SUBCATID=4";
                }
                if (programid == "5") //Unani Classical
                {

                    whprogram = " and mca.SUBCATID=5";
                }
                if (programid == "6") //Unani Petent
                {

                    whprogram = " and mca.SUBCATID=6";
                }
                if (programid == "7") //Kachhi
                {

                    whprogram = " and mca.SUBCATID=7";
                }
                if (programid == "8") //Dressing Material
                {

                    whprogram = " and mca.SUBCATID=8";
                }

                if (programid == "0") //Overall
                {


                }

                qry = @"  select mcid,MCATEGORY,count(itemid) as nos,sum(isscount) as isscount,Round(sum(isscount)/count(itemid)*100,2) issuedPer,sum(PhysicalStock) as nosstock,sum(OnlyPipeline) as nospipeline
,sum(RC) as rc,sum(Accepted) as accepted,sum(PriceOpened)as PriceOpened,sum(Evaluation) as Evaluation,sum(live) as live,sum(Tobe) as Tobe
from 
(

select mc.mcid, mc.MCATEGORY,m.itemcode,m.itemname,m.strength1,m.unit,
(nvl(DHS_INDENTQTY,0)+nvl(MITANIN,0)) as DHSAI,RUQC,
pipQty, case when nvl(RUQC,0)>0 then 1 else 0 end as PhysicalStock
, case when nvl(RUQC,0)=0 and nvl(pipQty,0) >0  then 1 else 0 end as OnlyPipeline
,case when r.ritemid is not null then 1 else 0 end as RC 
,case when r.ritemid is  null  and ac.ACCitemid is not null then 1 else 0 end as Accepted
,case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is not null then 1 else 0 end as PriceOpened
, case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is  null and evitemid is not null then 1 else 0 end as Evaluation
,case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is  null and evitemid is  null and litemid is not null then 1 else 0 end as live
,case when r.ritemid is  null  and ac.ACCitemid is  null and pr.pritemid is  null and evitemid is  null and litemid is  null and toitemid is not null then 1 else 0 end as Tobe
, case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as EDLSttus
,m.ISDEDL , m.ISRFORLP,m.itemid,case when iss_qty>0 then 1 else 0 end as isscount
from masitems m
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
inner join massubitemcategory mca on mca.SUBCATID=m.SUBCATID AND MCA.CATEGORYID=C.categoryid
left outer join 
(
select sum(iss_qty) as iss_qty
,di.itemid 
from 
(
select tbi.itemid,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty 
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes ty on ty.facilitytypeid=f.facilitytypeid 
        where tb.status = 'C' and tb.issuetype='NO' and ty.facilitytypeid  in (371)
 and  tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + yearid + @" ) and (select enddate from masaccyearsettings where accyrsetid=" + yearid + @") 
        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        group by tbi.itemid

        ) di 
           group by di.itemid 

) iss on iss.itemid=m.itemid

left outer join
(
select distinct itemid as ritemid from v_rcvalid r
) r on r.ritemid=m.itemid

left outer join 
(
select t.itemid as ACCitemid from v_tenderstatusallnew t where t.actioncode like 'Accept%'
) ac on ac.ACCitemid=m.itemid


left outer join 
(
select itemid as pritemid from v_tenderstatusallnew t where t.actioncode like 'Price Op%'
) pr on pr.pritemid=m.itemid


left outer join 
(
select itemid as evitemid from v_tenderstatusallnew t where t.actioncode in ('Claim Objection in','Cover-A in','Cover-B in')
) ev on ev.evitemid=m.itemid

left outer join 
(
select itemid as litemid from v_tenderstatusallnew t where t.actioncode in ('Live in')
) l on l.litemid=m.itemid

left outer join 
(
select itemid as toitemid from v_tenderstatusallnew t where t.actioncode in ('To be Retender')
) ti on ti.toitemid=m.itemid

inner join itemindent i on i.itemid=m.itemid 
left outer join
(
select sum(nvl(READY,0))+sum(nvl(IWHPIPE,0))+sum(nvl(UQC,0)) as RUQC,sum(nvl(TOTLPIPELINE,0)) as pipQty,s.itemid from VWHSTOCKWITHEXP s 
inner join masitems m on m.itemid=s.itemid
group by s.itemid
) s on s.itemid=m.itemid

where 1=1 and i.accyrsetid=" + yearid + @" and (nvl(DHS_INDENTQTY,0))>0
and IsFreez_ITPR is  null and mc.mcid in (4)
" + whprogram + @"
) group by MCATEGORY,mcid
order by mcid ";

            }




            var myList = _context.IndentvsIssuanceDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("getFunds")]
        public async Task<ActionResult<IEnumerable<FundsDTO>>> getFunds()
        {
            string qry = @" select budgetid,budgetname from MASBUDGET where budgetid not in (3) order by Orderid ";

            var myList = _context.FundsDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



        [HttpGet("fitReport")]
        public async Task<ActionResult<IEnumerable<FitReportDTO>>> fitReport(Int64 potype, Int64 budgetId)
        {
            string WhereClausePayeStatus = "";

            if (potype == 1)
            {
                WhereClausePayeStatus = " ";
            }
            if (potype == 2)
            {
                WhereClausePayeStatus = @" and fitunfit='Fit' order by lastmrcdt,sdackuserdt, ponoid ";
            }
            if (potype == 3)
            {
                WhereClausePayeStatus = @" and fitunfit='Not Fit' and  nvl(receiptqty,0)>0 order by lastmrcdt,sdackuserdt, ponoid ";
            }


            string qry = @"  
select MonthNumber, SUBSTR(monthDate, 1, 3)||'-'||substr(yr,-2) as FITMonth
,count(ponoid) as CountFitFile," + budgetId + @" as BudgetID,round(sum(nvl(LIBWITHOUTADM,0))/10000000,2) as TobepaydValue,yr from  (
  select ponoid,paymentRequired,suppliername,itemcode,itemname,pono,HasFile,FileID,PODATE,to_char(POQTY) as POQTY,TotalPOvalue,receiptqty,mrcdate,LASTMRCDT,QCPass,LastQCPaadedDT,receiptvalue,libWithoutAdm,libWithAdm,ToRecePer,PresentFile,reasonName
,fitunfit,hodtype,fileno,filedt,Source,case when sdackuserdt is not null then  to_char(sdackuserdt,'dd-MM-yyyy') else 'Not Received' end as SDDate  ,PHYRECEIPTDT,sdid

, to_char(CASE when sdackuserdt is not null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end ,'dd-MM-yyyy')  as FitDate
, to_char(CASE when sdackuserdt is not null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end ,'MONTH')  monthDate
,Extract (YEAR  from (CASE when sdackuserdt is not null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end)) yr
, to_char(CASE when sdackuserdt is not null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end ,'mm')  MonthNumber


from ( 


select op.ismanualpaid , op.supplierid,op.suppliername,mc.schemecode,m.itemcode,SUBSTR(m.itemname,1,30) as  itemname,m.unitcount,op.pono,to_char(op.PODATE,'dd-MM-yyyy') as podate,POQTY, nvl(receiptqty,0) as receiptqty, case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end  as ToRecePer
,op.lastmrcdt as LASTMRCDT,op.mrcdate,nvl(QCPass,0) as QCPass,case when (issdrequired is null or issdrequired = 'Y') then 'Y' else 'N' end sdrequired
,nvl(sdamount,0) as sdamount
,case when ((case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end)=1 and nvl(receiptqty,0)>nvl(QCPass,0)) then 'QC Pending' else case when (case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end)=1 and nvl(receiptqty,0)=0 then 'Pipeline'
else case when (case when (issdrequired is null or issdrequired = 'Y') then (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)=1 then 'SD Pending' 
else case when (case when nvl(res.reasonid,0)>0 and  (nvl(res.reasonid,0)=10 or nvl(res.reasonid,0)=11 or nvl(res.reasonid,0)=12) then 0 else case when nvl(res.reasonid,0)=0 then 0  else 1 end end)=1 then res.reasonName end end end end  reasonName
 ,nvl(FM.Presentfile,'MRC Desk') as Presentfile
,nvl(so.fileno,0 ) as fileno, to_char(so.filedt,'dd-MM-yyyy') as filedt
,touserid,md.deptname as  hodtype,ofi.fileid,CASE WHEN (ofi.fileid) is Null then 'false' else 'true' end HasFile
,op.paymentrequired

,totalpovalue as TotalPOvalue,totalpovalueadm as POValue5Per ,receiptvalue as receiptvalue
,receiptvalueadm,sancamount,sancamountadm,witheldamt,libnew as libWithoutAdm ,LibNewAdm as libWithAdm
 ,'DPDMIS' as Source
,validity


,case when (issdrequired is null or issdrequired = 'Y') then (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end as SDRequiredDoc
 
,case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end as ReceiptQCFlag
,case when nvl(res.reasonid,0)>0 and  (nvl(res.reasonid,0)=10 or nvl(res.reasonid,0)=11 or nvl(res.reasonid,0)=12)
 then 0 else case when nvl(res.reasonid,0)=0 then 0  else 1 end end  as ReasonNameTB
 ,case when  nvl(res.IsfitReason,0)=1  then 0 else 1 end ReasonNameTBNew
 ,  case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)=100
 and validity in ('Supplied and Time Expired','Supplied and Time Valid')
 and   ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)+
 (case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )+
(case when  nvl(res.IsfitReason,0)=1  then 0 else  (case when nvl(res.reasonid,0)=0 then 0 else 1 end) end ))=0 then 'Fit' 
 else
 case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)>=90
 and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)<100
 and validity in ('Supplied and Time Expired','Supplied and Time Valid')
 and   ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)+(case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )
 +(case when  nvl(res.IsfitReason,0)=1  then 0 else  (case when nvl(res.reasonid,0)=0 then 0 else 1 end) end ))=0 then 'Fit' 
 else
  case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)<90 and validity in ('Supplied and Time Expired')
 and ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)+(case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )
 +(case when  nvl(res.IsfitReason,0)=1  then 0 else (case when IsRelaxBelow90=1 then 0 else 1 end ) end ))=0 then 'Fit' else 
 case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)<90 
 and nvl(res.reasonid,0)=14
 and ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)
 +(case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )
 +(case when  nvl(res.IsfitReason,0)=1  then 0 else (case when IsRelaxBelow90=1 then 0 else 1 end ) end ))=0 
 then 'Fit' else 
 'Not Fit'

end end end end as fitunfit
,nvl(res.reasonid,0) as reasonid  
 ,paymentstatusNew,op.budgetid, op.PoNoID,sdackuserdt,sdid,qt.LastQCPaadedDT,PHYRECEIPTDT
 
from 
 v_popaymentstatus op
 inner join soorderplaced so on so.ponoid=op.ponoid
 left outer join masdepartment md on md.deptid=so.deptid
 inner join masschemes mc on mc.schemeid=op.schemeid
 inner join masitems m on m.itemid=op.itemid
 left outer join 
 (
 select max(fileid) as fileid,ponoid from SoSupplyOrderFiles 

 group by ponoid 
 ) ofi on ofi.ponoid=op.ponoid  
 left outer join 
(
select touserid,ponoid,us.FirstName|| '-' ||us.LastName as Presentfile from  MASFILEMOVEMENT fm
inner join usrusers us on us.userid=fm.touserid
where presentfileflag='Y'
) FM on FM.ponoid=op.ponoid
left outer join ldlevies l on l.schemeid=mc.schemeid and l.status='C' and l.isactive=1
 left outer join
(
select sdc.ponoid, sum(sdc.sdamount)sdamount,sd.sdid ,sd.sdackuserdt 
,sd.PHYRECEIPTDT  from sdsupmaster sd 
inner join  sdsupchild sdc on sdc.sdid = sd.sdid
where sd.status = 'C'  and sd.ishoreceipt='Y'
group by sdc.ponoid,sd.sdid,sd.sdackuserdt ,sd.PHYRECEIPTDT 
)sdiq on sdiq.ponoid = op.ponoid
  left outer join  
            (
            select s.ponoid,max(reportreceiveddate) as LastQCPaadedDT,max(reportreceiveddate) as reportreceiveddate,s.itemid,round(sysdate-max(reportreceiveddate),0) qcPassdays from qcsamples s  
            inner join soOrderPlaced so on so.ponoid=s.ponoid
            inner join masitems mi on mi.itemid=s.itemid
            left outer join qctests q on (s.sampleid=q.sampleid or s.refsampleid=q.sampleid )
            where  q.testresult='SQ' and reportreceiveddate is not null
            group by s.itemid,s.ponoid
            ) qt on qt.ponoid=op.ponoid  and qt.itemid=m.ItemID

left outer join  
(


select x.itemid,x.ponoid,sum(nvl(QCPass,0))-nvl(rs.RSqty,0) as QCPass,qastatus from 
(
select  i.itemid, t.ponoid, sum(nvl(tb.absrqty,0)) as QCPass,tb.inwno,
(case when (mi.qctest = 'N' and tb.qastatus = 0) then 'QC Not Required' when  tb.qastatus = 0 then 'Result Pending' when tb.qastatus = 1 then 'SQ' when tb.qastatus = 2 then 'NSQ'  end ) qastatus

 from tbreceipts t
 inner join soOrderPlaced op on op.ponoid=t.ponoid
inner join tbreceiptitems i on (i.receiptid = t.receiptid)
inner join tbreceiptbatches TB on (I.receiptitemid = TB.receiptitemid)
inner join masitems mi on mi.itemid = i.itemid
where op.accyrsetid>=539  and t.receipttype='NO' and t.status = 'C' 
and t.notindpdmis is null and i.notindpdmis is null and TB.notindpdmis is null
group by I.ItemID,  T.PoNoID,tb.inwno,mi.qctest,tb.qastatus
)x
 left outer join
                                             (

                                             select  ponoid,sum(issueqty+qissueqty) RSqty from (
                                                select distinct r.ponoid as rponoid
                                              ,tbo.issueqty , nvl(q.issueqty,0) qissueqty,rb.batchno ISS_Batchno,tbo.inwno
                                              ,  rb.ponoid as ponoid
,(case when (mi.qctest = 'N' and rb.qastatus = 0) then 'QC Not Required' when  rb.qastatus = 0 then 'Result Pending' when rb.qastatus = 1 then 'SQ' when rb.qastatus = 2 then 'NSQ'  end ) qastatus
                                                 from tbindents tb
                                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                                  inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                                  inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
                                                  inner join masitems mi on mi.itemid = tbi.itemid
 
                                                  left outer join
                                                  (
                                                     select tbi.itemid,tbo.issueqty , rb.batchno ISS_Batchno,tbo.inwno
                                                     from tbindents tb
                                                     inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                                      inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                                      inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
                                                      where tb.Status = 'C' and tb.issuetype in ('QA','QS') 
                                                  ) q on q.inwno = rb.inwno
                                                  inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
                                                  inner join tbreceipts r on r.receiptid=ri.receiptid
                                                  inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                                  where tb.Status = 'C' and tb.issuetype in ('RS') 
                                                  and tb.notindpdmis is null and r.notindpdmis is null and tbi.notindpdmis is null
                                                  and tbo.notindpdmis is null and rb.notindpdmis is null  and ri.notindpdmis is null                                    
                                               ) where qastatus  in ('QC Not Required','SQ') group by ponoid                                             
                                             ) rs on rs.ponoid=x.ponoid
                                             
                                             where x.qastatus  in ('QC Not Required','SQ')
group by x.itemid,x.ponoid,x.qastatus,rs.RSqty


) QCPass on QCPass.ponoid =op.ponoid

left outer join 
(
select Max(d.sorid) sorid,d.ponoid 
from soorderreason d 
group by d.ponoid
) resM on resM.ponoid=op.ponoid
left outer join 
(
select sr.sorid,ponoid,case when r.reasonid=8 and sr.remarks is not null  then r.reasonName||'('||sr.remarks||')' else r.reasonName end reasonName ,r.reasonid,sr.entrydate
,nvl(r.isfit,0) as IsfitReason
from soorderreason  sr
inner join reasonmaster  r on r.reasonid=sr.reasonid
where  r.reasonid not in (5,1) 
) res on res.sorid=resM.sorid and res.ponoid=resM.ponoid
 where 1=1 and paymentstatusNew='Not Paid'
and op.budgetid = " + budgetId + @"
 

 
 )x  where 1=1  and nvl(libWithoutAdm,0)>0  " + WhereClausePayeStatus + @"
 
 ) group by monthDate,MonthNumber,yr order by Yr,MonthNumber";

            var myList = _context.FitReportDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("fitReportDetail")]
        public async Task<ActionResult<IEnumerable<FitDetailDTO>>> fitReportDetail(string BudGetID, string FitUnfit, string yr, string month)
        {
            string WhereClausePayeStatus = "";
            string whereClauseYear = "";
            whereClauseYear = " and Extract (YEAR  from (CASE when sdackuserdt is not null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end)) = " + yr + @"      ";
            if (FitUnfit == "1")
            {
                WhereClausePayeStatus = " ";
            }
            if (FitUnfit == "2")
            {
                WhereClausePayeStatus = @" and fitunfit='Fit' order by lastmrcdt,sdackuserdt, x.ponoid ";
            }
            if (FitUnfit == "3")
            {
                WhereClausePayeStatus = @" and fitunfit='Not Fit' order by lastmrcdt,sdackuserdt, x.ponoid ";
            }

            string mMonyh = month;

            string whereclause2 = "  and  (to_char(CASE when sdackuserdt is not null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end ,'mm'))=" + mMonyh;


            string qry = @"  select x.ponoid,paymentRequired,suppliername,itemcode,itemname,pono,HasFile,FileID,PODATE,to_char(POQTY) as POQTY,Round(TotalPOvalue,0) as TotalPOvalue ,receiptqty,mrcdate,LASTMRCDT,QCPass,to_char(LastQCPaadedDT,'dd-MM-yyyy') as LastQCPaadedDT ,Round(receiptvalue,0) as receiptvalue,libWithoutAdm,libWithAdm,ToRecePer,PresentFile,reasonName
,fitunfit,hodtype,fileno,filedt,Source,case when sdackuserdt is not null then  to_char(sdackuserdt,'dd-MM-yyyy') else 'Not Received' end as SDDate  ,to_char(PHYRECEIPTDT,'dd-MM-yyyy') as PHYRECEIPTDT,sdid
, to_char(CASE when sdackuserdt is not null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end ,'dd-MM-yyyy')  as FitDate
, to_char(CASE when sdackuserdt is not null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end ,'MONTH')  monthDate
, to_char(CASE when sdackuserdt is not null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and LastQCPaadedDT>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end ,'mm')  MonthNumber
,case when  nvl(sc.sanctionid,'0')  ='0' then 'Not Generated'  else 'Generated' end as sanctionStatus
from ( 
select op.ismanualpaid , op.supplierid,op.suppliername,mc.schemecode,m.itemcode,SUBSTR(m.itemname,1,30) as  itemname,m.unitcount,op.pono,to_char(op.PODATE,'dd-MM-yyyy') as podate,POQTY, nvl(receiptqty,0) as receiptqty, case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end  as ToRecePer
,op.lastmrcdt as LASTMRCDT,op.mrcdate,nvl(QCPass,0) as QCPass,case when (issdrequired is null or issdrequired = 'Y') then 'Y' else 'N' end sdrequired
,nvl(sdamount,0) as sdamount
,case when ((case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end)=1 and nvl(receiptqty,0)>nvl(QCPass,0)) then 'QC Pending' else case when (case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end)=1 and nvl(receiptqty,0)=0 then 'Pipeline'
else case when (case when (issdrequired is null or issdrequired = 'Y') then (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)=1 then 'SD Pending' 
else case when (case when nvl(res.reasonid,0)>0 and  (nvl(res.reasonid,0)=10 or nvl(res.reasonid,0)=11 or nvl(res.reasonid,0)=12) then 0 else case when nvl(res.reasonid,0)=0 then 0  else 1 end end)=1 then res.reasonName end end end end  reasonName
 ,nvl(FM.Presentfile,'MRC Desk') as Presentfile
,nvl(so.fileno,0 ) as fileno, to_char(so.filedt,'dd-MM-yyyy') as filedt
,touserid,md.deptname as  hodtype,ofi.fileid,CASE WHEN (ofi.fileid) is Null then 'false' else 'true' end HasFile
,op.paymentrequired

,totalpovalue as TotalPOvalue,totalpovalueadm as POValue5Per ,receiptvalue as receiptvalue
,receiptvalueadm,sancamount,sancamountadm,witheldamt,libnew as libWithoutAdm ,LibNewAdm as libWithAdm
 ,'DPDMIS' as Source
,validity
,case when (issdrequired is null or issdrequired = 'Y') then (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end as SDRequiredDoc
,case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end as ReceiptQCFlag
,case when nvl(res.reasonid,0)>0 and  (nvl(res.reasonid,0)=10 or nvl(res.reasonid,0)=11 or nvl(res.reasonid,0)=12)
 then 0 else case when nvl(res.reasonid,0)=0 then 0  else 1 end end  as ReasonNameTB
 ,case when  nvl(res.IsfitReason,0)=1  then 0 else 1 end ReasonNameTBNew
 ,  case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)=100
 and validity in ('Supplied and Time Expired','Supplied and Time Valid')
 and   ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)+
 (case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )+
(case when  nvl(res.IsfitReason,0)=1  then 0 else  (case when nvl(res.reasonid,0)=0 then 0 else 1 end) end ))=0 then 'Fit' 
 else
 case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)>=90
 and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)<100
 and validity in ('Supplied and Time Expired','Supplied and Time Valid')
 and   ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)+(case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )
 +(case when  nvl(res.IsfitReason,0)=1  then 0 else  (case when nvl(res.reasonid,0)=0 then 0 else 1 end) end ))=0 then 'Fit' 
 else
  case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)<90 and validity in ('Supplied and Time Expired')
 and ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)+(case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )
 +(case when  nvl(res.IsfitReason,0)=1  then 0 else (case when IsRelaxBelow90=1 then 0 else 1 end ) end ))=0 then 'Fit' else 
 case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)<90 
 and nvl(res.reasonid,0)=14
 and ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)
 +(case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )
 +(case when  nvl(res.IsfitReason,0)=1  then 0 else (case when IsRelaxBelow90=1 then 0 else 1 end ) end ))=0 
 then 'Fit' else 
 'Not Fit'

end end end end as fitunfit
,nvl(res.reasonid,0) as reasonid  
 ,paymentstatusNew,op.budgetid, op.PoNoID,sdackuserdt,sdid,qt.LastQCPaadedDT,PHYRECEIPTDT
 
from 
 v_popaymentstatus op
 inner join soorderplaced so on so.ponoid=op.ponoid
 left outer join masdepartment md on md.deptid=so.deptid
 inner join masschemes mc on mc.schemeid=op.schemeid
 inner join masitems m on m.itemid=op.itemid
 left outer join 
 (
 select max(fileid) as fileid,ponoid from SoSupplyOrderFiles 

 group by ponoid 
 ) ofi on ofi.ponoid=op.ponoid  
 left outer join 
(
select touserid,ponoid,us.FirstName|| '-' ||us.LastName as Presentfile from  MASFILEMOVEMENT fm
inner join usrusers us on us.userid=fm.touserid
where presentfileflag='Y'
) FM on FM.ponoid=op.ponoid
left outer join ldlevies l on l.schemeid=mc.schemeid and l.status='C' and l.isactive=1
 left outer join
(
select sdc.ponoid, sum(sdc.sdamount)sdamount,sd.sdid ,sd.sdackuserdt 
,sd.PHYRECEIPTDT
from sdsupmaster sd 
inner join  sdsupchild sdc on sdc.sdid = sd.sdid
where sd.status = 'C'  and sd.ishoreceipt='Y'
group by sdc.ponoid,sd.sdid,sd.sdackuserdt ,sd.PHYRECEIPTDT 
)sdiq on sdiq.ponoid = op.ponoid
  left outer join  
            (
            select s.ponoid,max(reportreceiveddate) as LastQCPaadedDT,max(reportreceiveddate) as reportreceiveddate,s.itemid,round(sysdate-max(reportreceiveddate),0) qcPassdays from qcsamples s  
            inner join soOrderPlaced so on so.ponoid=s.ponoid
            inner join masitems mi on mi.itemid=s.itemid
            left outer join qctests q on (s.sampleid=q.sampleid or s.refsampleid=q.sampleid )
            where  q.testresult='SQ' and reportreceiveddate is not null
            group by s.itemid,s.ponoid
            ) qt on qt.ponoid=op.ponoid  and qt.itemid=m.ItemID

left outer join  
(


select x.itemid,x.ponoid,sum(nvl(QCPass,0))-nvl(rs.RSqty,0) as QCPass,qastatus from 
(
select  i.itemid, t.ponoid, sum(nvl(tb.absrqty,0)) as QCPass,tb.inwno,
(case when (mi.qctest = 'N' and tb.qastatus = 0) then 'QC Not Required' when  tb.qastatus = 0 then 'Result Pending' when tb.qastatus = 1 then 'SQ' when tb.qastatus = 2 then 'NSQ'  end ) qastatus

 from tbreceipts t
 inner join soOrderPlaced op on op.ponoid=t.ponoid
inner join tbreceiptitems i on (i.receiptid = t.receiptid)
inner join tbreceiptbatches TB on (I.receiptitemid = TB.receiptitemid)
inner join masitems mi on mi.itemid = i.itemid
where op.accyrsetid>=539  and t.receipttype='NO' and t.status = 'C' 
and t.notindpdmis is null and i.notindpdmis is null and TB.notindpdmis is null
group by I.ItemID,  T.PoNoID,tb.inwno,mi.qctest,tb.qastatus
)x
 left outer join
                                             (

                                             select  ponoid,sum(issueqty+qissueqty) RSqty from (
                                                select distinct r.ponoid as rponoid
                                              ,tbo.issueqty , nvl(q.issueqty,0) qissueqty,rb.batchno ISS_Batchno,tbo.inwno
                                              ,  rb.ponoid as ponoid
,(case when (mi.qctest = 'N' and rb.qastatus = 0) then 'QC Not Required' when  rb.qastatus = 0 then 'Result Pending' when rb.qastatus = 1 then 'SQ' when rb.qastatus = 2 then 'NSQ'  end ) qastatus
                                                 from tbindents tb
                                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                                  inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                                  inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
                                                  inner join masitems mi on mi.itemid = tbi.itemid
 
                                                  left outer join
                                                  (
                                                     select tbi.itemid,tbo.issueqty , rb.batchno ISS_Batchno,tbo.inwno
                                                     from tbindents tb
                                                     inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                                      inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                                      inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
                                                      where tb.Status = 'C' and tb.issuetype in ('QA','QS') 
                                                  ) q on q.inwno = rb.inwno
                                                  inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
                                                  inner join tbreceipts r on r.receiptid=ri.receiptid
                                                  inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                                  where tb.Status = 'C' and tb.issuetype in ('RS') 
                                                  and tb.notindpdmis is null and r.notindpdmis is null and tbi.notindpdmis is null
                                                  and tbo.notindpdmis is null and rb.notindpdmis is null  and ri.notindpdmis is null                                    
                                               ) where qastatus  in ('QC Not Required','SQ') group by ponoid                                             
                                             ) rs on rs.ponoid=x.ponoid
                                             
                                             where x.qastatus  in ('QC Not Required','SQ')
group by x.itemid,x.ponoid,x.qastatus,rs.RSqty


) QCPass on QCPass.ponoid =op.ponoid

left outer join 
(
select Max(d.sorid) sorid,d.ponoid 
from soorderreason d 
group by d.ponoid
) resM on resM.ponoid=op.ponoid
left outer join 
(
select sr.sorid,ponoid,case when r.reasonid=8 and sr.remarks is not null  then r.reasonName||'('||sr.remarks||')' else r.reasonName end reasonName ,r.reasonid,sr.entrydate
,nvl(r.isfit,0) as IsfitReason
from soorderreason  sr
inner join reasonmaster  r on r.reasonid=sr.reasonid
where  r.reasonid not in (5,1) 
) res on res.sorid=resM.sorid and res.ponoid=resM.ponoid
 where 1=1 and paymentstatusNew='Not Paid'
and op.budgetid = " + BudGetID + @"
 

 
 )x 

 left outer join
 (
 select max(sanctionid) sanctionid ,ponoid from blpsanctions  group by ponoid
 
 )sc on sc.ponoid=x.ponoid

where 1=1  and nvl(libWithoutAdm,0)>0  " + whereclause2 + " " + whereClauseYear + @"  " + WhereClausePayeStatus + @" 
   ";

            var myList = _context.FitDetailDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("GroupItemtypeRCStock")]
        public async Task<ActionResult<IEnumerable<StockGroupItemDTO>>> GroupItemtypeRCStock(string mcid, string Grouportype, string whid)
        {
            string qry = "";
            string twhclause = "";
            string sowhclause = "";
            if (whid != "1")
            {
                twhclause = " and  t.warehouseid=" + whid;
                sowhclause = " and   soi.warehouseid=" + whid;
            }

            if (Grouportype == "Group")
            {
                qry = @"

select groupname AS Particular,groupid AS ID,sum(RC) as RC,sum(Readycnt) as Readycnt,sum(Uqccount) as Uqccount,sum(piplinecnt) as piplinecnt
from 
(
 
select  nvl(g.groupname,'NA') groupname,nvl(g.groupid,1) as groupid,case when r.itemid is not null then 1 else 0 end as RC ,case when nvl(READY,0)>0 then 1 else 0 end as Readycnt
,case when nvl(UQC,0)>0 and nvl(READY,0)=0 then 1 else 0 end as Uqccount,case when nvl(TOTLPIPELINE,0)>0 and (nvl(READY,0)+nvl(UQC,0))=0 then 1 else 0 end as piplinecnt
from masitems m
left outer join masitemgroups g on g.groupid=m.groupid
left outer join 
(
select itemid,sum(nvl(ReadyForIssue,0)) as READY,sum(nvl(Pending,0)) as UQC from 
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems mi on mi.itemid= i.itemid
 inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
 inner join soordereditems odi on odi.ponoid = b.ponoid
 inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C' and mc.mcid=1   And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate " + twhclause + @"
     ) group by itemid
) st on st.itemid=m.itemid


left outer join
(
  select itemid, sum(pipelineQTY) TOTLPIPELINE from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid
 where op.status  in ('C','O')   " + sowhclause + @"
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having (case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) group by itemid
) pip on pip.itemid = m.itemid

inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID

left outer join 
(
select distinct itemid from v_rcvalid 
) r on r.itemid=m.itemid

where m.ISFREEZ_ITPR is null  and mc.mcid=" + mcid + @"
)
group by groupname,groupid
HAVING (sum(RC)+sum(Readycnt)+sum(Uqccount)+sum(piplinecnt))>0
order by groupname ";
            }
            else
            {
                qry = @" 
select itemtypename AS Particular,itemtypeid AS ID ,sum(RC) as RC,sum(Readycnt) as Readycnt,sum(Uqccount) as Uqccount,sum(piplinecnt) as piplinecnt
from 
(
 
select   nvl(ty.itemtypename,'NA') itemtypename,nvl(m.itemtypeid,1) as itemtypeid,case when r.itemid is not null then 1 else 0 end as RC ,case when nvl(READY,0)>0 then 1 else 0 end as Readycnt
,case when nvl(UQC,0)>0 and nvl(READY,0)=0 then 1 else 0 end as Uqccount,case when nvl(TOTLPIPELINE,0)>0 and (nvl(READY,0)+nvl(UQC,0))=0 then 1 else 0 end as piplinecnt
from masitems m
left outer join masitemtypes ty on ty.itemtypeid=m.itemtypeid
left outer join 
(
select itemid,sum(nvl(ReadyForIssue,0)) as READY,sum(nvl(Pending,0)) as UQC from 
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems mi on mi.itemid= i.itemid
 inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
 inner join soordereditems odi on odi.ponoid = b.ponoid
 inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C' and mc.mcid=1   And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate " + twhclause + @"
     ) group by itemid
) st on st.itemid=m.itemid


left outer join
(
  select itemid, sum(pipelineQTY) TOTLPIPELINE from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid
 where op.status  in ('C','O')  " + sowhclause + @"
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having (case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) group by itemid
) pip on pip.itemid = m.itemid

inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID

left outer join 
(
select distinct itemid from v_rcvalid 
) r on r.itemid=m.itemid

where m.ISFREEZ_ITPR is null  and mc.mcid=" + mcid + @"
)
group by itemtypename,itemtypeid
HAVING (sum(RC)+sum(Readycnt)+sum(Uqccount)+sum(piplinecnt))>0
order by itemtypename ";
            }


            var myList = _context.StockGroupItemDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("getPipelineDDL")]
        public async Task<ActionResult<IEnumerable<PipelineDTO>>> getPipelineDDL(string mcid,string whid,string userid)
        {
            string twarehoueid = ""; string trwarehoueid = ""; string soiwarehoueid = "";
            if (whid != "0")
            {
                twarehoueid = " and t.warehouseid =" + whid;
                trwarehoueid = " and tr.warehouseid =" + whid;
                soiwarehoueid= " and soi.warehouseid =" + whid;
            }
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid= " and mc.MCID =" + mcid;
            }

   

            if (userid == "10212"|| userid == "10025"|| userid == "10222")
            {
                //reagentonly
                whmcid = " and mc.MCID =3";
            }
            if (userid == "2933" || userid == "2933")
            {
                //ayush
                whmcid = " and mc.MCID =4";
            }
     


            string qry = @" 
select '('||itemcode||')'||days||' days-'||rtrim(ltrim(itemname))||',Received:'||to_char(receiptabsqty)||'/Ordered:'||to_char(absqty)||'-'||suppliername||'-'||pono||case when EXPECTEDDELIVERYDATE is not null then ' Expected Delivery:'||to_char(EXPECTEDDELIVERYDATE,'dd-MM-yyyy') else ' Not Dispatched' end as details,ponoid from 
(
select 
s.suppliername,
case when m.isedl2021 = 'Y' then 'EDL' else 'Non-EDL' end as edltype, m.itemcode, m.itemname,m.strength1,nvl(m.nablreq,'No') as nablreq ,op.pono,op.soissuedate, sum(soi.ABSQTY) as absqty
,nvl(disQTY,0) as disQTY,EXPECTEDDELIVERYDATE
,nvl(rec.receiptabsqty, 0)receiptabsqty,case when nvl(rec.receiptabsqty, 0)>0 then Round(nvl(rec.receiptabsqty, 0)/sum(soi.ABSQTY)*100,2) else 0 end as RecPer
,nvl(READY,0) as READY,nvl(UQC,0) as uqc
, case when nvl(READY,0)=0 and nvl(UQC,0)=0 then 'Stock out' else '' end as st,
op.extendeddate
, round(sysdate - op.soissuedate, 0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
Round((case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end)*oci.finalrategst,0) pipelinevalue
,OI.itemid, op.ponoid,receiptdelayexception
from soOrderPlaced OP
inner join massuppliers s on s.supplierid=op.supplierid
inner join SoOrderedItems OI on OI.PoNoID = OP.PoNoID
inner join sotranches t on t.ponoid=op.ponoid
inner join soorderdistribution soi on soi.orderitemid = OI.orderitemid "+ soiwarehoueid + @"
inner join masitems m on m.itemid = oi.itemid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID "+ whmcid + @"
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid = tr.receiptid
where tr.receipttype = 'NO' and tr.status = 'C' and tr.notindpdmis is null and tri.notindpdmis is null " + trwarehoueid + @"
group by tr.ponoid, tri.itemid
) rec on rec.ponoid = OP.PoNoID and rec.itemid = OI.itemid

left outer join 
(
select ponoid,sum(SUPPLYQTY) as disQTY,max(EXPECTEDDELIVERYDATE) as EXPECTEDDELIVERYDATE from posupplyplan
group by ponoid
) dis on dis.ponoid=op.ponoid

left outer join 
(
select itemid,sum(nvl(ReadyForIssue,0)) as READY,sum(nvl(Pending,0)) as UQC from 
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems mi on mi.itemid= i.itemid
 inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID  "+ whmcid + @"
 inner join soordereditems odi on odi.ponoid = b.ponoid
 inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C' "+ whmcid + @"  And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate "+ twarehoueid + @"
     ) group by itemid
) st on st.itemid=m.itemid

 where op.status  in ('C', 'O') " + whmcid + @" 
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,op.pono,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst,m.isedl2021,s.suppliername, m.itemname,m.strength1,nvl(READY,0),nvl(UQC,0),EXPECTEDDELIVERYDATE,nvl(disQTY,0),m.unit
 having(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end) > 0
) order by days desc ";
            var myList = _context.PipelineDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("getPipelineDDLTransit")]
        public async Task<ActionResult<IEnumerable<PipelineDTO>>> getPipelineDDLTransit(string mcid, string whid, string userid)
        {
            string twarehoueid = ""; string trwarehoueid = ""; string soiwarehoueid = "";
            if (whid != "0")
            {
                twarehoueid = " and t.warehouseid =" + whid;
                trwarehoueid = " and tr.warehouseid =" + whid;
                soiwarehoueid = " and soi.warehouseid =" + whid;
            }
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.MCID =" + mcid;
            }



            if (userid == "10212" || userid == "10025" || userid == "10222")
            {
                //reagentonly
                whmcid = " and mc.MCID =3";
            }
            if (userid == "2933" || userid == "2933")
            {
                //ayush
                whmcid = " and mc.MCID =4";
            }
            string qry = @"      select '('||itemcode||')'||rtrim(ltrim(itemname))||'-'||suppliername||'-'||pono  as details,ponoid from
        (
        select
        s.suppliername,
        case when m.isedl2021 = 'Y' then 'EDL' else 'Non-EDL' end as edltype, m.itemcode, m.itemname, m.strength1, nvl(m.nablreq,'No') as nablreq ,op.pono,op.soissuedate, sum(soi.ABSQTY) as absqty
,nvl(disQTY,0) as disQTY,EXPECTEDDELIVERYDATE
,nvl(rec.receiptabsqty, 0)receiptabsqty,case when nvl(rec.receiptabsqty, 0)>0 then Round(nvl(rec.receiptabsqty, 0)/sum(soi.ABSQTY)*100,2) else 0 end as RecPer

,op.extendeddate
, round(sysdate - op.soissuedate, 0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
Round((case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end)*oci.finalrategst,0) pipelinevalue
,OI.itemid, op.ponoid,receiptdelayexception
from soOrderPlaced OP
inner join massuppliers s on s.supplierid=op.supplierid
inner join SoOrderedItems OI on OI.PoNoID = OP.PoNoID
inner join sotranches t on t.ponoid= op.ponoid
inner join soorderdistribution soi on soi.orderitemid = OI.orderitemid  " + soiwarehoueid + @"
inner join masitems m on m.itemid = oi.itemid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID "+ whmcid + @"
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid = tr.receiptid
where tr.receipttype = 'NO' and tr.status = 'C' and tr.notindpdmis is null and tri.notindpdmis is null  " + trwarehoueid + @"
group by tr.ponoid, tri.itemid
) rec on rec.ponoid = OP.PoNoID and rec.itemid = OI.itemid

left outer join
(
select ponoid, sum(SUPPLYQTY) as disQTY, max(EXPECTEDDELIVERYDATE) as EXPECTEDDELIVERYDATE from posupplyplan
        group by ponoid
) dis on dis.ponoid=op.ponoid 
 where op.status  in ('C', 'O')  " + whmcid + @"
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,op.pono,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst,m.isedl2021,s.suppliername, m.itemname,m.strength1,EXPECTEDDELIVERYDATE,nvl(disQTY,0),m.unit
 having(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end) > 0
) order by days desc ";

  var myList = _context.PipelineDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
 return myList;

 }


  [HttpGet("getPipelineDetails")]
        public async Task<ActionResult<IEnumerable<PipelineDetailsDTO>>> getPipelineDetails(string ponoid, string itemid, string mcid, string whid,string userid,string supid)
        {

            string whmiteid = "";
            if (itemid != "0")
            {
                whmiteid = " and OI.itemid =" + itemid;
            }
            string whponoid = "";
            if (ponoid != "0")
            {
                whponoid = " and op.ponoid =" + ponoid;
            }
            string whsupid = "";
            if (supid != "0")
            {
                whsupid = " and op.supplierid =" + supid;
            }
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid =" + mcid;
            }

            string twarehoueid = ""; string trwarehoueid = ""; string soiwarehoueid = ""; string prwarehouse = "";
            if (whid != "0")
            {
                twarehoueid = " and t.warehouseid =" + whid;
                trwarehoueid = " and tr.warehouseid =" + whid;
                soiwarehoueid = " and soi.warehouseid =" + whid;
                prwarehouse= " and prg.WAREHOUSEID =" + whid;
            }

            if (userid == "10212" || userid == "10025" || userid == "10222")
            {
                //reagentonly
                whmcid = " and mc.MCID =3";
            }
            if (userid == "2933" || userid == "2933")
            {
                //ayush
                whmcid = " and mc.MCID =4";
            }
            string qry = @"  select 
s.suppliername,s.PHONE1,s.EMAIL,
case when m.isedl2021 = 'Y' then 'EDL' else 'Non-EDL' end as edltype, m.itemcode, m.itemname,m.strength1,m.unit,nvl(ty.itemtypename,'NA') as itemtypename,nvl(g.groupname,'NA') as groupname,
case when nvl(m.nablreq,'No')='Y' then 'Yes' else 'No' end  as nablreq ,
op.pono,op.soissuedate, sum(soi.ABSQTY) as absqty
,nvl(disQTY,0) as disQTY,EXPECTEDDELIVERYDATE
,nvl(rec.receiptabsqty, 0)receiptabsqty,case when nvl(rec.receiptabsqty, 0)>0 then Round(nvl(rec.receiptabsqty, 0)/sum(soi.ABSQTY)*100,2) else 0 end as RecPer
,nvl(READY,0) as READY,nvl(UQC,0) as uqc
, case when nvl(READY,0)=0 and nvl(UQC,0)=0 then 'Stock out' else 'Stock Available' end as st,
op.extendeddate
, round(sysdate - op.soissuedate, 0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
Round((case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end)*oci.finalrategst,0) pipelinevalue
,OI.itemid, op.ponoid,receiptdelayexception
,pr.Progress,pr.REMARKS,to_char(pr.ENTRYDATE,'dd-Mon-yyyy hh:mm:ss')  ENTRYDATE
from soOrderPlaced OP
inner join massuppliers s on s.supplierid=op.supplierid
inner join SoOrderedItems OI on OI.PoNoID = OP.PoNoID
inner join sotranches t on t.ponoid=op.ponoid
inner join soorderdistribution soi on soi.orderitemid = OI.orderitemid " + soiwarehoueid + @"
inner join masitems m on m.itemid = oi.itemid
left outer join masitemgroups g on g.groupid=m.groupid
left outer join masitemtypes ty on ty.itemtypeid=m.itemtypeid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid = tr.receiptid
where tr.receipttype = 'NO' and tr.status = 'C' and tr.notindpdmis is null and tri.notindpdmis is null "+ trwarehoueid + @"
group by tr.ponoid, tri.itemid
) rec on rec.ponoid = OP.PoNoID and rec.itemid = OI.itemid


left outer join 
(

select  r.REMID,r.REMARKS as Progress,prg.REMARKS,prg.ENTRYDATE,prg.ponoid from TBLRECVPROGRESS prg
inner join  MasRecRemarks r on r.REMID=prg.REMID
where 1=1 "+ prwarehouse + @"  and prg.ponoid="+ ponoid + @" and prg.LATFLAG='Y'

) pr on pr.ponoid=OP.PoNoID

left outer join 
(
select ponoid,sum(SUPPLYQTY) as disQTY,max(EXPECTEDDELIVERYDATE) as EXPECTEDDELIVERYDATE from posupplyplan
group by ponoid
) dis on dis.ponoid=op.ponoid

left outer join 
(
select itemid,sum(nvl(ReadyForIssue,0)) as READY,sum(nvl(Pending,0)) as UQC from 
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems mi on mi.itemid= i.itemid
 inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
 inner join soordereditems odi on odi.ponoid = b.ponoid
 inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C' and mc.mcid=1   And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate " + twarehoueid + @"
     ) group by itemid
) st on st.itemid=m.itemid

 where op.status  in ('C', 'O') " + whmiteid + @"  " + whponoid + @"  " + whmcid + @" "+ whsupid + @"
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,op.pono,disQTY,g.groupname,ty.itemtypename,s.PHONE1,s.EMAIL,m.unit,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst,m.isedl2021,s.suppliername, m.itemname,m.strength1,nvl(READY,0),nvl(UQC,0),EXPECTEDDELIVERYDATE,nvl(disQTY,0)
,pr.Progress,pr.REMARKS,pr.ENTRYDATE
 having(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end) > 0 ";
            var myList = _context.PipelineDetailsDbset
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }





        [HttpGet("GetFundBalance")]
        public async Task<ActionResult<IEnumerable<FundBalanceDTO>>> GetFundBalance(string budgetid, string yearid)
        {

            string whbudgetid = "";
            if (budgetid != "0")
            {
                whbudgetid = " and  b.budgetid =" + budgetid;


            }
            string qry = "";
            if (yearid == "542")
            {
                qry = @"  select b.BUDGETID, b.BUDGETNAME
,Round(op.CLOSINGBAL/10000000,2) as OPBalance
,Round((nvl(bd.ReceAfterOPAct,0)+nvl(antrc.actRecive,0))/10000000,2) as ActRecYear
,Round(nvl(totAdujst,0)/10000000,2) as totAdujst,0 RefundAmt
,Round((((nvl(bd.ReceAfterOPAct,0)+nvl(op.CLOSINGBAL,0)+nvl(antrc.actRecive,0))-nvl(totAdujst,0) )-nvl(rfdcl.RefundAmt,0))/10000000,2) as TotalFundAvl
,Round(nvl(bd.amtpaid,0)/10000000,2) as TotPaid
,Round(((( (nvl(op.CLOSINGBAL,0) + nvl(bd.ReceAfterOPAct,0)+nvl(antrc.actRecive,0) )-nvl(totAdujst,0))-nvl(bd.amtpaid,0))-nvl(RefundAmt,0))/10000000,2)  ClosingBal
from masbudget b
left outer join fundclosing541 op on op.budgetid=b.budgetid
 left outer join 
(
select  b.budgetid,nvl(ReceAfterOPAct,0)as ReceAfterOPAct,nvl(amtpaid,0) as amtpaid from   masbudgetdetails b
left outer join 
(
select sum(amount) as ReceAfterOPAct,budgetid from masbudgetdetails where isop !='Y' 
and RECEIVEDDATE between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) and (select ENDDATE from masaccyearsettings where ACCYRSETID=542)
and ISPROVISIONAL is null
 group by budgetid
) tot on tot.budgetid=b.budgetid

left outer join 
(
select so.budgetid,sum(round(s.totnetamount,0))+sum(s.admincharges) as amtpaid from blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid=s.ponoid
where 1=1  and bp.status='P'  and bp.aiddate between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) and (select ENDDATE from masaccyearsettings where ACCYRSETID=542)
group by so.budgetid 
) pa on pa.budgetid=b.budgetid
where 1=1 group by b.budgetid,ReceAfterOPAct,amtpaid

) bd  on bd.budgetid=b.budgetid

left outer join
(
select budgetid,sum(amount) as anticepetryamt,sum(actRecive) as actRecive from
(
select mb.budgetid,mb.bgid,mb.amount,nvl(actRecptAMOUNT,0) as actRecive

from masbudgetdetails mb
left outer join
(
select bgid ,sum(AMOUNT) as actRecptAMOUNT from masbudgetdetailsactualentry
where RECEIVEDDATE  between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) and (select ENDDATE from masaccyearsettings where ACCYRSETID=542)
group by bgid

) mbr on mbr.bgid=mb.bgid
where isop !='Y' and ISPROVISIONAL='Y' 
) group by budgetid
)antrc on antrc.budgetid=b.budgetid 
left outer join
(

select budgetidto as budgetid,totMin as totAdujst from (
select budgetid as budgetidto,'-'||sum(amount) as totMin 
from fundexpense 
where 1=1 and  type = 'FA' 
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate)=542
group by budgetid
union all
 select budgetidto,'+'||sum(amount) as totPlus 
 from fundexpense 
where 1=1 and  type = 'FA'
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate) =542
group by budgetidto
)

)adj on adj.budgetid=b.budgetid

left outer join
(
select budgetid,sum(amount) as RefundAmt from fundexpense 
where 1=1 and  type = 'TD' 
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate)=542
group by budgetid

)rfdcl on rfdcl.budgetid=b.budgetid
where 1=1 and b.budgetid not in (3) " + whbudgetid + @" order by b.budgetid  ";

            }
            if (yearid == "544")
            {
                qry = @" select b.BUDGETID, b.BUDGETNAME,
Round(cl.closingVal/10000000,2) as OPBalance,Round(nvl(cyr.CRTotRecvFund,0)/10000000,2) as ActRecYear,

Round(nvl(cyr.totAdujst,0)/10000000,2) as  totAdujst,Round(nvl(cyr.RefundAmt,0)/10000000,2) as RefundAmt
,Round((((nvl(cyr.CRTotRecvFund,0)+nvl(cl.closingVal,0))-nvl(cyr.totAdujst,0) )-nvl(cyr.RefundAmt,0))/10000000,2) as TotalFundAvl,
Round(nvl(cyr.CRTotPaidVal,0)/10000000,2) as TotPaid
,Round(((( (nvl(cl.closingVal,0) + nvl(cyr.CRTotRecvFund,0))-nvl(cyr.totAdujst,0))-nvl(cyr.CRTotPaidVal,0))-nvl(cyr.RefundAmt,0))/10000000,2)  ClosingBal
from masbudget b
left outer join
(
select b.BUDGETID, b.BUDGETNAME
,op.CLOSINGBAL as OPBalance
,nvl(bd.ReceAfterOPAct,0) as  ReceAfterOPAct, 
nvl(antrc.actRecive,0) as actRecive
,nvl(totAdujst,0) as totAdujst
,((nvl(bd.ReceAfterOPAct,0)+nvl(antrc.actRecive,0))-nvl(totAdujst,0) )-nvl(rfdcl.RefundAmt,0) as TotRecvFund
,nvl(bd.amtpaid,0) as TotPaidVal 
,(( (nvl(op.CLOSINGBAL,0) + nvl(bd.ReceAfterOPAct,0)+nvl(antrc.actRecive,0) )-nvl(totAdujst,0))-nvl(bd.amtpaid,0))-nvl(RefundAmt,0)  closingVal
from masbudget b
left outer join fundclosing541 op on op.budgetid=b.budgetid
 left outer join 
(
select  b.budgetid,nvl(ReceAfterOPAct,0)as ReceAfterOPAct,nvl(amtpaid,0) as amtpaid from   masbudgetdetails b
left outer join 
(
select sum(amount) as ReceAfterOPAct,budgetid from masbudgetdetails where isop !='Y' 
and RECEIVEDDATE between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) and (select ENDDATE from masaccyearsettings where ACCYRSETID=542)
and ISPROVISIONAL is null
 group by budgetid
) tot on tot.budgetid=b.budgetid

left outer join 
(
select so.budgetid,sum(round(s.totnetamount,0))+sum(s.admincharges) as amtpaid from blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid=s.ponoid
where 1=1  and bp.status='P'  and bp.aiddate between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) and (select ENDDATE from masaccyearsettings where ACCYRSETID=542)
group by so.budgetid 
) pa on pa.budgetid=b.budgetid
where 1=1 group by b.budgetid,ReceAfterOPAct,amtpaid

) bd  on bd.budgetid=b.budgetid

left outer join
(
select budgetid,sum(amount) as anticepetryamt,sum(actRecive) as actRecive from
(
select mb.budgetid,mb.bgid,mb.amount,nvl(actRecptAMOUNT,0) as actRecive

from masbudgetdetails mb
left outer join
(
select bgid ,sum(AMOUNT) as actRecptAMOUNT from masbudgetdetailsactualentry
where RECEIVEDDATE  between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) and (select ENDDATE from masaccyearsettings where ACCYRSETID=542)
group by bgid

) mbr on mbr.bgid=mb.bgid
where isop !='Y' and ISPROVISIONAL='Y'
) group by budgetid
)antrc on antrc.budgetid=b.budgetid 
left outer join
(

select budgetidto as budgetid,totMin as totAdujst from (
select budgetid as budgetidto,'-'||sum(amount) as totMin 
from fundexpense 
where 1=1 and  type = 'FA' 
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate)=542
group by budgetid
union all
 select budgetidto,'+'||sum(amount) as totPlus 
 from fundexpense 
where 1=1 and  type = 'FA'
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate) =542
group by budgetidto
)

)adj on adj.budgetid=b.budgetid

left outer join
(
select budgetid,sum(amount) as RefundAmt from fundexpense 
where 1=1 and  type = 'TD' 
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate)=542
group by budgetid

)rfdcl on rfdcl.budgetid=b.budgetid

)cl on cl.budgetid=b.budgetid
left outer join 
(

select b.BUDGETID, b.BUDGETNAME
,op.CLOSINGBAL as OPBalance
,nvl(bd.ReceAfterOPAct,0) as  ReceAfterOPAct, 
nvl(antrc.actRecive,0) as actRecive
,nvl(totAdujst,0) as totAdujst
,((nvl(bd.ReceAfterOPAct,0)+nvl(antrc.actRecive,0))-nvl(totAdujst,0) )-nvl(rfd.RefundAmt,0) as CRTotRecvFund
,nvl(rfd.RefundAmt,0) as RefundAmt
,nvl(bd.amtpaid,0) as CRTotPaidVal 
,((nvl(bd.ReceAfterOPAct,0)+nvl(antrc.actRecive,0))-nvl(totAdujst,0))+nvl(bd.amtpaid,0) closingVal
from masbudget b
left outer join fundclosing541 op on op.budgetid=b.budgetid
 left outer join 
(
select  b.budgetid,nvl(ReceAfterOPAct,0)as ReceAfterOPAct,nvl(amtpaid,0) as amtpaid from   masbudgetdetails b
left outer join 
(
select sum(amount) as ReceAfterOPAct,budgetid from masbudgetdetails where isop !='Y' 
and RECEIVEDDATE between (select STARTDATE from masaccyearsettings where ACCYRSETID=544) and (select ENDDATE from masaccyearsettings where ACCYRSETID=544)
and ISPROVISIONAL is null
 group by budgetid
) tot on tot.budgetid=b.budgetid

left outer join 
(
select so.budgetid,sum(round(s.totnetamount,0))+sum(s.admincharges) as amtpaid from blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid=s.ponoid
where 1=1  and bp.status='P'  and bp.aiddate between (select STARTDATE from masaccyearsettings where ACCYRSETID=544) and (select ENDDATE from masaccyearsettings where ACCYRSETID=544)
group by so.budgetid 
) pa on pa.budgetid=b.budgetid
where 1=1 group by b.budgetid,ReceAfterOPAct,amtpaid

) bd  on bd.budgetid=b.budgetid

left outer join
(
select budgetid,sum(amount) as anticepetryamt,sum(actRecive) as actRecive from
(
select mb.budgetid,mb.bgid,mb.amount,nvl(actRecptAMOUNT,0) as actRecive

from masbudgetdetails mb
left outer join
(
select bgid ,sum(AMOUNT) as actRecptAMOUNT from masbudgetdetailsactualentry
where RECEIVEDDATE  between (select STARTDATE from masaccyearsettings where ACCYRSETID=544) and (select ENDDATE from masaccyearsettings where ACCYRSETID=544)
group by bgid

) mbr on mbr.bgid=mb.bgid
where isop !='Y' and ISPROVISIONAL='Y'
) group by budgetid
)antrc on antrc.budgetid=b.budgetid 
left outer join
(

select budgetidto as budgetid,totMin as totAdujst from (
select budgetid as budgetidto,'-'||sum(amount) as totMin 
from fundexpense 
where 1=1 and  type = 'FA' 
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate)=544
group by budgetid
union all
 select budgetidto,'+'||sum(amount) as totPlus 
 from fundexpense 
where 1=1 and  type = 'FA'
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate) =544
group by budgetidto
)

)adj on adj.budgetid=b.budgetid

left outer join
(
select budgetid,sum(amount) as RefundAmt from fundexpense 
where 1=1 and  type = 'TD' 
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate)=542
group by budgetid

)rfd on rfd.budgetid=b.budgetid


)cyr on cyr.budgetid=b.budgetid
where 1=1 and b.budgetid not in (3) " + whbudgetid + @" order by b.budgetid ";


            }
            if (yearid == "545") //About to change by kaushal sir
            {
                qry = @" select b.BUDGETID, b.BUDGETNAME,
Round(cl.closingVal/10000000,2) as OPBalance,Round(nvl(cyr.CRTotRecvFund,0)/10000000,2) as ActRecYear,

Round(nvl(cyr.totAdujst,0)/10000000,2) as  totAdujst,Round(nvl(cyr.RefundAmt,0)/10000000,2) as RefundAmt
,Round((((nvl(cyr.CRTotRecvFund,0)+nvl(cl.closingVal,0))-nvl(cyr.totAdujst,0) )-nvl(cyr.RefundAmt,0))/10000000,2) as TotalFundAvl,
Round(nvl(cyr.CRTotPaidVal,0)/10000000,2) as TotPaid
,Round(((( (nvl(cl.closingVal,0) + nvl(cyr.CRTotRecvFund,0))-nvl(cyr.totAdujst,0))-nvl(cyr.CRTotPaidVal,0))-nvl(cyr.RefundAmt,0))/10000000,2)  ClosingBal
from masbudget b
left outer join
(
select b.BUDGETID, b.BUDGETNAME
,op.CLOSINGBAL as OPBalance
,nvl(bd.ReceAfterOPAct,0) as  ReceAfterOPAct, 
nvl(antrc.actRecive,0) as actRecive
,nvl(totAdujst,0) as totAdujst
,((nvl(bd.ReceAfterOPAct,0)+nvl(antrc.actRecive,0))-nvl(totAdujst,0) )-nvl(rfdcl.RefundAmt,0) as TotRecvFund
,nvl(bd.amtpaid,0) as TotPaidVal 
,(( (nvl(op.CLOSINGBAL,0) + nvl(bd.ReceAfterOPAct,0)+nvl(antrc.actRecive,0) )-nvl(totAdujst,0))-nvl(bd.amtpaid,0))-nvl(RefundAmt,0)  closingVal
from masbudget b
left outer join fundclosing541 op on op.budgetid=b.budgetid
 left outer join 
(
select  b.budgetid,nvl(ReceAfterOPAct,0)as ReceAfterOPAct,nvl(amtpaid,0) as amtpaid from   masbudgetdetails b
left outer join 
(
select sum(amount) as ReceAfterOPAct,budgetid from masbudgetdetails where isop !='Y' 
and RECEIVEDDATE between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) and (select ENDDATE from masaccyearsettings where ACCYRSETID=542)
and ISPROVISIONAL is null
 group by budgetid
) tot on tot.budgetid=b.budgetid

left outer join 
(
select so.budgetid,sum(round(s.totnetamount,0))+sum(s.admincharges) as amtpaid from blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid=s.ponoid
where 1=1  and bp.status='P'  and bp.aiddate between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) and (select ENDDATE from masaccyearsettings where ACCYRSETID=542)
group by so.budgetid 
) pa on pa.budgetid=b.budgetid
where 1=1 group by b.budgetid,ReceAfterOPAct,amtpaid

) bd  on bd.budgetid=b.budgetid

left outer join
(
select budgetid,sum(amount) as anticepetryamt,sum(actRecive) as actRecive from
(
select mb.budgetid,mb.bgid,mb.amount,nvl(actRecptAMOUNT,0) as actRecive

from masbudgetdetails mb
left outer join
(
select bgid ,sum(AMOUNT) as actRecptAMOUNT from masbudgetdetailsactualentry
where RECEIVEDDATE  between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) and (select ENDDATE from masaccyearsettings where ACCYRSETID=542)
group by bgid

) mbr on mbr.bgid=mb.bgid
where isop !='Y' and ISPROVISIONAL='Y'
) group by budgetid
)antrc on antrc.budgetid=b.budgetid 
left outer join
(

select budgetidto as budgetid,totMin as totAdujst from (
select budgetid as budgetidto,'-'||sum(amount) as totMin 
from fundexpense 
where 1=1 and  type = 'FA' 
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate)=542
group by budgetid
union all
 select budgetidto,'+'||sum(amount) as totPlus 
 from fundexpense 
where 1=1 and  type = 'FA'
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate) =542
group by budgetidto
)

)adj on adj.budgetid=b.budgetid

left outer join
(
select budgetid,sum(amount) as RefundAmt from fundexpense 
where 1=1 and  type = 'TD' 
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate)=542
group by budgetid

)rfdcl on rfdcl.budgetid=b.budgetid

)cl on cl.budgetid=b.budgetid
left outer join 
(

select b.BUDGETID, b.BUDGETNAME
,op.CLOSINGBAL as OPBalance
,nvl(bd.ReceAfterOPAct,0) as  ReceAfterOPAct, 
nvl(antrc.actRecive,0) as actRecive
,nvl(totAdujst,0) as totAdujst
,((nvl(bd.ReceAfterOPAct,0)+nvl(antrc.actRecive,0))-nvl(totAdujst,0) )-nvl(rfd.RefundAmt,0) as CRTotRecvFund
,nvl(rfd.RefundAmt,0) as RefundAmt
,nvl(bd.amtpaid,0) as CRTotPaidVal 
,((nvl(bd.ReceAfterOPAct,0)+nvl(antrc.actRecive,0))-nvl(totAdujst,0))+nvl(bd.amtpaid,0) closingVal
from masbudget b
left outer join fundclosing541 op on op.budgetid=b.budgetid
 left outer join 
(
select  b.budgetid,nvl(ReceAfterOPAct,0)as ReceAfterOPAct,nvl(amtpaid,0) as amtpaid from   masbudgetdetails b
left outer join 
(
select sum(amount) as ReceAfterOPAct,budgetid from masbudgetdetails where isop !='Y' 
and RECEIVEDDATE between (select STARTDATE from masaccyearsettings where ACCYRSETID=544) and (select ENDDATE from masaccyearsettings where ACCYRSETID=544)
and ISPROVISIONAL is null
 group by budgetid
) tot on tot.budgetid=b.budgetid

left outer join 
(
select so.budgetid,sum(round(s.totnetamount,0))+sum(s.admincharges) as amtpaid from blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid=s.ponoid
where 1=1  and bp.status='P'  and bp.aiddate between (select STARTDATE from masaccyearsettings where ACCYRSETID=544) and (select ENDDATE from masaccyearsettings where ACCYRSETID=544)
group by so.budgetid 
) pa on pa.budgetid=b.budgetid
where 1=1 group by b.budgetid,ReceAfterOPAct,amtpaid

) bd  on bd.budgetid=b.budgetid

left outer join
(
select budgetid,sum(amount) as anticepetryamt,sum(actRecive) as actRecive from
(
select mb.budgetid,mb.bgid,mb.amount,nvl(actRecptAMOUNT,0) as actRecive

from masbudgetdetails mb
left outer join
(
select bgid ,sum(AMOUNT) as actRecptAMOUNT from masbudgetdetailsactualentry
where RECEIVEDDATE  between (select STARTDATE from masaccyearsettings where ACCYRSETID=544) and (select ENDDATE from masaccyearsettings where ACCYRSETID=544)
group by bgid

) mbr on mbr.bgid=mb.bgid
where isop !='Y' and ISPROVISIONAL='Y'
) group by budgetid
)antrc on antrc.budgetid=b.budgetid 
left outer join
(

select budgetidto as budgetid,totMin as totAdujst from (
select budgetid as budgetidto,'-'||sum(amount) as totMin 
from fundexpense 
where 1=1 and  type = 'FA' 
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate)=544
group by budgetid
union all
 select budgetidto,'+'||sum(amount) as totPlus 
 from fundexpense 
where 1=1 and  type = 'FA'
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate) =544
group by budgetidto
)

)adj on adj.budgetid=b.budgetid

left outer join
(
select budgetid,sum(amount) as RefundAmt from fundexpense 
where 1=1 and  type = 'TD' 
and (select accyrsetid from masaccyearsettings where chequedate between startdate and enddate)=542
group by budgetid

)rfd on rfd.budgetid=b.budgetid


)cyr on cyr.budgetid=b.budgetid
where 1=1 and b.budgetid not in (3) " + whbudgetid + @" order by b.budgetid ";


            }

            var myList = _context.FundBalanceDbSet
.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;


        }

        [HttpGet("getItemDetailsWithHOD")]
        public async Task<ActionResult<IEnumerable<ItemIndentstockIssueDTO>>> getItemDetailsWithHOD(string mcid, string itemid, string groupid, string? itemtypeid, string edltype, string edlcat, string yearid, string dhsai, string dmai, string totalai, string redycnt, string uqccnt, string pipelinecnt, string rccnt, string whid)
        {


            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid=" + mcid;
            }
            string whmitemid = ""; string whmiitemid = ""; string whtbiitemid = "";
            if (itemid != "0")
            {
                whmitemid = " and m.itemid=" + itemid;
                whmiitemid = " and mi.itemid=" + itemid;
                whtbiitemid = " and tbi.itemid=" + itemid;
            }
            string whgroupid = "";
            if (groupid != "0")
            {
                whgroupid = " and m.groupid=" + groupid;
            }
            string whitemtypeid = "";
            if (itemtypeid != "0")
            {
                whitemtypeid = " and m.itemtypeid=" + itemtypeid;
            }
            string whedltype = "";
            if (edltype == "Y")
            {
                whedltype = " and m.isedl2021=Y";
            }

            if (edltype == "N")
            {
                whedltype = " and (case when m.isedl2021='Y' then 1 else 0 end)=0";
            }
            string whedlcat = "";
            if (edlcat != "0")
            {
                whedlcat = " and e.edlcat='" + edlcat + "'";
            }
            string whyearid = "";
            FacOperations f = new FacOperations(_context);

            string yearid1 = f.getACCYRSETID();
            if (yearid == "0")
            {
                whyearid = yearid1;
            }
            else
            {
                whyearid = yearid;
            }

            string whdhai = "";
            string whindentjoin = " left outer join ";
            if (dhsai == "Y")
            {
                whindentjoin = " inner join ";
                whdhai = " and (nvl(sum(DHS_INDENTQTY),0)+nvl(sum(MITANIN),0))>0 ";
            }


            string whtotalai = "";

            if (totalai == "Y")
            {
                whindentjoin = " inner join ";
                whdhai = " and (nvl(sum(DHS_INDENTQTY),0)+nvl(sum(MITANIN),0)+nvl(sum(DME_INDENTQTY),0))>0 ";
            }

            string whdmeai = "";

            if (dmai == "Y")
            {
                whindentjoin = " inner join ";
                whdmeai = " and (nvl(sum(DME_INDENTQTY),0))>0 ";
            }
            // string redycnt,string uqccnt,string pipelinecnt,string rccnt

            string whreadycnt = "";
            string whreadyuqcjoin = " left outer join ";
            if (redycnt == "Y")
            {
                whreadyuqcjoin = " inner join ";
                whreadycnt = " and (case when nvl(READY,0)>0 then 1 else 0 end)=1";
            }
            if (redycnt == "N")
            {

                whreadycnt = " and (case when nvl(READY,0)>0 then 1 else 0 end)=0";
            }

            string whonlyuqccnt = "";

            if (uqccnt == "Y")
            {
                whreadyuqcjoin = " inner join ";
                whonlyuqccnt = " and (case when nvl(UQC,0)>0 and nvl(READY,0)=0 then 1 else 0 end)=1";
            }


            string whonlypipeline = "";

            string whpipelinejoin = " left outer join ";
            if (pipelinecnt == "Y")
            {
                whpipelinejoin = " inner join ";
                whonlypipeline = " and (case when nvl(TOTLPIPELINE,0)>0 and (nvl(READY,0)+nvl(UQC,0))=0 then 1 else 0 end)=1";
            }

            string whonlyrc = "";

            string whonlyrcjoin = " left outer join ";
            if (rccnt == "Y")
            {
                whonlyrcjoin = " inner join ";
                whonlyrc = " and (case when r.itemid is not null then 1 else 0 end)=1";
            }

            string twhclause = "";
            string sowhclause = "";

            if (whid != "0")
            {
                twhclause = " and  t.warehouseid=" + whid;
                sowhclause = " and   soi.warehouseid=" + whid;
            }




            string qry = @"
select  nvl(g.groupname,'NA') groupname
,nvl(DHSAI,0) DHSAI ,nvl(dmeai,0) dmeai,nvl(DHSAI,0)+nvl(dmeai,0) as TotalAI
,m.itemcode,m.itemname,m.strength1,m.unit,ty.itemtypename
,nvl(READY,0) as readystock,nvl(UQC,0) as QCstock,nvl(TOTLPIPELINE,0) as totalpiplie
,case when m.isedl2021='Y' then 1 else 0 end as edltype,e.edl
,case when r.itemid is not  null then 'RC Valid' else 'RC Not Valid' end as rcstatus,RCRate,RCStartDT,RCEndDT
,nvl(dmeiss_qty,0) dmeissue,nvl(dhsiss_qty,0) dhsissue,nvl(dmeiss_qty,0)+nvl(dhsiss_qty,0) as totalissue
, nvl(dhspoqty,0) as dhspoqty ,nvl(DHSRQTY,0) as DHSRQTY,nvl(dmepoqty,0) as dmepoqty ,nvl(DMERQTY,0) DMERQTY

,case when r.itemid is not null then 1 else 0 end as RC ,case when nvl(READY,0)>0 then 1 else 0 end as Readycnt
,case when nvl(UQC,0)>0 and nvl(READY,0)=0 then 1 else 0 end as Uqccount,nvl(g.groupid,1) as groupid,
case when nvl(TOTLPIPELINE,0)>0 and (nvl(READY,0)+nvl(UQC,0))=0 then 1 else 0 end as piplinecnt,m.itemid,m.itemtypeid
from masitems m
 inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join masedl e on e.edlcat=m.edlcat
left outer join masitemtypes ty on ty.itemtypeid=m.itemtypeid
" + whindentjoin + @"
(
select nvl(sum(DHS_INDENTQTY),0)+nvl(sum(MITANIN),0) as DHSAI,nvl(sum(DME_INDENTQTY),0) as dmeai,i.itemid from itemindent i where  accyrsetid=" + whyearid + @" " + whdhai + @" " + whdmeai + @"
group by itemid
) i on i.itemid=m.itemid

left outer join masitemgroups g on g.groupid=m.groupid

" + whonlyrcjoin + @"
(
select itemid,Round(min(FINALRATEGST),2) as RCRate,min(RCSTART) RCStartDT,min(RCENDDT) as RCEndDT from v_rcvalid
 group by itemid
) r on r.itemid = m.itemid


" + whreadyuqcjoin + @"
(
select itemid,sum(nvl(ReadyForIssue,0)) as READY,sum(nvl(Pending,0)) as UQC from 
(
 select b.batchno, b.expdate, b.inwno, m.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when m.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when m.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems m on m.itemid= i.itemid
 inner join soordereditems odi on odi.ponoid = b.ponoid
 inner join aoccontractitems ci on ci.contractitemid = odi.contractitemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C' " + whgroupid + @"  " + whmitemid + @" " + whitemtypeid + @"  " + whedltype + @" " + twhclause + @"  And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
     ) group by itemid
) st on st.itemid=m.itemid


" + whpipelinejoin + @"
(
  select itemid, sum(pipelineQTY) TOTLPIPELINE from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid
 where op.status  in ('C','O')   " + whgroupid + @"  " + whmitemid + @" " + whitemtypeid + @"  " + whedltype + @" " + sowhclause + @" 
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having (case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) group by itemid
) pip on pip.itemid = m.itemid



left outer join 
(
select sum(iss_qty) as dhsiss_qty
,di.itemid 
from 
(
select tbi.itemid,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty 
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join masitems m on m.itemid= tbi.itemid
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes ty on ty.facilitytypeid=f.facilitytypeid 
        where tb.status = 'C'  " + whgroupid + @"  " + whmitemid + @" " + whitemtypeid + @"  " + whedltype + @"  and tb.issuetype='NO' and ty.facilitytypeid not in (371,378,364)
 and  tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + whyearid + @" ) 
 and (select enddate from masaccyearsettings where accyrsetid=" + whyearid + @" ) 
        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        group by tbi.itemid

        ) di 
           group by di.itemid 

) iss on iss.itemid=m.itemid

left outer join 
(
select sum(dmeiss_qty) as dmeiss_qty
,di.itemid 
from 
(
select tbi.itemid,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) dmeiss_qty 
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
        inner join masitems m on m.itemid= tbi.itemid
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes ty on ty.facilitytypeid=f.facilitytypeid 
        where tb.status = 'C'  " + whgroupid + @"  " + whmitemid + @" " + whitemtypeid + @"  " + whedltype + @"   and tb.issuetype='NO' and ty.facilitytypeid  in (378,364)
 and  tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + whyearid + @" ) 
 and (select enddate from masaccyearsettings where accyrsetid=" + whyearid + @") 
        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        group by tbi.itemid

        ) di 
           group by di.itemid 

) issdme on issdme.itemid=m.itemid


left outer join 
(

select itemid,nvl(sum(dhspoqty),0) as dhspoqty,sum(nvl(DHSRQTY,0)) as DHSRQTY,sum(nvl(dmepoqty,0)) as dmepoqty,sum(nvl(DMERQTY,0)) as DMERQTY
from 
(

select  op.ponoid,
                            oi.absqty as orderedqty,
                            nvl(sum(od.dhsqty), 0) dhspoqty,
                            nvl(sum(od.dhsqty), 0) * c.finalrategst dhspovalue,
                            nvl(r1.dhsrqty, 0) dhsrqty, nvl(r1.dhsrqty, 0) * c.finalrategst dhsrvalue,
                            nvl(sum(od.dmeqty), 0) dmepoqty,
                            nvl(sum(od.dmeqty), 0) * c.finalrategst dmepovalue,
                            nvl(r2.dmerqty, 0) dmerqty, nvl(r2.dmerqty, 0) * c.finalrategst dmervalue,
                            case when nvl(sum(od.dhsqty), 0) > 0 then 1 else 0 end dhspocnt,
                            case when nvl(sum(od.dmeqty), 0) > 0 then 1 else 0 end dmepocnt
                            , op.ACCYRSETID,m.itemid,y.SHACCYEAR as ACCYEAR
                            from masItems m
inner join soOrderedItems OI on(OI.ItemID = m.ItemID)
inner join soorderplaced op on(op.ponoid = oi.ponoid and op.status not in ('OC', 'WA1', 'I'))
 inner join masaccyearsettings y on y.ACCYRSETID=op.AIFINYEAR                                             
inner join soorderdistribution od on(od.orderitemid = oi.orderitemid)
inner join aoccontractitems c on c.contractitemid = oi.contractitemid
left outer join
(
select i.itemid, t.ponoid, nvl(sum(tb.absrqty), 0) as dhsrqty
                                                from tbreceipts t
                                                inner
                                                join tbreceiptitems i on (i.receiptid = t.receiptid)
                                                inner
                                                join tbreceiptbatches tb on (i.receiptitemid = tb.receiptitemid)
                                                where T.Status = 'C' and  T.receipttype = 'NO'
                                                and t.ponoid in (select ponoid from soorderplaced where deptid in (367, 371))
                                                and t.receiptid not in (select tr.receiptid
from tbindents t
inner join tbindentitems i on(i.indentid = t.indentid)
inner join tboutwards o on(o.indentitemid = i.indentitemid)
inner join tbreceiptbatches tb on(tb.inwno = o.inwno)
inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
inner join tbreceipts tr on tr.receiptid = ti.receiptid
where t.status = 'C' and t.issuetype in ('RS') )
                                                group by I.ItemID, T.PoNoID
                                                ) r1 on(r1.itemid = oi.itemid and r1.ponoid = op.ponoid)    
                                                
                                                left outer join
                                                (
                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty), 0) as dmerqty
                                                from tbreceipts t
                                                inner
                                                join tbreceiptitems i on (i.receiptid = t.receiptid)
                                                inner
                                                join tbreceiptbatches tb on (i.receiptitemid = tb.receiptitemid)
                                                where T.Status = 'C' and  T.receipttype = 'NO'
                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 364)
                                                and t.receiptid not in (select tr.receiptid
from tbindents t
inner join tbindentitems i on(i.indentid = t.indentid)
inner join tboutwards o on(o.indentitemid = i.indentitemid)
inner join tbreceiptbatches tb on(tb.inwno = o.inwno)
inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
inner join tbreceipts tr on tr.receiptid = ti.receiptid
where t.status = 'C' and t.issuetype in ('RS') )
group by I.ItemID, T.PoNoID
) r2 on(r2.itemid = oi.itemid and r2.ponoid = op.ponoid)
where 1 = 1  and op.AIFINYEAR =" + whyearid + @"  " + whmitemid + @" " + whgroupid + @"  " + whitemtypeid + @"  " + whedltype + @"
 group by op.ponoid,oi.absqty,c.finalrategst,r1.dhsrqty, r2.dmerqty ,op.ACCYRSETID,m.itemid,y.SHACCYEAR
) group by itemid         
) po on po.itemid=m.itemid
where m.ISFREEZ_ITPR is null  " + whmcid + @"  " + whmitemid + @" " + whgroupid + @"  " + whitemtypeid + @"  " + whedltype + @" " + whedlcat + @" " + whreadycnt + @"  " + whonlyuqccnt + @" " + whonlypipeline + @" " + whonlyrc + @" order by m.itemname";


            var myList = _context.ItemIndentstockIssueDbset
.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



        [HttpGet("getDhsDmeStock")]
        public async Task<ActionResult<IEnumerable<DhsDmeStockDTO>>> getDhsDmeStock(string itemid, string hodid,string whid , string distid,string facilityid,string userid, string coll_cmho)
        {

            //field stock
            string whItemCode = "";
            String whItemCode1 = "";
            FacOperations f = new FacOperations(_context);
            string itemCode = f.getitemcode(itemid);



            string whDistId = "";
            string districtid = "";
            string whWarehouseId = "";
            if (whid != "0")
            {
                whWarehouseId = " and d.warehouseid =" + whid;
            }

            if (coll_cmho == "Collector")

            {
                districtid = f.getCollectorDisid(userid);
                string cmhoid = f.getCMHOFacFromDistrict(districtid);
                Int64 whiddata = f.getWHID(cmhoid.ToString());
                whWarehouseId = " and d.warehouseid =" + Convert.ToString(whiddata);
                whDistId = " and d.districtid =" + districtid;


            }
            if (coll_cmho == "CMHO")
            {
                districtid = Convert.ToString(f.geFACTDistrictid(userid));
                string cmhoid = f.getCMHOFacFromDistrict(districtid);
                Int64 whiddata = f.getWHID(cmhoid.ToString());
                whWarehouseId = " and d.warehouseid =" + Convert.ToString(whiddata);
                whDistId = " and d.districtid =" + districtid;

            }


            if (string.IsNullOrEmpty(itemCode.Trim()) || itemCode == "0")
            {
                whItemCode = " ";
                whItemCode1 = " ";
            }
            else
            {
                // whItemCode = " and mi.edlitemcode='" + itemCode + "' ";
                whItemCode = " and mi.itemid=" + itemid + " ";
                //whItemCode1 = " and m.itemid='" + itemid + "' ";
                whItemCode1 = " and m.itemid=" + itemid + " ";
            }

            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and t.hodid =" + hodid;
            }

       

     
            if (distid != "0")
            {
                whDistId = " and d.districtid =" + distid;
            }

            string whFacId = "";
            if (facilityid != "0")
            {
                whFacId = " and f.facilityid =" + facilityid;
            }


            string qry = @"    select  m.itemname, m.itemid,m.itemcode,m.unitcount,nvl(DHSStock,0)+nvl(LPstock,0) as fieldstock
  ,round((nvl(DHSStock,0)+nvl(LPstock,0))/nvl(m.unitcount,1),0) as fieldstocksku
  from masitems m 
inner join 
                     (
                     select sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   DHSStock ,"+ itemid + @" as itemid
                from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                inner join masitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 inner join masdistricts d on d.districtid=f.districtid
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid "+ whhodid + @"
                left outer join 
                (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                    inner join masfacilities f  on f.facilityid=fs.facilityid 
                  inner join masdistricts d on d.districtid=f.districtid
                  where fs.status = 'C'   and f.isactive=1       "+ whDistId + @"  and fsi.itemid=" + itemid + @"     "+ whFacId + @"      
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  and f.isactive=1   and T.Status = 'C' and b.expdate>sysdate  
                  and mi.itemid=" + itemid + @"    "+ whWarehouseId + @"   "+ whDistId + @" "+ whFacId + @" 
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0 
              ) dhs on dhs.itemid=m.itemid
              
              
                           left outer join
                     (
                                   select sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   LPstock ," + itemid + @" as itemid
                from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                inner join lpmasitems mi on mi.lpitemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid  "+ whFacId + @"
                 inner join masdistricts d on d.districtid=f.districtid
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid "+ whhodid + @"
                left outer join 
                (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                    inner join masfacilities f  on f.facilityid=fs.facilityid 
                   inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid "+ whhodid + @"
                       inner join masdistricts d on d.districtid=f.districtid
                  where fs.status = 'C'       and f.isactive=1  " + whWarehouseId + @" "+ whFacId + @"
                 " + whDistId + @"
                  and fsi.itemid in ( select LPITEMID from lpmasitems where lpmasitems.edlitemcode='" + itemCode + @"')           
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  and f.isactive=1   and T.Status = 'C' and b.expdate>sysdate  and mi.edlitemcode is not null
                  and mi.lpitemid in ( select LPITEMID from lpmasitems where lpmasitems.edlitemcode='" + itemCode + @"')   
               " + whDistId + @"
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0 
                     ) LPst on LPst.itemid=m.itemid

        where 1=1  and m.itemid = " + itemid + @"  ";

            var myList = _context.DhsDmeStockDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("getfacwiseSTockIssuanceCoonsumptionm")]
        public async Task<ActionResult<IEnumerable<DistFactStockDTO>>> getfacwiseSTockIssuanceCoonsumptionm(string itemid, string hodid, string distid, string userid, string coll_cmho)
        {
            string whItemCode = "";
            String whItemCode1 = "";
            FacOperations f = new FacOperations(_context);
            string itemCode = f.getitemcode(itemid);

            string yearid = f.getACCYRSETID();

            string whDistId = "";
            string districtid = "";
            string whWarehouseId = "";


            string fwhDistId = "";
            if (coll_cmho == "Collector")

            {
                districtid = f.getCollectorDisid(userid);
                string cmhoid = f.getCMHOFacFromDistrict(districtid);
                Int64 whiddata = f.getWHID(cmhoid.ToString());
                whWarehouseId = " and d.warehouseid =" + Convert.ToString(whiddata);
                whDistId = " and d.districtid =" + districtid;
                fwhDistId = " and f.districtid =" + districtid;

            }
            if (coll_cmho == "CMHO")
            {
                districtid = Convert.ToString(f.geFACTDistrictid(userid));
                string cmhoid = f.getCMHOFacFromDistrict(districtid);
                Int64 whiddata = f.getWHID(cmhoid.ToString());
                whWarehouseId = " and d.warehouseid =" + Convert.ToString(whiddata);
                whDistId = " and d.districtid =" + districtid;
                fwhDistId = " and f.districtid =" + districtid;
            }


            if (string.IsNullOrEmpty(itemCode.Trim()) || itemCode == "0")
            {
                whItemCode = " ";
                whItemCode1 = " ";
            }
            else
            {
                // whItemCode = " and mi.edlitemcode='" + itemCode + "' ";
                whItemCode = " and mi.itemid=" + itemid + " ";
                //whItemCode1 = " and m.itemid='" + itemid + "' ";
                whItemCode1 = " and m.itemid=" + itemid + " ";
            }

            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and t.hodid =" + hodid;
            }
            if (hodid == "0")
            {
                whhodid = " and t.hodid  in (2,3,7)";
            }


            if (distid != "0")
            {
                whDistId = " and d.districtid =" + distid;
                fwhDistId = " and f.districtid =" + distid;
            }



            string qry = @"      select f.facilityid,f.facilityname,nvl(iss.WHIssued,0) as WHIssued,nvl(issueqty,0) as consumption 
   ,nvl(LPst.LPstock,0) as LPstock,nvl(st.stock,0) as stock,nvl(LPst.LPstock,0)+nvl(st.stock,0) as totalstock
   ,ROW_NUMBER() OVER ( ORDER BY t.orderdp ) AS ID
   ,to_Char(ROW_NUMBER() OVER ( ORDER BY t.orderdp ))||'.'||f.facilityname as facname
   from masfacilities f
     inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
   left outer join 
 (
    select f.facilityid,sum(nvl(ftbo.issueqty,0)) issueqty  
                     from tbfacilityissues fs 
                     inner join masfacilities f  on f.facilityid=fs.facilityid 
                      inner join masdistricts d on d.districtid=f.districtid
                     inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                     inner join vmasitems mi on mi.itemid=fsi.itemid 
                     inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                     inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid "+ whhodid + @"
                     where fs.status = 'C'  and fs.ISSUETYPE  ='NO'
                     and fs.ISSUEDDATE between (select startdate from masaccyearsettings where accyrsetid=" + yearid+ @" )
                     and (select enddate from masaccyearsettings where accyrsetid=" + yearid+@")
                     and f.isactive=1    and mi.edlitemcode='" + itemCode + @"'    "+whWarehouseId+@"   " +whDistId+ @"
                     group by mi.unitcount ,f.facilityid 
                     ) c on c.facilityid=f.facilityid    
                     
                     left outer join 
                     (
                     select f.facilityid,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*nvl(m.unitcount,1) WHIssued
 from tbindents tb
 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
 inner join masitems m on m.itemid=tbi.itemid
inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masdistricts d on d.districtid=f.districtid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid "+ whhodid + @"
where  1=1 and tbi.itemid=" + itemid + @" and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid="+yearid+@") 
and (select enddate from masaccyearsettings where accyrsetid=" + yearid+@")
"+ whhodid + @"  "+whDistId+ @"
group by f.facilityid,m.unitcount
                     ) iss on iss.facilityid=f.facilityid
                     
                           left outer join
                     (
                                   select sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   LPstock ,f.facilityid
                from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                inner join lpmasitems mi on mi.lpitemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 inner join masdistricts d on d.districtid=f.districtid
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid "+ whhodid + @"
                left outer join 
                (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                    inner join masfacilities f  on f.facilityid=fs.facilityid 
               inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid "+ whhodid + @"
                              inner join masdistricts d on d.districtid=f.districtid
                  where fs.status = 'C'       and f.isactive=1 
                  " + whDistId+ @"
                  and fsi.itemid in ( select LPITEMID from lpmasitems where lpmasitems.edlitemcode='"+ itemCode + @"')           
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  and f.isactive=1   and T.Status = 'C' and b.expdate>sysdate  and mi.edlitemcode is not null
                  and mi.lpitemid in ( select LPITEMID from lpmasitems where lpmasitems.edlitemcode='"+ itemCode + @"')   
                  " + whDistId+ @"
                  group by f.facilityid
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0 
                     ) LPst on LPst.facilityid=f.facilityid
                     left outer join
                     (
                     select sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   stock ,f.facilityid
                from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                inner join masitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid 
             inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid "+ whhodid + @"
                 inner join masdistricts d on d.districtid=f.districtid
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
                left outer join 
                (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                    inner join masfacilities f  on f.facilityid=fs.facilityid 
             inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid "+ whhodid + @"
                  where fs.status = 'C'   and f.isactive=1  " + fwhDistId + @"   and fsi.itemid=" + itemid + @"              
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  and f.isactive=1   and T.Status = 'C' and b.expdate>sysdate  
                  and mi.itemid=" + itemid + @"    "+ whWarehouseId + @"    "+whDistId+ @"   group by f.facilityid
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0 
              )st on st.facilityid=f.facilityid 
                     where  1=1 and f.isactive=1  "+ fwhDistId + @"
                    "+whhodid+@"
                     order by t.orderdp ";
            var myList = _context.DistFactStockDBset
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("dhsDmeYearConsumption")]
        public async Task<ActionResult<IEnumerable<dhsDmeYearConsumptionDTO>>> dhsDmeYearConsumption(string itemid, string hodid, string whid, string distid, string facilityid, string userid, string coll_cmho)
        {
            //Gyan
            //field consumption
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();
            string whItemCode = "";
            String whItemCode1 = "";
            string itemCode = f.getitemcode(itemid);
            string districtid = "";
            string whWarehouseId = "";
            string whDistId = "";
            if (distid != "0")
            {
                whDistId = " and d.districtid =" + distid;
            }

            if (whid != "0")
            {
                whWarehouseId = " and d.warehouseid =" + whid;
            }
            if (coll_cmho == "Collector")

            {
                districtid = f.getCollectorDisid(userid);
                string cmhoid = f.getCMHOFacFromDistrict(districtid);
                Int64 whiddata = f.getWHID(cmhoid.ToString());
                whWarehouseId = " and d.warehouseid =" + Convert.ToString(whiddata);
                whDistId = " and d.districtid =" + districtid;

            }
            if (coll_cmho == "CMHO")
            {
                districtid = Convert.ToString(f.geFACTDistrictid(userid));
                string cmhoid = f.getCMHOFacFromDistrict(districtid);
                Int64 whiddata = f.getWHID(cmhoid.ToString());

                whWarehouseId = " and d.warehouseid =" + Convert.ToString(whiddata);
                whDistId = " and d.districtid =" + districtid;

            }



            if (string.IsNullOrEmpty(itemCode.Trim()) || itemCode == "0")
            {
                whItemCode = " ";
            }
            else
            {
                whItemCode = " and mi.edlitemcode='" + itemCode + "' ";
            }

            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and t.hodid =" + hodid;
            }

         

        
            string whFacId = "";
            if (facilityid != "0")
            {
                whFacId = " and f.facilityid =" + facilityid;
            }

            string qry = @"    select 1 as hodid,sum(nvl(ftbo.issueqty,0)) issueqty ,mi.unitcount,Round(sum(nvl(ftbo.issueqty,0))/nvl(mi.unitcount,1),0)IssuedSKU 
                     from tbfacilityissues fs 
                     inner join masfacilities f  on f.facilityid=fs.facilityid 
                      inner join masdistricts d on d.districtid=f.districtid
                     inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                     inner join vmasitems mi on mi.itemid=fsi.itemid 
                     inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                     inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
                     where fs.status = 'C'  and fs.ISSUETYPE  ='NO'
                     and fs.ISSUEDDATE between (select startdate from masaccyearsettings where accyrsetid=" + yearid + @" )
                     and (select enddate from masaccyearsettings where accyrsetid= " + yearid + @")
                     and f.isactive=1   " + whItemCode + @"  " + whhodid + @" "+ whWarehouseId + @" "+ whDistId + @"  "+ whFacId + @" 
                     group by mi.unitcount  ";

            var myList = _context.dhsDmeYearConsumptionDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("districtWiseDhsDmeStock")]
        public async Task<ActionResult<IEnumerable<districtWiseDhsDmeStockDTO>>> districtWiseDhsDmeStock(string itemid)
        {
            FacOperations f = new FacOperations(_context);
            string itemCode = f.getitemcode(itemid);


            string qry = @"  select districtid as ID, districtid,districtname,edlitemcode,sum(DHSStock) as DHSStock,sum(DMEStock) as DMEStock
  from 
  (
  select mi.edlitemcode,case when t.hodid=2 then sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0)) else 0 end as   DHSStock ,d.districtid,d.districtname,case when t.hodid=3 then  sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0)) else 0 end as DMEStock
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 inner join masdistricts d on d.districtid=f.districtid
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'                 
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  and f.isactive=1 and t.hodid in (2,3)
                 and T.Status = 'C' and b.expdate>sysdate  and mi.edlitemcode is not null
                  and mi.edlitemcode='" + itemCode + @"' group by mi.edlitemcode,d.districtid,d.districtname,t.hodid 
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0  
              ) a
              group by districtid,districtname,edlitemcode
              order by districtname ";

            var myList = _context.districtWiseDhsDmeStockDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

       

        [HttpPost("insertTblRecvProgress")]
        public IActionResult insertTblRecvProgress(Int64 remid, string? remarks, Int64 ponoid, Int64 whid)
        {
            string dt1 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
            FacOperations f = new FacOperations( _context);

            string remarkStr = "";
            if(remarks == null || remarks.Length == 0 || remarks == "0" || remarks == "null")
            {
                remarkStr = "null";
            }
            else
            {
                remarkStr = "'"+ remarks + "'";
            }

            f.UpdateFlagforAlradyProgressed(ponoid, whid);
            string qry = @" insert into TBLRECVPROGRESS(REMID,REMARKS,PROGRESSDT,ENTRYDATE,PONOID,WAREHOUSEID,LATFLAG)
                        values ("+ remid + ","+ remarkStr + ",sysdate,'"+ dt1 + "',"+ ponoid + ","+ whid + ",'Y')  ";
            _context.Database.ExecuteSqlRaw(qry);
            return Ok("Successfully Saved");

        }

        [HttpPost("insertTblRecvProgress_WithVhicle")]
        public IActionResult insertTblRecvProgress_WithVhicle(Int64 remid, string? remarks, Int64 ponoid, Int64 whid,Int64? tranId,string? plateNo)
        {
            string dt1 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
            FacOperations f = new FacOperations(_context);

            string remarkStr = "";
            string strTranId = "";
            string strPlateNo = "";

            if (remarks == null || remarks.Length == 0 || remarks == "0" || remarks == "null")
            {
                remarkStr = "null";
            }
            else
            {
                remarkStr = "'" + remarks + "'";
            }

            if (tranId == null || tranId == 0)
            {
                strTranId = "null";
            }
            else
            {
                strTranId = "" + tranId + "";
            }

            if (plateNo == null || plateNo == "0")
            {
                strPlateNo = "null";
            }
            else
            {
                strPlateNo = "'" + plateNo + "'";
            }

            f.UpdateFlagforAlradyProgressed(ponoid, whid);
            string qry = @" insert into TBLRECVPROGRESS(REMID,REMARKS,PROGRESSDT,ENTRYDATE,PONOID,WAREHOUSEID,LATFLAG,TRANID,VPLATENO)
                        values (" + remid + "," + remarkStr + ",sysdate,'" + dt1 + "'," + ponoid + "," + whid + ",'Y',"+ strTranId + ","+ strPlateNo + ")  ";
            _context.Database.ExecuteSqlRaw(qry);
            return Ok("Successfully Saved");

        }



        [HttpGet("WHInTransitReason")]
        public async Task<ActionResult<IEnumerable<WHInTransitIssuesDTO>>> WHInTransitReason(string whid,string userid,string remid)
        {
            string whremid = "";
            if (remid == "0")
            {

            }
            else if (remid == "Null")
            {

            }
            else
            {
                whremid = " and prg.REMID=" + remid;
            }

            
            string whWarehouseid = ""; string toWHID = "";
            string innerwhid = "";
            if (whid != "0")
            {
                whWarehouseid = " and prg.WAREHOUSEID=" + whid;
                innerwhid = " and warehouseid = " + whid ;


            }
            string struserid = "  ";
            if (userid == "2926")
            {
                struserid = userid;
            }
            string qry = @" select  prg.PROGRESSID,r.REMID,r.REMARKS as Progress,prg.REMARKS,to_char(prg.ENTRYDATE,'dd-Mon-yyyy hh:mm:ss')ENTRYDATE,w.warehousename,op.pono,to_char(op.soissuedate,'dd-MM-yyyy') as podate
            ,s.suppliername,m.itemcode,m.itemname ,prg.WAREHOUSEID,prg.ponoid,ho.horemarks, to_char(ho.entrydt,'dd-Mon-yyyy hh:mm:ss') entrydt
            from TBLRECVPROGRESS prg
            inner join maswarehouses w on w.WAREHOUSEID=prg.WAREHOUSEID
            inner join soOrderPlaced OP on op.ponoid=prg.ponoid
            inner join SoOrderedItems OI on OI.PoNoID = prg.ponoid
            inner join masitems m on m.itemid = oi.itemid
            inner join massuppliers s on s.supplierid=op.supplierid
            inner join  MasRecRemarks r on r.REMID=prg.REMID
            left outer join 
            (select  warehouseid, ponoid from tbreceipts where status = 'C' and receipttype = 'NO' " + innerwhid + @") r on r.ponoid = OP.ponoid and r.warehouseid = w.warehouseid
            left join TBLRECVPROGRESSHOREMARKS ho on ho.progressid = prg.progressid and ho.isCurrent = 'Y'
            where  prg.LATFLAG='Y' and  r.ponoid is null  " + whWarehouseid + @"  " + whremid + @"
            order by prg.ENTRYDATE ";

            //            string qry = @" select  prg.PROGRESSID,r.REMID,r.REMARKS as Progress,prg.REMARKS,to_char(prg.ENTRYDATE,'dd-Mon-yyyy hh:mm:ss')ENTRYDATE,w.warehousename,op.pono,to_char(op.soissuedate,'dd-MM-yyyy') as podate
            //,s.suppliername,m.itemcode,m.itemname ,prg.WAREHOUSEID,prg.ponoid,ho.horemarks,ho.entrydt from TBLRECVPROGRESS prg
            //inner join maswarehouses w on w.WAREHOUSEID=prg.WAREHOUSEID
            //inner join soOrderPlaced OP on op.ponoid=prg.ponoid
            //inner join SoOrderedItems OI on OI.PoNoID = prg.ponoid
            //inner join masitems m on m.itemid = oi.itemid
            //inner join massuppliers s on s.supplierid=op.supplierid
            //inner join  MasRecRemarks r on r.REMID=prg.REMID
            //left join TBLRECVPROGRESSHOREMARKS ho on ho.progressid = prg.progressid and ho.isCurrent = 'Y'
            //where  prg.LATFLAG='Y' " + whWarehouseid + @"  " + whremid + @"
            //order by prg.ENTRYDATE ";

            var myList = _context.WHInTransitIssuesDbSet
.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }



        [HttpPost("insertTBLRECVPROGRESSHOREMARKS")]
        public IActionResult insertTBLRECVPROGRESSHOREMARKS(Int64 progressid, Int64 remid, string horemarks,string houserid)
        {
            string dt1 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
            //FacOperations f = new FacOperations(_context);

            //string remarkStr = "";
            //if (remarks == null || remarks.Length == 0 || remarks == "0" || remarks == "null")
            //{
            //    remarkStr = "null";
            //}
            //else
            //{
            //    remarkStr = "'" + remarks + "'";
            //}

            //f.UpdateFlagforAlradyProgressed(ponoid, whid);

            string qryUpdate = @" update TBLRECVPROGRESSHOREMARKS set isCurrent = 'N' where PROGRESSID = "+ progressid + " ";
            _context.Database.ExecuteSqlRaw(qryUpdate);

            string qry = @" insert into TBLRECVPROGRESSHOREMARKS( progressid, remid, horemarks, entrydt,isCurrent,houserid) values(" + progressid + ", "+ remid + ", '"+ horemarks + "', '"+ dt1  + "','Y',"+ houserid +")  ";
            _context.Database.ExecuteSqlRaw(qry);
            return Ok("Successfully Saved");

        }




    }
}
