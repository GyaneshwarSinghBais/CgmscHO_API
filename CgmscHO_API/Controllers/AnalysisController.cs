using CgmscHO_API.DTO;
using CgmscHO_API.HODTO;
using CgmscHO_API.Models;
using CgmscHO_API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CgmscHO_API.AnalysisDTO;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly OraDbContext _context;

        public AnalysisController(OraDbContext context)
        {
            _context = context;
        }

        [HttpGet("ABCanalysisSummary")]
        public async Task<ActionResult<IEnumerable<ABCanalysisSummaryDTO>>> ABCanalysisSummary(string yearid, string mcid, string isedl)
        {
            string whyearid = "";
            string whmcid = "";
            string whisedl = "";

            if (yearid != "0")
            {
                whyearid = " and accyrsetid=" + yearid;
            }
            
            if (mcid != "0")
            {
                whmcid = " and mc.mcid = " + mcid;
            }
           
            if (isedl != "0")
            {
                whisedl = " and nvl(MI.isedl2021,'N')='"+ isedl + "' " ;
            }


            string qry = @" select ABC_CATEGORY,count(ITEM_ID) as NoOfItems,round( sum(ORDER_VALUE)/100,2) as ORDER_VALUE,sum(RCValid) as RCValid,sum(RCNotValid) as RCNotValid from (
SELECT ITEM_ID,ITEMCODE
       DRUG_NAME,STRENGTH1,UNIT,ITEMTYPENAME,EDLCAT
 , case  when RCValid ='Valid' then 1 else 0 end as RCValid
       , case  when RCValid ='Not Valid' then 1 else 0 end as RCNotValid,
    
       ORDER_VALUE,
       SUM(ORDER_VALUE) 
           OVER (ORDER BY ORDER_VALUE DESC) AS CUMULATIVE_VALUE,
       ROUND(SUM(ORDER_VALUE) 
           OVER (ORDER BY ORDER_VALUE DESC) 
           / SUM(ORDER_VALUE) OVER () * 100, 2) AS CUMULATIVE_PERCENT,
       CASE
         WHEN ROUND(SUM(ORDER_VALUE) 
           OVER (ORDER BY ORDER_VALUE DESC) 
           / SUM(ORDER_VALUE) OVER () * 100, 2) <= 70 THEN 'A'
         WHEN ROUND(SUM(ORDER_VALUE) 
           OVER (ORDER BY ORDER_VALUE DESC) 
           / SUM(ORDER_VALUE) OVER () * 100, 2) <= 90 THEN 'B'
         ELSE 'C'
       END AS ABC_CATEGORY
FROM (

select x.ITEMID as ITEM_ID, ITEMCODE, ITEMNAME as DRUG_NAME, STRENGTH1,EDLCAT,ITEMTYPENAME, UNIT, EDLTYPE, ORDEREDVALUE as ORDER_VALUE
,case when rc.RCENDDate is null then 'Not Valid' else 'Valid' end  as RCValid

from (

select itemid, ITEMCODE, ITEMNAME, STRENGTH1, UNIT, EDLType,  EDLCAT, ITEMTYPENAME,  sum(ORDEREDQTY) as ORDEREDQTY,  
round(sum(ORDEREDVALUE)/100000,2) as ORDEREDVALUE, sum(RECEIPTQTY) as RECEIPTQTY ,round(sum(RECEIPTVALUE)/100000,2) as RECEIPTVALUE from (
select ms.schemecode tenderno,mi.itemcode,itemname,MI.itemid
,MI.strength1,MI.unit,MI.unitcount,mt.ITEMTYPENAME,edl.edl as EDLCAT

,op.ponoid,op.pono pono,to_char( op.soissuedate,'dd/mm/yyyy') podate,s.suppliername,
                            getfinalRateContract1(b.contractitemid,OP.soissuedate) finalrate,AbsQty as OrderedQty,nvl(ReceiptQty,0) ReceiptQty,
                            round((case when nvl(absqty,0)=0 then 0 else (nvl(receiptqty,0)/nvl(absqty,0))*100 end),0) supplypercent,

                            (nvl(absqty,0)* getfinalRateContract1(b.contractitemid,OP.soissuedate)) orderedvalue,
                            (nvl(ReceiptQty,0) * getfinalRateContract1(b.contractitemid,OP.soissuedate)) ReceiptValue 
                            ,EXTRACT(YEAR FROM op.soissuedate) AS Finyear
                            ,case when  nvl(MI.isedl2021,'N')='Y' then 'EDL' else 'NON EDL' end  as EDLType
                            from masItems MI
                                              inner join soordereditems oi on (oi.itemid = mi.itemid) 
                                              inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' ))
                                              inner join aoccontractitems b on (b.contractitemid = oi.contractitemid and b.itemid = oi.itemid)
                                              inner join masSuppliers S on (S.SupplierID = OP.SupplierID) 
                                              inner join masschemes MS on (MS.schemeid = OP.schemeid)
                                              inner join masitemcategories ic on ic.categoryid = mi.categoryid
                                              inner join masitemmaincategory mc on mc.MCID=ic.MCID
                                              inner join masitemtypes mt on mt.ITEMTYPEID=MI.ITEMTYPEID
                                              inner join masedl edl on edl.EDLCAT=MI.EDLCAT


                                              left outer join  
                                             (
                                                select distinct i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as receiptqty                  
                                                from tbreceipts t 
                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid) 
                                                where T.Status = 'C' and  T.receipttype = 'NO' and t.receiptid not in (select tr.receiptid
                                                                           from tbindents t  
                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                                                           where t.status = 'C' and t.issuetype in ('RS') )
                                                and tb.notindpdmis is null
                                                group by I.ItemID, T.PoNoID                       
                                              ) r on (r.itemid =oi.itemid and r.ponoid =op.ponoid) 
                                              where 1=1 " + whmcid + @" and op.soissuedate between (select STARTDATE from masaccyearsettings where 1=1 "+ whyearid + @") and (select ENDDATE from masaccyearsettings where 1=1 "+ whyearid + @")
                                              "+ whisedl + @"

                                              ) group by  itemid, ITEMCODE, ITEMNAME, STRENGTH1, UNIT, EDLType ,EDLCAT,ITEMTYPENAME
                                              
                                              )x
                                               left outer join 
                                              (
                                              select itemid ,max(RCENDDT) as RCENDDate,count(Distinct SupplierID) as cntSup from v_rcvalid
                                                group by itemid
                                              )rc on rc.itemid=x.itemid

                                         
                                                    

                                              order by ORDEREDVALUE desc


)
ORDER BY ORDER_VALUE DESC
) group by ABC_CATEGORY
order by ABC_CATEGORY;	 ";
            var myList = _context.ABCanalysisSummaryDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

//        [HttpGet("ABCanalysisSummaryDetail")]
//        public async Task<ActionResult<IEnumerable<ABCanalysisSummaryDetailDTO>>> ABCanalysisSummaryDetail(string yearid, string mcid, string isedl, string detail, string isRCvalid)
//        {
//            string whyearid = "";
//            string whmcid = "";
//            string whisedl = "";
//            string whDetail = "";
//            string whisRCvalid = "";

//            if (yearid != "0")
//            {
//                whyearid = " and accyrsetid=" + yearid;
//            }

//            if (mcid != "0")
//            {
//                whmcid = " and mc.mcid = " + mcid;
//            }

//            if (isedl != "0")
//            {
//               // whisedl = " and MI.isedl2021='" + isedl + "' ";
//                whisedl = " and nvl(MI.isedl2021,'N')='" + isedl + "' ";
//            }
//            if (detail != "0")
//            {
//                whDetail = "  and ABC_CATEGORY = '"+ detail + "'  ";
//            }
//            if (isRCvalid != "0")
//            {
//                if (isRCvalid == "Y")
//                {
//                    whisRCvalid = " and (case when  RCENDDate is null then 'Not Valid' else 'Valid' end) = 'Valid' ";
//                }
//                else if (isRCvalid == "N")
//                {
//                    whisRCvalid = " and (case when  RCENDDate is null then 'Not Valid' else 'Valid' end) = 'Not Valid' ";
//                }

               
//            }

//            string qry = @"  select * from (
//SELECT ITEM_ID,ITEMCODE,
//       DRUG_NAME,STRENGTH1,UNIT,ITEMTYPENAME,EDLCAT,case when  RCENDDate is null then 'Not Valid' else 'Valid' end RCStatus,RCENDDate
//       ,nvl(round(to_date(RCENDDate,'dd-MM-YYYY')-sysdate,0),0) rcremainingdays
//       , nvl(CNTSUP,0) as CNTSUP ,tenderstatus,
//       READYFORISSUE,PENDING,iwhPipeline,SupplierPipeline,
//       ORDER_VALUE,
//       SUM(ORDER_VALUE) 
//           OVER (ORDER BY ORDER_VALUE DESC) AS CUMULATIVE_VALUE,
//       ROUND(SUM(ORDER_VALUE) 
//           OVER (ORDER BY ORDER_VALUE DESC) 
//           / SUM(ORDER_VALUE) OVER () * 100, 2) AS CUMULATIVE_PERCENT,
//       CASE
//         WHEN ROUND(SUM(ORDER_VALUE) 
//           OVER (ORDER BY ORDER_VALUE DESC) 
//           / SUM(ORDER_VALUE) OVER () * 100, 2) <= 70 THEN 'A'
//         WHEN ROUND(SUM(ORDER_VALUE) 
//           OVER (ORDER BY ORDER_VALUE DESC) 
//           / SUM(ORDER_VALUE) OVER () * 100, 2) <= 90 THEN 'B'
//         ELSE 'C'
//       END AS ABC_CATEGORY
//FROM (

//select x.ITEMID as ITEM_ID, ITEMCODE, ITEMNAME as DRUG_NAME, STRENGTH1,EDLCAT,ITEMTYPENAME, UNIT, EDLTYPE, ORDEREDVALUE as ORDER_VALUE,RCENDDATE, CNTSUP,ts.ACTION tenderstatus 
//,nvl(READYFORISSUE,0) as READYFORISSUE,nvl(PENDING,0) as PENDING
//,nvl(IWHPipe.transferqty,0) as iwhPipeline
//, nvl(whpip.newpiple,0) as SupplierPipeline

//from (

//select itemid, ITEMCODE, ITEMNAME, STRENGTH1, UNIT, EDLType,  EDLCAT, ITEMTYPENAME,  sum(ORDEREDQTY) as ORDEREDQTY,   round(sum(ORDEREDVALUE)/100000,2) as ORDEREDVALUE, sum(RECEIPTQTY) as RECEIPTQTY ,round(sum(RECEIPTVALUE)/100000,2) as RECEIPTVALUE from (
//select ms.schemecode tenderno,mi.itemcode,itemname,MI.itemid
//,MI.strength1,MI.unit,MI.unitcount,mt.ITEMTYPENAME,edl.edl as EDLCAT

//,op.ponoid,op.pono pono,to_char( op.soissuedate,'dd/mm/yyyy') podate,s.suppliername,
//                            getfinalRateContract1(b.contractitemid,OP.soissuedate) finalrate,AbsQty as OrderedQty,nvl(ReceiptQty,0) ReceiptQty,
//                            round((case when nvl(absqty,0)=0 then 0 else (nvl(receiptqty,0)/nvl(absqty,0))*100 end),0) supplypercent,

//                            (nvl(absqty,0)* getfinalRateContract1(b.contractitemid,OP.soissuedate)) orderedvalue,
//                            (nvl(ReceiptQty,0) * getfinalRateContract1(b.contractitemid,OP.soissuedate)) ReceiptValue 
//                            ,EXTRACT(YEAR FROM op.soissuedate) AS Finyear
//                            ,case when  nvl(MI.isedl2021,'N')='Y' then 'EDL' else 'NON EDL' end  as EDLType
//                            from masItems MI
//                                              inner join soordereditems oi on (oi.itemid = mi.itemid) 
//                                              inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' ))
//                                              inner join aoccontractitems b on (b.contractitemid = oi.contractitemid and b.itemid = oi.itemid)
//                                              inner join masSuppliers S on (S.SupplierID = OP.SupplierID) 
//                                              inner join masschemes MS on (MS.schemeid = OP.schemeid)
//                                              inner join masitemcategories ic on ic.categoryid = mi.categoryid
//                                              inner join masitemmaincategory mc on mc.MCID=ic.MCID
//                                              inner join masitemtypes mt on mt.ITEMTYPEID=MI.ITEMTYPEID
//                                              inner join masedl edl on edl.EDLCAT=MI.EDLCAT


//                                              left outer join  
//                                             (
//                                                select distinct i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as receiptqty                  
//                                                from tbreceipts t 
//                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
//                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid) 
//                                                where T.Status = 'C' and  T.receipttype = 'NO' and t.receiptid not in (select tr.receiptid
//                                                                           from tbindents t  
//                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
//                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
//                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
//                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
//                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
//                                                                           where t.status = 'C' and t.issuetype in ('RS') )
//                                                and tb.notindpdmis is null
//                                                group by I.ItemID, T.PoNoID                       
//                                              ) r on (r.itemid =oi.itemid and r.ponoid =op.ponoid) 
//                                              where 1=1 "+ whmcid + " and op.soissuedate between (select STARTDATE from masaccyearsettings where 1=1 "+ whyearid + @") and (select ENDDATE from masaccyearsettings where 1=1 "+ whyearid + @")
//                                              "+ whisedl + @"

//                                              ) group by  itemid, ITEMCODE, ITEMNAME, STRENGTH1, UNIT, EDLType ,EDLCAT,ITEMTYPENAME
                                              
//                                              )x
//                                              left outer join 
//                                              (
//                                              select itemid ,max(RCENDDT) as RCENDDate,count(Distinct SupplierID) as cntSup from v_rcvalid
//                                                group by itemid
//                                              )rc on rc.itemid=x.itemid
//                                              left outer join
//                                             (
//                                             select ts.ITEMID, ACTION, ACTIONCODE,COV_A_OPDATE, COV_B_OPDATE, PRICEBIDDATE, SCHEMEID,COVA_BIDS, COVA_BIDB 
//                                             from v_tenderstatusallnew ts
//                                             inner join masitems m on m.itemid=ts.itemid
//                                                inner join masitemcategories c on c.categoryid = m.categoryid
//                                                inner join masitemmaincategory mc on mc.mcid = c.mcid
//                                             where 1=1 "+ whmcid + @"
//                                             ) ts on ts.itemid = x.itemid

//                                             left outer join 
//                                                    (                                                    
//                                                    select itemid, sum(READYFORISSUE) as READYFORISSUE,sum(PENDING) as  PENDING from (
//                                                    select mi.itemid, t.warehouseid,                                                    
//                                                     nvl((case when tbr.qastatus ='1' then (nvl(tbr.absrqty,0) - nvl(tbr.issueqty,0)) else (case when mi.Qctest ='N' and tbr.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(tbr.absrqty,0) - nvl(tbr.issueqty,0) ) end  end ) end ),0) ReadyForIssue,    
//                                                                        nvl(case when  mi.qctest='N' then 0 else (case when tbr.qastatus = 0 or tbr.qastatus = 3 then (nvl(tbr.absrqty,0)- nvl(tbr.issueqty,0)) end) end,0)  Pending    
//                                                    from tbreceiptbatches tbr
//                                                    inner join tbreceiptitems tbi on tbi.receiptitemid=tbr.receiptitemid
//                                                    inner join tbreceipts t on t.receiptid=tbi.receiptid
//                                                    inner join masitems mi on mi.itemid=tbi.itemid
//                                                    inner join masitemcategories c on c.categoryid = mi.categoryid
//                                                    inner join masitemmaincategory mc on mc.mcid = c.mcid
//                                                    where  T.Status = 'C'  "+ whmcid + @"
//                                                    and mi.isfreez_itpr is null
//                                                    "+ whisedl + @"
//                                                    And (tbr.ExpDate >= SysDate or nvl(tbr.ExpDate,SysDate) >= SysDate) and (tbr.Whissueblock = 0 or tbr.Whissueblock is null)
//                                                    and (nvl(ABSRQTY,0)-nvl(ISSUEQTY,0))>0 
//                                                    ) group by itemid                                                    
//                                                    )whs on  whs.itemid=x.itemid  
//                                                    left outer join
//                                                    (
//                                                    select i.itemid,sum(o.issueqty) as transferQTY
//                                                    from stktransfers t
//                                                    inner join stktransferitems i on i.transferid = t.transferid
//                                                    inner join tbindents ti on ti.transferid = t.transferid
//                                                    inner join tbindentitems tbi on tbi.indentid = ti.indentid and tbi.itemid = i.itemid
//                                                    inner join tboutwards o on o.indentitemid = tbi.indentitemid
//                                                    where t.status = 'C' 
//                                                    and t.transferid in (select transferid from tbindents where status = 'C' and transferid is not null)
//                                                    and t.transferid not in (select transferid from tbreceipts where status = 'C' and transferid is not null)
//                                                    and t.transferdate between '01-APR-25' and sysdate 
//                                                    group by i.itemid
//                                                    ) IWHPipe on  IWHPipe.itemid=x.itemid 
//                                                     left outer join 
//                                                                     (
//                                                    select  itemid,sum(pipelineQTY) newpiple 
//                                                    from (select  soi.warehouseid, m.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
//                                                    receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
//                                                    case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
//                                                    else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
//                                                    else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end as pipelineQTY
//                                                    from   soOrderPlaced OP  
//                                                    inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
//                                                    inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
//                                                    inner join masitems m on m.itemid = oi.itemid
//                                                    left outer join 
//                                                    (
//                                                    select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty, tr.warehouseid from tbreceipts tr 
//                                                    inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
//                                                    where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
//                                                    group by tr.ponoid,tri.itemid,tr.warehouseid
//                                                    ) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid and rec.warehouseid=soi.warehouseid
//                                                     where op.status  in ('C','O') --and m.categoryid in (52,53,54,55) --and m.itemcode = 'D395'
//                                                     group by soi.warehouseid, m.itemcode,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
//                                                     op.soissuedate,op.extendeddate ,receiptdelayexception  
//                                                     having (case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
//                                                    else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
//                                                    else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end) >0) group by itemid 
//                                                     ) whpip on whpip.itemid=x.itemid  

//                                              order by ORDEREDVALUE desc


//)
//ORDER BY ORDER_VALUE DESC
//) where 1=1 "+ whDetail + @"
//"+ whisRCvalid + @"
//";
//            var myList = _context.ABCanalysisSummaryDetailDbSet
//           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

//            return myList;

//        }



        [HttpGet("ABCanalysisSummaryDetail")]
        public async Task<ActionResult<IEnumerable<ABCanalysisSummaryDetailDTO>>> ABCanalysisSummaryDetail(
    string yearid, string mcid, string isedl)
        {
            string whyearid = "";
            string whmcid = "";
            string whisedl = "";

            // Apply filters
            if (yearid != "0")
                whyearid = " and accyrsetid = " + yearid;

            if (mcid != "0")
                whmcid = " and mc.mcid = " + mcid;

            if (isedl != "0")
                whisedl = " and nvl(MI.isedl2021,'N') = '" + isedl + "'";

            string qry = $@"  select * from (
SELECT ITEM_ID,ITEMCODE
       DRUG_NAME,STRENGTH1,UNIT,ITEMTYPENAME,EDLCAT,case when  RCENDDate is null then 'Not Valid' else 'Valid' end RCStatus,RCENDDate
       ,nvl(round(to_date(RCENDDate,'dd-MM-YYYY')-sysdate,0),0) rcremainingdays
       , nvl(CNTSUP,0) as CNTSUP ,tenderstatus,
       READYFORISSUE,PENDING,iwhPipeline,SupplierPipeline ,Pricecnt,Evalutioncnt,LiveCnt,Rentendercn,
       ORDER_VALUE,
       SUM(ORDER_VALUE) 
           OVER (ORDER BY ORDER_VALUE DESC) AS CUMULATIVE_VALUE,
       ROUND(SUM(ORDER_VALUE) 
           OVER (ORDER BY ORDER_VALUE DESC) 
           / SUM(ORDER_VALUE) OVER () * 100, 2) AS CUMULATIVE_PERCENT,
       CASE
         WHEN ROUND(SUM(ORDER_VALUE) 
           OVER (ORDER BY ORDER_VALUE DESC) 
           / SUM(ORDER_VALUE) OVER () * 100, 2) <= 70 THEN 'A'
         WHEN ROUND(SUM(ORDER_VALUE) 
           OVER (ORDER BY ORDER_VALUE DESC) 
           / SUM(ORDER_VALUE) OVER () * 100, 2) <= 90 THEN 'B'
         ELSE 'C'
       END AS ABC_CATEGORY
       
      
FROM (

select x.ITEMID as ITEM_ID, ITEMCODE, ITEMNAME as DRUG_NAME, STRENGTH1,EDLCAT,ITEMTYPENAME, UNIT, EDLTYPE, ORDEREDVALUE as ORDER_VALUE,RCENDDATE, CNTSUP,ts.ACTION tenderstatus 
,nvl(READYFORISSUE,0) as READYFORISSUE,nvl(PENDING,0) as PENDING
,nvl(IWHPipe.transferqty,0) as iwhPipeline
, nvl(whpip.newpiple,0) as SupplierPipeline,
 -- Tender status counts
            CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Price Opened in' THEN 'Yes' ELSE 'NO' END AS Pricecnt,
            CASE WHEN rc.RCENDDate IS NULL AND (ts.ACTIONCODE = 'Cover-A in' OR ts.ACTIONCODE = 'Claim Objection in') THEN 'Yes' ELSE 'NO' END AS Evalutioncnt,
            CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Live in' THEN 'Yes' ELSE 'NO' END AS LiveCnt,
            CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'To be Retender' THEN 'Yes' ELSE 'NO' END AS Rentendercn

from (

select itemid, ITEMCODE, ITEMNAME, STRENGTH1, UNIT, EDLType,  EDLCAT, ITEMTYPENAME,  sum(ORDEREDQTY) as ORDEREDQTY,   round(sum(ORDEREDVALUE)/100000,2) as ORDEREDVALUE, sum(RECEIPTQTY) as RECEIPTQTY ,round(sum(RECEIPTVALUE)/100000,2) as RECEIPTVALUE from (
select ms.schemecode tenderno,mi.itemcode,itemname,MI.itemid
,MI.strength1,MI.unit,MI.unitcount,mt.ITEMTYPENAME,edl.edl as EDLCAT

,op.ponoid,op.pono pono,to_char( op.soissuedate,'dd/mm/yyyy') podate,s.suppliername,
                            getfinalRateContract1(b.contractitemid,OP.soissuedate) finalrate,AbsQty as OrderedQty,nvl(ReceiptQty,0) ReceiptQty,
                            round((case when nvl(absqty,0)=0 then 0 else (nvl(receiptqty,0)/nvl(absqty,0))*100 end),0) supplypercent,

                            (nvl(absqty,0)* getfinalRateContract1(b.contractitemid,OP.soissuedate)) orderedvalue,
                            (nvl(ReceiptQty,0) * getfinalRateContract1(b.contractitemid,OP.soissuedate)) ReceiptValue 
                            ,EXTRACT(YEAR FROM op.soissuedate) AS Finyear
                            ,case when  nvl(MI.isedl2021,'N')='Y' then 'EDL' else 'NON EDL' end  as EDLType
                            from masItems MI
                                              inner join soordereditems oi on (oi.itemid = mi.itemid) 
                                              inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' ))
                                              inner join aoccontractitems b on (b.contractitemid = oi.contractitemid and b.itemid = oi.itemid)
                                              inner join masSuppliers S on (S.SupplierID = OP.SupplierID) 
                                              inner join masschemes MS on (MS.schemeid = OP.schemeid)
                                              inner join masitemcategories ic on ic.categoryid = mi.categoryid
                                              inner join masitemmaincategory mc on mc.MCID=ic.MCID
                                              inner join masitemtypes mt on mt.ITEMTYPEID=MI.ITEMTYPEID
                                              inner join masedl edl on edl.EDLCAT=MI.EDLCAT


                                              left outer join  
                                             (
                                                select distinct i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as receiptqty                  
                                                from tbreceipts t 
                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid) 
                                                where T.Status = 'C' and  T.receipttype = 'NO' and t.receiptid not in (select tr.receiptid
                                                                           from tbindents t  
                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                                                           where t.status = 'C' and t.issuetype in ('RS') )
                                                and tb.notindpdmis is null
                                                group by I.ItemID, T.PoNoID                       
                                              ) r on (r.itemid =oi.itemid and r.ponoid =op.ponoid) 
                                              where 1=1 "+ whmcid + @" and op.soissuedate between (select STARTDATE from masaccyearsettings where 1=1 "+ whyearid + @") and (select ENDDATE from masaccyearsettings where 1=1 "+ whyearid + @")
                                              "+ whisedl + @"

                                              ) group by  itemid, ITEMCODE, ITEMNAME, STRENGTH1, UNIT, EDLType ,EDLCAT,ITEMTYPENAME
                                              
                                              )x
                                              left outer join 
                                              (
                                              select itemid ,max(RCENDDT) as RCENDDate,count(Distinct SupplierID) as cntSup from v_rcvalid
                                                group by itemid
                                              )rc on rc.itemid=x.itemid
                                              left outer join
                                             (
                                             select ts.ITEMID, ACTION, ACTIONCODE,COV_A_OPDATE, COV_B_OPDATE, PRICEBIDDATE, SCHEMEID,COVA_BIDS, COVA_BIDB 
                                             from v_tenderstatusallnew ts
                                             inner join masitems m on m.itemid=ts.itemid
                                                inner join masitemcategories c on c.categoryid = m.categoryid
                                                inner join masitemmaincategory mc on mc.mcid = c.mcid
                                             where 1=1 "+ whmcid + @"
                                             ) ts on ts.itemid = x.itemid

                                             left outer join 
                                                    (                                                    
                                                    select itemid, sum(READYFORISSUE) as READYFORISSUE,sum(PENDING) as  PENDING from (
                                                    select mi.itemid, t.warehouseid,                                                    
                                                     nvl((case when tbr.qastatus ='1' then (nvl(tbr.absrqty,0) - nvl(tbr.issueqty,0)) else (case when mi.Qctest ='N' and tbr.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(tbr.absrqty,0) - nvl(tbr.issueqty,0) ) end  end ) end ),0) ReadyForIssue,    
                                                                        nvl(case when  mi.qctest='N' then 0 else (case when tbr.qastatus = 0 or tbr.qastatus = 3 then (nvl(tbr.absrqty,0)- nvl(tbr.issueqty,0)) end) end,0)  Pending    
                                                    from tbreceiptbatches tbr
                                                    inner join tbreceiptitems tbi on tbi.receiptitemid=tbr.receiptitemid
                                                    inner join tbreceipts t on t.receiptid=tbi.receiptid
                                                    inner join masitems mi on mi.itemid=tbi.itemid
                                                    inner join masitemcategories c on c.categoryid = mi.categoryid
                                                    inner join masitemmaincategory mc on mc.mcid = c.mcid
                                                    where  T.Status = 'C'  "+ whmcid + @"
                                                    and mi.isfreez_itpr is null
                                                    "+ whisedl + @"
                                                    And (tbr.ExpDate >= SysDate or nvl(tbr.ExpDate,SysDate) >= SysDate) and (tbr.Whissueblock = 0 or tbr.Whissueblock is null)
                                                    and (nvl(ABSRQTY,0)-nvl(ISSUEQTY,0))>0 
                                                    ) group by itemid                                                    
                                                    )whs on  whs.itemid=x.itemid  
                                                    left outer join
                                                    (
                                                    select i.itemid,sum(o.issueqty) as transferQTY
                                                    from stktransfers t
                                                    inner join stktransferitems i on i.transferid = t.transferid
                                                    inner join tbindents ti on ti.transferid = t.transferid
                                                    inner join tbindentitems tbi on tbi.indentid = ti.indentid and tbi.itemid = i.itemid
                                                    inner join tboutwards o on o.indentitemid = tbi.indentitemid
                                                    where t.status = 'C' 
                                                    and t.transferid in (select transferid from tbindents where status = 'C' and transferid is not null)
                                                    and t.transferid not in (select transferid from tbreceipts where status = 'C' and transferid is not null)
                                                    and t.transferdate between '01-APR-25' and sysdate 
                                                    group by i.itemid
                                                    ) IWHPipe on  IWHPipe.itemid=x.itemid 
                                                     left outer join 
                                                                     (
                                                    select  itemid,sum(pipelineQTY) newpiple 
                                                    from (select  soi.warehouseid, m.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
                                                    receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
                                                    case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
                                                    else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
                                                    else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end as pipelineQTY
                                                    from   soOrderPlaced OP  
                                                    inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
                                                    inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
                                                    inner join masitems m on m.itemid = oi.itemid
                                                    left outer join 
                                                    (
                                                    select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty, tr.warehouseid from tbreceipts tr 
                                                    inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
                                                    where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
                                                    group by tr.ponoid,tri.itemid,tr.warehouseid
                                                    ) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid and rec.warehouseid=soi.warehouseid
                                                     where op.status  in ('C','O') --and m.categoryid in (52,53,54,55) --and m.itemcode = 'D395'
                                                     group by soi.warehouseid, m.itemcode,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
                                                     op.soissuedate,op.extendeddate ,receiptdelayexception  
                                                     having (case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
                                                    else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
                                                    else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end) >0) group by itemid 
                                                     ) whpip on whpip.itemid=x.itemid  

                                              order by ORDEREDVALUE desc


)
ORDER BY ORDER_VALUE DESC
); ";

            var result = await _context.ABCanalysisSummaryDetailDbSet
                .FromSqlRaw(qry)
                .ToListAsync();

            return result;
        }


        [HttpGet("ABCanalysisWithRCvalid")]
        public async Task<ActionResult<IEnumerable<ABCanalysisWithRCvalidDTO>>> ABCanalysisWithRCvalid(
   string yearid, string mcid, string edlstatus)
        {
            string whyearid = "";
            string whmcid = "";
            string whisedl = "";

            // Apply filters
            if (yearid != "0")
                whyearid = " and accyrsetid = " + yearid;

            if (mcid != "0")
                whmcid = " and mc.mcid = " + mcid;

            if (edlstatus != "0")
                whisedl = " and ( CASE WHEN NVL(mi.isedl2021,'N')='Y' THEN 'EDL' ELSE 'NON EDL' END) = '"+ edlstatus + "'";

            string qry = $@"  SELECT 
    ABC_CATEGORY,
    COUNT(ITEM_ID) AS NoOfItems,
    ROUND(SUM(ORDER_VALUE)/100,2) AS ORDER_VALUE,
    SUM(RCValid) AS RCValid,
    SUM(RCNotValid) AS RCNotValid,
    SUM(Pricecnt) AS Pricecnt,
    SUM(Evalutioncnt) AS Evalutioncnt,
    SUM(LiveCnt) AS LiveCnt,
    SUM(Rentendercn) AS Rentendercn
FROM (
    SELECT 
        ITEM_ID,
        ITEMCODE,
        DRUG_NAME,
        STRENGTH1,
        UNIT,
        ITEMTYPENAME,
        EDLCAT,
        CASE WHEN RCValid = 'Valid' THEN 1 ELSE 0 END AS RCValid,
        CASE WHEN RCValid = 'Not Valid' THEN 1 ELSE 0 END AS RCNotValid,
        NVL(Pricecnt,0) AS Pricecnt,
        NVL(Evalutioncnt,0) AS Evalutioncnt,
        NVL(LiveCnt,0) AS LiveCnt,
        NVL(Rentendercn,0) AS Rentendercn,
        ORDER_VALUE,
        SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) AS CUMULATIVE_VALUE,
        ROUND(
            SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) / 
            SUM(ORDER_VALUE) OVER () * 100,2
        ) AS CUMULATIVE_PERCENT,
        CASE
            WHEN ROUND(SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) / SUM(ORDER_VALUE) OVER () * 100, 2) <= 70 THEN 'A'
            WHEN ROUND(SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) / SUM(ORDER_VALUE) OVER () * 100, 2) <= 90 THEN 'B'
            ELSE 'C'
        END AS ABC_CATEGORY
    FROM (
        SELECT 
            x.ITEMID AS ITEM_ID,
            x.ITEMCODE,
            x.ITEMNAME AS DRUG_NAME,
            x.STRENGTH1,
            x.EDLCAT,
            x.ITEMTYPENAME,
            x.UNIT,
            x.EDLTYPE,
            x.ORDEREDVALUE AS ORDER_VALUE,
            CASE WHEN rc.RCENDDate IS NULL THEN 'Not Valid' ELSE 'Valid' END AS RCValid,
            -- Tender status counts
            CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Price Opened in' THEN 1 ELSE 0 END AS Pricecnt,
            CASE WHEN rc.RCENDDate IS NULL AND (ts.ACTIONCODE = 'Cover-A in' OR ts.ACTIONCODE = 'Claim Objection in') THEN 1 ELSE 0 END AS Evalutioncnt,
            CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Live in' THEN 1 ELSE 0 END AS LiveCnt,
            CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'To be Retender' THEN 1 ELSE 0 END AS Rentendercn
        FROM (
            ---- ITEM + ORDER VALUE (with Receipt Join) ----
            SELECT 
                mi.itemid, 
                mi.itemcode, 
                mi.itemname, 
                mi.strength1, 
                mi.unit, 
                mt.ITEMTYPENAME, 
                edl.edl AS EDLCAT,
                CASE WHEN NVL(mi.isedl2021,'N')='Y' THEN 'EDL' ELSE 'NON EDL' END AS EDLType,
                SUM(NVL(oi.absqty,0)) AS OrderedQty,
                ROUND(SUM(NVL(oi.absqty,0) * getfinalRateContract1(b.contractitemid,op.soissuedate)) / 100000,2) AS OrderedValue
            FROM masItems mi
                 INNER JOIN soordereditems oi ON oi.itemid = mi.itemid
                 INNER JOIN soorderplaced op ON op.ponoid = oi.ponoid AND op.status NOT IN ('OC','WA1','I')
                 INNER JOIN aoccontractitems b ON b.contractitemid = oi.contractitemid AND b.itemid = oi.itemid
                 INNER JOIN masSuppliers s ON s.SupplierID = op.SupplierID
                 INNER JOIN masschemes ms ON ms.schemeid = op.schemeid
                 INNER JOIN masitemcategories ic ON ic.categoryid = mi.categoryid
                 INNER JOIN masitemmaincategory mc ON mc.MCID=ic.MCID
                 INNER JOIN masitemtypes mt ON mt.ITEMTYPEID=mi.ITEMTYPEID
                 INNER JOIN masedl edl ON edl.EDLCAT=mi.EDLCAT
                 LEFT OUTER JOIN (
                     SELECT DISTINCT i.itemid, t.ponoid, NVL(SUM(tb.absrqty),0) AS receiptqty
                     FROM tbreceipts t 
                          INNER JOIN tbreceiptitems i ON (i.receiptid = t.receiptid) 
                          INNER JOIN tbreceiptbatches tb ON (i.receiptitemid = tb.receiptitemid) 
                     WHERE t.Status = 'C' 
                       AND t.receipttype = 'NO' 
                       AND t.receiptid NOT IN (
                            SELECT tr.receiptid
                            FROM tbindents t  
                                 INNER JOIN tbindentitems i ON (i.indentid = t.indentid) 
                                 INNER JOIN tboutwards o ON (o.indentitemid = i.indentitemid) 
                                 INNER JOIN tbreceiptbatches tb ON (tb.inwno = o.inwno)
                                 INNER JOIN tbreceiptitems ti ON ti.receiptitemid = tb.receiptitemid
                                 INNER JOIN tbreceipts tr ON tr.receiptid = ti.receiptid
                            WHERE t.status = 'C' 
                              AND t.issuetype IN ('RS')
                       )
                       AND tb.notindpdmis IS NULL
                     GROUP BY i.itemid, t.ponoid
                 ) r ON r.itemid = oi.itemid AND r.ponoid = op.ponoid
            WHERE 1=1  
             "+ whmcid + @" 
              AND op.soissuedate BETWEEN 
                    (SELECT STARTDATE FROM masaccyearsettings WHERE 1=1  "+ whyearid + @") 
                    AND 
                    (SELECT ENDDATE FROM masaccyearsettings WHERE 1=1 "+ whyearid + @")
              AND NVL(mi.isedl2021,'N')='Y'
        "+ whisedl + @"
            GROUP BY mi.itemid, mi.itemcode, mi.itemname, mi.strength1, mi.unit, mt.ITEMTYPENAME, edl.edl, mi.isedl2021
        ) x
        LEFT JOIN (
            SELECT itemid, MAX(RCENDDT) AS RCENDDate 
            FROM v_rcvalid
            GROUP BY itemid
        ) rc ON rc.itemid = x.itemid
        LEFT JOIN (
            SELECT ts.ITEMID, ts.ACTIONCODE
            FROM v_tenderstatusallnew ts
                 INNER JOIN masitems m ON m.itemid=ts.itemid
                 INNER JOIN masitemcategories c ON c.categoryid = m.categoryid
                 INNER JOIN masitemmaincategory mc ON mc.mcid = c.mcid
            WHERE 1=1 "+ whmcid + @"
        ) ts ON ts.itemid = x.itemid
    )
    ORDER BY ORDER_VALUE DESC
) 
GROUP BY ABC_CATEGORY
ORDER BY ABC_CATEGORY;
 ";

            // Log the query for debugging
            System.Diagnostics.Debug.WriteLine(qry);

            var result = await _context.ABCanalysisWithRCvalidDbSet
                .FromSqlRaw(qry)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }


    }



}
