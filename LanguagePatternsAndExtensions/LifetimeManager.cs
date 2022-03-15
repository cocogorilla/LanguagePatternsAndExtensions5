using System;
using System.Threading;
using System.Threading.Tasks;

namespace LanguagePatternsAndExtensions
{
    /// <summary>
    /// LifeTimeManager provides threadsafe serialized access to a stored instance of type T using SemaphoreSlim internally
    /// </summary>
    /// <typeparam name="T">type of stored instance</typeparam>
    public class LifeTimeManager<T>
    {
        private readonly Func<Task<T>> _receiverAsync;
        private readonly Func<T, bool> _expirationDecider;
        private T _instance;
        private bool _initialized = false;
        private readonly SemaphoreSlim _semaphoreSlim;

        /// <summary>
        /// Construction requires a way to acquire an instance of T and a way to determine the expiration of the instance of T
        /// </summary>
        /// <param name="receiverAsync">T acquisition function</param>
        /// <param name="expirationDecider">T expiration function</param>
        /// <exception cref="ArgumentNullException">Both functions are required</exception>
        public LifeTimeManager(Func<Task<T>> receiverAsync, Func<T, bool> expirationDecider)
        {
            _receiverAsync = receiverAsync ?? throw new ArgumentNullException(nameof(receiverAsync));
            _expirationDecider = expirationDecider ?? throw new ArgumentNullException(nameof(expirationDecider));
            _semaphoreSlim = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Get serialized access to the value of T
        /// </summary>
        /// <param name="forceRefresh">An option to forcibly refresh the instance without respect to the expiration decider</param>
        /// <returns>An instance of T</returns>
        /// <exception cref="LifeTimeManagerException">Stored instances cannot be null</exception>
        public async Task<T> ReceiveMessage(bool forceRefresh = false)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (!_initialized || _expirationDecider(_instance) || forceRefresh)
                {
                    var result = await _receiverAsync();
                    if (result == null)
                        throw new LifeTimeManagerException("retrieval of the instance was null: null is not a supported value for lifetime management");
                    _instance = result;
                    _initialized = true;
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
