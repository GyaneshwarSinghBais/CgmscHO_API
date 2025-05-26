using CgmscHO_API.Models;
using CgmscHO_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReagentController : ControllerBase
    {
        private readonly OraDbContext _context;

        public ReagentController(OraDbContext context)
        {
            _context = context;
        }


        [HttpGet("getEquipmentName")]
        public async Task<ActionResult<IEnumerable<EquipmentDTO>>> getEquipmentName()
        {
            string qry = @"  select EQPNAME,MAKE,MODEL,MMID from masreagenteqp eqp
                            inner join masreagentmakemodel mm on mm.PMACHINEID=eqp.PMACHINEID ";
            var myList = _context.EquipmentDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        //SukhdevCode

        [HttpGet("getStockIssuanceStateReagent")]
        public async Task<ActionResult<IEnumerable<ReagentStateStockIssueDTO>>> getStockIssuanceStateReagent(Int64 mmid)
        {
            FacOperations f = new FacOperations(_context);

            string yearid = f.getACCYRSETID();
            string whereMMID = "";
            string whm_mmid = "";
            string whmi_mmid = "";
            if (mmid != 0)
            {
                whereMMID = " and mm.mmid=" + mmid + @" ";
                whm_mmid = " and m.mmid=" + mmid + @" ";
                whmi_mmid = " and mi.mmid=" + mmid + @" ";
            }

            string qry = @"   select m.MMID,m.eqpname,m.nosreagent,nvl(st.nos,0) as stkavailable,nvl(st.stockvalue,0) as stockvaluecr,
nvl(st.noswh,0) noswh,nvl(mmiss.nosissued,0) nosissued,
nvl(mmiss.issuedvaluecr,0) nosissuedvaluecr,nvl(nofpipeline,0) nosfacpipeline,nvl(facpipelinevalue,0) nosfacpipelinevalue,
nvl(flab.nosfaclabissued,0) nosfaclabissued,nvl(flab.faclabissuevaluecr,0) faclabissuevaluecr,
nvl(nosfacstock,0) nosfacstock,nvl(mmfacstk.facstockvaluecr,0) facstockvaluecr
from 
(
select mm.MMID,eqpname,
count(distinct m.itemid) as nosreagent 
from masreagenteqp eqp
inner join masreagentmakemodel mm on mm.PMACHINEID=eqp.PMACHINEID
inner join masitems m on m.mmid=mm.mmid
where m.isfreez_itpr is null " + whereMMID + @"
group by  mm.MMID,eqpname
) m
left outer join 
(
select count(distinct itemcode) as nos ,Round(sum(StockValue)/10000000,2) as stockvalue,mmid,count(distinct warehouseid) as noswh
from 
(
select itemcode,itemname,eqpname,sum(nvl(ReadyForIssue,0)) as CurrentStock,
sum(nvl(ReadyValue,0)) as StockValue,mmid, warehouseid  from 
(
           select w.districtid,w.warehouseid,w.WAREHOUSENAME, mi.itemcode,mi.itemname,eqp.eqpname,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,                         
mi.categoryid,ci.finalrategst,(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end )*ci.finalrategst ReadyValue
                ,mm.mmid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 left outer join masreagentmakemodel mm on mm.mmid = mi.mmid
                 left outer join masreagenteqp eqp on eqp.pmachineid = mm.pmachineid
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid  
                 inner join soordereditems si on si.ponoid=b.ponoid
                 inner join aoccontractitems ci on ci.contractitemid=si.contractitemid
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
                 and mi.categoryid=62 " + whereMMID + @"
                 ) group by itemcode,itemname,eqpname,mmid ,warehouseid
                 having sum(nvl(ReadyForIssue,0))>0 
) group by mmid
) st on st.mmid=m.mmid

left outer join
(
select mmid,count (distinct itemid) as nosissued,round(sum(issuevalue)/10000000,2) as issuedvaluecr from
(
            select m.mmid,m.itemid,sum(tbo.issueqty) issueqty,sum(tbo.issueqty)*c.finalrategst issuevalue from tboutwards tbo 
             inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
             inner join tbindents t on t.indentid = tbi.indentid 
             inner join masitems m on m.itemid = tbi.itemid
             inner join tbreceiptbatches b on b.inwno = tbo.inwno
             inner join soordereditems oi on oi.ponoid = b.ponoid
             inner join aoccontractitems c on c.contractitemid = oi.contractitemid
             where 1=1 " + whm_mmid + @"  and t.issuetype = 'NO' and t.status = 'C' and m.categoryid = 62  
             and t.indentdate between  (select startdate  from masaccyearsettings where accyrsetid = " + yearid + @") 
             and  (select enddate  from masaccyearsettings where accyrsetid = " + yearid + @") 
             group by m.mmid,m.itemid,c.finalrategst
             having sum(tbo.issueqty)>0 
             ) group by mmid
) mmiss on mmiss.mmid = m.mmid
left outer join
(
select mmid,count (distinct itemid) as nosfacstock,round(sum(stockvalue)/10000000,2) facstockvaluecr
from
(
select mi.mmid,mi.itemcode,mi.itemid
               ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   inhand_qty,round((sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0)))*(c.finalrategst/nvl(mi.unitcount,1)),2) stockvalue 
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join soordereditems oi on oi.ponoid = b.ponoid
                 inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                 inner join masitems mi on mi.itemid=i.itemid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C' and fs.issuetype = 'NO' and fs.issueddate >   '31-Mar-2023'                
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid                  
                 Where 1=1 and mi.categoryid = 62 " + whmi_mmid + @"
                 and T.Status = 'C'  and t.facreceipttype = 'NO' and t.facreceiptdate > '31-Mar-2023' and b.expdate>sysdate 
               group by mi.itemcode,mi.itemid,mi.mmid,c.finalrategst,mi.unitcount
               having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0  
                 ) group by mmid
) mmfacstk on mmfacstk.mmid = m.mmid
left outer join
(
 select mmid,count (distinct itemid) as nofpipeline,round(sum(facpipelinevalue)/10000000,2) facpipelinevalue
from
(
 select m.itemcode,m.itemid,m.mmid,sum(tbo.issueqty) facpipelineqty,(sum(tbo.issueqty))*c.finalrategst facpipelinevalue   
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join soordereditems oi on oi.ponoid = rb.ponoid
         inner join aoccontractitems c on c.contractitemid = oi.contractitemid
         inner join masitems m on m.itemid = tbi.itemid
         left outer join 
         (
         select distinct r.INDENTID,r.facreceiptno,r.facreceiptdate,ri.itemid from tbfacilityreceipts r 
            inner join tbfacilityreceiptitems ri on ri.facreceiptid=r.facreceiptid
            inner join tbfacilityreceiptbatches rb on rb.facreceiptitemid=ri.facreceiptitemid
            inner join masitems m on m.itemid=ri.itemid
            where r.status='C' and m.categoryid=62
         )r on r.INDENTID=tb.INDENTID and r.itemid=m.itemid

         where 1=1 " + whm_mmid + @"  and tb.status = 'C' and tb.issuetype='NO' 
         and tb.indentdate between '01-APR-23' and sysdate
         and m.categoryid in (62) and r.facreceiptno is  null      
        group by m.itemcode,m.itemid,m.mmid,c.finalrategst
          ) group by mmid
) mmfacpip on mmfacpip.mmid = m.mmid
left outer join
(
select mmid,count (distinct itemid) as nosfaclabissued,round(sum(faclabissuevalue)/10000000,2) faclabissuevaluecr
from
(
select  m.itemid,m.itemcode,m.mmid,sum(nvl(ftbo.issueqty,0)) faclabissueqty, round(sum(nvl(ftbo.issueqty,0))*(c.finalrategst/nvl(m.unitcount,1)),2) faclabissuevalue  
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid
                   inner join tbfacilityreceiptbatches b on b.inwno = ftbo.inwno
                   inner join tbfacilityreceiptitems ri on ri.facreceiptitemid=b.facreceiptitemid
                   inner join tbfacilityreceipts ft on ft.facreceiptid=ri.facreceiptid
                   inner join soordereditems oi on oi.ponoid = b.ponoid
                   inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                   inner join masitems m on m.itemid = fsi.itemid
                   where fs.status = 'C' " + whm_mmid + @"
                   and ft.facreceipttype = 'NO'
                   and fs.issuetype = 'NO' 
                   and ft.facreceiptdate >'31-Mar-2023' 
                   and  fs.issueddate >   '31-Mar-2023'
                   and m.categoryid = 62
                   group by m.itemid,m.itemcode,m.mmid,c.finalrategst,m.unitcount
   ) group by mmid 
) flab on flab.mmid = m.mmid  ";
            var myList = _context.ReagentStateStockIssueDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("getWarehouseWiseReagent")]
        public async Task<ActionResult<IEnumerable<WarehouseWiseReagentDTO>>> getWarehouseWiseReagent(Int64 mmid)
        {
            string whereMMID = "";
            if (mmid != 0)
            {
                whereMMID = " and mm.mmid=" + mmid + @" ";
            }

            string qry = @" select ROW_NUMBER() OVER (ORDER BY WAREHOUSENAME) AS serial_no, WAREHOUSENAME,eqpname,count(distinct itemcode) as nos ,Round(sum(StockValue)/1000000,2) as stockvalue,warehouseid,mmid
from 
(
select WAREHOUSENAME,itemcode,itemname,eqpname,sum(nvl(ReadyForIssue,0)) as CurrentStock,sum(nvl(ReadyValue,0)) as StockValue,mmid,warehouseid from (

           select w.districtid,w.warehouseid,w.WAREHOUSENAME, mi.itemcode,mi.itemname,eqp.eqpname,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,    
                    nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid,ci.finalrategst,

   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end )*ci.finalrategst ReadyValue
                ,mm.mmid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 left outer join masreagentmakemodel mm on mm.mmid = mi.mmid
                 left outer join masreagenteqp eqp on eqp.pmachineid = mm.pmachineid
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid  
                 inner join soordereditems si on si.ponoid=b.ponoid
                 inner join aoccontractitems ci on ci.contractitemid=si.contractitemid
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
                 and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  and mi.categoryid=62 " + whereMMID + @"
                 ) group by itemcode,itemname,eqpname,warehouseid,WAREHOUSENAME,mmid having sum(nvl(ReadyForIssue,0))>0
) group by eqpname,mmid,warehouseid,WAREHOUSENAME
order by WAREHOUSENAME,eqpname
  ";
            var myList = _context.WarehouseWiseReagentDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



        [HttpGet("getIssuedReagentYearly")]
        public async Task<ActionResult<IEnumerable<IssuedReagentYearlyDTO>>> getIssuedReagentYearly()
        {
            string qry = @" select ROW_NUMBER() OVER (ORDER BY eqpname) AS serial_no, eqpname,count(distinct itemcode) cntcount, round(sum(issuevalue)/10000000,2) as issuevalue,mmid from (


select eqpname,warehousename,itemcode,itemname,facilityname,tbi.needed indentqty,sum(iss_qty) issuedqty,finalrate,sum(finalvalue) issuevalue ,mmid
from (

select w.warehouseid,f.facilityid, w.warehousename,tbi.itemid,tbi.indentitemid,m.itemcode,m.itemname || ' -' || m.strength1 as itemname,rb.batchno ISS_Batchno,f.facilityname
         ,sum(tbo.issueqty) ISS_Qty ,sb.ACIbasicrateNew skurate,ACICST,ACGSTPvalue,ACIVAT,case when rb.ponoid=1111 then 0 else  sb.ACIsingleunitprice end  finalrate,
         (sum(tbo.issueqty) * sb.ACIbasicrateNew) as SKUValue , (sum(tbo.issueqty) * sb.ACIsingleunitprice) as FinalValue
         ,eqp.eqpname,mm.mmid
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join masitems m on m.itemid = tbi.itemid
           left outer join masreagentmakemodel mm on mm.mmid = m.mmid
        left outer join masreagenteqp eqp on eqp.pmachineid = mm.pmachineid
         inner join maswarehouses w on w.warehouseid = tb.warehouseid
         inner join masfacilities f on f.facilityid = tb.facilityid

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
               ,r.ReceiptDate,aci.gstflag,aci.singleunitprice,aci.singleunitprice,aci.finalrategst
      ) sb on sb.inwno=rb.inwno and sb.indentitemid=tbo.indentitemid
        where tb.status = 'C' and tb.issuetype='NO'
        and tb.indentdate between '01-APR-23' and '31-MAR-24' 
        and m.categoryid in (62)
        and tb.notindpdmis is null 
        and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        group by w.warehousename,tbi.itemid,tbi.indentitemid,m.itemcode,m.itemname, m.strength1,rb.batchno,f.facilityname,w.warehouseid,
        f.facilityid  ,sb.acisingleunitprice,sb.acibasicratenew,tb.indentdate,rb.ponoid 
        ,eqp.eqpname,mm.mmid
        ,acicst,acgstpvalue,acivat ) i 
        inner join tbindentitems tbi on tbi.itemid = i.itemid 
        and tbi.indentitemid = i.indentitemid 
        group by warehousename,itemcode,itemname,facilityname,tbi.needed,finalrate,eqpname,mmid

        ) group by eqpname,mmid order by eqpname
 ";
            var myList = _context.IssuedReagentYearlyDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("getCGMSCstockValue")]
        public async Task<ActionResult<IEnumerable<CGMSCReagentStockValueDTO>>> getCGMSCstockValue()
        {
            string qry = @" select ROW_NUMBER() OVER (ORDER BY eqpname) AS serial_no,a.eqpname,mr.nosreagent as Required,
count(distinct a.itemcode) as nos ,Round(sum(a.StockValue)/10000000,2) as stockvalue,a.mmid,count(distinct warehouseid) as noswh
from 
(
select itemcode,itemname,eqpname,sum(nvl(ReadyForIssue,0)) as CurrentStock,
sum(nvl(ReadyValue,0)) as StockValue,mmid, warehouseid  from (

           select w.districtid,w.warehouseid,w.WAREHOUSENAME, mi.itemcode,mi.itemname,eqp.eqpname,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,                         
mi.categoryid,ci.finalrategst,(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end )*ci.finalrategst ReadyValue
                ,mm.mmid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 left outer join masreagentmakemodel mm on mm.mmid = mi.mmid
                 left outer join masreagenteqp eqp on eqp.pmachineid = mm.pmachineid
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid  
                 inner join soordereditems si on si.ponoid=b.ponoid
                 inner join aoccontractitems ci on ci.contractitemid=si.contractitemid
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
                 and mi.categoryid=62 --and mm.mmid=5
                 ) group by itemcode,itemname,eqpname,mmid ,warehouseid
                 having sum(nvl(ReadyForIssue,0))>0 
)a
left outer join 
(
select mm.MMID,count(distinct m.itemid) as nosreagent  from masreagenteqp eqp
inner join masreagentmakemodel mm on mm.PMACHINEID=eqp.PMACHINEID
inner join masitems m on m.mmid=mm.mmid
group by  mm.MMID
) mr on mr.MMID=a.mmid
group by a.eqpname,a.mmid,mr.nosreagent
order by a.eqpname ";
            var myList = _context.CGMSCReagentStockValueDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("reagentStockAndSupplySummary")]
        public async Task<ActionResult<IEnumerable<ReagentStockAndSupplySummaryDTO>>> reagentStockAndSupplySummary()
        {
            string qry = @"   select  stkp, count(distinct facilityid) as nos from (
select 0 as SlNo,m.facilityname,m.eqpname,m.nosreagent,nvl(st.nos,0) as stkavailable,
nvl(st.stockvalue,0) as stockvaluecr,nvl(mmiss.nosissued,0) nosissued,
nvl(mmiss.issuedvaluecr,0) nosissuedvaluecr,nvl(nofpipeline,0) nosfacpipeline,nvl(facpipelinevalue,0) nosfacpipelinevalue,
nvl(flab.nosfaclabissued,0) nosfaclabissued,nvl(flab.faclabissuevaluecr,0) faclabissuevaluecr,
nvl(nosfacstock,0) nosfacstock,nvl(mmfacstk.facstockvaluecr,0) facstockvaluecr
,case when nvl(nofpipeline,0)>0 and nvl(nosfacstock,0)=0 then 'Receipt Pending and Stock out'  
else case when nvl(nofpipeline,0)>0 and  nvl(nosfacstock,0)>0 then 'Receipt Pending and Stock Available'
else case when nvl(nofpipeline,0)=0 and  nvl(nosfacstock,0)=0 and nvl(flab.nosfaclabissued,0)>0 then 'Issued to Lab and Stock out'
else case when nvl(nofpipeline,0)=0 and  nvl(nosfacstock,0)=0 and nvl(flab.nosfaclabissued,0)=0 and nvl(mmiss.nosissued,0)=0 then 'Required to Check Equipment as No Distribution received and Stock out'
else case when nvl(nofpipeline,0)=0 and  nvl(nosfacstock,0)=0 and nvl(flab.nosfaclabissued,0)=0 and nvl(mmiss.nosissued,0)=0 and nvl(st.nos,0)>0 then 'Required to Check Equipment or Distribution as stock Available in Warehouse and Stock out in Facility'
else case when nvl(flab.nosfaclabissued,0)=0  and  nvl(nosfacstock,0)>0 then 'Lab Issuance not done'
else 
'Not Required' end end end end end end as stkp
,m.facilityid,m.ORDERDP,EqAvailable,m.districtname,m.districtid,m.MMID,hf.FOOTER3,hf.FOOTER2,nvl(hf.FOOTER2,'-')||'('||hf.FOOTER3||')' as contact

from 
(
select d.districtid,d.districtname,mm.MMID,eqpname,
count(distinct m.itemid) as nosreagent,f.facilityid,f.facilityname ,ft.ORDERDP,case when ef.facilityid =f.facilityid and ef.mmid=mm.mmid then 'Equipment Available' else 'Equipment Not Available' end as EqAvailable
from masdistricts d,masfacilities f,masfacilityequipment ef,masfacilitytypes ft ,masreagenteqp eqp
inner join masreagentmakemodel mm on mm.PMACHINEID=eqp.PMACHINEID
inner join masitems m on m.mmid=mm.mmid
where m.isfreez_itpr is null and d.stateid=1 and ef.ISACTIVE=1   --and d.districtid=2207   and f.facilityid=23253   
and mm.mmid=3  and f.districtid=d.districtid
and ef.mmid=mm.mmid and ef.facilityid=f.facilityid and ft.facilitytypeid=f.facilitytypeid
group by  mm.MMID,eqpname,d.districtid,d.districtname,f.facilityid,f.facilityname ,ft.ORDERDP,ef.facilityid,ef.mmid,mm.mmid
) m
left outer join usrusers u on u.facilityid=m.facilityid
left outer join masfacheaderfooter hf on hf.userid=u.userid
left outer join 
(
select districtid,count(distinct itemcode) as nos ,Round(sum(StockValue)/100000,2) as stockvalue,mmid
from 
(
select districtid,itemcode,itemname,eqpname,sum(nvl(ReadyForIssue,0)) as CurrentStock,
sum(nvl(ReadyValue,0)) as StockValue,mmid  from 
(
           select w.districtid,w.warehouseid,w.WAREHOUSENAME, mi.itemcode,mi.itemname,eqp.eqpname,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,                         
mi.categoryid,ci.finalrategst,(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end )*ci.finalrategst ReadyValue
                ,mm.mmid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 left outer join masreagentmakemodel mm on mm.mmid = mi.mmid
                 left outer join masreagenteqp eqp on eqp.pmachineid = mm.pmachineid
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid  
                 inner join soordereditems si on si.ponoid=b.ponoid
                 inner join aoccontractitems ci on ci.contractitemid=si.contractitemid
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
                 and mi.categoryid=62
                 ) group by itemcode,itemname,eqpname,mmid ,districtid
                 having sum(nvl(ReadyForIssue,0))>0 
) group by mmid,districtid
) st on st.mmid=m.mmid and st.districtid = m.districtid

left outer join
(
select facilityid,districtid, mmid,count (distinct itemid) as nosissued,round(sum(issuevalue)/100000,2) as issuedvaluecr from
(
            select t.facilityid, f.districtid,m.mmid,m.itemid,sum(tbo.issueqty) issueqty,sum(tbo.issueqty)*c.finalrategst issuevalue from tboutwards tbo 
             inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
             inner join tbindents t on t.indentid = tbi.indentid 
               inner join masfacilities f on f.facilityid = t.facilityid
                inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid 
             inner join masitems m on m.itemid = tbi.itemid
                    left outer join masreagentmakemodel mm on mm.mmid = m.mmid
                 left outer join masreagenteqp eqp on eqp.pmachineid = mm.pmachineid
             inner join tbreceiptbatches b on b.inwno = tbo.inwno
             inner join soordereditems oi on oi.ponoid = b.ponoid
             inner join aoccontractitems c on c.contractitemid = oi.contractitemid
             where  t.issuetype = 'NO' and t.status = 'C' and m.categoryid = 62  
             and t.indentdate between  (select startdate  from masaccyearsettings where accyrsetid = 544) 
             and  (select enddate  from masaccyearsettings where accyrsetid = 544)
             group by m.mmid,m.itemid,c.finalrategst,f.districtid,t.facilityid
             having sum(tbo.issueqty)>0 
             ) group by mmid,districtid,facilityid
) mmiss on mmiss.mmid = m.mmid and mmiss.districtid = m.districtid and mmiss.facilityid=m.facilityid 
left outer join
(
select facilityid,districtid, mmid,count (distinct itemid) as nosfacstock,round(sum(stockvalue)/100000,2) facstockvaluecr
from
(
select f.facilityid,f.districtid,mi.mmid,mi.itemcode,mi.itemid
               ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   inhand_qty,round((sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0)))*(c.finalrategst/nvl(mi.unitcount,1)),2) stockvalue 
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid
                inner join masfacilities f on f.facilityid = t.facilityid 
                 inner join soordereditems oi on oi.ponoid = b.ponoid
                 inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                 inner join masitems mi on mi.itemid=i.itemid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C' and fs.issuetype = 'NO' and fs.issueddate >   '31-Mar-2023'                
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid                  
                 Where 1=1 and mi.categoryid = 62
                 and T.Status = 'C'  and t.facreceipttype = 'NO' and t.facreceiptdate > '31-Mar-2023' and b.expdate>sysdate 
               group by mi.itemcode,mi.itemid,mi.mmid,c.finalrategst,mi.unitcount,f.districtid,f.facilityid
               having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0  
                 ) group by mmid,districtid,facilityid
) mmfacstk on mmfacstk.mmid = m.mmid and mmfacstk.districtid = m.districtid and mmfacstk.facilityid=m.facilityid 
left outer join
(
 select facilityid,districtid,mmid,count (distinct itemid) as nofpipeline,round(sum(facpipelinevalue)/100000,2) facpipelinevalue
from
(
 select  f.facilityid,f.districtid,m.itemcode,m.itemid,m.mmid,sum(tbo.issueqty) facpipelineqty,(sum(tbo.issueqty))*c.finalrategst facpipelinevalue   
         from tbindents tb
         inner join masfacilities f on f.facilityid = tb.facilityid 
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join soordereditems oi on oi.ponoid = rb.ponoid
         inner join aoccontractitems c on c.contractitemid = oi.contractitemid
         inner join masitems m on m.itemid = tbi.itemid
         left outer join 
         (
         select distinct r.INDENTID,r.facreceiptno,r.facreceiptdate,ri.itemid from tbfacilityreceipts r 
            inner join tbfacilityreceiptitems ri on ri.facreceiptid=r.facreceiptid
            inner join tbfacilityreceiptbatches rb on rb.facreceiptitemid=ri.facreceiptitemid
            inner join masitems m on m.itemid=ri.itemid
            where r.status='C' and m.categoryid=62
         )r on r.INDENTID=tb.INDENTID and r.itemid=m.itemid

         where tb.status = 'C' and tb.issuetype='NO' 
         and tb.indentdate between '01-APR-23' and '31-MAR-24' 
         and m.categoryid in (62) and r.facreceiptno is  null      
        group by m.itemcode,m.itemid,m.mmid,c.finalrategst,f.districtid, f.facilityid
          ) group by mmid,districtid,facilityid
) mmfacpip on mmfacpip.mmid = m.mmid and mmfacpip.districtid = m.districtid and mmfacpip.facilityid=m.facilityid 
left outer join
(
select facilityid,districtid,mmid,count (distinct itemid) as nosfaclabissued,round(sum(faclabissuevalue)/100000,2) faclabissuevaluecr
from
(
select  f.facilityid,f.districtid,m.itemid,m.itemcode,m.mmid,sum(nvl(ftbo.issueqty,0)) faclabissueqty, round(sum(nvl(ftbo.issueqty,0))*(c.finalrategst/nvl(m.unitcount,1)),2) faclabissuevalue  
                     from tbfacilityissues fs 
                       inner join masfacilities f on f.facilityid = fs.facilityid 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid
                   inner join tbfacilityreceiptbatches b on b.inwno = ftbo.inwno
                   inner join tbfacilityreceiptitems ri on ri.facreceiptitemid=b.facreceiptitemid
                   inner join tbfacilityreceipts ft on ft.facreceiptid=ri.facreceiptid
                   inner join soordereditems oi on oi.ponoid = b.ponoid
                   inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                   inner join masitems m on m.itemid = fsi.itemid
                   where fs.status = 'C' 
                   and ft.facreceipttype = 'NO'
                   and fs.issuetype = 'NO' 
                   and ft.facreceiptdate >'31-Mar-2023' 
                   and  fs.issueddate >   '31-Mar-2023'
                   and m.categoryid = 62
                   group by m.itemid,m.itemcode,m.mmid,c.finalrategst,m.unitcount,f.districtid,f.facilityid
   ) group by mmid ,districtid,facilityid
) flab on flab.mmid = m.mmid and flab.districtid = m.districtid and flab.facilityid=m.facilityid 

where 1=1  --and m.mmid=3
) group by stkp ";
            var myList = _context.ReagentStockAndSupplySummaryDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("reagentStockAndSupply")]
        public async Task<ActionResult<IEnumerable<ReagentStockAndSupplyDTO>>> reagentStockAndSupply(string stockProgressStatus)
        {
            string whStockProgressCondition = "";
            
            if (stockProgressStatus != null || stockProgressStatus != "0")
            {
                whStockProgressCondition = @" and (case when nvl(nofpipeline,0)>0 and nvl(nosfacstock,0)=0 then 'Receipt Pending and Stock out'  
else case when nvl(nofpipeline,0)>0 and  nvl(nosfacstock,0)>0 then 'Receipt Pending and Stock Available'
else case when nvl(nofpipeline,0)=0 and  nvl(nosfacstock,0)=0 and nvl(flab.nosfaclabissued,0)>0 then 'Issued to Lab and Stock out'
else case when nvl(nofpipeline,0)=0 and  nvl(nosfacstock,0)=0 and nvl(flab.nosfaclabissued,0)=0 and nvl(mmiss.nosissued,0)=0 then 'Required to Check Equipment as No Distribution received and Stock out'
else case when nvl(nofpipeline,0)=0 and  nvl(nosfacstock,0)=0 and nvl(flab.nosfaclabissued,0)=0 and nvl(mmiss.nosissued,0)=0 and nvl(st.nos,0)>0 then 'Required to Check Equipment or Distribution as stock Available in Warehouse and Stock out in Facility'
else case when nvl(flab.nosfaclabissued,0)=0  and  nvl(nosfacstock,0)>0 then 'Lab Issuance not done'
else 
'Not Required' end end end end end end) = '"+ stockProgressStatus + @"' ";

            }

            string qry = @"   
select 0 as SlNo,m.facilityname,m.eqpname,m.nosreagent,nvl(st.nos,0) as stkavailable,
nvl(st.stockvalue,0) as stockvaluecr,nvl(mmiss.nosissued,0) nosissued,
nvl(mmiss.issuedvaluecr,0) nosissuedvaluecr,nvl(nofpipeline,0) nosfacpipeline,nvl(facpipelinevalue,0) nosfacpipelinevalue,
nvl(flab.nosfaclabissued,0) nosfaclabissued,nvl(flab.faclabissuevaluecr,0) faclabissuevaluecr,
nvl(nosfacstock,0) nosfacstock,nvl(mmfacstk.facstockvaluecr,0) facstockvaluecr
,case when nvl(nofpipeline,0)>0 and nvl(nosfacstock,0)=0 then 'Receipt Pending and Stock out'  
else case when nvl(nofpipeline,0)>0 and  nvl(nosfacstock,0)>0 then 'Receipt Pending and Stock Available'
else case when nvl(nofpipeline,0)=0 and  nvl(nosfacstock,0)=0 and nvl(flab.nosfaclabissued,0)>0 then 'Issued to Lab and Stock out'
else case when nvl(nofpipeline,0)=0 and  nvl(nosfacstock,0)=0 and nvl(flab.nosfaclabissued,0)=0 and nvl(mmiss.nosissued,0)=0 then 'Required to Check Equipment as No Distribution received and Stock out'
else case when nvl(nofpipeline,0)=0 and  nvl(nosfacstock,0)=0 and nvl(flab.nosfaclabissued,0)=0 and nvl(mmiss.nosissued,0)=0 and nvl(st.nos,0)>0 then 'Required to Check Equipment or Distribution as stock Available in Warehouse and Stock out in Facility'
else case when nvl(flab.nosfaclabissued,0)=0  and  nvl(nosfacstock,0)>0 then 'Lab Issuance not done'
else 
'Not Required' end end end end end end as stkp
,m.facilityid,m.ORDERDP,EqAvailable,m.districtname,m.districtid,m.MMID,hf.FOOTER3,hf.FOOTER2,nvl(hf.FOOTER2,'-')||'('||hf.FOOTER3||')' as contact

from 
(
select d.districtid,d.districtname,mm.MMID,eqpname,
count(distinct m.itemid) as nosreagent,f.facilityid,f.facilityname ,ft.ORDERDP,case when ef.facilityid =f.facilityid and ef.mmid=mm.mmid then 'Equipment Available' else 'Equipment Not Available' end as EqAvailable
from masdistricts d,masfacilities f,masfacilityequipment ef,masfacilitytypes ft ,masreagenteqp eqp
inner join masreagentmakemodel mm on mm.PMACHINEID=eqp.PMACHINEID
inner join masitems m on m.mmid=mm.mmid
where m.isfreez_itpr is null and d.stateid=1 and ef.ISACTIVE=1   --and d.districtid=2207   and f.facilityid=23253   
and mm.mmid=3  and f.districtid=d.districtid
and ef.mmid=mm.mmid and ef.facilityid=f.facilityid and ft.facilitytypeid=f.facilitytypeid
group by  mm.MMID,eqpname,d.districtid,d.districtname,f.facilityid,f.facilityname ,ft.ORDERDP,ef.facilityid,ef.mmid,mm.mmid
) m
left outer join usrusers u on u.facilityid=m.facilityid
left outer join masfacheaderfooter hf on hf.userid=u.userid
left outer join 
(
select districtid,count(distinct itemcode) as nos ,Round(sum(StockValue)/100000,2) as stockvalue,mmid
from 
(
select districtid,itemcode,itemname,eqpname,sum(nvl(ReadyForIssue,0)) as CurrentStock,
sum(nvl(ReadyValue,0)) as StockValue,mmid  from 
(
           select w.districtid,w.warehouseid,w.WAREHOUSENAME, mi.itemcode,mi.itemname,eqp.eqpname,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,                         
mi.categoryid,ci.finalrategst,(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end )*ci.finalrategst ReadyValue
                ,mm.mmid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 left outer join masreagentmakemodel mm on mm.mmid = mi.mmid
                 left outer join masreagenteqp eqp on eqp.pmachineid = mm.pmachineid
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid  
                 inner join soordereditems si on si.ponoid=b.ponoid
                 inner join aoccontractitems ci on ci.contractitemid=si.contractitemid
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
                 and mi.categoryid=62
                 ) group by itemcode,itemname,eqpname,mmid ,districtid
                 having sum(nvl(ReadyForIssue,0))>0 
) group by mmid,districtid
) st on st.mmid=m.mmid and st.districtid = m.districtid

left outer join
(
select facilityid,districtid, mmid,count (distinct itemid) as nosissued,round(sum(issuevalue)/100000,2) as issuedvaluecr from
(
            select t.facilityid, f.districtid,m.mmid,m.itemid,sum(tbo.issueqty) issueqty,sum(tbo.issueqty)*c.finalrategst issuevalue from tboutwards tbo 
             inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
             inner join tbindents t on t.indentid = tbi.indentid 
               inner join masfacilities f on f.facilityid = t.facilityid
                inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid 
             inner join masitems m on m.itemid = tbi.itemid
                    left outer join masreagentmakemodel mm on mm.mmid = m.mmid
                 left outer join masreagenteqp eqp on eqp.pmachineid = mm.pmachineid
             inner join tbreceiptbatches b on b.inwno = tbo.inwno
             inner join soordereditems oi on oi.ponoid = b.ponoid
             inner join aoccontractitems c on c.contractitemid = oi.contractitemid
             where  t.issuetype = 'NO' and t.status = 'C' and m.categoryid = 62  
             and t.indentdate between  (select startdate  from masaccyearsettings where accyrsetid = 544) 
             and  (select enddate  from masaccyearsettings where accyrsetid = 544)
             group by m.mmid,m.itemid,c.finalrategst,f.districtid,t.facilityid
             having sum(tbo.issueqty)>0 
             ) group by mmid,districtid,facilityid
) mmiss on mmiss.mmid = m.mmid and mmiss.districtid = m.districtid and mmiss.facilityid=m.facilityid 
left outer join
(
select facilityid,districtid, mmid,count (distinct itemid) as nosfacstock,round(sum(stockvalue)/100000,2) facstockvaluecr
from
(
select f.facilityid,f.districtid,mi.mmid,mi.itemcode,mi.itemid
               ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   inhand_qty,round((sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0)))*(c.finalrategst/nvl(mi.unitcount,1)),2) stockvalue 
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid
                inner join masfacilities f on f.facilityid = t.facilityid 
                 inner join soordereditems oi on oi.ponoid = b.ponoid
                 inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                 inner join masitems mi on mi.itemid=i.itemid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C' and fs.issuetype = 'NO' and fs.issueddate >   '31-Mar-2023'                
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid                  
                 Where 1=1 and mi.categoryid = 62
                 and T.Status = 'C'  and t.facreceipttype = 'NO' and t.facreceiptdate > '31-Mar-2023' and b.expdate>sysdate 
               group by mi.itemcode,mi.itemid,mi.mmid,c.finalrategst,mi.unitcount,f.districtid,f.facilityid
               having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0  
                 ) group by mmid,districtid,facilityid
) mmfacstk on mmfacstk.mmid = m.mmid and mmfacstk.districtid = m.districtid and mmfacstk.facilityid=m.facilityid 
left outer join
(
 select facilityid,districtid,mmid,count (distinct itemid) as nofpipeline,round(sum(facpipelinevalue)/100000,2) facpipelinevalue
from
(
 select  f.facilityid,f.districtid,m.itemcode,m.itemid,m.mmid,sum(tbo.issueqty) facpipelineqty,(sum(tbo.issueqty))*c.finalrategst facpipelinevalue   
         from tbindents tb
         inner join masfacilities f on f.facilityid = tb.facilityid 
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join soordereditems oi on oi.ponoid = rb.ponoid
         inner join aoccontractitems c on c.contractitemid = oi.contractitemid
         inner join masitems m on m.itemid = tbi.itemid
         left outer join 
         (
         select distinct r.INDENTID,r.facreceiptno,r.facreceiptdate,ri.itemid from tbfacilityreceipts r 
            inner join tbfacilityreceiptitems ri on ri.facreceiptid=r.facreceiptid
            inner join tbfacilityreceiptbatches rb on rb.facreceiptitemid=ri.facreceiptitemid
            inner join masitems m on m.itemid=ri.itemid
            where r.status='C' and m.categoryid=62
         )r on r.INDENTID=tb.INDENTID and r.itemid=m.itemid

         where tb.status = 'C' and tb.issuetype='NO' 
         and tb.indentdate between '01-APR-23' and '31-MAR-24' 
         and m.categoryid in (62) and r.facreceiptno is  null      
        group by m.itemcode,m.itemid,m.mmid,c.finalrategst,f.districtid, f.facilityid
          ) group by mmid,districtid,facilityid
) mmfacpip on mmfacpip.mmid = m.mmid and mmfacpip.districtid = m.districtid and mmfacpip.facilityid=m.facilityid 
left outer join
(
select facilityid,districtid,mmid,count (distinct itemid) as nosfaclabissued,round(sum(faclabissuevalue)/100000,2) faclabissuevaluecr
from
(
select  f.facilityid,f.districtid,m.itemid,m.itemcode,m.mmid,sum(nvl(ftbo.issueqty,0)) faclabissueqty, round(sum(nvl(ftbo.issueqty,0))*(c.finalrategst/nvl(m.unitcount,1)),2) faclabissuevalue  
                     from tbfacilityissues fs 
                       inner join masfacilities f on f.facilityid = fs.facilityid 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid
                   inner join tbfacilityreceiptbatches b on b.inwno = ftbo.inwno
                   inner join tbfacilityreceiptitems ri on ri.facreceiptitemid=b.facreceiptitemid
                   inner join tbfacilityreceipts ft on ft.facreceiptid=ri.facreceiptid
                   inner join soordereditems oi on oi.ponoid = b.ponoid
                   inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                   inner join masitems m on m.itemid = fsi.itemid
                   where fs.status = 'C' 
                   and ft.facreceipttype = 'NO'
                   and fs.issuetype = 'NO' 
                   and ft.facreceiptdate >'31-Mar-2023' 
                   and  fs.issueddate >   '31-Mar-2023'
                   and m.categoryid = 62
                   group by m.itemid,m.itemcode,m.mmid,c.finalrategst,m.unitcount,f.districtid,f.facilityid
   ) group by mmid ,districtid,facilityid
) flab on flab.mmid = m.mmid and flab.districtid = m.districtid and flab.facilityid=m.facilityid 

where 1=1  and m.mmid=3
"+ whStockProgressCondition + @"
order by m.ORDERDP   ";
            var myList = _context.ReagentStockAndSupplyDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("getCurrentLiability")]
        public async Task<ActionResult<IEnumerable<CurrentLiabilityDTO>>> getCurrentLiability(string budgetid)
        {


            string qry = @"  select count(distinct PoNoID) as TotalFile,round((round(nvl( sum(FITLibadm),0),0)+round(nvl( sum(NotFITLibadm),0),0))/10000000,2)as totlibwithAdm,
nvl(sum(nofile),0) as noofFileFit,round(round(nvl( sum(FITLibadm),0),0)/10000000,2) as FitLibValeWithAdmin
,nvl(count(distinct PoNoID)- nvl(sum(nofile),0),0) as noofFileNotFit,
round(round(nvl( sum(NotFITLibadm),0),0)/10000000,2) as NotFitLibValeWithAdmin
,round(((nvl(sum(FITLibadm),0)+nvl(sum(NotFITLibadm),0))-nvl(sum(receiptvalueadm),0))/10000000,2) as PipelineValue
,round(round(nvl( sum(NotFITLibadm),0),0)/10000000,2)-round(((nvl(sum(FITLibadm),0)+nvl(sum(NotFITLibadm),0))-nvl(sum(receiptvalueadm),0))/10000000,2) as NotfitButReceived
,round(nvl(ext.POVALUE_CR_ADM,0)/10000000,2) extandable_lib_cr
,round(round(nvl(sum(sancamountadm),0),0)/10000000,2) as sancamountadm
,round(nvl(sum(totalpovalue),0),0) totalpovalue 
,round( nvl(sum(totalpovalueadm),0),0) as totalpovalueadm ,
round(nvl(sum(receiptvalue),0),0) as receiptvalue

,round(nvl(sum(sancamount),0),0) as sancamount,

round(nvl(sum(witheldamt),0),0) as witheldamt

,round( nvl(sum(FITLib),0),0) as FITLib,round(nvl( sum(NotFITLib),0),0) as NotFITLib
from 
 masbudget b 
 left outer join
(
select PoNoID,budgetid,case when fitunfit='Fit' then 1 else 0 end nofile,case when fitunfit='Fit' then libnew else 0 end as FITLib,case when fitunfit='Fit' then LibNewAdm else 0 end as FITLibadm
,case when fitunfit='Not Fit' then libnew else 0 end as NotFITLib,case when fitunfit='Not Fit' then LibNewAdm else 0 end as NotFITLibadm
,totalpovalue,totalpovalueadm,receiptvalue,receiptvalueadm,sancamount,sancamountadm,witheldamt,libnew,LibNewAdm 
from 

(
select  budgetid,op.suppliername,op.pono,op.PoNoID,POQTY, nvl(receiptqty,0) as rec, case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end  as RecPer ,totalpovalue,totalpovalueadm,receiptvalue,receiptvalueadm,sancamount,sancamountadm,witheldamt,libnew,LibNewAdm 
,validity
,nvl(sdamount,0) as sdamount
,case when (issdrequired is null or issdrequired = 'Y') then 'Y' else 'N' end sdrequired
,case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end as SDRequiredDoc
 ,nvl(QCPass,0) as QCPass
,case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end as ReceiptQCFlag
,case when nvl(res.reasonid,0)>0 and  (nvl(res.reasonid,0)=10 or nvl(res.reasonid,0)=11 or nvl(res.reasonid,0)=12)
 then 0 else case when nvl(res.reasonid,0)=0 then 0  else 1 end end  as ReasonNameTB
 ,case when  nvl(res.IsfitReason,0)=1  then 0 else 1 end ReasonNameTBNew
 
 ,  case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)=100
 and validity in ('Supplied and Time Expired','Supplied and Time Valid')
 and   ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)+
 (case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )+
 (case when  nvl(res.IsfitReason,0)=1  then 0 else (case when IsRelaxBelow90=1 then 0 else 1 end ) end )
 )=0 then 'Fit' 
 else
 case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)>=90
 and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)<100
 and validity in ('Supplied and Time Expired','Supplied and Time Valid')
 and   ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)+(case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )
 +(case when  nvl(res.IsfitReason,0)=1  then 0 else (case when IsRelaxBelow90=1 then 0 else 1 end ) end ))=0 then 'Fit' 
 else
  case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)<90 and validity in ('Supplied and Time Expired')
 and ((case when (issdrequired is null or issdrequired = 'Y') then 
 (case when (nvl(sdamount,0)>=round(totalpovalue*5/100,0)) then 0 else 1 end  ) else 0 end)+(case when nvl(receiptqty,0)>0 and nvl(QCPass,0)>=nvl(receiptqty,0) then 0 else 1 end )
 +(case when  nvl(res.IsfitReason,0)=1  then 0 else (case when IsRelaxBelow90=1 then 0 else 1 end ) end ))=0 then 'Fit' else 
 'Not Fit' end end end as fitunfitold
 
 
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
 'Not Fit' end end end as fitunfit
 
from 
 v_popaymentstatus op
 inner join masschemes mc on mc.schemeid=op.schemeid
left outer join ldlevies l on l.schemeid=mc.schemeid and l.status='C' and l.isactive=1
 left outer join
(
select sdc.ponoid, sum(sdc.sdamount)sdamount,sd.sdid  from sdsupmaster sd 
inner join  sdsupchild sdc on sdc.sdid = sd.sdid
where sd.status = 'C'  and sd.ishoreceipt='Y'
group by sdc.ponoid,sd.sdid 
)sdiq on sdiq.ponoid = op.ponoid
left outer join  
(
select itemid,ponoid,sum(nvl(QCPass,0)) as QCPass,qastatus from 
(
select  i.itemid, t.ponoid, sum(nvl(tb.absrqty,0)) as QCPass,tb.inwno,
(case when (mi.qctest = 'N' and tb.qastatus = 0) then 'QC Not Required' when  tb.qastatus = 0 then 'Result Pending' when tb.qastatus = 1 then 'SQ' when tb.qastatus = 2 then 'NSQ'  end ) qastatus
 from tbreceipts t
 inner join soOrderPlaced op on op.ponoid=t.ponoid
inner join tbreceiptitems i on (i.receiptid = t.receiptid)
inner join tbreceiptbatches TB on (I.receiptitemid = TB.receiptitemid)
inner join masitems mi on mi.itemid = i.itemid
where op.accyrsetid>=539  and t.receipttype='NO' and t.status = 'C' and t.notindpdmis is null and i.notindpdmis is null and TB.notindpdmis is null
group by I.ItemID,  T.PoNoID,tb.inwno,mi.qctest,tb.qastatus
) where qastatus  in ('QC Not Required','SQ')

group by itemid,ponoid,qastatus
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
 where paymentstatusNew='Not Paid'
) 
)a on a.budgetid=b.budgetid


left outer join
(
select budgetid,sum(povalue) povalue_rs,round(sum(povalue)+ round(sum(povalue)*5/100,2),2) povalue_rs_adm, round(sum(povalue)/100000,2) povalue_lk
, round(round(sum(povalue)/100000,2) + (round(sum(povalue)/100000,2) * 5/100),2) povalue_lk_ADM,
round(sum(povalue)/10000000,2) povalue_cr,  round((sum(povalue)+ round(sum(povalue)*5/100,2))/10000000,2) povalue_cr_adm from (
select  m.itemcode,m.NABLREQ,OI.itemid,op.budgetid,
case when m.NABLREQ = 'Y' and round(sysdate-op.soissuedate,0) between 121 and 150 then  oi.ABSQTY
else case when  m.NABLREQ is null and round(sysdate-op.soissuedate,0) between 91 and 120 then  oi.ABSQTY
else 0 end end as poqty,
case when m.NABLREQ = 'Y' and round(sysdate-op.soissuedate,0) between 121 and 150 then  nvl(oi.ABSQTY*ci.finalrategst,0)
else case when  m.NABLREQ is null and round(sysdate-op.soissuedate,0) between 91 and 120 then  nvl(oi.ABSQTY*ci.finalrategst,0)
else 0 end end povalue,
round(sysdate-op.soissuedate,0) as days
from   soOrderPlaced OP  
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems ci on ci.contractitemid = oi.contractitemid
where op.status  in ('C','O') and (case when m.NABLREQ = 'Y' and round(sysdate-op.soissuedate,0) between 121 and 150 then  oi.ABSQTY
else case when  m.NABLREQ is null and round(sysdate-op.soissuedate,0) between 91 and 120 then  oi.ABSQTY
else 0 end end)>0 )
group by budgetid
) ext on ext.budgetid = b.budgetid
where b.ISVISIBLE is null and b.budgetid=" + budgetid + @"
group by a.budgetid, b.budgetid,b.budgetname,ext.POVALUE_CR_ADM,ext.POVALUE_RS_ADM,ext.POVALUE_LK_ADM
order by b.budgetid  ";
            var myList = _context.CurrentLiabilityDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



        [HttpGet("getWHreagentStock")]
        public async Task<ActionResult<IEnumerable<WHreagentStockDTO>>> getWHreagentStock(Int64 pmachineid)
        {

            string whPmachineid = "";

            if(pmachineid != 0)
            {
                whPmachineid = " and eqp.pmachineid = "+ pmachineid + "  ";
            }


            string qry = @" select ROW_NUMBER() OVER (ORDER BY eqpname) AS serial_no,a.eqpname,mr.nosreagent as Required,WAREHOUSENAME,
count(distinct a.itemcode) as nos ,Round(sum(a.StockValue)/10000000,2) as stockvalue,a.mmid,count(distinct warehouseid) as noswh
from 
(
select itemcode,itemname,eqpname,sum(nvl(ReadyForIssue,0)) as CurrentStock,WAREHOUSENAME,
sum(nvl(ReadyValue,0)) as StockValue,mmid, warehouseid  from (

           select w.districtid,w.warehouseid,w.WAREHOUSENAME, mi.itemcode,mi.itemname,eqp.eqpname,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,                         
mi.categoryid,ci.finalrategst,(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end )*ci.finalrategst ReadyValue
                ,mm.mmid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 left outer join masreagentmakemodel mm on mm.mmid = mi.mmid
                 left outer join masreagenteqp eqp on eqp.pmachineid = mm.pmachineid
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid  
                 inner join soordereditems si on si.ponoid=b.ponoid
                 inner join aoccontractitems ci on ci.contractitemid=si.contractitemid
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
                 and mi.categoryid=62
                 "+ whPmachineid + @"
                 ) group by itemcode,itemname,eqpname,mmid ,warehouseid,WAREHOUSENAME
                 having sum(nvl(ReadyForIssue,0))>0 
)a
left outer join 
(
select mm.MMID,count(distinct m.itemid) as nosreagent  from masreagenteqp eqp
inner join masreagentmakemodel mm on mm.PMACHINEID=eqp.PMACHINEID
inner join masitems m on m.mmid=mm.mmid
group by  mm.MMID
) mr on mr.MMID=a.mmid
group by a.eqpname,a.mmid,mr.nosreagent,a.WAREHOUSENAME
order by a.eqpname
  ";
            var myList = _context.WHreagentStockDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }



        [HttpGet("ddlMasreagenteqp")]
        public async Task<ActionResult<IEnumerable<DdlMasreagenteqpDTO>>> ddlMasreagenteqp()
        {

           


            string qry = @" select pmachineid, eqpname from masreagenteqp  ";
            var myList = _context.DdlMasreagenteqpDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

    }
}
