namespace Roman.RedisManager.Domain.Entities
{
    public class RedisSearchResult(IEnumerable<RedisKey> keys, long cursor)
    {
        private const long _emptyCursor = 0;

        public long Cursor { get; protected set; } = cursor;

        public IEnumerable<RedisKey> Keys { get; protected set; } = keys;

        /// <summary>
        /// If the search is being executed across a cluster, this dictionary will hold
        /// the most recent cursor value returned for each master node (keyed by
        /// "host:port" or simply the node index). A non‑zero value indicates that
        /// further scanning is required on that node.
        /// </summary>
        public Dictionary<string, long>? NodeCursors { get; init; }

        public bool HasMoreResults
        {
            get
            {
                // there are more results if the overall cursor is non‑zero or any
                // of the node cursors still has work left
                if (Cursor != _emptyCursor)
                    return true;

                if (NodeCursors != null)
                {
                    foreach (var val in NodeCursors.Values)
                    {
                        if (val != _emptyCursor)
                            return true;
                    }
                }

                return false;
            }
        }
    }
}
