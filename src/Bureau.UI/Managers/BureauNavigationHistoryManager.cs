namespace Bureau.UI.Managers
{
    public class BureauNavigationHistoryManager
    {
        private readonly Stack<string> _historyStack = new Stack<string>();
        private string _current = string.Empty;


        public void Push(string uri)
        {
            if (string.IsNullOrWhiteSpace(_current)) 
            {
                _current = uri;
            }
            else
            {
                _historyStack.Push(_current);
                _current = uri;
            }
        }

        public string Pop()
        {
            if (_historyStack.Count > 0) 
            {
                _current = _historyStack.Pop();
                return _current;
            }
            return null!;
        }

        public string Peek()
        {
            return _historyStack.Count > 0 ? _historyStack.Peek() : null!;
        }
    }
}
