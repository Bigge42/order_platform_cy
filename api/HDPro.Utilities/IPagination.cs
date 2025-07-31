namespace HDPro.Utilities
{
    public interface IPagination
    {
        int PageSize { get; set; }

        int PageIndex { get; set; }
    }
}
