namespace FreeGency.Application.Common.Pagination
{
    public abstract class PagedQuery
    {
        private const int _maxPageSize = 30;
        private readonly int _pageNumber = 1;
        private readonly int _pageSize = 10;

        public int PageNumber
        {
            get => _pageNumber;
            init => _pageNumber = value <= 0 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value switch
            {
                <= 0 => 10,
                > _maxPageSize => _maxPageSize,
                _ => value
            };
        }
    }
}
