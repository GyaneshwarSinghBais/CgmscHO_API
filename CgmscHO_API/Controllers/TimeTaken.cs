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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing;
using System.Net;
using CgmscHO_API.DTO;
using System.Diagnostics;
using System.Reflection;
using CgmscHO_API.MasterDTO;
using System.Diagnostics.Metrics;
using CgmscHO_API.WarehouseDTO;
using CgmscHO_API.PublicDTO;
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeTaken : ControllerBase
    {
        private readonly OraDbContext _context;
        public TimeTaken(OraDbContext context)
        {
            _context = context;
        }
        [HttpGet("NearExpRC")]
        public async Task<ActionResult<IEnumerable<RCNearExPSummary>>> NearExpRC(string mcid)
        {
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid = " + mcid;
            }
            string qry = "";
            qry = @" select mm, count(itemid) as nosRC,fdate,to_date(fdate,'dd-MM-yyyy') as fdatee
from 
(
select itemid,max(RCEndDT) as RCEndDT,extract(month from  max(RCEndDT)) || '-' || extract(year from  max(RCEndDT)) amonth,extract(month from  max(RCEndDT)) monthname,
to_char(max(RCEndDT),'MON')||'-'||to_char(max(RCEndDT), 'yyyy') mm,
'01'||'-'||to_char(max(RCEndDT),'MON')||'-'||to_char(max(RCEndDT), 'yyyy') fdate,
round( max(RCEndDT)-sysdate,0) as days

from (
select to_char(a.ContractStartDate,'dd-MM-yyyy') rcstart,a.ContractEndDate,
case when (ci.isextended='Y') then ci.rcextendedupto else ContractEndDate  end RCEndDT,
 case when ci.basicratenew is null then ci.basicrate else basicratenew end basicrate,
 ci.finalrategst
 ,case when   ci.percentvaluegst is null then   ci.percentvalue  else ci.percentvaluegst end Tax   
, ci.itemid,ms.schemeid,s.supplierid  
, case when sysdate between s.blacklistfdate and  s.blacklisttdate then 'Yes' else 'No' end as blacklisted,
case when (ci.isextended='Y') then round(rcextendedupto-sysdate,0) else  round(ContractEndDate-sysdate,0)  end as DayRemaining
from aoccontractitems ci
inner join aoccontracts a on a.contractid=ci.contractid
inner join massuppliers s on s.supplierid=a.supplierid
inner join masschemes ms on ms.schemeid=a.schemeid
inner join masitems m on m.itemid=ci.itemid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.mcid=c.mcid
where  ForceCloseRC is null  and a.status = 'C'  " + whmcid + @"
and  (sysdate between a.contractstartdate and a.contractenddate or 
sysdate between a.contractstartdate and  (case when ci.isextended='Y' then ci.rcextendedupto else a.contractenddate end))
) where blacklisted='No'
group  by itemid
) group by mm,fdate
order by to_date(fdate,'dd-MM-yyyy') ";



            var myList = _context.RCNearExPSummaryDbset
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("NearExpRCDetails")]
        public async Task<ActionResult<IEnumerable<RCNearExDetails>>> NearExpRCDetails(string mcid, string mmpara)
        {
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid = " + mcid;
            }
            string whmmpara = "";
            if (mmpara != "0")
            {
                whmmpara = " where mm='" + mmpara + "'";
            }
            string qry = "";
            qry = @" select itemcode,itemname,strength1,unit,basicrate,Tax as GST,finalrategst,RCStart,RCEndDT,itemid,mm
from 
(
select m.itemcode ,m.itemname,m.strength1,m.unit,m.itemid,min(d.rcstart) as RCStart,max(d.RCEndDT) as RCEndDT,
extract(month from  max(RCEndDT)) || '-' || extract(year from  max(RCEndDT)) amonth,extract(month from  max(RCEndDT)) monthname,
to_char(max(RCEndDT),'MON')||'-'||to_char(max(RCEndDT), 'yyyy') mm,
'01'||'-'||to_char(max(RCEndDT),'MON')||'-'||to_char(max(RCEndDT), 'yyyy') fdate,
round( max(RCEndDT)-sysdate,0) as days,min(basicrate) as basicrate,min(finalrategst) as finalrategst,min(Tax) as Tax
from (
select a.ContractStartDate rcstart,a.ContractEndDate, case when ci.basicratenew is null then ci.basicrate else ci.basicratenew end basicrate,
 ci.finalrategst,
case when (ci.isextended='Y') then ci.rcextendedupto else ContractEndDate  end RCEndDT
 ,case when   ci.percentvaluegst is null then   ci.percentvalue  else ci.percentvaluegst end Tax   
, ci.itemid,ms.schemeid,s.supplierid  
, case when sysdate between s.blacklistfdate and  s.blacklisttdate then 'Yes' else 'No' end as blacklisted,
case when (ci.isextended='Y') then round(rcextendedupto-sysdate,0) else  round(a.ContractEndDate-sysdate,0)  end as DayRemaining
from aoccontractitems ci
inner join aoccontracts a on a.contractid=ci.contractid
inner join massuppliers s on s.supplierid=a.supplierid
inner join masschemes ms on ms.schemeid=a.schemeid
inner join masitems m on m.itemid=ci.itemid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.mcid=c.mcid
where  ForceCloseRC is null  and a.status = 'C'  " + whmcid + @"
and  (sysdate between a.contractstartdate and a.contractenddate or 
sysdate between a.contractstartdate and  (case when ci.isextended='Y' then ci.rcextendedupto else a.contractenddate end))
) d
inner join masitems m on m.itemid=d.itemid
inner join masitemtypes ty on ty.itemtypeid=m.itemtypeid
where blacklisted='No' 
group  by m.itemcode ,m.itemname,m.strength1,m.unit,m.itemid
) "+whmmpara+@" order by RCEndDT desc ";



            var myList = _context.RCNearExDetailsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("SupplyDuration")]
        public async Task<ActionResult<IEnumerable<SupplyDuration>>> SupplyDuration()
        {

            string qry = "";
            qry = @" select  DURATION,count(so.ponoid) nos from sotranches t
inner join soorderplaced so  on so.ponoid=t.ponoid
where so.status in ('O')  and DURATION in (7,15,30,60,70,75,90)
group by DURATION
order by DURATION  ";



            var myList = _context.SupplyDurationDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("masupplier")]
        public async Task<ActionResult<IEnumerable<Massupplier>>> masupplier()
        {

            string qry = "";
            qry = @"  select supplierid, suppliername || to_char(nos),nos from
        (
select t.supplierid, t.suppliername, count(so.ponoid) nos from massuppliers t
inner join soorderplaced so on so.supplierid= t.supplierid
where so.status in ('O')
group by t.supplierid,t.suppliername
) order by nos desc ";



            var myList = _context.MassupplierDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("QCDaysItemType")]
        public async Task<ActionResult<IEnumerable<LabTestTimeTaken>>> QCDaysItemType()
        {

            string qry = "";
            qry = @"        select ITEMTYPEID, ITEMTYPECODE, ITEMTYPENAME, QCDAYSLAB  from masitemtypes
where QCDAYSLAB is not null
order by QCDAYSLAB desc ";



            var myList = _context.LabTestTimeTakenDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }








        [HttpGet("POSuppyTimeTakenYear")]
        public async Task<ActionResult<IEnumerable<SupplierTimeTaken>>> POSuppyTimeTakenYear(string mcid, string duration, string supplierid)
        {
            string whmcid = " and mc.mcid in (1,2,3,4) ";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid =" + mcid;
            }
            string whduration = " ";
            if (duration != "0")
            {
                whduration = " and DURATION=" + duration;
            }
            string whsupid = " ";
            if (supplierid != "0")
            {
                whsupid = " and so.supplierid=" + supplierid;
            }

            string qry = "";
            qry = @" select ACCYEAR as ID,ACCYRSETID,ACCYEAR,count(ponoid) as nospo,count(distinct itemid) nositems,round(sum(daysSupply)/count(ponoid),0)  TimetakenSupply
from 
(
select y.ACCYRSETID,y.ACCYEAR, so.ponoid,so.podate,si.itemid,mc.mcid,mc.mcategory,recedt,round((recedt-so.podate),0) as daysSupply,DURATION from  soorderplaced so 
inner join soordereditems si on si.ponoid=so.ponoid
inner join sotranches st on st.ponoid=so.ponoid
inner join masitems m on m.itemid = si.itemid 
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.mcid=c.mcid
inner join masaccyearsettings y on y.ACCYRSETID=so.ACCYRSETID
inner join 
(
select ponoid,max(receiptdate) as recedt from tbreceipts t 
where t.status  ='C' and t.receipttype='NO'
group by ponoid
) r on r.ponoid=so.ponoid
where so.status in ('O') " + whmcid + @"  " + whsupid + @"  " + whduration + @"
) group by ACCYRSETID,ACCYEAR
order by ACCYRSETID ";



            var myList = _context.SupplierTimeTakenDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("PaidTimeTaken")]
        public async Task<ActionResult<IEnumerable<TimeTakenPaid>>> PaidTimeTaken(string mcid, string HODID, string QCRequired)

        {
            string whmcid = " and mc.mcid in (1,2,3,4) ";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid =" + mcid;
            }
            string whhodid = "";
            if (HODID != "0")
            {
                whhodid = " and so.deptid=" + HODID;
            }


            string qry = "";

            if (QCRequired == "Y")
            {
                qry = @" select yrid, yr, count(ponoid) as nospo,round(sum(TOTNETAMOUNT)/10000000,2) as Gross,round(sum(daysincrec)/count(ponoid),0)  avgdayssincerec,round(sum(daysincQC)/count(ponoid),0)  avgdayssinceQC
from
(
select mc.mcategory, bp.aiddate, (select ACCYRSETID from masaccyearsettings where bp.aiddate between STARTDATE and ENDDATE) yrid,(select ACCYEAR from masaccyearsettings where bp.aiddate between STARTDATE and ENDDATE) yr,recedt,(bp.aiddate-r.recedt) as daysincrec,s.ponoid,(bp.aiddate- q.qcpasDT) as daysincQC,s.TOTNETAMOUNT from  blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid= s.ponoid  " + whhodid + @"
inner join soordereditems si on si.ponoid= so.ponoid
inner join masitems m on m.itemid = si.itemid and nvl(m.qctest,'N')='Y'
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.mcid= c.mcid
inner join
(
select ponoid, max(receiptdate) as recedt from tbreceipts t
where t.status= 'C' and t.receipttype= 'NO' and t.receiptdate>'01-Apr-2018'
group by ponoid
) r on r.ponoid=so.ponoid
inner join
(
select PONOID, max(REPORTRECEIVEDDATE) as qcpasDT from qcsamples s
inner join qctests t on t.sampleid= nvl(s.sampleid, s.refsampleid)
where nvl(s.NEWTESTRESULT, s.TESTRESULT)= 'SQ'
group by ponoid
) q on q.ponoid=so.ponoid

where bp.status='P' " + whmcid + @"
and bp.aiddate > '01-Apr-2021' 
) group by yr,yrid order by yrid ";
            }
            else
            {
                qry = @" select yrid,yr,count(ponoid) as nospo,round(sum(TOTNETAMOUNT)/10000000,2) as Gross,round(sum(daysincrec)/count(ponoid),0)  avgdayssincerec,round(sum(daysincrec)/count(ponoid),0)  as avgdayssinceQC
from 
(
select m.itemid,m.itemname,mc.mcategory,bp.aiddate,(select ACCYRSETID from masaccyearsettings where bp.aiddate between STARTDATE and ENDDATE) yrid,(select ACCYEAR from masaccyearsettings where bp.aiddate between STARTDATE and ENDDATE) yr,recedt,(bp.aiddate-r.recedt) as daysincrec,s.ponoid,s.TOTNETAMOUNT from  blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid=s.ponoid  " + whhodid + @"
inner join soordereditems si on si.ponoid=so.ponoid
inner join masitems m on m.itemid = si.itemid and nvl(m.qctest,'N')='N'
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.mcid=c.mcid
inner join 
(
select ponoid,max(receiptdate) as recedt from tbreceipts t 
where t.status='C' and t.receipttype='NO' and t.receiptdate>'01-Apr-2018'
group by ponoid
) r on r.ponoid=so.ponoid
where bp.status='P' and mc.mcid not in (1)  " + whmcid + @"
and bp.aiddate > '01-Apr-2021' 
) group by yr,yrid order by yrid";

            }



            var myList = _context.TimeTakenPaidDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }





        [HttpGet("QCTimeTakenYearwise")]
        public async Task<ActionResult<IEnumerable<TimeTakenYearQC>>> QCTimeTakenYearwise(string mcid)
        {
            string whmcid = " and mc.mcid in (1,2,3,4) ";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid =" + mcid;
            }


            string qry = "";
            qry = @" 
select ACCYRSETID,ACCYEAR,count(distinct itemid) as POnositems,sum(nossample) as totalsample,round(sum(daysincQC)/count(ponoid),0)  QCTimetaken
from 
(
select m.itemid,s.PONOID,recedt,max(REPORTRECEIVEDDATE) as qcpasDT,y.ACCYRSETID,y.ACCYEAR,round((max(REPORTRECEIVEDDATE)-recedt),0) as daysincQC
,count(s.sampleid) nossample
from qcsamples s 
inner join soorderplaced so on so.ponoid=s.ponoid
inner join soordereditems si on si.ponoid=so.ponoid
inner join masitems m on m.itemid = si.itemid and nvl(m.qctest,'N')='Y'
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.mcid=c.mcid
inner join 
(
select ponoid,min(receiptdate) as recedt from tbreceipts t 
where t.status='C' and t.receipttype='NO'
group by ponoid
) r on r.ponoid=so.ponoid
inner join masaccyearsettings y on y.ACCYRSETID=so.ACCYRSETID
inner join qctests t on t.sampleid=nvl(s.sampleid,s.refsampleid)
where nvl(s.NEWTESTRESULT,s.TESTRESULT)='SQ' " + whmcid + @"
group by s.ponoid,y.ACCYRSETID,y.ACCYEAR ,recedt,m.itemid
) group by ACCYRSETID,ACCYEAR
order by ACCYRSETID";



            var myList = _context.TimeTakenYearQCDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("QCLabYearAvgTime")]
        public async Task<ActionResult<IEnumerable<TimeTakenLabAll_Year>>> QCLabYearAvgTime(string yrid)
        {


            string whyearid = "";
            string whereyearid = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
                whereyearid = " and yrid =" + whyearid;
            }
            else if (yrid == "All")
            {
                whereyearid = "";
            }
            else
            {
                whereyearid = " and yrid =" + yrid;
            }


            string qry = "";
            qry = @" select LABID,labname,count(distinct SAMPLEID) as nossamples,round(sum(AnalysisTime)/count(distinct SAMPLEID),0) as AvgTimeTaken
from 
(
select l.labname||'-'||l.CITY as labname,qt.LABID,qt.SAMPLEID,qt.LABISSUEDATE,qt.SAMPLERECEIVEDDATE,qt.REPORTRECEIVEDDATE,round(qt.REPORTRECEIVEDDATE-qt.SAMPLERECEIVEDDATE,0) as AnalysisTime,

(select ACCYRSETID from masaccyearsettings where qt.REPORTRECEIVEDDATE between STARTDATE and ENDDATE) yrid
,(select ACCYEAR from masaccyearsettings where qt.REPORTRECEIVEDDATE between STARTDATE and ENDDATE) yr
from qctests qt
inner join qcsamples s on nvl(s.sampleid,s.refsampleid)=qt.sampleid
inner join qclabs l on l.labid=qt.LABID
where nvl(s.NEWTESTRESULT,s.TESTRESULT)='SQ' and qt.REPORTRECEIVEDDATE is not null and l.labid not in (398,405)
) where 1=1 " + whereyearid + @"
group by LABID,labname
order by labname";



            var myList = _context.TimeTakenLabAll_YearDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }




        [HttpGet("StockoutSummary")]
        public async Task<ActionResult<IEnumerable<EDLstockout>>> EDLStockout(string yrid, string isedl, string mcid)
        {


            string whyearid = "";
            string whereyearid = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
                whereyearid = " and accyrsetid =" + whyearid;
            }

            else
            {
                whereyearid = " and accyrsetid =" + yrid;
            }


            string qry = "";
            qry = @" select w.warehousename,p.WAREHOUSEID,count(itemid) as nosdrugs,sum(stockout) as stockout,case when sum(stockout)>0 then round((sum(stockout)/count(itemid))*100,2) else 0 end as stockoutP
from 
(
select wm.itemid,wm.WAREHOUSEID,ReadyForIssue,Pending, case when (nvl(ReadyForIssue,0)+nvl(Pending,0))=0 then 1 else 0 end as stockout  from 
(
select m.itemid,w.WAREHOUSEID from  masitems m
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.mcid=c.mcid
 inner join 
(
select itemid  from itemindent i  where 1 = 1 
and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0  and (case when i.isaireturn ='Y' then 1 else 0 end)=0
" + whereyearid + @"
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid
,maswarehouses w
where 1=1 and  m.ISFREEZ_ITPR is null and nvl(m.isedl2021,'N')='" + isedl + @"' and mc.mcid=" + mcid + @"
) wm
left outer join 
(
select warehouseid, itemid,(case when sum( A.ReadyForIssue)>0 then sum( A.ReadyForIssue) else 0 end) as ReadyForIssue,(case when sum(nvl(Pending,0)) >0 then sum(nvl(Pending,0)) else 0 end) Pending 
                  from 
                 (  
                 select t.warehouseid,mi.itemid,   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,  
                   case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end  Pending  
             
                  from tbreceiptbatches b  
                  inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid 
                  inner join tbreceipts t on t.receiptid=i.receiptid 
                  inner join masitems mi on mi.itemid=i.itemid 
inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.mcid=c.mcid
                 left outer join 
                  (  
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty   
                   from tboutwards tbo 
                   inner join tbindentitems tbi on tbi.indentitemid=tbo.indentitemid                   
                   inner join tbindents tb on tb.indentid=tbi.indentid
                   where   tb.status = 'C' 
                      and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null 
                   group by tbi.itemid,tb.warehouseid,tbo.inwno  
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid 
                 Where  T.Status = 'C' and (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) And (b.Whissueblock = 0 or b.Whissueblock is null)  
                 and mc.mcid=" + mcid + @"
                 and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null 
                 ) A group by warehouseid, itemid
) ws on ws.itemid=wm.itemid and ws.warehouseid=wm.warehouseid ) p 
inner join maswarehouses w on w.WAREHOUSEID=p.WAREHOUSEID
group by w.warehousename,p.WAREHOUSEID,w.ZONEID
order by w.ZONEID";



            var myList = _context.EDLstockoutDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("StockoutDetails")]
        public async Task<ActionResult<IEnumerable<EDLStockoutDetails>>> StockoutDetails(string yrid, string isedl, string mcid, string whid)
        {


            string whyearid = "";
            string whereyearid = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
                whereyearid = " and accyrsetid =" + whyearid;
            }

            else
            {
                whereyearid = " and accyrsetid =" + yrid;
            }


            string qry = "";
            qry = @" select m.itemid,m.itemcode,m.itemname,m.strength1,m.unit,mc.MCATEGORY,round(nvl(whai.WHAIQTY,0)/nvl(m.unitcount,1),0) as whaisku,
nvl(iss.IssuedQTY,0) as IssuedQTY,
nvl(ws.ReadyForIssue,0) as ReadyForIssue,nvl(ws.Pending,0) as UQC
,nvl(iwh.IWHPipeline,0) as IWHPipeline
,nvl(pip.TOTLPIPELINE,0) as Pipeline
,(nvl(ai.DHS_INDENTQTY, 0) + nvl(ai.mitanin, 0)) as DHS_INDENTQTY
from  
masitems m
inner join itemindent ai on  ai.itemid=m.itemid and ai.accyrsetid=" + yrid + @"  and (case when ai.isaireturn ='Y' then 1 else 0 end)=0 and  (nvl(ai.DHS_INDENTQTY, 0) + nvl(ai.mitanin, 0))>0
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.mcid=c.mcid

left outer join 
(

select tbo.ITEMID, sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))  IssuedQTY from tboutwards tbo
inner join tbindentitems tbi on tbi.indentitemid=tbo.indentitemid 
inner join tbindents tb on tb.indentid =tbi.indentid
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  1=1 and tb.status='C' and t.hodid  =2 and tb.WAREHOUSEID=" + whid + @"
and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + yrid + @"  ) 
and (select enddate from masaccyearsettings where accyrsetid=" + yrid + @" ) group by tbo.ITEMID
) iss on iss.itemid = m.itemid


left outer join 
(
select warehouseid, itemid,(case when sum( A.ReadyForIssue)>0 then sum( A.ReadyForIssue) else 0 end) as ReadyForIssue,(case when sum(nvl(Pending,0)) >0 then sum(nvl(Pending,0)) else 0 end) Pending 
                  from 
                 (  
                 select t.warehouseid,mi.itemid,   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,  
                   case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end  Pending  
             
                  from tbreceiptbatches b  
                  inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid 
                  inner join tbreceipts t on t.receiptid=i.receiptid 
                  inner join masitems mi on mi.itemid=i.itemid and   nvl(mi.isedl2021,'N')='" + isedl + @"'
                 left outer join 
                  (  
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty   
                   from tboutwards tbo 
                   inner join tbindentitems tbi on tbi.indentitemid=tbo.indentitemid                   
                   inner join tbindents tb on tb.indentid=tbi.indentid
                   where   tb.status = 'C' 
                   and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null 
                   group by tbi.itemid,tb.warehouseid,tbo.inwno  
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid 
                 Where  T.Status = 'C' and t.warehouseid=" + whid + @" and (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) And (b.Whissueblock = 0 or b.Whissueblock is null)  
                 
                 and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null 
                 ) A group by warehouseid, itemid
) ws on ws.itemid=m.itemid 

left outer join
(
  select itemid, sum(pipelineQTY) TOTLPIPELINE from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty, 0)receiptabsqty,
receiptdelayexception, round(sysdate - op.soissuedate, 0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end as pipelineQTY
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID = OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid = OI.orderitemid
inner join masitems m on m.itemid = oi.itemid and nvl(m.isedl2021,'N')='" + isedl + @"'
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid = tr.receiptid
where tr.receipttype = 'NO' and tr.status = 'C' and tr.notindpdmis is null and tri.notindpdmis is null
and tr.WAREHOUSEID =" + whid + @"
group by tr.ponoid, tri.itemid
) rec on rec.ponoid = OP.PoNoID and rec.itemid = OI.itemid 
 where op.status  in ('C', 'O') and op.deptid=367 and soi.WAREHOUSEID=" + whid + @"
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
select itemid,sum(transferqty) as IWHPipeline
from 
(
select transferno,t.transferdate,i.transferqty,
(select warehousename from maswarehouses where warehouseid = t.fromwarehouseid) fromwarehousename,
(select warehousename from maswarehouses where warehouseid = t.towarehouseid) towarehousename
,ti.indentdate as WHIssuedDT,nvl(WHIssueQTY,0) whissueQTY,
round(sysdate - ti.indentdate) pendingsince 
,m.itemid,t.fromwarehouseid ,t.towarehouseid,i.TRANSFERITEMID
from stktransfers t
inner join stktransferitems i on i.transferid = t.transferid
inner join masitems m on m.itemid = i.itemid and nvl(m.isedl2021,'N')='" + isedl + @"'
inner join 
(
select ti.indentdate,ti.warehouseid,ti.transferid,tbi.itemid,sum(nvl(tbo.issueqty,0)) WHIssueQTY   from tbindents ti
inner join tbindentitems tbi on tbi.indentid=ti.indentid
inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
where ti.status='C'
group by ti.indentdate,ti.warehouseid,ti.transferid,tbi.itemid
)ti on ti.transferid = t.transferid and m.itemid=ti.itemid
where t.status = 'C'  and  t.towarehouseid=" + whid + @" and 
t.transferid in (select transferid from tbindents where status = 'C' and transferid is not null)
and t.transferid not in (select transferid from tbreceipts where status = 'C' and transferid is not null)
and t.transferdate between '01-Apr-24' and sysdate 
) group by itemid
) iwh on iwh.itemid=m.itemid 

left outer join 
(
 select a.itemid, sum(nvl(CMHODISTQTY,0)) WHAIQTY
     from anualindent a
     inner join masfacilities f  on f.facilityid = a.facilityid  
     inner join masfacilitywh w on w.facilityid=f.facilityid      and w.warehouseid=" + whid + @"
      inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
     where a.status = 'C' and f.isactive = 1 and a.accyrsetid = " + yrid + @" and t.hodid = 2 
     and w.warehouseid=" + whid + @"
     group by a.itemid having sum(nvl(CMHODISTQTY, 0)) > 0
)whai on whai.itemid=m.itemid 

where 1=1 and  m.ISFREEZ_ITPR is null and nvl(m.isedl2021,'N')='" + isedl + @"' and mc.mcid=" + mcid + @" 
and (nvl(ReadyForIssue,0)+nvl(Pending,0))=0 
order  by m.itemname


   ";


            var myList = _context.EDLStockoutDetailsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("DropAppWarehousePerformance")]
        public async Task<ActionResult<IEnumerable<DropAppPerfomance>>> DropAppWarehousePerformance(string fromdt, string todate)
        {
            string qry = "";
            qry = @"select warehousename,nosvehicle,count(distinct districtid) nosdist,nosfac,count(distinct facilityid) as nooffacIndented,count(distinct NOCID) as nosindent,count(distinct indentid) as IndentIssued,
sum(dropindentid) as dropindentid
,count(distinct indentid)-sum(dropindentid) as Intrasit
,CASE WHEN sum(dropindentid)>0 THEN Round(sum(dropindentid)/(sum(dropindentid)+(count(distinct indentid)-sum(dropindentid)))*100,2) ELSE 0 END as DropPEr
,CASE WHEN sum(dropindentid)>0 THEN Round(sum(DaysTakenafterIssue)/sum(dropindentid),0) ELSE 0 END as AvgDAysTakenSinceIndentRec,
warehouseid
from 
(
select nosvehicle,nosfac,ind.NOCID,ind.facilityid,d.districtid,w.warehousename,w.warehouseid,d.districtname,nvl(nositems,0) IssueItems, case when nvl(nositems,0)>0 then 1 else 0 end as IndentCleared
,tbi.indentid,case when dropindentid is not null then 1 else 0 end as dropindentid,
case when DROPDATE is not null then round((to_date(to_char(DROPDATE,'dd-MM-yyyy'),'dd-MM-yyyy'))-ind.nocdate,0) else 0 end  as DaysTakenafterIssue
from mascgmscnoc ind 
inner join 
(
select distinct nocid as IndentNOCID from  mascgmscnocitems where nvl(BOOKEDQTY,0)>0
) indi on indi.IndentNOCID=ind.nocid
inner join masfacilities f on f.facilityid=ind.facilityid
inner join masdistricts d on d.districtid=f.districtid
INNER JOIN maswarehouses w on w.warehouseid=d.warehouseid
inner join
(
select count(VID) as nosvehicle,vh.warehouseid from  masvehical vh 
where  ISACTIVE='Y'
group by vh.warehouseid 

) vh on vh.warehouseid=w.warehouseid 

left outer join 
(
select count(f.facilityid) as nosfac,wh.warehouseid from masfacilities f
inner join masfacilitywh wh on wh.facilityid=f.facilityid
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
where ISACTIVE=1 and ft.hodid in (2,3,7,5) 
and f.facilitytypeid not in (383,387,375,380) 
group by wh.warehouseid 
) wf on wf.WAREHOUSEID=w.WAREHOUSEID

left outer join 
(
select tb.nocid,tb.indentid,nositems,di.dropindentid,DROPDATE from tbindents tb
left outer join
(
SELECT INDENTID AS dropindentid,MIN(ENTRYDATE) AS DROPDATE FROM TBINDENTTRAVALE 
WHERE ISDROP='Y' GROUP BY INDENTID
) di on di.dropindentid=tb.indentid

inner join 
(
select tbi.INDENTID,count(distinct ITEMID) as nositems from tbindentitems tbi
group by tbi.INDENTID
) tbi on tbi.INDENTID=tb.indentid
where tb.nocid is not null and tb.issuetype='NO' and tb.status='C'
) tbi on tbi.nocid=ind.nocid

where  ind.status='C' and ind.NOCDATE between '"+ fromdt + @"' and '"+ todate + @"'
) group by warehousename,warehouseid,nosvehicle,nosfac
order by warehouseid ";

            var myList = _context.DropAppPerfomanceDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("DelvieryDash")]
        public async Task<ActionResult<IEnumerable<DeliveryDash>>> DelvieryDash(int days)
        {

            string daybet = " sysdate and sysdate+1";
            if (days == 7)
            {

                daybet = " sysdate-7 and sysdate+1";
            }
            if (days == 1)
            {

                daybet = " sysdate-1 and sysdate";
            }

            string qry = "";
            qry = @" select tbi.TRAVALEID,case when (tbi.DROPLONGITUDE ='-00.0000' or tbi.DROPLATITUDE ='00.0000') then   f.LONGITUDE else tbi.DROPLONGITUDE  end as LONGITUDE,
case when (tbi.DROPLATITUDE ='-00.0000' or tbi.DROPLATITUDE ='00.0000') then f.LATITUDE  else tbi.DROPLATITUDE  end as LATITUDE  ,TO_CHAR(TO_TIMESTAMP(tbi.DROPDATE, 'DD-MM-RR HH:MI:SS.FF AM'), 'DD-MM-YYYY HH:MI:SS AM') as DROPDATE,
vh.VEHICALNO,f.facilityname 
,tbb.indentid,tbis.nositems,to_char(ind.NOCDATE,'dd-MM-yyyy') as indDT 
from TBTRAVALEMASTER tb 

inner join TBINDENTTRAVALE tbi on tbi.voucherid=tb.voucherid
inner join tbindents tbb on tbb.indentid=tbi.indentid
inner join 
(
select tbi.INDENTID,count(distinct ITEMID) as nositems from tbindentitems tbi
group by tbi.INDENTID
) tbis on tbis.INDENTID=tbb.indentid
inner join mascgmscnoc ind on ind.nocid=tbb.nocid
inner join masfacilities f on f.facilityid=tbb.facilityid
inner join masvehical vh on vh.VID=tb.VID
where  tbi.DROPDATE is not null
and DROPDATE between " + daybet+@"
order by tbi.DROPDATE desc ";

            var myList = _context.DeliveryDashDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("QCTimeTakenYear")]
        public async Task<ActionResult<IEnumerable<TimeTakenYearQC>>> QCTimeTakenYear(
      string mcid,
      string yearid,
      string itemtypeid)
        {
            string whmcid = " and mc.mcid in (1,2,3,4) ";
            if (mcid != "0")
                whmcid = " and mc.mcid =" + mcid;
            string whitemtype = "";
            if (itemtypeid != "0")
                whitemtype = " and ty.itemtypeid =" + itemtypeid;
            string whyearid = "";
            string whereyearid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(this._context);
                whyearid = f.getACCYRSETID();
                whereyearid = " and y.ACCYRSETID =" + whyearid;
                f = (FacOperations)null;
            }
            string qry = "";
            qry = " \r\nselect ACCYRSETID,ACCYEAR,count(distinct itemid) as POnositems,sum(nossample) as totalsample,round(sum(daysincQC)/count(ponoid),0)  QCTimetaken\r\nfrom \r\n(\r\nselect m.itemid,s.PONOID,recedt,max(REPORTRECEIVEDDATE) as qcpasDT,y.ACCYRSETID,y.ACCYEAR,round((max(REPORTRECEIVEDDATE)-recedt),0) as daysincQC\r\n,count(s.sampleid) nossample\r\nfrom qcsamples s \r\ninner join soorderplaced so on so.ponoid=s.ponoid\r\ninner join soordereditems si on si.ponoid=so.ponoid\r\ninner join masitems m on m.itemid = si.itemid and nvl(m.qctest,'N')='Y'\r\ninner join masitemtypes ty on ty.itemtypeid=m.itemtypeid\r\ninner join masitemcategories c on c.categoryid = m.categoryid\r\ninner join masitemmaincategory mc on mc.mcid=c.mcid\r\ninner join \r\n(\r\nselect ponoid,min(receiptdate) as recedt from tbreceipts t \r\nwhere t.status='C' and t.receipttype='NO'\r\ngroup by ponoid\r\n) r on r.ponoid=so.ponoid\r\ninner join masaccyearsettings y on y.ACCYRSETID=so.ACCYRSETID\r\ninner join qctests t on t.sampleid=nvl(s.sampleid,s.refsampleid)\r\nwhere nvl(s.NEWTESTRESULT,s.TESTRESULT)='SQ' " + whmcid + "   " + whereyearid + " " + whitemtype + "\r\ngroup by s.ponoid,y.ACCYRSETID,y.ACCYEAR ,recedt,m.itemid\r\n) group by ACCYRSETID,ACCYEAR\r\norder by ACCYRSETID";
            List<TimeTakenYearQC> myList = this._context.TimeTakenYearQCDbSet.FromSqlInterpolated<TimeTakenYearQC>(FormattableStringFactory.Create(qry)).ToList<TimeTakenYearQC>();
            ActionResult<IEnumerable<TimeTakenYearQC>> actionResult = (ActionResult<IEnumerable<TimeTakenYearQC>>)myList;
            whmcid = (string)null;
            whitemtype = (string)null;
            whyearid = (string)null;
            whereyearid = (string)null;
            qry = (string)null;
            myList = (List<TimeTakenYearQC>)null;
            return actionResult;
        }
    }
}
