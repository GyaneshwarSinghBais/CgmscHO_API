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
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CgmscHO_API.HODTO;
using CgmscHO_API.DTO;
using CgmscHO_API.Utility;
using CgmscHO_API.MasterDTO;
//using Broadline.Controls;
//using CgmscHO_API.Utility;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {

        private readonly OraDbContext _context;

        public MasterController(OraDbContext context)
        {
            _context = context;
        }



        //HO PO Related
        [HttpGet("MasSupplier")]
        public async Task<ActionResult<IEnumerable<SuplierDTO>>> MasSupplier(bool Is2019)
        {
            string wh2019year = "";
            if (Is2019)
            {
                wh2019year = " and op.accyrsetid>=539 ";
            }
            string qry = @" select distinct s.supplierid,s.suppliername from soorderplaced op
inner join massuppliers s on s.supplierid=op.supplierid
where op.status in ('C','O') " + wh2019year + @"
order by s.suppliername";
            var myList = _context.SuplierDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("MasSupplierPipeline")]
        public async Task<ActionResult<IEnumerable<SuplierDTO>>> MasSupplierPipeline(string wh)
        {
         
            string whclause = "";
            string whsoiclause = "";
            if (wh!="0")
            {
                whclause = " and  tr.warehouseid ="+ wh;
                whsoiclause = " and soi.warehouseid =" + wh;
            }
            string qry = @" select suppliername,supplierid from
        (
        select distinct 
        s.suppliername,s.supplierid
from soOrderPlaced OP
inner join massuppliers s on s.supplierid=op.supplierid
inner join SoOrderedItems OI on OI.PoNoID = OP.PoNoID
inner join masitems m on m.itemid = oi.itemid
inner join sotranches t on t.ponoid= op.ponoid
inner join soorderdistribution soi on soi.orderitemid = OI.orderitemid    "+ whsoiclause + @" 
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid = tr.receiptid
where tr.receipttype = 'NO' and tr.status = 'C' and tr.notindpdmis is null and tri.notindpdmis is null   " + whclause + @"
group by tr.ponoid, tri.itemid
) rec on rec.ponoid = OP.PoNoID and rec.itemid = OI.itemid


 where op.status  in ('C', 'O')   
 group by  op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,op.pono,
 op.soissuedate, op.extendeddate , receiptdelayexception,s.suppliername,s.supplierid,m.nablreq 
 having(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end) > 0
) order by suppliername";
            var myList = _context.SuplierDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }




        [HttpGet("MasPOs")]
        public async Task<ActionResult<IEnumerable<MasPODTO>>> MasPOs(bool Is2019, string supplierid)
        {
            string wh2019year = "";
            if (Is2019)
            {
                wh2019year = " and op.accyrsetid>=539 ";
            }
            string whSupplier = "";
            if (supplierid != "0")

            {
                whSupplier = " and op.supplierid=" + supplierid;
            }
            string qry = @" select  op.ponoid,op.pono||'/'||to_char(op.soissuedate,'dd-MM-yyyy') as name from soorderplaced op
where 1=1 " + whSupplier + @" and  op.status in ('C','O') " + wh2019year + @"
order by op.soissuedate desc
";
            var myList = _context.MasPODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;


        }

        [HttpGet("MasWH")]
        public async Task<ActionResult<IEnumerable<WHDTO>>> MasWH(bool allwh)
        {
            string qry = "";

            if (allwh)
            {
                qry = @" select WAREHOUSENAME||'('||GET_WHDistrict(WAREHOUSEID)||')' WAREHOUSENAME, WAREHOUSEID,Zoneid, 0 as userid,WAREHOUSENAME || '' as ONLYWHNAME  from maswarehouses
union all
select 'All' as WAREHOUSENAME ,1 as WAREHOUSEID,0 as Zoneid , 0 as userid, 'All' as ONLYWHNAME from dual
order by zoneid";
            }
            else
            {
                qry = @" select WAREHOUSENAME||'('||GET_WHDistrict(WAREHOUSEID)||')' WAREHOUSENAME, WAREHOUSEID,Zoneid, 0 as userid,WAREHOUSENAME || '' as ONLYWHNAME  from maswarehouses order by zoneid ";
            }
            var myList = _context.WHDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("MasWHParticular")]
        public async Task<ActionResult<IEnumerable<WHDTO>>> MasWHParticular(bool allwh, string whid)
        {
            string qry = "";

            string whwarehosueid = "";
            if (whid != "0")
            {
                whwarehosueid = " and WAREHOUSEID = " + whid;
            }

            if (allwh)
            {
                qry = @" select WAREHOUSENAME||'('||GET_WHDistrict(WAREHOUSEID)||')' WAREHOUSENAME, WAREHOUSEID,Zoneid, 'All' as ONLYWHNAME,WAREHOUSEID as USERID from maswarehouses
union all
select 'All' as WAREHOUSENAME ,1 as WAREHOUSEID,0 as Zoneid, 'All' as ONLYWHNAME,1 as USERID from dual
order by zoneid";
            }
            else
            {
                qry = @" select WAREHOUSENAME||'('||GET_WHDistrict(WAREHOUSEID)||')' WAREHOUSENAME, WAREHOUSEID,Zoneid,'All' as ONLYWHNAME,WAREHOUSEID as USERID from maswarehouses where 1=1 " + whwarehosueid + " order by zoneid ";
            }
            var myList = _context.WHDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("MasWHInfo")]
        public async Task<ActionResult<IEnumerable<MasWHInfoDTO>>> MasWHInfo()
        {

            string qry = @"  select WAREHOUSEID,WAREHOUSENAME,AMNAME,MOB1,w.amid,w.ADDRESS1,w.ADDRESS2,w.ADDRESS3,w.ZIP,w.EMAIL from maswarehouses w
left outer join masamgr a on a.amid=w.amid
left outer join maszone Z on z.zoneid=w.zoneid
order by z.zoneid,WAREHOUSEID ";
            var myList = _context.MasWHInfoDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

      //  async Task<ActionResult<IEnumerable<MasItemsDTO>>>
        [HttpGet("MasDistrict")]
        public async Task<ActionResult<IEnumerable<DistrictDTOWH>>>MasDistrict(bool allDist, string whid, string distid, string userid, string coll_cmho)
        {
            FacOperations f = new FacOperations(_context);
            string qry = "";

            string whdistid = "";
            if (distid != "0")
            {
                whdistid = " and  d.districtid= " + whid;
            }

            string whWarehouseId = "";

            if (coll_cmho != "0")
            {
                if (coll_cmho == "Coll")
                {
                    whdistid = " and  d.districtid = " + f.getCollectorDisid(userid);
                    distid = f.getCollectorDisid(userid);
                }

                if (coll_cmho == "CMHO")
                {
                    whdistid = " and  d.districtid = " + f.getCollectorDisid(userid);
                    distid = f.getCollectorDisid(userid);
                    // whdistid = " and  d.districtid = "+ Convert.ToString(f.geFACTDistrictid(userid));
                }

            }


            if (whid != "0")
            {
                whWarehouseId = " and  w.warehouseid= " + whid;
            }
            else if (whid == "0")
            {
                
            }
            else
            {
                whWarehouseId = " and  w.warehouseid= " + f.getWarehousefromDistrict(distid);
            }


            if (allDist)
            {


                qry = @" select districtname,distname,districtid,warehouseid,userid,emailid
from 
(
select d.districtname||'(WH:'||w.warehousename||')' as districtname,d.districtname distname ,d.districtid,d.warehouseid,u.userid,u.emailid from masdistricts d
inner join maswarehouses w on w.warehouseid=d.warehouseid
inner join usrusers u on u.districtid=d.districtid and u.roleid=482
where d.stateid=1 " + whWarehouseId + @"
union all
select 'All' as distictname,'All' as distname ,1 as districtid,1 as warehouseid,0 as userid,'-' as emailid from dual
)
order by warehouseid,districtid ";
            }
            else
            {
                qry = @" select   d.districtname||'(WH:'||w.warehousename||')' as districtname ,d.districtname as distname , d.districtid, d.warehouseid,u.userid,u.emailid from masdistricts d
inner join maswarehouses w on w.warehouseid= d.warehouseid
inner join usrusers u on u.districtid=d.districtid and u.roleid=482
where d.stateid= 1 " + whWarehouseId + @" " + whdistid + @" order by d.districtname ";
            }
            var myList = _context.DistrictDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("MasitemsfromIndent")]
        public async Task<ActionResult<IEnumerable<MasItemsDTO>>> MasitemsfromIndent( string mcid)
        {
            //            string qry = @" select mi.itemid, mi.itemname||'-'|| mi.itemcode as name, mi.itemcode, mi.itemname, mi.strength1, mi.unit, mi.unitcount, g.groupid, g.groupname, ty.ITEMTYPEID, ty.ITEMTYPENAME
            //, e.edlcat, e.edl,case when mi.isedl2021= 'Y' then 'EDL' else 'Non EDL' end as EDLtype
            //from masitems mi
            //left outer join masedl e on e.edlcat= mi.edlcat
            // inner join masitemcategories c on c.categoryid= mi.categoryid
            //inner join masitemmaincategory mc on mc.MCID= c.MCID
            //left outer join masitemtypes ty on ty.ITEMTYPEID= mi.ITEMTYPEID
            //left outer join masitemgroups g on g.groupid= mi.groupid
            //where 1=1 " + whclauseItem + @" and mi.ISFREEZ_ITPR is null " + whclauseItem + @"
            //order by mi.itemname ";

            string qry = @"  select mi.itemid, mi.itemname || '-' || mi.strength1 || '-' || mi.itemcode || ' ' || (case when mi.categoryid = 52 then g.groupname else '' end)  as name, mi.itemcode, mi.itemname, mi.strength1, mi.unit, mi.unitcount, g.groupid, g.groupname, 
ty.ITEMTYPEID, ty.ITEMTYPENAME
, e.edlcat, e.edl,case when mi.isedl2021 = 'Y' then 'EDL' else 'Non EDL' end as EDLtype
from masitems mi
inner
join
(
select itemid,case when i.isaireturn = 'Y' then 1 else case when i.ISAIRETURN_DME = 'Y' then 1 else 0 end end as AIReturn from
itemindent i  where 1 = 1
and(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0) + nvl(DME_INDENTQTY, 0)) > 0 and
(case when i.isaireturn = 'Y' then 1 else case when i.ISAIRETURN_DME = 'Y' then 1 else 0 end end)= 0
 and accyrsetid = (select accyrsetid from masaccyearsettings where  sysdate BETWEEN startdate and enddate) 
group by  itemid,i.isaireturn,i.ISAIRETURN_DME
) ai on ai.itemid = mi.itemid

left outer join masedl e on e.edlcat = mi.edlcat
 inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join masitemtypes ty on ty.ITEMTYPEID = mi.ITEMTYPEID
left outer join masitemgroups g on g.groupid = mi.groupid
where 1 = 1 and mi.ISFREEZ_ITPR is null and mc.MCID = "+ mcid + @"
order by mi.itemname ";
            var myList = _context.MasItemsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpGet("masddlUser")]
        public async Task<ActionResult<IEnumerable<UseriDDLDTO>>> masddlUser(string Usertype)
        {
            string qry = "";
            if (Usertype == "HOD")
            {
                qry = @" select firstname ||' ' ||lastname as textfield,u.userid,u.emailid,firstname,lastname,u.orderid, h.footer1 as SIDesig,h.FOOTER2 SIName,nvl(h.FOOTER3,'6263007758') as SIMobile,'-' as DEPEMAIL,r.rolename,r.roleid,0 as warehouseid,0 as districtid,0 as HimisDistrictid,0 as  BLID from usrusers u
inner join usrroles r on r.roleid=u.roleid
left outer join masfacheaderfooter h on h.userid=u.userid
where r.ISHOUSER='Y'
order by orderid ";
            }
            else if (Usertype == "WH")
            {
                qry = @" select w.WAREHOUSENAME as textfield,u.userid,u.STATUS,u.emailid,firstname,lastname,u.orderid,'Assistant Manager ' as SIDesig,am.AMNAME as  SIName,nvl(am.MOB1,'6263007758') as SIMobile
,w.EMAIL as DEPEMAIL ,ur.rolename,ur.roleid,w.warehouseid,0 as districtid,0 as HimisDistrictid,w.BLID as BLID
from usrusers u
inner join maswarehouses w on w.warehouseid=u.warehouseid
inner join masamgr am on am.amid=w.amid and am.ISACTIVE='Y'
 inner join usrroles ur on ur.roleid=u.roleid
left outer join masfacheaderfooter h on h.userid=u.userid
where  ur.roleid  in (437) and u.userid not in (11575)
order by w.zoneid ";
            }
            else if (Usertype == "DC")
            {
                qry = @" select d.districtname as textfield,u.userid,u.STATUS,u.emailid,firstname,lastname,u.orderid,'District Collector' as SIDesig,h.footer2 as SIName,
 nvl(h.FOOTER3, u.DEPMOBILE)  SIMobile
,'' as DEPEMAIL ,ur.rolename,ur.roleid, d.warehouseid,d.districtid,d.HimisDistrictid,0 as  BLID
from usrusers u
inner join masdistricts d on d.districtid = u.districtid
 inner join usrroles ur on ur.roleid = u.roleid
left outer join masfacheaderfooter h on h.userid = u.userid
where ur.roleid  in (482)
order by d.districtname ";
                    }

            else
            {
                qry = @" select firstname ||' ' ||lastname as textfield,u.userid,u.STATUS,u.emailid,firstname,lastname,u.orderid, h.footer1 as SIDesig,h.FOOTER2 SIName,case when h.FOOTER3 is null then to_char(DEPMOBILE) else to_char(h.FOOTER3) end as SIMobile
,u.DEPEMAIL ,r.rolename,r.roleid,0 as warehouseid ,0 as districtid,0 as HimisDistrictid,0 as  BLID
from usrusers u
inner join usrroles r on r.roleid=u.roleid
left outer join masfacheaderfooter h on h.userid=u.userid
where r.ISSectionUser='Y' and u.userid not in (3031,3041,2931)
and u.status='A'
order by orderid ";
            }
           
            var myList = _context.UseriDDLDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpGet("Masitems")]
        public async Task<ActionResult<IEnumerable<MasItemsDTO>>> Masitems(string itemid, string mcid, string edl, string groupid, string itemtypeid, string edlcat)
        {
            string whclauseItem = "";
            string whclause = "";
            if (itemid != "0")
            {
                whclauseItem = " and mi.itemid=" + itemid;
            }
            else
            {
                if (mcid != "0")
                {
                    whclause += " and mc.MCID =" + mcid;
                }
                if (edl == "Y")
                {
                    whclause += " and (case when mi.isedl2021= 'Y' then 'EDL' else 'Non EDL' end)='EDL'";
                }
                else if (edl == "N")
                {
                    whclause += " and (case when mi.isedl2021= 'Y' then 'EDL' else 'Non EDL' end)='Non EDL'";
                }
                else
                {

                }
                if (groupid != "0")
                {
                    whclause += " and g.groupid=" + groupid;
                }
                if (itemtypeid != "0")
                {
                    whclause += " and ty.ITEMTYPEID=" + itemtypeid;
                }
                if (edlcat != "N")
                {
                    whclause += " and e.edlcat=" + edlcat;
                }
            }
            //string whSupplier = "";

            string qry = @" select mi.itemid, mi.itemname||'-'|| mi.itemcode as name, mi.itemcode, mi.itemname, mi.strength1, mi.unit, mi.unitcount, g.groupid, g.groupname, ty.ITEMTYPEID, ty.ITEMTYPENAME
, e.edlcat, e.edl,case when mi.isedl2021= 'Y' then 'EDL' else 'Non EDL' end as EDLtype
from masitems mi
left outer join masedl e on e.edlcat= mi.edlcat
 inner join masitemcategories c on c.categoryid= mi.categoryid
inner join masitemmaincategory mc on mc.MCID= c.MCID
left outer join masitemtypes ty on ty.ITEMTYPEID= mi.ITEMTYPEID
left outer join masitemgroups g on g.groupid= mi.groupid
where 1=1 " + whclauseItem + @" and mi.ISFREEZ_ITPR is null " + whclauseItem + @"
order by mi.itemname ";
            var myList = _context.MasItemsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }




        [HttpGet("MasfacilityInfo")]
        public async Task<ActionResult<IEnumerable<FacilityMCDTO>>> MasfacilityInfo(string hod, string disid, string factypeid, string whid, string facid)
        {
            string whclause = ""; string whDistrict = ""; string whfactypeid = ""; string whwarehouseid = "";
            string whfacinfo = "";
            string whOrderBy = " order by f.districtid,f.facilitytypeid ";

            if (facid != "0")
            {
                whfacinfo = " and  f.facilityid =" + facid;
            }
            else
            {


                if (hod == "364")
                {
                    whclause = " and  f.facilitytypeid in ('364','378') ";
                }
                else if (hod == "371")
                {
                    whclause = " and  f.facilitytypeid in ('371') ";
                }
                else if (hod == "367")
                {
                    whclause = " and  f.facilitytypeid not in ('364','378','371') ";
                }
                else
                {
                    whclause = "  ";
                    whOrderBy = " order by ft.orderdp ";
                }

                if (disid != "0")
                {
                    whDistrict = " and  f.districtid =" + disid;
                }

                if (factypeid != "0")
                {
                    whfactypeid = " and  f.facilitytypeid =" + factypeid;
                }

                if (whid != "0")
                {
                    whwarehouseid = " and  fw.warehouseid =" + whid;
                }
            }


            string qry = @"  select f.FACILITYID, f.FACILITYNAME,h.footer1 as SIDesig,h.FOOTER2 SIName,h.FOOTER3 as SIMobile,h.DRNAME as HospitalIncharge,h.DRMOBILE as HIMobile,h.EMAIL as email,u.EMAILID as userid
,fw.warehouseid
from masfacilities f
inner join masfacilitywh fw on fw.facilityid=f.facilityid
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
left outer join usrusers u on u.facilityid=f.facilityid
left outer join masfacheaderfooter h on h.userid=u.userid
where  1=1  " + whfacinfo + @" " + whclause + @"  " + whDistrict + @" " + whfactypeid + @"  " + whwarehouseid + @"  
and f.isactive=1
order by f.districtid,f.facilitytypeid ";
            var myList = _context.FacilityMCDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("MasfacilityInfoUser")]
        public async Task<ActionResult<IEnumerable<FacilityMCDTO>>> MasfacilityInfoUser(string hod, string disid, string factypeid, string whid, string facid, string userid, string coll_cmho)
        {
            string whclause = ""; string whDistrict = ""; string whfactypeid = ""; string whwarehouseid = "";
            string whfacinfo = "";
            string whOrderBy = " order by f.districtid,f.facilitytypeid ";
            string districtidcol = "";
            FacOperations f = new FacOperations(_context);
            if (facid != "0")
            {
                whfacinfo = " and  f.facilityid =" + facid;
            }
            else
            {

                if (coll_cmho == "Collector")

                {
                    districtidcol = f.getCollectorDisid(userid);
                    whDistrict = " and  f.districtid =" + districtidcol;
                }
                if (coll_cmho == "CMHO")

                {
                    districtidcol = Convert.ToString(f.geFACTDistrictid(userid));
                    whDistrict = " and  f.districtid =" + districtidcol;
                }

                if (hod == "364")
                {
                    whclause = " and  f.facilitytypeid in ('364','378') ";
                }
                else if (hod == "371")
                {
                    whclause = " and  f.facilitytypeid in ('371') ";
                }
                else if (hod == "367")
                {
                    whclause = " and  f.facilitytypeid not in ('364','378','371') ";
                }
                else
                {
                    whclause = "  ";
                    whOrderBy = " order by ft.orderdp ";
                }

                if (disid != "0")
                {
                    whDistrict = " and  f.districtid =" + disid;
                }

                if (factypeid != "0")
                {
                    whfactypeid = " and  f.facilitytypeid =" + factypeid;
                }

                if (whid != "0")
                {
                    whwarehouseid = " and  fw.warehouseid =" + whid;
                }
            }


            string qry = @"  select f.FACILITYID, f.FACILITYNAME,h.footer1 as SIDesig,h.FOOTER2 SIName,h.FOOTER3 as SIMobile,h.DRNAME as HospitalIncharge,h.DRMOBILE as HIMobile,h.EMAIL as email,u.EMAILID as userid
,fw.warehouseid
from masfacilities f
inner join masfacilitywh fw on fw.facilityid=f.facilityid
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
left outer join usrusers u on u.facilityid=f.facilityid
left outer join masfacheaderfooter h on h.userid=u.userid
where  1=1  " + whfacinfo + @" " + whclause + @"  " + whDistrict + @" " + whfactypeid + @"  " + whwarehouseid + @"  
and f.isactive=1
order by f.districtid,f.facilitytypeid ";
            var myList = _context.FacilityMCDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpGet("getGroupName")]
        public async Task<ActionResult<IEnumerable<AllGroupNameDTO>>> getGroupName(bool AllGroup, string mcid)
        {
            string whMcid = "";
            string qry = "";

            if (mcid != "0")
            {
                whMcid = " and mc.mcid  =" + mcid;
            }

            if (AllGroup)
            {
                qry = @"     select  groupid, groupname from
     (
     select distinct g.groupid, g.groupname from masitemgroups g 
     inner join masitems m on m.groupid = g.groupid
      inner join masitemcategories c on c.categoryid=m.categoryid
     inner join masitemmaincategory mc on mc.MCID=c.MCID 
    where 1=1 " + whMcid + @"
     UNION ALL
     select 1 as groupid, 'ALL' as groupname from dual
     )
     order by groupid ";
            }
            else
            {
                qry = @"     select distinct g.groupid, g.groupname from masitemgroups g 
     inner join masitems m on m.groupid = g.groupid
      inner join masitemcategories c on c.categoryid=m.categoryid
     inner join masitemmaincategory mc on mc.MCID=c.MCID 
     where 1=1 " + whMcid + @"
     order by g.groupname ";
            }

            var myList = _context.AllGroupNameDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("MasRecRemarks")]
        public async Task<ActionResult<IEnumerable<MasRecRemarksDTO>>> MasRecRemarks(string whid,string whsup)
        {
            FacOperations f = new FacOperations(_context);
            string qry = "";

            string whwarehouseid = "";
            if (whid != "0")
            {
                whwarehouseid = "  and   REMENTERBY in  ('B','W')";
            }
            else if (whsup == "SUP")
            {
                whwarehouseid = "  and   REMENTERBY in  ('B','S')";
            }
            else
            {

            }
            qry = @"  select REMID,REMARKS from MasRecRemarks where 1=1  "+ whwarehouseid+ @" order by REMID ";

            var myList = _context.MasRecRemarksDbSet
          .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("MasRecRemarksWithIssues")]
        public async Task<ActionResult<IEnumerable<MasRecRemarksDTO>>> MasRecRemarksWithIssues(bool AllGroup ,string whid, string whsup,string userid)
        {
            FacOperations f = new FacOperations(_context);
            string qry = "";

            string whwarehouseid = "";
            string whclause = "";
            if (whsup == "WH")
            {
                whwarehouseid = "  and   r.REMENTERBY in  ('B','W')";
                whclause = @" and prg.WAREHOUSEID =  "+ whid;
            }
            else if (whsup == "SUP")
            {
                whwarehouseid = "  and   r.REMENTERBY in  ('B','S')";
               
            }
            else
            {

            }
           string  struserid = "";
            if (userid == "2926")
            {
                struserid = userid;
            }

            if (AllGroup)
            {
                qry = @" select REMID,REMARKS from 
(
 select  r.REMID,to_char(r.REMARKS)  as REMARKS from  MasRecRemarks r
left outer join TBLRECVPROGRESS prg on prg.REMID=r.REMID
where  prg.LATFLAG='Y'   " + whclause + @" " + whwarehouseid + @"
group by r.REMID,r.REMARKS
)a

  UNION ALL
     select 0  as REMID, 'ALL' as REMARKS from dual
order by REMID ";
            }
            else
            {
                qry = @" select  r.REMID,r.REMARKS||'-'||to_char(count(distinct PROGRESSID)) as REMARKS from  MasRecRemarks r
left outer join TBLRECVPROGRESS prg on prg.REMID=r.REMID
where  prg.LATFLAG='Y' " + whclause + @" " + whwarehouseid + @"
group by r.REMID,r.REMARKS
order by count(PROGRESSID) desc";

            }
            

            var myList = _context.MasRecRemarksDbSet
          .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("MasCouierLogin")]
        public async Task<ActionResult<IEnumerable<WHDTO>>> MasCouierLogin(bool allwh, string whid)
        {
            bool v = allwh;
            string wwhid = whid;
            string qry = "";
            //            qry = @" select WAREHOUSENAME WAREHOUSENAME, WAREHOUSEID,Zoneid from maswarehouses
            //union all
            //select 'HO-QC' as WAREHOUSENAME ,2931 as WAREHOUSEID,0 as Zoneid from dual
            //union all
            //select 'Head Office' as WAREHOUSENAME ,4474 as WAREHOUSEID,0 as Zoneid from dual
            //order by zoneid";

            qry = @" select userid, WAREHOUSENAME, WAREHOUSEID, ZONEID, 'All' as ONLYWHNAME from (
select  u.userid,  wh.WAREHOUSENAME, wh.WAREHOUSEID,wh.Zoneid from maswarehouses wh
inner join usrusers u on u.warehouseid = wh.warehouseid
union
select userid, case when USERID=2931 then 'HO-QC' else case when USERID=4474 then 'Head Office' else  'COURIER ADMIN' end end as WAREHOUSENAME,USERID as WAREHOUSEID,0 as Zoneid from usrusers where   userid in (11575)  --4474 2931,
) order by ZONEID ";

            var myList = _context.WHDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }
    }
}
