using System.Net.NetworkInformation;

namespace ImageResize.Shared
{
    public class Result<T>
    {

        public Result()
        {

        }

        public Result(bool success)
        {
            Success = success;
        }

        public Result(bool success, string message) : this(success)
        {
            Message = message;
        }

        public Result(bool success, T content) : this(success)
        {
            Content = content;
        }

        public Result(bool success, string message, T content) : this(success, message)
        {
            Content = content;
            AddPaging(content);
        }

        private void AddPaging(T content)
        {
            IPagedList x = content as IPagedList;
            if (x != null)
            {
                Paging p = new Paging
                {
                    PageIndex = x.PageIndex,
                    PageSize = x.PageSize,
                    TotalPages = x.TotalPages,
                    TotalItems = x.TotalItems,
                    HasNextPage = x.HasNextPage,
                    HasPreviousPage = x.HasPreviousPage,
                };

            }
        }

        public bool Success { get; set; } = false;
        public string Title { get; set; }
        public int? Status { get; set; }
        public string Detail { get; set; }
        public ICollection<ValidationError> ValidationErrors { get; set; }
        public string Message { get; set; }
        public T Content { get; set; }
        public string Instance { get; set; }
        public string Code { get; set; }
        public string Path { get; set; }
        public Paging Paging { get; set; }
    }

    public class Result : Result<object>
    {
        public Result()
        {

        }

        public Result(bool success) : base(success)
        {
        }

        public Result(bool success, string message) : base(success, message)
        {
        }

        public Result(bool success, object content) : base(success, string.Empty, content)
        {
        }

        public Result(bool success, string message, object content) : base(success, message, content)
        {
        }
    }
}
