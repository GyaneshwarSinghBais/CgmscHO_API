using CgmscHO_API.ANPRDTO;
using CgmscHO_API.DashHomeDTO;
using CgmscHO_API.DirectorateDTO;
using CgmscHO_API.DistrictDTO;
using CgmscHO_API.DistrictDTOs;
using CgmscHO_API.DTO;
using CgmscHO_API.FacilityDTO;
using CgmscHO_API.FinanceDTO;
using CgmscHO_API.HodDTO;
using CgmscHO_API.HODTO;
using CgmscHO_API.MasterDTO;
using CgmscHO_API.PublicDTO;
using CgmscHO_API.QCDTO;
using CgmscHO_API.WarehouseDTO;
using Microsoft.EntityFrameworkCore;

namespace CgmscHO_API.Models
{
    public class OraDbContext : DbContext
    {
        public DbSet<ProductsInfo> Products { get; set; }
        public DbSet<CGMSCitemWiseStock> CGMSCStock { get; set; }
        public DbSet<MasFacilityWardsModel> MasFacilityWards { get; set; }

        public DbSet<UsruserModel> Usruser { get; set; }
        public DbSet<VehicleModel> UsruserVehicle { get; set; }

        public DbSet<StockReportFacilityDTO> StockReportFacilityDTOs { get; set; }
        public DbSet<FacilityWardDTO> FacilityWardDTOs { get; set; }
        public DbSet<IncompleteWardIssueDTO> IncompleteWardIssueDTOs { get; set; }
        public DbSet<IncompleteWardIndentDTO> IncompleteWardIndentDbSet { get; set; }
        public DbSet<IndentItemsFromWardDTO> IndentItemsFromWardDbSet { get; set; }
        public DbSet<WardIssueItemsDTO> WardIssueItemsDbSet { get; set; }
        public DbSet<ItemStockDTO> ItemStockDBSet { get; set; }

        public DbSet<HoldItemStockDTO> HoldItemStockDBSet { get; set; }

        public DbSet<NearExpBatchDTO> NearExpBatchDTODBSet { get; set; }

        public DbSet<tbFacilityIssueItems> tbFacilityIssueItems { get; set; }
        public DbSet<IncompleteWardIssueItemsDTO> IncompleteWardIssueItemsDbSet { get; set; }
        public DbSet<getIssueItemIdDTO> getIssueItemIdDbSet { get; set; }
        public DbSet<getBatchesDTO> getBatchesDbSet { get; set; }
        public DbSet<tbFacilityOutwardsModel> tbFacilityOutwardsDbSet { get; set; }

        public DbSet<tbFacilityGenIssue> tbFacilityGenIssueDbSet { get; set; }
        public DbSet<FacilityInfoDTO> FacilityInfoDbSet { get; set; }
        public DbSet<masAccYearSettingsModel> masAccYearSettingsDbSet { get; set; }
        public DbSet<getCMHOfacfromDistDTO> getCMHOfacfromDistDbSet { get; set; }

        public DbSet<GenrateReceiptIssueNoDTO> GenrateReceiptIssueNoDbSet { get; set; }
        public DbSet<CategoryDTO> CategoryDTODbSet { get; set; }
        public DbSet<CategoryMainDTO> CategoryMainDTODbSet { get; set; }
        public DbSet<StockOutDTO> StockOutDTODbSet { get; set; }
        public DbSet<ReceiptMasterWHDTO> ReceiptMasterDbSet { get; set; }
        public DbSet<IndentDataDTO> IndentDataDbSet { get; set; }
        public DbSet<tbFacilityReceiptsModel> tbFacilityReceiptsDbSet { get; set; }
        public DbSet<ReceiptMasterDTO> ReceiptMasterDTODbSet { get; set; }
        public DbSet<MasRackDTO> MasRackDbSet { get; set; }
        public DbSet<ReceiptItemsDDL> ReceiptItemsDDLDbSet { get; set; }
        public DbSet<ReceiptDetailsDTO> ReceiptDetailsDbSet { get; set; }
        public DbSet<ExtractReceiptItemsDTO> ExtractReceiptItemsDbSet { get; set; }
        public DbSet<tbFacilityReceiptItemsModel> tbFacilityReceiptItemsDbSet { get; set; }
        public DbSet<tbFacilityReceiptBatchesModel> tbFacilityReceiptBatchesDbSet { get; set; }
        public DbSet<tbFacilityReceiptItemsDTO> tbFacilityReceiptItemsDbSet1 { get; set; }
        public DbSet<StockOutDetailDTO> StockOutDetailDbSet { get; set; }
        public DbSet<FacilityIndentToWHDTO> FacilityIndentToWHDbSet { get; set; }
        public DbSet<FacilityReceiptAgainstIndentDTO> FacilityReceiptAgainstIndentDbSet { get; set; }
        public DbSet<FacilityReceiptFromOtherFacilityOrLP_DTO> FacilityReceiptFromOtherFacilityOrLP_DbSet { get; set; }

        public DbSet<ItemDetailDTO> ItemDetailDbSet { get; set; }
        public DbSet<IndentAlertNewDTO> IndentAlertNewDbSet { get; set; }
        public DbSet<FacMonthIndentDTO> FacMonthIndentDbSet { get; set; }
        public DbSet<getIndentProgramDTO> getIndentProgramDbSet { get; set; }
        public DbSet<mascgmscnocitems> mascgmscnocitemsDbSet { get; set; }
        public DbSet<SavedFacIndentItemsDTO> SavedFacIndentItemsDbSet { get; set; }
        public DbSet<tbGenIndent> tbGenIndentDbSet { get; set; }
        public DbSet<GenWhIndentNoDTO> GenWhIndentNoDbSet { get; set; }
        public DbSet<ProgressRecDTO> ProgressRecDbSet { get; set; }

        public DbSet<FundsDTO> FundsDbSet { get; set; }
        public DbSet<FitReportDTO> FitReportDbSet { get; set; }
        public DbSet<ReagentStockAndSupplySummaryDTO> ReagentStockAndSupplySummaryDbSet { get; set; }
        public DbSet<ReagentStockAndSupplyDTO> ReagentStockAndSupplyDbSet { get; set; }
        public DbSet<WHreagentStockDTO> WHreagentStockDbSet { get; set; }
        public DbSet<DdlMasreagenteqpDTO> DdlMasreagenteqpDbSet { get; set; }
        public DbSet<GetAyushItemsDTO> GetAyushItemsDbSet { get; set; }
        public DbSet<GetFileStorageLocationDTO> GetFileStorageLocationDbSet { get; set; }
        public DbSet<GetOpeningStocksRptDTO> GetOpeningStocksRptDbSet { get; set; }
        public DbSet<GetfacreceiptitemidDTO> GetfacreceiptitemidDbSet { get; set; }
        public DbSet<GetInwardNoDTO> GetInwardNoDbSet { get; set; }
        public DbSet<GetFacilityReceiptIdDTO> GetFacilityReceiptIdDbSet { get; set; }
        public DbSet<GetFacilityReceiptItemIdDTO> GetFacilityReceiptItemIdDbSet { get; set; }
        public DbSet<GetItemCodeDTO> GetItemCodeDbSet { get; set; }
        public DbSet<GetFacilityCodeDTO> GetFacilityCodeDbSet { get; set; }
        public DbSet<GetHSccDTO> GetHSccDbSet { get; set; }
        public DbSet<GetWHsSerialNoDTO> GetWHsSerialNoDbSet { get; set; }
        public DbSet<GetFacReceiptIdDTO> GetFacReceiptIdDbSet { get; set; }
        public DbSet<GetHeaderInfoDTO> GetHeaderInfoDbSet { get; set; }
        public DbSet<WHIndentPendingReagentDTO> GetWHIndentPendingReagentDbSet { get; set; }
        public DbSet<WHIndentPendingReagentEQDTO> GetWHIndentPendingReagentEQDTODbSet { get; set; }
        public DbSet<ReagentIssueSummaryDTO> GetReagentIssueSummaryDTODbSet { get; set; }

        public DbSet<ReagentIndentIssueWHDetailsDTO> GetReagentIndentIssueWHDetailsDbSet { get; set; }
        public DbSet<EMDSummaryDTO> GetEMDSummaryDTODbSet { get; set; }

        public DbSet<EMDSummaryTenderDTO> GetEMDSummaryTenderDTODbSet { get; set; }

        public DbSet<EMDDetailsDTO> GetEMDDetailsDTODbSet { get; set; }

        public DbSet<EMDDashDTO> GetEMDDashDTODbSet { get; set; }

        public DbSet<EMDReleaseddetDTO> GetEMDReleaseddetDTODbSet { get; set; }

        public DbSet<NOCApprovedSummaryDTO> GetNOCApprovedSummaryDTODbSet { get; set; }
        public DbSet<NOCApprovedDetailsDTO> GetNOCApprovedDetailsDbSet { get; set; }

        public DbSet<InitiatedPendingIssueSummaryDTO> GetInitiatedPendingIssueSummaryDbSet { get; set; }
        public DbSet<InitiatedPendingIssueDetailsDTO> GetInitiatedPendingIssueDetailsDTODbSet { get; set; }
        public DbSet<IWHPipelineSummaryDTO> GetIWHPipelineSummaryDbSet { get; set; }
        public DbSet<IWHPipelineDetailsDTO> GetIWHPipelineDetailsDTODbSet { get; set; }
       
        


        // public DbSet<tbFacilityOutwardsUpdateModel> tbFacilityOutwardsUpdateModelDbSet { get; set; }

        public OraDbContext(DbContextOptions<OraDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }



        //HODTO
        public DbSet<HODYearWisePODTO> HODYearWisePODbSet { get; set; }
        public DbSet<HODIssueDTO> HODIssueDbSet { get; set; }

        public DbSet<SuplierDTO> SuplierDbSet { get; set; }

        public DbSet<MasPODTO> MasPODbSet { get; set; }

        public DbSet<CGMSCStockDTO> CGMSCStockDbSet { get; set; }
        public DbSet<StockVDTO> CGMSCStockValueDbSet { get; set; }
        public DbSet<WHDTO> WHDbSet { get; set; }
        public DbSet<MasItemsDTO> MasItemsDbSet { get; set; }
        public DbSet<WHWiseStockDTO> WHWiseStockDbSet { get; set; }
        public DbSet<TotalRCDTO> TotalRCDbSet { get; set; }
        public DbSet<TotalTDTO> TotalTDbSet { get; set; }
        public DbSet<QCLabOutDTO> QCLabOutDbSet { get; set; }
        public DbSet<QCLabTimeLineDetailsDTO> QCLabTimeLineDetailsDbSet { get; set; }
        public DbSet<QCLabTimeLineBatchDTO> QCLabTimeLineBatchDbSet { get; set; }


        public DbSet<QCLabWithDTO> QCLabWithDbSet { get; set; }
        public DbSet<UnpaidSupplierDTO> UnpaidSupplierDbSet { get; set; }
        public DbSet<SupplierLiabilityDTO> SupplierLiabilityDbSet { get; set; }
        public DbSet<SupplierPoDTO> SupplierPoDbSet { get; set; }
        public DbSet<MasWHInfoDTO> MasWHInfoDbSet { get; set; }
        public DbSet<PriceBidDrilDTO> PriceBidDrilDbSet { get; set; }

        public DbSet<YearWiseRCDTO> YearWiseRCDbSet { get; set; }
        public DbSet<TenderEveDTO> TenderEveDbSet { get; set; }
        public DbSet<QCSameBatchDTO> QCSameBatchDbSet { get; set; }
        public DbSet<QCSameBatchDetailsDTO> QCSameBatchDetailsDbset { get; set; }
        public DbSet<YearWiseExpiredDTO> YearWiseExpiredDbset { get; set; }
        public DbSet<POAlertAIDTO> POAlertAIDbSet { get; set; }
        public DbSet<POAlertDetailsDTO> POAlertDetailsDbSet { get; set; }
        public DbSet<WHIndentPendingDTO> WHIndentPendingDbSet { get; set; }
        public DbSet<WHIndentPendingDetailsDTO> WHIndentPendingDetailsDbSet { get; set; }
        public DbSet<CGMSCPublicStockDTO> CGMSCPublicStockDbSet { get; set; }
        public DbSet<NearExpDTO> NearExpDbSet { get; set; }
        public DbSet<NearExpDTOItems> NearExpItemsDbSet { get; set; }
        public DbSet<NearExpBatchWHDTO> NearExpBatchWHDbSet { get; set; }
        public DbSet<FacilityMCDTO> FacilityMCDbSet { get; set; }
        public DbSet<MCAIVSIssuanceDTO> MCAIVSIssuanceDBSet { get; set; }
        public DbSet<TenderstageDTO> TenderstageDbSet { get; set; }
        public DbSet<StockitemsDTO> StockitemsDbSet { get; set; }
        public DbSet<RCDetailDTO> RCDetailDbSet { get; set; }
        public DbSet<ItemIndentQtyDTO> ItemIndentQtyDbSet { get; set; }
        public DbSet<IndentvsIssuanceDTO> IndentvsIssuanceDbSet { get; set; }
        public DbSet<StockPotionItemDTO> StockPotionItemDbSet { get; set; }
        public DbSet<StockPostionDTO> StockPostionDbSet { get; set; }
        public DbSet<InitiatedPendingIssueSummaryDTO> facddlDBset { get; set; }
        public DbSet<FitDetailDTO> FitDetailDbSet { get; set; }
        public DbSet<PipelineDTO> PipelineDbSet { get; set; }
        public DbSet<ItemIndentstockIssueDTO> ItemIndentstockIssueDbset { get; set; }

        public DbSet<StockGroupItemDTO> StockGroupItemDbSet { get; set; }

        public DbSet<PipelineDetailsDTO> PipelineDetailsDbset { get; set; }
        public DbSet<FundBalanceDTO> FundBalanceDbSet { get; set; }

        public DbSet<EquipmentDTO> EquipmentDbSet { get; set; }

        public DbSet<ReagentStateStockIssueDTO> ReagentStateStockIssueDbSet { get; set; }
        public DbSet<WarehouseWiseReagentDTO> WarehouseWiseReagentDbSet { get; set; }
        public DbSet<IssuedReagentYearlyDTO> IssuedReagentYearlyDbSet { get; set; }
        public DbSet<CGMSCReagentStockValueDTO> CGMSCReagentStockValueDbSet { get; set; }
        public DbSet<CurrentLiabilityDTO> CurrentLiabilityDbSet { get; set; }
        public DbSet<DhsDmeStockDTO> DhsDmeStockDbSet { get; set; }
        public DbSet<dhsDmeYearConsumptionDTO> dhsDmeYearConsumptionDbSet { get; set; }
        public DbSet<districtWiseDhsDmeStockDTO> districtWiseDhsDmeStockDbSet { get; set; }
        public DbSet<DistrictDTOWH> DistrictDbSet { get; set; }
        public DbSet<DistFACwiseStockPostionDTO> DistFACwiseStockPostionDbSet { get; set; }
        public DbSet<WHWisePubStockDTO> WHWisePubStockDbSet { get; set; }

        public DbSet<ItemwisewhStockDTO> ItemwisewhStockDbSet { get; set; }
        public DbSet<DistwiseItemIssuanceDTO> DistwiseItemIssuanceDbSet { get; set; }
        public DbSet<FACwiseItemIssuanceDTO> FACwiseItemIssuanceDbSet { get; set; }
        public DbSet<AllGroupNameDTO> AllGroupNameDbSet { get; set; }
        public DbSet<FACwiseStockPostionDTO> FACwiseStockPostionDbset { get; set; }
        public DbSet<OPDCountDTO> OPDCountDbSet { get; set; }
        public DbSet<DRCountDTO> DrCountDbSet { get; set; }
        public DbSet<MasDoctorDTO> MasDoctorDbSet { get; set; }
        public DbSet<DiagnosysDTO> DiagnosysDbSet { get; set; }
        public DbSet<DistFactStockDTO> DistFactStockDBset { get; set; }
        public DbSet<MasRecRemarksDTO> MasRecRemarksDbSet { get; set; }

        public DbSet<WHInTransitIssuesDTO> WHInTransitIssuesDbSet { get; set; }

        public DbSet<TransportVoucherDTO> TransportVoucherDBSet { get; set; }

        //Courier App  Start
        public DbSet<PickDockets> PickDocketDbSet { get; set; }
        public DbSet<GetCourierTransactionModel> GetCourierTransactionDBSet { get; set; }
        public DbSet<pickedCourierToBeDropModel> pickedCourierToBeDropDBSet { get; set; }
        public DbSet<CourierStatusDTO> CourierStatusDbSet { get; set; }
        public DbSet<CourierPerformanceDTO> CourierPerformanceDbSet { get; set; }
        public DbSet<getUndroppedDocketDTO> getUndroppedDocketDbSet { get; set; }
        public DbSet<PickDocketDetailsLabDTO> PickDocketDetailsLabDbSet { get; set; }
        public DbSet<pickedCourierToBeDropForLabDTO> pickedCourierToBeDropForLabDbSet { get; set; }
        public DbSet<PendingToDropInLabDTO> PendingToDropInLabDbSet { get; set; }
        public DbSet<PickRaisedDTO> PickRaisedDbSet { get; set; }
        public DbSet<PendingToPickAndDropDTO> PendingToPickAndDropDbSet { get; set; }
        public DbSet<PendingToPickByItemDTO> PendingToPickByItemDbSet { get; set; }
        public DbSet<PendingToDropByItemDTO> PendingToDropByItemDbSet { get; set; }
        public DbSet<ItemDetailDDLDTO> ItemDetailDDLDbSet { get; set; }
        public DbSet<PendingToSendToLabDTO> PendingToSendToLabDbSet { get; set; }
        public DbSet<PendingToReceiptInHODTO> PendingToReceiptInHODbSet { get; set; }
        public DbSet<UnderLabSinceXdaysWithinTimelineDTO> UnderLabSinceXdaysWithinTimelineDbSet { get; set; }
        public DbSet<UnderLabSinceXdaysOutOfTimelineDTO> UnderLabSinceXdaysOutOfTimelineDbSet { get; set; }
        public DbSet<FinalStatusPendingInHOQCDTO> FinalStatusPendingInHOQCDbSet { get; set; }
        public DbSet<UndroppedDocketDetailsLabDTO> UndroppedDocketDetailsLabDbSet { get; set; }
        public DbSet<DistrictWiseStockDTO> DistrictWiseStockDbSet { get; set; }
        public DbSet<DdlItemWiseInHandQtyDTO> DdlItemWiseInHandQtyDbSet { get; set; }
        public DbSet<ItemWiseInHandQtyDetailDTO> ItemWiseInHandQtyDetailDbSet { get; set; }
        public DbSet<GetItemQtyDTO> GetItemQtyDbSet { get; set; }
        public DbSet<PendingToReceiptInLabDTO> PendingToReceiptInLabDbSet { get; set; }

        public DbSet<HODIssueSummaryDTO> GetHODIssueSummaryDbSet { get; set; }
        public DbSet<HODAIvsIssueDTO> GetHODAIvsIssueDTODbSet { get; set; }

        public DbSet<NOCPendingSummaryDTO> GetNOCPendingSummaryDbSet { get; set; }
        public DbSet<NOCPendingDetailsDTO> GetNOCPendingDetailsDTODbSet { get; set; }
        public DbSet<LabIssuePendingSummary> GetLabIssuePendingSummaryDbSet { get; set; }

        public DbSet<LabIssuePendingDetailsDTO> GetLabIssuePendingDetailsDbSet { get; set; }

        public DbSet<InTransitHOtoLabSummaryDTO> GetInTransitHOtoLabSummaryDbSet { get; set; }
        public DbSet<InTransitHOtoLabDetailDTO> GetInTransitHOtoLabDetailDbSet { get; set; }
        public DbSet<FacilityIssueCurrentStockDTO> FacilityIssueCurrentStockDbSet { get; set; }
        public DbSet<FacilityIssueDateWiseDTO> FacilityIssueDateWiseDbSet { get; set; }
        public DbSet<InsertMASWHVEHICLETRANSPORTDTO> InsertMASWHVEHICLETRANSPORTDbSet { get; set; }
        public DbSet<VhicleInfoDTO> VhicleInfoDbSet { get; set; }
        public DbSet<WarehousesModel> WarehousesDbSet { get; set; }
        public DbSet<GetEnterExitVhicleDTO> GetEnterExitVhicleDbSet { get; set; }
        public DbSet<EdlNonEdlIssuePercentSummaryDTO> EdlNonEdlIssuePercentSummaryDbSet { get; set; }
        public DbSet<IssuePerWisePerClickDTO> IssuePerWisePerClickDbSet { get; set; }
        public DbSet<IssuedPerWiseDTO> IssuedPerWiseDbSet { get; set; }

        public DbSet<WarehouseInfoDTO> WarehouseInfoDbSet { get; set; }
        public DbSet<FacCoverageDTO> FacCoverageDbSet { get; set; }
        public DbSet<SeasonIssueDTO> SeasonIssueDbSet { get; set; }
        public DbSet<StockPBalanceIndentSummaryDTO> StockPBalanceIndentSummaryDbSet { get; set; }
        public DbSet<StockPBalanceIndentDetailsDTO> StockPBalanceIndentDetailsDbSet { get; set; }
        public DbSet<RCNearExPSummary> RCNearExPSummaryDbset { get; set; }
        public DbSet<RCNearExDetails> RCNearExDetailsDbSet { get; set; }
        public DbSet<SupplyDuration> SupplyDurationDbSet { get; set; }
        public DbSet<SupplierTimeTaken> SupplierTimeTakenDbSet { get; set; }
        public DbSet<Massupplier> MassupplierDbSet { get; set; }
        public DbSet<TimeTakenPaid> TimeTakenPaidDbSet { get; set; }
        public DbSet<TimeTakenYearQC> TimeTakenYearQCDbSet { get; set; }
        public DbSet<TimeTakenLabAll_Year> TimeTakenLabAll_YearDbSet { get; set; }
        public DbSet<LabTestTimeTaken> LabTestTimeTakenDbSet { get; set; }

        public DbSet<EDLstockout> EDLstockoutDbSet { get; set; }
        public DbSet<EDLStockoutDetails> EDLStockoutDetailsDbSet { get; set; }
        public DbSet<HODYearWisePODTONew> HODYearWisePODTONewDbSet { get; set; }
        public DbSet<DirectorateIndentPOStatusDTO> DirectorateIndentPOStatusDbSet { get; set; }
        public DbSet<DirectorateGroupAI_PODTO> DirectorateGroupAI_PODbSet { get; set; }
        public DbSet<DropAppPerfomance> DropAppPerfomanceDbSet { get; set; }
        public DbSet<DisWiseIssueDTO> DisWiseIssueDTODbSet { get; set; }
        public DbSet<MonthwiseIssueDTO> MonthwiseIssueDbSet { get; set; }
        public DbSet<DeliveryDash> DeliveryDashDbSet { get; set; }
        public DbSet<UseriDDLDTO> UseriDDLDbSet { get; set; }


        //Collector Dash
        public DbSet<DistrictStkCount> DistrictStkCountDbSet { get; set; }
        public DbSet<WHStkcount> WHStkcountDbSet { get; set; }
        public DbSet<DHSissueItemsDTO> DHSissueItemsDbSet { get; set; }
        public DbSet<DMEissueItemsDTO> DMEissueItemsDbSet { get; set; }

        public DbSet<FacilityInfoAamDTO> FacilityInfoAamDbSet { get; set; }

        public DbSet<VeriftyOtpDTO> VeriftyOtpDbSet { get; set; }
        public DbSet<DistIssueGrowthDTO> DistIssueGrowthDbSet { get; set; }
        public DbSet<DistCGMSCSupplyDTO> DistCGMSCSupplyDBset { get; set; }
        public DbSet<DistFacwiseCGMSCSupplyIndentDTO> DistFacwiseCGMSCSupplyIndentDbSet { get; set; }
        public DbSet<GetVehicleNoDTO> GetVehicleNoDbSet { get; set; }
        public DbSet<GetLatLongDTO> GetLatLongDbSet { get; set; }

        public DbSet<CGMSCIndentPendingDTO> CGMSCIndentPendingDbSet { get; set; }
        public DbSet<IssuedCFYDTO> IssuedCFYDbSet { get; set; }
        public DbSet<IssueDaysDTO> IssueDaysDbSet { get; set; }
        public DbSet<ReceiptValuesDTO> ReceiptValuesDbSet { get; set; }
        public DbSet<DeliveryMonthDTsDTO> DeliveryMonthDbSet { get; set; }
        public DbSet<MasItemsDash> MasItemsDashDbSet { get; set; }
        public DbSet<IndentHomeDTO> IndentHomeDbSet { get; set; }

        public DbSet<YearwisePODTO> YearwisePODbSet { get; set; }
        public DbSet<StockHomeDTO> StockHomeDbSet { get; set; }
        public DbSet<TotalCurrentStockHomeDTO> TotalCurrentStockHomeDbSet { get; set; }
        public DbSet<NearExpMonthHomeDTO> NearExpMonthHomeDbSet { get; set; }
        public DbSet<IndentCntHome> IndentCntHomeDbSet { get; set; }
        public DbSet<StockoutPerHome> StockoutPerHomeDbSet { get; set; }

        public DbSet<RecFundsDTO> RecFundsDbSet { get; set; }
        public DbSet<AIPO_ReceiptDTO> AIPO_ReceiptDbSet { get; set; }
        public DbSet<PaidYearWiseDTO> PaidYearWiseDbSet { get; set; }
        public DbSet<IssueItemsYearwiseDTO> IssueItemsYearwiseDbSet { get; set; }
        public DbSet<PartiItem_RCDTO> PartiItem_RCDbSet { get; set; }
        public DbSet<PaidDateWiseDTO> PaidDateWiseDbSet { get; set; }
        public DbSet<YrLibDTO> YrLibDbSet { get; set; }
        public DbSet<PiplineLibDTO> PiplineLibDbSet { get; set; }
        public DbSet<RecFundsDetailsDTO> RecFundsDetailsDbSet { get; set; }
        public DbSet<GrossPaidDetails> GrossPaidDetailsDbSet { get; set; }
        public DbSet<QCHomeDashDTO> QCHomeDashDbSet { get; set; }
        public DbSet<QCHomeDashPlacewise> QCHomeDashPlacewiseDbSet { get; set; }
        public DbSet<QCPendingAreaDetails> QCPendingAreaDetailsDbSet { get; set; }
        public DbSet<SanctionChequePrepSummaryDTO> SanctionChequePrepSummaryDbSet { get; set; }
        public DbSet<LibDetailsDTO> LibDetailsDbSet { get; set; }

        //Courier App End



        public DbSet<AIvsIssueDTO> AIvsIssueDbSet { get; set; }

        public DbSet<DirectorateWithoutAIDTO> DirectorateWithoutAIDbSet { get; set; }

        public DbSet<ClGHospitalAIVSISSUEDTO> ClGHospitalAIVSISSUEDbSet { get; set; }

        public DbSet<YrsCollegeHospitalAIIssue> YrsCollegeHospitalAIIssueDbSet { get; set; }

        public DbSet<ClgHospitalWithoutAIIssueDTO> ClgHospitalWithoutAIIssueDbSet { get; set; }
        public DbSet<QCSamplePendingTimelinesDTO> QCSamplePendingTimelinesDbSet { get; set; }
        public DbSet<LabWisePendingDTO> LabWisePendingDbSet { get; set; }

        public DbSet<QCMonthWisePendingRecDTO> QCMonthWisePendingRecDbSet { get; set; }
        public DbSet<QCHoldItemDetails> QCHoldItemDetailsDbSet { get; set; }
        public DbSet<TenderStagesTotalDTO> TenderStagesTotalDbSet { get; set; }
        public DbSet<StatusDetailDTO> StatusDetailDbSet { get; set; }
        public DbSet<StatusItemDetailDTO> StatusItemDetailDbSet { get; set; }
        public DbSet<TotalTenderDTO> TotalTenderDbSet { get; set; }
        public DbSet<NoOfBiddersDTO> NoOfBiddersDbSet { get; set; }
        public DbSet<ConversationHodCgmscDTO> ConversationHodCgmscDbSet { get; set; }
        public DbSet<WhMangerSSODetailDTO> WhMangerSSODetailDbSet { get; set; }
        public DbSet<ToBeTenderDTO> ToBeTenderDbSet { get; set; }
        public DbSet<ToBeTenderDetailDTO> ToBeTenderDetailDbSet { get; set; }
        public DbSet<SchemeReceivedDTO> SchemeReceivedDbSet { get; set; }
        public DbSet<SchemeTenderStatusDTO> SchemeTenderStatusDbSet { get; set; }
        public DbSet<NsqDrugDetailsDTO> NsqDrugDetailsDbSet { get; set; }
        public DbSet<AIvsIssuanceDTO> AIvsIssuanceDbSet { get; set; }
        














        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<FacilityIssueDateWiseDTO>().HasNoKey();

            modelBuilder.Entity<EdlNonEdlIssuePercentSummaryDTO>().HasNoKey();
            modelBuilder.Entity<IssuePerWisePerClickDTO>().HasNoKey();
            modelBuilder.Entity<GetLatLongDTO>().HasNoKey();
            modelBuilder.Entity<StatusDetailDTO>().HasNoKey();
            modelBuilder.Entity<StatusItemDetailDTO>().HasNoKey();
            modelBuilder.Entity<TotalTenderDTO>().HasNoKey();
            modelBuilder.Entity<NoOfBiddersDTO>().HasNoKey();
            modelBuilder.Entity<ConversationHodCgmscDTO>().HasNoKey();
            modelBuilder.Entity<ToBeTenderDetailDTO>().HasNoKey(); 
            modelBuilder.Entity<SchemeReceivedDTO>().HasNoKey();
            modelBuilder.Entity<SchemeTenderStatusDTO>().HasNoKey();
            modelBuilder.Entity<NsqDrugDetailsDTO>().HasNoKey();
            modelBuilder.Entity<AIvsIssuanceDTO>().HasNoKey();








            //  modelBuilder.Entity<MasCgmscNocItems>().ToTable("MasCgmscNocItems");


        }


    }
}
