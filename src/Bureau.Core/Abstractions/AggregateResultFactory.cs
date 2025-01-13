namespace Bureau.Core
{
    //TODO treba popraviti obzirom na ResultError
    public class AggregateResultFactory
    {
        private List<string> _errors = new List<string>();
        public bool _isSuccess = true;

        public Result Result
        {
            get
            {
                return new Result(_isSuccess, new ResultError(string.Join(Environment.NewLine, _errors)));
            }
        }

        public void Add(Result result)
        {
            _isSuccess &= result.IsSuccess;

            _errors.Add(result.Error.ErrorMessage);
        }
    }

    public class AggregateResultFactory<T>
    {
        private List<T> _list;
        private List<string> _errors = new List<string>();
        public bool _isSuccess = true;

        public Result<List<T>> Result
        {
            get
            {
                return new Result<List<T>>(_list, _isSuccess, new ResultError(string.Join(Environment.NewLine, _errors)));
            }
        }

        public AggregateResultFactory() : this(3)
        {
        }

        public AggregateResultFactory(int capacity)
        {
            _list = new List<T>(capacity);
        }

        public void Add(Result<T> result)
        {
            _isSuccess &= result.IsSuccess;

            _errors.Add(result.Error.ErrorMessage);

            if (result.IsSuccess)
            {
                _list.Add(result.Value);
            }
        }
    }
}
