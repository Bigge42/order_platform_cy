namespace HDPro.Utilities
{
    public interface ISortable
    {
        string SortField { get; set; }

        SortDirection SortDirection { get; set; }
    }
}
