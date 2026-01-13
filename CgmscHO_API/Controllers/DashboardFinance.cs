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
using CgmscHO_API.FinanceDTO;
using System.Reflection;
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardFinance : ControllerBase
    {
        private readonly OraDbContext _context;
        public DashboardFinance(OraDbContext context)
        {
            _context = context;
        }
        [HttpGet("FundReivedBudgetID")]

        public async Task<ActionResult<IEnumerable<RecFundsDTO>>> FundReivedBudgetID(string bugetid, string yrid)
        {
            string whbudtgetid = "";
            string whbdbudtgetid = "";
            if (bugetid != "0")
            {

                whbudtgetid = " and budgetid =" + bugetid;
                whbdbudtgetid = " and bd.budgetid =" + bugetid;

            }
            string whyearid = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();

            }
            else
            {

                whyearid = yrid;
            }


            string qry = @"select yr.ACCYEAR,sum(RecAmt) as RecAmt,0 as refund,0 as adjust,r.ACCYRSETID
from 
(
select round(sum(actRecptAMOUNT)/10000000,2) as RecAmt ,ACCYRSETID
from 
(
select AMOUNT as actRecptAMOUNT,
(select ACCYRSETID from masaccyearsettings where RECEIVEDDATE between STARTDATE and  ENDDATE) as ACCYRSETID 
from masbudgetdetailsactualentry
where RECEIVEDDATE  between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) 
and (select ENDDATE from masaccyearsettings where ACCYRSETID=" + whyearid + @")
" + whbudtgetid + @"
) group by ACCYRSETID

union all

select round(sum(ReceAfterOPAct)/10000000,2) as RecAmt ,ACCYRSETID
from 
(
select AMOUNT as ReceAfterOPAct,budgetid, 
(select ACCYRSETID from masaccyearsettings where RECEIVEDDATE between STARTDATE and  ENDDATE) as ACCYRSETID from masbudgetdetails bd where 1=1 --isop !='Y' 
and RECEIVEDDATE 
between (select STARTDATE from masaccyearsettings where ACCYRSETID=542)
and (select ENDDATE from masaccyearsettings where ACCYRSETID=" + whyearid + @")
and ISPROVISIONAL is null 
" + whbdbudtgetid + @"
) group by ACCYRSETID
) r
inner join masaccyearsettings yr on yr.ACCYRSETID=r.ACCYRSETID
group by r.ACCYRSETID,yr.ACCYEAR
order by r.ACCYRSETID ";



            var myList = _context.RecFundsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("FundReivedBudgetDetails")]

        public async Task<ActionResult<IEnumerable<RecFundsDetailsDTO>>> FundReivedBudgetDetails(string bugetid, string yrid)
        {
            string whbudtgetid = "";
            string whbdbudtgetid = "";
            if (bugetid != "0")
            {

                whbudtgetid = " and budgetid =" + bugetid;
                whbdbudtgetid = " and bd.budgetid =" + bugetid;

            }
            string whyearid = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();

            }
            else
            {

                whyearid = yrid;
            }
            string qry = @" select to_char(RECEIVEDDATE,'dd-MM-yyyy') as RECEIVEDDATE, sum(actRecptAMOUNT) as RecAmt,round(sum(actRecptAMOUNT)/10000000,2) as RecCr,ACCYRSETID
from
(

select AMOUNT as actRecptAMOUNT, budgetid,
(select ACCYRSETID from masaccyearsettings where RECEIVEDDATE between STARTDATE and  ENDDATE) as ACCYRSETID 
,RECEIVEDDATE,REMARKS
,ABGID,BGID,FILEPATH
from masbudgetdetailsactualentry
where RECEIVEDDATE between(select STARTDATE from masaccyearsettings where ACCYRSETID = " + whyearid + @")
and(select ENDDATE from masaccyearsettings where ACCYRSETID = "+ whyearid + @")
"+ whbudtgetid + @"



union all


select AMOUNT as ReceAfterOPAct,budgetid, 
(
select ACCYRSETID from masaccyearsettings where RECEIVEDDATE between STARTDATE and  ENDDATE) as ACCYRSETID
,RECEIVEDDATE,REMARKS,0 as ABGID,BGID,FILEPATH
from masbudgetdetails bd where 1=1 --isop !='Y' 
and RECEIVEDDATE
between(select STARTDATE from masaccyearsettings where ACCYRSETID = "+ whyearid + @")
and(select ENDDATE from masaccyearsettings where ACCYRSETID = "+ whyearid + @")
and ISPROVISIONAL is null 
"+ whbdbudtgetid + @"
 ) group by RECEIVEDDATE,ACCYRSETID
 order by RECEIVEDDATE ";
          var myList = _context.RecFundsDetailsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        //        [HttpGet("PODetailsAgainstIndentYr")]

        //        public async Task<ActionResult<IEnumerable<AIPO_ReceiptDTO>>> PODetailsAgainstIndentYr(string bugetid, string yrid, string HOD)
        //        {
        //            string whbudtgetid = "";
        //            string whbdbudtgetid = "";
        //            if (bugetid != "0")
        //            {


        //                whbdbudtgetid = " and P.BudgetID = " + bugetid;

        //            }
        //            string whyearid = "";
        //            if (yrid == "0")
        //            {
        //                FacOperations f = new FacOperations(_context);


        //                whyearid = f.getACCYRSETID();

        //            }
        //            else
        //            {

        //                whyearid = yrid;
        //            }


        //            //string qry = @"  select AIFINYEAR,yr.ACCYEAR,
        //            // count(distinct itemid) NoofItem,
        //            // count(distinct ponoid) as NoofPO,
        //            //round(sum(dhspovalue)/10000000,2) POValue,
        //            //round(sum(dhsrvalue)/10000000,2) RecValue
        //            //,round(sum(totalpaid)/10000000,2) totalpaid
        //            //from (
        //            //select mc.mcid,mc.MCATEGORY,mi.itemcode,itemname,mi.unit,op.pono pono,op.ponoid,to_char( op.soissuedate,'dd/mm/yyyy') podate,
        //            //                            nvl(sum(od.dhsqty),0) dhspoqty,
        //            //                            nvl(sum(od.dhsqty),0)*c.finalrategst dhspovalue,
        //            //                            nvl(r1.dhsrqty,0) dhsrqty,nvl(r1.dhsrqty,0)*c.finalrategst dhsrvalue,                   
        //            //                            op.AIFINYEAR,mi.itemid,nvl(totalpaid,0) as totalpaid
        //            //                            from masItems MI
        //            //                                              inner join soOrderedItems OI on (OI.ItemID = MI.ItemID) 
        //            //                                              inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' )) 
        //            //                                              inner join soorderdistribution od on (od.orderitemid = oi.orderitemid) 
        //            //                                              inner join aoccontractitems c on c.contractitemid = oi.contractitemid
        //            //                                              inner join masitemcategories ic on ic.categoryid = mi.categoryid
        //            //                                              inner join masitemmaincategory mc on mc.MCID=ic.MCID
        //            //                                              left outer join  
        //            //                                                (
        //            //                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  
        //            //                                                from tbreceipts t 
        //            //                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
        //            //                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
        //            //                                                where T.Status = 'C' and  T.receipttype = 'NO' 
        //            //                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 367)
        //            //                                                and t.receiptid not in (select tr.receiptid
        //            //                                                                           from tbindents t  
        //            //                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
        //            //                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
        //            //                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
        //            //                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
        //            //                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
        //            //                                                                           where t.status = 'C' and t.issuetype in ('RS') )
        //            //                                                group by I.ItemID, T.PoNoID                       
        //            //                                                ) r1 on (r1.itemid =oi.itemid and r1.ponoid =op.ponoid) 

        //            //                                               left outer join
        //            //(
        //            //select so.ponoid,sum(nvl(s.totnetamount,0)) as totalpaid from soorderplaced so
        //            //inner join blpsanctions s on s.ponoid = so.ponoid
        //            //inner join blppayments pt on pt.paymentid = s.paymentid
        //            //where   pt.status = 'P' 
        //            //group by so.ponoid
        //            //)pt on pt.ponoid=op.ponoid 

        //            //                                              where 1=1 and op.AIFINYEAR between 542 and 546  
        //            //                                           " + whbdbudtgetid + @"
        //            //                                              group by mc.mcid,mc.MCATEGORY,op.pono,op.ponoid,op.soissuedate
        //            //                                              ,mi.itemcode,mi.itemid,itemname,mi.unit,c.finalrategst,r1.dhsrqty ,op.AIFINYEAR,totalpaid
        //            //                                              ) p
        //            //                                              inner join masaccyearsettings yr on yr.ACCYRSETID=p.AIFINYEAR
        //            //                                              group by AIFINYEAR,yr.ACCYEAR order by AIFINYEAR ";


        //            string qry = @" select ACCYRSETID as AIFINYEAR,ACCYEAR,count( distinct itemid) NOOFITEM,count(distinct ponoid) NOOFPO
        //,round(sum(POValue)/10000000,2) as POVALUE
        //,round(sum(nvl(POReceiptValue,0))/10000000,2) as RECVALUE
        //, case when round(sum(totnetamount)/10000000,2)>round(sum(nvl(POReceiptValue,0))/10000000,2) then round(sum(nvl(POReceiptValue,0))/10000000,2) else  round(sum(totnetamount)/10000000,2) end   as TOTALPAID

        //,round(sum(PipeLIvalue)/10000000,2) as PopipeLineValue

        //, case when  round(sum(BalPaymentRecv)/10000000,2)<0 then 0 else round(sum(BalPaymentRecv)/10000000,2) end   as Libility 
        //From (

        //select p.ACCYRSETID,ACCYEAR,
        //p.pono,p.soissuedate,i.ABSQTY as POQty,round(i.ABSQTY*c.finalrategst ,2) as POValue,POReceiptQty,round(nvl(POReceiptQty,0) *c.finalrategst,2) as POReceiptValue , round(nvl(pt.totnetamount,0),2) as totnetamount,P.ponoid
        //,(round(nvl(POReceiptQty,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  )  as BalPaymentRecv

        //,(round(nvl(i.ABSQTY,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  )  as BalPaymentPO,nvl(pp.PipeLIvalue,0) as PipeLIvalue 

        //,(case when m.nablreq = 'Y' and  round(sysdate-p.soissuedate,0) <= 150 then i.ABSQTY-nvl(POReceiptQty,0)
        // else case when m.ISSTERILITY = 'Y' and  round(sysdate-p.soissuedate,0) <= 100 then i.ABSQTY-nvl(POReceiptQty,0)
        //else case when p.extendeddate is null and round(sysdate-p.soissuedate,0) <= 90 then i.ABSQTY-nvl(POReceiptQty,0) 
        //else case when p.receiptdelayexception = 1 and sysdate <= p.extendeddate+1 then  i.ABSQTY-nvl(POReceiptQty,0) 
        //else case when p.extendeddate is not null and p.receiptdelayexception = 1 and  (p.extendeddate+1) <= p.soissuedate 
        //and round(sysdate-p.soissuedate,0) <= 90 then i.ABSQTY-nvl(POReceiptQty,0) else 0 end end end end end)  as Diff

        //,case when p.ACCYRSETID in (
        //(select ACCYRSETID from masaccyearsettings where sysdate between startdate and ENDDATE),(select max(ACCYRSETID) as ACCYRSETID from masaccyearsettings
        //where ACCYRSETID< (select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate ))  )
        //then 1 else case when nvl(POReceiptQty,0)>0 then 1 else 0 end   end  as DiifNew,m.itemid
        //from soorderplaced p
        //inner join soordereditems i on i.ponoid = p.ponoid
        //inner join masitems m on m.itemid=i.itemid
        //inner join aoccontractitems c on c.contractitemid = i.contractitemid
        // inner join masaccyearsettings yr on yr.ACCYRSETID=p.ACCYRSETID
        //inner join masbudget b on b.budgetid = p.budgetid 
        //left outer join  
        //(
        //select  t.ponoid, nvl(sum(tb.absrqty),0) as POReceiptQty                  
        //from tbreceipts t 
        //inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
        //inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
        // where T.Status = 'C' and  T.receipttype = 'NO' 
        // and t.ponoid in (select ponoid from soorderplaced where deptid = 367)
        // and t.receiptid not in (select tr.receiptid
        //from tbindents t  
        //inner join tbindentitems i on (i.indentid = t.indentid) 
        //inner join tboutwards o on (o.indentitemid =i.indentitemid) 
        //inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
        //inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
        //inner join tbreceipts tr on tr.receiptid = ti.receiptid
        //where t.status = 'C' and t.issuetype ='RS' )
        //group by I.ItemID, T.PoNoID                       
        //) r1 on ( r1.ponoid =p.ponoid) 

        //left outer join
        //(
        //select so.ponoid,sum(nvl(s.totnetamount,0)) as totnetamount from soorderplaced so
        //inner join blpsanctions s on s.ponoid = so.ponoid
        //inner join blppayments pt on pt.paymentid = s.paymentid
        //where   pt.status = 'P' 
        //group by so.ponoid
        //)pt on pt.ponoid=P.ponoid 

        // left outer join 
        // (
        // select ponoid,sum(pipelineQTY) pipelineQTY,FINALRATEGST,sum(pipelineQTY)*FINALRATEGST as PipeLIvalue from (
        //select  m.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
        //receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
        //case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
        //else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
        //else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
        //else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
        //else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end as pipelineQTY
        //,aci.FINALRATEGST
        //from   soOrderPlaced OP  
        //inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
        //inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
        //inner join masitems m on m.itemid = oi.itemid
        //inner join aoccontractitems aci on aci.contractitemid=OI.contractitemid
        //left outer join 
        //(
        //select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr 
        //inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
        //where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
        // and tr.receiptid not in (select tr.receiptid
        //                                                                           from tbindents t  
        //                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
        //                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
        //                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
        //                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
        //                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
        //                                                                           where t.status = 'C' and t.issuetype in ('RS') )
        //group by tr.ponoid,tri.itemid
        //) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid 
        // where op.status  in ('C','O') --and OP.soissuedate between '01-APR-2023' and '31-MAR-2024' --and m.itemcode = 'D196'


        // group by m.itemcode,m.nablreq,m.ISSTERILITY,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
        // op.soissuedate,op.extendeddate ,receiptdelayexception  ,aci.FINALRATEGST
        // having (case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
        // else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
        //else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
        //else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
        //else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate 
        //and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end) >0
        //)
        //group by ponoid,FINALRATEGST
        // )pp on pp.ponoid=P.ponoid

        //where 1=1  " + whbdbudtgetid + @"  and p.status not in ( 'OC','WA1','I' )  --and nvl(IsFullPayment,'N') ='N'
        //and p.soissuedate between '01-Apr-2022' and (select ENDDATE from masaccyearsettings where ACCYRSETID=545)

        // and (case when p.ACCYRSETID in (
        //(select ACCYRSETID from masaccyearsettings where sysdate between startdate and ENDDATE),(select max(ACCYRSETID) as ACCYRSETID from masaccyearsettings
        //where ACCYRSETID< (select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate ))  )
        //then 1 else case when nvl(POReceiptQty,0)>0 then 1 else 0 end   end)=1
        //group by p.pono,p.soissuedate,i.ABSQTY,p.ACCYRSETID,c.finalrategst,POReceiptQty,ACCYEAR,P.ponoid,pt.totnetamount,PP.PipeLIvalue,m.nablreq,m.ISSTERILITY,p.extendeddate ,p.receiptdelayexception,m.itemid
        //order by (round(nvl(POReceiptQty,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  ) 

        //) group by ACCYRSETID,ACCYEAR
        //order by ACCYEAR; ";



        //            var myList = _context.AIPO_ReceiptDbSet
        //           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //            return myList;

        //        }


        [HttpGet("PODetailsAgainstIndentYr")]
        public async Task<ActionResult<IEnumerable<AIPO_ReceiptDTO>>> PODetailsAgainstIndentYr(
     string bugetid,
     string yrid,
     string HOD)
        {
            string whbudtgetid = "";
            string whbdbudtgetid = "";
            if (bugetid != "0")
                whbdbudtgetid = " and op.BudgetID  =" + bugetid;
            string whyearid = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(this._context);
                whyearid = f.getACCYRSETID();
                f = (FacOperations)null;
            }
            else
                whyearid = yrid;
            string qry = "  select AIFINYEAR,yr.ACCYEAR,\r\n             count(distinct itemid) NoofItem,\r\n             count(distinct ponoid) as NoofPO,\r\n            round(sum(dhspovalue)/10000000,2) POValue,\r\n            round(sum(dhsrvalue)/10000000,2) RecValue\r\n            ,round(sum(totalpaid)/10000000,2) totalpaid\r\n            from (\r\n            select mc.mcid,mc.MCATEGORY,mi.itemcode,itemname,mi.unit,op.pono pono,op.ponoid,to_char( op.soissuedate,'dd/mm/yyyy') podate,\r\n                                        nvl(sum(od.dhsqty),0) dhspoqty,\r\n                                        nvl(sum(od.dhsqty),0)*c.finalrategst dhspovalue,\r\n                                        nvl(r1.dhsrqty,0) dhsrqty,nvl(r1.dhsrqty,0)*c.finalrategst dhsrvalue,                   \r\n                                        op.AIFINYEAR,mi.itemid,nvl(totalpaid,0) as totalpaid\r\n                                        from masItems MI\r\n                                                          inner join soOrderedItems OI on (OI.ItemID = MI.ItemID) \r\n                                                          inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' )) \r\n                                                          inner join soorderdistribution od on (od.orderitemid = oi.orderitemid) \r\n                                                          inner join aoccontractitems c on c.contractitemid = oi.contractitemid\r\n                                                          inner join masitemcategories ic on ic.categoryid = mi.categoryid\r\n                                                          inner join masitemmaincategory mc on mc.MCID=ic.MCID\r\n                                                          left outer join  \r\n                                                            (\r\n                                                            select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  \r\n                                                            from tbreceipts t \r\n                                                            inner join tbreceiptitems i on (i.receiptid = t.receiptid) \r\n                                                            inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)\r\n                                                            where T.Status = 'C' and  T.receipttype = 'NO' \r\n                                                            and t.ponoid in (select ponoid from soorderplaced where deptid = 367)\r\n                                                            and t.receiptid not in (select tr.receiptid\r\n                                                                                       from tbindents t  \r\n                                                                                       inner join tbindentitems i on (i.indentid = t.indentid) \r\n                                                                                       inner join tboutwards o on (o.indentitemid =i.indentitemid) \r\n                                                                                       inner join tbreceiptbatches tb on (tb.inwno = o.inwno)\r\n                                                                                       inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid\r\n                                                                                       inner join tbreceipts tr on tr.receiptid = ti.receiptid\r\n                                                                                       where t.status = 'C' and t.issuetype in ('RS') )\r\n                                                            group by I.ItemID, T.PoNoID                       \r\n                                                            ) r1 on (r1.itemid =oi.itemid and r1.ponoid =op.ponoid) \r\n\r\n                                                           left outer join\r\n            (\r\n            select so.ponoid,sum(nvl(s.totnetamount,0)) as totalpaid from soorderplaced so\r\n            inner join blpsanctions s on s.ponoid = so.ponoid\r\n            inner join blppayments pt on pt.paymentid = s.paymentid\r\n            where   pt.status = 'P' \r\n            group by so.ponoid\r\n            )pt on pt.ponoid=op.ponoid \r\n\r\n                                                          where 1=1 and op.AIFINYEAR between 542 and 546  \r\n                                                       " + whbdbudtgetid + "\r\n                                                          group by mc.mcid,mc.MCATEGORY,op.pono,op.ponoid,op.soissuedate\r\n                                                          ,mi.itemcode,mi.itemid,itemname,mi.unit,c.finalrategst,r1.dhsrqty ,op.AIFINYEAR,totalpaid\r\n                                                          ) p\r\n                                                          inner join masaccyearsettings yr on yr.ACCYRSETID=p.AIFINYEAR\r\n                                                          group by AIFINYEAR,yr.ACCYEAR order by AIFINYEAR ";
            List<AIPO_ReceiptDTO> myList = this._context.AIPO_ReceiptDbSet.FromSqlInterpolated<AIPO_ReceiptDTO>(FormattableStringFactory.Create(qry)).ToList<AIPO_ReceiptDTO>();
            ActionResult<IEnumerable<AIPO_ReceiptDTO>> actionResult = (ActionResult<IEnumerable<AIPO_ReceiptDTO>>)myList;
            whbudtgetid = (string)null;
            whbdbudtgetid = (string)null;
            whyearid = (string)null;
            qry = (string)null;
            myList = (List<AIPO_ReceiptDTO>)null;
            return actionResult;
        }


        [HttpGet("PaidYearwise_Budget")]

        public async Task<ActionResult<IEnumerable<PaidYearWiseDTO>>> PaidYearwise_Budget(string bugetid, string yrid)
        {
            string whbudtgetid = "";
            string whbdbudtgetid = "";
            if (bugetid != "0")
            {


                whbdbudtgetid = " and so.budgetid =" + bugetid;

            }
            string whyearid = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();

            }
            else
            {

                whyearid = yrid;
            }


            string qry = @" select pd.ACCYRSETID,yr.ACCYEAR,round(sum(amtpaid)/10000000,2) as AmountPaid ,count(distinct ponoid) NoofPO
from 
(
select (round(s.totnetamount,0))+(s.admincharges) as amtpaid,
(select ACCYRSETID from masaccyearsettings where bp.aiddate between STARTDATE and  ENDDATE) as ACCYRSETID
,so.ponoid
from blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid=s.ponoid
where 1=1  and bp.status='P' 
and bp.aiddate 
between (select STARTDATE from masaccyearsettings where ACCYRSETID=542) and (select ENDDATE from masaccyearsettings where ACCYRSETID=" + whyearid + @")
" + whbdbudtgetid + @"
)pd
inner join masaccyearsettings yr on yr.ACCYRSETID=pd.ACCYRSETID
group by pd.ACCYRSETID,yr.ACCYEAR
order by pd.ACCYRSETID ";



            var myList = _context.PaidYearWiseDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("GrossPaidDateWise")]

        public async Task<ActionResult<IEnumerable<PaidDateWiseDTO>>> GrossPaidDateWise(string rptype ,string bugetid, string fromdt, string todt)
        {
            string whdateclause = "";
            string whbdbudtgetid = "";
            
            if (bugetid != "0")
            {


                whbdbudtgetid = " and so.budgetid =" + bugetid;

            }

            if (fromdt != "0")
            {
                if (todt == "0")
                {
                    whdateclause = " and  bp.aiddate between '" + fromdt + "' and sydate";
                }
                else
                {
                    whdateclause = " and  bp.aiddate between '" + fromdt + "' and '" + todt + "'";
                }
            }
            string qry = "";
            if (rptype == "Fund")
            {
                 qry = @"select budgetid as ID,budgetname as Name, round(sum(amtpaid)/10000000,2) as AmountPaid ,count(distinct ponoid) NoofPO
from
(
select (round(s.totnetamount,0))+(s.admincharges) as amtpaid,so.ponoid,b.budgetid,b.budgetname
from blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid= s.ponoid
inner join MASBUDGET b on b.budgetid= so.budgetid and b.budgetid not in (3)
where 1=1  and bp.status='P'  and bp.aiddate>'01-Apr-2021'  " + whbdbudtgetid + @"
" + whdateclause + @"
)pd
group by budgetid, budgetname
order by round(sum(amtpaid)/10000000,2) desc ";
            }
            if (rptype == "Supplier")
            {
                qry = @"select SUPPLIERID as ID,SUPPLIERNAME as Name, round(sum(amtpaid)/10000000,2) as AmountPaid ,count(distinct ponoid) NoofPO
from 
(
select  (round(nvl(s.totnetamount,0)))+(nvl(s.admincharges,0)) as amtpaid,so.ponoid,sp.SUPPLIERID,sp.SUPPLIERNAME
from blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid=s.ponoid
inner join massuppliers sp on sp.SUPPLIERID=so.SUPPLIERID 
where 1=1  and bp.status='P' and bp.aiddate>'01-Apr-2021'  " + whbdbudtgetid + @"
" + whdateclause + @"
)pd
group by SUPPLIERID,SUPPLIERNAME
order by round(sum(amtpaid)/10000000,2) desc ";
            }
            var myList = _context.PaidDateWiseDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;


        }


        [HttpGet("GrossPaidDateWiseDetails")]

        public async Task<ActionResult<IEnumerable<GrossPaidDetails>>> GrossPaidDateWiseDetails(string bugetid, string fromdt, string todt, string supplierid, string yrid,string Indentyrid)
        {
            string whdateclause = "";
            string whbdbudtgetid = "";

            if (bugetid != "0")
            {


                whbdbudtgetid += " and so.budgetid =" + bugetid;

            }
            if (supplierid != "0")
            {


                whbdbudtgetid += " and so.supplierid =" + supplierid;

            }
            if (Indentyrid != "0")
            {
                whdateclause = " and so.AIFINYEAR=" + Indentyrid;
            }
            else
            {
                if (yrid == "0")
                {
                    if (fromdt != "0")
                    {
                        if (todt == "0")
                        {
                            whdateclause = " and  bp.aiddate between '" + fromdt + "' and sydate";
                        }
                        else
                        {
                            whdateclause = " and  bp.aiddate between '" + fromdt + "' and '" + todt + "'";
                        }
                    }
                }
                else
                {
                    whdateclause = " and  bp.aiddate between  (select STARTDATE from masaccyearsettings where ACCYRSETID=" + yrid + @") and (select ENDDATE from masaccyearsettings where ACCYRSETID=" + yrid + @")";
                }
            }
            string qry = @" select so.ponoid,m.itemcode,m.strength1,m.unit,m.itemname,aci.finalrategst,b.budgetname,sp.suppliername ,so.pono pono,to_char(so.soissuedate,'dd-mm-yyyy') podate
,    oi.absqty as orderedqty,round(oi.absqty*aci.finalrategst,0) as POvalue ,to_char(RECEIPTDATE,'dd-MM-yyyy') as RECEIPTDATE, nvl(r1.rqty,0) RQTY,round(nvl(r1.rqty,0)*aci.finalrategst,0) as RValue
, to_char(bp.aiddate,'dd-mm-yyyy') ChequeDT,
round(s.totnetamount,0) as GrossPaid ,(s.admincharges) as Admin
,bp.aidno as ChequeNo
,b.budgetid,sc.schemename,s.SANCTIONDATE
,(select ACCYEAR from masaccyearsettings where ACCYRSETID= so.aifinyear) as IndentYear
,pr.PROGRAM,nvl(so.FMRCODE,'-') as FMRCODE
from blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid= s.ponoid
left outer join  masprogram pr on pr.programid=so.programid
inner join masschemes sc on sc.schemeid=so.schemeid
inner join massuppliers sp on sp.supplierid=so.supplierid
inner join SoOrderedItems OI on OI.PoNoID=so.PoNoID
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems aci on aci.contractitemid=OI.contractitemid
inner join MASBUDGET b on b.budgetid= so.budgetid and b.budgetid not in (3)

inner join  
      (
      select  t.ponoid, nvl(sum(tb.absrqty),0) as rqty ,max(RECEIPTDATE) as RECEIPTDATE                 
      from tbreceipts t 
      inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
      inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
      where T.Status = 'C' and  T.receipttype = 'NO' 
      and t.ponoid in (select ponoid from soorderplaced where 1=1)
      and t.receiptid not in (select tr.receiptid
                                 from tbindents t  
                                 inner join tbindentitems i on (i.indentid = t.indentid) 
                                 inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                 inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                 inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                 inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                 where t.status = 'C' and t.issuetype in ('RS') )
      group by  T.PoNoID                       
      ) r1 on (r1.ponoid =so.ponoid) 

where 1=1  and bp.status='P'  and so.soissuedate>='01-Apr-2019' " + whdateclause + @"
"+ whbdbudtgetid + @"
order by bp.aiddate desc ";
            var myList = _context.GrossPaidDetailsDbSet
       .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }



            [HttpGet("Fund_Libilities")]

        public async Task<ActionResult<IEnumerable<YrLibDTO>>> Fund_Libilities(string bugetid)
        {
            string whdateclause = "";
            string whbdbudtgetid = ""; 
            string sobdbudtgetid = "";
            if (bugetid != "0")
            {


                whbdbudtgetid = " and P.budgetid =" + bugetid;
                sobdbudtgetid = " and so.budgetid =" + bugetid;

            }
     
            string qry = "";
            if (bugetid != "0")
            {
                //                qry = @" select ACCYRSETID as ID,ACCYEAR Name,count(ponoid) nospo,round(sum(Balance)/10000000,2) as Libvalue
                //from
                //(
                //select op.ACCYRSETID,yr.ACCYEAR,op.ponoid,
                //oi.absqty as  dhspoqty,
                //round(oi.absqty*c.finalrategst,0) dhspovalue,
                //nvl(r1.dhsrqty,0) dhsrqty,round(nvl(r1.dhsrqty,0)*c.finalrategst,0) dhsrvalue     ,nvl(amtpaid,0) as amtpaid    
                //,round(nvl(r1.dhsrqty,0)*c.finalrategst,0)-nvl(amtpaid,0)  as Balance
                // from soorderplaced op 
                // inner join masaccyearsettings yr on yr.ACCYRSETID=op.ACCYRSETID
                // inner join soOrderedItems OI on (OI.ponoid = op.ponoid) 
                //inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                //left outer join 
                //(
                //select (round(s.totnetamount,0)) as amtpaid,so.ponoid
                //from blppayments bp
                //inner join blpsanctions s on s.paymentid=bp.paymentid
                //inner join soorderplaced so on so.ponoid=s.ponoid
                //where 1=1  and bp.status='P' 
                //and bp.aiddate between  '01-Apr-2019' and Sysdate
                //" + sobdbudtgetid + @"
                //) pd on pd.ponoid=op.ponoid

                //left outer join  
                //(
                //select  t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  
                //from tbreceipts t 
                //inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
                //inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
                // where T.Status = 'C' and  T.receipttype = 'NO' 
                // and t.ponoid in (select ponoid from soorderplaced where deptid = 367)
                // and t.receiptid not in (select tr.receiptid
                //from tbindents t  
                //inner join tbindentitems i on (i.indentid = t.indentid) 
                //inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                //inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                //inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                //inner join tbreceipts tr on tr.receiptid = ti.receiptid
                //where t.status = 'C' and t.issuetype ='RS' )
                //group by I.ItemID, T.PoNoID                       
                //) r1 on ( r1.ponoid =op.ponoid) 

                //where 1=1  and round(nvl(r1.dhsrqty,0)*c.finalrategst,0)-nvl(amtpaid,0)   >1000 
                //and op.status not in ( 'OC','WA1','I' ) and op.soissuedate between   '01-Apr-2019' 
                // and Sysdate
                //" + whbdbudtgetid + @"
                //) group by ACCYRSETID,ACCYEAR order by ACCYRSETID ";


                qry = @" select ACCYRSETID as ID,ACCYEAR as Name,count(ponoid) nospo
,round(sum(POValue)/10000000,2) as POValue
,round(sum(nvl(POReceiptValue,0))/10000000,2) as ReceivedValue
,round(sum(PipeLIvalue)/10000000,2) as PopipeLineValue
, round(sum(totnetamount)/10000000,2)  as TotPaid

, round(sum(BalPaymentRecv)/10000000,2)  as Libility 
From (

select p.ACCYRSETID,ACCYEAR,
p.pono,p.soissuedate,i.ABSQTY as POQty,round(i.ABSQTY*c.finalrategst ,2) as POValue,POReceiptQty,round(nvl(POReceiptQty,0) *c.finalrategst,2) as POReceiptValue , round(nvl(pt.totnetamount,0),2) as totnetamount,P.ponoid
,(round(nvl(POReceiptQty,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  )  as BalPaymentRecv

,(round(nvl(i.ABSQTY,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  )  as BalPaymentPO,nvl(pp.PipeLIvalue,0) as PipeLIvalue 

,(case when m.nablreq = 'Y' and  round(sysdate-p.soissuedate,0) <= 150 then i.ABSQTY-nvl(POReceiptQty,0)
 else case when m.ISSTERILITY = 'Y' and  round(sysdate-p.soissuedate,0) <= 100 then i.ABSQTY-nvl(POReceiptQty,0)
else case when p.extendeddate is null and round(sysdate-p.soissuedate,0) <= 90 then i.ABSQTY-nvl(POReceiptQty,0) 
else case when p.receiptdelayexception = 1 and sysdate <= p.extendeddate+1 then  i.ABSQTY-nvl(POReceiptQty,0) 
else case when p.extendeddate is not null and p.receiptdelayexception = 1 and  (p.extendeddate+1) <= p.soissuedate 
and round(sysdate-p.soissuedate,0) <= 90 then i.ABSQTY-nvl(POReceiptQty,0) else 0 end end end end end)  as Diff

,case when p.ACCYRSETID in (
(select ACCYRSETID from masaccyearsettings where sysdate between startdate and ENDDATE),(select max(ACCYRSETID) as ACCYRSETID from masaccyearsettings
where ACCYRSETID< (select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate ))  )
then 1 else case when nvl(POReceiptQty,0)>0 then 1 else 0 end   end  as DiifNew
from soorderplaced p
inner join soordereditems i on i.ponoid = p.ponoid
inner join masitems m on m.itemid=i.itemid
inner join aoccontractitems c on c.contractitemid = i.contractitemid
 inner join masaccyearsettings yr on yr.ACCYRSETID=p.ACCYRSETID
inner join masbudget b on b.budgetid = p.budgetid 
left outer join  
(
select  t.ponoid, nvl(sum(tb.absrqty),0) as POReceiptQty                  
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
where t.status = 'C' and t.issuetype ='RS' )
group by I.ItemID, T.PoNoID                       
) r1 on ( r1.ponoid =p.ponoid) 

left outer join
(
select so.ponoid,sum(nvl(s.totnetamount,0)) as totnetamount from soorderplaced so
inner join blpsanctions s on s.ponoid = so.ponoid
inner join blppayments pt on pt.paymentid = s.paymentid
where   pt.status = 'P' 
group by so.ponoid
)pt on pt.ponoid=P.ponoid 

 left outer join 
 (
 select ponoid,sum(pipelineQTY) pipelineQTY,FINALRATEGST,sum(pipelineQTY)*FINALRATEGST as PipeLIvalue from (
select  m.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end as pipelineQTY
,aci.FINALRATEGST
from   soOrderPlaced OP  
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems aci on aci.contractitemid=OI.contractitemid
left outer join 
(
select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr 
inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid,tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid 
 where op.status  in ('C','O') --and OP.soissuedate between '01-APR-2023' and '31-MAR-2024' --and m.itemcode = 'D196'
 group by m.itemcode,m.nablreq,m.ISSTERILITY,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
 op.soissuedate,op.extendeddate ,receiptdelayexception  ,aci.FINALRATEGST
 having (case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
 else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate 
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end) >0
)
group by ponoid,FINALRATEGST
 )pp on pp.ponoid=P.ponoid

where 1=1 "+ whbdbudtgetid + @" and p.status not in ( 'OC','WA1','I' )  and nvl(IsFullPayment,'N') ='N'
and p.soissuedate between '01-Apr-2019' and sysdate
 
 and (case when p.ACCYRSETID in (
(select ACCYRSETID from masaccyearsettings where sysdate between startdate and ENDDATE),(select max(ACCYRSETID) as ACCYRSETID from masaccyearsettings
where ACCYRSETID< (select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate ))  )
then 1 else case when nvl(POReceiptQty,0)>0 then 1 else 0 end   end)=1
group by p.pono,p.soissuedate,i.ABSQTY,p.ACCYRSETID,c.finalrategst,POReceiptQty,ACCYEAR,P.ponoid,pt.totnetamount,PP.PipeLIvalue,m.nablreq,m.ISSTERILITY,p.extendeddate ,p.receiptdelayexception
order by (round(nvl(POReceiptQty,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  ) 

) group by ACCYRSETID,ACCYEAR
order by ACCYEAR";
            }
            else
            {
                //                qry = @" select budgetid as ID,BUDGETNAME as Name,count(ponoid) nospo,round(sum(Balance)/10000000,2) as Libvalue
                //from
                //(
                //select  b.budgetid,b.BUDGETNAME,op.ponoid,
                //oi.absqty as  dhspoqty,
                //round(oi.absqty*c.finalrategst,0) dhspovalue,
                //nvl(r1.dhsrqty,0) dhsrqty,round(nvl(r1.dhsrqty,0)*c.finalrategst,0) dhsrvalue     ,nvl(amtpaid,0) as amtpaid    
                //,round(nvl(r1.dhsrqty,0)*c.finalrategst,0)-nvl(amtpaid,0)  as Balance
                // from soorderplaced op 
                // inner join MASBUDGET b on b.budgetid=op.budgetid and   b.budgetid not in (3)
                // inner join masaccyearsettings yr on yr.ACCYRSETID=op.ACCYRSETID
                // inner join soOrderedItems OI on (OI.ponoid = op.ponoid) 
                //inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                //left outer join 
                //(
                //select (round(s.totnetamount,0)) as amtpaid,so.ponoid
                //from blppayments bp
                //inner join blpsanctions s on s.paymentid=bp.paymentid
                //inner join soorderplaced so on so.ponoid=s.ponoid
                //where 1=1  and bp.status='P' 
                //and bp.aiddate between  '01-Apr-2019' and  Sysdate
                //) pd on pd.ponoid=op.ponoid

                //left outer join  
                //(
                //select  t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  
                //from tbreceipts t 
                //inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
                //inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
                // where T.Status = 'C' and  T.receipttype = 'NO' 
                // and t.ponoid in (select ponoid from soorderplaced where 1=1)
                // and t.receiptid not in (select tr.receiptid
                //from tbindents t  
                //inner join tbindentitems i on (i.indentid = t.indentid) 
                //inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                //inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                //inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                //inner join tbreceipts tr on tr.receiptid = ti.receiptid
                //where t.status = 'C' and t.issuetype ='RS' )
                //group by I.ItemID, T.PoNoID                       
                //) r1 on ( r1.ponoid =op.ponoid) 

                //where 1=1  and round(nvl(r1.dhsrqty,0)*c.finalrategst,0)-nvl(amtpaid,0)   >1000 and op.status not in ( 'OC','WA1','I' ) and op.soissuedate between   '01-Apr-2019' 
                //and  Sysdate
                //) group by budgetid,BUDGETNAME order by budgetid ";




                qry = @"select budgetid as ID,BUDGETNAME as Name,count(ponoid) nospo
                                ,round(sum(POValue)/10000000,2) as POValue
                                ,round(sum(nvl(POReceiptValue,0))/10000000,2) as ReceivedValue
                                ,round(sum(PipeLIvalue)/10000000,2) as PopipeLineValue
                                , round(sum(totnetamount)/10000000,2)  as TotPaid
                             
                                , round(sum(BalPaymentRecv)/10000000,2)  as Libility 
                                From (

                                select p.ACCYRSETID,ACCYEAR,b.budgetid,b.BUDGETNAME,
                                p.pono,p.soissuedate,i.ABSQTY as POQty,round(i.ABSQTY*c.finalrategst ,2) as POValue,POReceiptQty,round(nvl(POReceiptQty,0) *c.finalrategst,2) as POReceiptValue , round(nvl(pt.totnetamount,0),2) as totnetamount,P.ponoid
                                ,(round(nvl(POReceiptQty,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  )  as BalPaymentRecv

                                ,(round(nvl(i.ABSQTY,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  )  as BalPaymentPO,nvl(pp.PipeLIvalue,0) as PipeLIvalue 

                                ,(case when m.nablreq = 'Y' and  round(sysdate-p.soissuedate,0) <= 150 then i.ABSQTY-nvl(POReceiptQty,0)
                                 else case when m.ISSTERILITY = 'Y' and  round(sysdate-p.soissuedate,0) <= 100 then i.ABSQTY-nvl(POReceiptQty,0)
                                else case when p.extendeddate is null and round(sysdate-p.soissuedate,0) <= 90 then i.ABSQTY-nvl(POReceiptQty,0) 
                                else case when p.receiptdelayexception = 1 and sysdate <= p.extendeddate+1 then  i.ABSQTY-nvl(POReceiptQty,0) 
                                else case when p.extendeddate is not null and p.receiptdelayexception = 1 and  (p.extendeddate+1) <= p.soissuedate 
                                and round(sysdate-p.soissuedate,0) <= 90 then i.ABSQTY-nvl(POReceiptQty,0) else 0 end end end end end)  as Diff

                                ,case when p.ACCYRSETID in (
                                (select ACCYRSETID from masaccyearsettings where sysdate between startdate and ENDDATE),(select max(ACCYRSETID) as ACCYRSETID from masaccyearsettings
                                where ACCYRSETID< (select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate ))  )
                                then 1 else case when nvl(POReceiptQty,0)>0 then 1 else 0 end   end  as DiifNew
                                from soorderplaced p
                                inner join soordereditems i on i.ponoid = p.ponoid
                                inner join masitems m on m.itemid=i.itemid
                                inner join aoccontractitems c on c.contractitemid = i.contractitemid
                                 inner join masaccyearsettings yr on yr.ACCYRSETID=p.ACCYRSETID
                                inner join masbudget b on b.budgetid = p.budgetid 
                                left outer join  
                                (
                                select  t.ponoid, nvl(sum(tb.absrqty),0) as POReceiptQty                  
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
                                where t.status = 'C' and t.issuetype ='RS' )
                                group by I.ItemID, T.PoNoID                       
                                ) r1 on ( r1.ponoid =p.ponoid) 

                                left outer join
                                (
                                select so.ponoid,sum(nvl(s.totnetamount,0)) as totnetamount from soorderplaced so
                                inner join blpsanctions s on s.ponoid = so.ponoid
                                inner join blppayments pt on pt.paymentid = s.paymentid
                                where   pt.status = 'P' 
                                group by so.ponoid
                                )pt on pt.ponoid=P.ponoid 

                                 left outer join 
                                 (
                                 select ponoid,sum(pipelineQTY) pipelineQTY,FINALRATEGST,sum(pipelineQTY)*FINALRATEGST as PipeLIvalue from (
                                select  m.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
                                receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
                                case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
                                else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
                                else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
                                else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
                                else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end as pipelineQTY
                                ,aci.FINALRATEGST
                                from   soOrderPlaced OP  
                                inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
                                inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
                                inner join masitems m on m.itemid = oi.itemid
                                inner join aoccontractitems aci on aci.contractitemid=OI.contractitemid
                                left outer join 
                                (
                                select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr 
                                inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
                                where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
                                group by tr.ponoid,tri.itemid
                                ) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid 
                                 where op.status  in ('C','O')
                                 group by m.itemcode,m.nablreq,m.ISSTERILITY,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
                                 op.soissuedate,op.extendeddate ,receiptdelayexception  ,aci.FINALRATEGST
                                 having (case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
                                 else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
                                else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
                                else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
                                else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate 
                                and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end) >0
                                )
                                group by ponoid,FINALRATEGST
                                 )pp on pp.ponoid=P.ponoid

                                where  p.status not in ( 'OC','WA1','I' )  and nvl(IsFullPayment,'N') ='N'
                                and p.soissuedate between '01-Apr-2019' and sysdate
 
                                 and (case when p.ACCYRSETID in (
                                (select ACCYRSETID from masaccyearsettings where sysdate between startdate and ENDDATE),(select max(ACCYRSETID) as ACCYRSETID from masaccyearsettings
                                where ACCYRSETID< (select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate ))  )
                                then 1 else case when nvl(POReceiptQty,0)>0 then 1 else 0 end   end)=1
                                group by p.pono,p.soissuedate,i.ABSQTY,p.ACCYRSETID,c.finalrategst,POReceiptQty,ACCYEAR,P.ponoid,pt.totnetamount,PP.PipeLIvalue,m.nablreq,m.ISSTERILITY,p.extendeddate ,p.receiptdelayexception,b.budgetid,b.BUDGETNAME
                                order by (round(nvl(POReceiptQty,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  ) 

                                ) group by budgetid,BUDGETNAME
                                order by budgetid";
            }
            
            var myList = _context.YrLibDbSet
       .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }
        [HttpGet("Pipeline_Libilities")]

        public async Task<ActionResult<IEnumerable<PiplineLibDTO>>> Pipeline_Libilities(string bugetid)
        {
            string whdateclause = "";
            string whbdbudtgetid = "";
        
            if (bugetid != "0")
            {


                whbdbudtgetid = " and op.budgetid =" + bugetid;
              

            }

            string qry = "";
        
                qry = @" select budgetid as ID,BUDGETNAME as Name,count(distinct itemid) as nositems,count(distinct ponoid) as nospo,round(sum(PipeLIvalue)/10000000,2) as PipeLIvalue
from 
(
 select budgetid,BUDGETNAME,itemid,ponoid,sum(pipelineQTY) pipelineQTY,FINALRATEGST,sum(pipelineQTY)*FINALRATEGST as PipeLIvalue 
 from (
select  m.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end as pipelineQTY
,aci.FINALRATEGST
,b.budgetid,b.BUDGETNAME
from   soOrderPlaced OP  
 inner join MASBUDGET b on b.budgetid=op.budgetid and   b.budgetid not in (3)
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems aci on aci.contractitemid=OI.contractitemid
left outer join 
(
select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr 
inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid,tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid 
 where op.status  in ('C','O')  "+ whbdbudtgetid + @"
 group by m.itemcode,m.nablreq,m.ISSTERILITY,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
 op.soissuedate,op.extendeddate ,receiptdelayexception  ,aci.FINALRATEGST
 ,b.budgetid,b.BUDGETNAME
 having (case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
 else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate 
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end) >0
)
group by ponoid,FINALRATEGST,budgetid,BUDGETNAME,itemid
) group by budgetid,BUDGETNAME
order by budgetid ";
          

            var myList = _context.PiplineLibDbSet
       .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpGet("Sanc_Cheque")]

        public async Task<ActionResult<IEnumerable<SanctionChequePrepSummaryDTO>>> Sanc_Cheque(string rptype, string bugetid)
        {
     
            string whbdbudtgetid = "";

            if (bugetid != "0")
            {


                whbdbudtgetid += " and so.budgetid =" + bugetid;

            }
            string qry = "";
            if (rptype == "Sanction")
            {
                 qry = @" select budgetname,round(sum(Sncamt)/10000000,2) as Sncamtcr,count(distinct ponoid) as nosPo,count(distinct supplierid) as nossupplier
from 
(

select (round(s.totnetamount,0)) as Sncamt,so.ponoid,s.SANCTIONDATE,so.pono,bp.aidno,bp.aiddate,bp.status,so.supplierid ,b.budgetname
from blpsanctions  s 
inner join soorderplaced so on so.ponoid=s.ponoid
inner join masbudget b on b.budgetid = so.budgetid 
left outer join blppayments bp on bp.paymentid=s.paymentid
where 1=1  and nvl(bp.status,'IN')='IN' 
 and so.soissuedate>='01-Apr-2019' and  s.SANCTIONDATE>='01-Apr-2019' 
" + whbdbudtgetid + @"  and (round(s.totnetamount,0))>0 and bp.aidno  is null
) group by budgetname  ";
            }
            else
            {
                qry = @"  select budgetname,round(sum(Sncamt)/10000000,2) as Sncamtcr,count(distinct ponoid) as nosPo,count(distinct supplierid) as nossupplier
from 
(

select (round(s.totnetamount,0)) as Sncamt,so.ponoid,s.SANCTIONDATE,so.pono,bp.aidno,bp.aiddate,bp.status,so.supplierid ,b.budgetname
from blpsanctions  s 
inner join soorderplaced so on so.ponoid=s.ponoid
inner join masbudget b on b.budgetid = so.budgetid 
inner join blppayments bp on bp.paymentid=s.paymentid
where 1=1  and nvl(bp.status,'IN')='IN' 
 and so.soissuedate>='01-Apr-2019' and  s.SANCTIONDATE>='01-Apr-2019' 
" + whbdbudtgetid + @" and (round(s.totnetamount,0))>0 and bp.aidno  is not null
)  group by budgetname  ";

            }

     
            var myList = _context.SanctionChequePrepSummaryDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

            [HttpGet("SanctionPrepDetails")]

        public async Task<ActionResult<IEnumerable<GrossPaidDetails>>> SanctionPrepDetails(string bugetid, string supplierid)
        {
            string whdateclause = "";
            string whbdbudtgetid = "";

            if (bugetid != "0")
            {


                whbdbudtgetid += " and so.budgetid =" + bugetid;

            }
            if (supplierid != "0")
            {


                whbdbudtgetid += " and so.supplierid =" + supplierid;

            }

            string qry = "";

            qry = @" select so.ponoid,m.itemcode,m.strength1,m.unit,m.itemname,aci.finalrategst,b.budgetname,sp.suppliername ,so.pono pono,to_char(so.soissuedate,'dd-mm-yyyy') podate
,    oi.absqty as orderedqty,round(oi.absqty*aci.finalrategst,0) as POvalue ,to_char(RECEIPTDATE,'dd-MM-yyyy') as RECEIPTDATE, nvl(r1.rqty,0) RQTY,round(nvl(r1.rqty,0)*aci.finalrategst,0) as RValue
, to_char(bp.aiddate,'dd-mm-yyyy') ChequeDT,
round(s.totnetamount,0) as GrossPaid ,(s.admincharges) as Admin
,bp.aidno as ChequeNo
,b.budgetid,sc.schemename,s.SANCTIONDATE
,(select ACCYEAR from masaccyearsettings where ACCYRSETID= so.aifinyear) as IndentYear
,pr.PROGRAM,nvl(so.FMRCODE,'-') as FMRCODE
from blpsanctions s 
inner join soorderplaced so on so.ponoid= s.ponoid
left outer join blppayments bp on bp.paymentid=s.paymentid
left outer join  masprogram pr on pr.programid=so.programid
inner join masschemes sc on sc.schemeid=so.schemeid
inner join massuppliers sp on sp.supplierid=so.supplierid
inner join SoOrderedItems OI on OI.PoNoID=so.PoNoID
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems aci on aci.contractitemid=OI.contractitemid
inner join MASBUDGET b on b.budgetid= so.budgetid and b.budgetid not in (3)

inner join 
      (
      select  t.ponoid, nvl(sum(tb.absrqty),0) as rqty ,max(RECEIPTDATE) as RECEIPTDATE                 
      from tbreceipts t 
      inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
      inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
      where T.Status = 'C' and  T.receipttype = 'NO' 
      and t.ponoid in (select ponoid from soorderplaced where 1=1)
      and t.receiptid not in (select tr.receiptid
                                 from tbindents t  
                                 inner join tbindentitems i on (i.indentid = t.indentid) 
                                 inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                 inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                 inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                 inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                 where t.status = 'C' and t.issuetype in ('RS') )
      group by  T.PoNoID                       
      ) r1 on (r1.ponoid =so.ponoid) 

where 1=1    and nvl(bp.status,'IN')='IN' 
 and so.soissuedate>='01-Apr-2019' and s.SANCTIONDATE is not null and  s.SANCTIONDATE>='01-Apr-2019' 
" + whbdbudtgetid + @" and (round(s.totnetamount,0))>0 and bp.aidno  is null
order by s.SANCTIONDATE ";
            var myList = _context.GrossPaidDetailsDbSet
       .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("ChequePrepDetails")]

        public async Task<ActionResult<IEnumerable<GrossPaidDetails>>> ChequePrepDetails(string bugetid, string supplierid)
        {
            string whdateclause = "";
            string whbdbudtgetid = "";

            if (bugetid != "0")
            {


                whbdbudtgetid += " and so.budgetid =" + bugetid;

            }
            if (supplierid != "0")
            {


                whbdbudtgetid += " and so.supplierid =" + supplierid;

            }
          
            string qry = @" select so.ponoid,m.itemcode,m.strength1,m.unit,m.itemname,aci.finalrategst,b.budgetname,sp.suppliername ,so.pono pono,to_char(so.soissuedate,'dd-mm-yyyy') podate
,    oi.absqty as orderedqty,round(oi.absqty*aci.finalrategst,0) as POvalue ,to_char(RECEIPTDATE,'dd-MM-yyyy') as RECEIPTDATE, nvl(r1.rqty,0) RQTY,round(nvl(r1.rqty,0)*aci.finalrategst,0) as RValue
, to_char(bp.aiddate,'dd-mm-yyyy') ChequeDT,
round(s.totnetamount,0) as GrossPaid ,(s.admincharges) as Admin
,bp.aidno as ChequeNo
,b.budgetid,sc.schemename,s.SANCTIONDATE
,(select ACCYEAR from masaccyearsettings where ACCYRSETID= so.aifinyear) as IndentYear
,pr.PROGRAM,nvl(so.FMRCODE,'-') as FMRCODE
from blppayments bp
inner join blpsanctions s on s.paymentid=bp.paymentid
inner join soorderplaced so on so.ponoid= s.ponoid
left outer join  masprogram pr on pr.programid=so.programid
inner join masschemes sc on sc.schemeid=so.schemeid
inner join massuppliers sp on sp.supplierid=so.supplierid
inner join SoOrderedItems OI on OI.PoNoID=so.PoNoID
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems aci on aci.contractitemid=OI.contractitemid
inner join MASBUDGET b on b.budgetid= so.budgetid and b.budgetid not in (3)

inner join  
      (
      select  t.ponoid, nvl(sum(tb.absrqty),0) as rqty ,max(RECEIPTDATE) as RECEIPTDATE                 
      from tbreceipts t 
      inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
      inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
      where T.Status = 'C' and  T.receipttype = 'NO' 
      and t.ponoid in (select ponoid from soorderplaced where 1=1)
      and t.receiptid not in (select tr.receiptid
                                 from tbindents t  
                                 inner join tbindentitems i on (i.indentid = t.indentid) 
                                 inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                 inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                 inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                 inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                 where t.status = 'C' and t.issuetype in ('RS') )
      group by  T.PoNoID                       
      ) r1 on (r1.ponoid =so.ponoid) 

where 1=1   and nvl(bp.status,'IN')='IN' 
 and so.soissuedate>='01-Apr-2019' and  s.SANCTIONDATE>='01-Apr-2019' 
" + whbdbudtgetid + @" and (round(s.totnetamount,0))>0 and bp.aidno  is not null
order by s.SANCTIONDATE desc ";
            var myList = _context.GrossPaidDetailsDbSet
       .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        //        [HttpGet("LibDetailsbasedOnYearID")]

        //        public async Task<ActionResult<IEnumerable<LibDetailsDTO>>> LibDetailsbasedOnYearID(string rptype,string yrid, string budgetid,string supplierid)
        //        {
        //            string whdateclause = "";
        //            string whbdbudtgetid = "";

        //            if (budgetid != "0")
        //            {


        //                whbdbudtgetid += " and P.BudgetID =" + budgetid;

        //            }
        //            if (supplierid != "0")
        //            {


        //                whbdbudtgetid += " and p.supplierid =" + supplierid;

        //            }
        //            if (yrid != "0")
        //            {
        //                whbdbudtgetid += " and p.ACCYRSETID="+ yrid;
        //            }
        //            string rpclause = "";
        //            if (rptype == "Year")
        //            {
        //                rpclause += " and p.ACCYRSETID=" + yrid;
        //            }
        //            else
        //            {
        //                rpclause += " and P.BudgetID =" + budgetid;
        //            }

        //            //            string qry = @" select so.ponoid,m.itemcode,m.strength1,m.unit,m.itemname,aci.finalrategst,b.budgetname,sp.suppliername ,so.pono pono,to_char(so.soissuedate,'dd-mm-yyyy') podate
        //            //,    oi.absqty as orderedqty,round(oi.absqty*aci.finalrategst,0) as POvalue ,to_char(RECEIPTDATE,'dd-MM-yyyy') as RECEIPTDATE, nvl(r1.rqty,0) RQTY,round(nvl(r1.rqty,0)*aci.finalrategst,0) as RValue
        //            //, to_char(bp.aiddate,'dd-mm-yyyy') ChequeDT,
        //            //round(s.totnetamount,0) as GrossPaid ,(s.admincharges) as Admin
        //            //,bp.aidno as ChequeNo
        //            //,b.budgetid,sc.schemename,s.SANCTIONDATE
        //            //,(select ACCYEAR from masaccyearsettings where ACCYRSETID= so.aifinyear) as IndentYear
        //            //,pr.PROGRAM,nvl(so.FMRCODE,'-') as FMRCODE
        //            //from blppayments bp
        //            //inner join blpsanctions s on s.paymentid=bp.paymentid
        //            //inner join soorderplaced so on so.ponoid= s.ponoid
        //            //left outer join  masprogram pr on pr.programid=so.programid
        //            //inner join masschemes sc on sc.schemeid=so.schemeid
        //            //inner join massuppliers sp on sp.supplierid=so.supplierid
        //            //inner join SoOrderedItems OI on OI.PoNoID=so.PoNoID
        //            //inner join masitems m on m.itemid = oi.itemid
        //            //inner join aoccontractitems aci on aci.contractitemid=OI.contractitemid
        //            //inner join MASBUDGET b on b.budgetid= so.budgetid and b.budgetid not in (3)

        //            //inner join  
        //            //      (
        //            //      select  t.ponoid, nvl(sum(tb.absrqty),0) as rqty ,max(RECEIPTDATE) as RECEIPTDATE                 
        //            //      from tbreceipts t 
        //            //      inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
        //            //      inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
        //            //      where T.Status = 'C' and  T.receipttype = 'NO' 
        //            //      and t.ponoid in (select ponoid from soorderplaced where 1=1)
        //            //      and t.receiptid not in (select tr.receiptid
        //            //                                 from tbindents t  
        //            //                                 inner join tbindentitems i on (i.indentid = t.indentid) 
        //            //                                 inner join tboutwards o on (o.indentitemid =i.indentitemid) 
        //            //                                 inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
        //            //                                 inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
        //            //                                 inner join tbreceipts tr on tr.receiptid = ti.receiptid
        //            //                                 where t.status = 'C' and t.issuetype in ('RS') )
        //            //      group by  T.PoNoID                       
        //            //      ) r1 on (r1.ponoid =so.ponoid) 

        //            //where 1=1   and nvl(bp.status,'IN')='IN' 
        //            // and so.soissuedate>='01-Apr-2019' and  s.SANCTIONDATE>='01-Apr-2019' 
        //            //" + whbdbudtgetid + @" and (round(s.totnetamount,0))>0 and bp.aidno  is not null
        //            //order by s.SANCTIONDATE desc ";

        //            string qry = @" 
        //select p.ponoid,m.itemcode,m.strength1,m.unit,m.itemname,c.finalrategst,b.budgetname,sp.suppliername,p.pono,to_char(p.soissuedate,'dd-mm-yyyy') podate,i.ABSQTY as orderedqty,round(i.ABSQTY*c.finalrategst ,2) as POValue,
        //to_char(RECEIPTDATE,'dd-MM-yyyy') as RECEIPTDATE,POReceiptQty as RQTY,round(nvl(POReceiptQty,0) *c.finalrategst,2) as RValue, round(nvl(pt.totnetamount,0),2) as GrossPaid,0 as Admin
        //,'-' as ChequeDT, '-' as ChequeNo,b.budgetid,sc.schemename,'' as SANCTIONDATE
        //,pr.PROGRAM,nvl(p.FMRCODE,'-') as FMRCODE,(select ACCYEAR from masaccyearsettings where ACCYRSETID= p.aifinyear) as IndentYear
        //,p.ACCYRSETID
        //,round((round(nvl(POReceiptQty,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  ),0)  as LibValueasPerREC
        //,round((round(nvl(POReceiptQty,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  )/100000,2)  as LibValueasPerRECLAcs
        //,(round(nvl(i.ABSQTY,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  )  as LibValueasPerPO,nvl(pp.PipeLIvalue,0) as PipeLIvalue 
        //,round(nvl(pp.PipeLIvalue,0)/100000,2) as PipeLIvalueLAcs

        //,(case when m.nablreq = 'Y' and  round(sysdate-p.soissuedate,0) <= 150 then i.ABSQTY-nvl(POReceiptQty,0)
        // else case when m.ISSTERILITY = 'Y' and  round(sysdate-p.soissuedate,0) <= 100 then i.ABSQTY-nvl(POReceiptQty,0)
        //else case when p.extendeddate is null and round(sysdate-p.soissuedate,0) <= 90 then i.ABSQTY-nvl(POReceiptQty,0) 
        //else case when p.receiptdelayexception = 1 and sysdate <= p.extendeddate+1 then  i.ABSQTY-nvl(POReceiptQty,0) 
        //else case when p.extendeddate is not null and p.receiptdelayexception = 1 and  (p.extendeddate+1) <= p.soissuedate 
        //and round(sysdate-p.soissuedate,0) <= 90 then i.ABSQTY-nvl(POReceiptQty,0) else 0 end end end end end)  as Diff

        //,case when p.ACCYRSETID in (
        //(select ACCYRSETID from masaccyearsettings where sysdate between startdate and ENDDATE),(select max(ACCYRSETID) as ACCYRSETID from masaccyearsettings
        //where ACCYRSETID< (select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate ))  )
        //then 1 else case when nvl(POReceiptQty,0)>0 then 1 else 0 end   end  as DiifNew
        //from soorderplaced p
        //left outer join  masprogram pr on pr.programid=p.programid
        //inner join masschemes sc on sc.schemeid=p.schemeid
        //inner join massuppliers sp on sp.supplierid=p.supplierid
        //inner join soordereditems i on i.ponoid = p.ponoid

        //inner join masitems m on m.itemid=i.itemid
        //inner join aoccontractitems c on c.contractitemid = i.contractitemid
        // inner join masaccyearsettings yr on yr.ACCYRSETID=p.ACCYRSETID
        //inner join masbudget b on b.budgetid = p.budgetid 
        //left outer join  
        //(
        //select  t.ponoid, nvl(sum(tb.absrqty),0) as POReceiptQty ,max(RECEIPTDATE) as RECEIPTDATE                  
        //from tbreceipts t 
        //inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
        //inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
        // where T.Status = 'C' and  T.receipttype = 'NO' 
        // and t.ponoid in (select ponoid from soorderplaced where deptid = 367)
        // and t.receiptid not in (select tr.receiptid
        //from tbindents t  
        //inner join tbindentitems i on (i.indentid = t.indentid) 
        //inner join tboutwards o on (o.indentitemid =i.indentitemid) 
        //inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
        //inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
        //inner join tbreceipts tr on tr.receiptid = ti.receiptid
        //where t.status = 'C' and t.issuetype ='RS' )
        //group by I.ItemID, T.PoNoID                       
        //) r1 on ( r1.ponoid =p.ponoid) 

        //left outer join
        //(
        //select so.ponoid,sum(nvl(s.totnetamount,0)) as totnetamount from soorderplaced so
        //inner join blpsanctions s on s.ponoid = so.ponoid
        //inner join blppayments pt on pt.paymentid = s.paymentid
        //where   pt.status = 'P' 
        //group by so.ponoid
        //)pt on pt.ponoid=P.ponoid 

        // left outer join 
        // (
        // select ponoid,sum(pipelineQTY) pipelineQTY,FINALRATEGST,sum(pipelineQTY)*FINALRATEGST as PipeLIvalue from (
        //select  m.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
        //receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
        //case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
        //else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
        //else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
        //else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
        //else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end as pipelineQTY
        //,aci.FINALRATEGST
        //from   soOrderPlaced OP  
        //inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
        //inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
        //inner join masitems m on m.itemid = oi.itemid
        //inner join aoccontractitems aci on aci.contractitemid=OI.contractitemid
        //left outer join 
        //(
        //select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr 
        //inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
        //where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
        //group by tr.ponoid,tri.itemid
        //) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid 
        // where op.status  in ('C','O') --and OP.soissuedate between '01-APR-2023' and '31-MAR-2024' --and m.itemcode = 'D196'
        // group by m.itemcode,m.nablreq,m.ISSTERILITY,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
        // op.soissuedate,op.extendeddate ,receiptdelayexception  ,aci.FINALRATEGST
        // having (case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
        // else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
        //else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
        //else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
        //else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate 
        //and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end) >0
        //)
        //group by ponoid,FINALRATEGST
        // )pp on pp.ponoid=P.ponoid

        //where 1=1 " + whbdbudtgetid + @" and p.status not in ( 'OC','WA1','I' )  and nvl(IsFullPayment,'N') ='N'
        //and p.soissuedate between '01-Apr-2019' and (select ENDDATE from masaccyearsettings where ACCYRSETID=545)

        // and (case when p.ACCYRSETID in (
        //(select ACCYRSETID from masaccyearsettings where sysdate between startdate and ENDDATE),(select max(ACCYRSETID) as ACCYRSETID from masaccyearsettings
        //where ACCYRSETID< (select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate ))  )
        //then 1 else case when nvl(POReceiptQty,0)>0 then 1 else 0 end   end)=1
        //"+ rpclause + @"
        //group by p.pono,p.soissuedate,i.ABSQTY,p.ACCYRSETID,c.finalrategst,POReceiptQty,ACCYEAR,P.ponoid,pt.totnetamount
        //,PP.PipeLIvalue,m.nablreq,m.ISSTERILITY,p.extendeddate ,p.receiptdelayexception,
        //p.ponoid,m.itemcode,m.strength1,m.unit,m.itemname,b.budgetname,sp.suppliername,RECEIPTDATE,b.budgetid,sc.schemename
        //,pr.PROGRAM,p.FMRCODE,p.aifinyear  ";
        //            var myList = _context.LibDetailsDbSet
        //       .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
        //            return myList;
        //        }



        [HttpGet("LibDetailsbasedOnYearID")]
        public async Task<ActionResult<IEnumerable<LibDetailsDTO>>> LibDetailsbasedOnYearID(
    string rptype,
    string yrid,
    string budgetid,
    string supplierid)
        {
            string whdateclause = "";
            string whbdbudtgetid = "";
            if (budgetid != "0")
                whbdbudtgetid = whbdbudtgetid + " and P.BudgetID =" + budgetid;
            if (supplierid != "0")
                whbdbudtgetid = whbdbudtgetid + " and p.supplierid =" + supplierid;
            if (yrid != "0")
                whbdbudtgetid = whbdbudtgetid + " and p.ACCYRSETID=" + yrid;
            string rpclause = "";
            rpclause = !(rptype == "Year") ? rpclause + " and P.BudgetID =" + budgetid : rpclause + " and p.ACCYRSETID=" + yrid;
            string qry = " \r\nselect p.ponoid,m.itemcode,m.strength1,m.unit,m.itemname,c.finalrategst,b.budgetname,sp.suppliername,p.pono,to_char(p.soissuedate,'dd-mm-yyyy') podate,i.ABSQTY as orderedqty,round(i.ABSQTY*c.finalrategst ,2) as POValue,\r\nto_char(RECEIPTDATE,'dd-MM-yyyy') as RECEIPTDATE,POReceiptQty as RQTY,round(nvl(POReceiptQty,0) *c.finalrategst,2) as RValue, round(nvl(pt.totnetamount,0),2) as GrossPaid,0 as Admin\r\n,'-' as ChequeDT, '-' as ChequeNo,b.budgetid,sc.schemename,'' as SANCTIONDATE\r\n,pr.PROGRAM,nvl(p.FMRCODE,'-') as FMRCODE,(select ACCYEAR from masaccyearsettings where ACCYRSETID= p.aifinyear) as IndentYear\r\n,p.ACCYRSETID\r\n,round((round(nvl(POReceiptQty,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  ),0)  as LibValueasPerREC\r\n,round((round(nvl(POReceiptQty,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  )/100000,2)  as LibValueasPerRECLAcs\r\n,(round(nvl(i.ABSQTY,0)*c.finalrategst ,2)-sum(nvl(pt.totnetamount,0))  )  as LibValueasPerPO,nvl(pp.PipeLIvalue,0) as PipeLIvalue \r\n,round(nvl(pp.PipeLIvalue,0)/100000,2) as PipeLIvalueLAcs\r\n\r\n,(case when m.nablreq = 'Y' and  round(sysdate-p.soissuedate,0) <= 150 then i.ABSQTY-nvl(POReceiptQty,0)\r\n else case when m.ISSTERILITY = 'Y' and  round(sysdate-p.soissuedate,0) <= 100 then i.ABSQTY-nvl(POReceiptQty,0)\r\nelse case when p.extendeddate is null and round(sysdate-p.soissuedate,0) <= 90 then i.ABSQTY-nvl(POReceiptQty,0) \r\nelse case when p.receiptdelayexception = 1 and sysdate <= p.extendeddate+1 then  i.ABSQTY-nvl(POReceiptQty,0) \r\nelse case when p.extendeddate is not null and p.receiptdelayexception = 1 and  (p.extendeddate+1) <= p.soissuedate \r\nand round(sysdate-p.soissuedate,0) <= 90 then i.ABSQTY-nvl(POReceiptQty,0) else 0 end end end end end)  as Diff\r\n\r\n,case when p.ACCYRSETID in (\r\n(select ACCYRSETID from masaccyearsettings where sysdate between startdate and ENDDATE),(select max(ACCYRSETID) as ACCYRSETID from masaccyearsettings\r\nwhere ACCYRSETID< (select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate ))  )\r\nthen 1 else case when nvl(POReceiptQty,0)>0 then 1 else 0 end   end  as DiifNew\r\nfrom soorderplaced p\r\nleft outer join  masprogram pr on pr.programid=p.programid\r\ninner join masschemes sc on sc.schemeid=p.schemeid\r\ninner join massuppliers sp on sp.supplierid=p.supplierid\r\ninner join soordereditems i on i.ponoid = p.ponoid\r\n\r\ninner join masitems m on m.itemid=i.itemid\r\ninner join aoccontractitems c on c.contractitemid = i.contractitemid\r\n inner join masaccyearsettings yr on yr.ACCYRSETID=p.ACCYRSETID\r\ninner join masbudget b on b.budgetid = p.budgetid \r\nleft outer join  \r\n(\r\nselect  t.ponoid, nvl(sum(tb.absrqty),0) as POReceiptQty ,max(RECEIPTDATE) as RECEIPTDATE                  \r\nfrom tbreceipts t \r\ninner join tbreceiptitems i on (i.receiptid = t.receiptid) \r\ninner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)\r\n where T.Status = 'C' and  T.receipttype = 'NO' \r\n and t.ponoid in (select ponoid from soorderplaced where deptid = 367)\r\n and t.receiptid not in (select tr.receiptid\r\nfrom tbindents t  \r\ninner join tbindentitems i on (i.indentid = t.indentid) \r\ninner join tboutwards o on (o.indentitemid =i.indentitemid) \r\ninner join tbreceiptbatches tb on (tb.inwno = o.inwno)\r\ninner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid\r\ninner join tbreceipts tr on tr.receiptid = ti.receiptid\r\nwhere t.status = 'C' and t.issuetype ='RS' )\r\ngroup by I.ItemID, T.PoNoID                       \r\n) r1 on ( r1.ponoid =p.ponoid) \r\n\r\nleft outer join\r\n(\r\nselect so.ponoid,sum(nvl(s.totnetamount,0)) as totnetamount from soorderplaced so\r\ninner join blpsanctions s on s.ponoid = so.ponoid\r\ninner join blppayments pt on pt.paymentid = s.paymentid\r\nwhere   pt.status = 'P' \r\ngroup by so.ponoid\r\n)pt on pt.ponoid=P.ponoid \r\n\r\n left outer join \r\n (\r\n select ponoid,sum(pipelineQTY) pipelineQTY,FINALRATEGST,sum(pipelineQTY)*FINALRATEGST as PipeLIvalue from (\r\nselect  m.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,\r\nreceiptdelayexception ,round(sysdate-op.soissuedate,0) as days,\r\ncase when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)\r\nelse case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)\r\nelse case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) \r\nelse case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) \r\nelse case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end as pipelineQTY\r\n,aci.FINALRATEGST\r\nfrom   soOrderPlaced OP  \r\ninner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID\r\ninner join soorderdistribution soi on soi.orderitemid=OI.orderitemid\r\ninner join masitems m on m.itemid = oi.itemid\r\ninner join aoccontractitems aci on aci.contractitemid=OI.contractitemid\r\nleft outer join \r\n(\r\nselect tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr \r\ninner join tbreceiptitems tri on tri.receiptid=tr.receiptid \r\nwhere tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null\r\ngroup by tr.ponoid,tri.itemid\r\n) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid \r\n where op.status  in ('C','O') \r\n group by m.itemcode,m.nablreq,m.ISSTERILITY,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,\r\n op.soissuedate,op.extendeddate ,receiptdelayexception  ,aci.FINALRATEGST\r\n having (case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)\r\n else case when m.ISSTERILITY = 'Y' and  round(sysdate-op.soissuedate,0) <= 100 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)\r\nelse case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) \r\nelse case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) \r\nelse case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate \r\nand round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end end) >0\r\n)\r\ngroup by ponoid,FINALRATEGST\r\n )pp on pp.ponoid=P.ponoid\r\n\r\nwhere 1=1 " + whbdbudtgetid + " and p.status not in ( 'OC','WA1','I' )  and nvl(IsFullPayment,'N') ='N'\r\nand p.soissuedate between '01-Apr-2019' and sysdate\r\n \r\n and (case when p.ACCYRSETID in (\r\n(select ACCYRSETID from masaccyearsettings where sysdate between startdate and ENDDATE),(select max(ACCYRSETID) as ACCYRSETID from masaccyearsettings\r\nwhere ACCYRSETID< (select ACCYRSETID from masaccyearsettings where sysdate between startdate and enddate ))  )\r\nthen 1 else case when nvl(POReceiptQty,0)>0 then 1 else 0 end   end)=1\r\n" + rpclause + "\r\ngroup by p.pono,p.soissuedate,i.ABSQTY,p.ACCYRSETID,c.finalrategst,POReceiptQty,ACCYEAR,P.ponoid,pt.totnetamount\r\n,PP.PipeLIvalue,m.nablreq,m.ISSTERILITY,p.extendeddate ,p.receiptdelayexception,\r\np.ponoid,m.itemcode,m.strength1,m.unit,m.itemname,b.budgetname,sp.suppliername,RECEIPTDATE,b.budgetid,sc.schemename\r\n,pr.PROGRAM,p.FMRCODE,p.aifinyear  ";
            List<LibDetailsDTO> myList = this._context.LibDetailsDbSet.FromSqlInterpolated<LibDetailsDTO>(FormattableStringFactory.Create(qry)).ToList<LibDetailsDTO>();
            ActionResult<IEnumerable<LibDetailsDTO>> actionResult = (ActionResult<IEnumerable<LibDetailsDTO>>)myList;
            whdateclause = (string)null;
            whbdbudtgetid = (string)null;
            rpclause = (string)null;
            qry = (string)null;
            myList = (List<LibDetailsDTO>)null;
            return actionResult;
        }


        [HttpGet("getFitUnfit")]
        public async Task<ActionResult<IEnumerable<getFitUnfitDTO>>> getFitUnfit(string fitunfit)
        {
            //string whMonth = "";
            string whfitunfit = "";


            if (fitunfit.ToLower() == "fit")
            {
                whfitunfit = " and  fitunfit='Fit'   ";

            }
            else if (fitunfit.ToLower() == "not fit")
            {
                whfitunfit = " and  fitunfit='Not Fit'   ";
            }





            string qry = @" select nvl(SECTIONNAME,'Technical') as SECTIONNAME,PresentFile ,budgetname as FundHead,suppliername,ACCYEAR as POYear,pono,PODATE,FMRCODE,PROGRAM,mcategory,itemcode,itemname,unit,strength1,POQTY,TotalPOvalue,receiptqty,receiptvalue,mrcdate,to_char(LastQCPaadedDT,'dd-MM-yyyy') as QCPassedDT,SDDate,fitunfit,validity,NSQStock,HoldStock,SupPenindSD,ponoid,itemid,supplierid
from 
(

select sc.SECTIONNAME,PresentFile,b.budgetname,b.budgetid ,x.supplierid,ponoid,paymentRequired,suppliername,mcid,mcategory,itemid,itemcode,itemname,unit,strength1,EDL21,EDL25,EDLType_2025,itemtypename,groupname,
ACCYEAR ,pono,PODATE,FMRCODE,POQTY as POQTY,TotalPOvalue,
receiptqty,mrcdate,LASTMRCDT,QCPass,LastQCPaadedDT,receiptvalue,libWithoutAdm,libWithAdm,ToRecePer,reasonName,hodtype,
fileno,filedt,Source,case when sdackuserdt is not null then  to_char(sdackuserdt,'dd-MM-yyyy') else 'Not Received' end as SDDate
,nvl(nsq.NSQStock,0) as NSQStock
,nvl(nsq.HoldStock,0) as HoldStock
,case when nvl(SupPenindSD,'Y') ='N' then 'Yes' else 'No' end  as SupPenindSD
 ,case when nvl(nsq.NSQStock,0)>0 or nvl(HoldStock,0)>0 or nvl(SupPenindSD,'Y')='N' then 'Not Fit' else fitunfit end as fitunfit1, fitunfit
 ,to_char(CASE when sdackuserdt !=null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and sdackuserdt>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end ,'dd-MM-yyyy')  as FitDate
 ,validity,PaymentStatus,PaymentDate,TAT_PO_Payment,TAT_Rec_Payment,TAT_QC_Payment,NotPaid_Rec_Since,NotPaid_QC_Since,HasFile,FileID,sdid ,touserid
,PROGRAM
from 
(
select op.ismanualpaid , op.supplierid,op.suppliername,mc.schemecode,m.itemcode,SUBSTR(m.itemname,1,30) as  itemname,m.unitcount,op.pono,to_char(op.PODATE,'dd-MM-yyyy') as podate,
so.FMRCODE,
POQTY, nvl(receiptqty,0) as receiptqty, case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end  as ToRecePer
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
 ,case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)=100
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
 case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)>10
  and  nvl(res.IsfitReason,0)=1 and  nvl(res.reasonid,0)=16 then 'Fit' else
 'Not Fit'
end end end end END as fitunfit
,nvl(res.reasonid,0) as reasonid
 ,paymentstatusNew,op.budgetid, op.PoNoID,sdackuserdt,sdid,qt.LastQCPaadedDT
 ,op.itemid,m.unit,m.strength1
 ,case when nvl(m.isedl2021,'N') ='Y' then 'Yes' else 'No' end as EDL21
,case when nvl(m.edl2025,'N') ='Y' then 'Yes' else 'No' end as EDL25
,case when m.EDL2025FLAG in ('DHS','CME','COMMON') then 'EDL' else 'NON-EDL' end as EDLType_2025
,mt.itemtypename,g.groupname,ACCYEAR,paymentstatusNew as PaymentStatus
,pd.PaymentDate, case when pd.PaymentDate is not null and paymentstatusNew='Paid' then round(pd.PaymentDate-op.PODATE) else 0 end as TAT_PO_Payment
,case when pd.PaymentDate is not null and op.lastmrcdt is not null and paymentstatusNew='Paid'  then round(pd.PaymentDate-op.lastmrcdt) else 0 end as TAT_Rec_Payment
,case when pd.PaymentDate is not null and op.lastmrcdt is not null and paymentstatusNew='Paid'  then round(pd.PaymentDate-qt.LastQCPaadedDT) else 0 end as TAT_QC_Payment
,case when pd.PaymentDate is not null and op.lastmrcdt is not null and paymentstatusNew='Not Paid'  then round(sysdate-op.lastmrcdt) else 0 end as NotPaid_Rec_Since
,case when pd.PaymentDate is not null and op.lastmrcdt is not null and paymentstatusNew='Not Paid'  then round(sysdate-qt.LastQCPaadedDT) else 0 end as NotPaid_QC_Since
,mc.mcid,mc.mcategory,mp.PROGRAM
from 
 v_popaymentstatus op
 inner join soorderplaced so on so.ponoid=op.ponoid and so.ponoid not in (52070,43626)
 left outer join masdepartment md on md.deptid=so.deptid
 left outer join masprogram mp on mp.PROGRAMID=so.PROGRAMID
 inner join masschemes mc on mc.schemeid=op.schemeid
 inner join masitems m on m.itemid=op.itemid
 inner join masitemcategories ci on ci.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.mcid=ci.mcid
 left outer join masitemtypes mt on mt.itemtypeid=m.itemtypeid
 left outer join masitemgroups g on g.groupid=m.groupid


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
select sdc.ponoid, sum(sdc.sdamount)sdamount,sd.sdid,sd.sdackuserdt  from sdsupmaster sd 
inner join  sdsupchild sdc on sdc.sdid = sd.sdid
where sd.status = 'C'  and sd.ishoreceipt='Y'
group by sdc.ponoid,sd.sdid,sd.sdackuserdt 
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

left outer join 
(
select p.aiddate as PaymentDate,max(s.ponoid) as pponoid from blpsanctions   s 
inner join blppayments p on p.paymentid=s.paymentid
where p.status='P'
group  by p.aiddate
) pd on pd.pponoid=op.ponoid
 where 1=1 and paymentstatusNew='Not Paid'  --and op.ponoid=60640

 )x 
 inner join   MASBUDGET  b on b.budgetid=x.budgetid
left outer join usrusers u on u.userid=x.touserid
left outer join massection sc on sc.SECTIONID=u.sectionid
left outer  join
(
          select SUPPLIERID, sum(RECOVERYAMOUNT) as RECOVERYAMOUNT, sum(NSQSTOCK) as NSQSTOCK, sum(HOLDSTOCK) as HOLDSTOCK from (

       select x.ponoid,x.supplierid ,nvl(np.TAXVALUE,0) as recoveryAmount,sum(NSQStock) as NSQStock,sum(HoldStock) as HoldStock from (
        select w.warehousename,mi.itemcode,mi.itemname,mi.strength1,mi.unit,nvl(b.absrqty,0) - nvl(iq.issueqty,0) as bal,
  mi.categoryid,mi.itemid,b.batchno,to_char(b.mfgdate,'dd-MM-yyyy') as mfgdate,to_char(b.expdate,'dd-MM-yyyy') as expdate,so.pono,to_char(so.podate,'dd-MM-yyyy') as podate,sp.suppliername 
  ,b.qastatus,b.Whissueblock,
  case when nvl(b.Whissueblock,0) =1 and b.qastatus not in (2) then 'Hold' else '' end  as HoldStatus  ,
  case when b.qastatus=1 then 'SQ' else case when b.qastatus=2 then 'NSQ' else 'Undefine'  end end  as QSStatus
  ,so.supplierid
  , case when b.Whissueblock =1 and qastatus not in (2) then nvl(b.absrqty,0) - nvl(iq.issueqty,0) else 0 end HoldStock
   , case when  qastatus in (2) then nvl(b.absrqty,0) - nvl(iq.issueqty,0) else 0 end NSQStock
  ,b.ponoid
  from tbreceiptbatches b 
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
                 inner join tbreceipts t on t.receiptid=i.receiptid
                 inner join masitems mi on mi.itemid=i.itemid
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid 
               left outer join  soorderplaced so on so.ponoid=b.ponoid
             left outer join  massuppliers sp on sp.supplierid=so.supplierid
                     left outer join
                 ( 
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty  
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' 
                   and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   group by tbi.itemid,tb.warehouseid,tbo.inwno 
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid 

                  Where  T.Status = 'C' 
                  and so.soissuedate >'01-APR-2022'
                  and b.expdate>sysdate
                --  and mi.QCTEST='Y' and (nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0
                    -- and qastatus  in (2)
                     )x
                             left outer join
                 (

                 select PONOID,sum(nvl(TAXVALUE,0)) as TAXVALUE from (

                 select t.taxtypeid,t.TAXTYPENAME,so.supplierid , bp.PONOID ,bp.TAXVALUE from BLPTAXOTHERPO bp 
                inner join blpsanctions s on s.sanctionid=bp.sanctionid
                inner join blppayments p on p.paymentid=s.paymentid
                inner join blptaxtypes t on t.taxtypeid=bp.taxtypeid
                inner join soorderplaced so on so.ponoid=bp.ponoid
                where p.status='P' --and t.TAXTYPEID= 257
                ) group by PONOID
                 )np on np.ponoid=x.ponoid

                     group by x.supplierid,x.ponoid,np.TAXVALUE
                     having (sum(NSQStock)>0 or sum(HoldStock)>0 ) and nvl(np.TAXVALUE,0)=0
                     ) group by SUPPLIERID

)nsq on nsq.supplierid=x.supplierid

 left outer join
 (


select distinct SUPPLIERID, ISHORECEIPT  as SupPenindSD 
from (

select so.supplierid, so.PONOID, so.PONO, so.SOISSUEDATE ,sdamount,nvl(ishoreceipt,'N') as ishoreceipt
,round(sysdate- nvl(so.extendeddate, so.SOISSUEDATE),0) PendingSDDays,r.ReceiptQTy
,case when round(sysdate-so.SOISSUEDATE,0)>120 and nvl(r.ReceiptQTy,0)=0 then 'False' else

'True'

end status
from soorderplaced so
left outer join
(
select sdc.ponoid, sum(sdc.sdamount)sdamount,sd.sdid,sd.ishoreceipt,sd.sdackuserdt 
from sdsupmaster sd 
inner join  sdsupchild sdc on sdc.sdid = sd.sdid
where sd.status = 'C'
and sd.ishoreceipt='Y'
group by sdc.ponoid,sd.sdid 
,sd.ishoreceipt,sd.sdackuserdt
)sd on sd.ponoid=so.ponoid
left outer join
(
select so.ponoid,sum(rb.absrqty) as ReceiptQTy from tbreceipts r 
inner join tbreceiptitems ri on ri.receiptid=r.receiptid
inner join tbreceiptbatches rb on rb.receiptitemid=ri.receiptitemid
inner join soorderplaced so on so.ponoid=rb.ponoid
where so.soissuedate>'01-APR-2022' and r.status='C'
and r.receipttype='NO'
group by so.ponoid
)r on r.ponoid=so.ponoid
where so.status not in ('WA1','OC','I','IN')
and so.soissuedate > '01-APR-2022'
and nvl(ishoreceipt,'N')='N'
and round(sysdate-so.SOISSUEDATE,0)>10
) where STATUS='True'
 )supSD on supSD.SUPPLIERID=x.supplierid
where 1=1--- and b.budgetid in (7,17,16,1,4)
)
where 1=1 and SUPPLIERNAME not in ('MOKSHIT CORPORATION')  "+ whfitunfit + @" 
order by SECTIONNAME

---(case when nvl(nsq.NSQStock,0)>0 or nvl(HoldStock,0)>0 or nvl(SupPenindSD,'Y')='N' then 'Not Fit' else fitunfit end)='Not Fit'""
 ";








            var myList = _context.getFitUnfitDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("getFitUnfitSummary")]
        public async Task<ActionResult<IEnumerable<getFitUnfitSummaryDTO>>> getFitUnfitSummary(string fitunfit)
        {
            //string whMonth = "";
            string whfitunfit = "";


            if (fitunfit.ToLower() == "fit")
            {
                whfitunfit = " and  fitunfit='Fit'   ";

            }
            else if (fitunfit.ToLower() == "not fit")
            {
                whfitunfit = " and  fitunfit='Not Fit'   ";
            }





            string qry = @" select SECTIONNAME,FundHead,count(ponoid) nosPO,round(sum(receiptvalue)/100000,2) as RecValueLAcs
from 
(

select nvl(SECTIONNAME,'Technical') as SECTIONNAME,PresentFile ,budgetname as FundHead,suppliername,ACCYEAR as POYear,pono,PODATE,FMRCODE,PROGRAM,mcategory,itemcode,itemname,unit,strength1,POQTY,TotalPOvalue,receiptqty,receiptvalue,mrcdate,to_char(LastQCPaadedDT,'dd-MM-yyyy') as QCPassedDT,SDDate,fitunfit,validity,NSQStock,HoldStock,SupPenindSD,ponoid,itemid,supplierid
from 
(

select sc.SECTIONNAME,PresentFile,b.budgetname,b.budgetid ,x.supplierid,ponoid,paymentRequired,suppliername,mcid,mcategory,itemid,itemcode,itemname,unit,strength1,EDL21,EDL25,EDLType_2025,itemtypename,groupname,
ACCYEAR ,pono,PODATE,FMRCODE,to_char(POQTY) as POQTY,TotalPOvalue,
receiptqty,mrcdate,LASTMRCDT,QCPass,LastQCPaadedDT,receiptvalue,libWithoutAdm,libWithAdm,ToRecePer,reasonName,hodtype,
fileno,filedt,Source,case when sdackuserdt is not null then  to_char(sdackuserdt,'dd-MM-yyyy') else 'Not Received' end as SDDate
,nvl(nsq.NSQStock,0) as NSQStock
,nvl(nsq.HoldStock,0) as HoldStock
,case when nvl(SupPenindSD,'Y') ='N' then 'Yes' else 'No' end  as SupPenindSD
 ,case when nvl(nsq.NSQStock,0)>0 or nvl(HoldStock,0)>0 or nvl(SupPenindSD,'Y')='N' then 'Not Fit' else fitunfit end as fitunfit1, fitunfit
 ,to_char(CASE when sdackuserdt !=null and sdackuserdt>LASTMRCDT and sdackuserdt>LastQCPaadedDT then sdackuserdt else case when LastQCPaadedDT>sdackuserdt and sdackuserdt>LASTMRCDT then LastQCPaadedDT else LASTMRCDT end end ,'dd-MM-yyyy')  as FitDate
 ,validity,PaymentStatus,PaymentDate,TAT_PO_Payment,TAT_Rec_Payment,TAT_QC_Payment,NotPaid_Rec_Since,NotPaid_QC_Since,HasFile,FileID,sdid ,touserid
,PROGRAM
from 
(
select op.ismanualpaid , op.supplierid,op.suppliername,mc.schemecode,m.itemcode,SUBSTR(m.itemname,1,30) as  itemname,m.unitcount,op.pono,to_char(op.PODATE,'dd-MM-yyyy') as podate,
so.FMRCODE,
POQTY, nvl(receiptqty,0) as receiptqty, case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end  as ToRecePer
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
 ,case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)=100
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
 case when nvl(receiptqty,0)=nvl(QCPass,0) and (case when nvl(receiptqty,0)>0 then  round(nvl(receiptqty,0)/POQTY*100,2) else 0 end)>10
  and  nvl(res.IsfitReason,0)=1 and  nvl(res.reasonid,0)=16 then 'Fit' else
 'Not Fit'
end end end end END as fitunfit
,nvl(res.reasonid,0) as reasonid
 ,paymentstatusNew,op.budgetid, op.PoNoID,sdackuserdt,sdid,qt.LastQCPaadedDT
 ,op.itemid,m.unit,m.strength1
 ,case when nvl(m.isedl2021,'N') ='Y' then 'Yes' else 'No' end as EDL21
,case when nvl(m.edl2025,'N') ='Y' then 'Yes' else 'No' end as EDL25
,case when m.EDL2025FLAG in ('DHS','CME','COMMON') then 'EDL' else 'NON-EDL' end as EDLType_2025
,mt.itemtypename,g.groupname,ACCYEAR,paymentstatusNew as PaymentStatus
,pd.PaymentDate, case when pd.PaymentDate is not null and paymentstatusNew='Paid' then round(pd.PaymentDate-op.PODATE) else 0 end as TAT_PO_Payment
,case when pd.PaymentDate is not null and op.lastmrcdt is not null and paymentstatusNew='Paid'  then round(pd.PaymentDate-op.lastmrcdt) else 0 end as TAT_Rec_Payment
,case when pd.PaymentDate is not null and op.lastmrcdt is not null and paymentstatusNew='Paid'  then round(pd.PaymentDate-qt.LastQCPaadedDT) else 0 end as TAT_QC_Payment
,case when pd.PaymentDate is not null and op.lastmrcdt is not null and paymentstatusNew='Not Paid'  then round(sysdate-op.lastmrcdt) else 0 end as NotPaid_Rec_Since
,case when pd.PaymentDate is not null and op.lastmrcdt is not null and paymentstatusNew='Not Paid'  then round(sysdate-qt.LastQCPaadedDT) else 0 end as NotPaid_QC_Since
,mc.mcid,mc.mcategory,mp.PROGRAM
from 
 v_popaymentstatus op
 inner join soorderplaced so on so.ponoid=op.ponoid and so.ponoid not in (52070,43626)
 left outer join masdepartment md on md.deptid=so.deptid
 left outer join masprogram mp on mp.PROGRAMID=so.PROGRAMID
 inner join masschemes mc on mc.schemeid=op.schemeid
 inner join masitems m on m.itemid=op.itemid
 inner join masitemcategories ci on ci.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.mcid=ci.mcid
 left outer join masitemtypes mt on mt.itemtypeid=m.itemtypeid
 left outer join masitemgroups g on g.groupid=m.groupid


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
select sdc.ponoid, sum(sdc.sdamount)sdamount,sd.sdid,sd.sdackuserdt  from sdsupmaster sd 
inner join  sdsupchild sdc on sdc.sdid = sd.sdid
where sd.status = 'C'  and sd.ishoreceipt='Y'
group by sdc.ponoid,sd.sdid,sd.sdackuserdt 
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

left outer join 
(
select p.aiddate as PaymentDate,max(s.ponoid) as pponoid from blpsanctions   s 
inner join blppayments p on p.paymentid=s.paymentid
where p.status='P'
group  by p.aiddate
) pd on pd.pponoid=op.ponoid
 where 1=1 and paymentstatusNew='Not Paid'  --and op.ponoid=60640

 )x 
 inner join   MASBUDGET  b on b.budgetid=x.budgetid
left outer join usrusers u on u.userid=x.touserid
left outer join massection sc on sc.SECTIONID=u.sectionid
left outer  join
(
          select SUPPLIERID, sum(RECOVERYAMOUNT) as RECOVERYAMOUNT, sum(NSQSTOCK) as NSQSTOCK, sum(HOLDSTOCK) as HOLDSTOCK from (

       select x.ponoid,x.supplierid ,nvl(np.TAXVALUE,0) as recoveryAmount,sum(NSQStock) as NSQStock,sum(HoldStock) as HoldStock from (
        select w.warehousename,mi.itemcode,mi.itemname,mi.strength1,mi.unit,nvl(b.absrqty,0) - nvl(iq.issueqty,0) as bal,
  mi.categoryid,mi.itemid,b.batchno,to_char(b.mfgdate,'dd-MM-yyyy') as mfgdate,to_char(b.expdate,'dd-MM-yyyy') as expdate,so.pono,to_char(so.podate,'dd-MM-yyyy') as podate,sp.suppliername 
  ,b.qastatus,b.Whissueblock,
  case when nvl(b.Whissueblock,0) =1 and b.qastatus not in (2) then 'Hold' else '' end  as HoldStatus  ,
  case when b.qastatus=1 then 'SQ' else case when b.qastatus=2 then 'NSQ' else 'Undefine'  end end  as QSStatus
  ,so.supplierid
  , case when b.Whissueblock =1 and qastatus not in (2) then nvl(b.absrqty,0) - nvl(iq.issueqty,0) else 0 end HoldStock
   , case when  qastatus in (2) then nvl(b.absrqty,0) - nvl(iq.issueqty,0) else 0 end NSQStock
  ,b.ponoid
  from tbreceiptbatches b 
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
                 inner join tbreceipts t on t.receiptid=i.receiptid
                 inner join masitems mi on mi.itemid=i.itemid
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid 
               left outer join  soorderplaced so on so.ponoid=b.ponoid
             left outer join  massuppliers sp on sp.supplierid=so.supplierid
                     left outer join
                 ( 
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty  
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' 
                   and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   group by tbi.itemid,tb.warehouseid,tbo.inwno 
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid 

                  Where  T.Status = 'C' 
                  and so.soissuedate >'01-APR-2022'
                  and b.expdate>sysdate
                --  and mi.QCTEST='Y' and (nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0
                    -- and qastatus  in (2)
                     )x
                             left outer join
                 (

                 select PONOID,sum(nvl(TAXVALUE,0)) as TAXVALUE from (

                 select t.taxtypeid,t.TAXTYPENAME,so.supplierid , bp.PONOID ,bp.TAXVALUE from BLPTAXOTHERPO bp 
                inner join blpsanctions s on s.sanctionid=bp.sanctionid
                inner join blppayments p on p.paymentid=s.paymentid
                inner join blptaxtypes t on t.taxtypeid=bp.taxtypeid
                inner join soorderplaced so on so.ponoid=bp.ponoid
                where p.status='P' --and t.TAXTYPEID= 257
                ) group by PONOID
                 )np on np.ponoid=x.ponoid

                     group by x.supplierid,x.ponoid,np.TAXVALUE
                     having (sum(NSQStock)>0 or sum(HoldStock)>0 ) and nvl(np.TAXVALUE,0)=0
                     ) group by SUPPLIERID

)nsq on nsq.supplierid=x.supplierid

 left outer join
 (


select distinct SUPPLIERID, ISHORECEIPT  as SupPenindSD 
from (

select so.supplierid, so.PONOID, so.PONO, so.SOISSUEDATE ,sdamount,nvl(ishoreceipt,'N') as ishoreceipt
,round(sysdate- nvl(so.extendeddate, so.SOISSUEDATE),0) PendingSDDays,r.ReceiptQTy
,case when round(sysdate-so.SOISSUEDATE,0)>120 and nvl(r.ReceiptQTy,0)=0 then 'False' else

'True'

end status
from soorderplaced so
left outer join
(
select sdc.ponoid, sum(sdc.sdamount)sdamount,sd.sdid,sd.ishoreceipt,sd.sdackuserdt 
from sdsupmaster sd 
inner join  sdsupchild sdc on sdc.sdid = sd.sdid
where sd.status = 'C'
and sd.ishoreceipt='Y'
group by sdc.ponoid,sd.sdid 
,sd.ishoreceipt,sd.sdackuserdt
)sd on sd.ponoid=so.ponoid
left outer join
(
select so.ponoid,sum(rb.absrqty) as ReceiptQTy from tbreceipts r 
inner join tbreceiptitems ri on ri.receiptid=r.receiptid
inner join tbreceiptbatches rb on rb.receiptitemid=ri.receiptitemid
inner join soorderplaced so on so.ponoid=rb.ponoid
where so.soissuedate>'01-APR-2022' and r.status='C'
and r.receipttype='NO'
group by so.ponoid
)r on r.ponoid=so.ponoid
where so.status not in ('WA1','OC','I','IN')
and so.soissuedate > '01-APR-2022'
and nvl(ishoreceipt,'N')='N'
and round(sysdate-so.SOISSUEDATE,0)>10
) where STATUS='True'
 )supSD on supSD.SUPPLIERID=x.supplierid
where 1=1--- and b.budgetid in (7,17,16,1,4)
)
where 1=1 and SUPPLIERNAME not in ('MOKSHIT CORPORATION') "+ whfitunfit + @" 
)  group by SECTIONNAME,FundHead
order by SECTIONNAME

---(case when nvl(nsq.NSQStock,0)>0 or nvl(HoldStock,0)>0 or nvl(SupPenindSD,'Y')='N' then 'Not Fit' else fitunfit end)='Not Fit'""
 ";








            var myList = _context.getFitUnfitSummaryDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

    }
}
