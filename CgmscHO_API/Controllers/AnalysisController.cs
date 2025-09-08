using CgmscHO_API.AnalysisDTO;
using CgmscHO_API.DTO;
using CgmscHO_API.HODTO;
using CgmscHO_API.LogAuditDTO;
using CgmscHO_API.MasterDTO;
using CgmscHO_API.Models;
using CgmscHO_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
                whisedl = " and nvl(MI.isedl2021,'N')='" + isedl + "' ";
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
                                              where 1=1 " + whmcid + @" and op.soissuedate between (select STARTDATE from masaccyearsettings where 1=1 " + whyearid + @") and (select ENDDATE from masaccyearsettings where 1=1 " + whyearid + @")
                                              " + whisedl + @"

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
                                              where 1=1 " + whmcid + @" and op.soissuedate between (select STARTDATE from masaccyearsettings where 1=1 " + whyearid + @") and (select ENDDATE from masaccyearsettings where 1=1 " + whyearid + @")
                                              " + whisedl + @"

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
                                             where 1=1 " + whmcid + @"
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
                                                    where  T.Status = 'C'  " + whmcid + @"
                                                    and mi.isfreez_itpr is null
                                                    " + whisedl + @"
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
   string yearid, string mcid, string isEDL)
        {
            string whyearid = "";
            string whmcid = "";
            string whisedl = "";

            // Apply filters
            if (yearid != "0")
                whyearid = " and accyrsetid = " + yearid;

            if (mcid != "0")
                whmcid = " and mc.mcid = " + mcid;

            if (isEDL != "0")
                whisedl = " AND NVL(mi.isedl2021,'N')= '" + isEDL + "'";

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
             " + whmcid + @" 
              AND op.soissuedate BETWEEN 
                    (SELECT STARTDATE FROM masaccyearsettings WHERE 1=1  " + whyearid + @") 
                    AND 
                    (SELECT ENDDATE FROM masaccyearsettings WHERE 1=1 " + whyearid + @")
              " + whisedl + @"
      
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
            WHERE 1=1 " + whmcid + @"
        ) ts ON ts.itemid = x.itemid
    )
    ORDER BY ORDER_VALUE DESC
) 
GROUP BY ABC_CATEGORY
ORDER BY ABC_CATEGORY;
 ";

            // Log the query for debugging
            //  System.Diagnostics.Debug.WriteLine(qry);


            //            string xyz = @" SELECT
            //    ABC_CATEGORY,
            //    COUNT(ITEM_ID) AS NoOfItems,
            //    ROUND(SUM(ORDER_VALUE) / 100, 2) AS ORDER_VALUE,
            //    SUM(RCValid) AS RCValid,
            //    SUM(RCNotValid) AS RCNotValid,
            //    SUM(Pricecnt) AS Pricecnt,
            //    SUM(Evalutioncnt) AS Evalutioncnt,
            //    SUM(LiveCnt) AS LiveCnt,
            //    SUM(Rentendercn) AS Rentendercn
            //FROM(
            //    SELECT
            //        ITEM_ID,
            //        ITEMCODE,
            //        DRUG_NAME,
            //        STRENGTH1,
            //        UNIT,
            //        ITEMTYPENAME,
            //        EDLCAT,
            //        CASE WHEN RCValid = 'Valid' THEN 1 ELSE 0 END AS RCValid,
            //        CASE WHEN RCValid = 'Not Valid' THEN 1 ELSE 0 END AS RCNotValid,
            //        NVL(Pricecnt, 0) AS Pricecnt,
            //        NVL(Evalutioncnt, 0) AS Evalutioncnt,
            //        NVL(LiveCnt, 0) AS LiveCnt,
            //        NVL(Rentendercn, 0) AS Rentendercn,
            //        ORDER_VALUE,
            //        SUM(ORDER_VALUE) OVER(ORDER BY ORDER_VALUE DESC) AS CUMULATIVE_VALUE,
            //        ROUND(
            //            SUM(ORDER_VALUE) OVER(ORDER BY ORDER_VALUE DESC) /
            //            SUM(ORDER_VALUE) OVER() * 100, 2
            //        ) AS CUMULATIVE_PERCENT,
            //        CASE
            //            WHEN ROUND(SUM(ORDER_VALUE) OVER(ORDER BY ORDER_VALUE DESC) / SUM(ORDER_VALUE) OVER() * 100, 2) <= 70 THEN 'A'
            //            WHEN ROUND(SUM(ORDER_VALUE) OVER(ORDER BY ORDER_VALUE DESC) / SUM(ORDER_VALUE) OVER() * 100, 2) <= 90 THEN 'B'
            //            ELSE 'C'
            //        END AS ABC_CATEGORY
            //    FROM(
            //        SELECT
            //            x.ITEMID AS ITEM_ID,
            //            x.ITEMCODE,
            //            x.ITEMNAME AS DRUG_NAME,
            //            x.STRENGTH1,
            //            x.EDLCAT,
            //            x.ITEMTYPENAME,
            //            x.UNIT,
            //            x.EDLTYPE,
            //            x.ORDEREDVALUE AS ORDER_VALUE,
            //            CASE WHEN rc.RCENDDate IS NULL THEN 'Not Valid' ELSE 'Valid' END AS RCValid,
            //            --Tender status counts
            //            CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Price Opened in' THEN 1 ELSE 0 END AS Pricecnt,
            //            CASE WHEN rc.RCENDDate IS NULL AND(ts.ACTIONCODE = 'Cover-A in' OR ts.ACTIONCODE = 'Claim Objection in') THEN 1 ELSE 0 END AS Evalutioncnt,
            //            CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Live in' THEN 1 ELSE 0 END AS LiveCnt,
            //            CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'To be Retender' THEN 1 ELSE 0 END AS Rentendercn
            //        FROM(
            //            ----ITEM + ORDER VALUE(with Receipt Join)----
            //            SELECT
            //                mi.itemid,
            //                mi.itemcode,
            //                mi.itemname,
            //                mi.strength1,
            //                mi.unit,
            //                mt.ITEMTYPENAME,
            //                edl.edl AS EDLCAT,
            //                CASE WHEN NVL(mi.isedl2021, 'N') = 'Y' THEN 'EDL' ELSE 'NON EDL' END AS EDLType,
            //                SUM(NVL(oi.absqty, 0)) AS OrderedQty,
            //                ROUND(SUM(NVL(oi.absqty, 0) * getfinalRateContract1(b.contractitemid, op.soissuedate)) / 100000, 2) AS OrderedValue
            //            FROM masItems mi
            //                 INNER JOIN soordereditems oi ON oi.itemid = mi.itemid
            //                 INNER JOIN soorderplaced op ON op.ponoid = oi.ponoid AND op.status NOT IN('OC', 'WA1', 'I')
            //                 INNER JOIN aoccontractitems b ON b.contractitemid = oi.contractitemid AND b.itemid = oi.itemid
            //                 INNER JOIN masSuppliers s ON s.SupplierID = op.SupplierID
            //                 INNER JOIN masschemes ms ON ms.schemeid = op.schemeid
            //                 INNER JOIN masitemcategories ic ON ic.categoryid = mi.categoryid
            //                 INNER JOIN masitemmaincategory mc ON mc.MCID = ic.MCID
            //                 INNER JOIN masitemtypes mt ON mt.ITEMTYPEID = mi.ITEMTYPEID
            //                 INNER JOIN masedl edl ON edl.EDLCAT = mi.EDLCAT
            //                 LEFT OUTER JOIN(
            //                     SELECT DISTINCT i.itemid, t.ponoid, NVL(SUM(tb.absrqty), 0) AS receiptqty
            //                     FROM tbreceipts t
            //                          INNER JOIN tbreceiptitems i ON(i.receiptid = t.receiptid)
            //                          INNER JOIN tbreceiptbatches tb ON(i.receiptitemid = tb.receiptitemid)
            //                     WHERE t.Status = 'C'
            //                       AND t.receipttype = 'NO'
            //                       AND t.receiptid NOT IN(
            //                            SELECT tr.receiptid
            //                            FROM tbindents t
            //                                 INNER JOIN tbindentitems i ON(i.indentid = t.indentid)
            //                                 INNER JOIN tboutwards o ON(o.indentitemid = i.indentitemid)
            //                                 INNER JOIN tbreceiptbatches tb ON(tb.inwno = o.inwno)
            //                                 INNER JOIN tbreceiptitems ti ON ti.receiptitemid = tb.receiptitemid
            //                                 INNER JOIN tbreceipts tr ON tr.receiptid = ti.receiptid
            //                            WHERE t.status = 'C'
            //                              AND t.issuetype IN('RS')
            //                       )
            //                       AND tb.notindpdmis IS NULL
            //                     GROUP BY i.itemid, t.ponoid
            //                 ) r ON r.itemid = oi.itemid AND r.ponoid = op.ponoid
            //            WHERE 1 = 1
            //               "+ whmcid + @"
            //              AND op.soissuedate BETWEEN
            //                    (SELECT STARTDATE FROM masaccyearsettings WHERE 1=1  "+ whyearid + @")
            //                    AND
            //                    (SELECT ENDDATE FROM masaccyearsettings WHERE 1=1  "+ whyearid + @")
            //              AND NVL(mi.isedl2021, 'N') = 'Y'
            //               and(CASE WHEN NVL(mi.isedl2021, 'N') = 'Y' THEN 'EDL' ELSE 'NON EDL' END) = 'EDL'
            //            GROUP BY mi.itemid, mi.itemcode, mi.itemname, mi.strength1, mi.unit, mt.ITEMTYPENAME, edl.edl, mi.isedl2021
            //        ) x
            //        LEFT JOIN(
            //            SELECT itemid, MAX(RCENDDT) AS RCENDDate
            //            FROM v_rcvalid
            //            GROUP BY itemid
            //        ) rc ON rc.itemid = x.itemid
            //        LEFT JOIN(
            //            SELECT ts.ITEMID, ts.ACTIONCODE
            //            FROM v_tenderstatusallnew ts
            //                 INNER JOIN masitems m ON m.itemid = ts.itemid
            //                 INNER JOIN masitemcategories c ON c.categoryid = m.categoryid
            //                 INNER JOIN masitemmaincategory mc ON mc.mcid = c.mcid
            //            WHERE " + whmcid + @"
            //        ) ts ON ts.itemid = x.itemid
            //    )
            //    ORDER BY ORDER_VALUE DESC
            //)
            //GROUP BY ABC_CATEGORY
            //ORDER BY ABC_CATEGORY  ";






            var result = await _context.ABCanalysisWithRCvalidDbSet
                .FromSqlRaw(qry)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }




        [HttpGet("ABC_VED_SDE_matrix")]
        public async Task<ActionResult<IEnumerable<ABC_VED_SDE_matrixDTO>>> ABC_VED_SDE_matrix(
   string yearid, string mcid, string isEDL)
        {
            string whyearid = "";
            string whmcid = "";
            string whisedl = "";


            // Apply filters
            if (yearid != "0")
                whyearid = " and accyrsetid = " + yearid;

            if (mcid != "0")
                whmcid = " and mc.mcid = " + mcid;

            if (isEDL != "0")
            {
                whisedl = " AND NVL(mi.isedl2021,'N')= '" + isEDL + "'";
            }


            string qry = $@"  select ABC_VED_SDE_CATEGORY,count(distinct  ITEM_ID) as nos, sum(RCValid) as rcvalid, sum(RCNotValid) as RCNotValid,sum(Pricecnt) as RCNotValidPricecnt, sum(Evalutioncnt) as RCNotValidEvalutioncnt
,sum(LiveCnt) as  RCnotValidLiveCnt,sum(Rentendercn) as  RCnotValidRentendercn 
from (
SELECT
    ITEM_ID,
    ITEMCODE,
    DRUG_NAME,
    STRENGTH1,
    UNIT,
    ITEMTYPENAME,
    EDLCAT,
    RCValid,
    RCNotValid,
    Pricecnt,
    Evalutioncnt,
    LiveCnt,
    Rentendercn,
    abc_category,
    vedcat,
    sde_class,

    /* NEW: Combined priority bucket */
    CASE
      /* --- Category I (Highest) --- */
      WHEN (
            UPPER(vedcat) = 'V' AND (UPPER(abc_category) IN ('A','B') OR UPPER(sde_class) IN ('S','D'))
          )
           OR UPPER(sde_class) = 'S'
           OR (UPPER(abc_category) = 'A' AND UPPER(sde_class) IN ('S','D'))
      THEN 'I'

      /* --- Category II (Medium) --- */
      WHEN  UPPER(vedcat) = 'E'
         OR UPPER(sde_class) = 'D'
         OR UPPER(abc_category) = 'B'
         OR (UPPER(vedcat) = 'V' AND UPPER(abc_category) = 'C')
      THEN 'II'

      /* --- Category III (Lowest) --- */
      ELSE 'III'
    END AS ABC_VED_SDE_CATEGORY

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
      END AS ABC_CATEGORY,
      vedcat,
      sde_class
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
          /* Tender status counts */
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Price Opened in' THEN 1 ELSE 0 END AS Pricecnt,
          CASE WHEN rc.RCENDDate IS NULL AND (ts.ACTIONCODE = 'Cover-A in' OR ts.ACTIONCODE = 'Claim Objection in') THEN 1 ELSE 0 END AS Evalutioncnt,
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Live in' THEN 1 ELSE 0 END AS LiveCnt,
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'To be Retender' THEN 1 ELSE 0 END AS Rentendercn,
          vedcat,
          sd.sde_class
      FROM (
          /* ---- ITEM + ORDER VALUE (with Receipt Join) ---- */
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
              ROUND(SUM(NVL(oi.absqty,0) * getfinalRateContract1(b.contractitemid,op.soissuedate)) / 100000,2) AS OrderedValue,
              mi.vedcat
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
          WHERE 1=1 " + whmcid + @" 
            AND op.soissuedate BETWEEN 
                  (SELECT STARTDATE FROM masaccyearsettings WHERE 1=1 " + whyearid + @") 
                  AND 
                  (SELECT ENDDATE FROM masaccyearsettings WHERE 1=1 " + whyearid + @")
            " + whisedl + @"
           
          GROUP BY mi.itemid, mi.itemcode, mi.itemname, mi.strength1, mi.unit, mt.ITEMTYPENAME, edl.edl, mi.isedl2021, mi.vedcat
      ) x
      LEFT JOIN (
          SELECT itemid, MAX(RCENDDT) AS RCENDDate 
          FROM v_rcvalid
          GROUP BY itemid
      ) rc ON rc.itemid = x.itemid
      LEFT JOIN (
          SELECT ts.ITEMID, ts.ACTIONCODE
          FROM v_tenderstatusallnew ts
               INNER JOIN masitems m ON m.itemid = ts.itemid
               INNER JOIN masitemcategories c ON c.categoryid = m.categoryid
               INNER JOIN masitemmaincategory mc ON mc.mcid = c.mcid
          WHERE 1=1  " + whmcid + @"           
      ) ts ON ts.itemid = x.itemid

      /* === join your SDE per-item classification === */
      LEFT JOIN (
        /* Your SDE subquery (agg + final) as previously built; must output (item_id, sde_class) */
        WITH base AS (
          SELECT 
            mi.itemid                  AS item_id,
            s.supplierid               AS supplier_id,
            op.soissuedate             AS po_date,
            r.receiptdate              AS receipt_date,
            (r.receiptdate - op.soissuedate) AS lead_time,
            CASE WHEN mi.nablreq = 'Y' THEN 90
                 WHEN op.isimported = 'Y' THEN 75
                 ELSE 60 END AS max_days
          FROM masItems mi
          JOIN soordereditems oi  ON oi.itemid = mi.itemid
          JOIN soorderplaced op   ON op.ponoid = oi.ponoid AND op.status NOT IN ('OC','WA1','I')
          JOIN aoccontractitems b ON b.contractitemid = oi.contractitemid AND b.itemid = oi.itemid
          JOIN masSuppliers s     ON s.SupplierID = op.SupplierID
          JOIN masitemcategories ic   ON ic.categoryid = mi.categoryid
          JOIN masitemmaincategory mc ON mc.MCID = ic.MCID
          LEFT JOIN (
            SELECT i.itemid, t.ponoid, MIN(t.receiptdate) AS receiptdate, SUM(tb.absrqty) AS receiptqty
            FROM tbreceipts t
            JOIN tbreceiptitems i   ON i.receiptid = t.receiptid
            JOIN tbreceiptbatches tb ON tb.receiptitemid = i.receiptitemid
            WHERE t.status = 'C'
              AND t.receipttype = 'NO'
              AND tb.notindpdmis IS NULL
            GROUP BY i.itemid, t.ponoid
          ) r ON r.itemid = oi.itemid AND r.ponoid = op.ponoid
          WHERE 1=1 " + whmcid + @"
           " + whisedl + @"
            AND r.receiptdate IS NOT NULL
            AND op.soissuedate BETWEEN ADD_MONTHS(TRUNC(SYSDATE), -36) AND SYSDATE
        ),
        agg AS (
          SELECT
            item_id,
            COUNT(*) AS total_orders,
            AVG(lead_time) AS avg_lead,
            STDDEV(lead_time) AS std_dev,
            CASE WHEN AVG(lead_time) = 0 THEN 0 ELSE STDDEV(lead_time) / AVG(lead_time) END AS cv,
            PERCENTILE_CONT(0.75) WITHIN GROUP (ORDER BY lead_time) AS p75_lead,
            PERCENTILE_CONT(0.90) WITHIN GROUP (ORDER BY lead_time) AS p90_lead,
            100 * AVG(CASE WHEN lead_time > max_days THEN 1 ELSE 0 END) AS late_rate,
            MAX(max_days) AS max_days,
            COUNT(DISTINCT supplier_id) AS supplier_count
          FROM base
          GROUP BY item_id
        )
        SELECT
          a.item_id,
          CASE
            WHEN ( (CASE WHEN a.p90_lead > a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 40 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.supplier_count <= 1 AND a.p75_lead >= 0.9 * a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.cv >= 0.6 THEN 1 ELSE 0 END) ) >= 2
              THEN 'S'
            WHEN (
                   (CASE WHEN a.p90_lead > a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 40 THEN 1 ELSE 0 END)
                 ) >= 1
                 OR (
                   (CASE WHEN a.p75_lead > 0.8 * a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 15 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.supplier_count = 2 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.cv BETWEEN 0.3 AND 0.6 THEN 1 ELSE 0 END)
                 ) >= 2
              THEN 'D'
            ELSE 'E'
          END AS sde_class
        FROM agg a
      ) sd
      ON sd.ITEM_ID = x.itemid
  )
)
) group by ABC_VED_SDE_CATEGORY
order by abc_ved_sde_category
 ";

            var result = await _context.ABC_VED_SDE_matrixDbSet
                .FromSqlRaw(qry)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }


        [HttpGet("ABC_VED_SDE_matrixDetail")]
        public async Task<ActionResult<IEnumerable<ABC_VED_SDE_matrixDetailDTO>>> ABC_VED_SDE_matrixDetail(
  string yearid, string mcid, string isEDL)
        {
            string whyearid = "";
            string whmcid = "";
            string whisedl = "";


            // Apply filters
            if (yearid != "0")
                whyearid = " and accyrsetid = " + yearid;

            if (mcid != "0")
                whmcid = " and mc.mcid not in (3,4) and mc.mcid = " + mcid;

            if (isEDL != "0")
            {
                whisedl = " AND NVL(mi.isedl2021,'N')= '" + isEDL + "'";
            }


            string qry = $@"  SELECT " + yearid + @" as accyrsetid,
    ITEM_ID,
    ITEMCODE,
    DRUG_NAME,
    STRENGTH1,
    UNIT,
    ITEMTYPENAME,
    EDLCAT,
    EDLTYPE,
     mcid,
    RCValid,
    RCNotValid,
    Pricecnt,
    Evalutioncnt,
    LiveCnt,
    Rentendercn,
    abc_category,
    vedcat,
    sde_class,

    /* NEW: Combined priority bucket */
    CASE
      /* --- Category I (Highest) --- */
      WHEN (
            UPPER(vedcat) = 'V' AND (UPPER(abc_category) IN ('A','B') OR UPPER(sde_class) IN ('S','D'))
          )
           OR UPPER(sde_class) = 'S'
           OR (UPPER(abc_category) = 'A' AND UPPER(sde_class) IN ('S','D'))
      THEN 'I'

      /* --- Category II (Medium) --- */
      WHEN  UPPER(vedcat) = 'E'
         OR UPPER(sde_class) = 'D'
         OR UPPER(abc_category) = 'B'
         OR (UPPER(vedcat) = 'V' AND UPPER(abc_category) = 'C')
      THEN 'II'

      /* --- Category III (Lowest) --- */
      ELSE 'III'
    END AS ABC_VED_SDE_CATEGORY

FROM (
  SELECT 
      ITEM_ID,
      ITEMCODE,
      DRUG_NAME,
      STRENGTH1,
      UNIT,
      ITEMTYPENAME,
      EDLCAT,EDLTYPE,
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
      END AS ABC_CATEGORY,
      vedcat,
      sde_class,mcid
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
          /* Tender status counts */
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Price Opened in' THEN 1 ELSE 0 END AS Pricecnt,
          CASE WHEN rc.RCENDDate IS NULL AND (ts.ACTIONCODE = 'Cover-A in' OR ts.ACTIONCODE = 'Claim Objection in') THEN 1 ELSE 0 END AS Evalutioncnt,
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Live in' THEN 1 ELSE 0 END AS LiveCnt,
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'To be Retender' THEN 1 ELSE 0 END AS Rentendercn,
          vedcat,
          sd.sde_class,mcid
      FROM (
          /* ---- ITEM + ORDER VALUE (with Receipt Join) ---- */
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
              ROUND(SUM(NVL(oi.absqty,0) * getfinalRateContract1(b.contractitemid,op.soissuedate)) / 100000,2) AS OrderedValue,
              mi.vedcat,mc.mcid
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
          WHERE 1=1 " + whmcid + @" 
            AND op.soissuedate BETWEEN 
                  (SELECT STARTDATE FROM masaccyearsettings WHERE 1=1 " + whyearid + @") 
                  AND 
                  (SELECT ENDDATE FROM masaccyearsettings WHERE 1=1 " + whyearid + @")
           " + whisedl + @"
           
          GROUP BY mi.itemid, mi.itemcode, mi.itemname, mi.strength1, mi.unit, mt.ITEMTYPENAME, edl.edl, mi.isedl2021, mi.vedcat,mc.mcid
      ) x
      LEFT JOIN (
          SELECT itemid, MAX(RCENDDT) AS RCENDDate 
          FROM v_rcvalid
          GROUP BY itemid
      ) rc ON rc.itemid = x.itemid
      LEFT JOIN (
          SELECT ts.ITEMID, ts.ACTIONCODE
          FROM v_tenderstatusallnew ts
               INNER JOIN masitems m ON m.itemid = ts.itemid
               INNER JOIN masitemcategories c ON c.categoryid = m.categoryid
               INNER JOIN masitemmaincategory mc ON mc.mcid = c.mcid
          WHERE 1=1 " + whmcid + @"          
      ) ts ON ts.itemid = x.itemid

      /* === join your SDE per-item classification === */
      LEFT JOIN (
        /* Your SDE subquery (agg + final) as previously built; must output (item_id, sde_class) */
        WITH base AS (
          SELECT 
            mi.itemid                  AS item_id,
            s.supplierid               AS supplier_id,
            op.soissuedate             AS po_date,
            r.receiptdate              AS receipt_date,
            (r.receiptdate - op.soissuedate) AS lead_time,
            CASE WHEN mi.nablreq = 'Y' THEN 90
                 WHEN op.isimported = 'Y' THEN 75
                 ELSE 60 END AS max_days
          FROM masItems mi
          JOIN soordereditems oi  ON oi.itemid = mi.itemid
          JOIN soorderplaced op   ON op.ponoid = oi.ponoid AND op.status NOT IN ('OC','WA1','I')
          JOIN aoccontractitems b ON b.contractitemid = oi.contractitemid AND b.itemid = oi.itemid
          JOIN masSuppliers s     ON s.SupplierID = op.SupplierID
          JOIN masitemcategories ic   ON ic.categoryid = mi.categoryid
          JOIN masitemmaincategory mc ON mc.MCID = ic.MCID
          LEFT JOIN (
            SELECT i.itemid, t.ponoid, MIN(t.receiptdate) AS receiptdate, SUM(tb.absrqty) AS receiptqty
            FROM tbreceipts t
            JOIN tbreceiptitems i   ON i.receiptid = t.receiptid
            JOIN tbreceiptbatches tb ON tb.receiptitemid = i.receiptitemid
            WHERE t.status = 'C'
              AND t.receipttype = 'NO'
              AND tb.notindpdmis IS NULL
            GROUP BY i.itemid, t.ponoid
          ) r ON r.itemid = oi.itemid AND r.ponoid = op.ponoid
          WHERE 1=1 " + whmcid + @"
            " + whisedl + @"
            AND r.receiptdate IS NOT NULL
            AND op.soissuedate BETWEEN ADD_MONTHS(TRUNC(SYSDATE), -36) AND SYSDATE
        ),
        agg AS (
          SELECT
            item_id,
            COUNT(*) AS total_orders,
            AVG(lead_time) AS avg_lead,
            STDDEV(lead_time) AS std_dev,
            CASE WHEN AVG(lead_time) = 0 THEN 0 ELSE STDDEV(lead_time) / AVG(lead_time) END AS cv,
            PERCENTILE_CONT(0.75) WITHIN GROUP (ORDER BY lead_time) AS p75_lead,
            PERCENTILE_CONT(0.90) WITHIN GROUP (ORDER BY lead_time) AS p90_lead,
            100 * AVG(CASE WHEN lead_time > max_days THEN 1 ELSE 0 END) AS late_rate,
            MAX(max_days) AS max_days,
            COUNT(DISTINCT supplier_id) AS supplier_count
          FROM base
          GROUP BY item_id
        )
        SELECT
          a.item_id,
          CASE
            WHEN ( (CASE WHEN a.p90_lead > a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 40 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.supplier_count <= 1 AND a.p75_lead >= 0.9 * a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.cv >= 0.6 THEN 1 ELSE 0 END) ) >= 2
              THEN 'S'
            WHEN (
                   (CASE WHEN a.p90_lead > a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 40 THEN 1 ELSE 0 END)
                 ) >= 1
                 OR (
                   (CASE WHEN a.p75_lead > 0.8 * a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 15 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.supplier_count = 2 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.cv BETWEEN 0.3 AND 0.6 THEN 1 ELSE 0 END)
                 ) >= 2
              THEN 'D'
            ELSE 'E'
          END AS sde_class
        FROM agg a
      ) sd
      ON sd.ITEM_ID = x.itemid
  )
);
 ";

            var result = await _context.ABC_VED_SDE_matrixDetailDbSet
                .FromSqlRaw(qry)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }






        [HttpGet("ABC_VED_SDE_matrixWithStockOut")]
        public async Task<ActionResult<IEnumerable<ABC_VED_SDE_matrixWithStockOutDTO>>> ABC_VED_SDE_matrixWithStockOut(
   string yearid, string mcid, string isEDL, string catType)
        {
            string whyearid = "";
            string whmcid = "";
            string whisedl = "";
            string whcatType = "";


            // Apply filters
            if (yearid != "0")
                whyearid = " and accyrsetid = " + yearid;

            if (mcid != "0")
                whmcid = " and mc.mcid = " + mcid;

            if (isEDL != "0")
            {
                whisedl = " AND NVL(mi.isedl2021,'N')= '" + isEDL + "'";
            }


            if (catType != "0")
            {
                if (catType.ToUpper() == "MATRIX")
                {
                    whcatType = "  ABC_VED_SDE_CATEGORY ";
                }
                if (catType.ToUpper() == "ABC")
                {
                    whcatType = "  ABC_CATEGORY ";
                }
                if (catType.ToUpper() == "SDE")
                {
                    whcatType = "  SDE_CLASS ";
                }
                if (catType.ToUpper() == "VED")
                {
                    whcatType = "  VEDCAT ";
                }


            }


            string qry = $@"  select " + whcatType + @" as category ,count(distinct ITEM_ID) as cntItems ,sum(STOCKOUT) as STOCKOUT, sum(STOCKIN) as STOCKIN, sum(STOCKOUTPOPIPE) as STOCKOUTPOPIPE, sum(RCVALID) as RCVALID, sum(RCNOTVALID) as RCNOTVALID, sum(PRICECNT) as PRICECNT
, sum(EVALUTIONCNT) as EVALUTIONCNT, sum(LIVECNT) as LIVECNT, sum(RENTENDERCN) as RENTENDERCN  from (


SELECT
    ITEM_ID,
    ITEMCODE,
    DRUG_NAME,
    STRENGTH1,
    UNIT,
    ITEMTYPENAME,
    EDLTYPE,
    EDLCAT,
     MCATEGORY,
      mcid,
    ORDER_VALUE,
    READYWTOCK, UQCSTOCK, SUPPLIERPIPELINE ,transferQTY   
    ,case when (nvl(READYWTOCK,0)+nvl(UQCSTOCK,0))=0 then 1 else 0 end as StockOut
    ,case when (nvl(READYWTOCK,0)+nvl(UQCSTOCK,0))>0 then 1 else 0 end as StockIn
    , case when (nvl(READYWTOCK,0)+nvl(UQCSTOCK,0))=0  and  nvl(transferQTY,0) =0 and  nvl(SUPPLIERPIPELINE,0)>0 then 1    else 0 end  as StockOutPoPipe
     , case when (nvl(READYWTOCK,0)+nvl(UQCSTOCK,0))=0  and  nvl(transferQTY,0) =0 and  nvl(SUPPLIERPIPELINE,0)>0 then  nvl(SUPPLIERPIPELINE,0)    else 0 end  as StockOutPoQty
    ,RCValid,
    RCNotValid,
    Pricecnt,
    Evalutioncnt,
    LiveCnt,
    Rentendercn,
    abc_category,
    vedcat,
    sde_class,

    /* NEW: Combined priority bucket */
    CASE
      /* --- Category I (Highest) --- */
      WHEN (
            UPPER(vedcat) = 'V' AND (UPPER(abc_category) IN ('A','B') OR UPPER(sde_class) IN ('S','D'))
          )
           OR UPPER(sde_class) = 'S'
           OR (UPPER(abc_category) = 'A' AND UPPER(sde_class) IN ('S','D'))
      THEN 'I'

      /* --- Category II (Medium) --- */
      WHEN  UPPER(vedcat) = 'E'
         OR UPPER(sde_class) = 'D'
         OR UPPER(abc_category) = 'B'
         OR (UPPER(vedcat) = 'V' AND UPPER(abc_category) = 'C')
      THEN 'II'

      /* --- Category III (Lowest) --- */
      ELSE 'III'
    END AS ABC_VED_SDE_CATEGORY

FROM (
  SELECT 
      ITEM_ID,
      ITEMCODE,
      DRUG_NAME,
      STRENGTH1,
      UNIT,
      ITEMTYPENAME,
      EDLCAT,
      EDLTYPE,
      MCATEGORY,
      mcid,
      CASE WHEN RCValid = 'Valid' THEN 1 ELSE 0 END AS RCValid,
      CASE WHEN RCValid = 'Not Valid' THEN 1 ELSE 0 END AS RCNotValid,
      NVL(Pricecnt,0) AS Pricecnt,
      NVL(Evalutioncnt,0) AS Evalutioncnt,
      NVL(LiveCnt,0) AS LiveCnt,
      NVL(Rentendercn,0) AS Rentendercn,
      ORDER_VALUE,
      READYWTOCK, UQCSTOCK, transferQTY,SUPPLIERPIPELINE,
      SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) AS CUMULATIVE_VALUE,
      ROUND(
          SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) / 
          SUM(ORDER_VALUE) OVER () * 100,2
      ) AS CUMULATIVE_PERCENT,
      CASE
          WHEN ROUND(SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) / SUM(ORDER_VALUE) OVER () * 100, 2) <= 70 THEN 'A'
          WHEN ROUND(SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) / SUM(ORDER_VALUE) OVER () * 100, 2) <= 90 THEN 'B'
          ELSE 'C'
      END AS ABC_CATEGORY,
      vedcat,
      sde_class
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
          x.MCATEGORY,
           x.mcid,
          x.ORDEREDVALUE AS ORDER_VALUE,
          CASE WHEN rc.RCENDDate IS NULL THEN 'Not Valid' ELSE 'Valid' END AS RCValid,
          /* Tender status counts */
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Price Opened in' THEN 1 ELSE 0 END AS Pricecnt,
          CASE WHEN rc.RCENDDate IS NULL AND (ts.ACTIONCODE = 'Cover-A in' OR ts.ACTIONCODE = 'Claim Objection in') THEN 1 ELSE 0 END AS Evalutioncnt,
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Live in' THEN 1 ELSE 0 END AS LiveCnt,
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'To be Retender' THEN 1 ELSE 0 END AS Rentendercn,
          vedcat,
          sd.sde_class
          ,nvl(READYFORISSUE,0) ReadyWtock
          ,nvl(PENDING,0) as UQCStock
          ,nvl(newpiple,0) as SupplierPipeLine
          ,nvl(transferQTY,0) as transferQTY
      FROM (
          /* ---- ITEM + ORDER VALUE (with Receipt Join) ---- */
          SELECT 
              mi.itemid, 
              mi.itemcode, 
              mi.itemname, 
              mi.strength1, 
              mi.unit, 
              mt.ITEMTYPENAME, 
              edl.edl AS EDLCAT,
              mc.mcid,
              mc.MCATEGORY,
              CASE WHEN NVL(mi.isedl2021,'N')='Y' THEN 'EDL' ELSE 'NON EDL' END AS EDLType,
              SUM(NVL(oi.absqty,0)) AS OrderedQty,
              ROUND(SUM(NVL(oi.absqty,0) * getfinalRateContract1(b.contractitemid,op.soissuedate)) / 100000,2) AS OrderedValue,
              mi.vedcat
              
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
          WHERE 1=1 " + whmcid + @" 
            AND op.soissuedate BETWEEN 
                  (SELECT STARTDATE FROM masaccyearsettings WHERE 1=1 " + whyearid + @") 
                  AND 
                  (SELECT ENDDATE FROM masaccyearsettings WHERE 1=1 " + whyearid + @")
            " + whisedl + @"
            
          GROUP BY mi.itemid, mi.itemcode, mi.itemname, mi.strength1, mi.unit, mt.ITEMTYPENAME, edl.edl, mi.isedl2021, mc.mcid,mc.MCATEGORY, mi.vedcat
      ) x
      LEFT JOIN (
          SELECT itemid, MAX(RCENDDT) AS RCENDDate 
          FROM v_rcvalid
          GROUP BY itemid
      ) rc ON rc.itemid = x.itemid
      LEFT JOIN (
          SELECT ts.ITEMID, ts.ACTIONCODE
          FROM v_tenderstatusallnew ts
               INNER JOIN masitems m ON m.itemid = ts.itemid
               INNER JOIN masitemcategories c ON c.categoryid = m.categoryid
               INNER JOIN masitemmaincategory mc ON mc.mcid = c.mcid
          WHERE 1=1 " + whmcid + @"           
      ) ts ON ts.itemid = x.itemid
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
where  T.Status = 'C'     " + whmcid + @"    
 " + whisedl + @"
and mi.isfreez_itpr is null

And (tbr.ExpDate >= SysDate or nvl(tbr.ExpDate,SysDate) >= SysDate) and (tbr.Whissueblock = 0 or tbr.Whissueblock is null)
and (nvl(ABSRQTY,0)-nvl(ISSUEQTY,0))>0 
) group by itemid

)whs on  whs.itemid= x.itemid

left outer join 
(
select  itemid,sum(pipelineQTY) newpiple 
from (

select  soi.warehouseid, mi.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end as pipelineQTY
from   soOrderPlaced OP  
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems mi on mi.itemid = oi.itemid
inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.mcid = c.mcid
left outer join 
(
select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty, tr.warehouseid from tbreceipts tr 
inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid,tri.itemid,tr.warehouseid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid and rec.warehouseid=soi.warehouseid
 where op.status  in ('C','O') " + whmcid + @"     " + whisedl + @" 
 group by soi.warehouseid, mi.itemcode,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
 op.soissuedate,op.extendeddate ,receiptdelayexception  
 having (case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end) >0

) 
group by itemid 
 ) whpip on whpip.itemid=x.itemid  
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
and t.transferdate between '01-APR-23' and sysdate 
group by i.itemid
) IWHPipe on  IWHPipe.itemid=x.itemid 

      /* === join your SDE per-item classification === */
      LEFT JOIN (
        /* Your SDE subquery (agg + final) as previously built; must output (item_id, sde_class) */
        WITH base AS (
          SELECT 
            mi.itemid                  AS item_id,
            s.supplierid               AS supplier_id,
            op.soissuedate             AS po_date,
            r.receiptdate              AS receipt_date,
            (r.receiptdate - op.soissuedate) AS lead_time,
            CASE WHEN mi.nablreq = 'Y' THEN 90
                 WHEN op.isimported = 'Y' THEN 75
                 ELSE 60 END AS max_days
          FROM masItems mi
          JOIN soordereditems oi  ON oi.itemid = mi.itemid
          JOIN soorderplaced op   ON op.ponoid = oi.ponoid AND op.status NOT IN ('OC','WA1','I')
          JOIN aoccontractitems b ON b.contractitemid = oi.contractitemid AND b.itemid = oi.itemid
          JOIN masSuppliers s     ON s.SupplierID = op.SupplierID
          JOIN masitemcategories ic   ON ic.categoryid = mi.categoryid
          JOIN masitemmaincategory mc ON mc.MCID = ic.MCID
          LEFT JOIN (
            SELECT i.itemid, t.ponoid, MIN(t.receiptdate) AS receiptdate, SUM(tb.absrqty) AS receiptqty
            FROM tbreceipts t
            JOIN tbreceiptitems i   ON i.receiptid = t.receiptid
            JOIN tbreceiptbatches tb ON tb.receiptitemid = i.receiptitemid
            WHERE t.status = 'C'
              AND t.receipttype = 'NO'
              AND tb.notindpdmis IS NULL
            GROUP BY i.itemid, t.ponoid
          ) r ON r.itemid = oi.itemid AND r.ponoid = op.ponoid
          WHERE 1=1 " + whmcid + @"
             " + whisedl + @"
            AND r.receiptdate IS NOT NULL
            AND op.soissuedate BETWEEN ADD_MONTHS(TRUNC(SYSDATE), -36) AND SYSDATE
        ),
        agg AS (
          SELECT
            item_id,
            COUNT(*) AS total_orders,
            AVG(lead_time) AS avg_lead,
            STDDEV(lead_time) AS std_dev,
            CASE WHEN AVG(lead_time) = 0 THEN 0 ELSE STDDEV(lead_time) / AVG(lead_time) END AS cv,
            PERCENTILE_CONT(0.75) WITHIN GROUP (ORDER BY lead_time) AS p75_lead,
            PERCENTILE_CONT(0.90) WITHIN GROUP (ORDER BY lead_time) AS p90_lead,
            100 * AVG(CASE WHEN lead_time > max_days THEN 1 ELSE 0 END) AS late_rate,
            MAX(max_days) AS max_days,
            COUNT(DISTINCT supplier_id) AS supplier_count
          FROM base
          GROUP BY item_id
        )
        SELECT
          a.item_id,
          CASE
            WHEN ( (CASE WHEN a.p90_lead > a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 40 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.supplier_count <= 1 AND a.p75_lead >= 0.9 * a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.cv >= 0.6 THEN 1 ELSE 0 END) ) >= 2
              THEN 'S'
            WHEN (
                   (CASE WHEN a.p90_lead > a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 40 THEN 1 ELSE 0 END)
                 ) >= 1
                 OR (
                   (CASE WHEN a.p75_lead > 0.8 * a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 15 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.supplier_count = 2 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.cv BETWEEN 0.3 AND 0.6 THEN 1 ELSE 0 END)
                 ) >= 2
              THEN 'D'
            ELSE 'E'
          END AS sde_class
        FROM agg a
      ) sd
      ON sd.ITEM_ID = x.itemid
  )
)
) group by " + whcatType + @"
order by " + whcatType + @" ";

            var result = await _context.ABC_VED_SDE_matrixWithStockOutDbSet
                .FromSqlRaw(qry)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }


        [HttpGet("ABC_VED_SDE_matrixWithStockOutDetail")]
        public async Task<ActionResult<IEnumerable<ABC_VED_SDE_matrixWithStockOutDetailDTO>>> ABC_VED_SDE_matrixWithStockOutDetail(
 string yearid, string mcid, string isEDL, string catType, string iCateogry, string columnFlag)
        {
            string whyearid = "";
            string whmcid = "";
            string whisedl = "";
            string whcatType = "";
            string whIcategory = "";
            string whcolumnFlag = "";



            // Apply filters
            if (yearid != "0")
                whyearid = " and accyrsetid = " + yearid;

            if (mcid != "0")
                whmcid = " and mc.mcid = " + mcid;

            if (isEDL != "0")
            {
                whisedl = " AND NVL(mi.isedl2021,'N')= '" + isEDL + "'";
            }

            if (columnFlag != "0")
            {
                if (columnFlag.ToUpper() == "STOCKOUT")
                {
                    whcolumnFlag = " and STOCKOUT = 'Yes' ";
                }
                if (columnFlag.ToUpper() == "STOCKIN")
                {
                    whcolumnFlag = " and STOCKIN = 'Yes' ";
                }
                if (columnFlag.ToUpper() == "STOCKOUTPOPIPE")
                {
                    whcolumnFlag = " and STOCKOUTPOPIPE = 'Yes' ";
                }
                if (columnFlag.ToUpper() == "RCVALID")
                {
                    whcolumnFlag = " and RCVALID = 'Yes' ";
                }
                if (columnFlag.ToUpper() == "RCNOTVALID")
                {
                    whcolumnFlag = " and RCNOTVALID = 'Yes' ";
                }
                if (columnFlag.ToUpper() == "PRICECNT")
                {
                    whcolumnFlag = " and PRICECNT = 'Yes' ";
                }
                if (columnFlag.ToUpper() == "EVALUTIONCNT")
                {
                    whcolumnFlag = " and EVALUTIONCNT = 'Yes' ";
                }
                if (columnFlag.ToUpper() == "LIVECNT")
                {
                    whcolumnFlag = " and LIVECNT = 'Yes' ";
                }
                if (columnFlag.ToUpper() == "RENTENDERCN")
                {
                    whcolumnFlag = " and RENTENDERCN = 'Yes' ";
                }

            }


            if (catType != "0")
            {
                if (catType.ToUpper() == "MATRIX")
                {
                    whcatType = " and  ABC_VED_SDE_CATEGORY is not null ";

                    whIcategory = " and  ABC_VED_SDE_CATEGORY='" + iCateogry + "' ";


                }
                if (catType.ToUpper() == "ABC")
                {
                    whcatType = " and  ABC_CATEGORY is not null ";
                    whIcategory = " and  ABC_CATEGORY='" + iCateogry + "' ";
                }
                if (catType.ToUpper() == "SDE")
                {
                    whcatType = " and  SDE_CLASS is not null ";
                    whIcategory = " and  SDE_CLASS='" + iCateogry + "' ";
                }
                if (catType.ToUpper() == "VED")
                {
                    whcatType = " and VEDCAT is not null ";
                    whIcategory = " and  VEDCAT='" + iCateogry + "' ";
                }


            }


            string qry = $@" select ITEM_ID, ITEMCODE, DRUG_NAME, STRENGTH1, UNIT, ITEMTYPENAME, EDLTYPE, EDLCAT, MCATEGORY, MCID, ORDER_VALUE, READYWTOCK, UQCSTOCK, SUPPLIERPIPELINE, TRANSFERQTY, STOCKOUT, STOCKIN, STOCKOUTPOPIPE, STOCKOUTPOQTY, RCVALID, RCNOTVALID, PRICECNT, EVALUTIONCNT, LIVECNT, RENTENDERCN, ABC_CATEGORY, VEDCAT, SDE_CLASS, ABC_VED_SDE_CATEGORY

from (
SELECT
    ITEM_ID,
    ITEMCODE,
    DRUG_NAME,
    STRENGTH1,
    UNIT,
    ITEMTYPENAME,
    EDLTYPE,
    EDLCAT,
     MCATEGORY,
      mcid,
    ORDER_VALUE,
    READYWTOCK, UQCSTOCK, SUPPLIERPIPELINE ,transferQTY   ,StockOut,StockIn,StockOutPoPipe,StockOutPoQty

    ,RCValid,
    RCNotValid,
    Pricecnt,
    Evalutioncnt,
    LiveCnt,
    Rentendercn,
    abc_category,
    vedcat,
    sde_class,

    /* NEW: Combined priority bucket */
    CASE
      /* --- Category I (Highest) --- */
      WHEN (
            UPPER(vedcat) = 'V' AND (UPPER(abc_category) IN ('A','B') OR UPPER(sde_class) IN ('S','D'))
          )
           OR UPPER(sde_class) = 'S'
           OR (UPPER(abc_category) = 'A' AND UPPER(sde_class) IN ('S','D'))
      THEN 'I'

      /* --- Category II (Medium) --- */
      WHEN  UPPER(vedcat) = 'E'
         OR UPPER(sde_class) = 'D'
         OR UPPER(abc_category) = 'B'
         OR (UPPER(vedcat) = 'V' AND UPPER(abc_category) = 'C')
      THEN 'II'

      /* --- Category III (Lowest) --- */
      ELSE 'III'
    END AS ABC_VED_SDE_CATEGORY

FROM (
  SELECT 
      ITEM_ID,
      ITEMCODE,
      DRUG_NAME,
      STRENGTH1,
      UNIT,
      ITEMTYPENAME,
      EDLCAT,
      EDLTYPE,
      MCATEGORY,
      mcid,
      CASE WHEN RCValid = 'Valid' THEN 'Yes' ELSE 'No' END AS RCValid,
      CASE WHEN RCValid = 'Not Valid' THEN 'Yes' ELSE 'No' END AS RCNotValid,
      NVL(Pricecnt,0) AS Pricecnt,
      NVL(Evalutioncnt,0) AS Evalutioncnt,
      NVL(LiveCnt,0) AS LiveCnt,
      NVL(Rentendercn,0) AS Rentendercn,
      ORDER_VALUE,
      READYWTOCK, UQCSTOCK, transferQTY,SUPPLIERPIPELINE
          ,case when (nvl(READYWTOCK,0)+nvl(UQCSTOCK,0))=0 then 'Yes' else 'No' end as StockOut
    ,case when (nvl(READYWTOCK,0)+nvl(UQCSTOCK,0))>0 then 'Yes' else 'No' end as StockIn
    , case when (nvl(READYWTOCK,0)+nvl(UQCSTOCK,0))=0  and  nvl(transferQTY,0) =0 and  nvl(SUPPLIERPIPELINE,0)>0 then  'Yes' else 'No' end  as StockOutPoPipe
     , case when (nvl(READYWTOCK,0)+nvl(UQCSTOCK,0))=0  and  nvl(transferQTY,0) =0 and  nvl(SUPPLIERPIPELINE,0)>0 then  nvl(SUPPLIERPIPELINE,0)    else 0 end  as StockOutPoQty
      
     , SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) AS CUMULATIVE_VALUE,
      ROUND(
          SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) / 
          SUM(ORDER_VALUE) OVER () * 100,2
      ) AS CUMULATIVE_PERCENT,
      CASE
          WHEN ROUND(SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) / SUM(ORDER_VALUE) OVER () * 100, 2) <= 70 THEN 'A'
          WHEN ROUND(SUM(ORDER_VALUE) OVER (ORDER BY ORDER_VALUE DESC) / SUM(ORDER_VALUE) OVER () * 100, 2) <= 90 THEN 'B'
          ELSE 'C'
      END AS ABC_CATEGORY,
      vedcat,
      sde_class
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
          x.MCATEGORY,
           x.mcid,
          x.ORDEREDVALUE AS ORDER_VALUE,
          CASE WHEN rc.RCENDDate IS NULL THEN 'Not Valid' ELSE 'Valid' END AS RCValid,
          /* Tender status counts */
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Price Opened in' THEN 'Yes' ELSE 'No' END AS Pricecnt,
          CASE WHEN rc.RCENDDate IS NULL AND (ts.ACTIONCODE = 'Cover-A in' OR ts.ACTIONCODE = 'Claim Objection in') THEN 'Yes' ELSE 'No' END AS Evalutioncnt,
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'Live in' THEN 'Yes' ELSE 'No' END AS LiveCnt,
          CASE WHEN rc.RCENDDate IS NULL AND ts.ACTIONCODE = 'To be Retender' THEN 'Yes' ELSE 'No' END AS Rentendercn,
          vedcat,
          sd.sde_class
          ,nvl(READYFORISSUE,0) ReadyWtock
          ,nvl(PENDING,0) as UQCStock
          ,nvl(newpiple,0) as SupplierPipeLine
          ,nvl(transferQTY,0) as transferQTY
      FROM (
          /* ---- ITEM + ORDER VALUE (with Receipt Join) ---- */
          SELECT 
              mi.itemid, 
              mi.itemcode, 
              mi.itemname, 
              mi.strength1, 
              mi.unit, 
              mt.ITEMTYPENAME, 
              edl.edl AS EDLCAT,
              mc.mcid,
              mc.MCATEGORY,
              CASE WHEN NVL(mi.isedl2021,'N')='Y' THEN 'EDL' ELSE 'NON EDL' END AS EDLType,
              SUM(NVL(oi.absqty,0)) AS OrderedQty,
              ROUND(SUM(NVL(oi.absqty,0) * getfinalRateContract1(b.contractitemid,op.soissuedate)) / 100000,2) AS OrderedValue,
              mi.vedcat
              
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
          WHERE 1=1 " + whmcid + @"            AND op.soissuedate BETWEEN 
                  (SELECT STARTDATE FROM masaccyearsettings WHERE 1=1 " + whyearid + @") 
                  AND 
                  (SELECT ENDDATE FROM masaccyearsettings WHERE 1=1 " + whyearid + @")
           " + whisedl + @"
            
          GROUP BY mi.itemid, mi.itemcode, mi.itemname, mi.strength1, mi.unit, mt.ITEMTYPENAME, edl.edl, mi.isedl2021, mc.mcid,mc.MCATEGORY, mi.vedcat
      ) x
      LEFT JOIN (
          SELECT itemid, MAX(RCENDDT) AS RCENDDate 
          FROM v_rcvalid
          GROUP BY itemid
      ) rc ON rc.itemid = x.itemid
      LEFT JOIN (
          SELECT ts.ITEMID, ts.ACTIONCODE
          FROM v_tenderstatusallnew ts
               INNER JOIN masitems m ON m.itemid = ts.itemid
               INNER JOIN masitemcategories c ON c.categoryid = m.categoryid
               INNER JOIN masitemmaincategory mc ON mc.mcid = c.mcid
          WHERE 1=1 " + whmcid + @" 
    ) ts ON ts.itemid = x.itemid
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
where  T.Status = 'C'      " + whmcid + @"  
" + whisedl + @"
and mi.isfreez_itpr is null

And (tbr.ExpDate >= SysDate or nvl(tbr.ExpDate,SysDate) >= SysDate) and (tbr.Whissueblock = 0 or tbr.Whissueblock is null)
and (nvl(ABSRQTY,0)-nvl(ISSUEQTY,0))>0 
) group by itemid

)whs on  whs.itemid= x.itemid

left outer join 
(
select  itemid,sum(pipelineQTY) newpiple 
from (

select  soi.warehouseid, mi.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end as pipelineQTY
from   soOrderPlaced OP  
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems mi on mi.itemid = oi.itemid
inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.mcid = c.mcid
left outer join 
(
select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty, tr.warehouseid from tbreceipts tr 
inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid,tri.itemid,tr.warehouseid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid and rec.warehouseid=soi.warehouseid
 where op.status  in ('C','O') " + whmcid + @" " + whisedl + @"
 group by soi.warehouseid, mi.itemcode,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
 op.soissuedate,op.extendeddate ,receiptdelayexception  
 having (case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end) >0

) 
group by itemid 
 ) whpip on whpip.itemid=x.itemid  
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
and t.transferdate between '01-APR-23' and sysdate 
group by i.itemid
) IWHPipe on  IWHPipe.itemid=x.itemid 

      /* === join your SDE per-item classification === */
      LEFT JOIN (
        /* Your SDE subquery (agg + final) as previously built; must output (item_id, sde_class) */
        WITH base AS (
          SELECT 
            mi.itemid                  AS item_id,
            s.supplierid               AS supplier_id,
            op.soissuedate             AS po_date,
            r.receiptdate              AS receipt_date,
            (r.receiptdate - op.soissuedate) AS lead_time,
            CASE WHEN mi.nablreq = 'Y' THEN 90
                 WHEN op.isimported = 'Y' THEN 75
                 ELSE 60 END AS max_days
          FROM masItems mi
          JOIN soordereditems oi  ON oi.itemid = mi.itemid
          JOIN soorderplaced op   ON op.ponoid = oi.ponoid AND op.status NOT IN ('OC','WA1','I')
          JOIN aoccontractitems b ON b.contractitemid = oi.contractitemid AND b.itemid = oi.itemid
          JOIN masSuppliers s     ON s.SupplierID = op.SupplierID
          JOIN masitemcategories ic   ON ic.categoryid = mi.categoryid
          JOIN masitemmaincategory mc ON mc.MCID = ic.MCID
          LEFT JOIN (
            SELECT i.itemid, t.ponoid, MIN(t.receiptdate) AS receiptdate, SUM(tb.absrqty) AS receiptqty
            FROM tbreceipts t
            JOIN tbreceiptitems i   ON i.receiptid = t.receiptid
            JOIN tbreceiptbatches tb ON tb.receiptitemid = i.receiptitemid
            WHERE t.status = 'C'
              AND t.receipttype = 'NO'
              AND tb.notindpdmis IS NULL
            GROUP BY i.itemid, t.ponoid
          ) r ON r.itemid = oi.itemid AND r.ponoid = op.ponoid
          WHERE 1=1 " + whmcid + @"
            " + whisedl + @"
            AND r.receiptdate IS NOT NULL
            AND op.soissuedate BETWEEN ADD_MONTHS(TRUNC(SYSDATE), -36) AND SYSDATE
        ),
        agg AS (
          SELECT
            item_id,
            COUNT(*) AS total_orders,
            AVG(lead_time) AS avg_lead,
            STDDEV(lead_time) AS std_dev,
            CASE WHEN AVG(lead_time) = 0 THEN 0 ELSE STDDEV(lead_time) / AVG(lead_time) END AS cv,
            PERCENTILE_CONT(0.75) WITHIN GROUP (ORDER BY lead_time) AS p75_lead,
            PERCENTILE_CONT(0.90) WITHIN GROUP (ORDER BY lead_time) AS p90_lead,
            100 * AVG(CASE WHEN lead_time > max_days THEN 1 ELSE 0 END) AS late_rate,
            MAX(max_days) AS max_days,
            COUNT(DISTINCT supplier_id) AS supplier_count
          FROM base
          GROUP BY item_id
        )
        SELECT
          a.item_id,
          CASE
            WHEN ( (CASE WHEN a.p90_lead > a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 40 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.supplier_count <= 1 AND a.p75_lead >= 0.9 * a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.cv >= 0.6 THEN 1 ELSE 0 END) ) >= 2
              THEN 'S'
            WHEN (
                   (CASE WHEN a.p90_lead > a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 40 THEN 1 ELSE 0 END)
                 ) >= 1
                 OR (
                   (CASE WHEN a.p75_lead > 0.8 * a.max_days THEN 1 ELSE 0 END) +
                   (CASE WHEN a.late_rate > 15 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.supplier_count = 2 THEN 1 ELSE 0 END) +
                   (CASE WHEN a.cv BETWEEN 0.3 AND 0.6 THEN 1 ELSE 0 END)
                 ) >= 2
              THEN 'D'
            ELSE 'E'
          END AS sde_class
        FROM agg a
      ) sd
      ON sd.ITEM_ID = x.itemid
  )
)

)
where 1=1 " + whcatType + @"  " + whcolumnFlag + @" " + whIcategory + @" ";

            var result = await _context.ABC_VED_SDE_matrixWithStockOutDetailDbSet
                .FromSqlRaw(qry)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }



        [HttpGet("pipelineSlippage")]
        public async Task<ActionResult<IEnumerable<pipelineSlippageDTO>>> pipelineSlippage()
        {


            string qry = @" select Timduration,count(distinct itemid) as nos,count(distinct ponoid) as NosPO from 
(
select  soi.warehouseid, mi.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception,DURATION as SupplyDuration ,round(sysdate-op.soissuedate,0) as days,

case when round(sysdate-op.soissuedate,0) >DURATION and  (round(sysdate-op.soissuedate,0)-DURATION)>=14 then '>14 Days'
else case when (round(sysdate-op.soissuedate,0) >DURATION and round(sysdate-op.soissuedate,0)-DURATION<14) then '1-14 Days'
else 'Timeline' end end as Timduration,

case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end as pipelineQTY
,round((nvl(rec.receiptabsqty,0)/sum(soi.ABSQTY)) *100,2) as per
,DURATION-round(sysdate-op.soissuedate,0) d
from   soOrderPlaced OP  
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems mi on mi.itemid = oi.itemid
inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.mcid = c.mcid
inner join sotranches t on t.ponoid=OP.ponoid
left outer join 
(
select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty, tr.warehouseid from tbreceipts tr 
inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid,tri.itemid,tr.warehouseid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid and rec.warehouseid=soi.warehouseid
 where op.status  in ('C','O') and mc.mcid=1 and nvl(mi.isedl2021,'N')='Y' 
 group by DURATION,soi.warehouseid, mi.itemcode,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
 op.soissuedate,op.extendeddate ,receiptdelayexception  
 having (case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1
and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end) >0
and round((nvl(rec.receiptabsqty,0)/sum(soi.ABSQTY)) *100,2)<90
) group by Timduration
 ";





            var myList = _context.pipelineSlippageDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("pipelineSlippageItemDetail")]
        public async Task<ActionResult<IEnumerable<PipelineSlippageDetailDTO>>> pipelineSlippageItemDetail(int flag)
        {
            // flag: 1 => ">14 Days", 2 => "1-14 Days"
            if (flag != 1 && flag != 2)
                return BadRequest("flag must be 1 (\">14 Days\") or 2 (\"1-14 Days\").");

            var bucket = flag == 1 ? ">14 Days" : "1-14 Days";

            string qry = @"  /* Detail (drill-down) query that returns EXACTLY one row per unique ITEMID
   for the selected bucket so the row count matches the summary `nos`.

   Use :pFlag = 1 for '>14 Days', :pFlag = 2 for '1-14 Days'
*/

WITH x AS (
    SELECT
        soi.warehouseid,
        mi.itemcode,
        mi.itemname,
        OI.itemid,
        op.ponoid,
        op.soissuedate,
        op.extendeddate,
        SUM(soi.ABSQTY) AS absqty,
        NVL(rec.receiptabsqty, 0) AS receiptabsqty,
        receiptdelayexception,
        DURATION AS SupplyDuration,
        ROUND(SYSDATE - op.soissuedate, 0) AS days,
        CASE
            WHEN ROUND(SYSDATE - op.soissuedate, 0) > DURATION
                 AND (ROUND(SYSDATE - op.soissuedate, 0) - DURATION) >= 14 THEN '>14 Days'
            WHEN (ROUND(SYSDATE - op.soissuedate, 0) > DURATION
                  AND (ROUND(SYSDATE - op.soissuedate, 0) - DURATION) < 14) THEN '1-14 Days'
            ELSE 'Timeline'
        END AS Timduration,
        CASE
            WHEN op.extendeddate IS NULL
                 AND ROUND(SYSDATE - op.soissuedate, 0) <= 120
              THEN SUM(soi.ABSQTY) - NVL(rec.receiptabsqty, 0)
            WHEN op.receiptdelayexception = 1
                 AND SYSDATE <= op.extendeddate + 1
              THEN SUM(soi.ABSQTY) - NVL(rec.receiptabsqty, 0)
            WHEN op.extendeddate IS NOT NULL
                 AND op.receiptdelayexception = 1
                 AND (op.extendeddate + 1) <= op.soissuedate
                 AND ROUND(SYSDATE - op.soissuedate, 0) <= 120
              THEN SUM(soi.ABSQTY) - NVL(rec.receiptabsqty, 0)
            ELSE 0
        END AS pipelineQTY,
        ROUND((NVL(rec.receiptabsqty,0) / SUM(soi.ABSQTY)) * 100, 2) AS per,
        DURATION - ROUND(SYSDATE - op.soissuedate, 0) AS d
    FROM   soOrderPlaced OP
    INNER JOIN SoOrderedItems OI       ON OI.PoNoID       = OP.PoNoID
    INNER JOIN soorderdistribution soi ON soi.orderitemid  = OI.orderitemid
    INNER JOIN masitems mi             ON mi.itemid        = OI.itemid
    INNER JOIN masitemcategories c     ON c.categoryid     = mi.categoryid
    INNER JOIN masitemmaincategory mc  ON mc.mcid          = c.mcid
    INNER JOIN sotranches t            ON t.ponoid         = OP.ponoid
    LEFT OUTER JOIN (
        SELECT tr.ponoid, tri.itemid, SUM(tri.receiptabsqty) AS receiptabsqty, tr.warehouseid
        FROM   tbreceipts tr
        INNER JOIN tbreceiptitems tri ON tri.receiptid = tr.receiptid
        WHERE  tr.receipttype = 'NO'
           AND tr.status = 'C'
           AND tr.notindpdmis IS NULL
           AND tri.notindpdmis IS NULL
        GROUP BY tr.ponoid, tri.itemid, tr.warehouseid
    ) rec ON rec.ponoid = OP.PoNoID
         AND rec.itemid = OI.itemid
         AND rec.warehouseid = soi.warehouseid
    WHERE op.status IN ('C','O')
      AND mc.mcid = 1
      AND NVL(mi.isedl2021,'N') = 'Y'
    GROUP BY
        DURATION,
        soi.warehouseid,
        mi.itemcode,
        mi.itemname,
        op.ponoid,
        op.soissuedate,
        op.extendeddate,
        OI.itemid,
        rec.receiptabsqty,
        op.soissuedate,
        op.extendeddate,
        receiptdelayexception
    HAVING
        (CASE
            WHEN op.extendeddate IS NULL
                 AND ROUND(SYSDATE - op.soissuedate, 0) <= 120
              THEN SUM(soi.ABSQTY) - NVL(rec.receiptabsqty, 0)
            WHEN op.receiptdelayexception = 1
                 AND SYSDATE <= op.extendeddate + 1
              THEN SUM(soi.ABSQTY) - NVL(rec.receiptabsqty, 0)
            WHEN op.extendeddate IS NOT NULL
                 AND op.receiptdelayexception = 1
                 AND (op.extendeddate + 1) <= op.soissuedate
                 AND ROUND(SYSDATE - op.soissuedate, 0) <= 120
              THEN SUM(soi.ABSQTY) - NVL(rec.receiptabsqty, 0)
            ELSE 0
        END) > 0
      AND ROUND((NVL(rec.receiptabsqty,0) / SUM(soi.ABSQTY)) * 100, 2) < 90
),
filt AS (
    SELECT *
    FROM   x
    WHERE  (Timduration = '"+ bucket + @"')
      
)
SELECT
    MIN(filt.Timduration)               AS Timduration,     -- constant per item after filter
    filt.itemid                         AS itemid,          -- UNIQUE per row -> matches summary `nos`
    MIN(filt.itemcode)                  AS itemcode,
    MIN(filt.itemname)                  AS itemname,
    SUM(filt.absqty)                    AS absqty_sum,
    SUM(filt.receiptabsqty)             AS receiptabsqty_sum,
    SUM(filt.pipelineQTY)               AS pipelineqty_sum,
    MIN(filt.per)                       AS min_per,         -- lowest % received (worst)
    MIN(filt.d)                         AS worst_d,         -- most overdue (lowest d)
    COUNT(DISTINCT filt.ponoid)         AS nospo            -- helpful in drilldown
FROM filt
GROUP BY filt.itemid
ORDER BY worst_d ASC, min_per ASC, itemcode;
 ";

            var pBucket = new OracleParameter("pBucket", OracleDbType.Varchar2) { Value = bucket };

            var rows = await _context.PipelineSlippageDetailDbSet
                .FromSqlRaw(qry, pBucket)
                .ToListAsync();

            return rows;
        }



        [HttpGet("PipelineSlippagePOItemDetailDTO")]
        public async Task<ActionResult<IEnumerable<PipelineSlippagePOItemDetailDTO>>> PipelineSlippagePOItemDetailDTO(int flag)
        {
            // flag: 1 => ">14 Days", 2 => "1-14 Days"
            if (flag != 1 && flag != 2)
                return BadRequest("flag must be 1 (\">14 Days\") or 2 (\"1-14 Days\").");

            var bucket = flag == 1 ? ">14 Days" : "1-14 Days";

            string qry = @" select itemcode,itemname,strength1,unit,suppliername,pono,soissuedate,extendeddate,sum(absqty) as POQTY,sum(receiptabsqty) as ReceivedQTY,Timduration
from 
(

select  op.pono,soi.warehouseid, mi.itemcode,mi.itemname,mi.strength1,mi.unit,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception,DURATION as SupplyDuration ,round(sysdate-op.soissuedate,0) as days,

case when round(sysdate-op.soissuedate,0) >DURATION and  (round(sysdate-op.soissuedate,0)-DURATION)>=14 then '>14 Days'
else case when (round(sysdate-op.soissuedate,0) >DURATION and round(sysdate-op.soissuedate,0)-DURATION<14) then '1-14 Days'
else 'Timeline' end end as Timduration,

case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end as pipelineQTY
,round((nvl(rec.receiptabsqty,0)/sum(soi.ABSQTY)) *100,2) as per
,DURATION-round(sysdate-op.soissuedate,0) d
,s.suppliername
from   soOrderPlaced OP  
inner join massuppliers s on s.supplierid=op.supplierid
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems mi on mi.itemid = oi.itemid
inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.mcid = c.mcid
inner join sotranches t on t.ponoid=OP.ponoid
left outer join 
(
select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty, tr.warehouseid from tbreceipts tr 
inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid,tri.itemid,tr.warehouseid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid and rec.warehouseid=soi.warehouseid
 where op.status  in ('C','O') and mc.mcid=1 and nvl(mi.isedl2021,'N')='Y' 
 group by DURATION,soi.warehouseid, mi.itemcode,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
 op.soissuedate,op.extendeddate ,receiptdelayexception  ,op.pono,mi.itemname,mi.strength1,mi.unit,suppliername
 having (case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1
and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 120 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end) >0
and round((nvl(rec.receiptabsqty,0)/sum(soi.ABSQTY)) *100,2)<90 
and (case when round(sysdate-op.soissuedate,0) >DURATION and  (round(sysdate-op.soissuedate,0)-DURATION)>=14 then '>14 Days'
else case when (round(sysdate-op.soissuedate,0) >DURATION and round(sysdate-op.soissuedate,0)-DURATION<14) then '1-14 Days'
else 'Timeline' end end )='"+ bucket + @"'
) group by itemcode,pono,soissuedate,extendeddate,Timduration,itemname,strength1,unit,suppliername
order by itemname
 ";

            var pBucket = new OracleParameter("pBucket", OracleDbType.Varchar2) { Value = bucket };

            var rows = await _context.PipelineSlippagePOItemDetailDbSet
                .FromSqlRaw(qry, pBucket)
                .ToListAsync();

            return rows;
        }


    }




}
