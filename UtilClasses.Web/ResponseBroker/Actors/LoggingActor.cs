using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using log4net;
using UtilClasses.Logging;
using System.Web;
using System.Web.Script.Serialization;

namespace UtilClasses.Web.ResponseBroker.Actors
{
    public class LoggingActor:IResponseActor
    {
        private readonly NullSafeLog _debugLog;
        private readonly NullSafeLog _callLog;
        private readonly NullSafeLog _errorLog;
        private readonly string _controllerName;
        private readonly HttpRequestMessage _request;
        public string UserName { get; set; }

        public LoggingActor(ILog debugLog, ILog callLog, ILog errorLog, ApiController controller) : this(debugLog,
            callLog, errorLog, controller.GetType().Name, controller.Request)
        {
            UserName = controller.User?.Identity.Name;
        }

        public LoggingActor(ILog debugLog, ILog callLog, ILog errorLog, string controllerName,
            HttpRequestMessage request)
        {
            _debugLog = new NullSafeLog(debugLog);
            _callLog = new NullSafeLog(callLog);
            _errorLog = new NullSafeLog(errorLog);
            _controllerName = controllerName;
            _request = request;
        }

        public void OnBadRequest()
        {
            LogBadRequest();
        }

        public void OnBadRequest(string message)
        {
            LogBadRequest(message);
        }

        public void OnBadRequest(Exception ex)
        {
            LogBadRequest(null, ex);
        }

        public void OnBadRequest(ModelStateDictionary modelState)
        {
            LogBadRequest("", null, true);
        }

        public void OnOk()
        {
            LogOk();
        }

        public void OnOk<T>(T content)
        {
            LogOk(content);
        }

        public void OnInternalServerError(Exception exception)
        {
            LogError(exception);
        }

        public void OnNotFound()
        {
            LogNotFound();
        }

        public void OnCreated<T>(Uri location, T content)
        {
            _callLog.Info(GetLogParams(content).GetCreatedString());
            LogRequestContent();
            LogRequestResult(content);
        }

        public void OnContent<T>(HttpStatusCode statusCode, T value)
        {
            LogStatusCode(statusCode, value);
        }

        public void OnStatusCode(HttpStatusCode status)
        {
            LogStatusCode(status);
        }

        public void OnUnauthorized(string message)
        {
            LogUnauthorized();
        }

        public void OnResponseMessage(HttpResponseMessage response)
        {
            LogStatusCode(response.StatusCode, response.Content.Headers.ContentLength);
        }

        public void Logout()
        {
            LogOk();//might need some improvement....
        }

        private void LogRequestContent()
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    var context = (HttpContextBase)_request.Properties["MS_HttpContext"];
                    context.Request.InputStream.Seek(0, SeekOrigin.Begin);
                    context.Request.InputStream.CopyTo(stream);
                    string requestBody = Encoding.UTF8.GetString(stream.ToArray());

                    if (string.IsNullOrEmpty(requestBody)) return;
                    _callLog.Debug("REQUEST: " + requestBody);
                    _debugLog.Debug("REQUEST: " + requestBody);
                }
            }
            catch (Exception)
            {
                // Do not log "logging-errors" :-)
            }

        }

        private void LogRequestResult<T>(T result)
        {
            var resultString = string.Empty;
            try
            {
                resultString = new JavaScriptSerializer().Serialize(result);

                if (string.IsNullOrEmpty(resultString)) return;
                _callLog.Debug("RESULT: " + resultString);
                _debugLog.Debug("RESULT: " + resultString);
            }
            catch (Exception)
            {
                // Do not log "logging-errors" :-)
            }                        
        }

        private LogParams GetLogParams()
        {
            return GetLogParams(new List<string>());
        }
        private LogParams GetLogParams<T>(T content)
        {
            return LogParams.Create(content, _request.GetQueryNameValuePairs(), _controllerName, UserName);
        }

        private void LogBadRequest(string message = null, Exception ex = null, bool forceDebugLog = false)
        {
            var lp = GetLogParams();
            if (null != ex)
                _debugLog.Error(lp.GetErrorString(ex, true));
            if (forceDebugLog)
                _debugLog.Error(lp.GetBadRequestString(message));
            _callLog.Info(lp.GetBadRequestString(message));
            LogRequestContent();
        }

        private void LogOk()
        {
            var lp = GetLogParams();
            _callLog.Info(lp.GetOKString());
            LogRequestContent();
        }

        private void LogOk<T>(T content)
        {
            var lp = GetLogParams(content);
            _callLog.Info(lp.GetOKString());
            LogRequestContent();
            LogRequestResult(content);
        }

        private void LogError(Exception ex)
        {
            var lp = GetLogParams();
            _debugLog.Error(lp.GetErrorString(ex, true));
            _errorLog.Error(lp.GetErrorString(ex, false));
            _callLog.Error(lp.GetErrorString(ex, false));
            LogRequestContent();
        }

        private void LogNotFound()
        {
            var lp = GetLogParams();
            _callLog.Info(lp.GetNotFoundString());
            LogRequestContent();
        }

        private void LogUnauthorized()
        {
            throw new NotImplementedException();
        }

        private void LogStatusCode(HttpStatusCode status)
        {
            _callLog.Info(GetLogParams().GetStatusCodeString(status));
            LogRequestContent();
        }

        private void LogStatusCode<T>(HttpStatusCode status, T val)
        {
            _callLog.Info(GetLogParams(val).GetStatusCodeString(status));
            LogRequestContent();
            LogRequestResult(val);
        }
        
    }
}
