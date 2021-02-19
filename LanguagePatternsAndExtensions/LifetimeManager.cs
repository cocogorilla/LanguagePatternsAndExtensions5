using System;
using System.Threading;
using System.Threading.Tasks;

namespace LanguagePatternsAndExtensions
{
    public class LifeTimeManager<T>
    {
        private readonly Func<Task<T>> _receiverAsync;
        private readonly Func<T, bool> _expirationDecider;
        private T _instance;
        private bool _initialized = false;
        private readonly SemaphoreSlim _semaphoreSlim;

        public LifeTimeManager(Func<Task<T>> receiverAsync, Func<T, bool> expirationDecider)
        {
            _receiverAsync = receiverAsync ?? throw new ArgumentNullException(nameof(receiverAsync));
            _expirationDecider = expirationDecider ?? throw new ArgumentNullException(nameof(expirationDecider));
            _semaphoreSlim = new SemaphoreSlim(1, 1);
        }

        public async Task<T> ReceiveMessage(bool forceRefresh = false)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (!_initialized || _expirationDecider(_instance) || forceRefresh)
                {
                    if (!_initialized)
                        _initialized = true;
                    var result = await _receiverAsync();
                    if (result == null)
                        throw new LifeTimeManagerException("retrieval of the instance was null: null is not a supported value for lifetime management");
                    _instance = result;
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
            return await Task.FromResult(_instance);
        }
    }
}
