using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QualityCheckDemo.POCO
{
    [Table("_TripartiteReview")]

    public partial class _TripartiteReview
    {
        public long ID { get; set; }
        public long PlanInfomationID { get; set; }
        [StringLength(50)]
        public string ReviewTaskCode { get; set; }
        [StringLength(50)]
        public string ProductBornCode { get; set; }
        public DateTime? HappenTime { get; set; }
        [StringLength(50)]
        public string TechnicalDirectorCode { get; set; }
        [StringLength(50)]
        public string TechnicalDirector { get; set; }
        [StringLength(50)]
        public string ManufacturingDirectorCode { get; set; }
        [StringLength(50)]
        public string ManufacturingDirector { get; set; }
        [StringLength(50)]
        public string ProblemDescription { get; set; }
        public int? BadType { get; set; }
        public long? ProcedureID { get; set; }
        [StringLength(50)]
        public string ProcedureCode { get; set; }
        [StringLength(50)]
        public string ProcedureName { get; set; }
        public long? FillerID { get; set; }
        [StringLength(50)]
        public string FillerCode { get; set; }
        [StringLength(50)]
        public string FillerName { get; set; }
        public int? ReviewStaffID { get; set; }
        [StringLength(50)]
        public string ReviewStaffCode { get; set; }
        [StringLength(50)]
        public string ReviewStaffName { get; set; }
        public int? RiskAssessment { get; set; }
        public int? ReviewResult { get; set; }
        [StringLength(50)]
        public string ReviewResultDescription { get; set; }
        public long? TechnicalFillerID { get; set; }
        [StringLength(50)]
        public string TechnicalFillerCode { get; set; }
        [StringLength(50)]
        public string TechnicalFillerName { get; set; }
        [StringLength(50)]
        public string ReworkDescription { get; set; }
        public int? InspectionResultType { get; set; }
        [StringLength(50)]
        public string InspectionResultDescription { get; set; }
        public long? CheckStaffID { get; set; }
        [StringLength(50)]
        public string CheckStaffCode { get; set; }
        [StringLength(50)]
        public string CheckStaffName { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal? OutsourcingCost { get; set; }
        public decimal? RepairCost { get; set; }
        public decimal? OtherCost { get; set; }
        public decimal? Sum { get; set; }
        public long? ProcurementReviewerID { get; set; }
        [StringLength(50)]
        public string ProcurementReviewerCode { get; set; }
        [StringLength(50)]
        public string ProcurementReviewerName { get; set; }
        public long? ProductionReviewerID { get; set; }
        [StringLength(50)]
        public string ProductionReviewerCode { get; set; }
        [StringLength(50)]
        public string ProductionReviewerName { get; set; }
        public bool? IsAvailable { get; set; }

        public DateTime? CreateTime { get; set; }
        [StringLength(50)]
        public string CreatorID { get; set; }
        public DateTime? LastModifiedTime { get; set; }
        [StringLength(50)]
        public string ModifierID { get; set; }
        [StringLength(50)]
        public string Remarks { get; set; }
        [StringLength(50)]
        public string Reserve1 { get; set; }
        [StringLength(50)]
        public string Reserve2 { get; set; }
        [StringLength(50)]
        public string Reserve3 { get; set; }
        public long? EquipmentID { get; set; }
        [StringLength(50)]
        public string EquipmentCode { get; set; }
        [StringLength(50)]
        public string EquipmentName { get; set; }
        public long? ReworkStaffID { get; set; }
        [StringLength(50)]
        public string ReworkStaffCode { get; set; }
        [StringLength(50)]
        public string ReworkStaffName { get; set; }
        public DateTime? ReworkCreateTime { get; set;  }

    }

}
