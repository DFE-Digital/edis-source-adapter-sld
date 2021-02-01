using System;

namespace Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData
{
    public class LearningDelivery
    {
        public int AimType { get; set; }
        public DateTime LearnStartDate { get; set; }
        public DateTime LearnPlanEndDate { get; set; }
        public int FundModel { get; set; }
        public int StdCode { get; set; }
        public string DelLocPostCode { get; set; }
        public string EpaOrgId { get; set; }
        public int CompStatus { get; set; }
        public DateTime LearnActEndDate { get; set; }
        public int WithdrawReason { get; set; }
        public int Outcome { get; set; }
        public DateTime AchDate { get; set; }
        public string OutGrade { get; set; }
        public int ProgType { get; set; }
    }
}