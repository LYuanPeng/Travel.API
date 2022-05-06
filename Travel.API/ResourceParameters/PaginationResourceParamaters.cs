namespace Travel.API.ResourceParameters
{
    public class PaginationResourceParamaters
    {
        private int _pageNumber = 1;
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
            set
            {
                if (value >= 1)
                {
                    _pageNumber = value;
                }
            }
        }

        private int _pageSize = 10;
        const int maxPageSzie = 50;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (value >= 1)
                {
                    _pageSize = (value > maxPageSzie) ? maxPageSzie : value;
                }
            }
        }
    }
}
