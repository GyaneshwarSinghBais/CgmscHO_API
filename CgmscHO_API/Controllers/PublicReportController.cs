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
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicReportController : ControllerBase
    {
        private readonly OraDbContext _context;
        public PublicReportController(OraDbContext context)
        {
            _context = context;
        }
        [HttpGet("ItemWHStock")]
        public async Task<ActionResult<IEnumerable<WHWisePubStockDTO>>> ItemWHStock(string mitemid)
        {



            string qry = "";

            qry = @" select W.WAREHOUSEID,w.WAREHOUSENAME ,A.itemcode as ITEMCODE,A.ItemName || '-' || A.strength1 as ItemName, A.SKU,
(case when sum( A.ReadyForIssue)>0 then sum( A.ReadyForIssue) else 0 end) as ReadyForIssue,
(case when sum(nvl(Pending,0)) >0 then sum(nvl(Pending,0)) else 0 end) Pending,
nvl(iss.issueqty,0) issueqty_CFY
                  from 
                  maswarehouses w 
                  left outer join 
                 (  
                 select w.WAREHOUSEID,mi.ITEMCODE, b.inwno,mi.ITEMNAME , mi.strength1 ,mi.unit as SKU , 
               (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,
                   case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end  Pending  
               
                  from tbreceiptbatches b  
                  inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
                  inner join tbreceipts t on t.receiptid=i.receiptid 
                  inner join masitems mi on mi.itemid=i.itemid 
                 left outer join MASWAREHOUSES w  on w.warehouseid=t.warehouseid 
                 left outer join 
                  (  
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty   
                   from tboutwards tbo, tbindentitems tbi , tbindents tb 
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null 
                     and tbi.itemid=" + mitemid + @"
                   group by tbi.itemid,tb.warehouseid,tbo.inwno  
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid 
                 Where  T.Status = 'C'  and (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) And (b.Whissueblock = 0 or b.Whissueblock is null)  and  mi.itemid = " + mitemid + @"
                 and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null 
                 ) A on A.warehouseid = w.warehouseid
                 left outer join
                 (
                   select tbi.itemid,tb.warehouseid,
                    sum(nvl(tbo.issueqty,0)) issueqty
                     from tbindents tb
                     inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                     inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                    where tb.status = 'C' and tb.issuetype='NO' and tbi.itemid = " + mitemid + @"
                    and tb.indentdate between 
                    (select startdate from masaccyearsettings where  sysdate BETWEEN startdate and enddate) 
                    and (select enddate from masaccyearsettings where  sysdate BETWEEN startdate and enddate)
                    group by tbi.itemid,tb.warehouseid
                 ) iss on iss.warehouseid = w.warehouseid
                 group by w.WAREHOUSENAME,A.itemcode,A.ItemName, A.strength1,A.SKU,iss.issueqty,W.WAREHOUSEID order by WAREHOUSENAME";




            var myList = _context.WHWisePubStockDbSet
               .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }




        [HttpGet("WHItemStock")]
        public async Task<ActionResult<IEnumerable<ItemwisewhStockDTO>>> WHItemStock(string whid, string mcid, string groupId)
        {
            string whGroupClauseMi = "";
            string whGroupClauseG = "";
            if (groupId != "1")
            {
                whGroupClauseMi = " and  mi.groupid = "+ groupId +" "; 
                    whGroupClauseG = " and  g.groupid = " + groupId + " ";
            }


            string qry = "";

            qry = @" select  to_char(ROW_NUMBER() OVER ( ORDER BY g.groupname ))||'.'||A.ItemName || '-' || A.strength1||'-'||A.itemcode as ItemName,A.itemid,mc.mcid,mc.mcategory categoryname,g.groupname,ty.itemtypename, 
nvl(ed.edl,'-') edltype,
A.itemcode as ITEMCODE,
 A.SKU,A.WAREHOUSENAME,(case when sum(nvl( A.ReadyForIssue,0))>0 then sum(nvl(A.ReadyForIssue,0)) else 0 end) as ReadyForIssue,
 (case when sum(nvl(Pending,0)) >0 then sum(nvl(Pending,0)) else 0 end) Pending,nvl(iss.issueqty,0) issueqty_cfy
   from                    
 (
     select w.WAREHOUSEID,w.warehousename,mi.itemid,mi.ITEMCODE, b.inwno,mi.ITEMNAME , mi.strength1 ,mi.unit as SKU ,                    
     (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,                    
     case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end  Pending                   
     from tbreceiptbatches b                   
     inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid                  
     inner join tbreceipts t on t.receiptid=i.receiptid                  
     inner join masitems mi on mi.itemid=i.itemid
     inner join masitemcategories c on c.categoryid=mi.categoryid
     inner join masitemmaincategory mc on mc.MCID=c.MCID
     inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid                   
     left outer join                  
         (
         select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty                      
         from tboutwards tbo, tbindentitems tbi , tbindents tb                    
         where  tbo.indentitemid=tbi.indentitemid 
         and tbi.indentid=tb.indentid and tb.status = 'C'                  
         group by tbi.itemid,tb.warehouseid,tbo.inwno
         ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid                   
     Where  T.Status = 'C'  and mi.ishide is null 
     And (b.Whissueblock = 0 or b.Whissueblock is null)  and  mc.MCID=" + mcid + @"
     and (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate)  
     and w.warehouseid=" + whid + @"  "+ whGroupClauseMi + @"         
 ) A   
  left outer join
    (
      select tbi.itemid,tb.warehouseid,sum(nvl(tbo.issueqty,0)) issueqty
      from tbindents tb
      inner join tbindentitems tbi on tbi.indentid=tb.indentid 
      inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
      where tb.status = 'C' and tb.issuetype='NO' and tb.warehouseid=" + whid + @"
      and tb.indentdate between (select startdate from masaccyearsettings where  sysdate BETWEEN startdate and enddate) and (select enddate from masaccyearsettings where  sysdate BETWEEN startdate and enddate)
      group by tbi.itemid,tb.warehouseid
    ) iss on iss.warehouseid = A.warehouseid and iss.itemid = A.itemid
     inner join masitems mia on mia.ITEMCODE=A.ITEMCODE            
     inner join masitemcategories c on c.categoryid=mia.categoryid
     inner join masitemmaincategory mc on mc.MCID=c.MCID 
     left outer join masedl ed on ed.edlcat=mia.edlcat 
     left outer join masitemgroups g on g.groupid=mia.groupid  
     left outer join  masitemtypes ty on ty.itemtypeid=mia.itemtypeid         where 1=1 and    mc.MCID=" + mcid + @" "+ whGroupClauseG + @"
     group by A.WAREHOUSEID,A.WAREHOUSENAME,A.itemid,A.itemcode,A.ItemName, A.strength1,A.SKU,mc.mcid,mc.mcategory,ed.edl ,ed.edlcat,g.groupname,ty.itemtypename ,iss.issueqty                  
     having sum(nvl(A.ReadyForIssue,0)) >0 or  sum(nvl(A.Pending,0)) >0 order by g.groupname ";




            var myList = _context.ItemwisewhStockDbSet
               .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("DistwiseIssuance")]
        public async Task<ActionResult<IEnumerable<DistwiseItemIssuanceDTO>>> DistwiseIssuance(string itemid,string yearid,string hodid,string startDT,string endDT)
        {
            FacOperations f = new FacOperations(_context);
            string whyearid = "";
            if (yearid == "0")
            {
                yearid = f.getACCYRSETID();
                whyearid = "  and tb.indentdate between (select startdate from masaccyearsettings where ACCYRSETID="+ yearid + @") and(select enddate from masaccyearsettings  where ACCYRSETID="+ yearid + @") ";
            }
            string WhgreterthanStartDT = "";
            //if (startDT!="0" && endDT!="0")
            //{
            //   // 17 - Feb - 2024
            //    WhgreterthanStartDT = " and tb.indentdate >= '" + startDT + "'";
            //}

            string WHetweentwoDate = "";
            if (startDT != "0" && endDT != "0")
            {
                // 17 - Feb - 2024
                WHetweentwoDate = " and tb.indentdate >= '" + startDT + "' and tb.indentdate <= '" + endDT + "'";
            }


            string qry = "";
            if (hodid == "2") //DHS
            {
                qry = @" select ROW_NUMBER() OVER (ORDER BY  di.districtid) AS ID, di.districtid, di.districtname, nvl(IssuedSKU,0) as IssuedSKU,nvl(IssuedNos,0) as IssuedNos from masdistricts di
left outer join
(

select f.districtid, sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) IssuedSKU ,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*nvl(m.unitcount,1) as IssuedNos
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid
         inner join masitems m on m.itemid= tbi.itemid
         inner join tboutwards tbo on tbo.indentitemid= tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno= tbo.inwno
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes ty on ty.facilitytypeid= f.facilitytypeid
        where tb.status = 'C' and tb.issuetype= 'NO' and ty.facilitytypeid not in (371,378,364) " + whyearid + @"

        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        and tbi.itemid= "+ itemid + @" "+ WhgreterthanStartDT + @" "+ WHetweentwoDate + @"
        group by f.districtid, m.unitcount
        ) dt on dt.districtid=di.districtid
        where di.stateid=1
        order by di.districtname";
            } 
            else if (hodid == "3") //DME
            {
                qry = @" select ROW_NUMBER() OVER (ORDER BY  f.facilityid) AS ID, f.facilityid as districtid,f.facilityname as districtname,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) IssuedSKU ,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*nvl(m.unitcount,1) as IssuedNos
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join masitems m on m.itemid=tbi.itemid
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masdistricts d on d.districtid=f.districtid
         inner join masfacilitytypes ty on ty.facilitytypeid=f.facilitytypeid 
        where tb.status = 'C' and tb.issuetype='NO' and ty.facilitytypeid  in (378,364) " + whyearid + @"
        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        and tbi.itemid=" + itemid + @" "+ WhgreterthanStartDT + @" "+ WHetweentwoDate + @"
        group by d.districtid,d.districtname,m.unitcount,f.facilityid,f.facilityname
        order by d.districtname ";
            }
            else if (hodid == "7") //Ayush
            {
                qry = @" select ROW_NUMBER() OVER (ORDER BY  d.districtid) AS ID, d.districtid,d.districtname,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) IssuedSKU ,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*nvl(m.unitcount,1) as IssuedNos
         from masdistricts d 
         inner join masfacilities f on f.districtid = d.districtid
           inner join tbindents tb on tb.facilityid=f.facilityid
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join masitems m on m.itemid=tbi.itemid
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         

         inner join masfacilitytypes ty on ty.facilitytypeid=f.facilitytypeid 
        where tb.status = 'C' and tb.issuetype='NO' and ty.facilitytypeid  in (371) " + whyearid + @"
        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        and tbi.itemid=" + itemid + @" "+ WhgreterthanStartDT + @" "+ WHetweentwoDate + @"
        group by d.districtid,d.districtname,m.unitcount
        order by d.districtname ";
            }
            else 
            {

                qry = @"  select ROW_NUMBER() OVER (ORDER BY  di.districtid) AS ID, di.districtid,di.districtname,nvl(IssuedSKU, 0) as IssuedSKU,nvl(IssuedNos, 0) as IssuedNos from masdistricts di
                left outer join
                (

                select f.districtid, sum(tbo.issueqty +nvl(tbo.reconcile_qty, 0)) IssuedSKU ,sum(tbo.issueqty + nvl(tbo.reconcile_qty, 0)) * nvl(m.unitcount, 1) as IssuedNos
         from tbindents tb
         inner
         join tbindentitems tbi on tbi.indentid = tb.indentid
         inner
         join masitems m on m.itemid = tbi.itemid
         inner
         join tboutwards tbo on tbo.indentitemid = tbi.indentitemid
         inner
         join tbreceiptbatches rb on rb.inwno = tbo.inwno
         inner
         join masfacilities f on f.facilityid = tb.facilityid
         inner
         join masfacilitytypes ty on ty.facilitytypeid = f.facilitytypeid
        where tb.status = 'C' and tb.issuetype = 'NO' " + whyearid + @"
        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null
        and tbo.notindpdmis is null and rb.notindpdmis is null
        and tbi.itemid = " + itemid + @" "+ WhgreterthanStartDT + @" "+ WHetweentwoDate + @"
        group by f.districtid,m.unitcount
        ) dt on dt.districtid = di.districtid
        where di.stateid = 1
        order by di.districtname ";
            }
            var myList = _context.DistwiseItemIssuanceDbSet
          .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("FacwiseIssuance")]
        public async Task<ActionResult<IEnumerable<FACwiseItemIssuanceDTO>>> FacwiseIssuance(string itemid, string yearid, string Para, string startDT, string endDT,string facid)
        {
            FacOperations f = new FacOperations(_context);
            string whyearid = "";
            if (yearid == "0")
            {
                yearid = f.getACCYRSETID();
                whyearid = "  and tb.indentdate between (select startdate from masaccyearsettings where ACCYRSETID=" + yearid + @") and(select enddate from masaccyearsettings  where ACCYRSETID=" + yearid + @") ";
            }
            string WhgreterthanStartDT = "";
            //if (startDT != "0" && endDT == "0")
            //{
            //    // 17 - Feb - 2024
            //    WhgreterthanStartDT = " and tb.indentdate >= '" + startDT + "'";
            //}

            string WHetweentwoDate = "";
            if (startDT != "0" && endDT != "0")
            {
                // 17 - Feb - 2024
                WHetweentwoDate = " and tb.indentdate >= '" + startDT + "' and tb.indentdate <= '" + endDT + "'";
            }
            string whfacid = "";
            string orderbyclause = "";
            if (facid != "0")
            {
                whfacid = " and f.facilityid=" + facid;
                orderbyclause = " order by  ty.ORDERDP ";
            }
            else
            {
                orderbyclause = "  order by tb.indentdate desc ";
            }

              string qry = "";
            if (Para == "QTY")
            {
               


                qry = @" select  ROW_NUMBER() OVER (ORDER BY  f.facilityid) AS ID, f.facilityid,'' as indentdate,f.facilityname || '(' || d.districtname || ')' as facilityname,sum(tbo.issueqty + nvl(tbo.reconcile_qty, 0)) IssuedSKU ,sum(tbo.issueqty + nvl(tbo.reconcile_qty, 0)) * nvl(m.unitcount, 1) as IssuedNos
         from tbindents tb
         inner
         join tbindentitems tbi on tbi.indentid = tb.indentid
         inner
         join masitems m on m.itemid = tbi.itemid
         inner
         join tboutwards tbo on tbo.indentitemid = tbi.indentitemid
         inner
         join tbreceiptbatches rb on rb.inwno = tbo.inwno
         inner
         join masfacilities f on f.facilityid = tb.facilityid
         inner
         join masdistricts d on d.districtid = f.districtid
         inner
         join masfacilitytypes ty on ty.facilitytypeid = f.facilitytypeid
        where tb.status = 'C' and tb.issuetype = 'NO'
" + whyearid + @" "+ whfacid + @"
        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null
        and tbo.notindpdmis is null and rb.notindpdmis is null
        and tbi.itemid = " + itemid + @" " + WhgreterthanStartDT + @" " + WHetweentwoDate + @"
        group by d.districtid,d.districtname,m.unitcount,f.facilityid,f.facilityname,ty.ORDERDP  "+ orderbyclause;
            }
         
            else
            {

                qry = @" select  ROW_NUMBER() OVER (ORDER BY  f.facilityid) AS ID, f.facilityid,to_char(tb.indentdate,'dd-MM-yyyy') as indentdate,f.facilityname || '(' || d.districtname || ')' as facilityname,sum(tbo.issueqty + nvl(tbo.reconcile_qty, 0)) IssuedSKU ,sum(tbo.issueqty + nvl(tbo.reconcile_qty, 0)) * nvl(m.unitcount, 1) as IssuedNos
         from tbindents tb
         inner
         join tbindentitems tbi on tbi.indentid = tb.indentid
         inner
         join masitems m on m.itemid = tbi.itemid
         inner
         join tboutwards tbo on tbo.indentitemid = tbi.indentitemid
         inner
         join tbreceiptbatches rb on rb.inwno = tbo.inwno
         inner
         join masfacilities f on f.facilityid = tb.facilityid
         inner
         join masdistricts d on d.districtid = f.districtid
         inner
         join masfacilitytypes ty on ty.facilitytypeid = f.facilitytypeid
        where tb.status = 'C' and tb.issuetype = 'NO'
" + whyearid + @"  "+ whfacid + @"   and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null
        and tbo.notindpdmis is null and rb.notindpdmis is null
        and tbi.itemid = " + itemid + @" " + WhgreterthanStartDT + @" " + WHetweentwoDate + @"
        group by d.districtid,d.districtname,m.unitcount,f.facilityid,f.facilityname,ty.ORDERDP,tb.indentdate  " + orderbyclause;
            }
            var myList = _context.FACwiseItemIssuanceDbSet
          .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }
    } 
}
