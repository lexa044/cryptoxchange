using System;
using System.Threading.Tasks;
using CryptoXchange.Models;

namespace CryptoXchange.Services
{
    public abstract class BaseService
    {
        #region [ Private Variables ]
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(BaseService));
        #endregion

        #region [ Constructor ]
        public BaseService()
        {
        }
        #endregion

        #region [ Protected Methods ]
        protected async Task<ServiceResponse<TResult>> ExecuteAsync<TResult>(Task<TResult> func)
        {
            var response = new ServiceResponse<TResult>();
            try
            {
                var task = func;
                await task;
                response.Result = task.Result;
                response.HasError = false;
                response.Exception = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                response.ReferenceNumber = string.Format("{0}_{1}", ex.HResult, DateTime.Now.Ticks);
                response.Result = default(TResult);
                response.HasError = true;
                response.Exception = ex;
            }
            return response;
        }

        protected ServiceResponse<TResult> Execute<TResult>(Func<TResult> func)
        {
            var response = new ServiceResponse<TResult>();
            try
            {
                response.Result = func.Invoke();
                response.HasError = false;
                response.Exception = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                response.ReferenceNumber = string.Format("{0}_{1}", ex.HResult, DateTime.Now.Ticks);
                response.Result = default(TResult);
                response.HasError = true;
                response.Exception = ex;
            }
            return response;
        }

        protected ServiceResponse Execute(Action action)
        {
            var response = new ServiceResponse();
            try
            {
                action.Invoke();
                response.HasError = false;
                response.Exception = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                response.ReferenceNumber = string.Format("{0}_{1}", ex.HResult, DateTime.Now.Ticks);
                response.HasError = true;
                response.Exception = ex;
            }
            return response;
        }
        #endregion
    }
}
