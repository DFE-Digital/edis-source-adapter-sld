namespace Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData
{
    public class SldPagedResult<T>
    {
        public T[] Items { get; set; }
        public int TotalNumberOfItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalNumberOfPages { get; set; }
    }
}