// Decompiled with JetBrains decompiler
// Type: CgmscHO_API.Models.OraDbContext
// Assembly: CgmscHO_API, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 61EB3058-7353-4D44-A0DF-AA01F6D1EC87
// Assembly location: D:\CGMSCHO_API_04-Apr-2025\CgmscHO_API.dll

using CgmscHO_API.DirectorateDTO;
using CgmscHO_API.Models;
using CgmscHO_API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardDME : ControllerBase
    {
        private readonly OraDbContext _context;

        public DashboardDME(OraDbContext context) => this._context = context;

        [HttpGet("DMEAIvsIssue")]
        public async Task<ActionResult<IEnumerable<AIvsIssueDTO>>> DMEAIvsIssue(
          string mcid,
          string yearid)
        {
            string whyearid = "";
            string whereyearid = "";
            string whmcid = " ";
            if (mcid != "0")
                whmcid = " and mc.mcid =" + mcid;
            string yid = "";

            if (yearid == "0")
            {
                FacOperations f = new FacOperations(this._context);
                whyearid = f.getACCYRSETID();
                whereyearid = " and  tb.indentdate between ( select startdate from masaccyearsettings where accyrsetid=" + whyearid + " ) and ( select enddate from masaccyearsettings where accyrsetid=" + whyearid + " )  ";
                yid = " and ma.accyrsetid =" + whyearid;
                f = (FacOperations)null;
            }
            else
            {
                whereyearid = " and  tb.indentdate between ( select startdate from masaccyearsettings where accyrsetid=539 ) and ( select enddate from masaccyearsettings where accyrsetid=" + whyearid + " )  ";
                yid = " and ma.accyrsetid >= 539 ";
            }
            string qry = "";
            qry = "  select ACCYEAR,accyrsetid,count(itemid) as nosIndent,sum(AIReturn) as AIReturn,sum(issueitems) as issueitems,round(sum(ISSValue)/10000000,2) as IssuedValuecr\r\nfrom \r\n(\r\nselect m.itemid,case when i.ISAIRETURN_DME ='Y' then 1 else 0 end as AIReturn,nvl(IssueQtyInLakh,0) as IssueQtyInLakh ,nvl(issueqty,0) issueqty,nvl(ISSValue,0) as ISSValue\r\n, case when  nvl(issueqty,0) >0 then 1 else 0 end  as issueitems,\r\ni.accyrsetid,ma.ACCYEAR\r\nfrom  masItems m \r\ninner join itemindent i on i.itemid=m.itemid\r\ninner join masaccyearsettings ma on ma.accyrsetid=i.accyrsetid\r\n inner join masitemcategories c on c.categoryid=m.categoryid\r\ninner join masitemmaincategory  mc on mc.mcid=c.mcid\r\n\r\n\r\n        left outer join\r\n                                             (\r\n                                              select  IssueYearID,itemid,unitcount,round((sum(issueqty)*unitcount)/100000,2) as IssueQtyInLakh\r\n                                              ,sum(issueqty) as issueqty\r\n                                            ,sum(nvl(ISSValue,0)) as ISSValue from \r\n                                            (\r\n\r\n   select tbi.itemid,tbo.inwno  ,(select ACCYRSETID from masaccyearsettings where tb.indentdate between STARTDATE and ENDDATE) as  IssueYearID\r\n\r\n   ,(tbo.issueqty) as IssueQty,(nvl(tbo.issueqty,0)*aci.finalrategst) ISSValue,m.unitcount\r\n\r\n                                             from tbindents tb\r\n                                             inner join tbindentitems tbi on tbi.indentid=tb.indentid \r\n                                              inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid\r\n                                              inner join tbreceiptbatches rb on rb.inwno=tbo.inwno\r\n                                              inner join soordereditems si on si.ponoid=rb.ponoid and si.itemid=tbi.itemid\r\n                                              inner join aoccontractitems aci on aci.contractitemid=si.contractitemid\r\n                                              inner join masfacilities f on f.facilityid = tb.facilityid\r\n                                              inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid\r\n                                              inner join masitems m on m.itemid=tbi.itemid\r\n                                             where tb.Status = 'C' and tb.issuetype='NO'   and ft.hodid=3                                        \r\n                                            " + whereyearid + "\r\n                                              ) where 1=1  group by itemid,unitcount,IssueYearID\r\n\r\n                                             )ISS on ISS.itemid=m.itemid and ISS.IssueYearID=i.accyrsetid\r\n\r\n\r\n  where nvl(i.dme_indentqty,0) >0 \r\n " + yid + "\r\n  and m.isfreez_itpr is null  " + whmcid + "\r\n  ) group by accyrsetid,ACCYEAR\r\n  order by accyrsetid desc ";
            List<AIvsIssueDTO> myList = this._context.AIvsIssueDbSet.FromSqlInterpolated<AIvsIssueDTO>(FormattableStringFactory.Create(qry)).ToList<AIvsIssueDTO>();
            ActionResult<IEnumerable<AIvsIssueDTO>> actionResult = (ActionResult<IEnumerable<AIvsIssueDTO>>)myList;
            whyearid = (string)null;
            whereyearid = (string)null;
            whmcid = (string)null;
            yid = (string)null;
            qry = null;
            myList = (List<AIvsIssueDTO>)null;
            return actionResult;
        }

        [HttpGet("DMEIssueWihtoutAI")]
        public async Task<ActionResult<IEnumerable<DirectorateWithoutAIDTO>>> DMEIssueWihtoutAI(
          string mcid,
          string yearid)
        {
            string whyearid = "";
            string whereyearid = "";
            string whmcid = " ";
            if (mcid != "0")
                whmcid = " and mc.mcid =" + mcid;
            string yid = "";

            FacOperations f = new FacOperations(this._context);
            whyearid = f.getACCYRSETID();

            if (yearid == "0")
            {
               
                whereyearid = " and  tb.indentdate between ( select startdate from masaccyearsettings where accyrsetid=" + whyearid + " ) and ( select enddate from masaccyearsettings where accyrsetid=" + whyearid + " )  ";
                yid = " and accyrsetid =" + whyearid;
                f = (FacOperations)null;
            }
            else
            {
                 whyearid = f.getACCYRSETID();
                whereyearid = " and  tb.indentdate between ( select startdate from masaccyearsettings where accyrsetid=539 ) and ( select enddate from masaccyearsettings where accyrsetid=" + whyearid + " )  ";
                yid = " and accyrsetid >= 539 ";
            }
            string qry = "";
            qry = " select ACCYEAR,IssueYearID,count(itemid) as nositemsissued,round(sum(ISSValue)/10000000,2) as IssuedValuecr\r\nfrom \r\n(\r\n       \r\n       \r\n       select ma.ACCYEAR, IssueYearID,a.itemid,a.unitcount,round((sum(issueqty)*unitcount)/100000,2) as IssueQtyInLakh\r\n                                              ,sum(issueqty) as issueqty\r\n                                            ,sum(nvl(ISSValue,0)) as ISSValue\r\n                                            ,nvl(dmai,0) as dmai\r\n                                            from \r\n                                            (\r\n\r\n   select tbi.itemid,tbo.inwno  ,(select ACCYRSETID from masaccyearsettings where tb.indentdate between STARTDATE and ENDDATE) as  IssueYearID\r\n\r\n   ,(tbo.issueqty) as IssueQty,(nvl(tbo.issueqty,0)*aci.finalrategst) ISSValue,m.unitcount\r\n\r\n                                             from tbindents tb\r\n                                             inner join tbindentitems tbi on tbi.indentid=tb.indentid \r\n                                              inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid\r\n                                              inner join tbreceiptbatches rb on rb.inwno=tbo.inwno\r\n                                              inner join soordereditems si on si.ponoid=rb.ponoid and si.itemid=tbi.itemid\r\n                                              inner join aoccontractitems aci on aci.contractitemid=si.contractitemid\r\n                                              inner join masfacilities f on f.facilityid = tb.facilityid\r\n                                              inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid\r\n                                              inner join masitems m on m.itemid=tbi.itemid\r\n                                  inner join masitemcategories c on c.categoryid=m.categoryid\r\ninner join masitemmaincategory  mc on mc.mcid=c.mcid\r\n                                              \r\n                                             where tb.Status = 'C' and tb.issuetype='NO'   and ft.hodid=3 " + whmcid + "\r\n                                        \r\n" + whereyearid + "\r\n\r\n                                              ) a\r\n                                       left outer join \r\n                                              (\r\n                                            select  i.itemid,nvl(i.dme_indentqty,0) dmai,i.accyrsetid from itemindent i  where 1= 1 " + yid + "\r\n                                              ) i on i.itemid=a.itemid and i.accyrsetid=a.IssueYearID\r\n                                              inner join masaccyearsettings ma on ma.accyrsetid=a.IssueYearID\r\n                                              where 1=1 and nvl(dmai,0)=0  \r\n                                              \r\n                                              group by a.itemid,a.unitcount,IssueYearID,dmai,ma.ACCYEAR\r\n                                                ) group by IssueYearID,ACCYEAR\r\n  order by IssueYearID desc ";
            List<DirectorateWithoutAIDTO> myList = this._context.DirectorateWithoutAIDbSet.FromSqlInterpolated<DirectorateWithoutAIDTO>(FormattableStringFactory.Create(qry)).ToList<DirectorateWithoutAIDTO>();
            ActionResult<IEnumerable<DirectorateWithoutAIDTO>> actionResult = (ActionResult<IEnumerable<DirectorateWithoutAIDTO>>)myList;
            whyearid = (string)null;
            whereyearid = (string)null;
            whmcid = (string)null;
            yid = (string)null;
            qry = (string)null;
            myList = (List<DirectorateWithoutAIDTO>)null;
            return actionResult;
        }

        [HttpGet("CollegeHospital_AIvsIssue")]
        public async Task<ActionResult<IEnumerable<ClGHospitalAIVSISSUEDTO>>> CollegeHospital_AIvsIssue(
          string mcid,
          string yearid)
        {
            string whyearid = "";
            string whereyearid = "";
            string whmcid = " ";
            if (mcid != "0")
                whmcid = " and mc.mcid =" + mcid;
            string yid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(this._context);
                whyearid = f.getACCYRSETID();
                f = (FacOperations)null;
            }
            else
                whyearid = yearid;
            string qry = "";
            qry = " select facilityname,facilityid,count(itemid) as nousitemsIndent,sum(Issuenous) as Issuenous,round(sum(ISSValue)/10000000,2) as issuedcr\r\nfrom \r\n(\r\n\r\nselect f.facilityname,m.itemid,vi.AI,vi.facilityid,IssueQtyInLakh,issueqty,ISSValue,case when nvl(vi.AI,0)>0 and nvl(issueqty,0)>0 then 1 else 0 end as Issuenous from v_institutionai vi\r\ninner join masitems m on m.itemid=vi.itemid\r\ninner join masfacilities f on f.facilityid =vi.facilityid\r\ninner join masaccyearsettings ma on ma.accyrsetid=vi.accyrsetid\r\n inner join masitemcategories c on c.categoryid=m.categoryid\r\ninner join masitemmaincategory  mc on mc.mcid=c.mcid\r\nleft outer join\r\n(                                              select  IssueYearID,itemid,unitcount,round((sum(issueqty)*unitcount)/100000,2) as IssueQtyInLakh\r\n                                              ,sum(issueqty) as issueqty\r\n                                            ,sum(nvl(ISSValue,0)) as ISSValue,facilityid from \r\n                                            (\r\n\r\n   select tbi.itemid,tbo.inwno  ,(select ACCYRSETID from masaccyearsettings where tb.indentdate between STARTDATE and ENDDATE) as  IssueYearID\r\n\r\n   ,(tbo.issueqty) as IssueQty,(nvl(tbo.issueqty,0)*aci.finalrategst) ISSValue,m.unitcount,tb.facilityid\r\n\r\n                                             from tbindents tb\r\n                                             inner join tbindentitems tbi on tbi.indentid=tb.indentid \r\n                                              inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid\r\n                                              inner join tbreceiptbatches rb on rb.inwno=tbo.inwno\r\n                                              inner join soordereditems si on si.ponoid=rb.ponoid and si.itemid=tbi.itemid\r\n                                              inner join aoccontractitems aci on aci.contractitemid=si.contractitemid\r\n                                              inner join masfacilities f on f.facilityid = tb.facilityid\r\n                                              inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid\r\n                                              inner join masitems m on m.itemid=tbi.itemid\r\n                                             where tb.Status = 'C' and tb.issuetype='NO'   and ft.hodid=3\r\n                                            and  tb.indentdate between ( select startdate from masaccyearsettings where accyrsetid=" + whyearid + " )\r\n                                            and ( select enddate from masaccyearsettings where accyrsetid= " + whyearid + " )  \r\n\r\n                                              ) where 1=1  group by itemid,unitcount,IssueYearID,facilityid\r\n\r\n                                             )ISS on ISS.itemid=m.itemid and ISS.IssueYearID=vi.accyrsetid and iss.facilityid=vi.facilityid\r\nwhere vi.ACCYRSETID=" + whyearid + " " + whmcid + " and nvl(vi.AI,0) >0\r\n) \r\ngroup by facilityname,facilityid\r\nhaving sum(Issuenous)>0\r\norder by round(sum(ISSValue)/10000000,2) desc ";
            List<ClGHospitalAIVSISSUEDTO> myList = this._context.ClGHospitalAIVSISSUEDbSet.FromSqlInterpolated<ClGHospitalAIVSISSUEDTO>(FormattableStringFactory.Create(qry)).ToList<ClGHospitalAIVSISSUEDTO>();
            ActionResult<IEnumerable<ClGHospitalAIVSISSUEDTO>> actionResult = (ActionResult<IEnumerable<ClGHospitalAIVSISSUEDTO>>)myList;
            whyearid = (string)null;
            whereyearid = (string)null;
            whmcid = (string)null;
            yid = (string)null;
            qry = (string)null;
            myList = (List<ClGHospitalAIVSISSUEDTO>)null;
            return actionResult;
        }

        [HttpGet("ClgHos_IssueWihtoutAI")]
        public async Task<ActionResult<IEnumerable<ClgHospitalWithoutAIIssueDTO>>> ClgHos_IssueWihtoutAI(
          string mcid,
          string whyearid)
        {
            string yearid = "";
            string whmcid = " ";
            if (mcid != "0")
                whmcid = " and mc.mcid =" + mcid;
            string yid = "";
            if (whyearid == "0")
            {
                FacOperations f = new FacOperations(this._context);
                yearid = f.getACCYRSETID();
                f = (FacOperations)null;
            }
            else
                yearid = whyearid;
            string qry = "";
            qry = "   select facilityname,facilityid,count(itemid) as NosWithAIItems,round(sum(ISSValue)/10000000,2) as issuedcr\r\nfrom \r\n(\r\n select  facilityname,IssueYearID,a.itemid,unitcount,round((sum(issueqty)*unitcount)/100000,2) as IssueQtyInLakh\r\n                                              ,sum(issueqty) as issueqty\r\n                                            ,sum(nvl(ISSValue,0)) as ISSValue,a.facilityid,sum(ai) as ai from \r\n                                            (\r\n\r\n   select tbi.itemid,tbo.inwno  ,(select ACCYRSETID from masaccyearsettings where tb.indentdate between STARTDATE and ENDDATE) as  IssueYearID\r\n\r\n   ,(tbo.issueqty) as IssueQty,(nvl(tbo.issueqty,0)*aci.finalrategst) ISSValue,m.unitcount,tb.facilityid,f.facilityname\r\n\r\n                                             from tbindents tb\r\n                                             inner join tbindentitems tbi on tbi.indentid=tb.indentid \r\n                                              inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid\r\n                                              inner join tbreceiptbatches rb on rb.inwno=tbo.inwno\r\n                                              inner join soordereditems si on si.ponoid=rb.ponoid and si.itemid=tbi.itemid\r\n                                              inner join aoccontractitems aci on aci.contractitemid=si.contractitemid\r\n                                              inner join masfacilities f on f.facilityid = tb.facilityid\r\n                                              inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid\r\n                                              inner join masitems m on m.itemid=tbi.itemid\r\n inner join masitemcategories c on c.categoryid=m.categoryid\r\ninner join masitemmaincategory  mc on mc.mcid=c.mcid\r\n\r\n                                             where tb.Status = 'C' and tb.issuetype='NO'   and ft.hodid=3 " + whmcid + "\r\n                                            and  tb.indentdate between ( select startdate from masaccyearsettings where accyrsetid=" + yearid + " )\r\n                                            and ( select enddate from masaccyearsettings where accyrsetid=" + yearid + ")  \r\n\r\n                                              )a \r\n                                                   left outer join \r\n                                              (\r\n                                            select  vi.itemid,nvl(vi.AI,0) ai,vi.facilityid,accyrsetid from v_institutionai vi  where 1= 1 and  accyrsetid=" + yearid + "\r\n                                              ) i on i.itemid=a.itemid and i.accyrsetid=a.IssueYearID and i.facilityid=a.facilityid\r\n                                              where 1=1 and IssueYearID=" + yearid + " group by a.itemid,a.unitcount,a.IssueYearID,a.facilityid,a.facilityname\r\n                                      \r\n                                               \r\n                                               having sum(ai)=0  \r\n                                               )  \r\ngroup by facilityname,facilityid\r\norder by round(sum(ISSValue)/10000000,2) desc ";
            List<ClgHospitalWithoutAIIssueDTO> myList = this._context.ClgHospitalWithoutAIIssueDbSet.FromSqlInterpolated<ClgHospitalWithoutAIIssueDTO>(FormattableStringFactory.Create(qry)).ToList<ClgHospitalWithoutAIIssueDTO>();
            ActionResult<IEnumerable<ClgHospitalWithoutAIIssueDTO>> actionResult = (ActionResult<IEnumerable<ClgHospitalWithoutAIIssueDTO>>)myList;
            yearid = (string)null;
            whmcid = (string)null;
            yid = (string)null;
            qry = (string)null;
            myList = (List<ClgHospitalWithoutAIIssueDTO>)null;
            return actionResult;
        }

        [HttpGet("CollegeYearwuse_AIvsIssue")]
        public async Task<ActionResult<IEnumerable<YrsCollegeHospitalAIIssue>>> CollegeYearwuse_AIvsIssue(
          string mcid,
          string facid)
        {
            string whyearid = "";
            string whereyearid = "";
            string whmcid = " ";
            if (mcid != "0")
                whmcid = " and mc.mcid =" + mcid;
            string qry = "";
            qry = " select accyrsetid,ACCYEAR ,count(itemid) as nousitemsIndent,sum(Issuenous) as Issuenous,round(sum(ISSValue)/10000000,2) as issuedcr\r\nfrom \r\n(\r\n\r\nselect f.facilityname,m.itemid,vi.AI,vi.facilityid,IssueQtyInLakh,issueqty,ISSValue,case when nvl(vi.AI,0)>0 and nvl(issueqty,0)>0 then 1 else 0 end as Issuenous,vi.accyrsetid,yr.ACCYEAR from v_institutionai vi\r\ninner join masaccyearsettings yr on yr.accyrsetid=vi.accyrsetid\r\ninner join masitems m on m.itemid=vi.itemid\r\ninner join masfacilities f on f.facilityid =vi.facilityid\r\ninner join masaccyearsettings ma on ma.accyrsetid=vi.accyrsetid\r\n inner join masitemcategories c on c.categoryid=m.categoryid\r\ninner join masitemmaincategory  mc on mc.mcid=c.mcid\r\nleft outer join\r\n(                                              select  IssueYearID,itemid,unitcount,round((sum(issueqty)*unitcount)/100000,2) as IssueQtyInLakh\r\n                                              ,sum(issueqty) as issueqty\r\n                                            ,sum(nvl(ISSValue,0)) as ISSValue,facilityid from \r\n                                            (\r\n\r\n   select tbi.itemid,tbo.inwno  ,(select ACCYRSETID from masaccyearsettings where tb.indentdate between STARTDATE and ENDDATE) as  IssueYearID\r\n\r\n   ,(tbo.issueqty) as IssueQty,(nvl(tbo.issueqty,0)*aci.finalrategst) ISSValue,m.unitcount,tb.facilityid\r\n\r\n                                             from tbindents tb\r\n                                             inner join tbindentitems tbi on tbi.indentid=tb.indentid \r\n                                              inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid\r\n                                              inner join tbreceiptbatches rb on rb.inwno=tbo.inwno\r\n                                              inner join soordereditems si on si.ponoid=rb.ponoid and si.itemid=tbi.itemid\r\n                                              inner join aoccontractitems aci on aci.contractitemid=si.contractitemid\r\n                                              inner join masfacilities f on f.facilityid = tb.facilityid\r\n                                              inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid\r\n                                              inner join masitems m on m.itemid=tbi.itemid\r\n                                             where tb.Status = 'C' and tb.issuetype='NO'   and ft.hodid=3\r\n                                            and  tb.indentdate > ( select startdate from masaccyearsettings where accyrsetid=529 )\r\n                                         \r\n\r\n                                              ) where 1=1  group by itemid,unitcount,IssueYearID,facilityid\r\n\r\n                                             )ISS on ISS.itemid=m.itemid and ISS.IssueYearID=vi.accyrsetid and iss.facilityid=vi.facilityid\r\nwhere 1=1   " + whmcid + " and nvl(vi.AI,0) >0 and vi.facilityid=" + facid + "\r\n) \r\ngroup by accyrsetid,ACCYEAR \r\nhaving sum(Issuenous)>0\r\norder by accyrsetid desc  ";
            List<YrsCollegeHospitalAIIssue> myList = this._context.YrsCollegeHospitalAIIssueDbSet.FromSqlInterpolated<YrsCollegeHospitalAIIssue>(FormattableStringFactory.Create(qry)).ToList<YrsCollegeHospitalAIIssue>();
            ActionResult<IEnumerable<YrsCollegeHospitalAIIssue>> actionResult = (ActionResult<IEnumerable<YrsCollegeHospitalAIIssue>>)myList;
            whyearid = (string)null;
            whereyearid = (string)null;
            whmcid = (string)null;
            qry = (string)null;
            myList = (List<YrsCollegeHospitalAIIssue>)null;
            return actionResult;
        }
    }
}
